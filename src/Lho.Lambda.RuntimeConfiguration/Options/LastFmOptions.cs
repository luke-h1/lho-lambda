using System.Diagnostics.CodeAnalysis;

namespace Lho.Lambda.RuntimeConfiguration.Options;

public class LastFmOptions
{
  public const string Section = "LastFm";

  public string? ApiKey { get; init; }

  public string? Username { get; init; }

  public LastFmCredentials RequireCredentials()
  {
    if (!IsValid(out var errorMessage))
    {
      throw new RuntimeConfigurationException(errorMessage);
    }

    return new LastFmCredentials(ApiKey!, Username!);
  }

  public bool IsValid([NotNullWhen(false)] out string? errorMessage)
  {
    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(ApiKey))
    {
      errors.Add($"Last.fm API key is not configured. Ensure {Section}:{nameof(ApiKey)} or LASTFM_API_KEY is set.");
    }

    if (string.IsNullOrWhiteSpace(Username))
    {
      errors.Add($"Last.fm username is not configured. Ensure {Section}:{nameof(Username)} or LASTFM_USERNAME is set.");
    }

    errorMessage = errors.Count == 0 ? null : string.Join(" ", errors);
    return errorMessage is null;
  }
}

public sealed record LastFmCredentials(string ApiKey, string Username);
