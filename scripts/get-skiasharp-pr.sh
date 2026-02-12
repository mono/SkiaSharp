#!/usr/bin/env bash

# get-skiasharp-pr.sh - Download SkiaSharp NuGet packages from a specific PR's build artifacts
# Usage: ./get-skiasharp-pr.sh PR_NUMBER [OPTIONS]

set -euo pipefail

# Constants
readonly ORGANIZATION="xamarin"
readonly PROJECT="public"
readonly PIPELINE_ID=4
readonly PREFERRED_ARTIFACT="nuget_preview"
readonly FALLBACK_ARTIFACT="nuget"
readonly BASE_URL="https://dev.azure.com/${ORGANIZATION}/${PROJECT}/_apis"

# Colors
readonly RED='\033[0;31m'
readonly GREEN='\033[0;32m'
readonly YELLOW='\033[1;33m'
readonly CYAN='\033[0;36m'
readonly RESET='\033[0m'

# Variables
INSTALL_PATH=""
PR_NUMBER=""
BUILD_ID=""
SUCCESSFUL_ONLY=false
FORCE=false
LIST_ONLY=false
VERBOSE=false
DRY_RUN=false

show_help() {
    cat << 'EOF'
SkiaSharp PR Package Download Script

DESCRIPTION:
    Downloads SkiaSharp NuGet packages from a specific pull request's build artifacts.
    Uses the Azure DevOps REST API (no authentication required for public builds).

    Packages are installed to ~/.skiasharp/hives/pr-{PRNumber}/packages/ by default.

USAGE:
    ./get-skiasharp-pr.sh PR_NUMBER [OPTIONS]

    PR_NUMBER                   Pull request number (required)

OPTIONS:
    -b, --build-id ID           Use specific build ID instead of finding latest
    -i, --install-path PATH     Directory prefix (default: ~/.skiasharp)
                                Packages install to: <path>/hives/pr-<PR>/packages/
    -s, --successful-only       Only consider successful builds
    -f, --force                 Overwrite existing packages
    -l, --list                  List available artifacts without downloading
    -v, --verbose               Enable verbose output
    --dry-run                   Show what would be done without performing actions
    -h, --help                  Show this help message

EXAMPLES:
    ./get-skiasharp-pr.sh 1234
    ./get-skiasharp-pr.sh 1234 --successful-only
    ./get-skiasharp-pr.sh 1234 --build-id 155745
    ./get-skiasharp-pr.sh 1234 --install-path ~/my-skiasharp
    ./get-skiasharp-pr.sh 1234 --list

    # One-liner (no repo clone needed):
    curl -fsSL https://raw.githubusercontent.com/mono/SkiaSharp/main/scripts/get-skiasharp-pr.sh | bash -s -- 1234

    # Then add as NuGet source:
    dotnet nuget add source ~/.skiasharp/hives/pr-1234/packages --name skiasharp-pr-1234

REQUIREMENTS:
    - curl (for API calls and downloads)
    - unzip (for extracting artifacts)
    - jq (for JSON parsing)

EOF
}

# Logging functions
say_verbose() {
    if [[ "$VERBOSE" == true ]]; then
        echo -e "${CYAN}[VERBOSE]${RESET} $1" >&2
    fi
}

say_error() {
    echo -e "${RED}Error: $1${RESET}" >&2
}

say_warn() {
    echo -e "${YELLOW}Warning: $1${RESET}" >&2
}

say_info() {
    echo -e "$1" >&2
}

say_success() {
    echo -e "${GREEN}$1${RESET}" >&2
}

