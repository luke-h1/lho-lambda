import AWSLambdaEvents
import AWSLambdaRuntime
import Foundation
import Logging

struct VersionResponse: Codable {
    let version: String
    let deployedAt: String
    let deployedBy: String
    let gitSha: String
}

actor VersionService {
    func handleVersion() async throws -> APIGatewayV2Response {
        let response = VersionResponse(
            version: Environment.Deploy.version,
            deployedAt: Environment.Deploy.deployedAt,
            deployedBy: Environment.Deploy.deployedBy,
            gitSha: Environment.Deploy.gitSha
        )
        return try ResponseBuilder.createResponse(body: response, includeCacheControl: false)
    }
}
