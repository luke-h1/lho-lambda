using System.Diagnostics;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lho.Lambda.Clients.LastFm;
using Lho.Lambda.Clients.Spotify;
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
    var stopwatch = Stopwatch.StartNew();
    var method = request.RequestContext?.Http?.Method ?? "GET";
    var path = NormalisePath(request.RawPath);
    var consumer = NormaliseConsumer(GetHeaderValue(request.Headers, "x-consumer"));
    var provider = path == "/api/now-playing"
      ? QueryStringParser.Get(request.RawQueryString, "provider", "lastfm", ["lastfm", "spotify"])
      : null;
    APIGatewayHttpApiV2ProxyResponse response;
    Exception? capturedException = null;
    SentryTelemetry.Initialise(ctx.Logger);
    using var transaction = SentryTelemetry.StartTransaction(ctx.Logger, $"{method} {path}", "http.server", new Dictionary<string, string?>
    {
      ["function"] = ctx.FunctionName,
      ["request_id"] = ctx.AwsRequestId,
      ["route"] = path,
      ["method"] = method,
      ["consumer"] = consumer,
      ["provider"] = provider
    });

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
          "/api/health" => ResponseBuilder.CreateResponse(new { status = "OK" }, includeCacheControl: false),
          "/api/version" => ResponseBuilder.CreateResponse(new VersionService(_runtimeConfig.Deployment).GetVersion(), includeCacheControl: false),
          "/api/now-playing" => await HandleNowPlaying(ctx, provider!),
          "/api/top-tracks" => await HandleTopTracks(request, ctx),
          _ => ResponseBuilder.ErrorResponse(404, "Not Found")
        };
      }
    }
    catch (Exception exception)
    {
      capturedException = exception;
      response = ResponseBuilder.ErrorResponse(500, "Internal Server Error");
      StructuredLog.Error(ctx.Logger, "api.request.error", exception, new Dictionary<string, object?>
      {
        ["requestId"] = ctx.AwsRequestId,
        ["method"] = method,
        ["route"] = path,
        ["consumer"] = consumer,
        ["provider"] = provider
      });
      await SentryTelemetry.CaptureExceptionAsync(exception, ctx.Logger, new Dictionary<string, string?>
      {
        ["function"] = ctx.FunctionName,
        ["request_id"] = ctx.AwsRequestId,
        ["route"] = path,
        ["method"] = method,
        ["consumer"] = consumer,
        ["provider"] = provider
      });
    }

    stopwatch.Stop();
    transaction?.Finish(response.StatusCode, capturedException);
    await RecordInvocation(ctx, method, path, response.StatusCode, stopwatch.Elapsed.TotalMilliseconds, consumer, provider);
    return response;
  }


  private async Task<APIGatewayHttpApiV2ProxyResponse> HandleNowPlaying(
    ILambdaContext context,
    string provider)
  {
    var service = new NowPlayingService(_cache, _spotifyApi, _lastFmApi, context.Logger, _runtimeConfig.Features);
    var response = string.Equals(provider, "spotify", StringComparison.OrdinalIgnoreCase)
      ? await service.GetSpotifyNowPlaying()
      : await service.GetNowPlaying();

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

    var response = await new TopTracksService(_spotifyApi, context.Logger).HandleTopTracks(timeRange, limit);
    return ResponseBuilder.CreateResponse(response, revalidateSeconds: 300);
  }

  private static async Task RecordInvocation(
    ILambdaContext context,
    string method,
    string path,
    int statusCode,
    double durationMs,
    string? consumer,
    string? provider)
  {
    var outcome = statusCode >= 500 ? "error" : statusCode >= 400 ? "client_error" : "success";
    StructuredLog.Info(context.Logger, "api.request", new Dictionary<string, object?>
    {
      ["requestId"] = context.AwsRequestId,
      ["function"] = context.FunctionName,
      ["method"] = method,
      ["route"] = path,
      ["statusCode"] = statusCode,
      ["durationMs"] = Math.Round(durationMs, 2),
      ["outcome"] = outcome,
      ["consumer"] = consumer,
      ["provider"] = provider
    });

    var metric = new InvocationMetric(
      FunctionName: context.FunctionName,
      Operation: "api",
      Route: path,
      Method: method,
      StatusCode: statusCode,
      DurationMs: durationMs,
      Outcome: outcome,
      Consumer: consumer,
      Provider: provider);

    await PrometheusMetrics.PushInvocationAsync(metric, context.Logger);
    await SentryTelemetry.RecordInvocationAsync(metric, context.Logger);
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

  private static string? NormaliseConsumer(string? consumer)
  {
    return consumer switch
    {
      "lhowsam-prod" or "lhowsam-dev" or "lhowsam-local" => consumer,
      null or "" => null,
      _ => "unknown"
    };
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
    return new HttpClient
    {
      BaseAddress = new Uri(baseAddress),
      Timeout = TimeSpan.FromSeconds(10)
    };
  }
}
