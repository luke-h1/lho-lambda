namespace Lho.Lambda.Observability;

public static class ObservabilityConfig
{
  public static string ServiceName => String("SERVICE_NAME", "now-playing");

  public static string EnvironmentName => String("ENVIRONMENT", "local");

  public static string Version => String("VERSION", "unknown");

  public static string GitSha => String("GIT_SHA", "unknown");

  public static string SentryEnvironment => String("SENTRY_ENVIRONMENT", EnvironmentName);

  public static string SentryRelease => String("SENTRY_RELEASE", Version);

  public static string? SentryDsn => OptionalString("SENTRY_DSN");

  public static string? PushgatewayUrl => OptionalString("PUSHGATEWAY_URL");

  public static string? PushgatewayAuthHeader => OptionalString("PUSHGATEWAY_AUTH_HEADER");

  public static string PushgatewayJob => String("PROMETHEUS_JOB", ServiceName);

  public static bool MetricsEnabled => Bool("METRICS_ENABLED", defaultValue: true) && !string.IsNullOrEmpty(PushgatewayUrl);

  public static double SentryTracesSampleRate => Double("SENTRY_TRACES_SAMPLE_RATE", defaultValue: 0.5);

  private static string String(string key, string defaultValue)
  {
    return Environment.GetEnvironmentVariable(key) ?? defaultValue;
  }

  private static string? OptionalString(string key)
  {
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrWhiteSpace(value) ? null : value;
  }

  private static bool Bool(string key, bool defaultValue)
  {
    var value = Environment.GetEnvironmentVariable(key);
    return value is null ? defaultValue : string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1";
  }

  private static double Double(string key, double defaultValue)
  {
    var value = Environment.GetEnvironmentVariable(key);
    return double.TryParse(value, out var parsed) ? Math.Clamp(parsed, 0, 1) : defaultValue;
  }
}
