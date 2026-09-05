#!/usr/bin/env bash
#
# Push the two agent-produced branches and create/update their draft PRs.
# The agent writes reports; this script owns complete PR templates and links.

set -euo pipefail

ARTIFACT_DIR="${SKIA_SYNC_ARTIFACT_DIR:-/tmp/gh-aw/agent}"
RUNTIME_DIR="${SKIA_SYNC_RUNTIME_DIR:-/tmp/gh-aw}"
SKILL_DIR="${SKIA_SYNC_SKILL_DIR:-$RUNTIME_DIR/update-skia}"
readonly ARTIFACT_DIR RUNTIME_DIR SKILL_DIR
SKIA_SUMMARY_FILE="$ARTIFACT_DIR/skia-sync-skia-summary.md"
SS_SUMMARY_FILE="$ARTIFACT_DIR/skia-sync-skiasharp-summary.md"

signal_error() {
  echo "::error::$*"
  return 1
}

validate_delivery_signal() {
  local path="$SKIA_SYNC_COMPLETION_SIGNAL_FILE"

  command -p python3 -I - "$path" <<'PY'
import json
import os
import stat
import sys
import unicodedata


def reject(message):
    print(f"::error::{message}", file=sys.stderr)
    raise SystemExit(1)


path = sys.argv[1]
flags = os.O_RDONLY | os.O_NONBLOCK | os.O_NOFOLLOW
if hasattr(os, "O_CLOEXEC"):
    flags |= os.O_CLOEXEC

try:
    descriptor = os.open(path, flags)
except OSError:
    reject(f"The sync completion signal must be a nonempty regular, non-symlink file: {path}")

metadata = os.fstat(descriptor)
if not stat.S_ISREG(metadata.st_mode):
    os.close(descriptor)
    reject(f"The sync completion signal must be a nonempty regular, non-symlink file: {path}")

with os.fdopen(descriptor, "rb") as signal_file:
    content = signal_file.read()

if not content:
    reject(f"The sync completion signal must be a nonempty regular, non-symlink file: {path}")
if b"\0" in content:
    reject("The sync completion signal contains a NUL byte.")

for offset, byte in enumerate(content):
    if byte < 0x20 and byte != 0x0A:
        reject(f"The sync completion signal contains an ambiguous control byte at offset {offset}.")

# JSONL permits one final LF. Any LF remaining after removing it denotes an
# additional or empty physical record and is rejected before JSON decoding.
record_bytes = content[:-1] if content.endswith(b"\n") else content
if b"\n" in record_bytes:
    record_count = record_bytes.count(b"\n") + 1
    reject(f"Expected exactly one accepted terminal sync record, found {record_count}.")
if not record_bytes:
    reject("Expected exactly one accepted terminal sync record, found 0.")

try:
    record_text = record_bytes.decode("utf-8")
except UnicodeDecodeError:
    reject("The sync completion signal record is not valid UTF-8.")


def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f"duplicate key: {key}")
        result[key] = value
    return result


try:
    record = json.loads(
        record_text,
        object_pairs_hook=unique_object,
        parse_constant=lambda value: (_ for _ in ()).throw(ValueError(f"invalid constant: {value}")),
    )
except (json.JSONDecodeError, ValueError):
    reject("Sync completion signal line 1 is not exactly one JSON object.")

if type(record) is not dict:
    reject("Sync completion signal line 1 is not exactly one JSON object.")


def reject_ambiguous_controls(value, path=()):
    if type(value) is str:
        for character in value:
            if unicodedata.category(character) == "Cc":
                if path == ("body",) and character in "\t\n\r":
                    continue
                reject("The accepted create_pull_request record contains an ambiguous control character.")
    elif type(value) is list:
        for index, item in enumerate(value):
            reject_ambiguous_controls(item, path + (index,))
    elif type(value) is dict:
        for key, item in value.items():
            reject_ambiguous_controls(key)
            reject_ambiguous_controls(item, path + (key,))


def require_clean_string(field):
    value = record.get(field)
    if type(value) is not str:
        reject(f"The accepted create_pull_request record has no string {field}.")
    if any(unicodedata.category(character) == "Cc" for character in value):
        reject(f"The accepted create_pull_request record {field} contains an ambiguous control character.")
    return value


record_type = record.get("type")
if type(record_type) is not str:
    reject("The accepted terminal sync record has no string type.")
if any(unicodedata.category(character) == "Cc" for character in record_type):
    reject("The accepted terminal sync record type contains an ambiguous control character.")
if record_type != "create_pull_request":
    reject(f"Expected terminal sync record type create_pull_request, found '{record_type}'.")

