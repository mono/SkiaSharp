#!/usr/bin/env bash
# Serve the SkiaSharp docs site locally for preview.
#
# Usage:
#   ./scripts/serve-site.sh                    # serve from docs-staging branch
#   ./scripts/serve-site.sh docs-live           # serve from docs-live branch
#
# Requirements: python3

set -euo pipefail

PORT="${PORT:-8080}"
BRANCH="${1:-docs-staging}"
REPO_URL="https://github.com/mono/SkiaSharp.git"

TEMP_DIR=$(mktemp -d)
trap 'rm -rf "$TEMP_DIR"' EXIT

echo "Cloning $BRANCH branch..."
git clone --depth=1 --branch "$BRANCH" "$REPO_URL" "$TEMP_DIR/repo" 2>/dev/null || {
    echo "Error: Branch '$BRANCH' does not exist."
    echo "Available branches: docs-staging, docs-live"
    exit 1
}

# Create SkiaSharp/ symlink to match GitHub Pages path
mkdir -p "$TEMP_DIR/serve"
ln -s "$TEMP_DIR/repo" "$TEMP_DIR/serve/SkiaSharp"

echo "Serving $BRANCH branch"
echo "Open: http://localhost:$PORT/SkiaSharp/"
echo ""
cd "$TEMP_DIR/serve"
python3 -m http.server "$PORT"
