using System.Net;
using System.Text;
using Lho.Lambda.Clients.Spotify;
using Lho.Lambda.RuntimeConfiguration.Options;
using Xunit;

namespace Lho.Lambda.Tests;

public class SpotifyApiTests
{
  private const string NowPlayingJson = """
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
    """;

  [Fact]
  public async Task RefreshFlowRequestsTokenAndSendsBearer()
  {
    var handler = new RoutingHandler(tokenJson: TokenJson("tok-1"), apiJson: NowPlayingJson);
    var api = CreateApi(handler, new MutableTimeProvider());

    var response = await api.GetNowPlaying();

    Assert.NotNull(response);
    Assert.Equal(1, handler.TokenRequestCount);
    Assert.Equal("Bearer tok-1", handler.LastAuthorizationHeader);
  }

  [Fact]
  public async Task CachedTokenIsReusedAcrossCalls()
  {
    var handler = new RoutingHandler(tokenJson: TokenJson("tok-1"), apiJson: NowPlayingJson);
    var api = CreateApi(handler, new MutableTimeProvider());

    await api.GetNowPlaying();
    await api.GetNowPlaying();

    Assert.Equal(1, handler.TokenRequestCount);
    Assert.Equal(2, handler.ApiRequestCount);
  }

  [Fact]
  public async Task ExpiredTokenIsRefreshed()
  {
    var time = new MutableTimeProvider();
    var handler = new RoutingHandler(tokenJson: TokenJson("tok-1", expiresIn: 3600), apiJson: NowPlayingJson);
    var api = CreateApi(handler, time);

    await api.GetNowPlaying();
    // The token is cached for expires_in - 60 seconds; step past that window.
    time.UtcNow = time.UtcNow.AddSeconds(3600);
    await api.GetNowPlaying();

    Assert.Equal(2, handler.TokenRequestCount);
  }

  [Fact]
  public async Task StaticAccessTokenSkipsTokenEndpoint()
  {
    var handler = new RoutingHandler(tokenJson: TokenJson("tok-1"), apiJson: NowPlayingJson);
    var api = new SpotifyApi(
      CreateHttpClient(handler),
      new SpotifyOptions { AccessToken = "static-token" });

    await api.GetNowPlaying();

    Assert.Equal(0, handler.TokenRequestCount);
    Assert.Equal("Bearer static-token", handler.LastAuthorizationHeader);
  }

  private static string TokenJson(string accessToken, int expiresIn = 3600)
  {
    return $$"""{ "access_token": "{{accessToken}}", "expires_in": {{expiresIn}} }""";
  }

  private static SpotifyApi CreateApi(RoutingHandler handler, TimeProvider timeProvider)
  {
    return new SpotifyApi(
      CreateHttpClient(handler),
      new SpotifyOptions
      {
        ClientId = "client-id",
        ClientSecret = "client-secret",
        RefreshToken = "refresh-token"
      },
      timeProvider);
  }

  private static HttpClient CreateHttpClient(HttpMessageHandler handler)
  {
    return new HttpClient(handler)
    {
      BaseAddress = new Uri("https://api.spotify.test/v1/")
    };
  }

  private sealed class MutableTimeProvider : TimeProvider
  {
    public DateTimeOffset UtcNow { get; set; } = new(2026, 7, 5, 12, 0, 0, TimeSpan.Zero);

    public override DateTimeOffset GetUtcNow()
    {
      return UtcNow;
    }
  }

  private sealed class RoutingHandler(string tokenJson, string apiJson) : HttpMessageHandler
  {
    public int TokenRequestCount { get; private set; }

    public int ApiRequestCount { get; private set; }

    public string? LastAuthorizationHeader { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      string body;
      if (request.RequestUri?.Host == "accounts.spotify.com")
      {
        TokenRequestCount++;
        body = tokenJson;
      }
      else
      {
        ApiRequestCount++;
        LastAuthorizationHeader = request.Headers.Authorization?.ToString();
        body = apiJson;
      }

      return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
      });
    }
  }
}
