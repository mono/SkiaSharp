#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
PUSH_SCRIPT="${SCRIPT_DIR}/../skia-sync-push-prs.sh"
WORKFLOW_LOCK="${SCRIPT_DIR}/../../workflows/auto-skia-sync.lock.yml"
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

verify_compiled_release_base_config() {
  python3 - "$WORKFLOW_LOCK" <<'PY'
import json
import re
import sys


lock_path = sys.argv[1]
lines = open(lock_path, encoding="utf-8").read().splitlines()

agent_headers = [index for index, line in enumerate(lines) if line == "  agent:"]
if len(agent_headers) != 1:
    raise SystemExit(f"expected one compiled agent job, found {len(agent_headers)}")

agent_start = agent_headers[0]
agent_end = next(
    (
        index
        for index in range(agent_start + 1, len(lines))
        if re.fullmatch(r"  [A-Za-z0-9_-]+:", lines[index])
    ),
    len(lines),
)
agent_lines = lines[agent_start:agent_end]

needs_headers = [index for index, line in enumerate(agent_lines) if line == "    needs:"]
if len(needs_headers) != 1:
    raise SystemExit(f"expected one compiled agent needs list, found {len(needs_headers)}")

needs_start = needs_headers[0] + 1
agent_needs = []
for line in agent_lines[needs_start:]:
    match = re.fullmatch(r"      - ([A-Za-z0-9_-]+)", line)
    if match:
        agent_needs.append(match.group(1))
    else:
        break
if agent_needs != ["activation", "pre_activation"]:
    raise SystemExit(f"compiled agent dependencies do not preserve activation and directly include pre_activation: {agent_needs}")

prefix = "GH_AW_SAFE_OUTPUTS_CONFIG: "
matches = [
    line.strip()[len(prefix):]
    for line in agent_lines
    if line.strip().startswith(prefix)
]
if len(matches) != 1:
    raise SystemExit(f"expected one emitted safe-output config in the agent job, found {len(matches)}")

serialized_config = json.loads(matches[0])
placeholder = "${{ needs.pre_activation.outputs.base_branch }}"
config = json.loads(serialized_config)
pull_request = config["create_pull_request"]
if pull_request.get("base_branch") != placeholder:
    raise SystemExit("compiled create_pull_request base_branch is not the resolved pre-activation base")
if pull_request.get("allowed_base_branches") != placeholder:
    raise SystemExit("compiled create_pull_request allowed_base_branches is not scoped to the resolved base")

release_base = "release/3.151.x"
runtime_config = json.loads(serialized_config.replace(placeholder, release_base))
runtime_pull_request = runtime_config["create_pull_request"]
if runtime_pull_request["base_branch"] != release_base:
    raise SystemExit("release/manual base did not flow into emitted base_branch")
if runtime_pull_request["allowed_base_branches"] != release_base:
    raise SystemExit("release/manual base allowlist is broader than the resolved branch")
PY
  echo "PASS: compiled-release-base-config"
}

verify_compiled_release_base_config

record >"$SIGNAL_FILE"
expect_success valid

BASE_BRANCH="release/3.151.x"
BASE_SHA="abcdef0123456789abcdef0123456789abcdef01"
record create_pull_request "$HEAD_BRANCH" "$BASE_BRANCH" mono/SkiaSharp "$BASE_SHA" >"$SIGNAL_FILE"
expect_success release-base
BASE_BRANCH="main"
BASE_SHA="0123456789abcdef0123456789abcdef01234567"

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
