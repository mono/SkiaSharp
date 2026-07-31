#!/usr/bin/env bash
#
# Materialize untrusted sync bundles into a fresh token-free verification workspace.

set -euo pipefail

export PATH="${SKIA_SYNC_SYSTEM_PATH:-/usr/bin:/bin}"
export GIT_NO_REPLACE_OBJECTS=1
unset BASH_ENV ENV CDPATH GIT_CONFIG_COUNT GIT_EXEC_PATH PYTHONHOME PYTHONPATH
unset DOTNET_STARTUP_HOOKS LD_PRELOAD

TRUSTED_DIR="${SKIA_SYNC_TRUSTED_DIR:-$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)}"
PACKAGE_DIR="${SKIA_SYNC_PACKAGE_DIR:?SKIA_SYNC_PACKAGE_DIR is required}"
EXPECTED_ENV="${SKIA_SYNC_EXPECTED_ENV:?SKIA_SYNC_EXPECTED_ENV is required}"
WORKSPACE="${SKIA_SYNC_WORKSPACE:?SKIA_SYNC_WORKSPACE is required}"
ARTIFACT_DIR="${SKIA_SYNC_ARTIFACT_DIR:?SKIA_SYNC_ARTIFACT_DIR is required}"
JQ_BIN="${SKIA_SYNC_JQ_BIN:-/usr/bin/jq}"

# shellcheck source=/dev/null
source "$TRUSTED_DIR/skia-sync-common.sh"

required_file() {
  local path="$1"
  if [[ ! -s "$path" ]]; then
    sync_error "Required verification input is missing or empty: $path"
    exit 1
  fi
}

required_file "$EXPECTED_ENV"
required_file "$PACKAGE_DIR/skia-sync-expected.env"
required_file "$PACKAGE_DIR/skia-sync-env.sh"
required_file "$PACKAGE_DIR/attestation.json"
required_file "$PACKAGE_DIR/skiasharp.bundle"
required_file "$PACKAGE_DIR/skia.bundle"
required_file "$PACKAGE_DIR/skia-sync-skia-summary.md"
required_file "$PACKAGE_DIR/skia-sync-skiasharp-summary.md"
required_file "$PACKAGE_DIR/skia-breaking-change-analysis.md"
required_file "$PACKAGE_DIR/skia-validation-review.md"
required_file "$PACKAGE_DIR/skia-dependency-decisions.md"
required_file "$PACKAGE_DIR/initial-test-output.txt"
required_file "$PACKAGE_DIR/test-output.txt"
required_file "$PACKAGE_DIR/skia-fork-patch-audit.md"

if ! cmp -s "$EXPECTED_ENV" "$PACKAGE_DIR/skia-sync-expected.env" ||
   ! cmp -s "$EXPECTED_ENV" "$PACKAGE_DIR/skia-sync-env.sh"; then
  sync_error "Validated package state does not match the pre-agent state artifact."
  exit 1
fi
load_sync_env "$EXPECTED_ENV"

ATTESTATION="$PACKAGE_DIR/attestation.json"
PARENT_HEAD=$("$JQ_BIN" -er '.parentHead' "$ATTESTATION")
SKIA_HEAD=$("$JQ_BIN" -er '.skiaHead' "$ATTESTATION")
GITLINK=$("$JQ_BIN" -er '.gitlink' "$ATTESTATION")
PARENT_BUNDLE_SHA=$("$JQ_BIN" -er '.parentBundleSha256' "$ATTESTATION")
SKIA_BUNDLE_SHA=$("$JQ_BIN" -er '.skiaBundleSha256' "$ATTESTATION")
if [[ "$("$JQ_BIN" -er '.headBranch' "$ATTESTATION")" != "$HEAD_BRANCH" ||
      "$GITLINK" != "$SKIA_HEAD" ||
      "$(sha256sum "$PACKAGE_DIR/skiasharp.bundle" | awk '{print $1}')" != "$PARENT_BUNDLE_SHA" ||
      "$(sha256sum "$PACKAGE_DIR/skia.bundle" | awk '{print $1}')" != "$SKIA_BUNDLE_SHA" ]]; then
  sync_error "Validated package attestation is inconsistent."
  exit 1
fi

rm -rf "$WORKSPACE" "$ARTIFACT_DIR"
mkdir -p "$WORKSPACE" "$ARTIFACT_DIR"

git -C "$WORKSPACE" init -q
git -C "$WORKSPACE" fetch -q \
  "$PACKAGE_DIR/skiasharp.bundle" \
  "refs/heads/${HEAD_BRANCH}:refs/heads/${HEAD_BRANCH}"
git -C "$WORKSPACE" switch -q "$HEAD_BRANCH"

git -C "$WORKSPACE" config \
  submodule.externals/skia.url \
  "$PACKAGE_DIR/skia.bundle"
git -C "$WORKSPACE" -c protocol.file.allow=always submodule update --init --recursive
git -C "$WORKSPACE/externals/skia" switch -q -C "$HEAD_BRANCH" "$SKIA_HEAD"

if [[ "$(git -C "$WORKSPACE" rev-parse HEAD)" != "$PARENT_HEAD" ]]; then
  sync_error "Parent bundle did not materialize the attested commit."
  exit 1
fi
validate_sync_checkout "$WORKSPACE"

cp "$EXPECTED_ENV" "$ARTIFACT_DIR/skia-sync-env.sh"
cp "$PACKAGE_DIR/skia-sync-skia-summary.md" "$ARTIFACT_DIR/skia-sync-skia-summary.md"
cp "$PACKAGE_DIR/skia-sync-skiasharp-summary.md" "$ARTIFACT_DIR/skia-sync-skiasharp-summary.md"
cp "$PACKAGE_DIR/skia-breaking-change-analysis.md" "$ARTIFACT_DIR/skia-breaking-change-analysis.md"
cp "$PACKAGE_DIR/skia-validation-review.md" "$ARTIFACT_DIR/skia-validation-review.md"
cp "$PACKAGE_DIR/skia-dependency-decisions.md" "$ARTIFACT_DIR/skia-dependency-decisions.md"
cp "$PACKAGE_DIR/initial-test-output.txt" "$ARTIFACT_DIR/initial-test-output.txt"
cp "$PACKAGE_DIR/test-output.txt" "$ARTIFACT_DIR/test-output.txt"
cp "$PACKAGE_DIR/skia-fork-patch-audit.md" "$ARTIFACT_DIR/skia-fork-patch-audit.md"
