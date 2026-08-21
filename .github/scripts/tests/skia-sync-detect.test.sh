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

cat >"${MOCK_BIN}/gh" <<'MOCK_GH'
#!/usr/bin/env bash
set -euo pipefail

[ "${1:-}" = api ] || { echo "unexpected gh command: $*" >&2; exit 1; }
endpoint="${2:?missing API endpoint}"

case "$endpoint" in
  repos/mono/SkiaSharp/contents/scripts/VERSIONS.txt\?ref=*)
    printf 'libSkiaSharp test milestone %s\n' "$TEST_MAIN_MS" | base64
    ;;
  repos/mono/skia/compare/*)
    printf '%s\n' "$endpoint" >>"$TEST_COMPARE_LOG"
    if [ "${TEST_COMPARE_FAIL:-false}" = true ]; then
      echo "gh: HTTP 503 Service Unavailable" >&2
      exit 1
    fi
    if [ -n "${TEST_SYNC_BRANCH:-}" ] &&
        [ "$endpoint" = "repos/mono/skia/compare/${TEST_UPSTREAM_SHA}...${TEST_SYNC_BRANCH}" ]; then
      printf '%s\n' "$TEST_SYNC_BEHIND"
    elif [ "$endpoint" = "repos/mono/skia/compare/${TEST_UPSTREAM_SHA}...${TEST_SKIA_BASE_BRANCH}" ]; then
      printf '%s\n' "$TEST_BASE_BEHIND"
    else
      echo "unexpected compare endpoint: $endpoint" >&2
      exit 1
    fi
    ;;
  *)
    echo "unexpected gh API endpoint: $endpoint" >&2
    exit 1
    ;;
esac
MOCK_GH

cat >"${MOCK_BIN}/git" <<'MOCK_GIT'
#!/usr/bin/env bash
set -euo pipefail

[ "${1:-}" = ls-remote ] || { echo "unexpected git command: $*" >&2; exit 1; }
shift
if [ "${1:-}" = --heads ]; then
  shift
fi

url="${1:?missing remote URL}"
ref="${2:?missing remote ref}"

case "$url" in
  https://github.com/google/skia.git)
    printf '%s\t%s\n' "$TEST_UPSTREAM_SHA" "$ref"
    ;;
  https://github.com/mono/SkiaSharp.git)
    if [ -n "${TEST_RELEASE_BRANCH:-}" ]; then
      printf '%s\trefs/heads/%s\n' "$TEST_RELEASE_SHA" "$TEST_RELEASE_BRANCH"
    fi
    ;;
  https://github.com/mono/skia.git)
    if [ -n "${TEST_SYNC_BRANCH:-}" ] &&
        [ "$ref" = "refs/heads/${TEST_SYNC_BRANCH}" ]; then
      printf '%s\t%s\n' "$TEST_SYNC_SHA" "$ref"
    elif [ "$ref" = "refs/heads/${TEST_SKIA_BASE_BRANCH}" ]; then
      printf '%s\t%s\n' "$TEST_BASE_SHA" "$ref"
    fi
    ;;
  *)
    echo "unexpected git remote: $url" >&2
    exit 1
    ;;
esac
MOCK_GIT

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
  local case_dir="${TMP_DIR}/${name}"
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
  : >"$compare_log"

  if ! env \
      PATH="${MOCK_BIN}:$PATH" \
      GITHUB_REPOSITORY=mono/SkiaSharp \
      GITHUB_SHA=trigger-sha \
      GITHUB_REF=refs/heads/main \
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
      bash "$DETECTOR" --output "$output" --target "$target" --base-branch "" \
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
  [ "$actual_compare" = "repos/mono/skia/compare/${upstream_sha}...${expected_compare_ref}" ] ||
    fail "$name: compared against '$actual_compare', expected '$expected_compare_ref'"
  grep -qx "skia_base_branch=${skia_base_branch}" "$output" ||
    fail "$name: resolved the wrong mono/skia base"
  grep -qx "head_branch=${head_branch}" "$output" ||
    fail "$name: resolved the wrong sync branch"

  echo "PASS: $name"
}

run_compare_failure_case() {
  local name=compare-api-failure
  local case_dir="${TMP_DIR}/${name}"
  local output="${case_dir}/output"
  local log="${case_dir}/log"
  local compare_log="${case_dir}/compare"
  local upstream_sha="upstream-${name}"

  mkdir -p "$case_dir"
  : >"$compare_log"

  if env \
      PATH="${MOCK_BIN}:$PATH" \
      GITHUB_REPOSITORY=mono/SkiaSharp \
      GITHUB_SHA=trigger-sha \
      GITHUB_REF=refs/heads/main \
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
      bash "$DETECTOR" --output "$output" --target 152 --base-branch "" \
      >"$log" 2>&1; then
    fail "$name: detector unexpectedly succeeded"
  fi

  if grep -q '^skip=' "$output"; then
    fail "$name: emitted a skip output while ancestry was unknown"
  fi
  grep -Fq "gh: HTTP 503 Service Unavailable" "$log" ||
    fail "$name: did not preserve gh stderr"
  grep -Fq "::error::Unable to compare upstream chrome/m152 (${upstream_sha}) against mono/skia skiasharp; ancestry is unknown, refusing to start sync." "$log" ||
    fail "$name: missing actionable error annotation"

  echo "PASS: $name"
}

run_case same-milestone-main-noop 152 152 "" "" 0 0 true skiasharp
run_case same-milestone-main-work 152 152 "" "" 3 0 false skiasharp
run_case existing-sync-needs-refresh 152 152 "" skia-sync/m152 0 2 false skia-sync/m152
run_case existing-sync-up-to-date 152 152 "" skia-sync/m152 4 0 true skia-sync/m152
run_case release-base-noop 151 152 release/3.151.x "" 0 0 true release/3.151.x
run_case true-milestone-bump-work 153 152 "" "" 5 0 false skiasharp
run_case explicit-work-output 152 152 "" "" 1 0 false skiasharp
run_compare_failure_case

echo "All skia-sync detector tests passed."
