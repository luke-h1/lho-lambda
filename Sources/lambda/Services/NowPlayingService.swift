import AWSLambdaEvents
import AWSLambdaRuntime
import Foundation
import Logging

actor NowPlayingService {
    private static let nowPlayingCacheKey = "NowPlaying"
    private static let revalidateSeconds = 3

    private let cache: MemoryCache
    private let spotifyApi: SpotifyApi
    private let logger: Logger

    init(cache: MemoryCache, spotifyApi: SpotifyApi, logger: Logger) {
        self.cache = cache
        self.spotifyApi = spotifyApi
        self.logger = logger
    }

    func handleNowPlaying() async throws -> NowPlayingResponse {
        do {
            if let cachedResponse: NowPlayingResponse = await cache.get(Self.nowPlayingCacheKey) {
                logger.info("Returning cached now playing response")
                return cachedResponse
            }

            if !Environment.shouldCallSpotify {
                return NowPlayingResponse(
                    isPlaying: false, maintenance: true, status: 200, album: "",
                    albumImageUrl: "", artist: "", songUrl: "", title: ""
                )
            }

            let nowPlayingResponse = try await spotifyApi.getNowPlaying()

            if nowPlayingResponse?.item == nil {
                logger.info("No song currently playing")

                return NowPlayingResponse(
                    isPlaying: false,
                    maintenance: nil,
                    status: 200,
                    album: "",
                    albumImageUrl: "",
                    artist: "",
                    songUrl: "",
                    title: ""
                )
            }

            guard let item = nowPlayingResponse?.item else {
                throw NowPlayingError.missingItem
            }

            let response = NowPlayingResponse(
                isPlaying: nowPlayingResponse?.isPlaying ?? false,
                maintenance: nil,
                status: 200,
                album: item.album.name,
                albumImageUrl: item.album.images.first?.url ?? "",
                artist: item.artists.map { $0.name }.joined(separator: ", "),
                songUrl: item.externalUrls.spotify,
                title: item.name
            )

            await cache.set(Self.nowPlayingCacheKey, value: response, expiration: 5.0)

            return response
        } catch {
            if let spotifyError = error as? SpotifyServiceError {
                logger.error("Error fetching nowplaying data from spotify: \(spotifyError.description)")
            } else {
                logger.error(
                    "Error fetching nowplaying data from spotify: \(error.localizedDescription)")
            }

            return NowPlayingResponse(
                isPlaying: false,
                maintenance: nil,
                status: 500,
                album: "",
                albumImageUrl: "",
                artist: "",
                songUrl: "",
                title: ""
            )
        }
    }
}

enum NowPlayingError: Error {
    case missingItem
}
