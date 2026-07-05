using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Lho.Lambda.Models;
using Lho.Lambda.RuntimeConfiguration.Options;
using Lho.Lambda.Serialization;

namespace Lho.Lambda.Clients.Spotify;

public class SpotifyApi
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    TypeInfoResolver = SpotifyJsonContext.Default
  };

  private readonly HttpClient _httpClient;
  private readonly SpotifyOptions _spotifyOptions;
  private readonly TimeProvider _timeProvider;
  private string? _cachedAccessToken;
  private DateTimeOffset? _tokenExpiresAt;

  public SpotifyApi(HttpClient httpClient, SpotifyOptions spotifyOptions, TimeProvider? timeProvider = null)
  {
    _httpClient = httpClient;
    _spotifyOptions = spotifyOptions;
    _timeProvider = timeProvider ?? TimeProvider.System;
  }

  public async Task<SpotifyResponse?> GetNowPlaying()
  {
    var request = new HttpRequestMessage(HttpMethod.Get, "me/player/currently-playing");
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
    var requestUri = $"me/top/tracks?time_range={Uri.EscapeDataString(timeRange)}&limit={boundedLimit}";
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
    if (!string.IsNullOrEmpty(_spotifyOptions.AccessToken))
    {
      return _spotifyOptions.AccessToken;
    }

    if (!string.IsNullOrEmpty(_cachedAccessToken) && _tokenExpiresAt > _timeProvider.GetUtcNow())
    {
      return _cachedAccessToken;
    }

    var credentials = _spotifyOptions.RequireRefreshCredentials();

    using var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token");
    var encodedCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{credentials.ClientId}:{credentials.ClientSecret}"));
    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", encodedCredentials);
    request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
      ["grant_type"] = "refresh_token",
      ["refresh_token"] = credentials.RefreshToken
    });

    var response = await _httpClient.SendAsync(request);
    await EnsureSuccess(response);

    var tokenResponse = await ReadJson<TokenResponse>(response)
        ?? throw new SpotifyServiceException("Empty response from token endpoint");

    _cachedAccessToken = tokenResponse.AccessToken;
    _tokenExpiresAt = _timeProvider.GetUtcNow().AddSeconds(tokenResponse.ExpiresIn - 60);

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
