namespace Lho.Lambda.RuntimeConfiguration.Options;

public class ObservabilityOptions
{
  public const string Section = "Observability";

  public string ServiceName { get; init; } = "now-playing";

  public string EnvironmentName { get; init; } = "local";

  public string Version { get; init; } = "unknown";

  public string GitSha { get; init; } = "unknown";

  public string SentryEnvironment { get; init; } = "local";

  public string SentryRelease { get; init; } = "unknown";

  public string? SentryDsn { get; init; }

  public string? PushgatewayUrl { get; init; }

  public string? PushgatewayAuthHeader { get; init; }

  public string PushgatewayJob { get; init; } = "now-playing";

  public bool MetricsEnabledFlag { get; init; } = true;

  public double SentryTracesSampleRate { get; init; } = 0.5;

  public bool MetricsEnabled => MetricsEnabledFlag && !string.IsNullOrEmpty(PushgatewayUrl);
}
