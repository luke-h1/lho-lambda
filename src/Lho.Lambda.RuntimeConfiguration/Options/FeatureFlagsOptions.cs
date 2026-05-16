namespace Lho.Lambda.RuntimeConfiguration.Options;

public class FeatureFlagsOptions
{
  public const string Section = "FeatureFlags";

  public bool ShouldCallSpotify { get; init; } = true;
}
