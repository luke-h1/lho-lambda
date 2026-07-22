using System.Text.Json;
using Lho.Lambda.Models;
using Lho.Lambda.RuntimeConfiguration.Options;
using Lho.Lambda.Serialization;

namespace Lho.Lambda.Clients.LastFm;

public class LastFmApi
{
  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    TypeInfoResolver = LastFmJsonContext.Default
  };

  private readonly HttpClient _httpClient;
  private readonly LastFmOptions _lastFmOptions;

  public LastFmApi(HttpClient httpClient, LastFmOptions lastFmOptions)
  {
    _httpClient = httpClient;
    _lastFmOptions = lastFmOptions;
  }

  public async Task<LastFmRecentTracksResponse> GetRecentTracks(int limit = 1)
  {
    var credentials = _lastFmOptions.RequireCredentials();

    var boundedLimit = Math.Clamp(limit, 1, 50);
    var requestUri = $"?method=user.getrecenttracks&user={Uri.EscapeDataString(credentials.Username)}&api_key={Uri.EscapeDataString(credentials.ApiKey)}&format=json&limit={boundedLimit}";

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
