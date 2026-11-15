using System.Text;
using System.Text.Json;
using Lho.Lambda.AlarmNotifier.Models;
using Lho.Lambda.AlarmNotifier.Serialization;
using Lho.Lambda.RuntimeConfiguration.Options;

namespace Lho.Lambda.AlarmNotifier.Clients;

public class DiscordWebhookClient(HttpClient httpClient, AlarmNotifierOptions options)
{
    private const int ContentLimit = 2000;
    private const int EmbedTitleLimit = 256;
    private const int EmbedDescriptionLimit = 4096;

    private readonly HttpClient _httpClient = httpClient;
    private readonly AlarmNotifierOptions _options = options;

    public async Task SendAlarm(AlarmNotification alarm, string environmentName, CancellationToken cancellationToken = default)
    {
        var payload = new DiscordWebhookPayload(
            Truncate($"CW alarm **{alarm.AlarmName}** -> `{alarm.NewStateValue}`", ContentLimit),
            [
                new DiscordEmbed(
                Truncate($"CW: {alarm.NewStateValue}", EmbedTitleLimit),
                Truncate(FormatPlainText(alarm, environmentName),EmbedDescriptionLimit),
                StateColor(alarm.NewStateValue)
            )
            ]
        );

        var body = JsonSerializer.Serialize(payload, AlarmNotifierJsonContext.Default.DiscordWebhookPayload);
        using var request = new HttpRequestMessage(HttpMethod.Post, _options.RequireDiscordWebhookUrl())
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Discord webhook returned {(int)response.StatusCode}: {Truncate(responseBody.Trim(), 1024)}");
        }
    }


    public static string FormatPlainText(AlarmNotification alarm, string environmentName)
    {
        var builder = new StringBuilder();
        builder.Append("CloudWatch alarm: ").Append(alarm.NewStateValue);
        builder.Append("\nAlarm: ").Append(alarm.AlarmName);
        AppendLine(builder, "Env", environmentName);
        AppendLine(builder, "Region", alarm.Region);
        AppendLine(builder, "Previous", alarm.OldStateValue);
        AppendLine(builder, "Time", alarm.StateChangeTime);
        AppendLine(builder, "Description", alarm.AlarmDescription);
        AppendLine(builder, "Reason", alarm.NewStateReason);
        return builder.ToString();
    }

    private static void AppendLine(StringBuilder builder, string label, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            builder.Append('\n').Append(label).Append(": ").Append(value);
        }
    }

    private static int StateColor(string? state)
    {
        return state?.ToUpperInvariant() switch
        {
            "ALARM" => 0xed4245,
            "OK" => 0x57f287,
            _ => 0xfee75c
        };
    }

    private static string Truncate(string value, int max)
    {
        return value.Length <= max ? value : string.Concat(value.AsSpan(0, max - 1), "…");
    }
}
