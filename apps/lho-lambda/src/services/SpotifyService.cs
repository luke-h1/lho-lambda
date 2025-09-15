using System.Net;
using System.Text;
using System.Text.Json;
using LhoLambda.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;

namespace LhoLambda.services;

public interface ISpotifyService
{
  Task<SpotifyNowPlayingResponse?> GetNowPlayingAsync();
}

public class SpotifyService : ISpotifyService
{
  private const string NowPlayingEndpoint = "https://api.spotify.com/v1/me/player/currently-playing";
  private const string TokenEndpoint = "https://accounts.spotify.com/api/token";
  private const string AccessTokenCacheKey = "SpotifyAccessToken";

  private readonly IMemoryCache _cache;
  private readonly IConfiguration _configuration;
  private readonly HttpClient _httpClient;
  private readonly ILogger<SpotifyService> _logger;

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true
  };

  public SpotifyService(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    IConfiguration configuration,
    ILogger<SpotifyService> logger
  )
  {
    _httpClient = httpClient;
    _cache = memoryCache;
    _configuration = configuration;
    _logger = logger;
  }

  public async Task<SpotifyNowPlayingResponse?> GetNowPlayingAsync()
  {
    try
    {
      var accessToken = await GetAccessTokenAsync();

      if (string.IsNullOrEmpty(accessToken))
      {
        _logger.LogWarning("Access token is null or empty");
        return null;
      }

      // Clear any existing authorization headers
      _httpClient.DefaultRequestHeaders.Authorization = null;
      _httpClient.DefaultRequestHeaders.Remove("Authorization");
      
      // Set Bearer token
      _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken}");

      var response = await _httpClient.GetAsync(NowPlayingEndpoint);

      // No song currently being played
      if (response.StatusCode == HttpStatusCode.NoContent) 
      {
        _logger.LogInformation("No content returned from Spotify - no song currently playing");
        return null;
      }

      if (!response.IsSuccessStatusCode)
      {
        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogWarning("Spotify now playing returned status code: {StatusCode}, Content: {Content}",
          response.StatusCode, errorContent);
        return null;
      }

      var content = await response.Content.ReadAsStringAsync();
      _logger.LogDebug("Spotify API response content: {Content}", content);
      
      return JsonSerializer.Deserialize<SpotifyNowPlayingResponse>(content, JsonOptions);
    }
    catch (JsonException jsonEx)
    {
      _logger.LogError(jsonEx, "Error deserializing Spotify now playing response");
      return null;
    }
    catch (HttpRequestException httpEx)
    {
      _logger.LogError(httpEx, "HTTP error getting Spotify now playing response");
      return null;
    }
    catch (Exception e)
    {
      _logger.LogError(e, "Error getting Spotify now playing response");
      return null;
    }
  }

  private async Task<string?> GetAccessTokenAsync()
  {
    // if (_cache.TryGetValue(AccessTokenCacheKey, out string? cachedToken)) return cachedToken;
    try
    {
      var clientId = _configuration["SPOTIFY_CLIENT_ID"];
      var clientSecret = _configuration["SPOTIFY_CLIENT_SECRET"];
      var refreshToken = _configuration["SPOTIFY_REFRESH_TOKEN"];

      if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(refreshToken))
      {
        _logger.LogError("Missing Spotify configuration values - ClientId: {ClientIdPresent}, ClientSecret: {ClientSecretPresent}, RefreshToken: {RefreshTokenPresent}",
          !string.IsNullOrEmpty(clientId), !string.IsNullOrEmpty(clientSecret), !string.IsNullOrEmpty(refreshToken));
        return null;
      }

      var basicAuth = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

      // Clear any existing authorization headers
      _httpClient.DefaultRequestHeaders.Authorization = null;
      _httpClient.DefaultRequestHeaders.Remove("Authorization");
      
      // Set Basic auth for token request
      _httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {basicAuth}");

      var content = new FormUrlEncodedContent([
        new KeyValuePair<string, string>("grant_type", "refresh_token"),
        new KeyValuePair<string, string>("refresh_token", refreshToken)
      ]);

      var response = await _httpClient.PostAsync(TokenEndpoint, content);

      if (!response.IsSuccessStatusCode)
      {
        var errorContent = await response.Content.ReadAsStringAsync();
        _logger.LogError("Failed to retrieve access token. Status {StatusCode}, Content: {Content}", 
          response.StatusCode, errorContent);
        return null;
      }

      var contentStr = await response.Content.ReadAsStringAsync();
      _logger.LogDebug("Token response content: {Content}", contentStr);
      
      var tokenResponse = JsonSerializer.Deserialize<SpotifyTokenResponse>(contentStr, JsonOptions);

      if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
      {
        _logger.LogError("Access token is null or empty in token response");
        return null;
      }

      return tokenResponse.AccessToken;
    }
    catch (JsonException jsonEx)
    {
      _logger.LogError(jsonEx, "Error deserializing token response");
      return null;
    }
    catch (HttpRequestException httpEx)
    {
      _logger.LogError(httpEx, "HTTP error retrieving access token");
      return null;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to retrieve access token");
      return null;
    }
  }
}
