namespace Lho.Lambda.RuntimeConfiguration.Options;

public class AuthorizerOptions
{
  public const string Section = "Authorizer";

  public string? ApiKey { get; init; }
}
