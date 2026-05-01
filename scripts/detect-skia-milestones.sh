#!/usr/bin/env bash
#
# detect-skia-milestones.sh — Pick a single milestone to bump.
#
# Accepts MODE env var: "next" (current+1) or "latest" (highest upstream).
# Defaults to "next" if unset.
#
# Writes to GITHUB_OUTPUT (if set) and stdout:
#   current  — current milestone (e.g. 147)
#   target   — the single milestone to process (e.g. 148)
#   latest   — highest upstream chrome/m* branch
#   mode     — "next" or "latest"
#
# Exits 1 if the target is already up-to-date or doesn't exist (skips agent).
#

set -euo pipefail

MODE="${MODE:-next}"

REPO_ROOT="$(git rev-parse --show-toplevel)"
SKIA_DIR="$REPO_ROOT/externals/skia"

emit() {
    echo "$1=$2"
    if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
        echo "$1=$2" >> "$GITHUB_OUTPUT"
    fi
}

# --- Current milestone ---
CURRENT=$(grep '^libSkiaSharp.*milestone' "$REPO_ROOT/scripts/VERSIONS.txt" | awk '{print $NF}')
emit "current" "$CURRENT"
emit "mode" "$MODE"

# --- Init submodule and remotes ---
cd "$REPO_ROOT"
git submodule update --init externals/skia 2>/dev/null || true

cd "$SKIA_DIR"
if ! git remote get-url upstream &>/dev/null; then
    git remote add upstream https://github.com/google/skia.git
fi
git fetch upstream --quiet 2>/dev/null
git fetch origin --quiet 2>/dev/null

# --- Find latest upstream milestone (numeric sort) ---
LATEST=$(git branch -r \
    | sed -n 's|.*upstream/chrome/m\([0-9]*\).*|\1|p' \
    | sort -n \
    | tail -1)
emit "latest" "$LATEST"

# --- Pick target based on mode ---
NEXT=$((CURRENT + 1))
emit "next" "$NEXT"

if [[ "$MODE" == "latest" ]]; then
    TARGET="$LATEST"
else
    TARGET="$NEXT"
fi

# Verify upstream branch exists
if ! git rev-parse --verify "upstream/chrome/m${TARGET}" &>/dev/null; then
    echo "::notice::upstream/chrome/m${TARGET} does not exist yet"
    exit 1
fi

# Check if already up-to-date
if git rev-parse --verify "origin/autobump/skia-m${TARGET}" &>/dev/null; then
    BRANCH_TIP=$(git rev-parse "origin/autobump/skia-m${TARGET}")
    UPSTREAM_TIP=$(git rev-parse "upstream/chrome/m${TARGET}")
    MERGE_BASE=$(git merge-base "$BRANCH_TIP" "upstream/chrome/m${TARGET}" 2>/dev/null || echo "")
    if [[ "$MERGE_BASE" == "$UPSTREAM_TIP" ]]; then
        echo "::notice::autobump/skia-m${TARGET} is already up-to-date"
        exit 1
    fi
    echo "autobump/skia-m${TARGET} exists but has new upstream commits"
else
    echo "autobump/skia-m${TARGET} does not exist yet — will create"
fi

emit "target" "$TARGET"
echo "Will process: m${TARGET} (mode=${MODE}, current=m${CURRENT}, latest=m${LATEST})"
