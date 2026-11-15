using System.Net;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Lho.Lambda.Clients.LastFm;
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
    public async Task NowPlayingReturnsCurrentLastFmTrack()
    {
        var function = CreateFunction(new StaticJsonHandler("""
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
          CreateRequest("/api/now-playing"),
          new TestLambdaContext());
        var body = JsonDocument.Parse(response.Body).RootElement;

        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Last.fm song", body.GetProperty("title").GetString());
        Assert.Equal("Last.fm artist", body.GetProperty("artist").GetString());
    }

    [Fact]
    public async Task RecentTracksReturnsMappedListWithNowPlayingAndTimestamps()
    {
        var function = CreateFunction(new StaticJsonHandler(/*lang=json,strict*/ """
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

    [Fact]
    public async Task TopTracksReturnsMappedListFromLastFm()
    {
        var lastFmHandler = new StaticJsonHandler(/*lang=json,strict*/ """
        {
          "toptracks": {
            "track": [
              {
                "name": "Top song",
                "artist": { "name": "Top artist" },
                "url": "https://last.fm/track/top",
                "image": [
                  { "#text": "https://example.com/small.jpg", "size": "small" },
                  { "#text": "https://example.com/large.jpg", "size": "extralarge" }
                ],
                "playcount": "9"
              }
            ]
          }
        }
        """);
        var function = CreateFunction(lastFmHandler);

        var response = await function.FunctionHandler(
          CreateRequest("/api/top-tracks", rawQueryString: "time_range=short_term&limit=5"),
          new TestLambdaContext());
        var tracks = JsonDocument.Parse(response.Body).RootElement.GetProperty("tracks");

        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, tracks.GetArrayLength());

        var first = tracks[0];
        Assert.Equal("Top song", first.GetProperty("title").GetString());
        Assert.Equal("Top artist", first.GetProperty("artist").GetString());
        Assert.Equal("", first.GetProperty("album").GetString());
        Assert.Equal("https://example.com/large.jpg", first.GetProperty("albumImageUrl").GetString());
        Assert.Equal("https://last.fm/track/top", first.GetProperty("songUrl").GetString());
        Assert.Contains("period=1month", lastFmHandler.LastRequestUri?.Query);
        Assert.Contains("limit=5", lastFmHandler.LastRequestUri?.Query);
    }

    [Theory]
    [InlineData("""{ "toptracks": { "track": { "name": "Solo", "artist": { "name": "A" }, "url": "https://last.fm/t", "image": [] } } }""", 1)]
    [InlineData("""{ "toptracks": { "track": [{ "name": "Solo", "artist": { "name": "A" }, "url": "https://last.fm/t", "image": [] }] } }""", 1)]
    [InlineData("""{ "toptracks": { "track": [] } }""", 0)]
    [InlineData("""{ "toptracks": { "track": null } }""", 0)]
    [InlineData("""{ "toptracks": {} }""", 0)]
    public async Task TopTracksNormalisesLastFmTrackShapes(string payload, int expectedCount)
    {
        var function = CreateFunction(new StaticJsonHandler(payload));

        var response = await function.FunctionHandler(
          CreateRequest("/api/top-tracks"),
          new TestLambdaContext());
        var tracks = JsonDocument.Parse(response.Body).RootElement.GetProperty("tracks");

        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedCount, tracks.GetArrayLength());
    }

    [Theory]
    [InlineData("""{ "recenttracks": { "track": { "name": "Solo", "artist": { "#text": "A" }, "album": { "#text": "B" }, "url": "https://last.fm/t", "image": [] } } }""", 1)]
    [InlineData("""{ "recenttracks": { "track": [] } }""", 0)]
    [InlineData("""{ "recenttracks": {} }""", 0)]
    public async Task RecentTracksNormalisesLastFmTrackShapes(string payload, int expectedCount)
    {
        var function = CreateFunction(new StaticJsonHandler(payload));

        var response = await function.FunctionHandler(
          CreateRequest("/api/recent-tracks"),
          new TestLambdaContext());
        var tracks = JsonDocument.Parse(response.Body).RootElement.GetProperty("tracks");

        Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(expectedCount, tracks.GetArrayLength());
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

    private static ApiFunction CreateFunction(StaticJsonHandler lastFmHandler)
    {
        var lastFmApi = new LastFmApi(
          CreateHttpClient(lastFmHandler),
          new LastFmOptions { ApiKey = "api-key", Username = "user" });

        return new ApiFunction(
          new RuntimeConfig(),
          new MemoryCache(),
          lastFmApi);
    }

    private static HttpClient CreateHttpClient(HttpMessageHandler handler)
    {
        return new HttpClient(handler)
        {
            BaseAddress = new Uri("https://lastfm.test/")
        };
    }

    private sealed class StaticJsonHandler(string json) : HttpMessageHandler
    {
        public int RequestCount { get; private set; }

        public Uri? LastRequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            LastRequestUri = request.RequestUri;
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
