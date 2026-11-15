using System.Text.Json.Serialization;

namespace Lho.Lambda.AlarmNotifier.Models;


public record DiscordWebhookPayload(
    [property: JsonPropertyName("content")] string Content,
    [property: JsonPropertyName("embeds")] IReadOnlyList<DiscordEmbed> Embeds);

public record DiscordEmbed(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("color")] int Color
);
