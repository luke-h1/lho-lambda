import Foundation

#if canImport(FoundationNetworking)
    import FoundationNetworking
#endif

enum SpotifyServiceError: Error {
    case missingAccessToken
    case invalidURL
    case invalidResponse
    case httpError(statusCode: Int)
}

actor SpotifyApi {
    private let baseURL: String
    private let accessToken: String?

    init(baseURL: String? = nil, accessToken: String? = nil) {
        self.baseURL = baseURL ?? "https://api.spotify.com/v1"
        self.accessToken =
            accessToken ?? ProcessInfo.processInfo.environment["SPOTIFY_ACCESS_TOKEN"]
    }

    func getNowPlaying() async throws -> SpotifyResponse? {
        guard let accessToken = accessToken else {
            throw SpotifyServiceError.missingAccessToken
        }

        guard let url = URL(string: "\(baseURL)/me/player/currently-playing") else {
            throw SpotifyServiceError.missingAccessToken
        }

        var request = URLRequest(url: url)
        request.httpMethod = "GET"
        request.setValue("Bearer \(accessToken)", forHTTPHeaderField: "Authorization")
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")

        let (data, response) = try await URLSession.shared.data(for: request)

        guard let httpResponse = response as? HTTPURLResponse else {
            throw SpotifyServiceError.invalidResponse
        }

        if httpResponse.statusCode == 204 {
            return nil
        }

        guard (200...299).contains(httpResponse.statusCode) else {
            throw SpotifyServiceError.httpError(statusCode: httpResponse.statusCode)
        }

        let decoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        return try decoder.decode(SpotifyResponse.self, from: data)

    }
}
