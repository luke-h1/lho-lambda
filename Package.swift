// swift-tools-version: 6.1
// The swift-tools-version declares the minimum version of Swift required to build this package.

import PackageDescription

let package = Package(
    name: "lambda",
    platforms: [.macOS(.v15)],
    products: [
        .executable(name: "lambda", targets: ["lambda"]),
        .executable(name: "authorizer", targets: ["authorizer"]),
    ],
    dependencies: [
        // for local dev
        .package(
            url: "https://github.com/swift-server/swift-aws-lambda-runtime.git", from: "2.4.0"),
        .package(url: "https://github.com/awslabs/swift-aws-lambda-events.git", from: "1.0.0"),
        .package(url: "https://github.com/apple/swift-log.git", "1.0.0"..<"1.8.0"),
    ],
    targets: [
        .executableTarget(
            name: "authorizer",
            dependencies: [
                .product(name: "AWSLambdaRuntime", package: "swift-aws-lambda-runtime"),
                .product(name: "AWSLambdaEvents", package: "swift-aws-lambda-events"),

            ]
        ),
        .executableTarget(
            name: "lambda",
            dependencies: [
                .product(name: "AWSLambdaRuntime", package: "swift-aws-lambda-runtime"),
                .product(name: "AWSLambdaEvents", package: "swift-aws-lambda-events"),
            ],
        ),
    ]
)
