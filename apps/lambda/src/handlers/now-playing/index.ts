import spotifyService from '@lambda/services/spotifyService';
import { Song, SongItem } from '@lambda/types/spotify';
import NodeCache from 'node-cache';

const nowPlayingHandler = async (): Promise<Song | string> => {
  const cache = new NodeCache({
    stdTTL: 5,
    checkperiod: 5,
    deleteOnExpire: true,
  });

  const cachedSong = cache.get<Song>('now-playing');

  if (cachedSong) {
    return JSON.stringify(cachedSong, null, 2);
  }

  if (process.env.SHOULD_CALL_SPOTIFY === 'false') {
    return JSON.stringify(
      {
        isPlaying: false,
        status: 200,
        maintenance: true,
      },
      null,
      2,
    );
  }

  const res = await spotifyService.getNowPlaying();
  if (res.status === 204 || res.status > 400) {
    return JSON.stringify(
      {
        isPlaying: false,
        status: 200,
      },
      null,
      2,
    );
  }

  const song = (await res.json()) as SongItem;

  if (!song.item) {
    return JSON.stringify(
      {
        isPlaying: false,
        status: 200,
      },
      null,
      2,
    );
  }

  const isPlaying = song.is_playing;
  const title = song.item.name;
  const artist = song.item.artists
    .map((_artist: { name: string }) => _artist.name)
    .join(', ');
  const album = song.item.album.name;

  // @ts-expect-error album images are optional
  const albumImageUrl = song.item.album.images[0].url;
  const songUrl = song.item.external_urls.spotify;

  cache.set('now-playing', {
    album,
    albumImageUrl,
    artist,
    isPlaying,
    songUrl,
    title,
  });

  return JSON.stringify(
    {
      album,
      albumImageUrl,
      artist,
      isPlaying,
      songUrl,
      title,
    },
    null,
    2,
  );
};
export default nowPlayingHandler;
