namespace Lho.Lambda.Observability;

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
