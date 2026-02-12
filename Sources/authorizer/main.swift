import AWSLambdaRuntime
import Foundation

private func secureCompare(_ a: String?, _ b: String?) -> Bool {
    guard let a = a, let b = b else { return a == nil && b == nil }
    guard a.count == b.count else { return false }
    return zip(a.utf8, b.utf8).reduce(0 as UInt8) { $0 | ($1.0 ^ $1.1) } == 0
}

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
    context.logger.debug("Authorizer invoked")

    let consumer = getHeaderValue(event.headers, key: "x-consumer")
    let validConsumers = ["lhowsam-dev", "lhowsam-prod", "lhowsam-local"]

    let apiKey = getHeaderValue(event.headers, key: "x-api-key")
    let validKey = ProcessInfo.processInfo.environment["API_KEY"]

    if !secureCompare(apiKey, validKey) {
        context.logger.info("Deny - API key invalid")
        return AuthorizerSimpleResponse(isAuthorized: false)
    }

    if let consumer = consumer, !validConsumers.contains(consumer) {
        context.logger.info("Deny - Invalid consumer")
        return AuthorizerSimpleResponse(isAuthorized: false)
    }

    context.logger.debug("Allow")
    return AuthorizerSimpleResponse(isAuthorized: true)
}

try await runtime.run()
