using System.Text.Json.Serialization;

namespace Lho.Lambda.Models;

public record NowPlayingResponse(
  bool IsPlaying,
  bool Maintenance,
  int Status,
  string Album,
  string AlbumImageUrl,
  string Artist,
  string SongUrl,
  string Title);

public record SpotifyResponse(
  [property: JsonPropertyName("is_playing")] bool IsPlaying,
  SpotifyItem? Item);

public record SpotifyItem(
  string Name,
  IReadOnlyList<SpotifyArtist> Artists,
  SpotifyAlbum Album,
  [property: JsonPropertyName("external_urls")] SpotifyExternalUrls ExternalUrls);

public record SpotifyArtist(string Name);

public record SpotifyImage(
  string Url,
  int? Height,
  int? Width);

public record SpotifyAlbum(
  string Name,
  IReadOnlyList<SpotifyImage> Images);

public record SpotifyExternalUrls(string Spotify);

public record VersionResponse(
  string Version,
  string DeployedAt,
  string DeployedBy,
  string GitSha);

public record TokenResponse(
  [property: JsonPropertyName("access_token")] string AccessToken,
  [property: JsonPropertyName("token_type")] string TokenType,
  [property: JsonPropertyName("expires_in")] int ExpiresIn);