expected = {
    "branch": (os.environ["SKIA_SYNC_HEAD_BRANCH"], "SKIA_SYNC_HEAD_BRANCH"),
    "base_branch": (os.environ["SKIA_SYNC_BASE_BRANCH"], "SKIA_SYNC_BASE_BRANCH"),
    "head_repo": ("mono/SkiaSharp", None),
    "base_commit": (os.environ["SKIA_SYNC_PARENT_BASE_SHA"], "SKIA_SYNC_PARENT_BASE_SHA"),
}
for field, (expected_value, workflow_name) in expected.items():
    value = require_clean_string(field)
    if value != expected_value:
        if field == "head_repo":
            reject(f"Completion signal head_repo '{value}' is not mono/SkiaSharp.")
        reject(f"Completion signal {field} '{value}' does not match {workflow_name}.")

title = require_clean_string("title")
if not title.startswith("[skia-sync]"):
    reject("Completion signal title must start with [skia-sync].")

reject_ambiguous_controls(record)
PY
}

: "${SKIA_SYNC_COMPLETION_SIGNAL_FILE:?SKIA_SYNC_COMPLETION_SIGNAL_FILE is required}"
: "${SKIA_SYNC_HEAD_BRANCH:?SKIA_SYNC_HEAD_BRANCH is required}"
: "${SKIA_SYNC_BASE_BRANCH:?SKIA_SYNC_BASE_BRANCH is required}"
: "${SKIA_SYNC_PARENT_BASE_SHA:?SKIA_SYNC_PARENT_BASE_SHA is required}"
validate_delivery_signal

if [[ "${SKIA_SYNC_VALIDATE_DELIVERY_SIGNAL_ONLY:-false}" == "true" ]]; then
  if [[ -n "${GH_TOKEN:-}" ]]; then
    signal_error "Delivery-signal-only validation is unavailable when write credentials are present."
    exit 1
  fi
  exit 0
fi

: "${GH_TOKEN:?GH_TOKEN is required}"
readonly WRITE_TOKEN="$GH_TOKEN"
unset GH_TOKEN

required_file() {
  local path="$1"
  if [[ ! -s "$path" ]]; then
    echo "::error::Required sync artifact is missing or empty: $path"
    exit 1
  fi
}

required_file "$SKIA_SUMMARY_FILE"
required_file "$SS_SUMMARY_FILE"
required_file "$ARTIFACT_DIR/skia-breaking-change-analysis.md"
required_file "$ARTIFACT_DIR/skia-validation-review.md"
required_file "$ARTIFACT_DIR/skia-dependency-decisions.md"
required_file "$ARTIFACT_DIR/skia-dependency-changes.json"
required_file "$ARTIFACT_DIR/skia-fork-patch-audit.md"
required_file "$ARTIFACT_DIR/initial-test-output.txt"
required_file "$ARTIFACT_DIR/test-output.txt"
required_file "$ARTIFACT_DIR/test-exit-code.txt"

: "${SKIA_SYNC_TARGET:?SKIA_SYNC_TARGET is required}"
: "${SKIA_SYNC_CURRENT:?SKIA_SYNC_CURRENT is required}"
: "${SKIA_SYNC_UPSTREAM_REF:?SKIA_SYNC_UPSTREAM_REF is required}"
: "${SKIA_SYNC_IS_RELEASE:?SKIA_SYNC_IS_RELEASE is required}"
: "${SKIA_SYNC_SKIA_BASE_BRANCH:?SKIA_SYNC_SKIA_BASE_BRANCH is required}"
: "${SKIA_SYNC_SKIA_BASE_SHA:?SKIA_SYNC_SKIA_BASE_SHA is required}"
: "${SKIA_SYNC_BASE_UPSTREAM_SHA:?SKIA_SYNC_BASE_UPSTREAM_SHA is required}"
: "${SKIA_SYNC_TARGET_UPSTREAM_SHA:?SKIA_SYNC_TARGET_UPSTREAM_SHA is required}"
: "${GITHUB_WORKSPACE:?GITHUB_WORKSPACE is required}"

TARGET="$SKIA_SYNC_TARGET"
CURRENT="$SKIA_SYNC_CURRENT"
UPSTREAM_REF="$SKIA_SYNC_UPSTREAM_REF"
IS_RELEASE="$SKIA_SYNC_IS_RELEASE"
BASE_BRANCH="$SKIA_SYNC_BASE_BRANCH"
SKIA_BASE_BRANCH="$SKIA_SYNC_SKIA_BASE_BRANCH"
SKIA_BASE_SHA="$SKIA_SYNC_SKIA_BASE_SHA"
HEAD_BRANCH="$SKIA_SYNC_HEAD_BRANCH"
BASE_UPSTREAM_SHA="$SKIA_SYNC_BASE_UPSTREAM_SHA"
TARGET_UPSTREAM_SHA="$SKIA_SYNC_TARGET_UPSTREAM_SHA"
SS_BASE_SHA="$SKIA_SYNC_PARENT_BASE_SHA"

