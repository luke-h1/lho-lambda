using System.Text.Json.Serialization;

namespace LhoLambda.Models;

public class SpotifyTrack
{
  [JsonPropertyName("album")]
  public SpotifyAlbum Album { get; set; } = new();

  [JsonPropertyName("artists")]
  public List<SpotifyArtist> Artists { get; set; } = new();

  [JsonPropertyName("external_urls")]
  public SpotifyExternalUrls ExternalUrls { get; set; } = new();

  [JsonPropertyName("href")]
  public string Href { get; set; } = string.Empty;

  [JsonPropertyName("id")]
  public string Id { get; set; } = string.Empty;

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("popularity")]
  public int Popularity { get; set; }

  [JsonPropertyName("preview_url")]
  public string? PreviewUrl { get; set; }

  [JsonPropertyName("track_number")]
  public int TrackNumber { get; set; }

  [JsonPropertyName("type")]
  public string Type { get; set; } = string.Empty;

  [JsonPropertyName("uri")]
  public string Uri { get; set; } = string.Empty;
}
