import AWSLambdaEvents
import AWSLambdaRuntime
import Foundation

// MARK: - Lambda Runtime

let router = createRouter()

let runtime = LambdaRuntime {
    (event: APIGatewayV2Request, context: LambdaContext) -> APIGatewayV2Response in

    return try await router.handle(event: event, context: context)
}

try await runtime.run()
