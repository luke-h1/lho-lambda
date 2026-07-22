using System.Net;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Lho.Lambda.Clients.LastFm;
using Lho.Lambda.Clients.Spotify;
using Lho.Lambda.Functions;
using Lho.Lambda.RuntimeConfiguration;
using Lho.Lambda.RuntimeConfiguration.Options;
using Lho.Lambda.Utils;
using Xunit;

namespace Lho.Lambda.Tests;

public class ApiFunctionTests
{
  [Fact]
  public async Task MissingRouteReturnsNotFound()
  {
    var function = new ApiFunction();

    var response = await function.FunctionHandler(CreateRequest("/api/missing"), new TestLambdaContext());

    Assert.Equal((int)HttpStatusCode.NotFound, response.StatusCode);
    Assert.Equal("Not Found", JsonDocument.Parse(response.Body).RootElement.GetProperty("error").GetString());
  }

  [Fact]
  public async Task NowPlayingLastFmProviderUsesMainLastFmPath()
  {
    var function = CreateFunction(
      spotifyHandler: new StaticJsonHandler("""
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
      """),
      lastFmHandler: new StaticJsonHandler("""
      {
        "recenttracks": {
          "track": [{
            "name": "Last.fm song",
            "artist": { "#text": "Last.fm artist" },
            "album": { "#text": "Last.fm album" },
            "url": "https://last.fm/track/current",
            "image": [{ "#text": "https://example.com/lastfm.jpg", "size": "large" }],
            "@attr": { "nowplaying": "true" }
          }]
        }
      }
      """));

    var response = await function.FunctionHandler(
      CreateRequest("/api/now-playing", rawQueryString: "provider=lastfm"),
      new TestLambdaContext());
    var body = JsonDocument.Parse(response.Body).RootElement;

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("Last.fm song", body.GetProperty("title").GetString());
    Assert.Equal("Last.fm artist", body.GetProperty("artist").GetString());
  }

  [Fact]
  public async Task NowPlayingSpotifyProviderUsesSpotifyPath()
  {
    var function = CreateFunction(
      spotifyHandler: new StaticJsonHandler("""
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
      """),
      lastFmHandler: new StaticJsonHandler("""
      {
        "recenttracks": {
          "track": [{
            "name": "Last.fm song",
            "artist": { "#text": "Last.fm artist" },
            "album": { "#text": "Last.fm album" },
            "url": "https://last.fm/track/current",
            "image": [{ "#text": "https://example.com/lastfm.jpg", "size": "large" }],
            "@attr": { "nowplaying": "true" }
          }]
        }
      }
      """));

    var response = await function.FunctionHandler(
      CreateRequest("/api/now-playing", rawQueryString: "provider=spotify"),
      new TestLambdaContext());
    var body = JsonDocument.Parse(response.Body).RootElement;

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
    Assert.Equal("Spotify song", body.GetProperty("title").GetString());
    Assert.Equal("Spotify artist", body.GetProperty("artist").GetString());
  }

  [Fact]
  public async Task NowPlayingUsesInjectedFeatureFlags()
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
    var spotifyApi = new SpotifyApi(
      CreateHttpClient(spotifyHandler),
      new SpotifyOptions { AccessToken = "test-token" });
    var function = new ApiFunction(
      new RuntimeConfig
      {
        Features = new FeatureFlagsOptions { ShouldCallSpotify = false }
      },
      new MemoryCache(),
      spotifyApi,
      new LastFmApi(
        CreateHttpClient(new StaticJsonHandler("{}"), "https://lastfm.test/"),
        new LastFmOptions { ApiKey = "api-key", Username = "user" }));

