namespace LhoLambda.Models;

public class NowPlayingResponse
{
  public int Status { get; set; } = 200;

  public bool? Maintenance { get; set; } = false;

  public bool IsPlaying { get; set; } = false;

  public string? Title { get; set; } = string.Empty;

  public string Artist { get; set; } = string.Empty;

  public string? Album { get; set; } = string.Empty;

  public string AlbumImageUrl { get; set; } = string.Empty;

  public string? SongUrl { get; set; } = string.Empty;
}
