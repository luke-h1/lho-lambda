#!/bin/bash

# Usage: ./scripts/test-simple.sh [path] [method]
# Example: ./scripts/test-simple.sh /api/healthcheck GET
# Example: ./scripts/test-simple.sh /api/now-playing GET

PATH_TO_TEST="${1:-/}"
METHOD="${2:-GET}"

echo "Testing $METHOD $PATH_TO_TEST"
echo ""

curl -X POST http://localhost:7000/invoke \
  -H "Content-Type: application/json" \
  -d "{
    \"version\": \"2.0\",
    \"routeKey\": \"$METHOD $PATH_TO_TEST\",
    \"rawPath\": \"$PATH_TO_TEST\",
    \"rawQueryString\": \"\",
    \"headers\": {},
    \"isBase64Encoded\": false,
    \"requestContext\": {
      \"accountId\": \"123456789012\",
      \"apiId\": \"test-api-id\",
      \"domainName\": \"localhost\",
      \"domainPrefix\": \"test\",
      \"http\": {
        \"method\": \"$METHOD\",
        \"path\": \"$PATH_TO_TEST\",
        \"protocol\": \"HTTP/1.1\",
        \"sourceIp\": \"127.0.0.1\",
        \"userAgent\": \"curl/test\"
      },
      \"requestId\": \"test-request-id\",
      \"routeKey\": \"$METHOD $PATH_TO_TEST\",
      \"stage\": \"\$default\",
      \"time\": \"09/Dec/2024:00:00:00 +0000\",
      \"timeEpoch\": 1702080000000
    }
  }"

echo ""
echo ""

