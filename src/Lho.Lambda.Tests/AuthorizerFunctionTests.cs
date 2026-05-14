using Lho.Lambda.Authorizer.Functions;
using Lho.Lambda.Authorizer.Models;
using Xunit;

namespace Lho.Lambda.Tests;

public class AuthorizerFunctionTests
{
  [Fact]
  public async Task AuthorizesKnownConsumerWithMatchingApiKey()
  {
    Environment.SetEnvironmentVariable("API_KEY", "secret");
    var function = new AuthorizerFunction();

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
    Environment.SetEnvironmentVariable("API_KEY", "secret");
    var function = new AuthorizerFunction();

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
    Environment.SetEnvironmentVariable("API_KEY", "secret");
    var function = new AuthorizerFunction();

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
    Environment.SetEnvironmentVariable("API_KEY", null);
    var function = new AuthorizerFunction();

    var response = await function.FunctionHandler(
      CreateRequest([]),
      new TestLambdaContext());

    Assert.False(response.IsAuthorized);
  }

  private static AuthorizerRequest CreateRequest(Dictionary<string, string> headers)
  {
    return new AuthorizerRequest
    {
      Headers = headers
    };
  }
}
