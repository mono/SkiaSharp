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
readonly DELIVERY_ARTIFACT_NAMES=(
  skia-sync-skia-summary.md
  skia-sync-skiasharp-summary.md
  skia-breaking-change-analysis.md
  skia-validation-review.md
  skia-dependency-decisions.md
  skia-dependency-changes.json
  skia-fork-patch-audit.md
  initial-test-output.txt
  test-output.txt
  test-exit-code.txt
)
readonly DELIVERY_PACKAGE_PAYLOAD_NAMES=(
  skiasharp.bundle
  skia.bundle
  skiasharp.patch
  skia.patch
  metadata.json
  completion-signal.jsonl
  "${DELIVERY_ARTIFACT_NAMES[@]}"
)

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

required_file() {
  local path="$1"
  if [[ ! -f "$path" || -L "$path" || ! -s "$path" ]]; then
    echo "::error::Required sync artifact is missing or not a nonempty regular, non-symlink file: $path"
    exit 1
  fi
}

package_git() {
  env -i \
    HOME="$SKIA_SYNC_PACKAGE_GIT_HOME" \
    PATH="/usr/bin:/bin" \
    GIT_CONFIG_GLOBAL=/dev/null \
    GIT_CONFIG_NOSYSTEM=1 \
    GIT_NO_REPLACE_OBJECTS=1 \
    GIT_TERMINAL_PROMPT=0 \
    "$SKIA_SYNC_GIT_BIN" \
    -c core.hooksPath=/dev/null \
    -c core.fsmonitor=false \
    -c credential.helper= \
    -c commit.gpgSign=false \
    -c safe.bareRepository=all \
    "$@"
}

reject_repository_state() {
  local git_runner="$1"
  local repo_dir="$2"
  local git_dir
  local local_config
  local replace_refs
  local unsafe_config

  git_dir=$("$git_runner" -C "$repo_dir" rev-parse --absolute-git-dir)
  if [[ -e "$git_dir/config.worktree" || -L "$git_dir/config.worktree" ]]; then
    echo "::error::Worktree Git configuration is forbidden in $repo_dir."
    exit 1
  fi

  local_config=$("$git_runner" -C "$repo_dir" config --local --no-includes --name-only --list)
  unsafe_config=$(printf '%s\n' "$local_config" | tr '[:upper:]' '[:lower:]' | grep -E \
    '^(extensions\.worktreeconfig|include(if\..*)?\.path|core\.(alternaterefscommand|attributesfile|editor|fsmonitor|hookspath|sshcommand|worktree)|diff\.(external|.*\.(command|textconv))|filter\..*\.(clean|smudge|process)|credential(\..*)?\.helper|remote\..*\.uploadpack|uploadpack\.packobjectshook|url\..*\.(insteadof|pushinsteadof)|commit\.gpgsign|gpg\..*|sequence\.editor)$' || true)
  if [[ -n "$unsafe_config" ]]; then
    echo "::error::Command-bearing local Git configuration is forbidden in $repo_dir: $unsafe_config"
    exit 1
  fi

  replace_refs=$("$git_runner" -C "$repo_dir" for-each-ref --format='%(refname)' refs/replace)
  if [[ -n "$replace_refs" ]]; then
    echo "::error::Replacement refs are forbidden in the validated repository: $repo_dir."
    exit 1
  fi
}

