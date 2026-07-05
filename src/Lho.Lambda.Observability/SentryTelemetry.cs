using Amazon.Lambda.Core;
using Lho.Lambda.RuntimeConfiguration.Options;

namespace Lho.Lambda.Observability;

public static class SentryTelemetry
{
  // Happy-path flushes sit on the request critical path; keep the wait short.
  private static readonly TimeSpan FlushTimeout = TimeSpan.FromMilliseconds(500);
  private static readonly TimeSpan ErrorFlushTimeout = TimeSpan.FromSeconds(2);
  private static readonly Lock InitLock = new();
  private static bool _initialised;

  public static bool Initialise(ObservabilityOptions options, ILambdaLogger logger)
  {
    return EnsureInitialised(options, logger);
  }

  public static SentryTransactionScope? StartTransaction(
    ObservabilityOptions options,
    ILambdaLogger logger,
    string name,
    string operation,
    IReadOnlyDictionary<string, string?> tags)
  {
    if (!EnsureInitialised(options, logger))
    {
      return null;
    }

    var transaction = SentrySdk.StartTransaction(name, operation);
    ApplyTags(transaction, options, tags);
    SentrySdk.ConfigureScope(scope =>
    {
      scope.Transaction = transaction;
      ApplyTags(scope, options, tags);
    });

    return new SentryTransactionScope(transaction);
  }

  public static async Task CaptureExceptionAsync(
    Exception exception,
    ObservabilityOptions options,
    ILambdaLogger logger,
    IReadOnlyDictionary<string, string?> tags)
  {
    if (!EnsureInitialised(options, logger))
    {
      return;
    }

    SentrySdk.CaptureException(exception, scope =>
    {
      ApplyTags(scope, options, tags);
    });

    await SentrySdk.FlushAsync(ErrorFlushTimeout);
  }

  public static async Task RecordInvocationAsync(InvocationMetric metric, ObservabilityOptions options, ILambdaLogger logger)
  {
    if (!EnsureInitialised(options, logger))
    {
      return;
    }

    var attributes = MetricAttributes(metric, options);

    SentrySdk.Metrics.EmitCounter("lambda.invocation", 1, attributes, null);
    SentrySdk.Metrics.EmitDistribution("lambda.invocation.duration", metric.DurationMs, MeasurementUnit.Duration.Millisecond, attributes, null);
    await SentrySdk.FlushAsync(FlushTimeout);
  }

  public static async Task FlushAsync(ObservabilityOptions options, ILambdaLogger logger)
  {
    if (!EnsureInitialised(options, logger))
    {
      return;
    }

    await SentrySdk.FlushAsync(FlushTimeout);
  }

  private static bool EnsureInitialised(ObservabilityOptions options, ILambdaLogger logger)
  {
    lock (InitLock)
    {
      if (_initialised)
      {
        return true;
      }
    }

    var dsn = options.SentryDsn;
    if (string.IsNullOrWhiteSpace(dsn))
    {
      return false;
    }

    lock (InitLock)
    {
      if (_initialised)
      {
        return true;
      }

      try
      {
        SentrySdk.Init(sentryOptions =>
        {
          sentryOptions.Dsn = dsn;
          sentryOptions.Environment = options.SentryEnvironment;
          sentryOptions.Release = options.SentryRelease;
          sentryOptions.AttachStacktrace = true;
          sentryOptions.SampleRate = 1.0f;
          sentryOptions.EnableMetrics = true;
          sentryOptions.MaxBreadcrumbs = 50;
          sentryOptions.TracesSampleRate = options.SentryTracesSampleRate;
          sentryOptions.SendDefaultPii = false;
        });
        _initialised = true;
      }
      catch (Exception exception)
      {
        logger.LogLine($"Failed to initialise Sentry: {exception.Message}");
        return false;
      }
    }

    return true;
  }

  private static Dictionary<string, object> MetricAttributes(InvocationMetric metric, ObservabilityOptions options)
  {
    var attributes = new Dictionary<string, object>
    {
      ["service"] = options.ServiceName,
      ["environment"] = options.EnvironmentName,
      ["version"] = options.Version,
      ["git_sha"] = options.GitSha,
      [InvocationTags.Function] = metric.FunctionName,
      [InvocationTags.Operation] = metric.Operation,
      [InvocationTags.Route] = metric.Route,
      [InvocationTags.Method] = metric.Method,
      [InvocationTags.StatusCode] = metric.StatusCode.ToString(),
      [InvocationTags.Outcome] = metric.Outcome
    };

    if (!string.IsNullOrWhiteSpace(metric.Consumer))
    {
      attributes[InvocationTags.Consumer] = metric.Consumer;
    }

    if (!string.IsNullOrWhiteSpace(metric.Provider))
    {
      attributes[InvocationTags.Provider] = metric.Provider;
    }

    return attributes;
  }

  private static void ApplyTags(IHasTags target, ObservabilityOptions options, IReadOnlyDictionary<string, string?> tags)
  {
    target.SetTag("service", options.ServiceName);
    target.SetTag("environment", options.SentryEnvironment);
    target.SetTag("version", options.SentryRelease);
    target.SetTag("git_sha", options.GitSha);

    foreach (var (key, value) in tags)
    {
      if (!string.IsNullOrWhiteSpace(value))
      {
        target.SetTag(key, value);
      }
    }
  }

  private static SpanStatus StatusFromHttpStatus(int statusCode)
  {
    return statusCode switch
    {
      >= 500 => SpanStatus.InternalError,
      404 => SpanStatus.NotFound,
      401 => SpanStatus.Unauthenticated,
      403 => SpanStatus.PermissionDenied,
      >= 400 => SpanStatus.InvalidArgument,
      _ => SpanStatus.Ok
    };
  }

  public sealed class SentryTransactionScope(ITransactionTracer transaction) : IDisposable
  {
    private bool _finished;

    public void SetTag(string key, string value)
    {
      if (_finished)
      {
        return;
      }

      transaction.SetTag(key, value);
      SentrySdk.ConfigureScope(scope => scope.SetTag(key, value));
    }

    public void Finish(int statusCode, Exception? exception = null)
    {
      if (_finished)
      {
        return;
      }

      _finished = true;
      var status = StatusFromHttpStatus(statusCode);

      if (exception is null)
      {
        transaction.Finish(status);
        return;
      }

      transaction.Finish(exception, status);
    }

    public void Dispose()
    {
      Finish(200);
    }
  }
}
