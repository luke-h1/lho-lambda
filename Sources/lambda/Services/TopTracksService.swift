import Foundation
import Logging

actor TopTracksService {
    private let spotifyApi: SpotifyApi
    private let logger: Logger

    init(spotifyApi: SpotifyApi, logger: Logger) {
        self.spotifyApi = spotifyApi
        self.logger = logger
    }

    func handleTopTracks(timeRange: String, limit: Int) async throws -> TopTracksApiResponse {
        do {
            let response = try await spotifyApi.getTopTracks(timeRange: timeRange, limit: limit)

            let tracks = response.items.map { item in
                TopTrackResponseItem(
                    title: item.name,
                    artist: item.artists.map(\.name).joined(separator: ", "),
                    album: item.album.name,
                    albumImageUrl: item.album.images.first?.url ?? "",
                    songUrl: item.externalUrls.spotify
                )
            }

            return TopTracksApiResponse(tracks: tracks)
        } catch {
            if let spotifyError = error as? SpotifyServiceError {
                logger.error("Top tracks fetch failed: \(spotifyError.description)")
            } else {
                logger.error("Top tracks fetch failed: \(error.localizedDescription)")
            }
            throw error
        }
    }
}
