using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Lho.Lambda.Functions;
using Xunit;

namespace Lho.Lambda.Tests;

public class ApiFunctionTests
{
  [Fact]
  public async Task HealthEndpointReturnsOkWithoutCacheHeader()
  {
    var function = new ApiFunction();

    var response = await function.FunctionHandler(CreateRequest("/test/api/health"), new TestLambdaContext());

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("no-cache", response.Headers["Cache-Control"]);
    Assert.Equal("application/json", response.Headers["content-type"]);
    Assert.Equal("OK", JsonDocument.Parse(response.Body).RootElement.GetProperty("status").GetString());
  }

  [Fact]
  public async Task MissingRouteReturnsNotFound()
  {
    var function = new ApiFunction();

    var response = await function.FunctionHandler(CreateRequest("/api/missing"), new TestLambdaContext());

    Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
    Assert.Equal("Not Found", JsonDocument.Parse(response.Body).RootElement.GetProperty("error").GetString());
  }

  [Fact]
  public async Task VersionEndpointUsesDeploymentEnvironmentValues()
  {
    SetEnvironment("VERSION", "1.2.3");
    SetEnvironment("DEPLOYED_AT", "2026-05-10T09:00:00Z");
    SetEnvironment("DEPLOYED_BY", "tests");
    SetEnvironment("GIT_SHA", "abc123");

    var function = new ApiFunction();

    var response = await function.FunctionHandler(CreateRequest("/invoke/api/version"), new TestLambdaContext());
    var body = JsonDocument.Parse(response.Body).RootElement;

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("1.2.3", body.GetProperty("version").GetString());
    Assert.Equal("2026-05-10T09:00:00Z", body.GetProperty("deployedAt").GetString());
    Assert.Equal("tests", body.GetProperty("deployedBy").GetString());
    Assert.Equal("abc123", body.GetProperty("gitSha").GetString());
  }

  [Fact]
  public async Task ApiHealthAndVersionRoutesPushInvocationMetrics()
  {
    var port = GetFreePort();
    using var listener = new HttpListener();
    listener.Prefixes.Add($"http://127.0.0.1:{port}/");
    listener.Start();

    ConfigureMetrics(port);
    try
    {
      var function = new ApiFunction();

      var healthGetMetric = await InvokeAndReadMetric(listener, function, CreateRequest("/api/health"));
      var healthHeadMetric = await InvokeAndReadMetric(listener, function, CreateRequest("/api/health", "HEAD"));
      var versionMetric = await InvokeAndReadMetric(listener, function, CreateRequest("/api/version"));

      Assert.Contains("route=\"/api/health\"", healthGetMetric);
      Assert.Contains("method=\"GET\"", healthGetMetric);
      Assert.Contains("status=\"200\"", healthGetMetric);
      Assert.Contains("route=\"/api/health\"", healthHeadMetric);
      Assert.Contains("method=\"HEAD\"", healthHeadMetric);
      Assert.Contains("status=\"200\"", healthHeadMetric);
      Assert.Contains("route=\"/api/version\"", versionMetric);
      Assert.Contains("method=\"GET\"", versionMetric);
      Assert.Contains("status=\"200\"", versionMetric);
    }
    finally
    {
      Environment.SetEnvironmentVariable("METRICS_ENABLED", null);
      Environment.SetEnvironmentVariable("PUSHGATEWAY_URL", null);
      Environment.SetEnvironmentVariable("PUSHGATEWAY_AUTH_HEADER", null);
      Environment.SetEnvironmentVariable("PROMETHEUS_JOB", null);
      Environment.SetEnvironmentVariable("ENVIRONMENT", null);
    }
  }

  private static APIGatewayHttpApiV2ProxyRequest CreateRequest(string path, string method = "GET")
  {
    return new APIGatewayHttpApiV2ProxyRequest
    {
      RawPath = path,
      RawQueryString = "",
      RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
      {
        Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
        {
          Method = method,
          Path = path
        }
      }
    };
  }

  private static void SetEnvironment(string key, string value)
  {
    Environment.SetEnvironmentVariable(key, value);
  }

  private static void ConfigureMetrics(int port)
  {
    Environment.SetEnvironmentVariable("METRICS_ENABLED", "true");
    Environment.SetEnvironmentVariable("PUSHGATEWAY_URL", $"http://127.0.0.1:{port}");
    Environment.SetEnvironmentVariable("PUSHGATEWAY_AUTH_HEADER", null);
    Environment.SetEnvironmentVariable("PROMETHEUS_JOB", "test-job");
    Environment.SetEnvironmentVariable("ENVIRONMENT", "test");
  }

  private static async Task<string> InvokeAndReadMetric(
    HttpListener listener,
    ApiFunction function,
    APIGatewayHttpApiV2ProxyRequest request)
  {
    var requestTask = listener.GetContextAsync();
    var responseTask = function.FunctionHandler(request, new TestLambdaContext());

    var context = await requestTask.WaitAsync(TimeSpan.FromSeconds(2));
    using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
    var body = await reader.ReadToEndAsync();
    context.Response.StatusCode = (int)HttpStatusCode.Accepted;
    context.Response.Close();

    var response = await responseTask.WaitAsync(TimeSpan.FromSeconds(2));
    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    return body;
  }

  private static int GetFreePort()
  {
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
  }
}