GIT_BIN=$(command -p -v git)
GH_BIN=$(command -p -v gh)
TRUSTED_DELIVERY_DIR=$(command -p mktemp -d \
  "${RUNNER_TEMP:?RUNNER_TEMP is required}/skia-sync-delivery.XXXXXX")
TRUSTED_GIT_HOME="$TRUSTED_DELIVERY_DIR/home"
TRUSTED_GIT_ASKPASS="$TRUSTED_DELIVERY_DIR/git-askpass.sh"
TRUSTED_GIT_PACK_OBJECTS="$TRUSTED_DELIVERY_DIR/git-pack-objects.sh"
TRUSTED_GIT_UPLOAD_PACK="$TRUSTED_DELIVERY_DIR/git-upload-pack.sh"
TRUSTED_SKIA_REPO="$TRUSTED_DELIVERY_DIR/skia.git"
TRUSTED_SS_REPO="$TRUSTED_DELIVERY_DIR/skiasharp.git"
readonly GIT_BIN GH_BIN TRUSTED_DELIVERY_DIR TRUSTED_GIT_HOME TRUSTED_GIT_ASKPASS
readonly TRUSTED_GIT_PACK_OBJECTS TRUSTED_GIT_UPLOAD_PACK
readonly TRUSTED_SKIA_REPO TRUSTED_SS_REPO
command -p chmod 700 "$TRUSTED_DELIVERY_DIR"

cleanup_trusted_delivery() {
  command -p rm -rf -- "$TRUSTED_DELIVERY_DIR"
}
trap cleanup_trusted_delivery EXIT

command -p mkdir -p "$TRUSTED_GIT_HOME"
# shellcheck disable=SC2016 # These variables expand only when Git invokes the generated helper.
printf '%s\n' \
  '#!/bin/sh' \
  'test "$1" = git && test "$2" = pack-objects' \
  'shift' \
  "exec \"$GIT_BIN\" \"\$@\"" >"$TRUSTED_GIT_PACK_OBJECTS"
printf '%s\n' \
  '#!/bin/sh' \
  "exec \"$GIT_BIN\" -c uploadpack.packObjectsHook=\"$TRUSTED_GIT_PACK_OBJECTS\" upload-pack \"\$@\"" \
  >"$TRUSTED_GIT_UPLOAD_PACK"
command -p chmod 700 "$TRUSTED_GIT_PACK_OBJECTS" "$TRUSTED_GIT_UPLOAD_PACK"

trusted_git() {
  env -i \
    HOME="$TRUSTED_GIT_HOME" \
    PATH="/usr/bin:/bin" \
    GIT_CONFIG_GLOBAL=/dev/null \
    GIT_CONFIG_NOSYSTEM=1 \
    GIT_NO_REPLACE_OBJECTS=1 \
    GIT_TERMINAL_PROMPT=0 \
    "$GIT_BIN" \
    -c core.hooksPath=/dev/null \
    -c core.fsmonitor=false \
    -c credential.helper= \
    -c safe.bareRepository=all \
    "$@"
}

reject_replace_refs() {
  local repo_dir="$1"
  local replace_refs

  replace_refs=$(trusted_git -C "$repo_dir" for-each-ref --format='%(refname)' refs/replace)
  if [[ -n "$replace_refs" ]]; then
    echo "::error::Replacement refs are forbidden in the validated repository: $repo_dir."
    exit 1
  fi
}

reject_unsafe_git_config() {
  local repo_dir="$1"
  local local_config
  local unsafe_config

  local_config=$(trusted_git -C "$repo_dir" config --local --no-includes --name-only --list)
  unsafe_config=$(printf '%s\n' "$local_config" | tr '[:upper:]' '[:lower:]' | grep -E \
    '^(include(if\..*)?\.path|core\.(alternaterefscommand|attributesfile|editor|fsmonitor|hookspath|sshcommand|worktree)|diff\.(external|.*\.(command|textconv))|filter\..*\.(clean|smudge|process)|remote\..*\.uploadpack|uploadpack\.packobjectshook)$' || true)
  if [[ -n "$unsafe_config" ]]; then
    echo "::error::Command-bearing local Git configuration is forbidden in $repo_dir: $unsafe_config"
    exit 1
  fi
}

