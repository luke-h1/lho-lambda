using System.Text.Json.Serialization;

namespace LhoLambda.Models;

public class SpotifyTokenResponse
{
  [JsonPropertyName("access_token")]
  public string AccessToken { get; set; } = string.Empty;

  [JsonPropertyName("token_type")]
  public string TokenType { get; set; } = string.Empty;

  [JsonPropertyName("expires_in")]
  public int ExpiresIn { get; set; }
}
