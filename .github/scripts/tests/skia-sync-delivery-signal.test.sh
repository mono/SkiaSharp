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
    raise SystemExit(
        "compiled agent dependencies do not preserve activation and directly include "
        f"pre_activation: {agent_needs}"
    )

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

safe_outputs_headers = [index for index, line in enumerate(lines) if line == "  safe_outputs:"]
if len(safe_outputs_headers) != 1:
    raise SystemExit(f"expected one compiled safe_outputs job, found {len(safe_outputs_headers)}")

safe_outputs_start = safe_outputs_headers[0]
safe_outputs_end = next(
    (
        index
        for index in range(safe_outputs_start + 1, len(lines))
        if re.fullmatch(r"  [A-Za-z0-9_-]+:", lines[index])
    ),
    len(lines),
)
safe_outputs_lines = lines[safe_outputs_start:safe_outputs_end]
safe_outputs_needs = []
safe_outputs_needs_start = safe_outputs_lines.index("    needs:") + 1
for line in safe_outputs_lines[safe_outputs_needs_start:]:
    match = re.fullmatch(r"      - ([A-Za-z0-9_-]+)", line)
    if match:
        safe_outputs_needs.append(match.group(1))
    else:
        break
expected_safe_outputs_needs = ["activation", "agent", "detection", "pre_activation"]
if safe_outputs_needs != expected_safe_outputs_needs:
    raise SystemExit(
        "compiled safe_outputs dependencies do not preserve generated gates and directly "
        f"include pre_activation: {safe_outputs_needs}"
    )

handler_prefix = "GH_AW_SAFE_OUTPUTS_HANDLER_CONFIG: "
handler_matches = [
    line.strip()[len(handler_prefix):]
    for line in safe_outputs_lines
    if line.strip().startswith(handler_prefix)
]
if len(handler_matches) != 1:
    raise SystemExit(f"expected one emitted safe-output handler config, found {len(handler_matches)}")
handler_config = json.loads(json.loads(handler_matches[0]))
handler_pull_request = handler_config["create_pull_request"]
if handler_pull_request.get("base_branch") != placeholder:
    raise SystemExit("safe_outputs handler base_branch cannot resolve the pre-activation base")
if handler_pull_request.get("allowed_base_branches") != placeholder:
    raise SystemExit("safe_outputs handler allowlist cannot resolve the pre-activation base")

release_base = "release/3.151.x"
runtime_config = json.loads(serialized_config.replace(placeholder, release_base))
runtime_pull_request = runtime_config["create_pull_request"]
if runtime_pull_request["base_branch"] != release_base:
    raise SystemExit("release/manual base did not flow into emitted base_branch")
if runtime_pull_request["allowed_base_branches"] != release_base:
    raise SystemExit("release/manual base allowlist is broader than the resolved branch")
runtime_handler_config = json.loads(json.loads(handler_matches[0]).replace(placeholder, release_base))
runtime_handler_pull_request = runtime_handler_config["create_pull_request"]
if runtime_handler_pull_request["base_branch"] != release_base:
    raise SystemExit("release/manual base did not flow into the safe_outputs handler")
if runtime_handler_pull_request["allowed_base_branches"] != release_base:
    raise SystemExit("safe_outputs handler release/manual allowlist is broader than the resolved branch")

compiled = "\n".join(lines)
required_hardening = {
    "two POSIX-shell post steps": compiled.count("shell: /bin/sh -e {0}") >= 2,
    "two BASH_ENV resets": compiled.count("BASH_ENV: /dev/null") >= 2,
    "loader-variable reset": compiled.count('LD_PRELOAD: ""') >= 2,
    "sanitized finalizer": "GIT_NO_REPLACE_OBJECTS=1" in compiled
    and "reject_unsafe_repository" in compiled,
    "sanitized credentialed launch": "exec /usr/bin/env -i" in compiled
    and "GH_TOKEN=" in compiled,
}
for contract, present in required_hardening.items():
    if not present:
        raise SystemExit(f"compiled workflow is missing {contract}")
PY
  echo "PASS: compiled-release-base-config"
}

