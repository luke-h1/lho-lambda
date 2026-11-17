using System.Text.Json.Serialization;
using Lho.Lambda.Models;

namespace Lho.Lambda.Serialization;

[JsonSourceGenerationOptions(PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(LastFmRecentTracksResponse))]
[JsonSerializable(typeof(LastFmTopTracksResponse))]
[JsonSerializable(typeof(LastFmTrack))]
[JsonSerializable(typeof(LastFmTopTrack))]
[JsonSerializable(typeof(LastFmErrorResponse))]
public partial class LastFmJsonContext : JsonSerializerContext;
