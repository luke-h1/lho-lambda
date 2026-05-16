using System.Net;
using System.Net.Sockets;
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
    Environment.SetEnvironmentVariable("SHOULD_CALL_SPOTIFY", "true");
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
  public async Task ApiHealthAndVersionRoutesPushInvocationMetrics()
  {
    var port = GetFreePort();
    using var listener = new HttpListener();
    listener.Prefixes.Add($"http://127.0.0.1:{port}/");
    listener.Start();

    ConfigureMetrics(port);
    try
    {
      var function = new ApiFunction();

      var healthGetMetric = await InvokeAndReadMetric(listener, function, CreateRequest("/api/health"));
      var healthHeadMetric = await InvokeAndReadMetric(listener, function, CreateRequest("/api/health", "HEAD"));
      var versionMetric = await InvokeAndReadMetric(listener, function, CreateRequest("/api/version"));

      Assert.Contains("route=\"/api/health\"", healthGetMetric);
      Assert.Contains("method=\"GET\"", healthGetMetric);
      Assert.Contains("status=\"200\"", healthGetMetric);
      Assert.Contains("route=\"/api/health\"", healthHeadMetric);
      Assert.Contains("method=\"HEAD\"", healthHeadMetric);
      Assert.Contains("status=\"200\"", healthHeadMetric);
      Assert.Contains("route=\"/api/version\"", versionMetric);
      Assert.Contains("method=\"GET\"", versionMetric);
      Assert.Contains("status=\"200\"", versionMetric);
    }
    finally
    {
      Environment.SetEnvironmentVariable("METRICS_ENABLED", null);
      Environment.SetEnvironmentVariable("PUSHGATEWAY_URL", null);
      Environment.SetEnvironmentVariable("PUSHGATEWAY_AUTH_HEADER", null);
      Environment.SetEnvironmentVariable("PROMETHEUS_JOB", null);
      Environment.SetEnvironmentVariable("ENVIRONMENT", null);
    }
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

  private static void ConfigureMetrics(int port)
  {
    Environment.SetEnvironmentVariable("METRICS_ENABLED", "true");
    Environment.SetEnvironmentVariable("PUSHGATEWAY_URL", $"http://127.0.0.1:{port}");
    Environment.SetEnvironmentVariable("PUSHGATEWAY_AUTH_HEADER", null);
    Environment.SetEnvironmentVariable("PROMETHEUS_JOB", "test-job");
    Environment.SetEnvironmentVariable("ENVIRONMENT", "test");
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

  private static async Task<string> InvokeAndReadMetric(
    HttpListener listener,
    ApiFunction function,
    APIGatewayHttpApiV2ProxyRequest request)
  {
    var requestTask = listener.GetContextAsync();
    var responseTask = function.FunctionHandler(request, new TestLambdaContext());

    var context = await requestTask.WaitAsync(TimeSpan.FromSeconds(2));
    using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
    var body = await reader.ReadToEndAsync();
    context.Response.StatusCode = (int)HttpStatusCode.Accepted;
    context.Response.Close();

    var response = await responseTask.WaitAsync(TimeSpan.FromSeconds(2));
    Assert.Equal((int)HttpStatusCode.OK, response.StatusCode);

    return body;
  }

  private static int GetFreePort()
  {
    using var listener = new TcpListener(IPAddress.Loopback, 0);
    listener.Start();
    return ((IPEndPoint)listener.LocalEndpoint).Port;
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
