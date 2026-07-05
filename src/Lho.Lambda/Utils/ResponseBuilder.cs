using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Lho.Lambda.Models;
using Lho.Lambda.Serialization;

namespace Lho.Lambda.Utils;

public static class ResponseBuilder
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    TypeInfoResolver = ApiResponseJsonContext.Default
  };

  private static readonly Dictionary<string, string> DefaultCorsHeaders = new()
  {
    ["content-type"] = "application/json",
    ["Access-Control-Allow-Origin"] = "*",
    ["Access-Control-Allow-Methods"] = "GET,HEAD,OPTIONS,POST,PUT,DELETE"
  };

  public static APIGatewayHttpApiV2ProxyResponse CreateResponse<T>(
    T body,
    bool includeCacheControl = true,
    int revalidateSeconds = 3)
  {
    var headers = new Dictionary<string, string>(DefaultCorsHeaders)
    {
      ["Cache-Control"] = includeCacheControl
        ? $"max-age={revalidateSeconds}, s-maxage={revalidateSeconds}, stale-while-revalidate={revalidateSeconds}, stale-if-error={revalidateSeconds}"
        : "no-cache"
    };

    return new APIGatewayHttpApiV2ProxyResponse
    {
      StatusCode = 200,
      Headers = headers,
      Body = JsonSerializer.Serialize(body, JsonOptions)
    };
  }

  public static APIGatewayHttpApiV2ProxyResponse ErrorResponse(int statusCode, string message)
  {
    return new APIGatewayHttpApiV2ProxyResponse
    {
      StatusCode = statusCode,
      Headers = new Dictionary<string, string>(DefaultCorsHeaders),
      Body = JsonSerializer.Serialize(new ErrorResponseBody(message), JsonOptions)
    };
  }
}
