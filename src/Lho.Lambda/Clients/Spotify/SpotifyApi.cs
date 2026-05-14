using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Lho.Lambda.Models;
using Lho.Lambda.Utils;

namespace Lho.Lambda.Clients.Spotify;

public class SpotifyApi
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true
  };

  private readonly HttpClient _httpClient;
  private readonly string _baseUrl;
  private readonly string? _accessToken;
  private readonly string? _clientId;
  private readonly string? _clientSecret;
  private readonly string? _refreshToken;
  private string? _cachedAccessToken;
  private DateTimeOffset? _tokenExpiresAt;

  public SpotifyApi()
      : this(new HttpClient { Timeout = TimeSpan.FromSeconds(10) }, "https://api.spotify.com/v1")
  {
  }

  public SpotifyApi(HttpClient httpClient, string baseUrl, string? accessToken = null)
  {
    _httpClient = httpClient;
    _baseUrl = baseUrl;
    _accessToken = accessToken ?? EnvironmentConfig.Spotify.AccessToken;
    _clientId = EnvironmentConfig.Spotify.ClientId;
    _clientSecret = EnvironmentConfig.Spotify.ClientSecret;
    _refreshToken = EnvironmentConfig.Spotify.RefreshToken;
  }

  public async Task<SpotifyResponse?> GetNowPlaying()
  {
    var request = new HttpRequestMessage(HttpMethod.Get, $"{_baseUrl}/me/player/currently-playing");
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    var response = await _httpClient.SendAsync(request);
    if (response.StatusCode == HttpStatusCode.NoContent)
    {
      return null;
    }

    await EnsureSuccess(response);
    return await ReadJson<SpotifyResponse>(response);
  }

  public async Task<SpotifyTopTracksResponse> GetTopTracks(string timeRange = "medium_term", int limit = 10)
  {
    var boundedLimit = Math.Clamp(limit, 1, 50);
    var requestUri = $"{_baseUrl}/me/top/tracks?time_range={Uri.EscapeDataString(timeRange)}&limit={boundedLimit}";
    var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await GetAccessToken());
    request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    var response = await _httpClient.SendAsync(request);
    await EnsureSuccess(response);

    return await ReadJson<SpotifyTopTracksResponse>(response)
        ?? throw new SpotifyServiceException("Empty response from Spotify API");
  }

  private async Task<string> GetAccessToken()
  {
    if (!string.IsNullOrEmpty(_accessToken))
    {
      return _accessToken;
    }

    if (!string.IsNullOrEmpty(_cachedAccessToken) && _tokenExpiresAt > DateTimeOffset.UtcNow)
    {
      return _cachedAccessToken;
    }

    if (string.IsNullOrEmpty(_refreshToken))
    {
      throw new SpotifyServiceException("Missing Spotify refresh token");
    }

    if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret))
    {
      throw new SpotifyServiceException("Missing Spotify client ID or client secret");
    }

    using var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
    var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
    request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      ["grant_type"] = "refresh_token",
      ["refresh_token"] = _refreshToken
    });

    var response = await _httpClient.SendAsync(request);
    await EnsureSuccess(response);

    var tokenResponse = await ReadJson<TokenResponse>(response)
        ?? throw new SpotifyServiceException("Empty response from token endpoint");

    _cachedAccessToken = tokenResponse.AccessToken;
    _tokenExpiresAt = DateTimeOffset.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60);

    return tokenResponse.AccessToken;
  }

  private static async Task EnsureSuccess(HttpResponseMessage response)
  {
    if (response.IsSuccessStatusCode)
    {
      return;
    }

    var body = await response.Content.ReadAsStringAsync();
    throw new SpotifyServiceException($"HTTP error with status code: {(int)response.StatusCode} - {body}");
  }

  private static async Task<T?> ReadJson<T>(HttpResponseMessage response)
  {
    var body = await response.Content.ReadAsStringAsync();
    if (string.IsNullOrWhiteSpace(body))
    {
      throw new SpotifyServiceException("Empty response from Spotify API");
    }

    return JsonSerializer.Deserialize<T>(body, JsonOptions);
  }
}

public class SpotifyServiceException(string message) : Exception(message);
