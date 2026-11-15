namespace Lho.Lambda.RuntimeConfiguration.Options;

public class AlarmNotifierOptions
{
    public const string Section = "AlarmNotifier";

    public string? DiscordWebhookUrl { get; init; }

    public string RequireDiscordWebhookUrl()
    {
        return string.IsNullOrWhiteSpace(DiscordWebhookUrl)
            ? throw new RuntimeConfigurationException(
              "Discord webhook URL is not configured"
            )
            : DiscordWebhookUrl;
    }
}
