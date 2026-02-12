import Foundation

#if canImport(FoundationNetworking)
    import FoundationNetworking
#endif

enum SpotifyServiceError: Error, CustomStringConvertible, Sendable {
    case missingAccessToken
    case missingRefreshToken
    case missingClientCredentials
    case invalidURL(urlString: String)
    case invalidResponse
    case httpError(statusCode: Int, message: String? = nil)
    case decodingError(String)

    var description: String {
        switch self {
        case .missingAccessToken:
            return "Missing Spotify access token"
        case .missingRefreshToken:
            return "Missing Spotify refresh token"
        case .missingClientCredentials:
            return "Missing Spotify client ID or client secret"
        case .invalidURL(let urlString):
            return "Invalid URL: \(urlString)"
        case .invalidResponse:
            return "Invalid response from Spotify API"
        case .httpError(let statusCode, let message):
            if let message = message {
                return "HTTP error with status code: \(statusCode) - \(message)"
            }
            return "HTTP error with status code: \(statusCode)"
        case .decodingError(let details):
            return "Failed to decode response: \(details)"
        }
    }
}

struct TokenResponse: Codable {
    let accessToken: String
    let tokenType: String
    let expiresIn: Int

    enum CodingKeys: String, CodingKey {
        case accessToken = "access_token"
        case tokenType = "token_type"
        case expiresIn = "expires_in"
    }
}

