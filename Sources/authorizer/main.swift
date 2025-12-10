import AWSLambdaEvents
import AWSLambdaRuntime
import Foundation

// MARK: - Helper Functions

/// Gets a header value from the request headers (case-insensitive)
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

/// Generates an IAM policy response for API Gateway
func generatePolicy(
    principalId: String,
    effect: APIGatewayLambdaAuthorizerPolicyResponse.PolicyDocument.Statement.Effect,
    resource: String
) -> APIGatewayLambdaAuthorizerPolicyResponse {
    return APIGatewayLambdaAuthorizerPolicyResponse(
        principalId: principalId,
        policyDocument: .init(statement: [
            .init(
                action: "execute-api:Invoke",
                effect: effect,
                resource: resource
            )
        ]),
        context: nil
    )
}

// MARK: - Lambda Runtime

let authorizerHandler:
    (APIGatewayLambdaAuthorizerRequest, LambdaContext) async throws ->
        APIGatewayLambdaAuthorizerPolicyResponse = {
            (request: APIGatewayLambdaAuthorizerRequest, context: LambdaContext) in

            let consumer = getHeaderValue(request.headers, key: "x-consumer")
            let validConsumers = ["lhowsam-dev", "lhowsam-prod", "lhowsam-local"]

            let apiKey = getHeaderValue(request.headers, key: "x-api-key")
            let validKey = ProcessInfo.processInfo.environment["API_KEY"]

            let resource = request.routeArn ?? "*"

            if apiKey != validKey {
                context.logger.info("Deny")
                return generatePolicy(
                    principalId: "user",
                    effect: .deny,
                    resource: resource
                )
            }

            if let consumer = consumer, !validConsumers.contains(consumer) {
                context.logger.info("Deny")
                return generatePolicy(
                    principalId: "user",
                    effect: .deny,
                    resource: resource
                )
            }

            return generatePolicy(
                principalId: "user",
                effect: .allow,
                resource: resource
            )
        }

let runtime = LambdaRuntime(body: authorizerHandler)
try await runtime.run()
