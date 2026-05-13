namespace Lho.Lambda.Utils;

public static class EnvironmentConfig
{
  public static string String(string key, string defaultValue = "")
  {
    return Environment.GetEnvironmentVariable(key) ?? defaultValue;
  }

  public static bool Bool(string key, bool defaultValue = false)
  {
    var value = Environment.GetEnvironmentVariable(key);
    return value is null ? defaultValue : string.Equals(value, "true", StringComparison.OrdinalIgnoreCase) || value == "1";
  }

  public static bool ShouldCallSpotify => Bool("SHOULD_CALL_SPOTIFY", defaultValue: true);

  public static class Spotify
  {
    public static string? ClientId => Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");

    public static string? ClientSecret => Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");

    public static string? RefreshToken => Environment.GetEnvironmentVariable("SPOTIFY_REFRESH_TOKEN");

    public static string? AccessToken => Environment.GetEnvironmentVariable("SPOTIFY_ACCESS_TOKEN");
  }

  public static class LastFm
  {
    public static string? ApiKey => Environment.GetEnvironmentVariable("LASTFM_API_KEY");

    public static string? Username => Environment.GetEnvironmentVariable("LASTFM_USERNAME");
  }

  public static class Deploy
  {
    public static string Version => String("VERSION", "unknown");

    public static string DeployedAt => String("DEPLOYED_AT", "unknown");

    public static string DeployedBy => String("DEPLOYED_BY", "unknown");

    public static string GitSha => String("GIT_SHA", "unknown");
  }
}
