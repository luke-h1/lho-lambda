#!/bin/bash

set -e

# Colors for output
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

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

CHANGELOG_FILE="$PROJECT_ROOT/CHANGELOG.md"
LAMBDA_CHANGELOG="$PROJECT_ROOT/apps/lho-lambda/CHANGELOG.md"
AUTHORIZER_CHANGELOG="$PROJECT_ROOT/apps/lho-authorizer/CHANGELOG.md"

# Check if this is a pre-release
is_prerelease() {
    # Check if this is a PR or non-main branch (same logic as version.sh)
    if [ "${GITHUB_EVENT_NAME:-}" = "pull_request" ] || [ "${GITHUB_REF_NAME:-$(git branch --show-current)}" != "main" ]; then
        return 0  # true - is pre-release
    else
        return 1  # false - is not pre-release
    fi
}

# Get current version from git tags
CURRENT_VERSION=$(git describe --tags --abbrev=0 2>/dev/null || echo "v0.0.0")
print_status "Current version: $CURRENT_VERSION"

# Generate new version based on conventional commits
# Check if there are any breaking changes
BREAKING_CHANGES=$(git log ${CURRENT_VERSION}..HEAD --pretty=format:"%s" | grep -E "BREAKING CHANGE|!" | wc -l || echo "0")
FEATURES=$(git log ${CURRENT_VERSION}..HEAD --pretty=format:"%s" | grep -E "^feat" | wc -l || echo "0")
FIXES=$(git log ${CURRENT_VERSION}..HEAD --pretty=format:"%s" | grep -E "^fix" | wc -l || echo "0")

CURRENT_VERSION_CLEAN=$(echo $CURRENT_VERSION | sed 's/v//')
IFS='.' read -ra VERSION_PARTS <<< "$CURRENT_VERSION_CLEAN"
MAJOR=${VERSION_PARTS[0]:-0}
MINOR=${VERSION_PARTS[1]:-0}
PATCH=${VERSION_PARTS[2]:-0}

if [ "$BREAKING_CHANGES" -gt 0 ]; then
    MAJOR=$((MAJOR + 1))
    MINOR=0
    PATCH=0
    RELEASE_TYPE="major"
elif [ "$FEATURES" -gt 0 ]; then
    MINOR=$((MINOR + 1))
    PATCH=0
    RELEASE_TYPE="minor"
elif [ "$FIXES" -gt 0 ]; then
    PATCH=$((PATCH + 1))
    RELEASE_TYPE="patch"
else
    print_warning "No conventional commits found. Using patch version bump."
    PATCH=$((PATCH + 1))
    RELEASE_TYPE="patch"
fi

NEW_VERSION="$MAJOR.$MINOR.$PATCH"
NEW_VERSION_TAG="v$NEW_VERSION"

print_status "New version will be: $NEW_VERSION_TAG ($RELEASE_TYPE)"

# Generate changelog for root
print_status "Generating root changelog..."
bunx conventional-changelog -p angular -i "$CHANGELOG_FILE" -s -r 0

# Generate changelog for lambda app
print_status "Generating lambda changelog..."
bunx conventional-changelog -p angular -i "$LAMBDA_CHANGELOG" -s -r 0 \
    --commit-path="apps/lho-lambda" \
    --pkg="$PROJECT_ROOT/apps/lho-lambda/src/lho-lambda.csproj"

# Generate changelog for authorizer app
print_status "Generating authorizer changelog..."
bunx conventional-changelog -p angular -i "$AUTHORIZER_CHANGELOG" -s -r 0 \
    --commit-path="apps/lho-authorizer" \
    --pkg="$PROJECT_ROOT/apps/lho-authorizer/src/lhoAuthorizer.csproj"

# Create git tag only if not pre-release and --tag flag is provided
if [ "${1:-}" = "--tag" ]; then
    if is_prerelease; then
        print_warning "Skipping git tag creation for pre-release version"
        print_status "Pre-release detected: GITHUB_EVENT_NAME=${GITHUB_EVENT_NAME:-}, branch=$(git branch --show-current)"
    else
        print_status "Creating git tag: $NEW_VERSION_TAG"
        git add .
        git commit -m "chore(release): $NEW_VERSION_TAG" || true
        git tag -a "$NEW_VERSION_TAG" -m "Release $NEW_VERSION_TAG"
        print_success "Tagged version $NEW_VERSION_TAG"
    fi
else
    print_status "Skipping git tag creation (--tag not provided)"
fi

# Export version for CI/CD
echo "NEW_VERSION=$NEW_VERSION" >> "$GITHUB_ENV" 2>/dev/null || true
echo "NEW_VERSION_TAG=$NEW_VERSION_TAG" >> "$GITHUB_ENV" 2>/dev/null || true
echo "RELEASE_TYPE=$RELEASE_TYPE" >> "$GITHUB_ENV" 2>/dev/null || true

print_success "Changelog generation completed!"
print_status "Summary:"
echo "  Current Version: $CURRENT_VERSION"
echo "  New Version: $NEW_VERSION_TAG"
echo "  Release Type: $RELEASE_TYPE"
echo "  Breaking Changes: $BREAKING_CHANGES"
echo "  Features: $FEATURES"
echo "  Fixes: $FIXES"
if is_prerelease; then
    echo "  Pre-release: Yes"
else
    echo "  Pre-release: No"
fi
