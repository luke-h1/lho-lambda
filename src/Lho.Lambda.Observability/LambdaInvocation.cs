using System.Diagnostics;
using Amazon.Lambda.Core;
using Lho.Lambda.RuntimeConfiguration.Options;

namespace Lho.Lambda.Observability;

public sealed record InvocationDescriptor(
  string Operation,
  string Route,
  string Method,
  string? Consumer = null,
  string LogEventPrefix = "api.request");

public sealed class LambdaInvocation : IAsyncDisposable
{
  private readonly ILambdaContext _context;
  private readonly InvocationDescriptor _descriptor;
  private readonly ObservabilityOptions _options;
  private readonly Stopwatch _stopwatch;
  private readonly Dictionary<string, string?> _tags;
  private readonly SentryTelemetry.SentryTransactionScope? _transaction;
  private Exception? _failure;
  private bool _completed;

  private LambdaInvocation(
    ILambdaContext context,
    InvocationDescriptor descriptor,
    ObservabilityOptions options,
    Dictionary<string, string?> tags,
    SentryTelemetry.SentryTransactionScope? transaction)
  {
    _context = context;
    _descriptor = descriptor;
    _options = options;
    _tags = tags;
    _transaction = transaction;
    _stopwatch = Stopwatch.StartNew();
  }

  public static LambdaInvocation Begin(ILambdaContext context, InvocationDescriptor descriptor, ObservabilityOptions options)
  {
    var tags = new Dictionary<string, string?>
    {
      [InvocationTags.Function] = context.FunctionName,
      [InvocationTags.RequestId] = context.AwsRequestId,
      [InvocationTags.Operation] = descriptor.Operation,
      [InvocationTags.Route] = descriptor.Route,
      [InvocationTags.Method] = descriptor.Method,
      [InvocationTags.Consumer] = descriptor.Consumer
    };

    var transaction = SentryTelemetry.StartTransaction(
      options,
      context.Logger,
      $"{descriptor.Method} {descriptor.Route}",
      "http.server",
      tags);

    return new LambdaInvocation(context, descriptor, options, tags, transaction);
  }

  public void SetTag(string key, string? value)
  {
    _tags[key] = value;
    if (!string.IsNullOrWhiteSpace(value))
    {
      _transaction?.SetTag(key, value);
    }
  }

  public void SetProvider(string provider)
  {
    SetTag(InvocationTags.Provider, provider);
  }

  public void SetReason(string reason)
  {
    SetTag(InvocationTags.Reason, reason);
  }

  public async Task RecordFailureAsync(Exception exception)
  {
    _failure = exception;
    StructuredLog.Error(_context.Logger, $"{_descriptor.LogEventPrefix}.error", exception, _options, LogFields());
    await SentryTelemetry.CaptureExceptionAsync(exception, _options, _context.Logger, _tags);
  }

  public async Task CompleteAsync(int statusCode, string? outcome = null)
  {
    if (_completed)
    {
      return;
    }

    _completed = true;
    _stopwatch.Stop();
    outcome ??= statusCode >= 500 ? "error" : statusCode >= 400 ? "client_error" : "success";

    _transaction?.Finish(statusCode, _failure);

    var fields = LogFields();
    fields[InvocationTags.StatusCode] = statusCode;
    fields[InvocationTags.DurationMs] = Math.Round(_stopwatch.Elapsed.TotalMilliseconds, 2);
    fields[InvocationTags.Outcome] = outcome;
    StructuredLog.Info(_context.Logger, _descriptor.LogEventPrefix, _options, fields);

    var metric = new InvocationMetric(
      FunctionName: _context.FunctionName,
      Operation: _descriptor.Operation,
      Route: _descriptor.Route,
      Method: _descriptor.Method,
      StatusCode: statusCode,
      DurationMs: _stopwatch.Elapsed.TotalMilliseconds,
      Outcome: outcome,
      Consumer: _descriptor.Consumer,
      Provider: _tags.GetValueOrDefault(InvocationTags.Provider));

    await SentryTelemetry.RecordInvocationAsync(metric, _options, _context.Logger);
  }

  public async ValueTask DisposeAsync()
  {
    if (!_completed)
    {
      await CompleteAsync(_failure is null ? 200 : 500);
    }
  }

  private Dictionary<string, object?> LogFields()
  {
    var fields = new Dictionary<string, object?>();
    foreach (var (key, value) in _tags)
    {
      if (value is not null)
      {
        fields[key] = value;
      }
    }

    return fields;
  }
}
