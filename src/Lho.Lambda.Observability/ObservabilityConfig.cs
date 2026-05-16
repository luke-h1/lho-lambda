using Lho.Lambda.RuntimeConfiguration;
using Lho.Lambda.RuntimeConfiguration.Options;

namespace Lho.Lambda.Observability;

public static class ObservabilityConfig
{
  private static ObservabilityOptions Config => RuntimeConfig.Current.Observability;

  public static string ServiceName => Config.ServiceName;

  public static string EnvironmentName => Config.EnvironmentName;

  public static string Version => Config.Version;

  public static string GitSha => Config.GitSha;

  public static string SentryEnvironment => Config.SentryEnvironment;

  public static string SentryRelease => Config.SentryRelease;

  public static string? SentryDsn => Config.SentryDsn;

  public static string? PushgatewayUrl => Config.PushgatewayUrl;

  public static string? PushgatewayAuthHeader => Config.PushgatewayAuthHeader;

  public static string PushgatewayJob => Config.PushgatewayJob;

  public static bool MetricsEnabled => Config.MetricsEnabled;

  public static double SentryTracesSampleRate => Config.SentryTracesSampleRate;
}