prepare_push_repo() {
  local source_dir="$1"
  local destination="$2"
  local commit_sha="$3"
  local base_sha="$4"
  local fetched_sha

  trusted_git init --bare "$destination" >/dev/null
  trusted_git -C "$destination" fetch --quiet --no-tags \
    --upload-pack="$TRUSTED_GIT_UPLOAD_PACK" "$source_dir" "$commit_sha"
  fetched_sha=$(trusted_git -C "$destination" rev-parse "FETCH_HEAD^{commit}")
  if [[ "$fetched_sha" != "$commit_sha" ]]; then
    echo "::error::Trusted delivery repository did not capture validated commit ${commit_sha}."
    exit 1
  fi
  if ! trusted_git -C "$destination" cat-file -e "${base_sha}^{commit}"; then
    trusted_git -C "$destination" fetch --quiet --no-tags \
      --upload-pack="$TRUSTED_GIT_UPLOAD_PACK" "$source_dir" "$base_sha"
  fi
  fetched_sha=$(trusted_git -C "$destination" rev-parse "${base_sha}^{commit}")
  if [[ "$fetched_sha" != "$base_sha" ]]; then
    echo "::error::Trusted delivery repository did not capture validated base ${base_sha}."
    exit 1
  fi
  if [[ -n "$(trusted_git -C "$destination" for-each-ref --format='%(refname)')" ]]; then
    echo "::error::Trusted delivery repository unexpectedly contains mutable refs."
    exit 1
  fi
}

reject_replace_refs "$GITHUB_WORKSPACE"
reject_replace_refs "$GITHUB_WORKSPACE/externals/skia"
reject_unsafe_git_config "$GITHUB_WORKSPACE"
reject_unsafe_git_config "$GITHUB_WORKSPACE/externals/skia"

SS_VALIDATED_HEAD_SHA=$(trusted_git -C "$GITHUB_WORKSPACE" rev-parse "refs/heads/${HEAD_BRANCH}^{commit}")
SKIA_VALIDATED_HEAD_SHA=$(trusted_git -C "$GITHUB_WORKSPACE/externals/skia" rev-parse "refs/heads/${HEAD_BRANCH}^{commit}")
readonly SS_VALIDATED_HEAD_SHA SKIA_VALIDATED_HEAD_SHA
prepare_push_repo "$GITHUB_WORKSPACE" "$TRUSTED_SS_REPO" "$SS_VALIDATED_HEAD_SHA" "$SS_BASE_SHA"
prepare_push_repo "$GITHUB_WORKSPACE/externals/skia" "$TRUSTED_SKIA_REPO" \
  "$SKIA_VALIDATED_HEAD_SHA" "$SKIA_BASE_SHA"

assert_resolved() {
  local artifact_name="$1"
  local artifact_value="$2"
  local workflow_name="$3"
  local workflow_value="$4"
  if [[ -z "$workflow_value" || "$artifact_value" != "$workflow_value" ]]; then
    echo "::error::Handoff $artifact_name does not match workflow-resolved $workflow_name."
    exit 1
  fi
}

