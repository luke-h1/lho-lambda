using Amazon.Lambda.Core;
using Lho.Lambda.Clients.Spotify;
using Lho.Lambda.Models;
using Lho.Lambda.Observability;

namespace Lho.Lambda.Services;

public class TopTracksService(SpotifyApi spotifyApi, ILambdaLogger logger)
{
  public async Task<TopTracksApiResponse> HandleTopTracks(string timeRange, int limit)
  {
    try
    {
      var response = await spotifyApi.GetTopTracks(timeRange, limit);
      var tracks = response.Items
        .Select(item => new TopTrackResponseItem(
          Title: item.Name,
          Artist: string.Join(", ", item.Artists.Select(artist => artist.Name)),
          Album: item.Album.Name,
          AlbumImageUrl: item.Album.Images.FirstOrDefault()?.Url ?? "",
          SongUrl: item.ExternalUrls.Spotify))
        .ToArray();

      return new TopTracksApiResponse(tracks);
    }
    catch (Exception exception)
    {
      logger.LogLine($"Top tracks fetch failed: {exception}");
      await SentryTelemetry.CaptureExceptionAsync(exception, logger, new Dictionary<string, string?>
      {
        ["operation"] = "top-tracks",
        ["time_range"] = timeRange
      });
      throw;
    }
  }
}
