namespace LhoLambda.Models;

public class VersionResponse
{
  public string Version { get; set; } = string.Empty;

  public string? DeployedBy { get; set; }

  public string? DeployedAt { get; set; }

  public string GitSha { get; set; } = string.Empty;
}
