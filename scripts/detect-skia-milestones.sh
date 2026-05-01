#!/usr/bin/env bash
#
# detect-skia-milestones.sh — Discover current milestone, upstream targets, and existing PRs.
#
# Outputs lines of KEY=VALUE for the agent to parse:
#   CURRENT=147
#   LATEST=148
#   NEXT=148
#   TARGET_0=148
#   TARGET_1=149       (if different from NEXT)
#   UPSTREAM_m148=afe8b760ada...
#   BRANCH_EXISTS_m148=true|false
#   PR_SKIA_m148=205   (or empty)
#   PR_SKIASHARP_m148=3797  (or empty)
#

set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
SKIA_DIR="$REPO_ROOT/externals/skia"

# --- Current milestone ---
CURRENT=$(grep '^libSkiaSharp.*milestone' "$REPO_ROOT/scripts/VERSIONS.txt" | awk '{print $NF}')
echo "CURRENT=$CURRENT"

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
echo "LATEST=$LATEST"

# --- Compute targets ---
NEXT=$((CURRENT + 1))
echo "NEXT=$NEXT"

# Build deduplicated, sorted target list
TARGETS=()
if [[ "$NEXT" -le "$LATEST" ]]; then
    TARGETS+=("$NEXT")
fi
if [[ "$LATEST" -ne "$NEXT" && "$LATEST" -gt "$CURRENT" ]]; then
    TARGETS+=("$LATEST")
fi

# Sort targets numerically
IFS=$'\n' TARGETS=($(printf '%s\n' "${TARGETS[@]}" | sort -n)); unset IFS

for i in "${!TARGETS[@]}"; do
    echo "TARGET_$i=${TARGETS[$i]}"
done

if [[ ${#TARGETS[@]} -eq 0 ]]; then
    echo "NO_TARGETS=true"
    exit 0
fi

# --- For each target, gather info ---
for T in "${TARGETS[@]}"; do
    UPSTREAM_REF="upstream/chrome/m${T}"

    # Upstream tip SHA
    if git rev-parse --verify "$UPSTREAM_REF" &>/dev/null; then
        echo "UPSTREAM_m${T}=$(git rev-parse "$UPSTREAM_REF")"
    else
        echo "UPSTREAM_m${T}="
        continue
    fi

    # Does our branch exist?
    if git rev-parse --verify "origin/autobump/skia-m${T}" &>/dev/null; then
        echo "BRANCH_EXISTS_m${T}=true"

        # Is it up-to-date?
        BRANCH_TIP=$(git rev-parse "origin/autobump/skia-m${T}")
        MERGE_BASE=$(git merge-base "$BRANCH_TIP" "$UPSTREAM_REF" 2>/dev/null || echo "")
        UPSTREAM_TIP=$(git rev-parse "$UPSTREAM_REF")
        if [[ "$MERGE_BASE" == "$UPSTREAM_TIP" ]]; then
            echo "UP_TO_DATE_m${T}=true"
        else
            echo "UP_TO_DATE_m${T}=false"
        fi
    else
        echo "BRANCH_EXISTS_m${T}=false"
        echo "UP_TO_DATE_m${T}=false"
    fi
done

echo "DISCOVERY_COMPLETE=true"
