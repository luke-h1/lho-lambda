using System.Text.Json.Serialization;

namespace LhoLambda.Models;

public class SpotifyNowPlayingResponse
{
  [JsonPropertyName("is_playing")]
  public bool IsPlaying { get; set; }

  [JsonPropertyName("item")]
  public SpotifyTrack? Item { get; set; }
}
