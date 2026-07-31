#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
ROOT=$(mktemp -d)
trap 'rm -rf "$ROOT"' EXIT

PARENT="$ROOT/parent"
SKIA_SOURCE="$ROOT/skia-source"
PACKAGE="$ROOT/package"
EXPECTED="$ROOT/expected.env"
VERIFIED="$ROOT/verified"
EVIDENCE="$ROOT/evidence"
HEAD_BRANCH="skia-sync/test"
JQ_BIN="${SKIA_SYNC_JQ_BIN:-/usr/bin/jq}"

configure_repo() {
  git -C "$1" config user.email "sync-test@example.com"
  git -C "$1" config user.name "Sync Test"
}

git init -q "$SKIA_SOURCE"
configure_repo "$SKIA_SOURCE"
echo "skia" >"$SKIA_SOURCE/file.txt"
git -C "$SKIA_SOURCE" add file.txt
git -C "$SKIA_SOURCE" commit -q -m "skia base"
git -C "$SKIA_SOURCE" switch -q -c "$HEAD_BRANCH"

git init -q "$PARENT"
configure_repo "$PARENT"
echo "parent" >"$PARENT/parent.txt"
git -C "$PARENT" -c protocol.file.allow=always submodule add -q "$SKIA_SOURCE" externals/skia
git -C "$PARENT" add .
git -C "$PARENT" commit -q -m "parent base"
git -C "$PARENT" switch -q -c "$HEAD_BRANCH"
git -C "$PARENT/externals/skia" switch -q "$HEAD_BRANCH"

PARENT_HEAD=$(git -C "$PARENT" rev-parse HEAD)
SKIA_HEAD=$(git -C "$PARENT/externals/skia" rev-parse HEAD)
mkdir -p "$PACKAGE"
git -C "$PARENT" bundle create "$PACKAGE/skiasharp.bundle" "refs/heads/${HEAD_BRANCH}"
git -C "$PARENT/externals/skia" bundle create "$PACKAGE/skia.bundle" "refs/heads/${HEAD_BRANCH}"

cat >"$EXPECTED" <<EOF
TARGET=200
CURRENT=199
UPSTREAM_REF=chrome/m200
IS_RELEASE=false
BASE_BRANCH=main
SKIA_BASE_BRANCH=skiasharp
SKIA_BASE_SHA=$SKIA_HEAD
HEAD_BRANCH=$HEAD_BRANCH
BASE_UPSTREAM_SHA=$SKIA_HEAD
TARGET_UPSTREAM_SHA=$SKIA_HEAD
PARENT_REMOTE_HEAD=none
SKIA_REMOTE_HEAD=none
EOF
cp "$EXPECTED" "$PACKAGE/skia-sync-expected.env"
cp "$EXPECTED" "$PACKAGE/skia-sync-env.sh"

for report in skia-sync-skia-summary.md skia-sync-skiasharp-summary.md; do
  printf '## Changes\n\nFixture.\n\n## Testing\n\nFixture.\n\n## Human review\n\nFixture.\n' >"$PACKAGE/$report"
done
for report in skia-breaking-change-analysis.md skia-validation-review.md skia-dependency-decisions.md; do
  printf '# Fixture\n\nEvidence.\n' >"$PACKAGE/$report"
done

test_summary() {
  local assembly="$1"
  local passed="$2"
  local skipped="$3"
  printf 'Passed! - Failed: 0, Passed: %s, Skipped: %s, Total: %s, Duration: 1s - %s (net10.0|x64)\n' \
    "$passed" "$skipped" "$((passed + skipped))" "$assembly"
}

{
  echo "SKIA_SYNC_TEST_EVIDENCE full stage=initial solution=tests/SkiaSharp.Tests.Console.slnx tfm=net10.0 unfiltered=true"
  test_summary SkiaSharp.Tests.SingletonInit.dll 1 0
  test_summary SkiaSharp.Tests.dll 10 0
  test_summary SkiaSharp.Direct3D.Tests.dll 2 3
  test_summary SkiaSharp.Vulkan.Tests.dll 23 2
} >"$PACKAGE/initial-test-output.txt"
cp "$PACKAGE/initial-test-output.txt" "$PACKAGE/test-output.txt"
sed -i 's/stage=initial/stage=final/' "$PACKAGE/test-output.txt"

python3 "$SCRIPT_DIR/../../.agents/skills/update-skia/scripts/audit_fork_patches.py" \
  --skia-root "$PARENT/externals/skia" \
  --old-upstream "$SKIA_HEAD" \
  --new-upstream "$SKIA_HEAD" \
  --fork-base "$SKIA_HEAD" \
  --merged-head "$SKIA_HEAD" \
  --output "$PACKAGE/skia-fork-patch-audit.md"

