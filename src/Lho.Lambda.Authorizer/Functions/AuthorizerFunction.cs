using System.Security.Cryptography;
using System.Text;
using Amazon.Lambda.Core;
using Lho.Lambda.Authorizer.Models;

namespace Lho.Lambda.Authorizer.Functions;

public class AuthorizerFunction
{
    private static readonly HashSet<string> ValidConsumers = ["lhowsam-dev", "lhowsam-prod", "lhowsam-local"];

    public Task<AuthorizerSimpleResponse> FunctionHandler(AuthorizerRequest request, ILambdaContext context)
    {
        context.Logger.LogLine("Authorizer invoked");

        var apiKey = GetHeaderValue(request.Headers, "x-api-key");
        var validKey = Environment.GetEnvironmentVariable("API_KEY");

        if (!SecureCompare(apiKey, validKey))
        {
            context.Logger.LogLine("Deny - API key invalid");
            return Task.FromResult(new AuthorizerSimpleResponse(false));
        }

        var consumer = GetHeaderValue(request.Headers, "x-consumer");
        if (consumer is not null && !ValidConsumers.Contains(consumer))
        {
            context.Logger.LogLine("Deny - Invalid consumer");
            return Task.FromResult(new AuthorizerSimpleResponse(false));
        }

        context.Logger.LogLine("Allow");
        return Task.FromResult(new AuthorizerSimpleResponse(true));
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
            return first is null && second is null;
        }

        var firstBytes = Encoding.UTF8.GetBytes(first);
        var secondBytes = Encoding.UTF8.GetBytes(second);

        return firstBytes.Length == secondBytes.Length &&
               CryptographicOperations.FixedTimeEquals(firstBytes, secondBytes);
    }
}
