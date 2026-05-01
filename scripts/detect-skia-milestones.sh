#!/usr/bin/env bash
#
# detect-skia-milestones.sh — Discover current milestone, upstream targets, and existing PRs.
#
# When GITHUB_OUTPUT is set (GitHub Actions), writes key=value to it for use in
# workflow expressions. Otherwise prints to stdout for local testing.
#
# Exits with code 1 if there is nothing to do (no targets or all up-to-date),
# which causes gh aw pre-activation to skip the agent job entirely.
#
# Output keys:
#   current       — current milestone number (e.g. 147)
#   next          — current + 1
#   latest        — highest upstream chrome/m* branch
#   targets       — comma-separated target milestones to process (e.g. "148,149")
#   target_count  — number of targets (0, 1, or 2)
#

set -euo pipefail

REPO_ROOT="$(git rev-parse --show-toplevel)"
SKIA_DIR="$REPO_ROOT/externals/skia"

# Helper: write key=value to GITHUB_OUTPUT (if set) and stdout
emit() {
    echo "$1=$2"
    if [[ -n "${GITHUB_OUTPUT:-}" ]]; then
        echo "$1=$2" >> "$GITHUB_OUTPUT"
    fi
}

# --- Current milestone ---
CURRENT=$(grep '^libSkiaSharp.*milestone' "$REPO_ROOT/scripts/VERSIONS.txt" | awk '{print $NF}')
emit "current" "$CURRENT"

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

# --- Compute targets ---
NEXT=$((CURRENT + 1))
emit "next" "$NEXT"

# Build deduplicated, sorted target list
TARGETS=()
if [[ "$NEXT" -le "$LATEST" ]]; then
    TARGETS+=("$NEXT")
fi
if [[ "$LATEST" -ne "$NEXT" && "$LATEST" -gt "$CURRENT" ]]; then
    TARGETS+=("$LATEST")
fi

# Sort targets numerically
if [[ ${#TARGETS[@]} -gt 0 ]]; then
    IFS=$'\n' TARGETS=($(printf '%s\n' "${TARGETS[@]}" | sort -n)); unset IFS
fi

emit "target_count" "${#TARGETS[@]}"

if [[ ${#TARGETS[@]} -eq 0 ]]; then
    echo "::notice::No milestones ahead of m$CURRENT (latest upstream: m$LATEST)"
    exit 1
fi

# --- For each target, check if up-to-date ---
WORK_NEEDED=()
for T in "${TARGETS[@]}"; do
    UPSTREAM_REF="upstream/chrome/m${T}"

    if ! git rev-parse --verify "$UPSTREAM_REF" &>/dev/null; then
        continue
    fi

    # Does our branch exist and is it current?
    if git rev-parse --verify "origin/autobump/skia-m${T}" &>/dev/null; then
        BRANCH_TIP=$(git rev-parse "origin/autobump/skia-m${T}")
        MERGE_BASE=$(git merge-base "$BRANCH_TIP" "$UPSTREAM_REF" 2>/dev/null || echo "")
        UPSTREAM_TIP=$(git rev-parse "$UPSTREAM_REF")
        if [[ "$MERGE_BASE" == "$UPSTREAM_TIP" ]]; then
            echo "  m$T: up-to-date, skipping"
            continue
        fi
    fi

    WORK_NEEDED+=("$T")
done

if [[ ${#WORK_NEEDED[@]} -eq 0 ]]; then
    echo "::notice::All targets up-to-date (checked: ${TARGETS[*]})"
    exit 1
fi

# Emit the final targets list (only milestones that need work)
emit "targets" "$(IFS=,; echo "${WORK_NEEDED[*]}")"

echo "Milestones needing work: ${WORK_NEEDED[*]}"