PARENT_BUNDLE_SHA=$(sha256sum "$PACKAGE/skiasharp.bundle" | awk '{print $1}')
SKIA_BUNDLE_SHA=$(sha256sum "$PACKAGE/skia.bundle" | awk '{print $1}')
"$JQ_BIN" -n \
  --arg headBranch "$HEAD_BRANCH" \
  --arg parentHead "$PARENT_HEAD" \
  --arg skiaHead "$SKIA_HEAD" \
  --arg gitlink "$SKIA_HEAD" \
  --arg parentBundleSha256 "$PARENT_BUNDLE_SHA" \
  --arg skiaBundleSha256 "$SKIA_BUNDLE_SHA" \
  '{
    headBranch: $headBranch,
    parentHead: $parentHead,
    skiaHead: $skiaHead,
    gitlink: $gitlink,
    parentBundleSha256: $parentBundleSha256,
    skiaBundleSha256: $skiaBundleSha256
  }' >"$PACKAGE/attestation.json"

SKIA_SYNC_TRUSTED_DIR="$SCRIPT_DIR" \
SKIA_SYNC_PACKAGE_DIR="$PACKAGE" \
SKIA_SYNC_EXPECTED_ENV="$EXPECTED" \
SKIA_SYNC_WORKSPACE="$VERIFIED" \
SKIA_SYNC_ARTIFACT_DIR="$EVIDENCE" \
SKIA_SYNC_JQ_BIN="${SKIA_SYNC_JQ_BIN:-/usr/bin/jq}" \
SKIA_SYNC_SYSTEM_PATH="${SKIA_SYNC_SYSTEM_PATH:-/usr/bin:/bin}" \
  bash "$SCRIPT_DIR/skia-sync-prepare-verification.sh"

test "$(git -C "$VERIFIED" rev-parse HEAD)" = "$PARENT_HEAD"
test "$(git -C "$VERIFIED/externals/skia" rev-parse HEAD)" = "$SKIA_HEAD"
cmp -s "$EXPECTED" "$EVIDENCE/skia-sync-env.sh"

FRESH_TESTS="$ROOT/fresh-tests"
SEALED="$ROOT/sealed"
SEAL_STAGING="$ROOT/seal-staging"
SEAL_WORKSPACE="$ROOT/seal-workspace"
mkdir -p "$FRESH_TESTS"
cp "$PACKAGE/test-output.txt" "$FRESH_TESTS/test-output.txt"
{
  echo "SKIA_SYNC_TEST_EVIDENCE vulkan backend=ganesh filter=*CreateVkContextIsValid*"
  test_summary SkiaSharp.Vulkan.Tests.dll 1 0
} >"$FRESH_TESTS/vulkan-ganesh-evidence.txt"
{
  echo "SKIA_SYNC_TEST_EVIDENCE vulkan backend=graphite filter=*GraphiteVkContextIsCreatedFromRawHandles*"
  test_summary SkiaSharp.Vulkan.Tests.dll 1 0
} >"$FRESH_TESTS/vulkan-graphite-evidence.txt"

SKIA_SYNC_AUDIT_SCRIPT="$SCRIPT_DIR/../../.agents/skills/update-skia/scripts/audit_fork_patches.py" \
SKIA_SYNC_EXPECTED_ENV="$EXPECTED" \
SKIA_SYNC_OUTPUT_DIR="$SEALED" \
SKIA_SYNC_SOURCE_PACKAGE_DIR="$PACKAGE" \
SKIA_SYNC_STAGING_DIR="$SEAL_STAGING" \
SKIA_SYNC_TEST_EVIDENCE_DIR="$FRESH_TESTS" \
SKIA_SYNC_TEST_VALIDATOR="$SCRIPT_DIR/../../.agents/skills/update-skia/scripts/validate_test_output.py" \
SKIA_SYNC_TRUSTED_DIR="$SCRIPT_DIR" \
SKIA_SYNC_WORKSPACE="$SEAL_WORKSPACE" \
SKIA_SYNC_JQ_BIN="${SKIA_SYNC_JQ_BIN:-/usr/bin/jq}" \
SKIA_SYNC_SYSTEM_PATH="${SKIA_SYNC_SYSTEM_PATH:-/usr/bin:/bin}" \
  bash "$SCRIPT_DIR/skia-sync-seal-package.sh"

cmp -s "$PACKAGE/skiasharp.bundle" "$SEALED/skiasharp.bundle"
cmp -s "$PACKAGE/skia.bundle" "$SEALED/skia.bundle"

printf '\nUNKNOWN=value\n' >>"$PACKAGE/skia-sync-expected.env"
if SKIA_SYNC_TRUSTED_DIR="$SCRIPT_DIR" \
   SKIA_SYNC_PACKAGE_DIR="$PACKAGE" \
   SKIA_SYNC_EXPECTED_ENV="$EXPECTED" \
   SKIA_SYNC_WORKSPACE="$VERIFIED" \
   SKIA_SYNC_ARTIFACT_DIR="$EVIDENCE" \
   SKIA_SYNC_JQ_BIN="${SKIA_SYNC_JQ_BIN:-/usr/bin/jq}" \
   SKIA_SYNC_SYSTEM_PATH="${SKIA_SYNC_SYSTEM_PATH:-/usr/bin:/bin}" \
     bash "$SCRIPT_DIR/skia-sync-prepare-verification.sh"; then
  echo "Tampered pre-agent state unexpectedly validated." >&2
  exit 1
fi

echo "skia sync verification package tests passed"
