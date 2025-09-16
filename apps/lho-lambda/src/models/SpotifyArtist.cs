using System.Text.Json.Serialization;

namespace LhoLambda.Models;

public class SpotifyArtist
{
  [JsonPropertyName("external_urls")]
  public SpotifyExternalUrls ExternalUrls { get; set; } = new();

  [JsonPropertyName("href")]
  public string Href { get; set; } = string.Empty;

  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("type")]
  public string Type { get; set; } = string.Empty;

  [JsonPropertyName("uri")]
  public string Uri { get; set; } = string.Empty;
}
