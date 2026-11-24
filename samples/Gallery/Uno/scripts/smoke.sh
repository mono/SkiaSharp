#!/usr/bin/env bash
#
# Headless smoke test for the Uno Platform WASM gallery sample.
# See specs/001-uno-wasm-sample/contracts/smoke-test.md for the full contract.
#
# Usage:
#   bash samples/Gallery/Uno/scripts/smoke.sh <publish-wwwroot-dir> [--port N] [--screenshot PATH]
#
# Checks, in order:
#   1. Inputs present (index.html, _framework/, uno-config.js, package_*/)
#   2. Static server (python3 http.server) serves the app under a sub-path
#      (/smoke-prefix/), so any root-absolute asset reference fails the check
#   3. Headless Chromium (bundled via Playwright in this script's node_modules)
#      boots the page without unignored console errors
#   4. DOM contains the "SkiaSharp Gallery" marker
#
# Exits 0 on success, non-zero on any failure. On exit (including error) the
# background static server is torn down.
#
# First-run cost: installs Playwright + Chromium into samples/Gallery/Uno/scripts/node_modules
# and $PLAYWRIGHT_BROWSERS_PATH (default ~/.cache/ms-playwright). Subsequent runs reuse.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
DRIVER="$SCRIPT_DIR/smoke-driver.mjs"

WWWROOT="${1:-}"
PORT=""
SCREENSHOT=""
if [[ -n "${1:-}" ]]; then shift; fi
while [[ $# -gt 0 ]]; do
    case "$1" in
        --port) PORT="$2"; shift 2;;
        --screenshot) SCREENSHOT="$2"; shift 2;;
        *) echo "smoke.sh: unknown arg: $1" >&2; exit 2;;
    esac
done

if [[ -z "$WWWROOT" ]]; then
    echo "smoke.sh: missing argument <publish-wwwroot-dir>" >&2
    exit 2
fi
if [[ ! -d "$WWWROOT" ]]; then
    echo "smoke.sh: not a directory: $WWWROOT" >&2
    exit 1
fi

# Check 1 — inputs present
# Uno.Wasm.Bootstrap 10 (Uno.Sdk 7) writes uno-config.js at the wwwroot root,
# next to index.html; package_<hash>/ holds the bootstrapper scripts and assets.
for path in \
    "$WWWROOT/index.html" \
    "$WWWROOT/_framework" \
    "$WWWROOT/uno-config.js" \
; do
    [[ -e "$path" ]] || { echo "smoke.sh: expected path missing: $path" >&2; exit 1; }
done
if ! compgen -G "$WWWROOT/package_*/" >/dev/null; then
    echo "smoke.sh: expected path missing: $WWWROOT/package_*/" >&2
    exit 1
fi

# Dependencies
command -v python3 >/dev/null 2>&1 || { echo "smoke.sh: python3 not found" >&2; exit 1; }
command -v node    >/dev/null 2>&1 || { echo "smoke.sh: node not found" >&2; exit 1; }
command -v npm     >/dev/null 2>&1 || { echo "smoke.sh: npm not found" >&2; exit 1; }

# Ensure Playwright matches package.json
(cd "$SCRIPT_DIR" && npm install --silent --no-audit --no-fund) >&2

# Ensure the matching Chromium binary is present
export PLAYWRIGHT_BROWSERS_PATH="${PLAYWRIGHT_BROWSERS_PATH:-$HOME/.cache/ms-playwright}"
(cd "$SCRIPT_DIR" && node_modules/.bin/playwright install chromium) >&2

# Pick a free port if not provided
if [[ -z "$PORT" ]]; then
    PORT=5050
    while (exec 6<>/dev/tcp/127.0.0.1/"$PORT") 2>/dev/null; do
        exec 6>&- 6<&- 2>/dev/null
        PORT=$((PORT + 1))
        if [[ $PORT -gt 5150 ]]; then
            echo "smoke.sh: no free port in 5050..5150" >&2
            exit 1
        fi
    done
fi

# Register cleanup before creating anything it removes, so every exit path
# (including a failure while preparing the serve root) tears it down.
cleanup() {
    if [[ -n "${SERVER_PID:-}" ]] && kill -0 "$SERVER_PID" 2>/dev/null; then
        kill "$SERVER_PID" 2>/dev/null || true
        wait "$SERVER_PID" 2>/dev/null || true
    fi
    [[ -f "${SERVER_LOG:-}" ]] && rm -f "$SERVER_LOG"
    [[ -d "${SERVE_ROOT:-}" ]] && rm -rf "$SERVE_ROOT"
}
trap cleanup EXIT

# Start the static server. Serve the app from a sub-path, as the site does
# (/SkiaSharp/gallery-uno/, /SkiaSharp/staging/<PR>/...), so a root-absolute
# asset reference 404s here instead of only after deployment.
SERVE_ROOT="$(mktemp -d)"
SERVE_PREFIX="smoke-prefix"
ln -s "$(cd "$WWWROOT" && pwd)" "$SERVE_ROOT/$SERVE_PREFIX"
SERVER_LOG="$(mktemp)"
(cd "$SERVE_ROOT" && python3 -m http.server "$PORT" --bind 127.0.0.1) \
    >"$SERVER_LOG" 2>&1 &
SERVER_PID=$!

# Wait for server readiness (up to 10s)
for _ in $(seq 1 40); do
    if curl -sf -o /dev/null "http://127.0.0.1:$PORT/$SERVE_PREFIX/"; then
        break
    fi
    sleep 0.25
done
if ! curl -sf -o /dev/null "http://127.0.0.1:$PORT/$SERVE_PREFIX/"; then
    echo "smoke.sh: static server did not come up on port $PORT" >&2
    echo "--- server log: ---" >&2
    cat "$SERVER_LOG" >&2 || true
    exit 1
fi

# Run the browser check
SMOKE_URL="http://127.0.0.1:$PORT/$SERVE_PREFIX/" \
SMOKE_MARKER="${SMOKE_MARKER:-SkiaSharp Gallery}" \
SMOKE_SCREENSHOT="${SCREENSHOT:-}" \
    node --experimental-vm-modules "$DRIVER"
