import Foundation

struct NowPlayingResponse: Codable {
    let isPlaying: Bool
    let maintenance: Bool?
    let status: Int
    let album: String
    let albumImageUrl: String
    let artist: String
    let songUrl: String
    let title: String
}

struct SpotifyResponse: Codable {
    let isPlaying: Bool
    let item: SpotifyItem?
}

struct SpotifyItem: Codable {
    let name: String
    let artists: [SpotifyArtist]
    let album: SpotifyAlbum
    let externalUrls: SpotifyExternalUrls
}

struct SpotifyArtist: Codable {
    let name: String
}

struct SpotifyImage: Codable {
    let url: String
    let height: Int?
    let width: Int?
}

struct SpotifyAlbum: Codable {
    let name: String
    let images: [SpotifyImage]
}

struct SpotifyExternalUrls: Codable {
    let spotify: String
}

struct SpotifyTopTracksResponse: Codable {
    let items: [SpotifyItem]
}

struct TopTrackResponseItem: Codable {
    let title: String
    let artist: String
    let album: String
    let albumImageUrl: String
    let songUrl: String
}

struct TopTracksApiResponse: Codable {
    let tracks: [TopTrackResponseItem]
}
