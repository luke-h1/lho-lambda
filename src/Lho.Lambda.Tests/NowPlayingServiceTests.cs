using System.Net;
using System.Text;
using Lho.Lambda.Clients.LastFm;
using Lho.Lambda.Clients.Spotify;
using Lho.Lambda.Services;
using Lho.Lambda.Utils;
using Xunit;

namespace Lho.Lambda.Tests;

public class NowPlayingServiceTests
{
  [Fact]
  public async Task SpotifyReturnsEmptyResponseWhenItemIsNotPlaying()
  {
    Environment.SetEnvironmentVariable("SHOULD_CALL_SPOTIFY", "true");
    var spotifyApi = new SpotifyApi(
      new HttpClient(new StaticJsonHandler("""
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
      "https://api.spotify.test/v1",
      accessToken: "test-token");
    var lastFmApi = new LastFmApi(new HttpClient(new StaticJsonHandler("{}")), "https://lastfm.test/", "api-key", "user");
    var service = new NowPlayingService(new MemoryCache(), spotifyApi, lastFmApi, new TestLambdaLogger());

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

  private sealed class StaticJsonHandler(string json) : HttpMessageHandler
  {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      var response = new HttpResponseMessage(HttpStatusCode.OK)
      {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
      };

      return Task.FromResult(response);
    }
  }
}
