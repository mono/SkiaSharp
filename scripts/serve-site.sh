#!/usr/bin/env bash
# Serve the SkiaSharp docs site locally for preview.
#
# Usage:
#   ./scripts/serve-site.sh                    # serve from docs-staging branch
#   ./scripts/serve-site.sh docs-live           # serve from docs-live branch
#   ./scripts/serve-site.sh --local             # serve from local output/site/ (after build)
#
# Requirements: python3 (for http.server)

set -euo pipefail

PORT="${PORT:-8080}"
REPO_URL="https://github.com/mono/SkiaSharp.git"

if [ "${1:-}" = "--local" ]; then
    SERVE_DIR="$(git rev-parse --show-toplevel)/output/site"
    if [ ! -d "$SERVE_DIR" ]; then
        echo "Error: $SERVE_DIR does not exist."
        echo "Build the site first, or run without --local to serve from a remote branch."
        exit 1
    fi
    echo "Serving local build from: $SERVE_DIR"
    echo "Open: http://localhost:$PORT/SkiaSharp/"
    echo ""

    # Create a SkiaSharp/ subdirectory to match GitHub Pages path
    TEMP_DIR=$(mktemp -d)
    trap 'rm -rf "$TEMP_DIR"' EXIT
    ln -s "$SERVE_DIR" "$TEMP_DIR/SkiaSharp"
    cd "$TEMP_DIR"
    python3 -m http.server "$PORT"
else
    BRANCH="${1:-docs-staging}"
    TEMP_DIR=$(mktemp -d)
    trap 'rm -rf "$TEMP_DIR"' EXIT

    echo "Cloning $BRANCH branch..."
    git clone --depth=1 --branch "$BRANCH" "$REPO_URL" "$TEMP_DIR/repo" 2>/dev/null || {
        echo "Error: Branch '$BRANCH' does not exist."
        echo "Available branches: docs-staging, docs-live"
        exit 1
    }

    # Create SkiaSharp/ subdirectory to match GitHub Pages path
    mkdir -p "$TEMP_DIR/serve"
    ln -s "$TEMP_DIR/repo" "$TEMP_DIR/serve/SkiaSharp"

    echo "Serving $BRANCH branch"
    echo "Open: http://localhost:$PORT/SkiaSharp/"
    echo ""
    cd "$TEMP_DIR/serve"
    python3 -m http.server "$PORT"
fi
