using System.Web;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lho.Lambda.Clients.LastFm;
using Lho.Lambda.Models;
using Lho.Lambda.Observability;
using Lho.Lambda.RuntimeConfiguration;
using Lho.Lambda.Services;
using Lho.Lambda.Utils;

namespace Lho.Lambda.Functions;

public class ApiFunction(RuntimeConfig runtimeConfig, MemoryCache cache, LastFmApi lastFmApi)
{
    private readonly RuntimeConfig _runtimeConfig = runtimeConfig;
    private readonly MemoryCache _cache = cache;
    private readonly LastFmApi _lastFmApi = lastFmApi;

    public ApiFunction()
      : this(RuntimeConfig.Current)
    {

    }

    public ApiFunction(RuntimeConfig runtimeConfig)
      : this(
        runtimeConfig,
        new MemoryCache(),
        new LastFmApi(CreateHttpClient("https://ws.audioscrobbler.com/2.0/"), runtimeConfig.LastFm))
    {

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
            response = !IsSupportedMethod(method, path)
                ? ResponseBuilder.ErrorResponse(404, "Not Found")
                : path switch
                {
                    "/api/health" => ResponseBuilder.CreateResponse(new HealthResponse("OK"), includeCacheControl: false),
                    "/api/version" => ResponseBuilder.CreateResponse(BuildVersionResponse(), includeCacheControl: false),
                    "/api/now-playing" => await HandleNowPlaying(ctx, invocation),
                    "/api/recent-tracks" => await HandleRecentTracks(request),
                    "/api/top-tracks" => await HandleTopTracks(request, invocation),
                    _ => ResponseBuilder.ErrorResponse(404, "Not Found")
                };
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
      ILambdaContext context,
      LambdaInvocation invocation)
    {
        var service = new NowPlayingService(_cache, _lastFmApi, context.Logger, _runtimeConfig.Observability);
        var result = await service.HandleNowPlaying();
        invocation.SetProvider(result.Provider);

        return ResponseBuilder.CreateResponse(result.Response, revalidateSeconds: 3);
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandleRecentTracks(
      APIGatewayHttpApiV2ProxyRequest request)
    {
        var limit = int.TryParse(GetQueryParam(request.RawQueryString, "limit"), out var parsedLimit)
          ? Math.Clamp(parsedLimit, 1, 50)
          : 10;

        var recentTracks = await _lastFmApi.GetRecentTracks(limit);
        var tracks = recentTracks.RecentTracks.Tracks
          .Select(static track => new RecentTrackResponseItem(
            Title: track.Name,
            Artist: track.Artist.Text,
            Album: track.Album.Text,
            AlbumImageUrl: LargestImageUrl(track.Images),
            SongUrl: track.Url,
            NowPlaying: string.Equals(track.Attributes?.NowPlaying, "true", StringComparison.OrdinalIgnoreCase),
            PlayedAt: long.TryParse(track.Date?.Uts, out var uts) ? uts : null))
          .ToArray();

        return ResponseBuilder.CreateResponse(new RecentTracksApiResponse(tracks), revalidateSeconds: 30);
    }

    private async Task<APIGatewayHttpApiV2ProxyResponse> HandleTopTracks(
      APIGatewayHttpApiV2ProxyRequest request,
      LambdaInvocation invocation)
    {
        var timeRange = GetQueryParam(
          request.RawQueryString,
          "time_range",
          "medium_term",
          ["short_term", "medium_term", "long_term"]);
        invocation.SetTag(InvocationTags.TimeRange, timeRange);
        var limit = int.TryParse(GetQueryParam(request.RawQueryString, "limit"), out var parsedLimit) ? parsedLimit : 20;

        var topTracks = await _lastFmApi.GetTopTracks(MapTimeRangeToPeriod(timeRange), limit);

        var tracks = topTracks.TopTracks.Tracks.Select(static track => new TopTrackResponseItem(
          Title: track.Name,
          Artist: track.Artist.Name,
          Album: "",
          AlbumImageUrl: LargestImageUrl(track.Images),
          SongUrl: track.Url))
          .ToArray();

        return ResponseBuilder.CreateResponse(new TopTracksApiResponse(tracks), revalidateSeconds: 300);
    }

    private static string LargestImageUrl(IReadOnlyList<LastFmImage> images)
    {
        return images.LastOrDefault(static image => !string.IsNullOrEmpty(image.Url))?.Url ?? "";
    }

    private static string MapTimeRangeToPeriod(string timeRange)
    {
        return timeRange switch
        {
            "short_term" => "1month",
            "long_term" => "overall",
            _ => "6month"
        };
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
            _ when path.StartsWith('/') => path,
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
