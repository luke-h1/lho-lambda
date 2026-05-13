using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lho.Lambda.Clients.LastFm;
using Lho.Lambda.Clients.Spotify;
using Lho.Lambda.Services;
using Lho.Lambda.Utils;

namespace Lho.Lambda.Functions;


public class ApiFunction
{
  private static readonly MemoryCache Cache = new();
  private static readonly SpotifyApi SpotifyApi = new();
  private static readonly LastFmApi LastFmApi = new();

  private readonly MemoryCache _cache;
  private readonly SpotifyApi _spotifyApi;
  private readonly LastFmApi _lastFmApi;

  public ApiFunction()
    : this(Cache, SpotifyApi, LastFmApi)
  {

  }

  public ApiFunction(MemoryCache cache, SpotifyApi spotifyApi, LastFmApi lastFmApi)
  {
    _cache = cache;
    _spotifyApi = spotifyApi;
    _lastFmApi = lastFmApi;
  }

  public async Task<APIGatewayHttpApiV2ProxyResponse> FunctionHandler(
    APIGatewayHttpApiV2ProxyRequest request,
    ILambdaContext ctx
  )
  {
    var method = request.RequestContext?.Http?.Method ?? "GET";
    var path = NormalisePath(request.RawPath);
    ctx.Logger.LogLine($"Request {method} {path}");

    if (!string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
    {
      return ResponseBuilder.ErrorResponse(404, "Not Found");
    }

    return path switch
    {
      "/api/health" => ResponseBuilder.CreateResponse(new { status = "OK" }, includeCacheControl: false),
      "/api/version" => ResponseBuilder.CreateResponse(VersionService.GetVersion(), includeCacheControl: false),
      "/api/now-playing" => await HandleNowPlaying(request, ctx),
      "/api/top-tracks" => await HandleTopTracks(request, ctx),
      _ => ResponseBuilder.ErrorResponse(404, "Not Found")
    };
  }


  private async Task<APIGatewayHttpApiV2ProxyResponse> HandleNowPlaying(
    APIGatewayHttpApiV2ProxyRequest request,
    ILambdaContext context)
  {
    var provider = QueryStringParser.Get(
      request.RawQueryString,
      "provider",
      "lastfm",
      ["lastfm", "spotify"]);
    var response = await new NowPlayingService(_cache, _spotifyApi, _lastFmApi, context.Logger).HandleNowPlaying(provider);

    return ResponseBuilder.CreateResponse(response, revalidateSeconds: 3);
  }

  private async Task<APIGatewayHttpApiV2ProxyResponse> HandleTopTracks(
    APIGatewayHttpApiV2ProxyRequest request,
    ILambdaContext context)
  {
    var timeRange = QueryStringParser.Get(
      request.RawQueryString,
      "time_range",
      "medium_term",
      ["short_term", "medium_term", "long_term"]);
    var rawLimit = QueryStringParser.Get(request.RawQueryString, "limit", "20");
    var limit = int.TryParse(rawLimit, out var parsedLimit) ? Math.Clamp(parsedLimit, 1, 50) : 20;

    try
    {
      var response = await new TopTracksService(_spotifyApi, context.Logger).HandleTopTracks(timeRange, limit);
      return ResponseBuilder.CreateResponse(response, revalidateSeconds: 300);
    }
    catch (Exception exception)
    {
      context.Logger.LogLine($"Top tracks failed: {exception}");
      return ResponseBuilder.ErrorResponse(500, "Unable to fetch top tracks");
    }
  }

  private static string NormalisePath(string? rawPath)
  {
    var path = string.IsNullOrEmpty(rawPath) ? "/" : rawPath;
    var knownStages = new[] { "/test", "/staging", "/live", "/prod" };

    foreach (var stage in knownStages)
    {
      if (path.StartsWith(stage + "/", StringComparison.Ordinal))
      {
        path = path[stage.Length..];
        break;
      }

      if (string.Equals(path, stage, StringComparison.Ordinal))
      {
        path = "/";
        break;
      }
    }

    if (!path.StartsWith("/invoke", StringComparison.Ordinal))
    {
      return path;
    }

    path = path[7..];
    return path switch
    {
      "" => "/",
      var value when value.StartsWith("/", StringComparison.Ordinal) => value,
      _ => "/" + path
    };
  }
}
