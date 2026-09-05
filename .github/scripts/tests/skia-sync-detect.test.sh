#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
DETECTOR="${SCRIPT_DIR}/../skia-sync-detect.sh"
TMP_DIR=$(mktemp -d)
MOCK_BIN="${TMP_DIR}/bin"
mkdir -p "$MOCK_BIN"
trap 'rm -rf "$TMP_DIR"' EXIT

fail() {
  echo "FAIL: $*" >&2
  exit 1
}

cp "${SCRIPT_DIR}/fixtures/skia-sync-gh" "${MOCK_BIN}/gh"
cp "${SCRIPT_DIR}/fixtures/skia-sync-git" "${MOCK_BIN}/git"

chmod +x "${MOCK_BIN}/gh" "${MOCK_BIN}/git"

run_case() {
  local name="$1"
  local target="$2"
  local main_ms="$3"
  local release_branch="$4"
  local sync_branch="$5"
  local base_behind="$6"
  local sync_behind="$7"
  local expected_skip="$8"
  local expected_compare_ref="$9"
  local owner="${10}"
  local repository="${owner}/SkiaSharp"
  local skia_repository="${owner}/skia"
  local case_dir="${TMP_DIR}/${owner}-${name}"
  local output="${case_dir}/output"
  local log="${case_dir}/log"
  local compare_log="${case_dir}/compare"
  local skia_base_branch="skiasharp"
  local head_branch="skia-sync/m${target}"
  local upstream_sha="upstream-${name}"

  if [ -n "$release_branch" ]; then
    skia_base_branch="$release_branch"
    head_branch="skia-sync/${release_branch//\//-}"
  fi

  mkdir -p "$case_dir"
  printf '%s\n' \
    '[submodule "externals/skia"]' \
    '	path = externals/skia' \
    "	url = https://github.com/${skia_repository}.git" \
    '[submodule "docs"]' \
    '	path = docs' \
    "	url = https://github.com/${owner}/SkiaSharp-API-docs" \
    >"$case_dir/.gitmodules"
  : >"$compare_log"

  if ! env \
      PATH="${MOCK_BIN}:$PATH" \
      GITHUB_REPOSITORY="$repository" \
      GITHUB_SHA=trigger-sha \
      GITHUB_REF=refs/heads/main \
      SKIASHARP_IDENTITY_ROOT="$case_dir" \
      TEST_MAIN_MS="$main_ms" \
      TEST_RELEASE_BRANCH="$release_branch" \
      TEST_RELEASE_SHA=release-sha \
      TEST_SYNC_BRANCH="$sync_branch" \
      TEST_SYNC_SHA=sync-sha \
      TEST_SKIA_BASE_BRANCH="$skia_base_branch" \
      TEST_BASE_SHA=base-sha \
      TEST_UPSTREAM_SHA="$upstream_sha" \
      TEST_BASE_BEHIND="$base_behind" \
      TEST_SYNC_BEHIND="$sync_behind" \
      TEST_COMPARE_LOG="$compare_log" \
      TEST_REPOSITORY="$repository" \
      TEST_REPOSITORY_GIT_URL="https://github.com/${repository}.git" \
      TEST_SKIA_REPOSITORY="$skia_repository" \
      TEST_SKIA_GIT_URL="https://github.com/${skia_repository}.git" \
      "$BASH" "$DETECTOR" --output "$output" --target "$target" --base-branch "" \
      >"$log" 2>&1; then
    cat "$log" >&2
    fail "$name: detector failed"
  fi

  local skip_count
  local actual_skip
  local actual_compare
  skip_count=$(grep -c '^skip=' "$output" || true)
  actual_skip=$(sed -n 's/^skip=//p' "$output")
  actual_compare=$(cat "$compare_log")

  [ "$skip_count" -eq 1 ] ||
    fail "$name: expected exactly one explicit skip output, got $skip_count"
  [ "$actual_skip" = "$expected_skip" ] ||
    fail "$name: expected skip=$expected_skip, got skip=$actual_skip"
  [ "$actual_compare" = "repos/${skia_repository}/compare/${upstream_sha}...${expected_compare_ref}" ] ||
    fail "$name: compared against '$actual_compare', expected '$expected_compare_ref'"
  grep -qx "repository=${repository}" "$output" ||
    fail "$name: resolved the wrong SkiaSharp repository"
  grep -qx "skia_repository=${skia_repository}" "$output" ||
    fail "$name: resolved the wrong paired Skia repository"
  grep -qx "skia_base_branch=${skia_base_branch}" "$output" ||
    fail "$name: resolved the wrong paired Skia base"
  grep -qx "head_branch=${head_branch}" "$output" ||
    fail "$name: resolved the wrong sync branch"

  echo "PASS: $name"
}