# Check dependencies
check_dependencies() {
    local missing=()
    
    if ! command -v curl >/dev/null 2>&1; then
        missing+=("curl")
    fi
    
    if ! command -v unzip >/dev/null 2>&1; then
        missing+=("unzip")
    fi
    
    if ! command -v jq >/dev/null 2>&1; then
        missing+=("jq")
    fi
    
    if [[ ${#missing[@]} -gt 0 ]]; then
        say_error "Missing required dependencies: ${missing[*]}"
        say_info "Please install them and try again."
        exit 1
    fi
    
    say_verbose "All dependencies found: curl, unzip, jq"
}

# Parse command line arguments
parse_args() {
    for arg in "$@"; do
        if [[ "$arg" == "-h" || "$arg" == "--help" ]]; then
            show_help
            exit 0
        fi
    done

    if [[ $# -lt 1 ]]; then
        say_error "PR number is required."
        say_info "Use --help for usage information."
        exit 1
    fi

    if [[ "$1" == --* ]]; then
        say_error "First argument must be a PR number, not an option. Got: '$1'"
        say_info "Use --help for usage information."
        exit 1
    fi

    if [[ "$1" =~ ^[1-9][0-9]*$ ]]; then
        PR_NUMBER="$1"
        shift
    else
        say_error "First argument must be a valid PR number (positive integer)"
        exit 1
    fi

    while [[ $# -gt 0 ]]; do
        case $1 in
            -b|--build-id)
                if [[ $# -lt 2 || -z "$2" ]]; then
                    say_error "Option '$1' requires a value"
                    exit 1
                fi
                if [[ ! "$2" =~ ^[0-9]+$ ]]; then
                    say_error "Build ID must be a number. Got: '$2'"
                    exit 1
                fi
                BUILD_ID="$2"
                shift 2
                ;;
            -i|--install-path)
                if [[ $# -lt 2 || -z "$2" ]]; then
                    say_error "Option '$1' requires a value"
                    exit 1
                fi
                INSTALL_PATH="$2"
                shift 2
                ;;
            -s|--successful-only)
                SUCCESSFUL_ONLY=true
                shift
                ;;
            -f|--force)
                FORCE=true
                shift
                ;;
            -l|--list)
                LIST_ONLY=true
                shift
                ;;
            -v|--verbose)
                VERBOSE=true
                shift
                ;;
            --dry-run)
                DRY_RUN=true
                shift
                ;;
            *)
                say_error "Unknown option '$1'"
                say_info "Use --help for usage information."
                exit 1
                ;;
        esac
    done

    # Set default install path
    if [[ -z "$INSTALL_PATH" ]]; then
        INSTALL_PATH="${HOME}/.skiasharp"
    fi
}

# Make Azure DevOps API call
azdo_api_call() {
    local endpoint="$1"
    local error_message="${2:-Failed to call Azure DevOps API}"
    
    local url="${BASE_URL}/${endpoint}"
    say_verbose "API call: $url"
    
    local response
    if ! response=$(curl -fsSL "$url" 2>&1); then
        say_error "$error_message: $response"
        return 1
    fi
    
    printf "%s" "$response"
}

# Find build for PR
find_build_for_pr() {
    local pr_number="$1"
    local successful_only="$2"
    
    local source_branch="refs/pull/${pr_number}/merge"
    
    if [[ "$successful_only" == true ]]; then
        say_info "Finding latest successful build for PR #${pr_number}..."
        local endpoint="build/builds?api-version=7.1&definitions=${PIPELINE_ID}&reasonFilter=pullRequest&statusFilter=completed&resultFilter=succeeded&\$top=20"
    else
        say_info "Finding latest build for PR #${pr_number}..."
        local endpoint="build/builds?api-version=7.1&definitions=${PIPELINE_ID}&reasonFilter=pullRequest&\$top=50"
    fi
    
    local response
    if ! response=$(azdo_api_call "$endpoint" "Failed to query builds"); then
        return 1
    fi
    
    # Filter to builds for this specific PR and get the latest
    local build
    build=$(echo "$response" | jq -r --arg branch "$source_branch" '
        .value 
        | map(select(.sourceBranch == $branch)) 
        | sort_by(.queueTime) 
        | reverse 
        | .[0] // empty
    ')
    
    if [[ -z "$build" || "$build" == "null" ]]; then
        if [[ "$successful_only" == true ]]; then
            say_error "No successful builds found for PR #${pr_number}"
            say_info "Check: https://dev.azure.com/xamarin/public/_build?definitionId=${PIPELINE_ID}"
        else
            say_error "No builds found for PR #${pr_number}"
            say_info "Check if the PR exists at: https://dev.azure.com/xamarin/public/_build?definitionId=${PIPELINE_ID}"
        fi
        return 1
    fi
    
    local build_id build_number status result finish_time start_time url
    build_id=$(echo "$build" | jq -r '.id')
    build_number=$(echo "$build" | jq -r '.buildNumber')
    status=$(echo "$build" | jq -r '.status')
    result=$(echo "$build" | jq -r '.result // "in progress"')
    finish_time=$(echo "$build" | jq -r '.finishTime // empty')
    start_time=$(echo "$build" | jq -r '.startTime // empty')
    url=$(echo "$build" | jq -r '._links.web.href')
    
    if [[ "$status" == "completed" && "$result" == "succeeded" ]]; then
        say_success "Found successful build: ${build_number} (ID: ${build_id})"
        say_info "  Finished: ${finish_time}"
    elif [[ "$status" == "completed" ]]; then
        say_warn "Found completed build (result: ${result}): ${build_number} (ID: ${build_id})"
        say_info "  Finished: ${finish_time}"
        say_warn "  Note: Build did not succeed - artifacts may be incomplete"
    else
        say_warn "Found in-progress build: ${build_number} (ID: ${build_id})"
        say_info "  Started: ${start_time}"
        say_warn "  Note: Build still running - artifacts may not be available yet"
    fi
    
    say_info "  URL: ${url}"
    
    printf "%s" "$build_id"
}

# Get artifacts for build
get_build_artifacts() {
    local build_id="$1"
    
    say_verbose "Getting artifacts for build ${build_id}..."
    
    local endpoint="build/builds/${build_id}/artifacts?api-version=7.1"
    local response
    if ! response=$(azdo_api_call "$endpoint" "Failed to get artifacts"); then
        return 1
    fi
    
    printf "%s" "$response"
}

# Download artifact
download_artifact() {
    local artifact_url="$1"
    local output_path="$2"
    
    say_info "Downloading artifact..."
    say_verbose "URL: ${artifact_url}"
    say_verbose "Output: ${output_path}"
    
    if [[ "$DRY_RUN" == true ]]; then
        say_info "[DRY RUN] Would download artifact to ${output_path}"
        return 0
    fi
    
    # Create parent directory
    mkdir -p "$(dirname "$output_path")"
    
    # Download with progress
    if ! curl -fSL --progress-bar "$artifact_url" -o "$output_path"; then
        say_error "Failed to download artifact"
        return 1
    fi
    
    say_success "Download complete: $(du -h "$output_path" | cut -f1)"
}

# Extract packages from artifact
extract_packages() {
    local zip_path="$1"
    local dest_path="$2"
    local filter="$3"
    local force="$4"
    
    say_info "Extracting packages matching '${filter}'..."
    
    if [[ "$DRY_RUN" == true ]]; then
        say_info "[DRY RUN] Would extract packages to ${dest_path}"
        return 0
    fi
    
    # Create temp directory for extraction
    local temp_dir
    temp_dir=$(mktemp -d)
    
    # Extract zip
    if ! unzip -q "$zip_path" -d "$temp_dir"; then
        say_error "Failed to extract artifact"
        rm -rf "$temp_dir"
        return 1
    fi
    
    # Create destination directory
    mkdir -p "$dest_path"
    
    # Find and copy matching packages
    local count=0
    local skipped=0
    
    while IFS= read -r -d '' nupkg; do
        local filename
        filename=$(basename "$nupkg")
        local dest_file="${dest_path}/${filename}"
        
        if [[ -f "$dest_file" && "$force" != true ]]; then
            say_verbose "Skipping (exists): ${filename}"
            ((skipped++))
        else
            cp "$nupkg" "$dest_file"
            say_verbose "Extracted: ${filename}"
            ((count++))
        fi
    done < <(find "$temp_dir" -name "$filter" -print0 2>/dev/null)
    
    # Cleanup
    rm -rf "$temp_dir"
    
    if [[ $count -eq 0 && $skipped -eq 0 ]]; then
        say_warn "No packages found matching '${filter}'"
        return 1
    fi
    
    say_success "Extracted ${count} packages (skipped ${skipped} existing)"
}

# Main function
main() {
    parse_args "$@"
    check_dependencies
    
    echo ""
    echo -e "${CYAN}SkiaSharp PR Package Downloader${RESET}"
    echo -e "${CYAN}================================${RESET}"
    echo ""
    
    # Find or use provided build ID
    local build_id
    if [[ -n "$BUILD_ID" ]]; then
        say_info "Using provided build ID: ${BUILD_ID}"
        build_id="$BUILD_ID"
    else
        if ! build_id=$(find_build_for_pr "$PR_NUMBER" "$SUCCESSFUL_ONLY"); then
            exit 1
        fi
    fi
    
    # Get artifacts
    local artifacts
    if ! artifacts=$(get_build_artifacts "$build_id"); then
        exit 1
    fi
    
    local artifact_count
    artifact_count=$(echo "$artifacts" | jq '.value | length')
    
    if [[ "$artifact_count" -eq 0 ]]; then
        if [[ "$SUCCESSFUL_ONLY" != true ]]; then
            say_error "No artifacts found for build ${build_id}"
            say_info "The build may still be creating artifacts. Try again later, or use --successful-only to get the last successful build."
        else
            say_error "No artifacts found for build ${build_id}"
        fi
        exit 1
    fi
    
    # List mode
    if [[ "$LIST_ONLY" == true ]]; then
        echo ""
        say_info "Available artifacts:"
        echo "$artifacts" | jq -r '.value[].name' | while read -r name; do
            say_info "  - ${name}"
        done
        exit 0
    fi
    
    # Find nuget artifact (prefer nuget_preview)
    local artifact_name download_url using_preview
    
    download_url=$(echo "$artifacts" | jq -r --arg name "$PREFERRED_ARTIFACT" '.value[] | select(.name == $name) | .resource.downloadUrl // empty')
    
    if [[ -n "$download_url" ]]; then
        artifact_name="$PREFERRED_ARTIFACT"
        using_preview=true
    else
        say_verbose "nuget_preview artifact not found, trying full nuget artifact..."
        download_url=$(echo "$artifacts" | jq -r --arg name "$FALLBACK_ARTIFACT" '.value[] | select(.name == $name) | .resource.downloadUrl // empty')
        artifact_name="$FALLBACK_ARTIFACT"
        using_preview=false
    fi
    
    if [[ -z "$download_url" ]]; then
        say_warn "Available artifacts:"
        echo "$artifacts" | jq -r '.value[].name' | while read -r name; do
            say_info "  - ${name}"
        done
        if [[ "$SUCCESSFUL_ONLY" != true ]]; then
            say_error "Neither '${PREFERRED_ARTIFACT}' nor '${FALLBACK_ARTIFACT}' artifact found."
            say_info "The build may still be in progress. Try --successful-only to get the last successful build."
        else
            say_error "Neither '${PREFERRED_ARTIFACT}' nor '${FALLBACK_ARTIFACT}' artifact found."
        fi
        exit 1
    fi
    
    say_info "Using artifact: ${artifact_name}"
    
    # Setup paths
    local packages_path="${INSTALL_PATH}/hives/pr-${PR_NUMBER}/packages"
    local temp_dir
    temp_dir=$(mktemp -d)
    local zip_path="${temp_dir}/artifact.zip"
    
    echo ""
    say_info "Installing to: ${packages_path}"
    echo ""
    
    # Download
    if ! download_artifact "$download_url" "$zip_path"; then
        rm -rf "$temp_dir"
        exit 1
    fi
    
    # Extract (nuget_preview already has only prerelease; for full nuget, filter to prerelease)
    local filter
    if [[ "$using_preview" == true ]]; then
        filter="*.nupkg"
    else
        filter="*-*.nupkg"
    fi
    
    if ! extract_packages "$zip_path" "$packages_path" "$filter" "$FORCE"; then
        rm -rf "$temp_dir"
        exit 1
    fi
    
    # Cleanup
    rm -rf "$temp_dir"
    
    echo ""
    say_success "Done!"
    echo ""
    say_info "To use these packages, add as a NuGet source:"
    say_info "  dotnet nuget add source ${packages_path} --name skiasharp-pr-${PR_NUMBER}"
    echo ""
}

main "$@"
