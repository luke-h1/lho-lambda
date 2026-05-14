using System.Collections.Concurrent;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Amazon.Lambda.Core;

namespace Lho.Lambda.Observability;

public static class PrometheusMetrics
{
  private static readonly HttpClient HttpClient = new()
  {
    Timeout = TimeSpan.FromSeconds(2)
  };
  private static readonly ConcurrentDictionary<string, long> Counters = new();

  public static async Task PushInvocationAsync(InvocationMetric metric, ILambdaLogger logger)
  {
    if (!ObservabilityConfig.MetricsEnabled)
    {
      return;
    }

    try
    {
      var counterKey = $"{metric.FunctionName}|{metric.Operation}|{metric.Route}|{metric.Method}|{metric.StatusCode}|{metric.Outcome}|{metric.Consumer}|{metric.Provider}";
      var count = Counters.AddOrUpdate(counterKey, 1, (_, current) => current + 1);
      var body = BuildBody(metric, count);
      var endpoint = BuildPushgatewayEndpoint(metric.FunctionName);

      using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
      request.Content = new StringContent(body, Encoding.UTF8);
      request.Content.Headers.ContentType = new MediaTypeHeaderValue("text/plain")
      {
        CharSet = "utf-8"
      };
      AddConfiguredAuthHeader(request);

      using var response = await HttpClient.SendAsync(request);
      if (!response.IsSuccessStatusCode)
      {
        logger.LogLine($"Pushgateway rejected metrics with status {(int)response.StatusCode}");
      }
    }
    catch (Exception exception)
    {
      logger.LogLine($"Failed to push Prometheus metrics: {exception.Message}");
    }
  }

  private static string BuildBody(InvocationMetric metric, long count)
  {
    var labels = Labels(metric);
    var durationSeconds = metric.DurationMs / 1000d;
    var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    return string.Join('\n', [
      "# TYPE lho_lambda_invocations_total counter",
      $"lho_lambda_invocations_total{{{labels}}} {count.ToString(CultureInfo.InvariantCulture)}",
      "# TYPE lho_lambda_invocation_duration_seconds gauge",
      $"lho_lambda_invocation_duration_seconds{{{labels}}} {durationSeconds.ToString("0.###", CultureInfo.InvariantCulture)}",
      "# TYPE lho_lambda_last_invocation_timestamp_seconds gauge",
      $"lho_lambda_last_invocation_timestamp_seconds{{{labels}}} {timestamp.ToString(CultureInfo.InvariantCulture)}",
      ""
    ]);
  }

  private static string Labels(InvocationMetric metric)
  {
    var labels = new Dictionary<string, string>
    {
      ["service"] = ObservabilityConfig.ServiceName,
      ["environment"] = ObservabilityConfig.EnvironmentName,
      ["version"] = ObservabilityConfig.Version,
      ["git_sha"] = ObservabilityConfig.GitSha,
      ["function"] = metric.FunctionName,
      ["operation"] = metric.Operation,
      ["route"] = metric.Route,
      ["method"] = metric.Method,
      ["status"] = metric.StatusCode.ToString(CultureInfo.InvariantCulture),
      ["outcome"] = metric.Outcome
    };

    if (!string.IsNullOrWhiteSpace(metric.Consumer))
    {
      labels["consumer"] = metric.Consumer;
    }

    if (!string.IsNullOrWhiteSpace(metric.Provider))
    {
      labels["provider"] = metric.Provider;
    }

    return string.Join(",", labels.Select(label => $"{label.Key}=\"{EscapeLabelValue(label.Value)}\""));
  }

  private static Uri BuildPushgatewayEndpoint(string functionName)
  {
    var baseUri = ObservabilityConfig.PushgatewayUrl!.TrimEnd('/');
    var job = Uri.EscapeDataString(ObservabilityConfig.PushgatewayJob);
    var environment = Uri.EscapeDataString(ObservabilityConfig.EnvironmentName);
    var function = Uri.EscapeDataString(functionName);

    return new Uri($"{baseUri}/metrics/job/{job}/environment/{environment}/function/{function}");
  }

  private static void AddConfiguredAuthHeader(HttpRequestMessage request)
  {
    var authHeader = ObservabilityConfig.PushgatewayAuthHeader;
    if (string.IsNullOrWhiteSpace(authHeader))
    {
      return;
    }

    var separator = authHeader.IndexOf('=', StringComparison.Ordinal);
    if (separator <= 0 || separator == authHeader.Length - 1)
    {
      return;
    }

    var name = authHeader[..separator].Trim();
    var value = authHeader[(separator + 1)..].Trim();
    if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value))
    {
      return;
    }

    request.Headers.TryAddWithoutValidation(name, value);
  }

  private static string EscapeLabelValue(string value)
  {
    return value
      .Replace("\\", "\\\\", StringComparison.Ordinal)
      .Replace("\n", "\\n", StringComparison.Ordinal)
      .Replace("\"", "\\\"", StringComparison.Ordinal);
  }
}

public sealed record InvocationMetric(
  string FunctionName,
  string Operation,
  string Route,
  string Method,
  int StatusCode,
  double DurationMs,
  string Outcome,
  string? Consumer = null,
  string? Provider = null);
