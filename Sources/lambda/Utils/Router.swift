import AWSLambdaEvents
import AWSLambdaRuntime
import Foundation

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
        -> APIGatewayV2Response
    {
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

        context.logger.info(
            "Received request - Method: \(methodString), rawPath: '\(event.rawPath)', processed path: '\(requestPath)', available routes: \(routes.map { "\($0.method.rawValue) \($0.path)" }) + \(asyncRoutes.map { "\($0.method.rawValue) \($0.path)" })"
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

        return APIGatewayV2Response(
            statusCode: .notFound,
            headers: ["content-type": "application/json"],
            body: #"{"error": "Not Found"}"#
        )
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

    router.add(method: .GET, path: "/api/health") { event, context in
        context.logger.info("Health check endpoint called")
        return APIGatewayV2Response(
            statusCode: .ok,
            headers: ["content-type": "application/json"],
            body: #"{"status": "OK"}"#
        )
    }

    router.add(method: .GET, path: "/api/version") { event, context async throws in
        context.logger.info("Version endpoint called")
        let versionService = VersionService()
        return try await versionService.handleVersion()
    }

    router.add(method: .GET, path: "/api/now-playing") {
        event, context async throws -> APIGatewayV2Response in
        context.logger.info("GET /api/now-playing called")
        let nowplayingService = NowPlayingService(
            cache: cache,
            spotifyApi: spotifyApi,
            logger: context.logger
        )
        let response = try await nowplayingService.handleNowPlaying()

        let encoder = JSONEncoder()
        let bodyData = try encoder.encode(response)
        let bodyString = String(data: bodyData, encoding: .utf8) ?? "{}"

        return APIGatewayV2Response(
            statusCode: .ok,
            headers: [
                "content-type": "application/json",
                "Access-Control-Allow-Origin": "*",
                "Access-Control-Allow-Methods": "GET,OPTIONS,POST,PUT,DELETE",
                "Cache-Control":
                    "max-age=3, s-maxage=3, stale-while-revalidate=3, stale-if-error=3",
            ],
            body: bodyString
        )
    }

    return router
}