run_compare_failure_case() {
  local owner="$1"
  local name=compare-api-failure
  local repository="${owner}/SkiaSharp"
  local skia_repository="${owner}/skia"
  local case_dir="${TMP_DIR}/${owner}-${name}"
  local output="${case_dir}/output"
  local log="${case_dir}/log"
  local compare_log="${case_dir}/compare"
  local upstream_sha="upstream-${name}"

  mkdir -p "$case_dir"
  printf '%s\n' \
    '[submodule "externals/skia"]' \
    '	path = externals/skia' \
    "	url = https://github.com/${skia_repository}.git" \
    '[submodule "docs"]' \
    '	path = docs' \
    "	url = https://github.com/${owner}/SkiaSharp-API-docs" \
    >"$case_dir/.gitmodules"
  : >"$compare_log"

  if env \
      PATH="${MOCK_BIN}:$PATH" \
      GITHUB_REPOSITORY="$repository" \
      GITHUB_SHA=trigger-sha \
      GITHUB_REF=refs/heads/main \
      SKIASHARP_IDENTITY_ROOT="$case_dir" \
      TEST_MAIN_MS=152 \
      TEST_RELEASE_BRANCH="" \
      TEST_RELEASE_SHA=release-sha \
      TEST_SYNC_BRANCH="" \
      TEST_SYNC_SHA=sync-sha \
      TEST_SKIA_BASE_BRANCH=skiasharp \
      TEST_BASE_SHA=base-sha \
      TEST_UPSTREAM_SHA="$upstream_sha" \
      TEST_BASE_BEHIND=0 \
      TEST_SYNC_BEHIND=0 \
      TEST_COMPARE_FAIL=true \
      TEST_COMPARE_LOG="$compare_log" \
      TEST_REPOSITORY="$repository" \
      TEST_REPOSITORY_GIT_URL="https://github.com/${repository}.git" \
      TEST_SKIA_REPOSITORY="$skia_repository" \
      TEST_SKIA_GIT_URL="https://github.com/${skia_repository}.git" \
      "$BASH" "$DETECTOR" --output "$output" --target 152 --base-branch "" \
      >"$log" 2>&1; then
    fail "$name: detector unexpectedly succeeded"
  fi

  if grep -q '^skip=' "$output"; then
    fail "$name: emitted a skip output while ancestry was unknown"
  fi
  grep -Fq "gh: HTTP 503 Service Unavailable" "$log" ||
    fail "$name: did not preserve gh stderr"
  grep -Fq "::error::Unable to compare upstream chrome/m152 (${upstream_sha}) against ${skia_repository} skiasharp; ancestry is unknown, refusing to start sync." "$log" ||
    fail "$name: missing actionable error annotation"

  echo "PASS: ${owner}-${name}"
}

run_suite() {
  local owner="$1"
  run_case same-milestone-main-noop 152 152 "" "" 0 0 true skiasharp "$owner"
  run_case same-milestone-main-work 152 152 "" "" 3 0 false skiasharp "$owner"
  run_case existing-sync-needs-refresh 152 152 "" skia-sync/m152 0 2 false skia-sync/m152 "$owner"
  run_case existing-sync-up-to-date 152 152 "" skia-sync/m152 4 0 true skia-sync/m152 "$owner"
  run_case release-base-noop 151 152 release/3.151.x "" 0 0 true release/3.151.x "$owner"
  run_case true-milestone-bump-work 153 152 "" "" 5 0 false skiasharp "$owner"
  run_case explicit-work-output 152 152 "" "" 1 0 false skiasharp "$owner"
  run_compare_failure_case "$owner"
}

run_suite mono
run_suite dotnet

echo "All skia-sync detector tests passed."
