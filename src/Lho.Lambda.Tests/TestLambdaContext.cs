using Amazon.Lambda.Core;

namespace Lho.Lambda.Tests;

public class TestLambdaContext : ILambdaContext
{
  public string AwsRequestId => "test-request";

  public IClientContext ClientContext => null!;

  public string FunctionName => "test-function";

  public string FunctionVersion => "test-version";

  public ICognitoIdentity Identity => null!;

  public string InvokedFunctionArn => "arn:aws:lambda:eu-west-2:123456789012:function:test";

  public ILambdaLogger Logger { get; } = new TestLambdaLogger();

  public string LogGroupName => "/aws/lambda/test";

  public string LogStreamName => "test-stream";

  public int MemoryLimitInMB => 256;

  public TimeSpan RemainingTime => TimeSpan.FromMinutes(1);
}

public class TestLambdaLogger : ILambdaLogger
{
  public void Log(string message)
  {
  }

  public void LogLine(string message)
  {
  }
}
