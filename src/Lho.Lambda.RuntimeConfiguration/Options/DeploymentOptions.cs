namespace Lho.Lambda.RuntimeConfiguration.Options;

public class DeploymentOptions
{
  public const string Section = "Deployment";

  public string Version { get; init; } = "unknown";

  public string DeployedAt { get; init; } = "unknown";

  public string DeployedBy { get; init; } = "unknown";

  public string GitSha { get; init; } = "unknown";
}
