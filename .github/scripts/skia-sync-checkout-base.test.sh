#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
REPO_ROOT=$(cd "$SCRIPT_DIR/../.." && pwd)
TEST_ROOT=$(mktemp -d)
trap 'rm -rf "$TEST_ROOT"' EXIT

git_configure() {
  git -C "$1" config user.name "Skia Sync Test"
  git -C "$1" config user.email "skia-sync-test@example.com"
}

LEAF="$TEST_ROOT/leaf"
git init -q -b main "$LEAF"
git_configure "$LEAF"
echo "m150 dependency" >"$LEAF/version.txt"
git -C "$LEAF" add version.txt
git -C "$LEAF" commit -q -m "leaf m150"
LEAF_M150=$(git -C "$LEAF" rev-parse HEAD)
echo "m151 dependency" >"$LEAF/version.txt"
git -C "$LEAF" commit -qam "leaf m151"
LEAF_M151=$(git -C "$LEAF" rev-parse HEAD)

SKIA="$TEST_ROOT/skia"
git init -q -b main "$SKIA"
git_configure "$SKIA"
printf 'out/\nthird_party/externals/\n' >"$SKIA/.gitignore"
git -C "$SKIA" -c protocol.file.allow=always submodule add -q "$LEAF" third_party/leaf
git -C "$SKIA/third_party/leaf" checkout -q "$LEAF_M150"
git -C "$SKIA" add .gitignore .gitmodules third_party/leaf
git -C "$SKIA" commit -q -m "skia m150"
SKIA_M150=$(git -C "$SKIA" rev-parse HEAD)
git -C "$SKIA" branch release/4.150.x
git -C "$SKIA/third_party/leaf" checkout -q "$LEAF_M151"
git -C "$SKIA" add third_party/leaf
git -C "$SKIA" commit -q -m "skia m151"
SKIA_M151=$(git -C "$SKIA" rev-parse HEAD)

PARENT="$TEST_ROOT/parent"
git init -q -b main "$PARENT"
git_configure "$PARENT"
mkdir -p "$PARENT/scripts" "$PARENT/native/linux"
echo "output/" >"$PARENT/.gitignore"
echo "libSkiaSharp milestone 150" >"$PARENT/scripts/VERSIONS.txt"
echo "release-150-build" >"$PARENT/native/linux/build-marker.txt"
git -C "$PARENT" -c protocol.file.allow=always submodule add -q "$SKIA" externals/skia
git -C "$PARENT/externals/skia" checkout -q "$SKIA_M150"
git -C "$PARENT" add .
git -C "$PARENT" commit -q -m "parent m150"
PARENT_M150=$(git -C "$PARENT" rev-parse HEAD)
git -C "$PARENT" branch release/4.150.x

echo "libSkiaSharp milestone 151" >"$PARENT/scripts/VERSIONS.txt"
echo "main-151-build" >"$PARENT/native/linux/build-marker.txt"
git -C "$PARENT/externals/skia" checkout -q "$SKIA_M151"
git -C "$PARENT" add .
git -C "$PARENT" commit -q -m "parent m151"
PARENT_M151=$(git -C "$PARENT" rev-parse HEAD)

SOURCE="$TEST_ROOT/source"
TARGET="$TEST_ROOT/isolated"
git clone -q "$PARENT" "$SOURCE"
SOURCE_BEFORE=$(git -C "$SOURCE" rev-parse HEAD)

GITHUB_WORKSPACE="$SOURCE" \
SKIA_SYNC_WORKSPACE="$TARGET" \
base_branch="release/4.150.x" \
  bash "$SCRIPT_DIR/skia-sync-checkout-base.sh"

test "$SOURCE_BEFORE" = "$PARENT_M151"
test "$(git -C "$SOURCE" rev-parse HEAD)" = "$PARENT_M151"
test "$(cat "$SOURCE/scripts/VERSIONS.txt")" = "libSkiaSharp milestone 151"
test "$(cat "$SOURCE/native/linux/build-marker.txt")" = "main-151-build"
git -C "$SOURCE" diff --quiet

test "$(git -C "$TARGET" rev-parse HEAD)" = "$PARENT_M150"
test "$(cat "$TARGET/scripts/VERSIONS.txt")" = "libSkiaSharp milestone 150"
test "$(cat "$TARGET/native/linux/build-marker.txt")" = "release-150-build"
test "$(git -C "$TARGET/externals/skia" rev-parse HEAD)" = "$SKIA_M150"
test "$(git -C "$TARGET/externals/skia/third_party/leaf" rev-parse HEAD)" = "$LEAF_M150"
test -z "$(git -C "$TARGET" submodule status --recursive | grep -E '^[+-U]' || true)"

mkdir -p \
  "$TARGET/externals/skia/third_party/externals" \
  "$TARGET/externals/skia/out/linux/x64" \
  "$TARGET/output/native/linux/x64"
echo "hydrated dependency" >"$TARGET/externals/skia/third_party/externals/cache-marker"
echo "ninja object" >"$TARGET/externals/skia/out/linux/x64/cache-marker"
echo "native output" >"$TARGET/output/native/linux/x64/cache-marker"

SKIA_SYNC_BASE_BRANCH="release/4.150.x" \
SKIA_SYNC_PARENT_BASE_SHA="$PARENT_M150" \
SKIA_SYNC_SKIA_BASE_BRANCH="release/4.150.x" \
SKIA_SYNC_SKIA_BASE_SHA="$SKIA_M150" \
SKIA_SYNC_HEAD_BRANCH="skia-sync/release-4.150.x" \
  python3 "$REPO_ROOT/.agents/skills/update-skia/scripts/prepare_branches.py" \
    --repo-root "$TARGET"

test "$(git -C "$TARGET" branch --show-current)" = "skia-sync/release-4.150.x"
test "$(git -C "$TARGET/externals/skia" branch --show-current)" = "skia-sync/release-4.150.x"
test "$(cat "$TARGET/externals/skia/third_party/externals/cache-marker")" = "hydrated dependency"
test "$(cat "$TARGET/externals/skia/out/linux/x64/cache-marker")" = "ninja object"
test "$(cat "$TARGET/output/native/linux/x64/cache-marker")" = "native output"

echo "Exact release tree and native base state survive Phase 04 branch creation."
