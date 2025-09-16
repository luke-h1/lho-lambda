using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace lhoAuthorizer;

public class Function
{
  public Task<APIGatewayCustomAuthorizerResponse> FunctionHandler(APIGatewayCustomAuthorizerRequest request,
    ILambdaContext context)  {
    try
    {
      var consumer = GetHeaderValue(request.Headers, "x-consumer");
      string[] validConsumers =
      [
        "lhowsam-dev",
        "lhowsam-prod",
        "lhowsam-local"
      ];


      var referer = GetHeaderValue(request.Headers, "Referer") ??
                    GetHeaderValue(request.Headers, "referer") ?? "unknown";

      var apiKey = GetHeaderValue(request.Headers, "x-api-key");
      var validKey = Environment.GetEnvironmentVariable("API_KEY");

      if (apiKey != validKey)
      {
        context.Logger.LogInformation("Deny");
        return GeneratePolicy("user", "Deny", request.MethodArn);
      }

      // ReSharper disable once InvertIf
      if (!validConsumers.Contains(consumer ?? "") && apiKey == validKey)
      {
        context.Logger.LogInformation("Deny - invalid consumer");
        return GeneratePolicy("user", "Deny",  request.MethodArn);
      }

      return GeneratePolicy("user", "Allow", request.MethodArn);

    }
    catch (Exception e)
    {
      context.Logger.LogError($"Error in authorizer: {e.Message}");
      return GeneratePolicy("user", "Deny", request.MethodArn);
    }
  }

  private static string? GetHeaderValue(IDictionary<string, string>? headers, string key)
  {
    // ReSharper disable once UseNullPropagation
    if (headers == null)
    {
      return null;
    }

    var kvpHeaders = headers.FirstOrDefault(h => string.Equals(h.Key, key, StringComparison.OrdinalIgnoreCase));

    return kvpHeaders.Key != null ? kvpHeaders.Value : null;
  }

  private static Task<APIGatewayCustomAuthorizerResponse> GeneratePolicy(string principalId, string effect, string resource)
  {
    return Task.FromResult(new APIGatewayCustomAuthorizerResponse
    {
      PrincipalID = principalId,
      PolicyDocument = new APIGatewayCustomAuthorizerPolicy
      {
        Version = "2012-10-17",
        Statement =
        [
          new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
          {
            Action = ["execute-api:Invoke"],
            Effect = effect,
            Resource = [resource]
          }
        ]
      }
    });
  }
}
