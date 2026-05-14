using System.Net;
using System.Net.Sockets;
using Lho.Lambda.Observability;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Lho.Lambda.Tests;

public class PrometheusMetricsTests
{
  [Fact]
  public async Task PushInvocationCountsEachProviderSeriesSeparately()
  {
    var port = GetFreePort();
    using var listener = new HttpListener();
    listener.Prefixes.Add($"http://127.0.0.1:{port}/");
    listener.Start();

    ConfigureMetrics(port);

    var lastFmBody = await PushAndReadBody(listener, new InvocationMetric(
      FunctionName: "provider-counter-test-function",
      Operation: "now-playing",
      Route: "/api/now-playing",
      Method: "GET",
      StatusCode: 200,
      DurationMs: 12,
      Outcome: "success",
      Provider: "lastfm"));

    var spotifyBody = await PushAndReadBody(listener, new InvocationMetric(
      FunctionName: "provider-counter-test-function",
      Operation: "now-playing",
      Route: "/api/now-playing",
      Method: "GET",
      StatusCode: 200,
      DurationMs: 14,
      Outcome: "success",
      Provider: "spotify"));

    Assert.Equal("1", InvocationTotalValue(lastFmBody));
    Assert.Contains("provider=\"lastfm\"", InvocationTotalLine(lastFmBody));
    Assert.Equal("1", InvocationTotalValue(spotifyBody));
    Assert.Contains("provider=\"spotify\"", InvocationTotalLine(spotifyBody));
  }

  [Fact]
  public async Task PushInvocationAddsConfiguredAuthorizationHeader()
  {
    var port = GetFreePort();
    using var listener = new HttpListener();
    listener.Prefixes.Add($"http://127.0.0.1:{port}/");
    listener.Start();

    ConfigureMetrics(port);
    Environment.SetEnvironmentVariable("PUSHGATEWAY_AUTH_HEADER", "Authorization=Basic dXNlcjpwYXNz");

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

  private static void ConfigureMetrics(int port)
  {
    Environment.SetEnvironmentVariable("METRICS_ENABLED", "true");
    Environment.SetEnvironmentVariable("PUSHGATEWAY_URL", $"http://127.0.0.1:{port}");
    Environment.SetEnvironmentVariable("PUSHGATEWAY_AUTH_HEADER", null);
    Environment.SetEnvironmentVariable("PROMETHEUS_JOB", "test-job");
    Environment.SetEnvironmentVariable("ENVIRONMENT", "test");
  }

  private static async Task<string> PushAndReadBody(HttpListener listener, InvocationMetric metric)
  {
    var requestTask = listener.GetContextAsync();
    var pushTask = PrometheusMetrics.PushInvocationAsync(metric, new TestLambdaLogger());

    var context = await requestTask.WaitAsync(TimeSpan.FromSeconds(2));
    using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
    var body = await reader.ReadToEndAsync();
    context.Response.StatusCode = (int)HttpStatusCode.Accepted;
    context.Response.Close();

    await pushTask.WaitAsync(TimeSpan.FromSeconds(2));

    return body;
  }

  private static string InvocationTotalValue(string body)
  {
    var line = InvocationTotalLine(body);
    return line[(line.LastIndexOf(' ') + 1)..];
  }

  private static string InvocationTotalLine(string body)
  {
    return body
      .Split('\n', StringSplitOptions.RemoveEmptyEntries)
      .Single(line => line.StartsWith("lho_lambda_invocations_total{", StringComparison.Ordinal));
  }

  private static int GetFreePort()
  {
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
  }
}
