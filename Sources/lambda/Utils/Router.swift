import AWSLambdaEvents
import AWSLambdaRuntime
import Foundation
import HTTPTypes

final class Router: @unchecked Sendable {
    typealias Handler = (APIGatewayV2Request, LambdaContext) throws -> APIGatewayV2Response
    typealias AsyncHandler = (APIGatewayV2Request, LambdaContext) async throws ->
        APIGatewayV2Response

    private var routes: [Route] = []
    private var asyncRoutes: [AsyncRoute] = []

    func add(method: HTTPMethod, path: String, handler: @escaping Handler) {
        routes.append(Route(method: method, path: path, handler: handler))
    }

    func add(method: HTTPMethod, path: String, handler: @escaping AsyncHandler) {
        asyncRoutes.append(AsyncRoute(method: method, path: path, handler: handler))
    }

    func handle(event: APIGatewayV2Request, context: LambdaContext) async throws
        -> APIGatewayV2Response {
        let methodString = String(describing: event.context.http.method)
        let requestMethod = HTTPMethod(rawValue: methodString) ?? .GET
        var requestPath = event.rawPath

        // Strip stage prefix if present (e.g., /test/api/health -> /api/health)
        let knownStages = ["/test", "/staging", "/live", "/prod"]
        for stage in knownStages {
            if requestPath.hasPrefix(stage + "/") {
                requestPath = String(requestPath.dropFirst(stage.count))
                break
            } else if requestPath == stage {
                requestPath = "/"
                break
            }
        }

        if requestPath.hasPrefix("/invoke") {
            requestPath = String(requestPath.dropFirst(7))
            if requestPath.isEmpty {
                requestPath = "/"
            } else if !requestPath.hasPrefix("/") {
                requestPath = "/" + requestPath
            }
        }

        context.logger.debug(
            "Request \(methodString) \(requestPath)"
        )

        for asyncRoute in asyncRoutes {
            if asyncRoute.method == requestMethod && asyncRoute.matches(path: requestPath) {
                return try await asyncRoute.handler(event, context)
            }
        }

        for route in routes {
            if route.method == requestMethod && route.matches(path: requestPath) {
                return try route.handler(event, context)
            }
        }

        return ResponseBuilder.errorResponse(statusCode: HTTPResponse.Status.notFound, message: "Not Found")
    }
}

struct Route {
    let method: HTTPMethod
    let path: String
    let handler: Router.Handler

    func matches(path: String) -> Bool {
        return self.path == path
    }
}

struct AsyncRoute {
    let method: HTTPMethod
    let path: String
    let handler: Router.AsyncHandler

    func matches(path: String) -> Bool {
        return self.path == path
    }
}

enum HTTPMethod: String {
    case GET
    case POST
    case PUT
    case DELETE
    case PATCH
    case OPTIONS
}

func createRouter() -> Router {
    let router = Router()
    let cache = MemoryCache()
    let spotifyApi = SpotifyApi()

    router.add(method: .GET, path: "/api/health") { _, _ in
        (try? ResponseBuilder.createResponse(body: ["status": "OK"], includeCacheControl: false)) ?? ResponseBuilder.errorResponse(statusCode: HTTPResponse.Status.internalServerError, message: "Encoding error")
    }

    router.add(method: .GET, path: "/api/version") { _, context async throws in
        context.logger.info("Version endpoint called")
        let versionService = VersionService()
        return try await versionService.handleVersion()
    }

    router.add(method: .GET, path: "/api/now-playing") {
        _, context async throws -> APIGatewayV2Response in
        let nowplayingService = NowPlayingService(
            cache: cache,
            spotifyApi: spotifyApi,
            logger: context.logger
        )
        let response = try await nowplayingService.handleNowPlaying()
        return try ResponseBuilder.createResponse(body: response, revalidateSeconds: 3)
    }

    router.add(method: .GET, path: "/api/top-tracks") {
        event, context async throws -> APIGatewayV2Response in
        let timeRange = parseQueryParam(
            event: event, key: "time_range",
            defaultValue: "medium_term",
            allowedValues: ["short_term", "medium_term", "long_term"]
        )
        let limit = parseQueryParam(event: event, key: "limit", defaultValue: "20")
        let limitInt = min(50, max(1, Int(limit) ?? 20))

        let topTracksService = TopTracksService(
            spotifyApi: spotifyApi,
            logger: context.logger
        )
        do {
            let response = try await topTracksService.handleTopTracks(
                timeRange: timeRange,
                limit: limitInt
            )
            return try ResponseBuilder.createResponse(body: response, revalidateSeconds: 300)
        } catch {
            context.logger.error("Top tracks failed: \(error)")
            return ResponseBuilder.errorResponse(
                statusCode: HTTPResponse.Status.internalServerError,
                message: "Unable to fetch top tracks"
            )
        }
    }

    return router
}

private func parseQueryParam(
    event: APIGatewayV2Request, key: String, defaultValue: String,
    allowedValues: [String]? = nil
) -> String {
    let raw = event.rawQueryString
    guard let queryItems = URLComponents(string: "?\(raw)")?.queryItems else {
        return defaultValue
    }
    guard let value = queryItems.first(where: { $0.name == key })?.value, !value.isEmpty else {
        return defaultValue
    }
    if let allowed = allowedValues, !allowed.contains(value) {
        return defaultValue
    }
    return value
}
