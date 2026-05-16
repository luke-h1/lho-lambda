using Amazon.Lambda.Core;
using Lho.Lambda.Clients.LastFm;
using Lho.Lambda.Clients.Spotify;
using Lho.Lambda.Models;
using Lho.Lambda.Observability;
using Lho.Lambda.RuntimeConfiguration.Options;
using Lho.Lambda.Utils;

namespace Lho.Lambda.Services;

public class NowPlayingService(
  MemoryCache cache,
  SpotifyApi spotifyApi,
  LastFmApi lastFmApi,
  ILambdaLogger logger,
  FeatureFlagsOptions featureFlags)
{
  private const string LastFmProvider = "lastfm";
  private const string SpotifyProvider = "spotify";
  private const string LastFmCacheKey = "NowPlaying:lastfm";
  private const string SpotifyCacheKey = "NowPlaying:spotify";

  public async Task<NowPlayingResponse> HandleNowPlaying(string provider = LastFmProvider)
  {
    return string.Equals(provider, SpotifyProvider, StringComparison.OrdinalIgnoreCase)
      ? await GetSpotifyNowPlaying()
      : await GetNowPlaying();
  }

  public async Task<NowPlayingResponse> GetNowPlaying()
  {
    return await GetCachedNowPlaying(LastFmCacheKey, LastFmProvider, HandleLastFmNowPlaying);
  }

  public async Task<NowPlayingResponse> GetSpotifyNowPlaying()
  {
    return await GetCachedNowPlaying(SpotifyCacheKey, SpotifyProvider, HandleSpotifyNowPlaying);
  }

  private async Task<NowPlayingResponse> GetCachedNowPlaying(
    string cacheKey,
    string provider,
    Func<Task<NowPlayingResponse>> fetch)
  {
    try
    {
      var cachedResponse = cache.Get<NowPlayingResponse>(cacheKey);
      if (cachedResponse is not null)
      {
        logger.LogLine("Returning cached now playing response");
        return cachedResponse;
      }

      var response = await fetch();

      if (response.Status == 200 && !string.IsNullOrEmpty(response.Title))
      {
        cache.Set(cacheKey, response, TimeSpan.FromSeconds(5));
      }

      return response;
    }
    catch (Exception exception)
    {
      logger.LogLine($"Error fetching now playing data from {provider}: {exception}");
      await SentryTelemetry.CaptureExceptionAsync(exception, logger, new Dictionary<string, string?>
      {
        ["operation"] = "now-playing",
        ["provider"] = provider
      });
      return EmptyResponse(status: 500);
    }
  }

  private async Task<NowPlayingResponse> HandleLastFmNowPlaying()
  {
    var recentTracksResponse = await lastFmApi.GetRecentTracks();
    var track = recentTracksResponse.RecentTracks.Tracks.FirstOrDefault();
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

  private async Task<NowPlayingResponse> HandleSpotifyNowPlaying()
  {
    if (!featureFlags.ShouldCallSpotify)
    {
      return EmptyResponse(maintenance: true, status: 200);
    }

    var nowPlayingResponse = await spotifyApi.GetNowPlaying();
    if (nowPlayingResponse?.Item is null || !nowPlayingResponse.IsPlaying)
    {
      logger.LogLine("No song currently playing");
      return EmptyResponse(status: 200);
    }

    var item = nowPlayingResponse.Item;
    return new NowPlayingResponse(
        IsPlaying: nowPlayingResponse.IsPlaying,
        Maintenance: false,
        Status: 200,
        Album: item.Album.Name,
        AlbumImageUrl: item.Album.Images.FirstOrDefault()?.Url ?? "",
        Artist: string.Join(", ", item.Artists.Select(artist => artist.Name)),
        SongUrl: item.ExternalUrls.Spotify,
        Title: item.Name);
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
