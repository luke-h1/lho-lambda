using Amazon.Lambda.Core;
using Lho.Lambda.Clients.LastFm;
using Lho.Lambda.Clients.Spotify;
using Lho.Lambda.Models;
using Lho.Lambda.Utils;

namespace Lho.Lambda.Services;

public class NowPlayingService(MemoryCache cache, SpotifyApi spotifyApi, LastFmApi lastFmApi, ILambdaLogger logger)
{
    private const string LastFmProvider = "lastfm";
    private const string SpotifyProvider = "spotify";

    public async Task<NowPlayingResponse> HandleNowPlaying(string provider = LastFmProvider)
    {
        try
        {
            var cacheKey = $"NowPlaying:{provider}";
            var cachedResponse = cache.Get<NowPlayingResponse>(cacheKey);
            if (cachedResponse is not null)
            {
                logger.LogLine("Returning cached now playing response");
                return cachedResponse;
            }

            var response = provider switch
            {
                SpotifyProvider => await HandleSpotifyNowPlaying(),
                _ => await HandleLastFmNowPlaying()
            };

            if (response.Status == 200 && !string.IsNullOrEmpty(response.Title))
            {
                cache.Set(cacheKey, response, TimeSpan.FromSeconds(5));
            }

            return response;
        }
        catch (Exception exception)
        {
            logger.LogLine($"Error fetching now playing data from {provider}: {exception}");
            return EmptyResponse(maintenance: null, status: 500);
        }
    }

    private async Task<NowPlayingResponse> HandleLastFmNowPlaying()
    {
        var recentTracksResponse = await lastFmApi.GetRecentTracks();
        var track = recentTracksResponse.RecentTracks.Tracks.FirstOrDefault();
        if (track is null || !IsNowPlaying(track))
        {
            logger.LogLine("No song currently playing");
            return EmptyResponse(maintenance: null, status: 200);
        }

        return new NowPlayingResponse(
            IsPlaying: true,
            Maintenance: null,
            Status: 200,
            Album: track.Album.Text,
            AlbumImageUrl: track.Images.LastOrDefault(image => !string.IsNullOrEmpty(image.Url))?.Url ?? "",
            Artist: track.Artist.Text,
            SongUrl: track.Url,
            Title: track.Name);
    }

    private async Task<NowPlayingResponse> HandleSpotifyNowPlaying()
    {
        if (!EnvironmentConfig.ShouldCallSpotify)
        {
            return EmptyResponse(maintenance: true, status: 200);
        }

        var nowPlayingResponse = await spotifyApi.GetNowPlaying();
        if (nowPlayingResponse?.Item is null)
        {
            logger.LogLine("No song currently playing");
            return EmptyResponse(maintenance: null, status: 200);
        }

        var item = nowPlayingResponse.Item;
        return new NowPlayingResponse(
            IsPlaying: nowPlayingResponse.IsPlaying,
            Maintenance: null,
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

    private static NowPlayingResponse EmptyResponse(bool? maintenance, int status)
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
