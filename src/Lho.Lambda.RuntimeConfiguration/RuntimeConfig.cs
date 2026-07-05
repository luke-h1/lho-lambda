using Lho.Lambda.RuntimeConfiguration.Options;

namespace Lho.Lambda.RuntimeConfiguration;

public class RuntimeConfig
{
  public LastFmOptions LastFm { get; init; } = new();

  public SpotifyOptions Spotify { get; init; } = new();

  public DeploymentOptions Deployment { get; init; } = new();

  public ObservabilityOptions Observability { get; init; } = new();

  public AuthorizerOptions Authorizer { get; init; } = new();

  public FeatureFlagsOptions Features { get; init; } = new();

  private static readonly Lazy<RuntimeConfig> Cached = new(() => FromEnvironment(Environment.GetEnvironmentVariable));

  public static RuntimeConfig Current => Cached.Value;

  public static RuntimeConfig FromEnvironment(IReadOnlyDictionary<string, string?> environment)
  {
    return FromEnvironment(key => environment.TryGetValue(key, out var value) ? value : null);
  }

  public static RuntimeConfig FromEnvironment(Func<string, string?> getEnvironmentVariable)
  {
    var environmentName = String(getEnvironmentVariable, "ENVIRONMENT", "local");
    var serviceName = String(getEnvironmentVariable, "SERVICE_NAME", "now-playing");
    var version = String(getEnvironmentVariable, "VERSION", "unknown");

    return new RuntimeConfig
    {
      LastFm = new LastFmOptions
      {
        ApiKey = OptionalString(getEnvironmentVariable, "LASTFM_API_KEY"),
        Username = OptionalString(getEnvironmentVariable, "LASTFM_USERNAME")
      },
      Spotify = new SpotifyOptions
      {
        ClientId = OptionalString(getEnvironmentVariable, "SPOTIFY_CLIENT_ID"),
        ClientSecret = OptionalString(getEnvironmentVariable, "SPOTIFY_CLIENT_SECRET"),
        RefreshToken = OptionalString(getEnvironmentVariable, "SPOTIFY_REFRESH_TOKEN"),
        AccessToken = OptionalString(getEnvironmentVariable, "SPOTIFY_ACCESS_TOKEN")
      },
      Deployment = new DeploymentOptions
      {
        Version = version,
        DeployedAt = String(getEnvironmentVariable, "DEPLOYED_AT", "unknown"),
        DeployedBy = String(getEnvironmentVariable, "DEPLOYED_BY", "unknown"),
        GitSha = String(getEnvironmentVariable, "GIT_SHA", "unknown")
      },
      Observability = new ObservabilityOptions
      {
        ServiceName = serviceName,
        EnvironmentName = environmentName,
        Version = version,
        GitSha = String(getEnvironmentVariable, "GIT_SHA", "unknown"),
        SentryEnvironment = String(getEnvironmentVariable, "SENTRY_ENVIRONMENT", environmentName),
        SentryRelease = String(getEnvironmentVariable, "SENTRY_RELEASE", version),
        SentryDsn = OptionalString(getEnvironmentVariable, "SENTRY_DSN"),
        SentryTracesSampleRate = Double(getEnvironmentVariable, "SENTRY_TRACES_SAMPLE_RATE", defaultValue: 0.5)
      },
      Authorizer = new AuthorizerOptions
      {
        ApiKey = OptionalString(getEnvironmentVariable, "API_KEY")
      },
      Features = new FeatureFlagsOptions
      {
        ShouldCallSpotify = Bool(getEnvironmentVariable, "SHOULD_CALL_SPOTIFY", defaultValue: true)
      }
    };
  }

  private static string String(Func<string, string?> getEnvironmentVariable, string key, string defaultValue)
  {
    return getEnvironmentVariable(key) ?? defaultValue;
  }

  private static string? OptionalString(Func<string, string?> getEnvironmentVariable, string key)
  {
    var value = getEnvironmentVariable(key);
    return string.IsNullOrWhiteSpace(value) ? null : value;
  }

  private static bool Bool(Func<string, string?> getEnvironmentVariable, string key, bool defaultValue)
  {
    var value = getEnvironmentVariable(key);
    return value is null ? defaultValue : string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1";
  }

  private static double Double(Func<string, string?> getEnvironmentVariable, string key, double defaultValue)
  {
    var value = getEnvironmentVariable(key);
    return double.TryParse(value, out var parsed) ? Math.Clamp(parsed, 0, 1) : defaultValue;
  }
}
