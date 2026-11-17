using System.Text.Json;
using Lho.Lambda.Models;
using Lho.Lambda.RuntimeConfiguration.Options;
using Lho.Lambda.Serialization;

namespace Lho.Lambda.Clients.LastFm;

public class LastFmApi(HttpClient httpClient, LastFmOptions lastFmOptions)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        TypeInfoResolver = LastFmJsonContext.Default
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly LastFmOptions _lastFmOptions = lastFmOptions;


    public async Task<LastFmTopTracksResponse> GetTopTracks(string period = "6month", int limit = 20)
    {
        var credentials = _lastFmOptions.RequireCredentials();

        var boundedLimit = Math.Clamp(limit, 1, 50);
        var requestUri = $"?method=user.gettoptracks&user={Uri.EscapeDataString(credentials.Username)}&api_key={Uri.EscapeDataString(credentials.ApiKey)}&format=json&period={Uri.EscapeDataString(period)}&limit={boundedLimit}";

        var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));

        await EnsureSuccess(response, credentials.Username);

        return await ReadJson<LastFmTopTracksResponse>(response) ?? throw new LastFmServiceException("Empty response from last.fm API");
    }

    public async Task<LastFmRecentTracksResponse> GetRecentTracks(int limit = 1)
    {
        var credentials = _lastFmOptions.RequireCredentials();

        var boundedLimit = Math.Clamp(limit, 1, 50);
        var requestUri = $"?method=user.getrecenttracks&user={Uri.EscapeDataString(credentials.Username)}&api_key={Uri.EscapeDataString(credentials.ApiKey)}&format=json&limit={boundedLimit}";

        var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));
        await EnsureSuccess(response, credentials.Username);

        return await ReadJson<LastFmRecentTracksResponse>(response) ??
               throw new LastFmServiceException("Empty response from last.fm API");
    }

    private static async Task EnsureSuccess(HttpResponseMessage response, string username)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync();
        var error = TryReadError(body);

        if (error is null)
        {
            throw new LastFmServiceException($"HTTP error with status code: {(int)response.StatusCode} - {body}");
        }

        throw new LastFmServiceException(DescribeError(error, username), error.Error);
    }

    private static LastFmErrorResponse? TryReadError(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        try
        {
            var error = JsonSerializer.Deserialize<LastFmErrorResponse>(body, JsonOptions);
            return error is { Error: > 0 } ? error : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string DescribeError(LastFmErrorResponse error, string username)
    {
        return error.Error switch
        {
            LastFmErrorCode.InvalidParameters =>
                $"Last.fm rejected the request for user '{username}': {error.Message}. Check that LASTFM_USERNAME is an existing Last.fm account.",
            LastFmErrorCode.InvalidApiKey =>
                $"Last.fm rejected the API key: {error.Message}. Check LASTFM_API_KEY.",
            LastFmErrorCode.LoginRequired =>
                $"Last.fm account '{username}' hides its recent listening, so the API cannot read recent tracks or now playing. Disable 'Hide recent listening information' in the account's privacy settings.",
            _ => $"Last.fm error {error.Error}: {error.Message}"
        };
    }

    private static async Task<T?> ReadJson<T>(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new LastFmServiceException("Empty response from Last.fm API");
        }

        return JsonSerializer.Deserialize<T>(body, JsonOptions);
    }
}

public static class LastFmErrorCode
{
    public const int InvalidParameters = 6;
    public const int InvalidApiKey = 10;
    public const int LoginRequired = 17;
}

public class LastFmServiceException(string message, int? errorCode = null) : Exception(message)
{
    public int? ErrorCode { get; } = errorCode;
}
