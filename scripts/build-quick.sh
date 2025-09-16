#!/bin/bash

set -e

echo "🔨 Quick build for local development..."

# Get project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Build main lambda
echo "📦 Building lho-lambda..."
cd "$PROJECT_ROOT/apps/lho-lambda/src"
dotnet publish -c Release

# Build authorizer
echo "🔐 Building lho-authorizer..."
cd "$PROJECT_ROOT/apps/lho-authorizer/src"
dotnet publish -c Release

echo "✅ Build complete!"
