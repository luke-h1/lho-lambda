using LhoLambda.Models;
using LhoLambda.services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace LhoLambda.Controllers;

[ApiController]
public class NowPlayingController : ControllerBase
{
  private const string NowPlayingCacheKey = "NowPlaying";
  private const int RevalidateSeconds = 2;
  private readonly IMemoryCache _cache;
  private readonly IConfiguration _configuration;
  private readonly ILogger<NowPlayingController> _logger;
  private readonly ISpotifyService _spotifyService;

  public NowPlayingController(
    ISpotifyService spotifyService,
    IMemoryCache cache,
    IConfiguration configuration,
    ILogger<NowPlayingController> logger)
  {
    _spotifyService = spotifyService;
    _cache = cache;
    _configuration = configuration;
    _logger = logger;
  }

  [HttpGet("api/now-playing")]
  public async Task<ActionResult<NowPlayingResponse>> NowPlaying()
  {
    try
    {
      if (_cache.TryGetValue(NowPlayingCacheKey, out var cachedNowPlaying))
      {
        AddCorsAndCacheHeaders(true);
        return Ok(cachedNowPlaying);
      }

      var shouldCallSpotify = _configuration.GetValue<string>("SHOULD_CALL_SPOTIFY");

      if (string.Equals(shouldCallSpotify, "false", StringComparison.OrdinalIgnoreCase))
      {
        _logger.LogInformation("Now playing is disabled");
        var maintenanceResponse = new NowPlayingResponse
        {
          IsPlaying = false,
          Maintenance = true,
          Status = 200,
          Album = string.Empty,
          AlbumImageUrl = string.Empty,
          Artist = string.Empty,
          SongUrl = string.Empty,
          Title = string.Empty
        };

        AddCorsAndCacheHeaders(true);
        return Ok(maintenanceResponse);
      }

      var spotifyResponse = await _spotifyService.GetNowPlayingAsync();

			_logger.LogInformation($"Received response from spotify -> {spotifyResponse}");


      if (spotifyResponse?.Item == null)
      {
        _logger.LogInformation("No track currently playing");
        var notPlayingResponse = new NowPlayingResponse
        {
          IsPlaying = false

        };
        AddCorsAndCacheHeaders(true);
        return Ok(notPlayingResponse);
      }

      var response = new NowPlayingResponse
      {
        IsPlaying = spotifyResponse.IsPlaying,
        Title = spotifyResponse.Item.Name,
        Artist = string.Join(", ", spotifyResponse.Item.Artists.Select(a => a.Name)),
        Album = spotifyResponse.Item.Album.Name,
        AlbumImageUrl = spotifyResponse.Item.Album.Images.FirstOrDefault()?.Url ?? string.Empty,
        SongUrl = spotifyResponse.Item.ExternalUrls.Spotify,
        Status = 200
      };

      _cache.Set(NowPlayingCacheKey, response, TimeSpan.FromSeconds(5));

      AddCorsAndCacheHeaders(true);
      return Ok(response);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error fetching now playing data");
      var errorResponse = new NowPlayingResponse
      {
        IsPlaying = false,
        Status = 500,
      };

      AddCorsAndCacheHeaders(true);
      return Ok(errorResponse);
    }
  }

  private void AddCorsAndCacheHeaders(bool includeCacheControl)
  {
    Response.Headers.Append("Access-Control-Allow-Origin", "*");
    Response.Headers.Append("Access-Control-Allow-Methods", "GET,OPTIONS,POST,PUT,DELETE");

    if (includeCacheControl)
      Response.Headers.Append("Cache-Control",
        $"max-age={RevalidateSeconds}, s-maxage={RevalidateSeconds}, stale-while-revalidate={RevalidateSeconds}, stale-if-error={RevalidateSeconds}");
    else
      Response.Headers.Append("Cache-Control", "no-cache");
  }
}
