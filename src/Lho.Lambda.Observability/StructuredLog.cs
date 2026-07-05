using System.Text.Json;
using Amazon.Lambda.Core;
using Lho.Lambda.RuntimeConfiguration.Options;

namespace Lho.Lambda.Observability;

public static class StructuredLog
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public static void Info(ILambdaLogger logger, string eventName, ObservabilityOptions options, IReadOnlyDictionary<string, object?> fields)
  {
    Write(logger, "info", eventName, options, fields);
  }

  public static void Error(ILambdaLogger logger, string eventName, Exception exception, ObservabilityOptions options, IReadOnlyDictionary<string, object?> fields)
  {
    var enrichedFields = new Dictionary<string, object?>(fields)
    {
      [InvocationTags.ErrorType] = exception.GetType().Name,
      [InvocationTags.ErrorMessage] = exception.Message
    };

    Write(logger, "error", eventName, options, enrichedFields);
  }

  private static void Write(ILambdaLogger logger, string level, string eventName, ObservabilityOptions options, IReadOnlyDictionary<string, object?> fields)
  {
    var payload = new Dictionary<string, object?>
    {
      ["timestamp"] = DateTimeOffset.UtcNow.ToString("O"),
      ["level"] = level,
      ["event"] = eventName,
      ["service"] = options.ServiceName,
      ["environment"] = options.EnvironmentName,
      ["version"] = options.Version,
      ["git_sha"] = options.GitSha
    };

    foreach (var (key, value) in fields)
    {
      payload[key] = value;
    }

    logger.LogLine(JsonSerializer.Serialize(payload, JsonOptions));
  }
}
