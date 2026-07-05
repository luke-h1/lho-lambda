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

    var first = await service.HandleNowPlaying(null);
    var second = await service.HandleNowPlaying(null);

    Assert.Equal("lastfm", first.Provider);
    Assert.True(first.Response.IsPlaying);
    Assert.Equal("Last.fm song", first.Response.Title);
    Assert.Equal("Last.fm artist", first.Response.Artist);
    Assert.Equal("Last.fm album", first.Response.Album);
    Assert.Equal("https://example.com/large.jpg", first.Response.AlbumImageUrl);
    Assert.Same(first.Response, second.Response);
    Assert.Equal(1, lastFmHandler.RequestCount);
  }

  [Fact]
  public async Task NowPlayingCachesEmptyResponseWhenNothingIsPlaying()
  {
    var lastFmHandler = new StaticJsonHandler("""
      {
        "recenttracks": {
          "track": [{
            "name": "Older song",
            "artist": { "#text": "Older artist" },
            "album": { "#text": "Older album" },
            "url": "https://last.fm/track/older",
            "image": []
          }]
        }
      }
      """);
    var service = CreateService(
      spotifyHandler: new StaticJsonHandler("{}"),
      lastFmHandler: lastFmHandler);

    var first = await service.HandleNowPlaying(null);
    var second = await service.HandleNowPlaying(null);

    Assert.False(first.Response.IsPlaying);
    Assert.Equal(200, first.Response.Status);
    Assert.Same(first.Response, second.Response);
    Assert.Equal(1, lastFmHandler.RequestCount);
  }

  [Fact]
  public async Task NowPlayingDoesNotCacheFailureResponses()
  {
    var lastFmHandler = new StaticJsonHandler("lastfm failed", HttpStatusCode.InternalServerError);
    var service = CreateService(
      spotifyHandler: new StaticJsonHandler("{}"),
      lastFmHandler: lastFmHandler);

    var first = await service.HandleNowPlaying(null);
    var second = await service.HandleNowPlaying(null);

    Assert.Equal(500, first.Response.Status);
    Assert.Equal(500, second.Response.Status);
    Assert.Equal(2, lastFmHandler.RequestCount);
  }

  [Fact]
  public async Task NowPlayingFailureReturnsEmptyStatus500ViewModel()
  {
    var service = CreateService(
      spotifyHandler: new StaticJsonHandler("{}"),
      lastFmHandler: new StaticJsonHandler("lastfm failed", HttpStatusCode.InternalServerError));

    var result = await service.HandleNowPlaying(null);
    var response = result.Response;

    Assert.False(response.IsPlaying);
    Assert.False(response.Maintenance);
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

    var first = await service.HandleNowPlaying("spotify");
    var second = await service.HandleNowPlaying("spotify");

    Assert.Equal("spotify", first.Provider);
    Assert.True(first.Response.IsPlaying);
    Assert.Equal("Spotify song", first.Response.Title);
    Assert.Equal("Spotify artist", first.Response.Artist);
    Assert.Equal("Spotify album", first.Response.Album);
    Assert.Equal("https://example.com/spotify.jpg", first.Response.AlbumImageUrl);
    Assert.Same(first.Response, second.Response);
    Assert.Equal(1, spotifyHandler.RequestCount);
  }

  [Fact]
  public async Task SpotifyReturnsEmptyResponseWhenItemIsNotPlaying()
  {
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
      new FeatureFlagsOptions { ShouldCallSpotify = true },
      new ObservabilityOptions());

    var response = (await service.HandleNowPlaying("spotify")).Response;

    Assert.False(response.IsPlaying);
    Assert.False(response.Maintenance);
    Assert.Equal(200, response.Status);
    Assert.Equal("", response.Album);
    Assert.Equal("", response.AlbumImageUrl);
    Assert.Equal("", response.Artist);
    Assert.Equal("", response.SongUrl);
    Assert.Equal("", response.Title);
  }

  [Fact]
  public async Task InvalidProviderFallsBackToLastFm()
  {
    var spotifyHandler = new StaticJsonHandler("{}");
    var lastFmHandler = new StaticJsonHandler("""
      { "recenttracks": { "track": [] } }
      """);
    var service = CreateService(spotifyHandler, lastFmHandler);

    var result = await service.HandleNowPlaying("garbage");

    Assert.Equal("lastfm", result.Provider);
    Assert.Equal(1, lastFmHandler.RequestCount);
    Assert.Equal(0, spotifyHandler.RequestCount);
  }

  [Fact]
  public async Task SpotifyProviderIsCaseInsensitive()
  {
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

    var result = await service.HandleNowPlaying("SPOTIFY");

    Assert.Equal("spotify", result.Provider);
    Assert.Equal("Spotify song", result.Response.Title);
    Assert.Equal(1, spotifyHandler.RequestCount);
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
      new FeatureFlagsOptions { ShouldCallSpotify = true },
      new ObservabilityOptions());
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
