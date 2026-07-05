using System.Web;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lho.Lambda.Clients.LastFm;
using Lho.Lambda.Clients.Spotify;
using Lho.Lambda.Models;
using Lho.Lambda.Observability;
using Lho.Lambda.RuntimeConfiguration;
using Lho.Lambda.Services;
using Lho.Lambda.Utils;

namespace Lho.Lambda.Functions;


public class ApiFunction
{
  private readonly RuntimeConfig _runtimeConfig;
  private readonly MemoryCache _cache;
  private readonly SpotifyApi _spotifyApi;
  private readonly LastFmApi _lastFmApi;

  public ApiFunction()
    : this(RuntimeConfig.Current)
  {

  }

  public ApiFunction(RuntimeConfig runtimeConfig)
    : this(
      runtimeConfig,
      new MemoryCache(),
      new SpotifyApi(CreateHttpClient("https://api.spotify.com/v1/"), runtimeConfig.Spotify),
      new LastFmApi(CreateHttpClient("https://ws.audioscrobbler.com/2.0/"), runtimeConfig.LastFm))
  {

  }

  public ApiFunction(RuntimeConfig runtimeConfig, MemoryCache cache, SpotifyApi spotifyApi, LastFmApi lastFmApi)
  {
    _runtimeConfig = runtimeConfig;
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
    var consumer = Consumers.Normalise(GetHeaderValue(request.Headers, "x-consumer"));

    await using var invocation = LambdaInvocation.Begin(
      ctx,
      new InvocationDescriptor("api", path, method, consumer),
      _runtimeConfig.Observability);

    APIGatewayHttpApiV2ProxyResponse response;

    try
    {
      if (!IsSupportedMethod(method, path))
      {
        response = ResponseBuilder.ErrorResponse(404, "Not Found");
      }
      else
      {
        response = path switch
        {
          "/api/health" => ResponseBuilder.CreateResponse(new HealthResponse("OK"), includeCacheControl: false),
          "/api/version" => ResponseBuilder.CreateResponse(BuildVersionResponse(), includeCacheControl: false),
          "/api/now-playing" => await HandleNowPlaying(request, ctx, invocation),
          "/api/top-tracks" => await HandleTopTracks(request, ctx, invocation),
          _ => ResponseBuilder.ErrorResponse(404, "Not Found")
        };
      }
    }
    catch (Exception exception)
    {
      response = ResponseBuilder.ErrorResponse(500, "Internal Server Error");
      await invocation.RecordFailureAsync(exception);
    }

    await invocation.CompleteAsync(response.StatusCode);
    return response;
  }


  private async Task<APIGatewayHttpApiV2ProxyResponse> HandleNowPlaying(
    APIGatewayHttpApiV2ProxyRequest request,
    ILambdaContext context,
    LambdaInvocation invocation)
  {
    var service = new NowPlayingService(_cache, _spotifyApi, _lastFmApi, context.Logger, _runtimeConfig.Features, _runtimeConfig.Observability);
    var result = await service.HandleNowPlaying(GetQueryParam(request.RawQueryString, "provider"));
    invocation.SetProvider(result.Provider);

    return ResponseBuilder.CreateResponse(result.Response, revalidateSeconds: 3);
  }

  private async Task<APIGatewayHttpApiV2ProxyResponse> HandleTopTracks(
    APIGatewayHttpApiV2ProxyRequest request,
    ILambdaContext context,
    LambdaInvocation invocation)
  {
    var timeRange = GetQueryParam(
      request.RawQueryString,
      "time_range",
      "medium_term",
      ["short_term", "medium_term", "long_term"]);
    invocation.SetTag(InvocationTags.TimeRange, timeRange);
    var limit = int.TryParse(GetQueryParam(request.RawQueryString, "limit"), out var parsedLimit) ? parsedLimit : 20;

    var topTracks = await _spotifyApi.GetTopTracks(timeRange, limit);
    var tracks = topTracks.Items
      .Select(item => new TopTrackResponseItem(
        Title: item.Name,
        Artist: string.Join(", ", item.Artists.Select(artist => artist.Name)),
        Album: item.Album.Name,
        AlbumImageUrl: item.Album.Images.FirstOrDefault()?.Url ?? "",
        SongUrl: item.ExternalUrls.Spotify))
      .ToArray();

    return ResponseBuilder.CreateResponse(new TopTracksApiResponse(tracks), revalidateSeconds: 300);
  }

  private VersionResponse BuildVersionResponse()
  {
    return new VersionResponse(
      Version: _runtimeConfig.Deployment.Version,
      DeployedAt: _runtimeConfig.Deployment.DeployedAt,
      DeployedBy: _runtimeConfig.Deployment.DeployedBy,
      GitSha: _runtimeConfig.Deployment.GitSha);
  }

  private static string? GetQueryParam(string? rawQueryString, string key)
  {
    if (string.IsNullOrEmpty(rawQueryString))
    {
      return null;
    }

    var value = HttpUtility.ParseQueryString(rawQueryString)[key];
    return string.IsNullOrEmpty(value) ? null : value;
  }

  private static string GetQueryParam(
    string? rawQueryString,
    string key,
    string defaultValue,
    IReadOnlyCollection<string> allowedValues)
  {
    var value = GetQueryParam(rawQueryString, key);
    return value is not null && allowedValues.Contains(value) ? value : defaultValue;
  }

  private static readonly string[] KnownStages = ["/test", "/staging", "/live", "/prod"];

  private static string NormalisePath(string? rawPath)
  {
    var path = string.IsNullOrEmpty(rawPath) ? "/" : rawPath;

    foreach (var stage in KnownStages)
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
      _ when path.StartsWith("/", StringComparison.Ordinal) => path,
      _ => "/" + path
    };
  }

  private static string? GetHeaderValue(IDictionary<string, string>? headers, string key)
  {
    if (headers is null)
    {
      return null;
    }

    foreach (var (headerKey, value) in headers)
    {
      if (string.Equals(headerKey, key, StringComparison.OrdinalIgnoreCase))
      {
        return value;
      }
    }

    return null;
  }

  private static bool IsSupportedMethod(string method, string path)
  {
    if (string.Equals(method, "GET", StringComparison.OrdinalIgnoreCase))
    {
      return true;
    }

    return string.Equals(method, "HEAD", StringComparison.OrdinalIgnoreCase) && path == "/api/health";
  }

  private static HttpClient CreateHttpClient(string baseAddress)
  {
    // Recycle pooled connections so a thawed container does not reuse sockets
    // that went stale while the Lambda was frozen.
    var handler = new SocketsHttpHandler
    {
      PooledConnectionLifetime = TimeSpan.FromMinutes(2),
      ConnectTimeout = TimeSpan.FromSeconds(2)
    };

    return new HttpClient(handler)
    {
      BaseAddress = new Uri(baseAddress),
      Timeout = TimeSpan.FromSeconds(10)
    };
  }
}