MANIFEST_JSON=$(trusted_git -C "$TRUSTED_SS_REPO" show "${SS_VALIDATED_HEAD_SHA}:cgmanifest.json")
MANIFEST_SKIA_HEAD=$(jq -er '
  .registrations[]
  | select(.component.git.repositoryUrl == "https://github.com/mono/skia.git")
  | .component.git.commitHash
' <<<"$MANIFEST_JSON")
MANIFEST_UPSTREAM_SHA=$(jq -er '
  .registrations[]
  | select(.component.other.name == "skia")
  | .upstream_merge_commit
' <<<"$MANIFEST_JSON")
MANIFEST_MILESTONE=$(jq -er '
  .registrations[]
  | select(.component.other.name == "skia")
  | .chrome_milestone
' <<<"$MANIFEST_JSON")
MANIFEST_UPSTREAM_REF=$(jq -er '
  .registrations[]
  | select(.component.other.name == "skia")
  | .upstream_ref
' <<<"$MANIFEST_JSON")
MANIFEST_UPSTREAM_VERSION=$(jq -er '
  .registrations[]
  | select(.component.other.name == "skia")
  | .component.other.version
' <<<"$MANIFEST_JSON")
LOCAL_SKIA_HEAD="$SKIA_VALIDATED_HEAD_SHA"
PARENT_GITLINK=$(trusted_git -C "$TRUSTED_SS_REPO" ls-tree "$SS_VALIDATED_HEAD_SHA" externals/skia | awk '{print $3}')

assert_resolved CGMANIFEST_SKIA_HEAD "$MANIFEST_SKIA_HEAD" LOCAL_SKIA_HEAD "$LOCAL_SKIA_HEAD"
assert_resolved PARENT_GITLINK "$PARENT_GITLINK" LOCAL_SKIA_HEAD "$LOCAL_SKIA_HEAD"
assert_resolved CGMANIFEST_UPSTREAM_SHA "$MANIFEST_UPSTREAM_SHA" SKIA_SYNC_TARGET_UPSTREAM_SHA "$TARGET_UPSTREAM_SHA"
assert_resolved CGMANIFEST_UPSTREAM_REF "$MANIFEST_UPSTREAM_REF" SKIA_SYNC_UPSTREAM_REF "$UPSTREAM_REF"
assert_resolved CGMANIFEST_MILESTONE "$MANIFEST_MILESTONE" SKIA_SYNC_TARGET "$TARGET"
assert_resolved CGMANIFEST_UPSTREAM_VERSION "$MANIFEST_UPSTREAM_VERSION" EXPECTED_UPSTREAM_VERSION "chrome/m${TARGET}"

if [[ "$(tr -d '[:space:]' < "$ARTIFACT_DIR/test-exit-code.txt")" != "0" ]]; then
  echo "::error::The final unfiltered test command did not exit successfully."
  exit 1
fi

BRANCH="$HEAD_BRANCH"
SS_BASE="$BASE_BRANCH"
SKIA_BASE="$SKIA_BASE_BRANCH"
IS_RELEASE="${IS_RELEASE:-false}"
UPDATED_AT=$(date -u +%Y-%m-%dT%H:%M:%SZ)
WORKFLOW_LINK="[skia-upstream-sync](https://github.com/${GITHUB_REPOSITORY:-mono/SkiaSharp}/actions/workflows/auto-skia-sync.lock.yml)"

command -p python3 -I "$SKILL_DIR/scripts/audit_fork_patches.py" \
  --skia-root "$TRUSTED_SKIA_REPO" \
  --old-upstream "$BASE_UPSTREAM_SHA" \
  --new-upstream "$TARGET_UPSTREAM_SHA" \
  --fork-base "$SKIA_BASE_SHA" \
  --merged-head "$SKIA_VALIDATED_HEAD_SHA" \
  --output "$ARTIFACT_DIR/skia-fork-patch-audit.md" \
  --validate

for report in "$SKIA_SUMMARY_FILE" "$SS_SUMMARY_FILE"; do
  for heading in "## Changes" "## Testing" "## Human review"; do
    if ! tr -d '\r' <"$report" | grep -qE "^${heading}[[:space:]]*$"; then
      echo "::error::$report must contain the exact heading '$heading'."
      exit 1
    fi
  done
done

if [[ "$UPSTREAM_REF" == "main" ]]; then
  SS_TITLE="[skia-sync] Merge upstream Skia main (tip)"
  SKIA_TITLE="$SS_TITLE"
  SS_BODY_INTRO="Automated bleeding-edge sync from the tip of upstream Skia (google/skia main)."
  SKIA_BODY_INTRO="Automated upstream merge of google/skia main (tip)."
  IS_MILESTONE_BUMP=false
elif [[ "$CURRENT" == "$TARGET" ]]; then
  SS_TITLE="[skia-sync] Merge upstream chrome/m${TARGET} bug fixes"
  SKIA_TITLE="[skia-sync] Merge upstream chrome/m${TARGET}"
  SS_BODY_INTRO="Automated upstream bug-fix sync for m${TARGET}."
  SKIA_BODY_INTRO="Automated upstream merge of \`chrome/m${TARGET}\`."
  IS_MILESTONE_BUMP=false
else
  SS_TITLE="[skia-sync] Update skia to milestone ${TARGET}"
  SKIA_TITLE="[skia-sync] Merge upstream chrome/m${TARGET}"
  SS_BODY_INTRO="Automated Skia milestone bump from m${CURRENT} to m${TARGET}."
  SKIA_BODY_INTRO="Automated upstream merge of \`chrome/m${TARGET}\`."
  IS_MILESTONE_BUMP=true
fi
if [[ "$IS_RELEASE" == "true" ]]; then
  SS_BODY_INTRO+=" Targeting release branch \`${SS_BASE}\` (mono/skia \`${SKIA_BASE}\`)."
fi

push_branch() {
  local repo_dir="$1"
  local repo_url="$2"
  local commit_sha="$3"
  local remote_line
  local pushed_line

  local remote_sha
  local lease
  remote_line=$(trusted_git_with_token -C "$repo_dir" ls-remote --heads \
    "https://github.com/${repo_url}.git" "refs/heads/${BRANCH}")
  remote_sha="${remote_line%%[[:space:]]*}"
  if [[ -n "$remote_sha" ]]; then
    lease="--force-with-lease=refs/heads/${BRANCH}:${remote_sha}"
  else
    lease="--force-with-lease=refs/heads/${BRANCH}:"
  fi
  trusted_git_with_token -C "$repo_dir" push "https://github.com/${repo_url}.git" \
    "${commit_sha}:refs/heads/${BRANCH}" \
    "$lease"
  pushed_line=$(trusted_git_with_token -C "$repo_dir" ls-remote --heads \
    "https://github.com/${repo_url}.git" "refs/heads/${BRANCH}")
  if [[ "${pushed_line%%[[:space:]]*}" != "$commit_sha" ]]; then
    echo "::error::Remote branch ${repo_url}:${BRANCH} does not match validated commit ${commit_sha}."
    exit 1
  fi
}

trusted_git_with_token() {
  env -i \
    HOME="$TRUSTED_GIT_HOME" \
    PATH="/usr/bin:/bin" \
    GIT_ASKPASS="$TRUSTED_GIT_ASKPASS" \
    GIT_CONFIG_GLOBAL=/dev/null \
    GIT_CONFIG_NOSYSTEM=1 \
    GIT_NO_REPLACE_OBJECTS=1 \
    GIT_TERMINAL_PROMPT=0 \
    SKIA_SYNC_WRITE_TOKEN="$WRITE_TOKEN" \
    "$GIT_BIN" \
    -c core.hooksPath=/dev/null \
    -c core.fsmonitor=false \
    -c credential.helper= \
    -c safe.bareRepository=all \
    "$@"
}

trusted_gh() {
  (
    cd "$TRUSTED_DELIVERY_DIR"
    env -i \
      HOME="$TRUSTED_GIT_HOME" \
      PATH="/usr/bin:/bin" \
      GH_CONFIG_DIR="$TRUSTED_DELIVERY_DIR/gh-config" \
      GH_PROMPT_DISABLED=1 \
      GH_TOKEN="$WRITE_TOKEN" \
      "$GH_BIN" "$@"
  )
}

changed_check() {
  local repo_dir="$1"
  local base="$2"
  local head="$3"
  shift 3
  local status
  if trusted_git -C "$repo_dir" diff --quiet "${base}...${head}" -- "$@"; then
    printf ' '
  else
    status=$?
    if [[ "$status" -eq 1 ]]; then
      printf 'x'
    else
      return "$status"
    fi
  fi
}

render_template() {
  local template="$1"
  local values_path="$2"
  local output_path="$3"

  jq -nr \
    --arg template "$template" \
    --slurpfile values "$values_path" \
    '
      if ($values | length) != 1 or ($values[0] | type) != "object" then
        error("template values must be a JSON object")
      else
        $values[0] as $values
        | $template
        | gsub("\\{\\{(?<key>[A-Z0-9_]+)\\}\\}";
            .key as $key
            | if ($values | has($key) | not) then
                error("missing template value: \($key)")
              elif ($values[$key] | type) != "string" then
                error("template value \($key) must be a string")
              else
                $values[$key]
              end)
        | sub("[[:space:]]+$"; "")
      end
    ' >"$output_path"
}

render_skia_body() {
  local companion_url="$1"
  local output="$2"
  local values="$ARTIFACT_DIR/skia-pr-values.json"
  local template

  template=$(cat <<'EOF'
> [!NOTE]
> **Required merge method**
>
> **Merge commit only. Do not squash or rebase this PR.** The two-parent merge ancestry is required
> so future syncs can prove which upstream commits are already integrated.

## Description

{{BODY_INTRO}}

This pull request was produced by {{WORKFLOW_LINK}}.

**SkiaSharp issue**

N/A — automated upstream synchronization.

**Required SkiaSharp PR**

Requires {{COMPANION_PR_URL}}

**Areas affected**

- [{{CAPI_CHECK}}] C API (`include/c`, `src/c`)
- [{{DEPS_CHECK}}] Native dependency / `DEPS`
- [{{BUILD_CHECK}}] Build (gn / build files)
- [x] Upstream Skia merge or rebase
- [{{RENDERING_CHECK}}] Rendering output / behavior
- [ ] Other

{{AUTOMATED_REPORT}}

## Checklist

- [x] Targets the `{{BASE_BRANCH}}` branch
- [x] `Changes` above lists every added/changed C API export or states that none changed
- [x] Companion `mono/SkiaSharp` PR linked above

_Last rendered by the sync workflow: {{UPDATED_AT}}_
EOF
)

  jq -n \
    --arg BODY_INTRO "$SKIA_BODY_INTRO" \
    --arg WORKFLOW_LINK "$WORKFLOW_LINK" \
    --arg COMPANION_PR_URL "$companion_url" \
    --arg CAPI_CHECK "$(changed_check "$TRUSTED_SKIA_REPO" "$SKIA_BASE_SHA" "$SKIA_VALIDATED_HEAD_SHA" include/c src/c)" \
    --arg DEPS_CHECK "$(changed_check "$TRUSTED_SKIA_REPO" "$SKIA_BASE_SHA" "$SKIA_VALIDATED_HEAD_SHA" DEPS)" \
    --arg BUILD_CHECK "$(changed_check "$TRUSTED_SKIA_REPO" "$SKIA_BASE_SHA" "$SKIA_VALIDATED_HEAD_SHA" BUILD.gn third_party)" \
    --arg RENDERING_CHECK " " \
    --arg BASE_BRANCH "$SKIA_BASE" \
    --rawfile AUTOMATED_REPORT "$SKIA_SUMMARY_FILE" \
    --arg UPDATED_AT "$UPDATED_AT" \
    '{
      BODY_INTRO: $BODY_INTRO,
      WORKFLOW_LINK: $WORKFLOW_LINK,
      COMPANION_PR_URL: $COMPANION_PR_URL,
      CAPI_CHECK: $CAPI_CHECK,
      DEPS_CHECK: $DEPS_CHECK,
      BUILD_CHECK: $BUILD_CHECK,
      RENDERING_CHECK: $RENDERING_CHECK,
      BASE_BRANCH: $BASE_BRANCH,
      AUTOMATED_REPORT: $AUTOMATED_REPORT,
      UPDATED_AT: $UPDATED_AT
    }' >"$values"

  render_template "$template" "$values" "$output"
}

render_skiasharp_body() {
  local companion_url="$1"
  local output="$2"
  local values="$ARTIFACT_DIR/skiasharp-pr-values.json"
  local generated_check
  local template

  template=$(cat <<'EOF'
## Description

{{BODY_INTRO}}

This pull request was produced by {{WORKFLOW_LINK}}.

**Related issues**

N/A — automated upstream synchronization.

**Required skia PR**

Requires {{COMPANION_PR_URL}}

**Areas affected**

- [{{MANAGED_CHECK}}] Managed API (`binding/`)
- [{{NATIVE_CHECK}}] Native / C API (`externals/skia/src/c`, `include/c`)
- [{{GENERATED_CHECK}}] Generated P/Invoke bindings
- [x] Native dependency or Skia update
- [{{INTEGRATIONS_CHECK}}] Views & integrations
- [{{RENDERING_CHECK}}] Rendering output / visual behavior
- [ ] Performance
- [{{TESTS_CHECK}}] Tests
- [{{BUILD_CHECK}}] Build, packaging, or CI
- [{{DOCS_CHECK}}] Documentation or samples

{{AUTOMATED_REPORT}}

## Checklist

- [x] Tests added or updated when behavior required them, or the report explains why not
- [x] `Changes` above lists all public API and behavioral changes or states that none changed
- [{{DOCS_FOLLOWUP_CHECK}}] Documentation follow-up filed, or no public API changed
- [x] Companion `mono/skia` PR linked above and bindings regenerated

_Last rendered by the sync workflow: {{UPDATED_AT}}_
EOF
)

  generated_check=$(changed_check "$TRUSTED_SS_REPO" "$SS_BASE_SHA" "$SS_VALIDATED_HEAD_SHA" ':(glob)**/*.generated.cs')
  jq -n \
    --arg BODY_INTRO "$SS_BODY_INTRO" \
    --arg WORKFLOW_LINK "$WORKFLOW_LINK" \
    --arg COMPANION_PR_URL "$companion_url" \
    --arg MANAGED_CHECK "$(changed_check "$TRUSTED_SS_REPO" "$SS_BASE_SHA" "$SS_VALIDATED_HEAD_SHA" binding)" \
    --arg NATIVE_CHECK "$(changed_check "$TRUSTED_SKIA_REPO" "$SKIA_BASE_SHA" "$SKIA_VALIDATED_HEAD_SHA" include/c src/c)" \
    --arg GENERATED_CHECK "$generated_check" \
    --arg INTEGRATIONS_CHECK "$(changed_check "$TRUSTED_SS_REPO" "$SS_BASE_SHA" "$SS_VALIDATED_HEAD_SHA" views source)" \
    --arg RENDERING_CHECK " " \
    --arg TESTS_CHECK "$(changed_check "$TRUSTED_SS_REPO" "$SS_BASE_SHA" "$SS_VALIDATED_HEAD_SHA" tests)" \
    --arg BUILD_CHECK "$(changed_check "$TRUSTED_SS_REPO" "$SS_BASE_SHA" "$SS_VALIDATED_HEAD_SHA" native scripts .github)" \
    --arg DOCS_CHECK "$(changed_check "$TRUSTED_SS_REPO" "$SS_BASE_SHA" "$SS_VALIDATED_HEAD_SHA" documentation samples)" \
    --arg DOCS_FOLLOWUP_CHECK "$([[ "$generated_check" == " " ]] && printf x || printf ' ')" \
    --rawfile AUTOMATED_REPORT "$SS_SUMMARY_FILE" \
    --arg UPDATED_AT "$UPDATED_AT" \
    '{
      BODY_INTRO: $BODY_INTRO,
      WORKFLOW_LINK: $WORKFLOW_LINK,
      COMPANION_PR_URL: $COMPANION_PR_URL,
      MANAGED_CHECK: $MANAGED_CHECK,
      NATIVE_CHECK: $NATIVE_CHECK,
      GENERATED_CHECK: $GENERATED_CHECK,
      INTEGRATIONS_CHECK: $INTEGRATIONS_CHECK,
      RENDERING_CHECK: $RENDERING_CHECK,
      TESTS_CHECK: $TESTS_CHECK,
      BUILD_CHECK: $BUILD_CHECK,
      DOCS_CHECK: $DOCS_CHECK,
      DOCS_FOLLOWUP_CHECK: $DOCS_FOLLOWUP_CHECK,
      AUTOMATED_REPORT: $AUTOMATED_REPORT,
      UPDATED_AT: $UPDATED_AT
    }' >"$values"

  render_template "$template" "$values" "$output"
}

find_pr() {
  local repo="$1"
  local base="$2"
  local expected_sha="$3"
  local repository_name="${repo#*/}"
  local matches

  matches=$(trusted_gh pr list \
    --repo "$repo" \
    --head "$BRANCH" \
    --base "$base" \
    --state open \
    --json number,headRepository,headRepositoryOwner,headRefName,headRefOid,baseRefName)
  jq -r \
    --arg repo "$repo" \
    --arg owner mono \
    --arg repository_name "$repository_name" \
    --arg head "$BRANCH" \
    --arg base "$base" \
    --arg sha "$expected_sha" \
    '
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
    ' <<<"$matches"
}

ensure_pr() {
  local repo="$1"
  local base="$2"
  local title="$3"
  local body_file="$4"
  local expected_sha="$5"
  local pr

  pr=$(find_pr "$repo" "$base" "$expected_sha")
  if [[ -z "$pr" ]]; then
    trusted_gh pr create --repo "$repo" \
      --head "$BRANCH" \
      --base "$base" \
      --title "$title" \
      --draft \
      --body-file "$body_file" >/dev/null
    pr=$(find_pr "$repo" "$base" "$expected_sha")
  fi
  if [[ -z "$pr" ]]; then
    echo "::error::Failed to create or find the $repo PR for $BRANCH."
    exit 1
  fi
  printf '%s' "$pr"
}

patch_pr() {
  local repo="$1"
  local pr="$2"
  local title="$3"
  local body_file="$4"
  local request="$ARTIFACT_DIR/pr-patch.json"

  jq -n --arg title "$title" --rawfile body "$body_file" '{title: $title, body: $body}' >"$request"
  trusted_gh api --method PATCH "repos/${repo}/pulls/${pr}" --input "$request" >/dev/null
}

apply_labels() {
  local repo="$1"
  local pr="$2"
  local request="$ARTIFACT_DIR/pr-labels.json"

  jq -n --argjson bump "$IS_MILESTONE_BUMP" \
    '{labels: (["type/milestone-sync", "partner/agentic-workflows"]
      + (if $bump then ["type/milestone-bump"] else [] end))}' >"$request"
  trusted_gh api --method POST "repos/${repo}/issues/${pr}/labels" --input "$request" >/dev/null
}

echo "Pushing $BRANCH to mono/skia and mono/SkiaSharp with guarded leases..."
# shellcheck disable=SC2016 # These variables expand only when Git invokes the generated helper.
printf '%s\n' \
  '#!/bin/sh' \
  'case "$1" in' \
  '  *Username*) printf "%s\n" "x-access-token" ;;' \
  '  *Password*) printf "%s\n" "$SKIA_SYNC_WRITE_TOKEN" ;;' \
  '  *) exit 1 ;;' \
  'esac' >"$TRUSTED_GIT_ASKPASS"
command -p chmod 700 "$TRUSTED_GIT_ASKPASS"

push_branch "$TRUSTED_SKIA_REPO" mono/skia "$SKIA_VALIDATED_HEAD_SHA"
push_branch "$TRUSTED_SS_REPO" mono/SkiaSharp "$SS_VALIDATED_HEAD_SHA"

SKIA_BODY="$ARTIFACT_DIR/skia-pr-body.md"
SS_BODY="$ARTIFACT_DIR/skiasharp-pr-body.md"

render_skia_body "Pending companion PR creation in this workflow run." "$SKIA_BODY"
SKIA_PR=$(ensure_pr mono/skia "$SKIA_BASE" "$SKIA_TITLE" "$SKIA_BODY" "$SKIA_VALIDATED_HEAD_SHA")
SKIA_PR_URL="https://github.com/mono/skia/pull/${SKIA_PR}"

render_skiasharp_body "$SKIA_PR_URL" "$SS_BODY"
SS_PR=$(ensure_pr mono/SkiaSharp "$SS_BASE" "$SS_TITLE" "$SS_BODY" "$SS_VALIDATED_HEAD_SHA")
SS_PR_URL="https://github.com/mono/SkiaSharp/pull/${SS_PR}"

render_skia_body "$SS_PR_URL" "$SKIA_BODY"
render_skiasharp_body "$SKIA_PR_URL" "$SS_BODY"
patch_pr mono/skia "$SKIA_PR" "$SKIA_TITLE" "$SKIA_BODY"
patch_pr mono/SkiaSharp "$SS_PR" "$SS_TITLE" "$SS_BODY"
apply_labels mono/skia "$SKIA_PR"
apply_labels mono/SkiaSharp "$SS_PR"

echo "Created or updated reciprocal draft PRs:"
echo "  $SKIA_PR_URL"
echo "  $SS_PR_URL"