actor SpotifyApi {
    private static let requestTimeout: TimeInterval = 10
    private let baseURL: String
    private let session: URLSession
    private let accessToken: String?
    private let clientId: String?
    private let clientSecret: String?
    private let refreshToken: String?
    private var cachedAccessToken: String?
    private var tokenExpiresAt: Date?

    init(baseURL: String? = nil, accessToken: String? = nil, session: URLSession? = nil) {
        self.baseURL = baseURL ?? "https://api.spotify.com/v1"
        let config: URLSessionConfiguration = {
            let c = URLSessionConfiguration.default
            c.timeoutIntervalForRequest = Self.requestTimeout
            c.timeoutIntervalForResource = Self.requestTimeout
            return c
        }()
        self.session = session ?? URLSession(configuration: config)
        self.accessToken = accessToken ?? Environment.Spotify.accessToken
        self.clientId = Environment.Spotify.clientId
        self.clientSecret = Environment.Spotify.clientSecret
        self.refreshToken = Environment.Spotify.refreshToken
    }

    private func getAccessToken() async throws -> String {
        if let accessToken = accessToken {
            return accessToken
        }

        if let cachedToken = cachedAccessToken,
            let expiresAt = tokenExpiresAt,
            expiresAt > Date() {
            return cachedToken
        }

        guard let refreshToken = refreshToken else {
            throw SpotifyServiceError.missingRefreshToken
        }

        guard let clientId = clientId, let clientSecret = clientSecret else {
            throw SpotifyServiceError.missingClientCredentials
        }

        let tokenURL = "https://accounts.spotify.com/api/token"
        guard let url = URL(string: tokenURL) else {
            throw SpotifyServiceError.invalidURL(urlString: tokenURL)
        }

        let credentials = "\(clientId):\(clientSecret)"
        guard let credentialsData = credentials.data(using: .utf8) else {
            throw SpotifyServiceError.invalidResponse
        }
        let base64Credentials = credentialsData.base64EncodedString()

        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("Basic \(base64Credentials)", forHTTPHeaderField: "Authorization")
        request.setValue("application/x-www-form-urlencoded", forHTTPHeaderField: "Content-Type")

        var components = URLComponents()
        components.scheme = "https"
        components.host = "accounts.spotify.com"
        components.path = "/api/token"
        components.queryItems = [
            URLQueryItem(name: "grant_type", value: "refresh_token"),
            URLQueryItem(name: "refresh_token", value: refreshToken)
        ]
        guard let queryString = components.url?.query else {
            throw SpotifyServiceError.invalidResponse
        }
        request.httpBody = queryString.data(using: .utf8)

        let (data, response) = try await session.data(for: request)

        guard let httpResponse = response as? HTTPURLResponse else {
            throw SpotifyServiceError.invalidResponse
        }

        guard (200...299).contains(httpResponse.statusCode) else {
            let errorMessage =
                String(data: data, encoding: .utf8) ?? "Unable to read error response"
            throw SpotifyServiceError.httpError(
                statusCode: httpResponse.statusCode, message: errorMessage)
        }

        guard !data.isEmpty else {
            throw SpotifyServiceError.decodingError("Empty response from token endpoint")
        }

        let responseString = String(data: data, encoding: .utf8) ?? "Unable to decode response"

        let lowercasedResponse = responseString.lowercased()
        if lowercasedResponse.contains("\"error\"")
            || lowercasedResponse.contains("error_description") {
            throw SpotifyServiceError.httpError(
                statusCode: httpResponse.statusCode, message: responseString)
        }

        if responseString.trimmingCharacters(in: .whitespacesAndNewlines).hasPrefix("<") {
            throw SpotifyServiceError.httpError(
                statusCode: httpResponse.statusCode,
                message: "Received HTML response instead of JSON: \(responseString.prefix(200))")
        }

        let decoder = JSONDecoder()
        do {
            let tokenResponse = try decoder.decode(TokenResponse.self, from: data)
            cachedAccessToken = tokenResponse.accessToken
            tokenExpiresAt = Date().addingTimeInterval(TimeInterval(tokenResponse.expiresIn - 60))

            return tokenResponse.accessToken
        } catch {
            let errorDetails = "\(error)"
            let errorMessage: String
            if let decodingError = error as? DecodingError {
                switch decodingError {
                case .keyNotFound(let key, let context):
                    errorMessage =
                        "Missing key '\(key.stringValue)' at \(context.codingPath.map { $0.stringValue }.joined(separator: ".")). Response body: \(responseString)"
                case .typeMismatch(let type, let context):
                    errorMessage =
                        "Type mismatch for '\(context.codingPath.map { $0.stringValue }.joined(separator: "."))': expected \(type). Response body: \(responseString)"
                case .valueNotFound(let type, let context):
                    errorMessage =
                        "Value not found for '\(context.codingPath.map { $0.stringValue }.joined(separator: "."))': expected \(type). Response body: \(responseString)"
                case .dataCorrupted(let context):
                    errorMessage =
                        "Data corrupted at \(context.codingPath.map { $0.stringValue }.joined(separator: ".")): \(context.debugDescription). Response body: \(responseString)"
                @unknown default:
                    errorMessage =
                        "Decoding error: \(errorDetails). Response body: \(responseString)"
                }
            } else {
                errorMessage =
                    "Failed to decode token response: \(errorDetails). Response body: \(responseString)"
            }
            throw SpotifyServiceError.decodingError(errorMessage)
        }

    }

    func getNowPlaying() async throws -> SpotifyResponse? {
        let accessToken = try await getAccessToken()

        let urlString = "\(baseURL)/me/player/currently-playing"
        guard let url = URL(string: urlString) else {
            throw SpotifyServiceError.invalidURL(urlString: urlString)
        }

        var request = URLRequest(url: url)
        request.httpMethod = "GET"
        request.setValue("Bearer \(accessToken)", forHTTPHeaderField: "Authorization")
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")

        let (data, response) = try await session.data(for: request)

        guard let httpResponse = response as? HTTPURLResponse else {
            throw SpotifyServiceError.invalidResponse
        }

        if httpResponse.statusCode == 204 {
            return nil
        }

        guard (200...299).contains(httpResponse.statusCode) else {
            let errorMessage =
                String(data: data, encoding: .utf8) ?? "Unable to read error response"
            throw SpotifyServiceError.httpError(
                statusCode: httpResponse.statusCode, message: errorMessage)
        }

        guard !data.isEmpty else {
            throw SpotifyServiceError.decodingError("Empty response from Spotify API")
        }

        return try decodeSpotifyResponse(SpotifyResponse.self, from: data)
    }

    func getTopTracks(timeRange: String = "medium_term", limit: Int = 10) async throws
        -> SpotifyTopTracksResponse {
        let accessToken = try await getAccessToken()

        var components = URLComponents(string: "\(baseURL)/me/top/tracks")!
        components.queryItems = [
            URLQueryItem(name: "time_range", value: timeRange),
            URLQueryItem(name: "limit", value: String(min(50, max(1, limit))))
        ]

        guard let url = components.url else {
            throw SpotifyServiceError.invalidURL(
                urlString: components.string ?? "\(baseURL)/me/top/tracks")
        }

        var request = URLRequest(url: url)
        request.httpMethod = "GET"
        request.setValue("Bearer \(accessToken)", forHTTPHeaderField: "Authorization")
        request.setValue("application/json", forHTTPHeaderField: "Content-Type")

        let (data, response) = try await session.data(for: request)

        guard let httpResponse = response as? HTTPURLResponse else {
            throw SpotifyServiceError.invalidResponse
        }

        guard (200...299).contains(httpResponse.statusCode) else {
            let errorMessage =
                String(data: data, encoding: .utf8) ?? "Unable to read error response"
            throw SpotifyServiceError.httpError(
                statusCode: httpResponse.statusCode, message: errorMessage)
        }

        guard !data.isEmpty else {
            throw SpotifyServiceError.decodingError("Empty response from Spotify API")
        }

        return try decodeSpotifyResponse(SpotifyTopTracksResponse.self, from: data)
    }

    private func decodeSpotifyResponse<T: Decodable>(_ type: T.Type, from data: Data) throws
        -> T {
        let decoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        do {
            return try decoder.decode(type, from: data)
        } catch {
            let errorDetails = "\(error)"
            if let decodingError = error as? DecodingError {
                switch decodingError {
                case .keyNotFound(let key, let context):
                    throw SpotifyServiceError.decodingError(
                        "Missing key '\(key.stringValue)' at \(context.codingPath.map { $0.stringValue }.joined(separator: "."))"
                    )
                case .typeMismatch(let type, let context):
                    throw SpotifyServiceError.decodingError(
                        "Type mismatch for '\(context.codingPath.map { $0.stringValue }.joined(separator: "."))': expected \(type)"
                    )
                case .valueNotFound(let type, let context):
                    throw SpotifyServiceError.decodingError(
                        "Value not found for '\(context.codingPath.map { $0.stringValue }.joined(separator: "."))': expected \(type)"
                    )
                case .dataCorrupted(let context):
                    throw SpotifyServiceError.decodingError(
                        "Data corrupted at \(context.codingPath.map { $0.stringValue }.joined(separator: ".")): \(context.debugDescription)"
                    )
                @unknown default:
                    throw SpotifyServiceError.decodingError("Decoding error: \(errorDetails)")
                }
            }
            throw SpotifyServiceError.decodingError(
                "Failed to decode Spotify response: \(errorDetails)")
        }
    }
}
