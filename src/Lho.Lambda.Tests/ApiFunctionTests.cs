using System.Net;
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

  private static APIGatewayHttpApiV2ProxyRequest CreateRequest(string path)
  {
    return new APIGatewayHttpApiV2ProxyRequest
    {
      RawPath = path,
      RawQueryString = "",
      RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
      {
        Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
        {
          Method = "GET",
          Path = path
        }
      }
    };
  }

  private static void SetEnvironment(string key, string value)
  {
    Environment.SetEnvironmentVariable(key, value);
  }
}
