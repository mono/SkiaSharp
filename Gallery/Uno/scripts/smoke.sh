#!/usr/bin/env bash
#
# Headless smoke test for the Uno Platform WASM gallery sample.
# See specs/001-uno-wasm-sample/contracts/smoke-test.md for the full contract.
#
# Usage:
#   bash samples/Gallery/Uno/scripts/smoke.sh <publish-wwwroot-dir> [--port N] [--screenshot PATH]
#
# Checks, in order:
#   1. Inputs present (index.html, _framework/, package_*/uno-config.js)
#   2. Static server (python3 http.server) serves 200 on /
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
for path in \
    "$WWWROOT/index.html" \
    "$WWWROOT/_framework" \
; do
    [[ -e "$path" ]] || { echo "smoke.sh: expected path missing: $path" >&2; exit 1; }
done
if ! compgen -G "$WWWROOT/package_*/uno-config.js" >/dev/null; then
    echo "smoke.sh: expected path missing: $WWWROOT/package_*/uno-config.js" >&2
    exit 1
fi

# Dependencies
command -v python3 >/dev/null 2>&1 || { echo "smoke.sh: python3 not found" >&2; exit 1; }
command -v node    >/dev/null 2>&1 || { echo "smoke.sh: node not found" >&2; exit 1; }
command -v npm     >/dev/null 2>&1 || { echo "smoke.sh: npm not found" >&2; exit 1; }

# Ensure Playwright is installed in scripts/node_modules
if [[ ! -d "$SCRIPT_DIR/node_modules/playwright" ]]; then
    echo "smoke.sh: installing Playwright (first run) ..." >&2
    (cd "$SCRIPT_DIR" && npm install --silent --no-audit --no-fund) >&2
fi

# Ensure Chromium binary is present
export PLAYWRIGHT_BROWSERS_PATH="${PLAYWRIGHT_BROWSERS_PATH:-$HOME/.cache/ms-playwright}"
if ! ls "$PLAYWRIGHT_BROWSERS_PATH"/chromium-* >/dev/null 2>&1; then
    echo "smoke.sh: installing Playwright Chromium (first run) ..." >&2
    (cd "$SCRIPT_DIR" && node_modules/.bin/playwright install chromium) >&2
fi

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

# Start the static server
SERVER_LOG="$(mktemp)"
(cd "$WWWROOT" && python3 -m http.server "$PORT" --bind 127.0.0.1) \
    >"$SERVER_LOG" 2>&1 &
SERVER_PID=$!

cleanup() {
    if [[ -n "${SERVER_PID:-}" ]] && kill -0 "$SERVER_PID" 2>/dev/null; then
        kill "$SERVER_PID" 2>/dev/null || true
        wait "$SERVER_PID" 2>/dev/null || true
    fi
    [[ -f "${SERVER_LOG:-}" ]] && rm -f "$SERVER_LOG"
}
trap cleanup EXIT

# Wait for server readiness (up to 10s)
for _ in $(seq 1 40); do
    if curl -sf -o /dev/null "http://127.0.0.1:$PORT/"; then
        break
    fi
    sleep 0.25
done
if ! curl -sf -o /dev/null "http://127.0.0.1:$PORT/"; then
    echo "smoke.sh: static server did not come up on port $PORT" >&2
    echo "--- server log: ---" >&2
    cat "$SERVER_LOG" >&2 || true
    exit 1
fi

# Run the browser check
SMOKE_URL="http://127.0.0.1:$PORT/" \
SMOKE_MARKER="${SMOKE_MARKER:-SkiaSharp Gallery}" \
SMOKE_SCREENSHOT="${SCREENSHOT:-}" \
    node --experimental-vm-modules "$DRIVER"