    var response = await function.FunctionHandler(
      CreateRequest("/api/now-playing", rawQueryString: "provider=spotify"),
      new TestLambdaContext());
    var body = JsonDocument.Parse(response.Body).RootElement;

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
    Assert.False(body.GetProperty("isPlaying").GetBoolean());
    Assert.True(body.GetProperty("maintenance").GetBoolean());
    Assert.Equal(0, spotifyHandler.RequestCount);
  }

  [Fact]
  public async Task RecentTracksReturnsMappedListWithNowPlayingAndTimestamps()
  {
    var function = CreateFunction(
      spotifyHandler: new StaticJsonHandler("{}"),
      lastFmHandler: new StaticJsonHandler("""
      {
        "recenttracks": {
          "track": [
            {
              "name": "Live song",
              "artist": { "#text": "Live artist" },
              "album": { "#text": "Live album" },
              "url": "https://last.fm/track/live",
              "image": [
                { "#text": "https://example.com/small.jpg", "size": "small" },
                { "#text": "https://example.com/large.jpg", "size": "large" }
              ],
              "@attr": { "nowplaying": "true" }
            },
            {
              "name": "Older song",
              "artist": { "#text": "Older artist" },
              "album": { "#text": "Older album" },
              "url": "https://last.fm/track/older",
              "image": [{ "#text": "https://example.com/older.jpg", "size": "large" }],
              "date": { "uts": "1687000000", "#text": "17 Jun 2023, 12:00" }
            }
          ]
        }
      }
      """));

    var response = await function.FunctionHandler(
      CreateRequest("/api/recent-tracks", rawQueryString: "limit=10"),
      new TestLambdaContext());
    var tracks = JsonDocument.Parse(response.Body).RootElement.GetProperty("tracks");

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
    Assert.Equal(2, tracks.GetArrayLength());

    var first = tracks[0];
    Assert.Equal("Live song", first.GetProperty("title").GetString());
    Assert.Equal("Live artist", first.GetProperty("artist").GetString());
    Assert.Equal("https://example.com/large.jpg", first.GetProperty("albumImageUrl").GetString());
    Assert.True(first.GetProperty("nowPlaying").GetBoolean());
    Assert.Equal(JsonValueKind.Null, first.GetProperty("playedAt").ValueKind);

    var second = tracks[1];
    Assert.Equal("Older song", second.GetProperty("title").GetString());
    Assert.False(second.GetProperty("nowPlaying").GetBoolean());
    Assert.Equal(1687000000L, second.GetProperty("playedAt").GetInt64());
  }

  [Theory]
  [InlineData("/api/health", "GET")]
  [InlineData("/api/health", "HEAD")]
  [InlineData("/api/version", "GET")]
  public async Task ApiHealthAndVersionRoutesReturnOk(string path, string method)
  {
    var function = new ApiFunction();

    var response = await function.FunctionHandler(CreateRequest(path, method), new TestLambdaContext());

    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
  }

  private static APIGatewayHttpApiV2ProxyRequest CreateRequest(
    string path,
    string method = "GET",
    string rawQueryString = "")
  {
    return new APIGatewayHttpApiV2ProxyRequest
    {
      RawPath = path,
      RawQueryString = rawQueryString,
      RequestContext = new APIGatewayHttpApiV2ProxyRequest.ProxyRequestContext
      {
        Http = new APIGatewayHttpApiV2ProxyRequest.HttpDescription
        {
          Method = method,
          Path = path
        }
      }
    };
  }

  private static ApiFunction CreateFunction(StaticJsonHandler spotifyHandler, StaticJsonHandler lastFmHandler)
  {
    var spotifyApi = new SpotifyApi(
      CreateHttpClient(spotifyHandler),
      new SpotifyOptions { AccessToken = "test-token" });
    var lastFmApi = new LastFmApi(
      CreateHttpClient(lastFmHandler, "https://lastfm.test/"),
      new LastFmOptions { ApiKey = "api-key", Username = "user" });

    return new ApiFunction(
      new RuntimeConfig
      {
        Features = new FeatureFlagsOptions { ShouldCallSpotify = true }
      },
      new MemoryCache(),
      spotifyApi,
      lastFmApi);
  }

  private static HttpClient CreateHttpClient(HttpMessageHandler handler, string baseAddress = "https://api.spotify.test/v1/")
  {
    return new HttpClient(handler)
    {
      BaseAddress = new Uri(baseAddress)
    };
  }

  private sealed class StaticJsonHandler(string json) : HttpMessageHandler
  {
    public int RequestCount { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      RequestCount++;
      var response = new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
      };

      return Task.FromResult(response);
    }
  }
}
