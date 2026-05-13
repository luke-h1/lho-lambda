using Lho.Lambda.Models;
using Lho.Lambda.Utils;

namespace Lho.Lambda.Services;

public static class VersionService
{
  public static VersionResponse GetVersion()
  {
    return new VersionResponse(
      Version: EnvironmentConfig.Deploy.Version,
      DeployedAt: EnvironmentConfig.Deploy.DeployedAt,
      DeployedBy: EnvironmentConfig.Deploy.DeployedBy,
      GitSha: EnvironmentConfig.Deploy.GitSha);
  }
}
