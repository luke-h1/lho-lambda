using System.Text.Json.Serialization;

namespace Lho.Lambda.Authorizer.Models;

public class AuthorizerRequest
{
  public string? Version { get; init; }

  public string? Type { get; init; }

  public string? RouteArn { get; init; }

  public string? RouteKey { get; init; }

  public string? RawPath { get; init; }

  public string? RawQueryString { get; init; }

  public Dictionary<string, string>? Headers { get; init; }

  public AuthorizerRequestContext? RequestContext { get; init; }
}

public class AuthorizerRequestContext
{
  public HttpContext? Http { get; init; }
}

public class HttpContext
{
  public string? Method { get; init; }

  public string? Path { get; init; }
}

public record AuthorizerSimpleResponse(
  [property: JsonPropertyName("isAuthorized")] bool IsAuthorized);
