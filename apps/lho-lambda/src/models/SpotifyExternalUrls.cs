using System.Text.Json.Serialization;

namespace LhoLambda.Models;

public class SpotifyExternalUrls
{
  [JsonPropertyName("spotify")]
  public string Spotify { get; set; } = string.Empty;
}