stage_delivery_package() {
  local package_dir="${SKIA_SYNC_DELIVERY_PACKAGE_DIR:?SKIA_SYNC_DELIVERY_PACKAGE_DIR is required}"
  local parent_repo="${SKIA_SYNC_PARENT_REPO_DIR:-${GITHUB_WORKSPACE:?GITHUB_WORKSPACE is required}}"
  local skia_repo="${SKIA_SYNC_SKIA_REPO_DIR:-$parent_repo/externals/skia}"
  local parent_head_sha
  local skia_head_sha
  local parent_gitlink_sha
  local parent_bundle_sha256
  local skia_bundle_sha256
  local parent_patch_sha256
  local skia_patch_sha256
  local safe_output_bundle
  local threat_detection_dir="${SKIA_SYNC_THREAT_DETECTION_SOURCE_DIR:-${RUNNER_TEMP:?RUNNER_TEMP is required}/gh-aw}"
  local name

  : "${RUNNER_TEMP:?RUNNER_TEMP is required}"
  : "${GITHUB_SHA:?GITHUB_SHA is required}"
  : "${SKIA_SYNC_COMPLETION_SIGNAL_FILE:?SKIA_SYNC_COMPLETION_SIGNAL_FILE is required}"
  : "${SKIA_SYNC_HEAD_BRANCH:?SKIA_SYNC_HEAD_BRANCH is required}"
  : "${SKIA_SYNC_BASE_BRANCH:?SKIA_SYNC_BASE_BRANCH is required}"
  : "${SKIA_SYNC_PARENT_BASE_SHA:?SKIA_SYNC_PARENT_BASE_SHA is required}"
  : "${SKIA_SYNC_SKIA_BASE_BRANCH:?SKIA_SYNC_SKIA_BASE_BRANCH is required}"
  : "${SKIA_SYNC_SKIA_BASE_SHA:?SKIA_SYNC_SKIA_BASE_SHA is required}"
  : "${SKIA_SYNC_CURRENT:?SKIA_SYNC_CURRENT is required}"
  : "${SKIA_SYNC_TARGET:?SKIA_SYNC_TARGET is required}"
  : "${SKIA_SYNC_UPSTREAM_REF:?SKIA_SYNC_UPSTREAM_REF is required}"
  : "${SKIA_SYNC_IS_RELEASE:?SKIA_SYNC_IS_RELEASE is required}"
  : "${SKIA_SYNC_BASE_UPSTREAM_SHA:?SKIA_SYNC_BASE_UPSTREAM_SHA is required}"
  : "${SKIA_SYNC_TARGET_UPSTREAM_SHA:?SKIA_SYNC_TARGET_UPSTREAM_SHA is required}"

  command -p python3 -I - "$RUNNER_TEMP" "$package_dir" <<'PY'
import os
import stat
import sys

runner_temp = os.path.realpath(sys.argv[1])
package_dir = os.path.realpath(sys.argv[2])
if os.path.dirname(package_dir) != runner_temp or not os.path.basename(package_dir).startswith("skia-sync-delivery-package."):
    raise SystemExit("::error::The delivery package directory is not a runner-owned randomized path.")
metadata = os.lstat(package_dir)
if not stat.S_ISDIR(metadata.st_mode) or stat.S_ISLNK(metadata.st_mode) or metadata.st_uid != os.getuid():
    raise SystemExit("::error::The delivery package directory is not a runner-owned regular directory.")
if stat.S_IMODE(metadata.st_mode) & 0o077:
    raise SystemExit("::error::The delivery package directory permissions are not private.")
if os.listdir(package_dir):
    raise SystemExit("::error::The delivery package directory must start empty.")
PY

  SKIA_SYNC_GIT_BIN=$(command -p -v git)
  SKIA_SYNC_PACKAGE_GIT_HOME="$package_dir/.git-home"
  export SKIA_SYNC_GIT_BIN SKIA_SYNC_PACKAGE_GIT_HOME
  command -p mkdir -m 700 "$SKIA_SYNC_PACKAGE_GIT_HOME"

  reject_repository_state package_git "$parent_repo"
  reject_repository_state package_git "$skia_repo"
  validate_delivery_signal
  for name in "${DELIVERY_ARTIFACT_NAMES[@]}"; do
    required_file "$ARTIFACT_DIR/$name"
  done

  parent_head_sha=$(package_git -C "$parent_repo" rev-parse "refs/heads/${SKIA_SYNC_HEAD_BRANCH}^{commit}")
  skia_head_sha=$(package_git -C "$skia_repo" rev-parse "refs/heads/${SKIA_SYNC_HEAD_BRANCH}^{commit}")
  package_git -C "$parent_repo" merge-base --is-ancestor "$SKIA_SYNC_PARENT_BASE_SHA" "$parent_head_sha"
  package_git -C "$skia_repo" merge-base --is-ancestor "$SKIA_SYNC_SKIA_BASE_SHA" "$skia_head_sha"
  # shellcheck disable=SC2016 # The awk field references must not expand in Bash.
  parent_gitlink_sha=$(package_git -C "$parent_repo" ls-tree "$parent_head_sha" externals/skia |
    command -p awk '$1 == "160000" && $2 == "commit" && $4 == "externals/skia" { print $3 }')
  if [[ "$parent_gitlink_sha" != "$skia_head_sha" ]]; then
    signal_error "The final parent gitlink does not match the final nested Skia head."
    exit 1
  fi

  safe_output_bundle=$(command -p python3 -I - "$threat_detection_dir" <<'PY'
import os
import stat
import sys

root = sys.argv[1]
entries = [
    entry for entry in os.scandir(root)
    if entry.name.startswith("aw-") and entry.name.endswith(".bundle")
]
if len(entries) != 1:
    raise SystemExit(
        f"::error::Expected exactly one staged create_pull_request bundle, found {len(entries)}."
    )
entry = entries[0]
metadata = entry.stat(follow_symlinks=False)
if (not stat.S_ISREG(metadata.st_mode) or entry.is_symlink()
        or metadata.st_nlink != 1 or metadata.st_size == 0):
    raise SystemExit(
        "::error::The staged create_pull_request bundle must be a nonempty standalone regular file."
    )
sys.stdout.write(entry.path)
PY
  )
  if [[ "$(package_git bundle list-heads "$safe_output_bundle")" != \
        "$parent_head_sha refs/heads/$SKIA_SYNC_HEAD_BRANCH" ]]; then
    signal_error "The staged create_pull_request bundle does not match the final parent head."
    exit 1
  fi

  package_git -C "$parent_repo" bundle create "$package_dir/skiasharp.bundle" \
    "refs/heads/$SKIA_SYNC_HEAD_BRANCH"
  package_git -C "$skia_repo" bundle create "$package_dir/skia.bundle" \
    "refs/heads/$SKIA_SYNC_HEAD_BRANCH"
  package_git -C "$parent_repo" diff --binary --no-ext-diff --no-textconv \
    "$SKIA_SYNC_PARENT_BASE_SHA" "$parent_head_sha" >"$package_dir/skiasharp.patch"
  package_git -C "$skia_repo" diff --binary --no-ext-diff --no-textconv \
    "$SKIA_SYNC_SKIA_BASE_SHA" "$skia_head_sha" >"$package_dir/skia.patch"
  required_file "$package_dir/skiasharp.patch"
  required_file "$package_dir/skia.patch"

  read -r parent_bundle_sha256 skia_bundle_sha256 parent_patch_sha256 skia_patch_sha256 < <(
    command -p python3 -I - \
      "$package_dir/skiasharp.bundle" \
      "$package_dir/skia.bundle" \
      "$package_dir/skiasharp.patch" \
      "$package_dir/skia.patch" <<'PY'
import hashlib
import sys


def digest(path):
    value = hashlib.sha256()
    with open(path, "rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            value.update(chunk)
    return value.hexdigest()


print(*(digest(path) for path in sys.argv[1:]))
PY
  )

  for name in "${DELIVERY_ARTIFACT_NAMES[@]}"; do
    command -p install -m 600 "$ARTIFACT_DIR/$name" "$package_dir/$name"
  done
  command -p install -m 600 "$SKIA_SYNC_COMPLETION_SIGNAL_FILE" \
    "$package_dir/completion-signal.jsonl"

  jq -n \
    --argjson schema_version 1 \
    --arg repository mono/SkiaSharp \
    --arg workflow_sha "$GITHUB_SHA" \
    --arg current "$SKIA_SYNC_CURRENT" \
    --arg target "$SKIA_SYNC_TARGET" \
    --arg upstream_ref "$SKIA_SYNC_UPSTREAM_REF" \
    --arg is_release "$SKIA_SYNC_IS_RELEASE" \
    --arg base_branch "$SKIA_SYNC_BASE_BRANCH" \
    --arg skia_base_branch "$SKIA_SYNC_SKIA_BASE_BRANCH" \
    --arg head_branch "$SKIA_SYNC_HEAD_BRANCH" \
    --arg parent_base_sha "$SKIA_SYNC_PARENT_BASE_SHA" \
    --arg skia_base_sha "$SKIA_SYNC_SKIA_BASE_SHA" \
    --arg base_upstream_sha "$SKIA_SYNC_BASE_UPSTREAM_SHA" \
    --arg target_upstream_sha "$SKIA_SYNC_TARGET_UPSTREAM_SHA" \
    --arg parent_head_sha "$parent_head_sha" \
    --arg skia_head_sha "$skia_head_sha" \
    --arg parent_gitlink_sha "$parent_gitlink_sha" \
    --arg parent_bundle_sha256 "$parent_bundle_sha256" \
    --arg skia_bundle_sha256 "$skia_bundle_sha256" \
    --arg parent_patch_sha256 "$parent_patch_sha256" \
    --arg skia_patch_sha256 "$skia_patch_sha256" \
    '{
      schema_version: $schema_version,
      repository: $repository,
      workflow_sha: $workflow_sha,
      current: $current,
      target: $target,
      upstream_ref: $upstream_ref,
      is_release: $is_release,
      base_branch: $base_branch,
      skia_base_branch: $skia_base_branch,
      head_branch: $head_branch,
      parent_base_sha: $parent_base_sha,
      skia_base_sha: $skia_base_sha,
      base_upstream_sha: $base_upstream_sha,
      target_upstream_sha: $target_upstream_sha,
      parent_head_sha: $parent_head_sha,
      skia_head_sha: $skia_head_sha,
      parent_gitlink_sha: $parent_gitlink_sha,
      parent_bundle_sha256: $parent_bundle_sha256,
      skia_bundle_sha256: $skia_bundle_sha256,
      parent_patch_sha256: $parent_patch_sha256,
      skia_patch_sha256: $skia_patch_sha256
    }' >"$package_dir/metadata.json"

  command -p rm -rf -- "$SKIA_SYNC_PACKAGE_GIT_HOME"
  command -p python3 -I - "$package_dir" "${DELIVERY_PACKAGE_PAYLOAD_NAMES[@]}" <<'PY'
import hashlib
import os
import sys

root = sys.argv[1]
names = sorted(sys.argv[2:])
with open(os.path.join(root, "SHA256SUMS"), "x", encoding="ascii", newline="\n") as output:
    for name in names:
        path = os.path.join(root, name)
        digest = hashlib.sha256()
        with open(path, "rb") as payload:
            for chunk in iter(lambda: payload.read(1024 * 1024), b""):
                digest.update(chunk)
        output.write(f"{digest.hexdigest()}  {name}\n")
PY
  command -p chmod 600 "$package_dir"/*
  for name in "${DELIVERY_PACKAGE_PAYLOAD_NAMES[@]}" SHA256SUMS; do
    required_file "$package_dir/$name"
  done

  for name in \
    aw-final-skiasharp.bundle \
    aw-final-skia.bundle \
    aw-final-skiasharp.patch \
    aw-final-skia.patch; do
    if [[ -e "$threat_detection_dir/$name" || -L "$threat_detection_dir/$name" ]]; then
      signal_error "Threat-detection input already exists: $threat_detection_dir/$name"
      exit 1
    fi
  done
  command -p install -m 600 "$package_dir/skiasharp.bundle" \
    "$threat_detection_dir/aw-final-skiasharp.bundle"
  command -p install -m 600 "$package_dir/skia.bundle" \
    "$threat_detection_dir/aw-final-skia.bundle"
  command -p install -m 600 "$package_dir/skiasharp.patch" \
    "$threat_detection_dir/aw-final-skiasharp.patch"
  command -p install -m 600 "$package_dir/skia.patch" \
    "$threat_detection_dir/aw-final-skia.patch"
  echo "Staged immutable Skia sync delivery package."
}

verify_delivery_package() {
  local package_dir="${SKIA_SYNC_DELIVERY_PACKAGE_DIR:?SKIA_SYNC_DELIVERY_PACKAGE_DIR is required}"
  local verified_root="${SKIA_SYNC_VERIFIED_ROOT:?SKIA_SYNC_VERIFIED_ROOT is required}"
  local metadata="$package_dir/metadata.json"
  local parent_repo="$verified_root/skiasharp.git"
  local skia_repo="$verified_root/skia.git"
  local parent_head_sha
  local skia_head_sha

  : "${GITHUB_SHA:?GITHUB_SHA is required}"
  : "${SKIA_SYNC_CURRENT:?SKIA_SYNC_CURRENT is required}"
  : "${SKIA_SYNC_TARGET:?SKIA_SYNC_TARGET is required}"
  : "${SKIA_SYNC_UPSTREAM_REF:?SKIA_SYNC_UPSTREAM_REF is required}"
  : "${SKIA_SYNC_IS_RELEASE:?SKIA_SYNC_IS_RELEASE is required}"
  : "${SKIA_SYNC_BASE_BRANCH:?SKIA_SYNC_BASE_BRANCH is required}"
  : "${SKIA_SYNC_SKIA_BASE_BRANCH:?SKIA_SYNC_SKIA_BASE_BRANCH is required}"
  : "${SKIA_SYNC_HEAD_BRANCH:?SKIA_SYNC_HEAD_BRANCH is required}"
  : "${SKIA_SYNC_EXPECTED_PARENT_BASE_SHA:?SKIA_SYNC_EXPECTED_PARENT_BASE_SHA is required}"
  : "${SKIA_SYNC_EXPECTED_SKIA_BASE_SHA:?SKIA_SYNC_EXPECTED_SKIA_BASE_SHA is required}"

  command -p python3 -I - "$package_dir" "${DELIVERY_PACKAGE_PAYLOAD_NAMES[@]}" <<'PY'
import hashlib
import json
import os
import re
import stat
import sys
import unicodedata


def reject(message):
    raise SystemExit(f"::error::{message}")


root = sys.argv[1]
payload_names = list(sys.argv[2:])
allowed_names = set(payload_names) | {"SHA256SUMS"}
try:
    root_metadata = os.lstat(root)
except OSError:
    reject("The delivery package directory is missing.")
if not stat.S_ISDIR(root_metadata.st_mode) or stat.S_ISLNK(root_metadata.st_mode):
    reject("The delivery package path must be a regular, non-symlink directory.")

entries = {}
for entry in os.scandir(root):
    if entry.name not in allowed_names:
        reject(f"Unexpected delivery package entry: {entry.name}")
    metadata = entry.stat(follow_symlinks=False)
    if not stat.S_ISREG(metadata.st_mode) or entry.is_symlink() or metadata.st_nlink != 1 or metadata.st_size == 0:
        reject(f"Delivery package entry is not a nonempty standalone regular file: {entry.name}")
    entries[entry.name] = metadata
if set(entries) != allowed_names:
    missing = sorted(allowed_names - set(entries))
    reject(f"Delivery package is missing required entries: {', '.join(missing)}")

checksum_path = os.path.join(root, "SHA256SUMS")
with open(checksum_path, "rb") as checksum_file:
    checksum_content = checksum_file.read()
if b"\0" in checksum_content:
    reject("Delivery package checksums contain a NUL byte.")
try:
    checksum_text = checksum_content.decode("ascii")
except UnicodeDecodeError:
    reject("Delivery package checksums are not ASCII.")
checksum_lines = checksum_text.splitlines()
if len(checksum_lines) != len(payload_names):
    reject("Delivery package checksum manifest has the wrong number of entries.")
checksums = {}
for line in checksum_lines:
    match = re.fullmatch(r"([0-9a-f]{64})  ([A-Za-z0-9][A-Za-z0-9._-]*)", line)
    if not match or match.group(2) in checksums:
        reject("Delivery package checksum manifest is malformed.")
    checksums[match.group(2)] = match.group(1)
if set(checksums) != set(payload_names):
    reject("Delivery package checksum manifest does not exactly match the allowlist.")
for name in payload_names:
    digest = hashlib.sha256()
    with open(os.path.join(root, name), "rb") as payload:
        for chunk in iter(lambda: payload.read(1024 * 1024), b""):
            digest.update(chunk)
    if digest.hexdigest() != checksums[name]:
        reject(f"Delivery package digest mismatch: {name}")

def unique_object(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise ValueError(f"duplicate key: {key}")
        result[key] = value
    return result

try:
    with open(os.path.join(root, "metadata.json"), encoding="utf-8") as metadata_file:
        metadata = json.load(
            metadata_file,
            object_pairs_hook=unique_object,
            parse_constant=lambda value: (_ for _ in ()).throw(ValueError(value)),
        )
except (OSError, UnicodeDecodeError, json.JSONDecodeError, ValueError):
    reject("Delivery package metadata is malformed.")

expected_keys = {
    "schema_version", "repository", "workflow_sha", "current", "target", "upstream_ref",
    "is_release", "base_branch", "skia_base_branch", "head_branch", "parent_base_sha",
    "skia_base_sha", "base_upstream_sha", "target_upstream_sha", "parent_head_sha",
    "skia_head_sha", "parent_gitlink_sha", "parent_bundle_sha256", "skia_bundle_sha256",
    "parent_patch_sha256", "skia_patch_sha256",
}
if type(metadata) is not dict or set(metadata) != expected_keys or metadata.get("schema_version") != 1:
    reject("Delivery package metadata does not match schema version 1.")
for key, value in metadata.items():
    if key == "schema_version":
        continue
    if type(value) is not str or any(unicodedata.category(character) == "Cc" for character in value):
        reject(f"Delivery package metadata field is not a clean string: {key}")

expected = {
    "repository": "mono/SkiaSharp",
    "workflow_sha": os.environ["GITHUB_SHA"],
    "current": os.environ["SKIA_SYNC_CURRENT"],
    "target": os.environ["SKIA_SYNC_TARGET"],
    "upstream_ref": os.environ["SKIA_SYNC_UPSTREAM_REF"],
    "is_release": os.environ["SKIA_SYNC_IS_RELEASE"],
    "base_branch": os.environ["SKIA_SYNC_BASE_BRANCH"],
    "skia_base_branch": os.environ["SKIA_SYNC_SKIA_BASE_BRANCH"],
    "head_branch": os.environ["SKIA_SYNC_HEAD_BRANCH"],
    "parent_base_sha": os.environ["SKIA_SYNC_EXPECTED_PARENT_BASE_SHA"],
    "skia_base_sha": os.environ["SKIA_SYNC_EXPECTED_SKIA_BASE_SHA"],
}
for key, expected_value in expected.items():
    if metadata.get(key) != expected_value:
        reject(f"Delivery package metadata mismatch: {key}")
for key in ("workflow_sha", "parent_base_sha", "skia_base_sha", "base_upstream_sha",
            "target_upstream_sha", "parent_head_sha", "skia_head_sha", "parent_gitlink_sha"):
    if not re.fullmatch(r"[0-9a-f]{40}", metadata[key]):
        reject(f"Delivery package metadata field is not a full lowercase SHA: {key}")
if metadata["parent_gitlink_sha"] != metadata["skia_head_sha"]:
    reject("Delivery package parent gitlink does not match the nested Skia head.")
for field, name in {
    "parent_bundle_sha256": "skiasharp.bundle",
    "skia_bundle_sha256": "skia.bundle",
    "parent_patch_sha256": "skiasharp.patch",
    "skia_patch_sha256": "skia.patch",
}.items():
    if not re.fullmatch(r"[0-9a-f]{64}", metadata[field]):
        reject(f"Delivery package metadata field is not a SHA-256 digest: {field}")
    if metadata[field] != checksums[name]:
        reject(f"Delivery package metadata digest mismatch: {field}")
PY

  SKIA_SYNC_GIT_BIN=$(command -p -v git)
  SKIA_SYNC_PACKAGE_GIT_HOME="$verified_root/home"
  export SKIA_SYNC_GIT_BIN SKIA_SYNC_PACKAGE_GIT_HOME
  if [[ ! -d "$verified_root" || -L "$verified_root" ||
        -n "$(command -p find "$verified_root" -mindepth 1 -maxdepth 1 -print -quit)" ]]; then
    signal_error "The verified delivery root must be an empty regular directory."
    exit 1
  fi
  command -p chmod 700 "$verified_root"
  command -p mkdir -m 700 "$SKIA_SYNC_PACKAGE_GIT_HOME"

  parent_head_sha=$(jq -er .parent_head_sha "$metadata")
  skia_head_sha=$(jq -er .skia_head_sha "$metadata")
  SKIA_SYNC_PARENT_BASE_SHA=$(jq -er .parent_base_sha "$metadata")
  SKIA_SYNC_SKIA_BASE_SHA=$(jq -er .skia_base_sha "$metadata")
  SKIA_SYNC_BASE_UPSTREAM_SHA=$(jq -er .base_upstream_sha "$metadata")
  SKIA_SYNC_TARGET_UPSTREAM_SHA=$(jq -er .target_upstream_sha "$metadata")
  export SKIA_SYNC_PARENT_BASE_SHA SKIA_SYNC_SKIA_BASE_SHA
  export SKIA_SYNC_BASE_UPSTREAM_SHA SKIA_SYNC_TARGET_UPSTREAM_SHA

  SKIA_SYNC_COMPLETION_SIGNAL_FILE="$package_dir/completion-signal.jsonl"
  export SKIA_SYNC_COMPLETION_SIGNAL_FILE
  validate_delivery_signal

  verify_bundle_head() {
    local bundle="$1"
    local expected_sha="$2"
    local expected_ref="refs/heads/$SKIA_SYNC_HEAD_BRANCH"
    local actual
    actual=$(package_git bundle list-heads "$bundle")
    if [[ "$actual" != "$expected_sha $expected_ref" ]]; then
      signal_error "Delivery bundle does not contain exactly the expected immutable head: $bundle"
      exit 1
    fi
  }

  verify_bundle_head "$package_dir/skiasharp.bundle" "$parent_head_sha"
  verify_bundle_head "$package_dir/skia.bundle" "$skia_head_sha"
  package_git init --bare "$parent_repo" >/dev/null
  package_git init --bare "$skia_repo" >/dev/null
  package_git -C "$parent_repo" fetch --quiet --no-tags "$package_dir/skiasharp.bundle" \
    "refs/heads/$SKIA_SYNC_HEAD_BRANCH:refs/heads/$SKIA_SYNC_HEAD_BRANCH"
  package_git -C "$skia_repo" fetch --quiet --no-tags "$package_dir/skia.bundle" \
    "refs/heads/$SKIA_SYNC_HEAD_BRANCH:refs/heads/$SKIA_SYNC_HEAD_BRANCH"
  [[ "$(package_git -C "$parent_repo" rev-parse "refs/heads/${SKIA_SYNC_HEAD_BRANCH}^{commit}")" == "$parent_head_sha" ]]
  [[ "$(package_git -C "$skia_repo" rev-parse "refs/heads/${SKIA_SYNC_HEAD_BRANCH}^{commit}")" == "$skia_head_sha" ]]
  package_git -C "$parent_repo" merge-base --is-ancestor "$SKIA_SYNC_PARENT_BASE_SHA" "$parent_head_sha"
  package_git -C "$skia_repo" merge-base --is-ancestor "$SKIA_SYNC_SKIA_BASE_SHA" "$skia_head_sha"
  # shellcheck disable=SC2016 # The awk field references must not expand in Bash.
  if [[ "$(package_git -C "$parent_repo" ls-tree "$parent_head_sha" externals/skia |
      command -p awk '$1 == "160000" && $2 == "commit" && $4 == "externals/skia" { print $3 }')" != \
        "$skia_head_sha" ]]; then
    signal_error "Verified parent gitlink does not match the verified nested Skia head."
    exit 1
  fi
  package_git -C "$parent_repo" diff --binary --no-ext-diff --no-textconv \
    "$SKIA_SYNC_PARENT_BASE_SHA" "$parent_head_sha" |
    command -p cmp -s - "$package_dir/skiasharp.patch"
  package_git -C "$skia_repo" diff --binary --no-ext-diff --no-textconv \
    "$SKIA_SYNC_SKIA_BASE_SHA" "$skia_head_sha" |
    command -p cmp -s - "$package_dir/skia.patch"
  command -p rm -rf -- "$SKIA_SYNC_PACKAGE_GIT_HOME"

  if [[ -n "${SKIA_SYNC_DELIVERY_ENV_FILE:-}" ]]; then
    {
      printf 'SKIA_SYNC_ARTIFACT_DIR=%s\n' "$package_dir"
      printf 'SKIA_SYNC_COMPLETION_SIGNAL_FILE=%s\n' "$SKIA_SYNC_COMPLETION_SIGNAL_FILE"
      printf 'SKIA_SYNC_PARENT_REPO_DIR=%s\n' "$parent_repo"
      printf 'SKIA_SYNC_SKIA_REPO_DIR=%s\n' "$skia_repo"
      printf 'SKIA_SYNC_PARENT_BASE_SHA=%s\n' "$SKIA_SYNC_PARENT_BASE_SHA"
      printf 'SKIA_SYNC_SKIA_BASE_SHA=%s\n' "$SKIA_SYNC_SKIA_BASE_SHA"
      printf 'SKIA_SYNC_BASE_UPSTREAM_SHA=%s\n' "$SKIA_SYNC_BASE_UPSTREAM_SHA"
      printf 'SKIA_SYNC_TARGET_UPSTREAM_SHA=%s\n' "$SKIA_SYNC_TARGET_UPSTREAM_SHA"
    } >>"$SKIA_SYNC_DELIVERY_ENV_FILE"
  fi
  echo "Verified immutable Skia sync delivery package."
}

verify_detection_attestation() {
  local package_dir="${SKIA_SYNC_DELIVERY_PACKAGE_DIR:?SKIA_SYNC_DELIVERY_PACKAGE_DIR is required}"
  local detection_dir="${SKIA_SYNC_THREAT_DETECTION_DIR:?SKIA_SYNC_THREAT_DETECTION_DIR is required}"
  local detection_verified_root

  required_file "$package_dir/metadata.json"
  detection_verified_root=$(command -p mktemp -d "${RUNNER_TEMP:-/tmp}/skia-sync-detection-verified.XXXXXX")
  command -p chmod 700 "$detection_verified_root"
  GITHUB_SHA=$(jq -er .workflow_sha "$package_dir/metadata.json")
  SKIA_SYNC_CURRENT=$(jq -er .current "$package_dir/metadata.json")
  SKIA_SYNC_TARGET=$(jq -er .target "$package_dir/metadata.json")
  SKIA_SYNC_UPSTREAM_REF=$(jq -er .upstream_ref "$package_dir/metadata.json")
  SKIA_SYNC_IS_RELEASE=$(jq -er .is_release "$package_dir/metadata.json")
  SKIA_SYNC_BASE_BRANCH=$(jq -er .base_branch "$package_dir/metadata.json")
  SKIA_SYNC_SKIA_BASE_BRANCH=$(jq -er .skia_base_branch "$package_dir/metadata.json")
  SKIA_SYNC_HEAD_BRANCH=$(jq -er .head_branch "$package_dir/metadata.json")
  SKIA_SYNC_EXPECTED_PARENT_BASE_SHA=$(jq -er .parent_base_sha "$package_dir/metadata.json")
  SKIA_SYNC_EXPECTED_SKIA_BASE_SHA=$(jq -er .skia_base_sha "$package_dir/metadata.json")
  SKIA_SYNC_VERIFIED_ROOT="$detection_verified_root"
  SKIA_SYNC_DELIVERY_ENV_FILE=
  export GITHUB_SHA SKIA_SYNC_CURRENT SKIA_SYNC_TARGET SKIA_SYNC_UPSTREAM_REF
  export SKIA_SYNC_IS_RELEASE SKIA_SYNC_BASE_BRANCH SKIA_SYNC_SKIA_BASE_BRANCH
  export SKIA_SYNC_HEAD_BRANCH SKIA_SYNC_EXPECTED_PARENT_BASE_SHA
  export SKIA_SYNC_EXPECTED_SKIA_BASE_SHA SKIA_SYNC_VERIFIED_ROOT
  export SKIA_SYNC_DELIVERY_ENV_FILE
  verify_delivery_package
  command -p rm -rf -- "$detection_verified_root"
  command -p python3 -I - "$package_dir" "$detection_dir" <<'PY'
import hashlib
import os
import stat
import sys


def reject(message):
    raise SystemExit(f"::error::{message}")


def digest_regular(path):
    try:
        metadata = os.lstat(path)
    except OSError:
        reject(f"Threat-detection input is missing: {path}")
    if (not stat.S_ISREG(metadata.st_mode) or stat.S_ISLNK(metadata.st_mode)
            or metadata.st_nlink != 1 or metadata.st_size == 0):
        reject(f"Threat-detection input is not a nonempty standalone regular file: {path}")
    value = hashlib.sha256()
    with open(path, "rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            value.update(chunk)
    return value.digest()


package_dir, detection_dir = sys.argv[1:]
mapping = {
    "skiasharp.bundle": "aw-final-skiasharp.bundle",
    "skia.bundle": "aw-final-skia.bundle",
    "skiasharp.patch": "aw-final-skiasharp.patch",
    "skia.patch": "aw-final-skia.patch",
}
for package_name, detection_name in mapping.items():
    package_digest = digest_regular(os.path.join(package_dir, package_name))
    detection_digest = digest_regular(os.path.join(detection_dir, detection_name))
    if package_digest != detection_digest:
        reject(f"Threat-detection input does not match immutable delivery payload: {detection_name}")

verified_names = set(mapping.values())
for entry in os.scandir(detection_dir):
    if (entry.name.startswith("aw-")
            and (entry.name.endswith(".bundle") or entry.name.endswith(".patch"))
            and entry.name not in verified_names):
        metadata = entry.stat(follow_symlinks=False)
        if not stat.S_ISREG(metadata.st_mode) and not stat.S_ISLNK(metadata.st_mode):
            reject(f"Unexpected non-file threat-detection input cannot be removed: {entry.path}")
        os.unlink(entry.path)

remaining = {
    entry.name for entry in os.scandir(detection_dir)
    if entry.name.startswith("aw-")
    and (entry.name.endswith(".bundle") or entry.name.endswith(".patch"))
}
if remaining != verified_names:
    reject("Threat detection does not contain exactly the verified final Git inputs.")
PY
  echo "Verified detector attestation inputs against the immutable delivery package."
}

if [[ "${1:-}" == "--stage-delivery-package" ]]; then
  stage_delivery_package
  exit 0
fi

if [[ "${1:-}" == "--verify-detection-attestation" ]]; then
  verify_detection_attestation
  exit 0
fi

if [[ "${1:-}" == "--verify-delivery-package" ]]; then
  verify_delivery_package
  exit 0
fi

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

for name in "${DELIVERY_ARTIFACT_NAMES[@]}"; do
  required_file "$ARTIFACT_DIR/$name"
done

: "${SKIA_SYNC_TARGET:?SKIA_SYNC_TARGET is required}"
: "${SKIA_SYNC_CURRENT:?SKIA_SYNC_CURRENT is required}"
: "${SKIA_SYNC_UPSTREAM_REF:?SKIA_SYNC_UPSTREAM_REF is required}"
: "${SKIA_SYNC_IS_RELEASE:?SKIA_SYNC_IS_RELEASE is required}"
: "${SKIA_SYNC_SKIA_BASE_BRANCH:?SKIA_SYNC_SKIA_BASE_BRANCH is required}"
: "${SKIA_SYNC_SKIA_BASE_SHA:?SKIA_SYNC_SKIA_BASE_SHA is required}"
: "${SKIA_SYNC_BASE_UPSTREAM_SHA:?SKIA_SYNC_BASE_UPSTREAM_SHA is required}"
: "${SKIA_SYNC_TARGET_UPSTREAM_SHA:?SKIA_SYNC_TARGET_UPSTREAM_SHA is required}"
: "${RUNNER_TEMP:?RUNNER_TEMP is required}"

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
PARENT_SOURCE_REPO="${SKIA_SYNC_PARENT_REPO_DIR:-${GITHUB_WORKSPACE:?GITHUB_WORKSPACE is required}}"
SKIA_SOURCE_REPO="${SKIA_SYNC_SKIA_REPO_DIR:-$PARENT_SOURCE_REPO/externals/skia}"
readonly PARENT_SOURCE_REPO SKIA_SOURCE_REPO

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

reject_repository_state trusted_git "$PARENT_SOURCE_REPO"
reject_repository_state trusted_git "$SKIA_SOURCE_REPO"

SS_VALIDATED_HEAD_SHA=$(trusted_git -C "$PARENT_SOURCE_REPO" rev-parse "refs/heads/${HEAD_BRANCH}^{commit}")
SKIA_VALIDATED_HEAD_SHA=$(trusted_git -C "$SKIA_SOURCE_REPO" rev-parse "refs/heads/${HEAD_BRANCH}^{commit}")
readonly SS_VALIDATED_HEAD_SHA SKIA_VALIDATED_HEAD_SHA
prepare_push_repo "$PARENT_SOURCE_REPO" "$TRUSTED_SS_REPO" "$SS_VALIDATED_HEAD_SHA" "$SS_BASE_SHA"
prepare_push_repo "$SKIA_SOURCE_REPO" "$TRUSTED_SKIA_REPO" \
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

: "${GH_TOKEN:?GH_TOKEN is required}"
readonly WRITE_TOKEN="$GH_TOKEN"
unset GH_TOKEN

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