verify_delivery_source_hardening() {
  python3 - "$PUSH_SCRIPT" <<'PY'
import sys


source = open(sys.argv[1], encoding="utf-8").read()

if source.count("command -p python3 -I") != 2:
    raise SystemExit("every trusted Python invocation must use isolated mode")

signal_gate = source.index("\nvalidate_delivery_signal\n")
token_capture = source.index('readonly WRITE_TOKEN="$GH_TOKEN"')
token_removal = source.index("unset GH_TOKEN")
artifact_gate = source.index('\nrequired_file "$SKIA_SUMMARY_FILE"')
if not signal_gate < token_capture < token_removal < artifact_gate:
    raise SystemExit("write credentials are not removed immediately after signal validation")

if "remote set-url" in source:
    raise SystemExit("credentialed delivery must not use agent-controlled remote metadata")
if source.count("GIT_CONFIG_NOSYSTEM=1") != 2:
    raise SystemExit("trusted Git commands do not consistently disable system configuration")
if source.count("GIT_CONFIG_GLOBAL=/dev/null") != 2:
    raise SystemExit("trusted Git commands do not consistently disable global configuration")
if source.count("-c core.hooksPath=/dev/null") != 2:
    raise SystemExit("trusted Git commands do not consistently disable hooks")
if source.count("-c credential.helper=") != 2:
    raise SystemExit("trusted Git commands do not consistently disable credential helpers")
if '"${commit_sha}:refs/heads/${BRANCH}"' not in source:
    raise SystemExit("delivery does not push the immutable validated commit")
if 'push_branch "$TRUSTED_SKIA_REPO" mono/skia "$SKIA_VALIDATED_HEAD_SHA"' not in source:
    raise SystemExit("mono/skia delivery is not bound to its validated head")
if 'push_branch "$TRUSTED_SS_REPO" mono/SkiaSharp "$SS_VALIDATED_HEAD_SHA"' not in source:
    raise SystemExit("mono/SkiaSharp delivery is not bound to its validated head")
if '${SS_VALIDATED_HEAD_SHA}:cgmanifest.json' not in source:
    raise SystemExit("manifest validation is not bound to the immutable SkiaSharp head")
if '--merged-head "$SKIA_VALIDATED_HEAD_SHA"' not in source:
    raise SystemExit("fork audit is not bound to the immutable Skia head")
if 'ls-tree "$SS_VALIDATED_HEAD_SHA" externals/skia' not in source:
    raise SystemExit("gitlink validation is not bound to the immutable SkiaSharp head")
if '"${RUNNER_TEMP:?RUNNER_TEMP is required}/skia-sync-delivery.XXXXXX"' not in source:
    raise SystemExit("trusted delivery workspace is not rooted in runner-owned temporary storage")
if '"${RUNTIME_DIR}/delivery.XXXXXX"' in source:
    raise SystemExit("trusted delivery workspace cannot be created in immutable runtime assets")
if 'command -p chmod 700 "$TRUSTED_DELIVERY_DIR"' not in source:
    raise SystemExit("trusted delivery workspace permissions are not explicitly private")
if "trap cleanup_trusted_delivery EXIT" not in source:
    raise SystemExit("trusted delivery workspace cleanup is not unconditional")
if 'command -p rm -rf -- "$TRUSTED_DELIVERY_DIR"' not in source:
    raise SystemExit("trusted delivery workspace cleanup is not path-bound")
if source.count('rev-parse "refs/heads/${HEAD_BRANCH}^{commit}"') != 2:
    raise SystemExit("validated heads are not resolved through explicit local branch refs")
if source.count("GIT_NO_REPLACE_OBJECTS=1") != 2:
    raise SystemExit("trusted Git paths do not consistently disable replacement refs")
if source.count("-c safe.bareRepository=all") != 2:
    raise SystemExit("trusted Git paths cannot operate on the isolated bare repositories")
if source.count('reject_replace_refs "$GITHUB_WORKSPACE') != 2:
    raise SystemExit("both source repositories are not checked for replacement refs")
if source.count('reject_unsafe_git_config "$GITHUB_WORKSPACE') != 2:
    raise SystemExit("both source repositories are not checked for command-bearing Git config")
if source.count('--upload-pack="$TRUSTED_GIT_UPLOAD_PACK"') != 2:
    raise SystemExit("exact-object copies are not forced through the trusted upload-pack wrapper")
if "Trusted delivery repository unexpectedly contains mutable refs." not in source:
    raise SystemExit("clean delivery repositories are not verified to be ref-free")
if 'show "${SS_VALIDATED_HEAD_SHA}:cgmanifest.json"' not in source or \
        '"$TRUSTED_SS_REPO"' not in source:
    raise SystemExit("manifest validation is not performed in the clean delivery repository")
if '--skia-root "$TRUSTED_SKIA_REPO"' not in source:
    raise SystemExit("fork validation is not performed in the clean delivery repository")
required_pr_identity = [
    '--base "$base"',
    "headRepositoryOwner.login == $owner",
    "headRepository.nameWithOwner == $repo",
    ".headRefName == $head",
    ".headRefOid == $sha",
    ".baseRefName == $base",
]
for identity_check in required_pr_identity:
    if identity_check not in source:
        raise SystemExit(f"existing PR reuse is missing identity check: {identity_check}")
PY
  echo "PASS: delivery-source-hardening"
}

