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

# Get the script directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Function to get version from git tag
get_version_from_git() {
    git describe --tags --exact-match 2>/dev/null || git describe --tags --abbrev=0 2>/dev/null || echo "v0.0.0"
}

# Function to get version without 'v' prefix
get_clean_version() {
    local version=$(get_version_from_git)
    local clean_version="${version#v}"
    
    # Add pre-release suffix if this is a PR or non-main branch
    if [ "${GITHUB_EVENT_NAME:-}" = "pull_request" ] || [ "${GITHUB_REF_NAME:-$(git branch --show-current)}" != "main" ]; then
        local git_sha_short=$(get_git_sha_short)
        echo "${clean_version}-pre.${git_sha_short}"
    else
        echo "${clean_version}"
    fi
}

# Function to get git commit SHA
get_git_sha() {
    git rev-parse HEAD 2>/dev/null || echo "unknown"
}

# Function to get short git SHA
get_git_sha_short() {
    git rev-parse --short HEAD 2>/dev/null || echo "unknown"
}

# Function to update .NET project version
update_dotnet_version() {
    local project_file="$1"
    local version="$2"
    
    if [ ! -f "$project_file" ]; then
        print_error "Project file not found: $project_file"
        return 1
    fi
    
    print_status "Updating version in $project_file to $version"
    
    # Update or add Version property
    if grep -q "<Version>" "$project_file"; then
        sed -i.bak "s|<Version>.*</Version>|<Version>$version</Version>|" "$project_file"
    else
        # Add Version property to PropertyGroup
        sed -i.bak "/<PropertyGroup>/a\\
    <Version>$version</Version>" "$project_file"
    fi
    
    # Clean up backup file
    rm -f "$project_file.bak"
    
    print_success "Updated version in $project_file"
}

# Main execution
case "${1:-}" in
    "get")
        get_clean_version
        ;;
    "get-tag")
        get_version_from_git
        ;;
    "get-sha")
        get_git_sha
        ;;
    "get-sha-short")
        get_git_sha_short
        ;;
    "update")
        VERSION=$(get_clean_version)
        print_status "Updating .NET project versions to $VERSION"
        
        # Update main lambda project
        update_dotnet_version "$PROJECT_ROOT/apps/lho-lambda/src/lho-lambda.csproj" "$VERSION"
        
        # Update authorizer project
        update_dotnet_version "$PROJECT_ROOT/apps/lho-authorizer/src/lhoAuthorizer.csproj" "$VERSION"
        
        print_success "All project versions updated to $VERSION"
        ;;
    "info")
        VERSION=$(get_clean_version)
        VERSION_TAG=$(get_version_from_git)
        GIT_SHA=$(get_git_sha)
        GIT_SHA_SHORT=$(get_git_sha_short)
        
        print_status "Version Information:"
        echo "  Clean Version: $VERSION"
        echo "  Version Tag: $VERSION_TAG"
        echo "  Git SHA: $GIT_SHA"
        echo "  Git SHA Short: $GIT_SHA_SHORT"
        
        # Export for CI/CD
        echo "VERSION=$VERSION" >> "$GITHUB_ENV" 2>/dev/null || true
        echo "VERSION_TAG=$VERSION_TAG" >> "$GITHUB_ENV" 2>/dev/null || true
        echo "GIT_SHA=$GIT_SHA" >> "$GITHUB_ENV" 2>/dev/null || true
        echo "GIT_SHA_SHORT=$GIT_SHA_SHORT" >> "$GITHUB_ENV" 2>/dev/null || true
        ;;
    *)
        print_error "Usage: $0 {get|get-tag|get-sha|get-sha-short|update|info}"
        echo ""
        echo "Commands:"
        echo "  get           - Get clean version (without 'v' prefix)"
        echo "  get-tag       - Get version with 'v' prefix"
        echo "  get-sha       - Get full git SHA"
        echo "  get-sha-short - Get short git SHA"
        echo "  update        - Update .NET project files with current version"
        echo "  info          - Show all version information and export to env"
        exit 1
        ;;
esac
