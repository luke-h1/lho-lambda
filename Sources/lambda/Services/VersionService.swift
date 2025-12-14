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
        let version = ProcessInfo.processInfo.environment["VERSION"] ?? "unknown"
        let deployedAt = ProcessInfo.processInfo.environment["DEPLOYED_AT"] ?? "unknown"
        let deployedBy = ProcessInfo.processInfo.environment["DEPLOYED_BY"] ?? "unknown"
        let gitSha = ProcessInfo.processInfo.environment["GIT_SHA"] ?? "unknown"

        let response = VersionResponse(
            version: version, deployedAt: deployedAt, deployedBy: deployedBy, gitSha: gitSha
        )

        return ResponseBuilder.createResponse(body: response, includeCacheControl: false)
    }
}