verify_conflicting_tag_resolution() {
  local repo="$TMP_DIR/conflicting-ref-repo"
  local branch="skia-sync/m152"
  local branch_sha
  local resolved_sha
  local tag_sha

  git init -q "$repo"
  git -C "$repo" config user.name "Skia Sync Test"
  git -C "$repo" config user.email "skia-sync@example.invalid"
  git -C "$repo" commit -q --allow-empty -m tag-target
  tag_sha=$(git -C "$repo" rev-parse HEAD)
  git -C "$repo" tag "$branch"
  git -C "$repo" switch -q -c "$branch"
  git -C "$repo" commit -q --allow-empty -m branch-target
  branch_sha=$(git -C "$repo" rev-parse "refs/heads/${branch}^{commit}")
  resolved_sha=$(git -C "$repo" rev-parse "refs/heads/${branch}^{commit}")

  if [[ "$resolved_sha" != "$branch_sha" || "$resolved_sha" == "$tag_sha" ]]; then
    echo "FAIL: conflicting-tag-resolution"
    exit 1
  fi
  echo "PASS: conflicting-tag-resolution"
}

verify_replacement_ref_rejection() {
  local repo="$TMP_DIR/replacement-ref-repo"
  local original_sha
  local replacement_sha
  local replaced_subject
  local original_subject
  local clean_subject
  local clean_repo="$TMP_DIR/replacement-ref-clean.git"
  local replace_refs

  git init -q "$repo"
  git -C "$repo" config user.name "Skia Sync Test"
  git -C "$repo" config user.email "skia-sync@example.invalid"
  git -C "$repo" commit -q --allow-empty -m original
  original_sha=$(git -C "$repo" rev-parse HEAD)
  git -C "$repo" commit -q --allow-empty -m replacement
  replacement_sha=$(git -C "$repo" rev-parse HEAD)
  git -C "$repo" replace "$original_sha" "$replacement_sha"

  replaced_subject=$(git -C "$repo" show -s --format=%s "$original_sha")
  original_subject=$(GIT_NO_REPLACE_OBJECTS=1 git -C "$repo" show -s --format=%s "$original_sha")
  replace_refs=$(GIT_NO_REPLACE_OBJECTS=1 git -C "$repo" for-each-ref --format='%(refname)' refs/replace)
  git init -q --bare "$clean_repo"
  GIT_NO_REPLACE_OBJECTS=1 git -C "$clean_repo" \
    -c safe.bareRepository=all fetch -q --no-tags "$repo" "$original_sha"
  clean_subject=$(git -C "$clean_repo" -c safe.bareRepository=all show -s --format=%s FETCH_HEAD)
  if [[ "$replaced_subject" != "replacement" ||
        "$original_subject" != "original" ||
        "$clean_subject" != "original" ||
        "$replace_refs" != refs/replace/* ]]; then
    echo "FAIL: replacement-ref-rejection"
    exit 1
  fi
  echo "PASS: replacement-ref-rejection"
}

verify_source_git_helper_isolation() {
  local repo="$TMP_DIR/source-helper-repo"
  local clean_repo="$TMP_DIR/source-helper-clean.git"
  local marker="$TMP_DIR/source-helper-ran"
  local pack_objects="$TMP_DIR/trusted-pack-objects.sh"
  local upload_pack="$TMP_DIR/trusted-upload-pack.sh"
  local commit_sha
  local fetched_sha
  local refs

  git init -q "$repo"
  git -C "$repo" config user.name "Skia Sync Test"
  git -C "$repo" config user.email "skia-sync@example.invalid"
  git -C "$repo" commit -q --allow-empty -m source
  commit_sha=$(git -C "$repo" rev-parse HEAD)
  git -C "$repo" config uploadpack.packObjectsHook \
    "printf poisoned >'$marker'; git pack-objects --revs --stdout"

  # shellcheck disable=SC2016 # These variables expand only when Git invokes the fixture helper.
  printf '%s\n' \
    '#!/bin/sh' \
    'test "$1" = git && test "$2" = pack-objects' \
    'shift' \
    'exec /usr/bin/git "$@"' >"$pack_objects"
  printf '%s\n' \
    '#!/bin/sh' \
    "exec /usr/bin/git -c uploadpack.packObjectsHook='$pack_objects' upload-pack \"\$@\"" \
    >"$upload_pack"
  chmod 700 "$pack_objects" "$upload_pack"

  git init -q --bare "$clean_repo"
  GIT_NO_REPLACE_OBJECTS=1 git -C "$clean_repo" -c safe.bareRepository=all \
    fetch -q --no-tags --upload-pack="$upload_pack" "$repo" "$commit_sha"
  fetched_sha=$(git -C "$clean_repo" -c safe.bareRepository=all rev-parse FETCH_HEAD)
  refs=$(git -C "$clean_repo" -c safe.bareRepository=all for-each-ref --format='%(refname)')
  if [[ -e "$marker" || "$fetched_sha" != "$commit_sha" || -n "$refs" ]]; then
    echo "FAIL: source-git-helper-isolation"
    exit 1
  fi
  echo "PASS: source-git-helper-isolation"
}

verify_git_hook_isolation() {
  local repo="$TMP_DIR/hook-isolation-repo"
  local hooks="$TMP_DIR/malicious-hooks"
  local marker="$TMP_DIR/git-hook-ran"

  git init -q "$repo"
  git -C "$repo" config user.name "Skia Sync Test"
  git -C "$repo" config user.email "skia-sync@example.invalid"
  mkdir "$hooks"
  printf '%s\n' '#!/bin/sh' "printf poisoned >'$marker'" >"$hooks/pre-commit"
  chmod 700 "$hooks/pre-commit"
  git -C "$repo" config core.hooksPath "$hooks"
  git -C "$repo" -c core.hooksPath=/dev/null commit -q --allow-empty -m isolated

  if [[ -e "$marker" ]]; then
    echo "FAIL: git-hook-isolation"
    exit 1
  fi
  echo "PASS: git-hook-isolation"
}

verify_pr_identity_selection() {
  local fixture="$TMP_DIR/pr-identity.json"
  local filter="$TMP_DIR/pr-identity.jq"
  local selected

  jq -n '[
    {
      number: 1,
      headRepositoryOwner: {login: "attacker"},
      headRepository: {name: "SkiaSharp", nameWithOwner: "attacker/SkiaSharp"},
      headRefName: "skia-sync/m152",
      headRefOid: "validated",
      baseRefName: "main"
    },
    {
      number: 2,
      headRepositoryOwner: {login: "mono"},
      headRepository: {name: "SkiaSharp", nameWithOwner: "mono/SkiaSharp"},
      headRefName: "skia-sync/m152",
      headRefOid: "validated",
      baseRefName: "release/incorrect"
    },
    {
      number: 3,
      headRepository: {name: "SkiaSharp", nameWithOwner: "mono/SkiaSharp"},
      headRefName: "skia-sync/m152",
      headRefOid: "validated",
      baseRefName: "main"
    },
    {
      number: 4,
      headRepositoryOwner: {login: "mono"},
      headRepository: {name: "SkiaSharp", nameWithOwner: "mono/SkiaSharp"},
      headRefName: "skia-sync/m152",
      headRefOid: "validated",
      baseRefName: "main"
    }
  ]' >"$fixture"
  cat >"$filter" <<'JQ'
[
  .[]
  | select(
      .headRepositoryOwner.login == $owner
      and .headRepository.name == $repository_name
      and .headRepository.nameWithOwner == $repo
      and .headRefName == $head
      and .headRefOid == $sha
      and .baseRefName == $base
    )
]
| if length > 1 then error("multiple exact delivery PRs") else .[0].number // empty end
JQ
  selected=$(jq -r \
    --arg repo mono/SkiaSharp \
    --arg owner mono \
    --arg repository_name SkiaSharp \
    --arg head skia-sync/m152 \
    --arg base main \
    --arg sha validated \
    -f "$filter" "$fixture")
  if [[ "$selected" != 4 ]]; then
    echo "FAIL: pr-identity-selection"
    exit 1
  fi
  if jq -e '[.[] | select(.number == 4)] + [.[] | select(.number == 4) | .number = 5]' "$fixture" |
      jq -r \
        --arg repo mono/SkiaSharp \
        --arg owner mono \
        --arg repository_name SkiaSharp \
        --arg head skia-sync/m152 \
        --arg base main \
        --arg sha validated \
        -f "$filter" >/dev/null 2>&1; then
    echo "FAIL: pr-identity-selection"
    exit 1
  fi
  echo "PASS: pr-identity-selection"
}

verify_shell_startup_isolation() {
  local marker="$TMP_DIR/shell-startup-poisoned"
  local poison="$TMP_DIR/shell-startup-poison.sh"
  local probe="$TMP_DIR/shell-startup-probe.sh"
  local inner="$TMP_DIR/shell-startup-inner.sh"

  printf '%s\n' '#!/bin/sh' "printf poisoned >'$marker'" >"$poison"
  printf '%s\n' \
    '#!/bin/bash' \
    "test -z \"\${BASH_ENV:-}\${ENV:-}\${LD_PRELOAD:-}\"" \
    >"$inner"
  printf '%s\n' \
    "exec /usr/bin/env -i PATH=/usr/bin:/bin /bin/bash --noprofile --norc '$inner'" \
    >"$probe"
  chmod 700 "$poison" "$probe" "$inner"

  BASH_ENV="$poison" ENV="$poison" LD_PRELOAD="" /bin/sh -e "$probe"
  if [[ -e "$marker" ]]; then
    echo "FAIL: shell-startup-isolation"
    exit 1
  fi
  echo "PASS: shell-startup-isolation"
}

verify_compiled_release_base_config
verify_delivery_source_hardening
verify_conflicting_tag_resolution
verify_replacement_ref_rejection
verify_source_git_helper_isolation
verify_git_hook_isolation
verify_pr_identity_selection
verify_shell_startup_isolation

record >"$SIGNAL_FILE"
expect_success valid

SHADOW_DIR="${TMP_DIR}/python-shadow"
SHADOW_MARKER="${TMP_DIR}/python-shadow-loaded"
MISSING_ARTIFACT_DIR="${TMP_DIR}/missing-artifacts"
mkdir "$SHADOW_DIR"
mkdir "$MISSING_ARTIFACT_DIR"
cat >"${SHADOW_DIR}/json.py" <<'PY'
import os

with open(os.environ["SHADOW_MARKER"], "w", encoding="utf-8") as marker:
    marker.write(os.environ.get("GH_TOKEN", "missing"))
raise RuntimeError("checkout-local json module was imported")
PY
cat >"${SHADOW_DIR}/sitecustomize.py" <<'PY'
import os

with open(os.environ["SHADOW_MARKER"], "w", encoding="utf-8") as marker:
    marker.write(os.environ.get("GH_TOKEN", "missing"))
PY
record >"$SIGNAL_FILE"
if (
  cd "$SHADOW_DIR"
  env \
    GH_TOKEN="test-write-token" \
    PYTHONPATH="$SHADOW_DIR" \
    SHADOW_MARKER="$SHADOW_MARKER" \
    SKIA_SYNC_ARTIFACT_DIR="$MISSING_ARTIFACT_DIR" \
    SKIA_SYNC_COMPLETION_SIGNAL_FILE="$SIGNAL_FILE" \
    SKIA_SYNC_HEAD_BRANCH="$HEAD_BRANCH" \
    SKIA_SYNC_BASE_BRANCH="$BASE_BRANCH" \
    SKIA_SYNC_PARENT_BASE_SHA="$BASE_SHA" \
    bash "$PUSH_SCRIPT" >"$LOG_FILE" 2>&1
); then
  fail "isolated-python: expected the later missing-artifact gate to reject"
fi
if ! grep -Fq "Required sync artifact is missing or empty" "$LOG_FILE"; then
  cat "$LOG_FILE" >&2
  fail "isolated-python: validation did not reach the later artifact gate"
fi
if [[ -e "$SHADOW_MARKER" ]]; then
  fail "isolated-python: checkout-local Python startup/import code observed GH_TOKEN"
fi
echo "PASS: isolated-python"

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

record | jq -c '.body = "Line one\nLine two\tTabbed\r\n"' >"$SIGNAL_FILE"
expect_success body-formatting

record | jq -c '.body = "Unexpected DEL: \u007f"' >"$SIGNAL_FILE"
expect_failure body-del-control "record contains an ambiguous control character"

record | jq -c '.body = "Unexpected C1: \u0085"' >"$SIGNAL_FILE"
expect_failure body-c1-control "record contains an ambiguous control character"

record | jq -c '.extra = {"nested": ["Unexpected C1: \u0085"]}' >"$SIGNAL_FILE"
expect_failure nested-c1-control "record contains an ambiguous control character"

record | jq -c '. + {"Unexpected DEL \u007f key": "value"}' >"$SIGNAL_FILE"
expect_failure key-del-control "record contains an ambiguous control character"

record noop >"$SIGNAL_FILE"
expect_failure terminal-type-mismatch "Expected terminal sync record type create_pull_request"

echo "All skia-sync delivery signal tests passed."
