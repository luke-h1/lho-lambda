using System.Security.Cryptography;
using System.Text;
using Amazon.Lambda.Core;
using Lho.Lambda.Authorizer.Models;
using Lho.Lambda.Observability;
using Lho.Lambda.RuntimeConfiguration;
using Lho.Lambda.RuntimeConfiguration.Options;

namespace Lho.Lambda.Authorizer.Functions;

public class AuthorizerFunction
{
  private readonly AuthorizerOptions _authorizerOptions;
  private readonly ObservabilityOptions _observabilityOptions;

  public AuthorizerFunction()
    : this(RuntimeConfig.Current.Authorizer, RuntimeConfig.Current.Observability)
  {

  }

  public AuthorizerFunction(AuthorizerOptions authorizerOptions, ObservabilityOptions observabilityOptions)
  {
    _authorizerOptions = authorizerOptions;
    _observabilityOptions = observabilityOptions;
  }

  public async Task<AuthorizerSimpleResponse> FunctionHandler(AuthorizerRequest request, ILambdaContext context)
  {
    var consumer = Consumers.Normalise(GetHeaderValue(request.Headers, "x-consumer"));
    var route = request.RouteKey ?? request.RequestContext?.Http?.Path ?? "authorizer";
    var method = request.RequestContext?.Http?.Method ?? "AUTH";
    var isAuthorized = false;

    await using var invocation = LambdaInvocation.Begin(
      context,
      new InvocationDescriptor("authorizer", route, method, consumer, LogEventPrefix: "authorizer.request"),
      _observabilityOptions);

    try
    {
      var apiKey = GetHeaderValue(request.Headers, "x-api-key");
      var validKey = _authorizerOptions.ApiKey;

      if (!SecureCompare(apiKey, validKey))
      {
        invocation.SetReason("invalid_api_key");
        return new AuthorizerSimpleResponse(false);
      }

      if (consumer is not null && !Consumers.IsValid(consumer))
      {
        invocation.SetReason("invalid_consumer");
        return new AuthorizerSimpleResponse(false);
      }

      isAuthorized = true;
      invocation.SetReason("allowed");
      return new AuthorizerSimpleResponse(true);
    }
    catch (Exception exception)
    {
      invocation.SetReason("exception");
      await invocation.RecordFailureAsync(exception);
      throw;
    }
    finally
    {
      await invocation.CompleteAsync(
        isAuthorized ? 200 : 401,
        isAuthorized ? "success" : "denied");
    }
  }

  private static string? GetHeaderValue(IReadOnlyDictionary<string, string>? headers, string key)
  {
    if (headers is null)
    {
      return null;
    }

    foreach (var (headerKey, value) in headers)
    {
      if (string.Equals(headerKey, key, StringComparison.OrdinalIgnoreCase))
      {
        return value;
      }
    }

    return null;
  }

  private static bool SecureCompare(string? first, string? second)
  {
    if (first is null || second is null)
    {
      return false;
    }

    var firstBytes = Encoding.UTF8.GetBytes(first);
    var secondBytes = Encoding.UTF8.GetBytes(second);

    return firstBytes.Length == secondBytes.Length &&
           CryptographicOperations.FixedTimeEquals(firstBytes, secondBytes);
  }
}
