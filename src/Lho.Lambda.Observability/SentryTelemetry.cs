using Amazon.Lambda.Core;

namespace Lho.Lambda.Observability;

public static class SentryTelemetry
{
  private static readonly Lock InitLock = new();
  private static bool _initialised;

  public static bool Initialise(ILambdaLogger logger)
  {
    return EnsureInitialised(logger);
  }

  public static SentryTransactionScope? StartTransaction(
    ILambdaLogger logger,
    string name,
    string operation,
    IReadOnlyDictionary<string, string?> tags)
  {
    if (!EnsureInitialised(logger))
    {
      return null;
    }

    var transaction = SentrySdk.StartTransaction(name, operation);
    ApplyTags(transaction, tags);
    SentrySdk.ConfigureScope(scope =>
    {
      scope.Transaction = transaction;
      ApplyTags(scope, tags);
    });

    return new SentryTransactionScope(transaction);
  }

  public static async Task CaptureExceptionAsync(
    Exception exception,
    ILambdaLogger logger,
    IReadOnlyDictionary<string, string?> tags)
  {
    if (!EnsureInitialised(logger))
    {
      return;
    }

    SentrySdk.CaptureException(exception, scope =>
    {
      ApplyTags(scope, tags);
    });

    await SentrySdk.FlushAsync(TimeSpan.FromSeconds(2));
  }

  public static async Task RecordInvocationAsync(InvocationMetric metric, ILambdaLogger logger)
  {
    if (!EnsureInitialised(logger))
    {
      return;
    }

    var attributes = MetricAttributes(metric);

    SentrySdk.Metrics.EmitCounter("lambda.invocation", 1, attributes, null);
    SentrySdk.Metrics.EmitDistribution("lambda.invocation.duration", metric.DurationMs, MeasurementUnit.Duration.Millisecond, attributes, null);
    await SentrySdk.FlushAsync(TimeSpan.FromSeconds(2));
  }

  public static async Task FlushAsync(ILambdaLogger logger)
  {
    if (!EnsureInitialised(logger))
    {
      return;
    }

    await SentrySdk.FlushAsync(TimeSpan.FromSeconds(2));
  }

  private static bool EnsureInitialised(ILambdaLogger logger)
  {
    lock (InitLock)
    {
      if (_initialised)
      {
        return true;
      }
    }

    var dsn = ObservabilityConfig.SentryDsn;
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
        SentrySdk.Init(options =>
        {
          options.Dsn = dsn;
          options.Environment = ObservabilityConfig.SentryEnvironment;
          options.Release = ObservabilityConfig.SentryRelease;
          options.AttachStacktrace = true;
          options.SampleRate = 1.0f;
          options.EnableMetrics = true;
          options.MaxBreadcrumbs = 50;
          options.TracesSampleRate = ObservabilityConfig.SentryTracesSampleRate;
          options.SendDefaultPii = false;
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

  private static Dictionary<string, object> MetricAttributes(InvocationMetric metric)
  {
    var attributes = new Dictionary<string, object>
    {
      ["service"] = ObservabilityConfig.ServiceName,
      ["environment"] = ObservabilityConfig.EnvironmentName,
      ["version"] = ObservabilityConfig.Version,
      ["git_sha"] = ObservabilityConfig.GitSha,
      ["function"] = metric.FunctionName,
      ["operation"] = metric.Operation,
      ["route"] = metric.Route,
      ["method"] = metric.Method,
      ["status"] = metric.StatusCode.ToString(),
      ["outcome"] = metric.Outcome
    };

    if (!string.IsNullOrWhiteSpace(metric.Consumer))
    {
      attributes["consumer"] = metric.Consumer;
    }

    if (!string.IsNullOrWhiteSpace(metric.Provider))
    {
      attributes["provider"] = metric.Provider;
    }

    return attributes;
  }

  private static void ApplyTags(IHasTags target, IReadOnlyDictionary<string, string?> tags)
  {
    target.SetTag("service", ObservabilityConfig.ServiceName);
    target.SetTag("environment", ObservabilityConfig.SentryEnvironment);
    target.SetTag("version", ObservabilityConfig.SentryRelease);
    target.SetTag("git_sha", ObservabilityConfig.GitSha);

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
