import AWSLambdaRuntime
import Foundation

struct AuthorizerRequest: Codable {
    let version: String?
    let type: String?
    let routeArn: String?
    let routeKey: String?
    let rawPath: String?
    let rawQueryString: String?
    let headers: [String: String]?
    let requestContext: RequestContext?

    struct RequestContext: Codable {
        let http: HTTPContext?

        struct HTTPContext: Codable {
            let method: String?
            let path: String?
        }
    }
}

struct AuthorizerSimpleResponse: Codable {
    let isAuthorized: Bool
}

func getHeaderValue(_ headers: [String: String]?, key: String) -> String? {
    guard let headers = headers else {
        return nil
    }

    for (headerKey, value) in headers {
        if headerKey.caseInsensitiveCompare(key) == .orderedSame {
            return value
        }
    }

    return nil
}

let runtime = LambdaRuntime {
    (event: AuthorizerRequest, context: LambdaContext) -> AuthorizerSimpleResponse in

    context.logger.info("Authorizer invoked")
    context.logger.info("Headers: \(String(describing: event.headers))")
    context.logger.info("RouteArn: \(String(describing: event.routeArn))")

    let consumer = getHeaderValue(event.headers, key: "x-consumer")
    let validConsumers = ["lhowsam-dev", "lhowsam-prod", "lhowsam-local"]

    let apiKey = getHeaderValue(event.headers, key: "x-api-key")
    let validKey = ProcessInfo.processInfo.environment["API_KEY"]

    context.logger.info("API Key from header: '\(apiKey ?? "nil")'")

    if apiKey != validKey {
        context.logger.info("Deny - API key mismatch")
        return AuthorizerSimpleResponse(isAuthorized: false)
    }

    if let consumer = consumer, !validConsumers.contains(consumer) {
        context.logger.info("Deny - Invalid consumer: \(consumer)")
        return AuthorizerSimpleResponse(isAuthorized: false)
    }

    context.logger.info("Allow - Authorization successful")
    return AuthorizerSimpleResponse(isAuthorized: true)
}

try await runtime.run()
