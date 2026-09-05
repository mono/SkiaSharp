#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
PUSH_SCRIPT="${SCRIPT_DIR}/../skia-sync-push-prs.sh"
TMP_DIR=$(mktemp -d)
SIGNAL_FILE="${TMP_DIR}/outputs.jsonl"
LOG_FILE="${TMP_DIR}/output.log"
HEAD_BRANCH="skia-sync/m152"
BASE_BRANCH="main"
BASE_SHA="0123456789abcdef0123456789abcdef01234567"
trap 'rm -rf "$TMP_DIR"' EXIT

fail() {
  echo "FAIL: $*" >&2
  exit 1
}

record() {
  jq -cn \
    --arg type "${1:-create_pull_request}" \
    --arg branch "${2:-$HEAD_BRANCH}" \
    --arg base_branch "${3:-$BASE_BRANCH}" \
    --arg head_repo "${4:-mono/SkiaSharp}" \
    --arg base_commit "${5:-$BASE_SHA}" \
    --arg title "${6:-[skia-sync] Update skia to milestone 152}" \
    '{
      type: $type,
      branch: $branch,
      base_branch: $base_branch,
      head_repo: $head_repo,
      base_commit: $base_commit,
      title: $title,
      body: "Validated staged completion signal."
    }'
}

run_validator() {
  env -u GH_TOKEN \
    SKIA_SYNC_COMPLETION_SIGNAL_FILE="$SIGNAL_FILE" \
    SKIA_SYNC_HEAD_BRANCH="$HEAD_BRANCH" \
    SKIA_SYNC_BASE_BRANCH="$BASE_BRANCH" \
    SKIA_SYNC_PARENT_BASE_SHA="$BASE_SHA" \
    SKIA_SYNC_VALIDATE_DELIVERY_SIGNAL_ONLY=true \
    bash "$PUSH_SCRIPT" >"$LOG_FILE" 2>&1
}

expect_success() {
  local name="$1"
  if ! run_validator; then
    cat "$LOG_FILE" >&2
    fail "$name: validator rejected a valid completion signal"
  fi
  echo "PASS: $name"
}

expect_failure() {
  local name="$1"
  local expected="$2"
  if run_validator; then
    fail "$name: validator unexpectedly accepted the completion signal"
  fi
  if ! grep -Fq "$expected" "$LOG_FILE"; then
    cat "$LOG_FILE" >&2
    fail "$name: missing expected error '$expected'"
  fi
  echo "PASS: $name"
}

record >"$SIGNAL_FILE"
expect_success valid

rm "$SIGNAL_FILE"
expect_failure missing "must be a nonempty regular, non-symlink file"

: >"$SIGNAL_FILE"
expect_failure empty "must be a nonempty regular, non-symlink file"
rm "$SIGNAL_FILE"

record >"${TMP_DIR}/real-output.jsonl"
ln -s "${TMP_DIR}/real-output.jsonl" "$SIGNAL_FILE"
expect_failure symlink "must be a nonempty regular, non-symlink file"
rm "$SIGNAL_FILE"

mkdir "$SIGNAL_FILE"
expect_failure non-regular "must be a nonempty regular, non-symlink file"
rmdir "$SIGNAL_FILE"

printf '{not-json}\n' >"$SIGNAL_FILE"
expect_failure malformed-json "line 1 is not exactly one JSON object"

{
  record
  printf '\n'
} >"$SIGNAL_FILE"
expect_failure trailing-newline-record "Expected exactly one accepted terminal sync record, found 2"

record | tr -d '\n' >"$SIGNAL_FILE"
printf 'trailing-garbage' >>"$SIGNAL_FILE"
expect_failure trailing-garbage "line 1 is not exactly one JSON object"

record >"$SIGNAL_FILE"
printf '\0trailing-garbage' >>"$SIGNAL_FILE"
expect_failure nul-trailing-garbage "contains a NUL byte"

record | tr -d '\n' >"$SIGNAL_FILE"
printf '\t' >>"$SIGNAL_FILE"
expect_failure ambiguous-control-byte "contains an ambiguous control byte"

{
  record
  record
} >"$SIGNAL_FILE"
expect_failure duplicate "Expected exactly one accepted terminal sync record, found 2"

{
  record
  record noop
} >"$SIGNAL_FILE"
expect_failure extra-terminal-type "Expected exactly one accepted terminal sync record, found 2"

record create_pull_request wrong-branch >"$SIGNAL_FILE"
expect_failure branch-mismatch "does not match SKIA_SYNC_HEAD_BRANCH"

record create_pull_request $'skia-sync/m152\n' >"$SIGNAL_FILE"
expect_failure trailing-newline-branch "branch contains an ambiguous control character"

record create_pull_request $'skia-sync/\nm152' >"$SIGNAL_FILE"
expect_failure embedded-newline-branch "branch contains an ambiguous control character"

record create_pull_request "$HEAD_BRANCH" release/3.152.x >"$SIGNAL_FILE"
expect_failure base-branch-mismatch "does not match SKIA_SYNC_BASE_BRANCH"

record create_pull_request "$HEAD_BRANCH" "$BASE_BRANCH" mono/skia >"$SIGNAL_FILE"
expect_failure head-repo-mismatch "is not mono/SkiaSharp"

record create_pull_request "$HEAD_BRANCH" "$BASE_BRANCH" mono/SkiaSharp deadbeef >"$SIGNAL_FILE"
expect_failure base-commit-mismatch "does not match SKIA_SYNC_PARENT_BASE_SHA"

record create_pull_request "$HEAD_BRANCH" "$BASE_BRANCH" mono/SkiaSharp "$BASE_SHA" "Update skia" >"$SIGNAL_FILE"
expect_failure title-prefix-mismatch "title must start with [skia-sync]"

record create_pull_request "$HEAD_BRANCH" "$BASE_BRANCH" mono/SkiaSharp "$BASE_SHA" $'[skia-sync]\nUpdate skia' >"$SIGNAL_FILE"
expect_failure title-control-character "title contains an ambiguous control character"

record noop >"$SIGNAL_FILE"
expect_failure terminal-type-mismatch "Expected terminal sync record type create_pull_request"

echo "All skia-sync delivery signal tests passed."
