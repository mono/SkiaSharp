#!/usr/bin/env bash
#
# Seal original source-run bundles after a separate candidate test job succeeds.

set -euo pipefail

export PATH="${SKIA_SYNC_SYSTEM_PATH:-/usr/bin:/bin}"
export GIT_NO_REPLACE_OBJECTS=1
unset BASH_ENV ENV CDPATH GIT_CONFIG_COUNT GIT_EXEC_PATH PYTHONHOME PYTHONPATH
unset DOTNET_STARTUP_HOOKS LD_PRELOAD

TRUSTED_DIR="${SKIA_SYNC_TRUSTED_DIR:-$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)}"
AUDIT_SCRIPT="${SKIA_SYNC_AUDIT_SCRIPT:?SKIA_SYNC_AUDIT_SCRIPT is required}"
TEST_VALIDATOR="${SKIA_SYNC_TEST_VALIDATOR:?SKIA_SYNC_TEST_VALIDATOR is required}"
SOURCE_PACKAGE_DIR="${SKIA_SYNC_SOURCE_PACKAGE_DIR:?SKIA_SYNC_SOURCE_PACKAGE_DIR is required}"
EXPECTED_ENV="${SKIA_SYNC_EXPECTED_ENV:?SKIA_SYNC_EXPECTED_ENV is required}"
TEST_EVIDENCE_DIR="${SKIA_SYNC_TEST_EVIDENCE_DIR:?SKIA_SYNC_TEST_EVIDENCE_DIR is required}"
OUTPUT_DIR="${SKIA_SYNC_OUTPUT_DIR:?SKIA_SYNC_OUTPUT_DIR is required}"
WORKSPACE="${SKIA_SYNC_WORKSPACE:?SKIA_SYNC_WORKSPACE is required}"
STAGING_DIR="${SKIA_SYNC_STAGING_DIR:?SKIA_SYNC_STAGING_DIR is required}"

# shellcheck source=/dev/null
source "$TRUSTED_DIR/skia-sync-common.sh"

SKIA_SYNC_TRUSTED_DIR="$TRUSTED_DIR" \
SKIA_SYNC_PACKAGE_DIR="$SOURCE_PACKAGE_DIR" \
SKIA_SYNC_EXPECTED_ENV="$EXPECTED_ENV" \
SKIA_SYNC_WORKSPACE="$WORKSPACE" \
SKIA_SYNC_ARTIFACT_DIR="$STAGING_DIR" \
  bash "$TRUSTED_DIR/skia-sync-prepare-verification.sh"

load_sync_env "$EXPECTED_ENV"
python3 "$AUDIT_SCRIPT" \
  --skia-root "$WORKSPACE/externals/skia" \
  --old-upstream "$BASE_UPSTREAM_SHA" \
  --new-upstream "$TARGET_UPSTREAM_SHA" \
  --fork-base "$SKIA_BASE_SHA" \
  --merged-head "$HEAD_BRANCH" \
  --output "$STAGING_DIR/skia-fork-patch-audit.md" \
  --validate

for file in test-output.txt vulkan-ganesh-evidence.txt vulkan-graphite-evidence.txt; do
  if [[ ! -s "$TEST_EVIDENCE_DIR/$file" ]]; then
    sync_error "Fresh test evidence is missing or empty: $TEST_EVIDENCE_DIR/$file"
    exit 1
  fi
  cp "$TEST_EVIDENCE_DIR/$file" "$STAGING_DIR/$file"
done

python3 "$TEST_VALIDATOR" \
  --initial "$STAGING_DIR/initial-test-output.txt" \
  --final "$STAGING_DIR/test-output.txt" \
  --ganesh "$STAGING_DIR/vulkan-ganesh-evidence.txt" \
  --graphite "$STAGING_DIR/vulkan-graphite-evidence.txt"

rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"
for file in \
  attestation.json \
  skiasharp.bundle \
  skia.bundle \
  skia-sync-expected.env \
  skia-sync-env.sh \
  skia-sync-skia-summary.md \
  skia-sync-skiasharp-summary.md; do
  cp "$SOURCE_PACKAGE_DIR/$file" "$OUTPUT_DIR/$file"
done
cp "$STAGING_DIR/initial-test-output.txt" "$OUTPUT_DIR/initial-test-output.txt"
cp "$STAGING_DIR/test-output.txt" "$OUTPUT_DIR/test-output.txt"
cp "$STAGING_DIR/vulkan-ganesh-evidence.txt" "$OUTPUT_DIR/vulkan-ganesh-evidence.txt"
cp "$STAGING_DIR/vulkan-graphite-evidence.txt" "$OUTPUT_DIR/vulkan-graphite-evidence.txt"
cp "$STAGING_DIR/skia-fork-patch-audit.md" "$OUTPUT_DIR/skia-fork-patch-audit.md"
cp "$STAGING_DIR/skia-breaking-change-analysis.md" "$OUTPUT_DIR/skia-breaking-change-analysis.md"
cp "$STAGING_DIR/skia-validation-review.md" "$OUTPUT_DIR/skia-validation-review.md"
cp "$STAGING_DIR/skia-dependency-decisions.md" "$OUTPUT_DIR/skia-dependency-decisions.md"
chmod -R a-w "$OUTPUT_DIR"
