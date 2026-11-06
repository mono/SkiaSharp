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
    ms="$TEST_MAIN_MS"
    if [[ "$endpoint" =~ ref=release/[0-9]+\.([0-9]+)\.x$ ]]; then
      ms="${BASH_REMATCH[1]}"
    fi
    printf 'libSkiaSharp test milestone %s\n' "$ms" | base64
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

if [ "${1:-}" = check-ref-format ]; then
  exit 0
fi
[ "${1:-}" = ls-remote ] || { echo "unexpected git command: $*" >&2; exit 1; }
shift
exit_code=false
while [ "${1:-}" = --heads ] || [ "${1:-}" = --exit-code ]; do
  [ "$1" = --exit-code ] && exit_code=true
  shift
done

url="${1:?missing remote URL}"
ref="${2:?missing remote ref}"

case "$url" in
  https://github.com/google/skia.git)
    printf '%s\t%s\n' "$TEST_UPSTREAM_SHA" "$ref"
    ;;
  https://github.com/mono/SkiaSharp.git)
    # A glob lists the release branch; an exact ref only matches that branch (or main).
    if [ -n "${TEST_RELEASE_BRANCH:-}" ] &&
        { [[ "$ref" == *'*'* ]] || [ "$ref" = "refs/heads/${TEST_RELEASE_BRANCH}" ]; }; then
      printf '%s\trefs/heads/%s\n' "$TEST_RELEASE_SHA" "$TEST_RELEASE_BRANCH"
    elif [ "$ref" = refs/heads/main ]; then
      printf 'main-sha\trefs/heads/main\n'
    elif [ "$exit_code" = true ]; then
      exit 2
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
  local base_override="${10:-}"
  local expected_head="${11:-}"
  local case_dir="${TMP_DIR}/${name}"
  local output="${case_dir}/output"
  local log="${case_dir}/log"
  local compare_log="${case_dir}/compare"
  local skia_base_branch="skiasharp"
  local head_branch="skia-sync/m${target}"
  local upstream_sha="upstream-${name}"

  if [ -n "$release_branch" ] && [ "$target" -le "$main_ms" ] && { [ -z "$base_override" ] || [ "$base_override" = "$release_branch" ]; }; then
    skia_base_branch="$release_branch"
    head_branch="skia-sync/${release_branch//\//-}"
  fi
  [ -z "$expected_head" ] || head_branch="$expected_head"

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
      bash "$DETECTOR" --output "$output" --target "$target" --base-branch "$base_override" \
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

  if [ "$skia_base_branch" != skiasharp ]; then
    grep -qx "is_release=true" "$output" || fail "$name: release route not flagged"
    grep -qx "base_branch=${skia_base_branch}" "$output" ||
      fail "$name: parent base is not the release line"
  else
    grep -qx "is_release=false" "$output" || fail "$name: main route flagged as release"
  fi

  echo "PASS: $name"
}

run_override_error_case() {
  local name="$1"
  local target="$2"
  local override="$3"
  local case_dir="${TMP_DIR}/${name}"
  mkdir -p "$case_dir"
  if env \
      PATH="${MOCK_BIN}:$PATH" \
      GITHUB_REPOSITORY=mono/SkiaSharp \
      GITHUB_SHA=trigger-sha \
      GITHUB_REF=refs/heads/main \
      TEST_MAIN_MS=157 \
      TEST_RELEASE_BRANCH=release/4.157.x \
      TEST_RELEASE_SHA=release-sha \
      TEST_SYNC_BRANCH="" \
      TEST_SYNC_SHA=sync-sha \
      TEST_SKIA_BASE_BRANCH=release/4.157.x \
      TEST_BASE_SHA=base-sha \
      TEST_UPSTREAM_SHA="upstream-${name}" \
      TEST_BASE_BEHIND=1 \
      TEST_SYNC_BEHIND=0 \
      TEST_COMPARE_LOG="${case_dir}/compare" \
      bash "$DETECTOR" --output "${case_dir}/output" --target "$target" --base-branch "$override" \
      >"${case_dir}/log" 2>&1; then
    fail "$name: detector unexpectedly succeeded"
  fi
  echo "PASS: $name"
}

run_override_infers_target_case() {
  local name=release-override-infers-target
  local case_dir="${TMP_DIR}/${name}"
  mkdir -p "$case_dir"
  env \
      PATH="${MOCK_BIN}:$PATH" GITHUB_REPOSITORY=mono/SkiaSharp GITHUB_SHA=trigger-sha \
      GITHUB_REF=refs/heads/main GITHUB_RUN_NUMBER=3 TEST_MAIN_MS=157 \
      TEST_RELEASE_BRANCH=release/4.157.x TEST_RELEASE_SHA=release-sha TEST_SYNC_BRANCH="" \
      TEST_SYNC_SHA=sync-sha TEST_SKIA_BASE_BRANCH=release/4.157.x TEST_BASE_SHA=base-sha \
      TEST_UPSTREAM_SHA=upstream-x TEST_BASE_BEHIND=1 TEST_SYNC_BEHIND=0 \
      TEST_COMPARE_LOG="${case_dir}/compare" \
      bash "$DETECTOR" --output "${case_dir}/output" --target "" --base-branch release/4.157.x \
      >"${case_dir}/log" 2>&1 || { cat "${case_dir}/log" >&2; fail "$name: detector failed"; }
  grep -qx "target=157" "${case_dir}/output" || fail "$name: target not inferred"
  grep -qx "skia_base_branch=release/4.157.x" "${case_dir}/output" || fail "$name: wrong skia base"
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
# Servicing for the milestone main still reports routes to its release line (m157).
run_case servicing-same-milestone-release 157 157 release/4.157.x "" 2 0 false release/4.157.x
# A real bump (m158 while main is m157) goes to main even though a release line exists.
run_case bump-ignores-other-release-line 158 157 release/4.157.x "" 2 0 false skiasharp
run_case explicit-release-override 157 157 release/4.157.x "" 2 0 false release/4.157.x release/4.157.x
run_case non-release-override-unchanged 157 157 "" "" 2 0 false skiasharp main skia-sync/m157-main
run_override_infers_target_case
run_override_error_case release-override-wrong-milestone 158 release/4.157.x
run_override_error_case missing-override-branch 157 release/4.999.x
run_compare_failure_case

echo "All skia-sync detector tests passed."
