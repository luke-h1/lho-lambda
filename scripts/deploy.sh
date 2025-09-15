#!/bin/bash

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m'

print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check arguments
if [ "$#" -ne 1 ]; then
    print_error "Usage: $0 <environment>"
    print_error "Example: $0 staging"
    exit 1
fi

ENVIRONMENT="$1"

if [ "$ENVIRONMENT" != "staging" ] && [ "$ENVIRONMENT" != "live" ]; then
    print_error "Invalid environment. Must be 'staging' or 'live'"
    exit 1
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

print_status "Deploying to $ENVIRONMENT environment..."

# Step 1: Build
print_status "Step 1: Building .NET applications..."
if ! "$SCRIPT_DIR/build.sh"; then
    print_error "Build failed"
    exit 1
fi

# Step 2: Terraform
print_status "Step 2: Deploying with Terraform..."
cd "$PROJECT_ROOT/terraform"

# Initialize terraform
print_status "Initializing Terraform..."
if ! terraform init \
    -backend-config="key=vpc/$ENVIRONMENT.tfstate" \
    -backend-config="bucket=nowplaying-$
