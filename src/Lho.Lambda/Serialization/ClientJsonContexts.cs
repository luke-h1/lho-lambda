using System.Text.Json.Serialization;
using Lho.Lambda.Models;

namespace Lho.Lambda.Serialization;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(LastFmRecentTracksResponse))]
public partial class LastFmJsonContext : JsonSerializerContext;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(SpotifyResponse))]
[JsonSerializable(typeof(SpotifyTopTracksResponse))]
[JsonSerializable(typeof(TokenResponse))]
public partial class SpotifyJsonContext : JsonSerializerContext;
