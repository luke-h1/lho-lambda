using Amazon.Lambda.Core;
using Lho.Lambda.Clients.LastFm;
using Lho.Lambda.Models;
using Lho.Lambda.Observability;
using Lho.Lambda.RuntimeConfiguration.Options;
using Lho.Lambda.Utils;

namespace Lho.Lambda.Services;

public sealed record NowPlayingResult(NowPlayingResponse Response, string Provider);

public class NowPlayingService(
  MemoryCache cache,
  LastFmApi lastFmApi,
  ILambdaLogger logger,
  ObservabilityOptions observability)
{
    private const string LastFmProvider = "lastfm";
    private const string CacheKey = "NowPlaying:lastfm";

    public async Task<NowPlayingResult> HandleNowPlaying()
    {
        var response = await GetCachedNowPlaying();
        return new NowPlayingResult(response, LastFmProvider);
    }

    private async Task<NowPlayingResponse> GetCachedNowPlaying()
    {
        try
        {
            var cachedResponse = cache.Get<NowPlayingResponse>(CacheKey);
            if (cachedResponse is not null)
            {
                logger.LogLine("Returning cached now playing response");
                return cachedResponse;
            }

            var response = await HandleLastFmNowPlaying();

            if (response.Status == 200)
            {
                cache.Set(CacheKey, response, TimeSpan.FromSeconds(5));
            }

            return response;
        }
        catch (Exception exception)
        {
            logger.LogLine($"Error fetching now playing data from {LastFmProvider}: {exception}");
            await SentryTelemetry.CaptureExceptionAsync(exception, observability, logger, new Dictionary<string, string?>
            {
                [InvocationTags.Operation] = "now-playing",
                [InvocationTags.Provider] = LastFmProvider
            });
            return EmptyResponse(status: 500);
        }
    }

    private async Task<NowPlayingResponse> HandleLastFmNowPlaying()
    {
        var recentTracksResponse = await lastFmApi.GetRecentTracks();
        var tracks = recentTracksResponse.RecentTracks?.Tracks ?? [];
        var track = tracks.Count > 0 ? tracks[0] : null;
        if (track is null || !IsNowPlaying(track))
        {
            logger.LogLine("No song currently playing");
            return EmptyResponse(status: 200);
        }

        return new NowPlayingResponse(
            IsPlaying: true,
            Maintenance: false,
            Status: 200,
            Album: track.Album.Text,
            AlbumImageUrl: track.Images.LastOrDefault(image => !string.IsNullOrEmpty(image.Url))?.Url ?? "",
            Artist: track.Artist.Text,
            SongUrl: track.Url,
            Title: track.Name);
    }

    private static bool IsNowPlaying(LastFmTrack track)
    {
        return string.Equals(track.Attributes?.NowPlaying, "true", StringComparison.OrdinalIgnoreCase);
    }

    private static NowPlayingResponse EmptyResponse(int status, bool maintenance = false)
    {
        return new NowPlayingResponse(
            IsPlaying: false,
            Maintenance: maintenance,
            Status: status,
            Album: "",
            AlbumImageUrl: "",
            Artist: "",
            SongUrl: "",
            Title: "");
    }
}
