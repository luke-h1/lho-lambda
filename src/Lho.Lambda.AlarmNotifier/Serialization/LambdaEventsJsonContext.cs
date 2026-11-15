using System.Text.Json.Serialization;
using Lho.Lambda.AlarmNotifier.Models;

namespace Lho.Lambda.AlarmNotifier.Serialization;

[JsonSerializable(typeof(SnsEvent))]
public partial class LambdaEventsJsonContext : JsonSerializerContext;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(AlarmNotification))]
[JsonSerializable(typeof(DiscordWebhookPayload))]
public partial class AlarmNotifierJsonContext : JsonSerializerContext;
