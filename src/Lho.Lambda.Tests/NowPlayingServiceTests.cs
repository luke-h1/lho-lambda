using System.Net;
using System.Text;
using Lho.Lambda.Clients.LastFm;
using Lho.Lambda.Clients.Spotify;
using Lho.Lambda.RuntimeConfiguration.Options;
using Lho.Lambda.Services;
using Lho.Lambda.Utils;
using Xunit;

namespace Lho.Lambda.Tests;

public class NowPlayingServiceTests
{
  [Fact]
  public async Task NowPlayingUsesLastFmAndCachesResult()
  {
    var lastFmHandler = new StaticJsonHandler("""
      {
        "recenttracks": {
          "track": [{
            "name": "Last.fm song",
            "artist": { "#text": "Last.fm artist" },
            "album": { "#text": "Last.fm album" },
            "url": "https://last.fm/track/current",
            "image": [
              { "#text": "https://example.com/small.jpg", "size": "small" },
              { "#text": "https://example.com/large.jpg", "size": "large" }
            ],
            "@attr": { "nowplaying": "true" }
          }]
        }
      }
      """);
    var service = CreateService(
      spotifyHandler: new StaticJsonHandler("{}"),
      lastFmHandler: lastFmHandler);

    var first = await service.GetNowPlaying();
    var second = await service.GetNowPlaying();

    Assert.True(first.IsPlaying);
    Assert.Equal("Last.fm song", first.Title);
    Assert.Equal("Last.fm artist", first.Artist);
    Assert.Equal("Last.fm album", first.Album);
    Assert.Equal("https://example.com/large.jpg", first.AlbumImageUrl);
    Assert.Same(first, second);
    Assert.Equal(1, lastFmHandler.RequestCount);
  }

  [Fact]
  public async Task NowPlayingFailureReturnsEmptyStatus500ViewModel()
  {
    var service = CreateService(
      spotifyHandler: new StaticJsonHandler("{}"),
      lastFmHandler: new StaticJsonHandler("lastfm failed", HttpStatusCode.InternalServerError));

    var response = await service.GetNowPlaying();

    Assert.False(response.IsPlaying);
    Assert.Null(response.Maintenance);
    Assert.Equal(500, response.Status);
    Assert.Equal("", response.Album);
    Assert.Equal("", response.AlbumImageUrl);
    Assert.Equal("", response.Artist);
    Assert.Equal("", response.SongUrl);
    Assert.Equal("", response.Title);
  }

  [Fact]
  public async Task SpotifyNowPlayingUsesSpotifyAndCachesResult()
  {
    Environment.SetEnvironmentVariable("SHOULD_CALL_SPOTIFY", "true");
    var spotifyHandler = new StaticJsonHandler("""
      {
        "is_playing": true,
        "item": {
          "name": "Spotify song",
          "artists": [{ "name": "Spotify artist" }],
          "album": {
            "name": "Spotify album",
            "images": [{ "url": "https://example.com/spotify.jpg" }]
          },
          "external_urls": { "spotify": "https://open.spotify.com/track/spotify" }
        }
      }
      """);
    var service = CreateService(
      spotifyHandler: spotifyHandler,
      lastFmHandler: new StaticJsonHandler("{}"));

    var first = await service.GetSpotifyNowPlaying();
    var second = await service.GetSpotifyNowPlaying();

    Assert.True(first.IsPlaying);
    Assert.Equal("Spotify song", first.Title);
    Assert.Equal("Spotify artist", first.Artist);
    Assert.Equal("Spotify album", first.Album);
    Assert.Equal("https://example.com/spotify.jpg", first.AlbumImageUrl);
    Assert.Same(first, second);
    Assert.Equal(1, spotifyHandler.RequestCount);
  }

  [Fact]
  public async Task SpotifyReturnsEmptyResponseWhenItemIsNotPlaying()
  {
    Environment.SetEnvironmentVariable("SHOULD_CALL_SPOTIFY", "true");
    var spotifyApi = new SpotifyApi(
      CreateHttpClient(new StaticJsonHandler("""
      {
        "is_playing": false,
        "item": {
          "name": "Paused song",
          "artists": [{ "name": "Paused artist" }],
          "album": {
            "name": "Paused album",
            "images": [{ "url": "https://example.com/cover.jpg" }]
          },
          "external_urls": { "spotify": "https://open.spotify.com/track/paused" }
        }
      }
      """)),
      new SpotifyOptions { AccessToken = "test-token" });
    var lastFmApi = new LastFmApi(
      CreateHttpClient(new StaticJsonHandler("{}"), "https://lastfm.test/"),
      new LastFmOptions { ApiKey = "api-key", Username = "user" });
    var service = new NowPlayingService(
      new MemoryCache(),
      spotifyApi,
      lastFmApi,
      new TestLambdaLogger(),
      new FeatureFlagsOptions { ShouldCallSpotify = true });

    var response = await service.HandleNowPlaying("spotify");

    Assert.False(response.IsPlaying);
    Assert.Null(response.Maintenance);
    Assert.Equal(200, response.Status);
    Assert.Equal("", response.Album);
    Assert.Equal("", response.AlbumImageUrl);
    Assert.Equal("", response.Artist);
    Assert.Equal("", response.SongUrl);
    Assert.Equal("", response.Title);
  }

  private static NowPlayingService CreateService(StaticJsonHandler spotifyHandler, StaticJsonHandler lastFmHandler)
  {
    var spotifyApi = new SpotifyApi(
      CreateHttpClient(spotifyHandler),
      new SpotifyOptions { AccessToken = "test-token" });
    var lastFmApi = new LastFmApi(
      CreateHttpClient(lastFmHandler, "https://lastfm.test/"),
      new LastFmOptions { ApiKey = "api-key", Username = "user" });

    return new NowPlayingService(
      new MemoryCache(),
      spotifyApi,
      lastFmApi,
      new TestLambdaLogger(),
      new FeatureFlagsOptions { ShouldCallSpotify = true });
  }

  private static HttpClient CreateHttpClient(HttpMessageHandler handler, string baseAddress = "https://api.spotify.test/v1/")
  {
    return new HttpClient(handler)
    {
      BaseAddress = new Uri(baseAddress)
    };
  }

  private sealed class StaticJsonHandler(
    string json,
    HttpStatusCode statusCode = HttpStatusCode.OK) : HttpMessageHandler
  {
    public int RequestCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      RequestCount++;
      var response = new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
      };
      response.StatusCode = statusCode;

      return Task.FromResult(response);
    }
  }
}
