namespace Lho.Lambda.RuntimeConfiguration;

public static class Consumers
{
  public const string Unknown = "unknown";

  public static readonly IReadOnlySet<string> Valid =
    new HashSet<string>(StringComparer.Ordinal) { "lhowsam-dev", "lhowsam-prod", "lhowsam-local" };

  public static bool IsValid(string consumer)
  {
    return Valid.Contains(consumer);
  }

  public static string? Normalise(string? consumer)
  {
    return consumer switch
    {
      null or "" => null,
      _ when Valid.Contains(consumer) => consumer,
      _ => Unknown
    };
  }
}
