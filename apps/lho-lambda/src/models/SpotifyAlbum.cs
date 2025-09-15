using System.Text.Json.Serialization;

namespace LhoLambda.Models;

public class SpotifyAlbum
{
  [JsonPropertyName("album_type")]
  public string AlbumType { get; set; } = string.Empty;

  [JsonPropertyName("artists")]
  public List<SpotifyArtist> Artists { get; set; } = new();

  [JsonPropertyName("external_urls")]
  public SpotifyExternalUrls ExternalUrls { get; set; } = new();

  [JsonPropertyName("images")]
  public List<SpotifyImage> Images { get; set; } = new();

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [JsonPropertyName("release_date")]
  public string ReleaseDate { get; set; } = string.Empty;
}
