using System.Text.Json.Serialization;

namespace LhoLambda.Models;

public class SpotifyImage
{
  [JsonPropertyName("height")]
  public int Height { get; set; }

  [JsonPropertyName("url")]
  public string Url { get; set; } = string.Empty;

  [JsonPropertyName("width")]
  public int Width { get; set; }
}
