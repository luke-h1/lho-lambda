namespace Lho.Lambda.Models;

public record HealthResponse(string Status);

public record ErrorResponseBody(string Error);

public record NowPlayingResponse(
  bool IsPlaying,
  bool Maintenance,
  int Status,
  string Album,
  string AlbumImageUrl,
  string Artist,
  string SongUrl,
  string Title);

public record VersionResponse(
  string Version,
  string DeployedAt,
  string DeployedBy,
  string GitSha);
