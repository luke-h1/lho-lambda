import AWSLambdaEvents
import Foundation

enum ResponseBuilder {
    static func createResponse<T: Encodable>(
        body: T,
        includeCacheControl: Bool,
        revalidateSeconds: Int = 3
    ) -> APIGatewayV2Response {
        let encoder = JSONEncoder()

        var headers: [String: String] = [
            "content-type": "application/json",
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Methods": "GET,OPTIONS,POST,PUT,DELETE",
        ]

        if includeCacheControl {
            headers["Cache-Control"] =
                "max-age=\(revalidateSeconds), s-maxage=\(revalidateSeconds), stale-while-revalidate=\(revalidateSeconds), stale-if-error=\(revalidateSeconds)"
        } else {
            headers["Cache-Control"] = "no-cache"
        }

        let bodyData = try? encoder.encode(body)
        let bodyString = bodyData.flatMap { String(data: $0, encoding: .utf8) } ?? "{}"

        return APIGatewayV2Response(
            statusCode: .ok,
            headers: headers,
            body: bodyString
        )
    }
}
