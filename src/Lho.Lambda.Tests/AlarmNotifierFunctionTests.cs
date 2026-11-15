using System.Net;
using System.Text.Json;
using Lho.Lambda.AlarmNotifier.Clients;
using Lho.Lambda.AlarmNotifier.Functions;
using Lho.Lambda.AlarmNotifier.Models;
using Lho.Lambda.RuntimeConfiguration.Options;
using Xunit;

namespace Lho.Lambda.Tests;

public class AlarmNotifierFunctionTests
{
    private const string WebhookUrl = "https://discord.test/api/webhooks/1/token";

    [Fact]
    public async Task PostsAlarmStateToDiscord()
    {
        var handler = new RecordingHandler(HttpStatusCode.NoContent);
        var function = CreateFunction(handler);

        await function.FunctionHandler(CreateEvent(AlarmMessage("ALARM")), new TestLambdaContext());

        var request = Assert.Single(handler.Requests);
        Assert.Equal($"{WebhookUrl}?wait=true", request.Uri);

        using var payload = JsonDocument.Parse(request.Body);
        Assert.Equal("CW alarm **now-playing-lambda-live-invocations-anomaly** -> `ALARM`", payload.RootElement.GetProperty("content").GetString());

        var embed = payload.RootElement.GetProperty("embeds")[0];
        Assert.Equal("CW: ALARM", embed.GetProperty("title").GetString());
        Assert.Equal(0xed4245, embed.GetProperty("color").GetInt32());
        Assert.Contains("Env: live", embed.GetProperty("description").GetString());
        Assert.Contains("Reason: Thresholds Crossed", embed.GetProperty("description").GetString());
    }

    [Theory]
    [InlineData("OK")]
    [InlineData("INSUFFICIENT_DATA")]
    public async Task PostsEveryStateTransition(string state)
    {
        var handler = new RecordingHandler(HttpStatusCode.NoContent);
        var function = CreateFunction(handler);

        await function.FunctionHandler(CreateEvent(AlarmMessage(state)), new TestLambdaContext());

        var request = Assert.Single(handler.Requests);
        using var payload = JsonDocument.Parse(request.Body);
        Assert.Equal(
            $"CW alarm **now-playing-lambda-live-invocations-anomaly** -> `{state}`",
            payload.RootElement.GetProperty("content").GetString());
    }

    [Fact]
    public async Task ThrowsWhenDiscordRejectsTheWebhook()
    {
        var function = CreateFunction(new RecordingHandler(HttpStatusCode.BadRequest));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
          function.FunctionHandler(CreateEvent(AlarmMessage("ALARM")), new TestLambdaContext()));
    }

    [Fact]
    public async Task ThrowsOnMalformedMessageButStillNotifiesValidRecords()
    {
        var handler = new RecordingHandler(HttpStatusCode.NoContent);
        var function = CreateFunction(handler);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
          function.FunctionHandler(CreateEvent("not json", AlarmMessage("ALARM")), new TestLambdaContext()));

        Assert.Single(handler.Requests);
    }

    [Fact]
    public void ParseAlarmMessageRequiresAlarmName()
    {
        Assert.Throws<InvalidOperationException>(() => AlarmNotifierFunction.ParseAlarmMessage("{\"NewStateValue\":\"ALARM\"}"));
    }

    private static AlarmNotifierFunction CreateFunction(RecordingHandler handler)
    {
        var client = new DiscordWebhookClient(
          new HttpClient(handler),
          new AlarmNotifierOptions { DiscordWebhookUrl = WebhookUrl });

        return new AlarmNotifierFunction(client, "live");
    }

    private static SnsEvent CreateEvent(params string[] messages)
    {
        return new SnsEvent
        {
            Records = [.. messages.Select(message => new SnsEventRecord { Sns = new SnsMessage { Message = message } })]
        };
    }

    private static string AlarmMessage(string state)
    {
        return JsonSerializer.Serialize(new Dictionary<string, string>
        {
            ["AlarmName"] = "now-playing-lambda-live-invocations-anomaly",
            ["AlarmDescription"] = "Unusual invocation rate on the now-playing Lambda (live)",
            ["NewStateValue"] = state,
            ["NewStateReason"] = "Thresholds Crossed",
            ["StateChangeTime"] = "2026-10-15T20:00:00.000+0000",
            ["Region"] = "EU (London)",
            ["OldStateValue"] = "OK"
        });
    }

    private sealed class RecordingHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        public List<(string Uri, string Body)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
            Requests.Add((request.RequestUri!.ToString(), body));
            return new HttpResponseMessage(statusCode);
        }
    }
}
