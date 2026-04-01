#!/usr/bin/env bash
# Serve the SkiaSharp docs site locally for preview.
#
# Usage:
#   ./scripts/serve-site.sh                    # serve from docs-staging branch
#   ./scripts/serve-site.sh docs-live           # serve from docs-live branch
#
# Requirements: python3
#
# The script caches the clone in a fixed location and reuses it on
# subsequent runs, pulling the latest changes each time.

set -euo pipefail

PORT="${PORT:-8080}"
BRANCH="${1:-docs-staging}"
REPO_URL="https://github.com/mono/SkiaSharp.git"

CACHE_ROOT="${TMPDIR:-/tmp}/skiasharp-site-preview"
REPO_DIR="$CACHE_ROOT/repo"
SERVE_DIR="$CACHE_ROOT/serve"

mkdir -p "$CACHE_ROOT"

if [ -d "$REPO_DIR/.git" ]; then
    # Reuse existing clone — fetch and reset to latest
    echo "Updating cached clone..."
    git -C "$REPO_DIR" fetch origin "$BRANCH" --depth=1 2>/dev/null
    git -C "$REPO_DIR" checkout -B "$BRANCH" "origin/$BRANCH" 2>/dev/null
    git -C "$REPO_DIR" reset --hard "origin/$BRANCH" 2>/dev/null
    git -C "$REPO_DIR" clean -fdx 2>/dev/null
else
    # Fresh clone
    rm -rf "$REPO_DIR"
    echo "Cloning $BRANCH branch..."
    git clone --depth=1 --branch "$BRANCH" "$REPO_URL" "$REPO_DIR" 2>/dev/null || {
        echo "Error: Branch '$BRANCH' does not exist."
        echo "Available branches: docs-staging, docs-live"
        exit 1
    }
fi

SHA=$(git -C "$REPO_DIR" rev-parse --short HEAD)
echo "Serving $BRANCH @ $SHA"

# Create SkiaSharp/ symlink to match GitHub Pages path
mkdir -p "$SERVE_DIR"
[ -L "$SERVE_DIR/SkiaSharp" ] || ln -s "$REPO_DIR" "$SERVE_DIR/SkiaSharp"

echo "Open: http://localhost:$PORT/SkiaSharp/"
echo "Cache: $CACHE_ROOT"
echo ""
cd "$SERVE_DIR"
python3 -m http.server "$PORT"
