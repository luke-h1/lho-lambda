import AWSLambdaEvents
import Foundation
import HTTPTypes

enum ResponseBuilder {
    static let defaultCORSHeaders: [String: String] = [
        "content-type": "application/json",
        "Access-Control-Allow-Origin": "*",
        "Access-Control-Allow-Methods": "GET,OPTIONS,POST,PUT,DELETE"
    ]

    static func createResponse<T: Encodable>(
        body: T,
        includeCacheControl: Bool = true,
        revalidateSeconds: Int = 3
    ) throws -> APIGatewayV2Response {
        let encoder = JSONEncoder()
        var headers = defaultCORSHeaders

        if includeCacheControl {
            headers["Cache-Control"] =
                "max-age=\(revalidateSeconds), s-maxage=\(revalidateSeconds), stale-while-revalidate=\(revalidateSeconds), stale-if-error=\(revalidateSeconds)"
        } else {
            headers["Cache-Control"] = "no-cache"
        }

        let bodyData = try encoder.encode(body)
        let bodyString = String(data: bodyData, encoding: .utf8) ?? "{}"

        return APIGatewayV2Response(
            statusCode: .ok,
            headers: headers,
            body: bodyString
        )
    }

    static func errorResponse(statusCode: HTTPResponse.Status, message: String) -> APIGatewayV2Response {
        let body = (try? JSONEncoder().encode(["error": message])).flatMap { String(data: $0, encoding: .utf8) } ?? #"{"error":"Unknown error"}"#
        return APIGatewayV2Response(
            statusCode: statusCode,
            headers: defaultCORSHeaders,
            body: body
        )
    }
}
