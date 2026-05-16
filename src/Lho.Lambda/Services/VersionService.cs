using Lho.Lambda.Models;
using Lho.Lambda.RuntimeConfiguration.Options;

namespace Lho.Lambda.Services;

public class VersionService(DeploymentOptions deployment)
{
  public VersionResponse GetVersion()
  {
    return new VersionResponse(
      Version: deployment.Version,
      DeployedAt: deployment.DeployedAt,
      DeployedBy: deployment.DeployedBy,
      GitSha: deployment.GitSha);
  }
}
