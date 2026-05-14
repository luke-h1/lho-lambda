using System.Text.Json;
using Amazon.Lambda.Core;

namespace Lho.Lambda.Observability;

public static class StructuredLog
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
  };

  public static void Info(ILambdaLogger logger, string eventName, IReadOnlyDictionary<string, object?> fields)
  {
    Write(logger, "info", eventName, fields);
  }

  public static void Error(ILambdaLogger logger, string eventName, Exception exception, IReadOnlyDictionary<string, object?> fields)
  {
    var enrichedFields = new Dictionary<string, object?>(fields)
    {
      ["errorType"] = exception.GetType().Name,
      ["errorMessage"] = exception.Message
    };

    Write(logger, "error", eventName, enrichedFields);
  }

  private static void Write(ILambdaLogger logger, string level, string eventName, IReadOnlyDictionary<string, object?> fields)
  {
    var payload = new Dictionary<string, object?>
    {
      ["timestamp"] = DateTimeOffset.UtcNow.ToString("O"),
      ["level"] = level,
      ["event"] = eventName,
      ["service"] = ObservabilityConfig.ServiceName,
      ["environment"] = ObservabilityConfig.EnvironmentName,
      ["version"] = ObservabilityConfig.Version,
      ["gitSha"] = ObservabilityConfig.GitSha
    };

    foreach (var (key, value) in fields)
    {
      payload[key] = value;
    }

    logger.LogLine(JsonSerializer.Serialize(payload, JsonOptions));
  }
}
