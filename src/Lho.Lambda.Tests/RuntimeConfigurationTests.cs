using Lho.Lambda.RuntimeConfiguration;
using Xunit;

namespace Lho.Lambda.Tests;

public class RuntimeConfigurationTests
{
    [Fact]
    public void FromEnvironmentBuildsTypedSections()
    {
        var config = RuntimeConfig.FromEnvironment(new Dictionary<string, string?>
        {
            ["LASTFM_API_KEY"] = "lastfm-key",
            ["LASTFM_USERNAME"] = "lastfm-user",
            ["VERSION"] = "1.2.3",
            ["DEPLOYED_AT"] = "2026-05-10T09:00:00Z",
            ["DEPLOYED_BY"] = "tests",
            ["GIT_SHA"] = "abc123",
            ["SENTRY_DSN"] = "https://sentry.example",
            ["API_KEY"] = "authorizer-key"
        });

        Assert.Equal("lastfm-key", config.LastFm.RequireCredentials().ApiKey);
        Assert.Equal("lastfm-user", config.LastFm.RequireCredentials().Username);
        Assert.Equal("1.2.3", config.Deployment.Version);
        Assert.Equal("2026-05-10T09:00:00Z", config.Deployment.DeployedAt);
        Assert.Equal("tests", config.Deployment.DeployedBy);
        Assert.Equal("abc123", config.Deployment.GitSha);
        Assert.Equal("https://sentry.example", config.Observability.SentryDsn);
        Assert.Equal("authorizer-key", config.Authorizer.ApiKey);
    }

    [Fact]
    public void MissingLastFmCredentialsFailOnlyWhenLastFmSectionIsRequired()
    {
        var config = RuntimeConfig.FromEnvironment(new Dictionary<string, string?>());

        var exception = Assert.Throws<RuntimeConfigurationException>(() => config.LastFm.RequireCredentials());

        Assert.Contains("LASTFM_API_KEY", exception.Message, StringComparison.Ordinal);
        Assert.Contains("LASTFM_USERNAME", exception.Message, StringComparison.Ordinal);
        Assert.Equal("unknown", config.Deployment.Version);
    }

    [Fact]
    public void MissingObservabilitySinksDisableThem()
    {
        var config = RuntimeConfig.FromEnvironment(new Dictionary<string, string?>());

        Assert.Null(config.Observability.SentryDsn);
    }

    [Theory]
    [InlineData("lhowsam-prod", "lhowsam-prod")]
    [InlineData("lhowsam-dev", "lhowsam-dev")]
    [InlineData("lhowsam-local", "lhowsam-local")]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("someone-else", "unknown")]
    public void ConsumerNormalisationMapsHeaderValues(string? consumer, string? expected)
    {
        Assert.Equal(expected, Consumers.Normalise(consumer));
    }

    [Fact]
    public void UnknownConsumerIsNotValid()
    {
        Assert.True(Consumers.IsValid("lhowsam-prod"));
        Assert.False(Consumers.IsValid(Consumers.Unknown));
    }

    [Fact]
    public void MissingAuthorizerApiKeyIsOptionalConfiguration()
    {
        var config = RuntimeConfig.FromEnvironment(new Dictionary<string, string?>());

        Assert.Null(config.Authorizer.ApiKey);
    }
}
