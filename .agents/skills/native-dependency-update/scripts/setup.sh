#!/bin/bash
set -euo pipefail

# Native Dependency Update — Environment Setup
#
# Usage: setup.sh <dep> [skia_target_branch] [skiasharp_target_branch]
#
# Examples:
#   ./setup.sh libpng                          # default: skiasharp + main
#   ./setup.sh libpng skiasharp main           # explicit default
#   ./setup.sh libpng skiasharp release/3.119.x  # release branch

DEP="${1:?Usage: setup.sh <dep> [skia_target_branch] [skiasharp_target_branch]}"
SKIA_TARGET="${2:-skiasharp}"
SKIASHARP_TARGET="${3:-main}"

export PATH="/usr/local/share/dotnet:/opt/homebrew/bin:$PATH"

echo "=== Native Dependency Update Setup ==="
echo "  Dependency:              $DEP"
echo "  Skia target branch:     $SKIA_TARGET"
echo "  SkiaSharp target branch: $SKIASHARP_TARGET"
echo ""

# --- Step 1: Set target branch (if not main) ---
if [ "$SKIASHARP_TARGET" != "main" ]; then
    echo "--- Setting SkiaSharp to target branch: $SKIASHARP_TARGET ---"
    git fetch origin "$SKIASHARP_TARGET"
    git reset --hard "origin/$SKIASHARP_TARGET"
fi

# --- Step 2: Initialize all submodules ---
echo "--- Initializing submodules (externals/skia is ~900MB, may take ~8 min on first clone) ---"
git submodule update --init
echo "    Submodules initialized."

# --- Step 3: Unshallow the target dependency ---
DEP_DIR="externals/skia/third_party/externals/$DEP"
if [ -d "$DEP_DIR" ]; then
    echo "--- Unshallowing $DEP_DIR ---"
    pushd "$DEP_DIR" > /dev/null
    if git rev-parse --is-shallow-repository 2>/dev/null | grep -q true; then
        git fetch --unshallow origin
        echo "    Unshallowed."
    else
        echo "    Already a full clone."
    fi
    popd > /dev/null
else
    echo "--- WARNING: $DEP_DIR does not exist yet."
    echo "    It will be created by git-sync-deps during the native build step."
    echo "    Re-run this script after the first build to unshallow it."
fi

# --- Step 4: Determine skia base and create feature branch ---
echo "--- Setting up skia feature branch ---"
pushd externals/skia > /dev/null

# The submodule is checked out at a specific commit after submodule update.
SUBMODULE_COMMIT=$(git rev-parse HEAD)
echo "    Submodule commit: $SUBMODULE_COMMIT"

# Fetch and verify the requested skia target branch
if ! git fetch origin "$SKIA_TARGET" 2>/dev/null; then
    echo ""
    echo "=== ERROR: Branch 'origin/$SKIA_TARGET' does not exist in mono/skia ==="
    echo "Ask the user which skia branch to target for this update."
    popd > /dev/null
    exit 1
fi

if ! git merge-base --is-ancestor "$SUBMODULE_COMMIT" "origin/$SKIA_TARGET" 2>/dev/null; then
    echo ""
    echo "=== ERROR: Submodule commit $SUBMODULE_COMMIT is NOT on origin/$SKIA_TARGET ==="
    echo "The SkiaSharp target branch uses a skia commit that isn't on '$SKIA_TARGET'."
    echo "Ask the user which skia branch to target for this update."
    popd > /dev/null
    exit 1
fi

echo "    Confirmed: submodule commit is on origin/$SKIA_TARGET"
git checkout -b "dev/update-$DEP" "origin/$SKIA_TARGET"

popd > /dev/null
echo "    Skia branch created."

# --- Step 5: Verify environment ---
echo ""
echo "=== Verification ==="

ERRORS=0

echo -n "  dotnet:       "
if dotnet --version 2>/dev/null; then
    true
else
    echo "NOT FOUND — ensure .NET SDK is installed and PATH includes /usr/local/share/dotnet"
    ERRORS=$((ERRORS + 1))
fi

echo -n "  DEPS:         "
if [ -f externals/skia/DEPS ]; then
    echo "OK"
else
    echo "MISSING — submodule init may have failed"
    ERRORS=$((ERRORS + 1))
fi

echo -n "  depot_tools:  "
if [ -f externals/depot_tools/ninja.py ]; then
    echo "OK"
else
    echo "MISSING — submodule init may have failed"
    ERRORS=$((ERRORS + 1))
fi

echo -n "  Skia branch:  "
pushd externals/skia > /dev/null
BRANCH=$(git branch --show-current)
echo "$BRANCH"
if [ "$BRANCH" != "dev/update-$DEP" ]; then
    echo "    ERROR: Expected dev/update-$DEP"
    ERRORS=$((ERRORS + 1))
fi
popd > /dev/null

echo -n "  Dep directory: "
if [ -d "$DEP_DIR" ]; then
    echo "OK ($DEP_DIR)"
else
    echo "MISSING (will be created during native build)"
fi

echo ""
if [ "$ERRORS" -gt 0 ]; then
    echo "=== SETUP FAILED: $ERRORS error(s) above ==="
    exit 1
fi

echo "=== Setup Complete ==="
echo ""
echo "Proceed to Phase 1: Discovery"
echo ""
echo "Environment reminders (do not persist across bash calls):"
echo "  - Prefix dotnet commands with: export PATH=\"/usr/local/share/dotnet:/opt/homebrew/bin:\$PATH\" &&"
echo "  - Prefix gh write commands with: unset GH_TOKEN &&"
echo "  - Use grep -E (not grep -P) on macOS"
