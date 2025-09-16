#!/bin/bash

set -e

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Get the script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

print_status "Building .NET Lambda functions..."
print_status "Project root: $PROJECT_ROOT"

# Configuration
CONFIGURATION="Release"
RUNTIME="linux-arm64"
OUTPUT_PATH="bin/Release/net8.0/publish"

print_status "Cleaning previous builds..."
rm -rf "$PROJECT_ROOT/apps/lho-lambda/src/$OUTPUT_PATH"
rm -rf "$PROJECT_ROOT/apps/lho-authorizer/src/$OUTPUT_PATH"
rm -f "$PROJECT_ROOT/lambda.zip"
rm -f "$PROJECT_ROOT/authorizer.zip"

print_status "Building main lambda (lho-lambda)..."
cd "$PROJECT_ROOT/apps/lho-lambda/src"

if ! dotnet restore; then
    print_error "Failed to restore packages for lho-lambda"
    exit 1
fi

if ! dotnet publish \
    --configuration "$CONFIGURATION" \
    --runtime "$RUNTIME" \
    --self-contained false \
    --output "$OUTPUT_PATH"; then
    print_error "Failed to publish lho-lambda"
    exit 1
fi

print_success "Main lambda built successfully"

print_status "Building authorizer lambda (lho-authorizer)..."
cd "$PROJECT_ROOT/apps/lho-authorizer/src"

if ! dotnet restore; then
    print_error "Failed to restore packages for lho-authorizer"
    exit 1
fi

if ! dotnet publish \
    --configuration "$CONFIGURATION" \
    --runtime "$RUNTIME" \
    --self-contained false \
    --output "$OUTPUT_PATH"; then
    print_error "Failed to publish lho-authorizer"
    exit 1
fi

print_success "Authorizer lambda built successfully"

print_status "Verifying build outputs..."

MAIN_LAMBDA_OUTPUT="$PROJECT_ROOT/apps/lho-lambda/src/$OUTPUT_PATH"
AUTHORIZER_OUTPUT="$PROJECT_ROOT/apps/lho-authorizer/src/$OUTPUT_PATH"

if [ ! -d "$MAIN_LAMBDA_OUTPUT" ]; then
    print_error "Main lambda output directory not found: $MAIN_LAMBDA_OUTPUT"
    exit 1
fi

if [ ! -d "$AUTHORIZER_OUTPUT" ]; then
    print_error "Authorizer output directory not found: $AUTHORIZER_OUTPUT"
    exit 1
fi

if [ ! -f "$MAIN_LAMBDA_OUTPUT/lho-lambda.dll" ]; then
    print_error "Main lambda assembly not found: $MAIN_LAMBDA_OUTPUT/lho-lambda.dll"
    exit 1
fi

if [ ! -f "$AUTHORIZER_OUTPUT/lhoAuthorizer.dll" ]; then
    print_error "Authorizer assembly not found: $AUTHORIZER_OUTPUT/lhoAuthorizer.dll"
    exit 1
fi

print_success "All build outputs verified successfully"

print_status "Build Summary:"
echo "  Main Lambda: $MAIN_LAMBDA_OUTPUT"
echo "  Authorizer:  $AUTHORIZER_OUTPUT"
echo ""
print_success "Build completed successfully! You can now run terraform apply."

print_status "Next steps:"
echo "  cd terraform"
echo "  terraform plan"
echo "  terraform apply"
