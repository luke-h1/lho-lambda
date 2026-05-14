using System.Net;
using System.Net.Sockets;
using Lho.Lambda.Observability;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Lho.Lambda.Tests;

public class PrometheusMetricsTests
{
  [Fact]
  public async Task PushInvocationAddsConfiguredAuthorizationHeader()
  {
    var port = GetFreePort();
    using var listener = new HttpListener();
    listener.Prefixes.Add($"http://127.0.0.1:{port}/");
    listener.Start();

    Environment.SetEnvironmentVariable("METRICS_ENABLED", "true");
    Environment.SetEnvironmentVariable("PUSHGATEWAY_URL", $"http://127.0.0.1:{port}");
    Environment.SetEnvironmentVariable("PUSHGATEWAY_AUTH_HEADER", "Authorization=Basic dXNlcjpwYXNz");
    Environment.SetEnvironmentVariable("PROMETHEUS_JOB", "test-job");
    Environment.SetEnvironmentVariable("ENVIRONMENT", "test");

    var requestTask = listener.GetContextAsync();

    var pushTask = PrometheusMetrics.PushInvocationAsync(
      new InvocationMetric(
        FunctionName: "test-function",
        Operation: "test-operation",
        Route: "/test",
        Method: "GET",
        StatusCode: 200,
        DurationMs: 12,
        Outcome: "success"),
      new TestLambdaLogger());

    var context = await requestTask.WaitAsync(TimeSpan.FromSeconds(2));
    var authorizationHeader = context.Request.Headers["Authorization"];
    context.Response.StatusCode = (int)HttpStatusCode.Accepted;
    context.Response.Close();

    await pushTask.WaitAsync(TimeSpan.FromSeconds(2));

    Assert.Equal("Basic dXNlcjpwYXNz", authorizationHeader);
  }

  private static int GetFreePort()
  {
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
  }
}
