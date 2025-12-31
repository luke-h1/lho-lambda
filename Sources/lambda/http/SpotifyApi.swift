import Foundation

#if canImport(FoundationNetworking)
    import FoundationNetworking
#endif

enum SpotifyServiceError: Error, CustomStringConvertible {
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
    private let baseURL: String
    private let accessToken: String?
    private let clientId: String?
    private let clientSecret: String?
    private let refreshToken: String?
    private var cachedAccessToken: String?
    private var tokenExpiresAt: Date?

    init(baseURL: String? = nil, accessToken: String? = nil) {
        self.baseURL = baseURL ?? "https://api.spotify.com/v1"
        let env = ProcessInfo.processInfo.environment
        self.accessToken = accessToken ?? env["SPOTIFY_ACCESS_TOKEN"]
        self.clientId = env["SPOTIFY_CLIENT_ID"]
        self.clientSecret = env["SPOTIFY_CLIENT_SECRET"]
        self.refreshToken = env["SPOTIFY_REFRESH_TOKEN"]
    }

    private func getAccessToken() async throws -> String {
        // If we have a direct access token, use it
        if let accessToken = accessToken {
            return accessToken
        }

        // If we have a cached token that hasn't expired, use it
        if let cachedToken = cachedAccessToken,
            let expiresAt = tokenExpiresAt,
            expiresAt > Date()
        {
            return cachedToken
        }

        // Otherwise, refresh the token
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

        // Create basic auth header
        let credentials = "\(clientId):\(clientSecret)"
        guard let credentialsData = credentials.data(using: .utf8) else {
            throw SpotifyServiceError.invalidResponse
        }
        let base64Credentials = credentialsData.base64EncodedString()

        var request = URLRequest(url: url)
        request.httpMethod = "POST"
        request.setValue("Basic \(base64Credentials)", forHTTPHeaderField: "Authorization")
        request.setValue("application/x-www-form-urlencoded", forHTTPHeaderField: "Content-Type")

        let refreshTokenEncoded =
            refreshToken.addingPercentEncoding(withAllowedCharacters: .urlQueryAllowed)
            ?? refreshToken
        let bodyString = "grant_type=refresh_token&refresh_token=\(refreshTokenEncoded)"
        request.httpBody = bodyString.data(using: .utf8)

        let (data, response) = try await URLSession.shared.data(for: request)

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

        // Log the response for debugging if it's not what we expect
        let responseString = String(data: data, encoding: .utf8) ?? "Unable to decode response"

        // Check if the response is an error response from Spotify
        if responseString.contains("\"error\"") {
            throw SpotifyServiceError.httpError(
                statusCode: httpResponse.statusCode, message: responseString)
        }

        let decoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        do {
            let tokenResponse = try decoder.decode(TokenResponse.self, from: data)
            // Cache the token (subtract 60 seconds for safety margin)
            cachedAccessToken = tokenResponse.accessToken
            tokenExpiresAt = Date().addingTimeInterval(TimeInterval(tokenResponse.expiresIn - 60))

            return tokenResponse.accessToken
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
            // Include the actual response in the error for debugging
            throw SpotifyServiceError.decodingError(
                "Failed to decode token response: \(errorDetails). Response body: \(responseString)"
            )
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

        let (data, response) = try await URLSession.shared.data(for: request)

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

        let decoder = JSONDecoder()
        decoder.keyDecodingStrategy = .convertFromSnakeCase
        do {
            return try decoder.decode(SpotifyResponse.self, from: data)
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
