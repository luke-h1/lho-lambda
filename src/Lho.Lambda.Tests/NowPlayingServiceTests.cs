using System.Net;
using System.Text;
using Lho.Lambda.Clients.LastFm;
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
        var lastFmHandler = new StaticJsonHandler(/*lang=json,strict*/ """
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
        var service = CreateService(lastFmHandler);

        var first = await service.HandleNowPlaying();
        var second = await service.HandleNowPlaying();

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
        var lastFmHandler = new StaticJsonHandler(/*lang=json,strict*/ """
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
        var service = CreateService(lastFmHandler);

        var first = await service.HandleNowPlaying();
        var second = await service.HandleNowPlaying();

        Assert.False(first.Response.IsPlaying);
        Assert.Equal(200, first.Response.Status);
        Assert.Same(first.Response, second.Response);
        Assert.Equal(1, lastFmHandler.RequestCount);
    }

    [Fact]
    public async Task NowPlayingDoesNotCacheFailureResponses()
    {
        var lastFmHandler = new StaticJsonHandler("lastfm failed", HttpStatusCode.InternalServerError);
        var service = CreateService(lastFmHandler);

        var first = await service.HandleNowPlaying();
        var second = await service.HandleNowPlaying();

        Assert.Equal(500, first.Response.Status);
        Assert.Equal(500, second.Response.Status);
        Assert.Equal(2, lastFmHandler.RequestCount);
    }

    [Fact]
    public async Task NowPlayingFailureReturnsEmptyStatus500ViewModel()
    {
        var service = CreateService(new StaticJsonHandler("lastfm failed", HttpStatusCode.InternalServerError));

        var result = await service.HandleNowPlaying();
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
    public async Task NowPlayingReturnsEmptyResponseWhenNoTracks()
    {
        var lastFmHandler = new StaticJsonHandler(/*lang=json,strict*/ """
      { "recenttracks": { "track": [] } }
      """);
        var service = CreateService(lastFmHandler);

        var result = await service.HandleNowPlaying();

        Assert.Equal("lastfm", result.Provider);
        Assert.False(result.Response.IsPlaying);
        Assert.Equal(200, result.Response.Status);
        Assert.Equal(1, lastFmHandler.RequestCount);
    }

    private static NowPlayingService CreateService(StaticJsonHandler lastFmHandler)
    {
        var lastFmApi = new LastFmApi(
          CreateHttpClient(lastFmHandler),
          new LastFmOptions { ApiKey = "api-key", Username = "user" });

        return new NowPlayingService(
          new MemoryCache(),
          lastFmApi,
          new TestLambdaLogger(),
          new ObservabilityOptions());
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://lastfm.test/")
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
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
                StatusCode = statusCode
            };

            return Task.FromResult(response);
        }
    }
}
