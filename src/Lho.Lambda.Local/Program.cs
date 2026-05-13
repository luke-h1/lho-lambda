using System.Net;
using System.Text;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Lho.Lambda.Functions;

var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:7000/");
listener.Start();

Console.WriteLine("Listening on http://localhost:7000/invoke");

var function = new ApiFunction();
var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
};

while (true)
{
    var context = await listener.GetContextAsync();
    _ = Task.Run(async () =>
    {
        try
        {
            if (context.Request.HttpMethod != "POST" || context.Request.Url?.AbsolutePath != "/invoke")
            {
                context.Response.StatusCode = 404;
                context.Response.Close();
                return;
            }

            using var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            var body = await reader.ReadToEndAsync();
            var request = JsonSerializer.Deserialize<APIGatewayHttpApiV2ProxyRequest>(body, jsonOptions)
                          ?? new APIGatewayHttpApiV2ProxyRequest();

            var response = await function.FunctionHandler(request, new LocalLambdaContext());
            context.Response.StatusCode = response.StatusCode;

            foreach (var (key, value) in response.Headers)
            {
                context.Response.Headers[key] = value;
            }

            var bytes = Encoding.UTF8.GetBytes(response.Body ?? "");
            context.Response.ContentLength64 = bytes.Length;
            await context.Response.OutputStream.WriteAsync(bytes);
            context.Response.Close();
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            context.Response.StatusCode = 500;
            context.Response.Close();
        }
    });
}

internal class LocalLambdaContext : ILambdaContext
{
    public string AwsRequestId => Guid.NewGuid().ToString("N");

    public IClientContext ClientContext => null!;

    public string FunctionName => "lho-lambda-local";

    public string FunctionVersion => "local";

    public ICognitoIdentity Identity => null!;

    public string InvokedFunctionArn => "local";

    public ILambdaLogger Logger { get; } = new LocalLambdaLogger();

    public string LogGroupName => "local";

    public string LogStreamName => "local";

    public int MemoryLimitInMB => 256;

    public TimeSpan RemainingTime => TimeSpan.FromMinutes(5);
}

internal class LocalLambdaLogger : ILambdaLogger
{
    public void Log(string message)
    {
        Console.Write(message);
    }

    public void LogLine(string message)
    {
        Console.WriteLine(message);
    }
}
