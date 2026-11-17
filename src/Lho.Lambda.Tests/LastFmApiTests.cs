using System.Net;
using System.Text;
using Lho.Lambda.Clients.LastFm;
using Lho.Lambda.RuntimeConfiguration.Options;
using Xunit;

namespace Lho.Lambda.Tests;

public class LastFmApiTests
{
    [Fact]
    public async Task HiddenRecentListeningIsReportedWithTheAccountName()
    {
        var api = CreateApi(HttpStatusCode.Forbidden, /*lang=json,strict*/ """
      { "message": "Login: User required to be logged in", "error": 17 }
      """);

        var exception = await Assert.ThrowsAsync<LastFmServiceException>(() => api.GetRecentTracks());

        Assert.Equal(LastFmErrorCode.LoginRequired, exception.ErrorCode);
        Assert.Contains("'private-user'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("Hide recent listening information", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task UnknownUserIsReportedWithTheAccountName()
    {
        var api = CreateApi(HttpStatusCode.NotFound, /*lang=json,strict*/ """
      { "message": "User not found", "error": 6 }
      """);

        var exception = await Assert.ThrowsAsync<LastFmServiceException>(() => api.GetTopTracks());

        Assert.Equal(LastFmErrorCode.InvalidParameters, exception.ErrorCode);
        Assert.Contains("'private-user'", exception.Message, StringComparison.Ordinal);
        Assert.Contains("LASTFM_USERNAME", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvalidApiKeyIsReported()
    {
        var api = CreateApi(HttpStatusCode.Forbidden, /*lang=json,strict*/ """
      { "message": "Invalid API key - You must be granted a valid key by last.fm", "error": 10 }
      """);

        var exception = await Assert.ThrowsAsync<LastFmServiceException>(() => api.GetRecentTracks());

        Assert.Equal(LastFmErrorCode.InvalidApiKey, exception.ErrorCode);
        Assert.Contains("LASTFM_API_KEY", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public async Task NonJsonFailureKeepsTheHttpStatusAndBody()
    {
        var api = CreateApi(HttpStatusCode.BadGateway, "<html>upstream down</html>");

        var exception = await Assert.ThrowsAsync<LastFmServiceException>(() => api.GetRecentTracks());

        Assert.Null(exception.ErrorCode);
        Assert.Contains("502", exception.Message, StringComparison.Ordinal);
        Assert.Contains("upstream down", exception.Message, StringComparison.Ordinal);
    }

    private static LastFmApi CreateApi(HttpStatusCode statusCode, string body)
    {
        var httpClient = new HttpClient(new StaticHandler(statusCode, body))
        {
            BaseAddress = new Uri("https://lastfm.test/")
        };

        return new LastFmApi(httpClient, new LastFmOptions { ApiKey = "api-key", Username = "private-user" });
    }

    private sealed class StaticHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            };

            return Task.FromResult(response);
        }
    }
}
