using System.Text;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.SystemTextJson;
using Lho.Lambda.Authorizer.Models;
using Xunit;

namespace Lho.Lambda.Tests;

public class LambdaSerializerTests
{
  [Fact]
  public void ApiSerializerRoundTripsHttpApiV2Events()
  {
    var serializer = new SourceGeneratorLambdaJsonSerializer<Lho.Lambda.Serialization.LambdaEventsJsonContext>();
    const string requestJson = """
      {
        "rawPath": "/api/now-playing",
        "rawQueryString": "provider=lastfm",
        "headers": { "x-consumer": "lhowsam-prod" },
        "requestContext": { "http": { "method": "GET", "path": "/api/now-playing" } }
      }
      """;

    var request = serializer.Deserialize<APIGatewayHttpApiV2ProxyRequest>(ToStream(requestJson));

    Assert.Equal("/api/now-playing", request.RawPath);
    Assert.Equal("provider=lastfm", request.RawQueryString);
    Assert.Equal("lhowsam-prod", request.Headers["x-consumer"]);
    Assert.Equal("GET", request.RequestContext.Http.Method);

    var response = new APIGatewayHttpApiV2ProxyResponse
    {
      StatusCode = 200,
      Body = """{"status":"OK"}""",
      Headers = new Dictionary<string, string> { ["content-type"] = "application/json" }
    };

    var body = JsonDocument.Parse(Serialize(serializer, response)).RootElement;

    Assert.Equal(200, body.GetProperty("statusCode").GetInt32());
    Assert.Equal("""{"status":"OK"}""", body.GetProperty("body").GetString());
    Assert.Equal("application/json", body.GetProperty("headers").GetProperty("content-type").GetString());
  }

  [Fact]
  public void AuthorizerSerializerRoundTripsAuthorizerEvents()
  {
    var serializer = new SourceGeneratorLambdaJsonSerializer<Authorizer.Serialization.LambdaEventsJsonContext>();
    const string requestJson = """
      {
        "version": "2.0",
        "type": "REQUEST",
        "routeKey": "GET /api/now-playing",
        "headers": { "x-api-key": "secret", "x-consumer": "lhowsam-prod" },
        "requestContext": { "http": { "method": "GET", "path": "/api/now-playing" } }
      }
      """;

    var request = serializer.Deserialize<AuthorizerRequest>(ToStream(requestJson));

    Assert.Equal("GET /api/now-playing", request.RouteKey);
    Assert.Equal("secret", request.Headers?["x-api-key"]);
    Assert.Equal("GET", request.RequestContext?.Http?.Method);

    var body = JsonDocument.Parse(Serialize(serializer, new AuthorizerSimpleResponse(true))).RootElement;

    Assert.True(body.GetProperty("isAuthorized").GetBoolean());
  }

  private static MemoryStream ToStream(string json)
  {
    return new MemoryStream(Encoding.UTF8.GetBytes(json));
  }

  private static string Serialize<T>(ILambdaSerializer serializer, T value)
  {
    using var stream = new MemoryStream();
    serializer.Serialize(value, stream);
    return Encoding.UTF8.GetString(stream.ToArray());
  }
}
