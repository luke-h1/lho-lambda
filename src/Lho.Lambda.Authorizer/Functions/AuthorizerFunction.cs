using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Amazon.Lambda.Core;
using Lho.Lambda.Authorizer.Models;
using Lho.Lambda.Observability;

namespace Lho.Lambda.Authorizer.Functions;

public class AuthorizerFunction
{
    private static readonly HashSet<string> ValidConsumers = ["lhowsam-dev", "lhowsam-prod", "lhowsam-local"];

    public async Task<AuthorizerSimpleResponse> FunctionHandler(AuthorizerRequest request, ILambdaContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var consumer = NormaliseConsumer(GetHeaderValue(request.Headers, "x-consumer"));
        var route = request.RouteKey ?? request.RequestContext?.Http?.Path ?? "authorizer";
        var method = request.RequestContext?.Http?.Method ?? "AUTH";
        var reason = "allowed";
        var isAuthorized = false;
        Exception? capturedException = null;
        SentryTelemetry.Initialise(context.Logger);
        using var transaction = SentryTelemetry.StartTransaction(context.Logger, $"{method} {route}", "http.server", new Dictionary<string, string?>
        {
            ["function"] = context.FunctionName,
            ["request_id"] = context.AwsRequestId,
            ["operation"] = "authorizer",
            ["route"] = route,
            ["method"] = method,
            ["consumer"] = consumer
        });

        try
        {
            var apiKey = GetHeaderValue(request.Headers, "x-api-key");
            var validKey = Environment.GetEnvironmentVariable("API_KEY");

            if (!SecureCompare(apiKey, validKey))
            {
                reason = "invalid_api_key";
                return new AuthorizerSimpleResponse(false);
            }

            if (consumer is not null && !ValidConsumers.Contains(consumer))
            {
                reason = "invalid_consumer";
                return new AuthorizerSimpleResponse(false);
            }

            isAuthorized = true;
            return new AuthorizerSimpleResponse(true);
        }
        catch (Exception exception)
        {
            capturedException = exception;
            reason = "exception";
            StructuredLog.Error(context.Logger, "authorizer.error", exception, new Dictionary<string, object?>
            {
                ["requestId"] = context.AwsRequestId,
                ["function"] = context.FunctionName,
                ["route"] = route,
                ["method"] = method,
                ["consumer"] = consumer
            });
            await SentryTelemetry.CaptureExceptionAsync(exception, context.Logger, new Dictionary<string, string?>
            {
                ["function"] = context.FunctionName,
                ["request_id"] = context.AwsRequestId,
                ["operation"] = "authorizer",
                ["route"] = route,
                ["consumer"] = consumer
            });
            throw;
        }
        finally
        {
            stopwatch.Stop();
            var statusCode = isAuthorized ? 200 : 401;
            var outcome = isAuthorized ? "success" : "denied";
            StructuredLog.Info(context.Logger, "authorizer.request", new Dictionary<string, object?>
            {
                ["requestId"] = context.AwsRequestId,
                ["function"] = context.FunctionName,
                ["route"] = route,
                ["method"] = method,
                ["statusCode"] = statusCode,
                ["durationMs"] = Math.Round(stopwatch.Elapsed.TotalMilliseconds, 2),
                ["outcome"] = outcome,
                ["reason"] = reason,
                ["consumer"] = consumer
            });

            var metric = new InvocationMetric(
                FunctionName: context.FunctionName,
                Operation: "authorizer",
                Route: route,
                Method: method,
                StatusCode: statusCode,
                DurationMs: stopwatch.Elapsed.TotalMilliseconds,
                Outcome: outcome,
                Consumer: consumer);

            transaction?.Finish(statusCode, capturedException);
            await PrometheusMetrics.PushInvocationAsync(metric, context.Logger);
            await SentryTelemetry.RecordInvocationAsync(metric, context.Logger);
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

    private static string? NormaliseConsumer(string? consumer)
    {
        return consumer switch
        {
            "lhowsam-prod" or "lhowsam-dev" or "lhowsam-local" => consumer,
            null or "" => null,
            _ => "unknown"
        };
    }
}
