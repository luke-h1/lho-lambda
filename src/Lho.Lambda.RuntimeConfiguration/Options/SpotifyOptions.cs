using System.Diagnostics.CodeAnalysis;

namespace Lho.Lambda.RuntimeConfiguration.Options;

public class SpotifyOptions
{
  public const string Section = "Spotify";

  public string? ClientId { get; init; }

  public string? ClientSecret { get; init; }

  public string? RefreshToken { get; init; }

  public string? AccessToken { get; init; }

  public SpotifyRefreshCredentials RequireRefreshCredentials()
  {
    if (!IsRefreshTokenFlowValid(out var errorMessage))
    {
      throw new RuntimeConfigurationException(errorMessage);
    }

    return new SpotifyRefreshCredentials(ClientId!, ClientSecret!, RefreshToken!);
  }

  public bool IsRefreshTokenFlowValid([NotNullWhen(false)] out string? errorMessage)
  {
    var errors = new List<string>();

    if (string.IsNullOrWhiteSpace(ClientId))
    {
      errors.Add($"Spotify client ID is not configured. Ensure {Section}:{nameof(ClientId)} or SPOTIFY_CLIENT_ID is set.");
    }

    if (string.IsNullOrWhiteSpace(ClientSecret))
    {
      errors.Add($"Spotify client secret is not configured. Ensure {Section}:{nameof(ClientSecret)} or SPOTIFY_CLIENT_SECRET is set.");
    }

    if (string.IsNullOrWhiteSpace(RefreshToken))
    {
      errors.Add($"Spotify refresh token is not configured. Ensure {Section}:{nameof(RefreshToken)} or SPOTIFY_REFRESH_TOKEN is set.");
    }

    errorMessage = errors.Count == 0 ? null : string.Join(" ", errors);
    return errorMessage is null;
  }
}

public sealed record SpotifyRefreshCredentials(string ClientId, string ClientSecret, string RefreshToken);
