using System.Text.Json;
using Amazon.Lambda.Core;
using Lho.Lambda.AlarmNotifier.Clients;
using Lho.Lambda.AlarmNotifier.Models;
using Lho.Lambda.AlarmNotifier.Serialization;
using Lho.Lambda.RuntimeConfiguration;

namespace Lho.Lambda.AlarmNotifier.Functions;

public class AlarmNotifierFunction(DiscordWebhookClient discord, string environmentName)
{
    private readonly DiscordWebhookClient _discord = discord;
    private readonly string _environmentName = environmentName;

    public AlarmNotifierFunction() : this(new DiscordWebhookClient(CreateHttpClient(),
        RuntimeConfig.Current.AlarmNotifier
    ), RuntimeConfig.Current.Observability.EnvironmentName)
    {

    }

    public async Task FunctionHandler(SnsEvent snsEvent, ILambdaContext context)
    {
        if (snsEvent.Records is not { Count: > 0 } records)
        {
            throw new InvalidOperationException("SNS event has no records");
        }

        Exception? firstError = null;

        foreach (var record in records)
        {
            AlarmNotification alarm;

            try
            {
                alarm = ParseAlarmMessage(record.Sns?.Message);
            }
            catch (Exception exception) when (exception is JsonException or InvalidOperationException)
            {
                context.Logger.LogLine($"skipping sns record: {exception.Message}");
                firstError ??= exception;
                continue;
            }
            context.Logger.LogLine($"alarm received {alarm.AlarmName} -> {alarm.NewStateValue} for reason -> {alarm.NewStateReason}");

            if (!string.Equals(alarm.NewStateValue, "ALARM", StringComparison.OrdinalIgnoreCase))
            {
                context.Logger.LogLine($"skipping non-alarm state {alarm.NewStateValue} for {alarm.AlarmName}");
                continue;
            }


            try
            {
                await _discord.SendAlarm(alarm, _environmentName);
            }
            catch (Exception exception)
            {
                context.Logger.LogLine($"notification failed for {alarm.AlarmName}: {exception.Message}");
                firstError ??= exception;
            }
        }

        if (firstError is not null)
        {
            throw new InvalidOperationException("One or more alarm notifications failed", firstError);
        }

    }

    public static AlarmNotification ParseAlarmMessage(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new InvalidOperationException("SNS record has no message");
        }

        var alarm = JsonSerializer.Deserialize(message, AlarmNotifierJsonContext.Default.AlarmNotification);
        if (string.IsNullOrEmpty(alarm?.AlarmName))
        {
            throw new InvalidOperationException("CloudWatch alarm message missing required field AlarmName");
        }

        return alarm;
    }

    private static HttpClient CreateHttpClient()
    {
        return new HttpClient(new SocketsHttpHandler { AllowAutoRedirect = false })
        {
            Timeout = TimeSpan.FromSeconds(5)
        };
    }
}
