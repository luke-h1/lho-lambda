using System.Text.Json;
using Lho.Lambda.Models;
using Lho.Lambda.Utils;

namespace Lho.Lambda.Clients.LastFm;

public class LastFmApi
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true
  };

  private readonly HttpClient _httpClient;
  private readonly string _baseUrl;
  private readonly string? _apiKey;
  private readonly string? _username;


  public LastFmApi()
    : this(new HttpClient { Timeout = TimeSpan.FromSeconds(10) }, "https://ws.audioscrobbler.com/2.0/")
  {
  }

  public LastFmApi(HttpClient httpClient, string baseUrl, string? apiKey = null, string? username = null)
  {
    _httpClient = httpClient;
    _baseUrl = baseUrl;
    _apiKey = apiKey ?? EnvironmentConfig.LastFm.ApiKey;
    _username = username ?? EnvironmentConfig.LastFm.Username;
  }

  public async Task<LastFmRecentTracksResponse> GetRecentTracks()
  {
    if (string.IsNullOrEmpty(_apiKey))
    {
      throw new LastFmServiceException("Missing Last.fm API key");
    }

    if (string.IsNullOrEmpty(_username))
    {
      throw new LastFmServiceException("Missing Last.fm username");
    }

    var requestUri = $"{_baseUrl}?method=user.getrecenttracks&user={Uri.EscapeDataString(_username)}&api_key={Uri.EscapeDataString(_apiKey)}&format=json&limit=1";

    var response = await _httpClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, requestUri));
    await EnsureSuccess(response);

    return await ReadJson<LastFmRecentTracksResponse>(response) ??
           throw new LastFmServiceException("Empty response from last.fm API");
  }

  private static async Task EnsureSuccess(HttpResponseMessage response)
  {
    if (response.IsSuccessStatusCode)
    {
      return;
    }

    var body = await response.Content.ReadAsStringAsync();
    throw new LastFmServiceException($"HTTP error with status code: {(int)response.StatusCode} - {body}");
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

public class LastFmServiceException(string message) : Exception(message);
