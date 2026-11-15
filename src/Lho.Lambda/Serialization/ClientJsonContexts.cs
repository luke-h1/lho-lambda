using System.Text.Json.Serialization;
using Lho.Lambda.Models;

namespace Lho.Lambda.Serialization;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(LastFmRecentTracksResponse))]
[JsonSerializable(typeof(LastFmTopTracksResponse))]
public partial class LastFmJsonContext : JsonSerializerContext;
