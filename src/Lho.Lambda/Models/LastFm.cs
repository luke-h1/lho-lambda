using System.Text.Json.Serialization;

namespace Lho.Lambda.Models;

public record LastFmRecentTracksResponse(
  [property: JsonPropertyName("recenttracks")] LastFmRecentTracks RecentTracks);

public record LastFmRecentTracks(
  [property: JsonPropertyName("track")] IReadOnlyList<LastFmTrack> Tracks);

public record LastFmTrack(
  [property: JsonPropertyName("name")] string Name,
  [property: JsonPropertyName("artist")] LastFmTextValue Artist,
  [property: JsonPropertyName("album")] LastFmTextValue Album,
  [property: JsonPropertyName("url")] string Url,
  [property: JsonPropertyName("image")] IReadOnlyList<LastFmImage> Images,
  [property: JsonPropertyName("@attr")] LastFmTrackAttributes? Attributes,
  [property: JsonPropertyName("date")] LastFmDate? Date = null);

public record LastFmTextValue(
  [property: JsonPropertyName("#text")] string Text);

public record LastFmImage(
  [property: JsonPropertyName("#text")] string Url,
  [property: JsonPropertyName("size")] string Size);

public record LastFmTrackAttributes(
  [property: JsonPropertyName("nowplaying")] string? NowPlaying);

public record LastFmDate(
  [property: JsonPropertyName("uts")] string? Uts,
  [property: JsonPropertyName("#text")] string? Text);

public record RecentTrackResponseItem(
  string Title,
  string Artist,
  string Album,
  string AlbumImageUrl,
  string SongUrl,
  bool NowPlaying,
  long? PlayedAt);

public record RecentTracksApiResponse(IReadOnlyList<RecentTrackResponseItem> Tracks);
