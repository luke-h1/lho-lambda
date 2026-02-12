import Foundation

enum Environment {
    private static let env = ProcessInfo.processInfo.environment

    static func string(_ key: String, default defaultValue: String = "") -> String {
        env[key] ?? defaultValue
    }

    static func bool(_ key: String, default defaultValue: Bool = false) -> Bool {
        guard let value = env[key] else { return defaultValue }
        return value.lowercased() == "true" || value == "1"
    }

    enum Spotify {
        static var clientId: String? { env["SPOTIFY_CLIENT_ID"] }
        static var clientSecret: String? { env["SPOTIFY_CLIENT_SECRET"] }
        static var refreshToken: String? { env["SPOTIFY_REFRESH_TOKEN"] }
        static var accessToken: String? { env["SPOTIFY_ACCESS_TOKEN"] }
    }

    enum Deploy {
        static var version: String { Environment.string("VERSION", default: "unknown") }
        static var deployedAt: String { Environment.string("DEPLOYED_AT", default: "unknown") }
        static var deployedBy: String { Environment.string("DEPLOYED_BY", default: "unknown") }
        static var gitSha: String { Environment.string("GIT_SHA", default: "unknown") }
    }

    static var shouldCallSpotify: Bool {
        Environment.bool("SHOULD_CALL_SPOTIFY", default: true)
    }
}
