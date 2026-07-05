using Lho.Lambda.Authorizer.Functions;
using Lho.Lambda.Authorizer.Models;
using Lho.Lambda.RuntimeConfiguration.Options;
using Xunit;

namespace Lho.Lambda.Tests;

public class AuthorizerFunctionTests
{
  [Fact]
  public async Task AuthorizesKnownConsumerWithMatchingApiKey()
  {
    var function = CreateFunction(apiKey: "secret");

    var response = await function.FunctionHandler(
      CreateRequest(new Dictionary<string, string>
      {
        ["X-API-Key"] = "secret",
        ["X-Consumer"] = "lhowsam-prod"
      }),
      new TestLambdaContext());

    Assert.True(response.IsAuthorized);
  }

  [Fact]
  public async Task DeniesUnknownConsumer()
  {
    var function = CreateFunction(apiKey: "secret");

    var response = await function.FunctionHandler(
      CreateRequest(new Dictionary<string, string>
      {
        ["x-api-key"] = "secret",
        ["x-consumer"] = "someone-else"
      }),
      new TestLambdaContext());

    Assert.False(response.IsAuthorized);
  }

  [Fact]
  public async Task DeniesMismatchedApiKey()
  {
    var function = CreateFunction(apiKey: "secret");

    var response = await function.FunctionHandler(
      CreateRequest(new Dictionary<string, string>
      {
        ["x-api-key"] = "wrong"
      }),
      new TestLambdaContext());

    Assert.False(response.IsAuthorized);
  }

  [Fact]
  public async Task DeniesMissingApiKeyConfiguration()
  {
    var function = CreateFunction(apiKey: null);

    var response = await function.FunctionHandler(
      CreateRequest([]),
      new TestLambdaContext());

    Assert.False(response.IsAuthorized);
  }

  private static AuthorizerFunction CreateFunction(string? apiKey)
  {
    return new AuthorizerFunction(
      new AuthorizerOptions { ApiKey = apiKey },
      new ObservabilityOptions());
  }

  private static AuthorizerRequest CreateRequest(Dictionary<string, string> headers)
  {
    return new AuthorizerRequest
    {
      Headers = headers
    };
  }
}
