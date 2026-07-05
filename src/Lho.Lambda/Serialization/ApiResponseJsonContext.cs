using System.Text.Json.Serialization;
using Lho.Lambda.Models;

namespace Lho.Lambda.Serialization;

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(NowPlayingResponse))]
[JsonSerializable(typeof(TopTracksApiResponse))]
[JsonSerializable(typeof(VersionResponse))]
[JsonSerializable(typeof(HealthResponse))]
[JsonSerializable(typeof(ErrorResponseBody))]
public partial class ApiResponseJsonContext : JsonSerializerContext;
