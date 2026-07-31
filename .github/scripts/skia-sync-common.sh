#!/usr/bin/env bash

sync_error() {
  echo "::error::$*" >&2
}

read_sync_value() {
  local file="$1"
  local key="$2"
  local pattern="$3"
  local lines=()
  local value

  mapfile -t lines < <(grep -E "^${key}=" "$file" || true)
  if [[ "${#lines[@]}" -ne 1 ]]; then
    sync_error "$file must contain exactly one ${key}=... assignment."
    return 1
  fi

  value="${lines[0]#*=}"
  if [[ ! "$value" =~ $pattern ]]; then
    sync_error "$file contains an invalid value for ${key}."
    return 1
  fi
  printf -v "$key" '%s' "$value"
}

load_sync_env() {
  local file="$1"
  local count

  if [[ ! -s "$file" ]]; then
    sync_error "Sync environment file is missing or empty: $file"
    return 1
  fi

  count=$(awk 'NF { count++ } END { print count + 0 }' "$file")
  if [[ "$count" -ne 12 ]]; then
    sync_error "$file must contain exactly the 12 supported assignments."
    return 1
  fi

  read_sync_value "$file" TARGET '^[0-9]+$' || return 1
  read_sync_value "$file" CURRENT '^[0-9]+$' || return 1
  read_sync_value "$file" UPSTREAM_REF '^[A-Za-z0-9._/-]+$' || return 1
  read_sync_value "$file" IS_RELEASE '^(true|false)$' || return 1
  read_sync_value "$file" BASE_BRANCH '^[A-Za-z0-9._/-]+$' || return 1
  read_sync_value "$file" SKIA_BASE_BRANCH '^[A-Za-z0-9._/-]+$' || return 1
  read_sync_value "$file" SKIA_BASE_SHA '^[0-9a-f]{40}$' || return 1
  read_sync_value "$file" HEAD_BRANCH '^[A-Za-z0-9._/-]+$' || return 1
  read_sync_value "$file" BASE_UPSTREAM_SHA '^[0-9a-f]{40}$' || return 1
  read_sync_value "$file" TARGET_UPSTREAM_SHA '^[0-9a-f]{40}$' || return 1
  read_sync_value "$file" PARENT_REMOTE_HEAD '^(none|[0-9a-f]{40})$' || return 1
  read_sync_value "$file" SKIA_REMOTE_HEAD '^(none|[0-9a-f]{40})$' || return 1
}

validate_sync_checkout() {
  local workspace="$1"
  local parent_branch
  local skia_branch
  local parent_head
  local skia_head
  local parent_ref
  local skia_ref
  local gitlink
  local hidden
  local replacements
  local status

  parent_branch=$(git -C "$workspace" branch --show-current)
  skia_branch=$(git -C "$workspace/externals/skia" branch --show-current)
  if [[ "$parent_branch" != "$HEAD_BRANCH" || "$skia_branch" != "$HEAD_BRANCH" ]]; then
    sync_error "Both repositories must be checked out on $HEAD_BRANCH (parent=$parent_branch, skia=$skia_branch)."
    return 1
  fi

  parent_head=$(git -C "$workspace" rev-parse HEAD)
  skia_head=$(git -C "$workspace/externals/skia" rev-parse HEAD)
  parent_ref=$(git -C "$workspace" rev-parse "refs/heads/${HEAD_BRANCH}")
  skia_ref=$(git -C "$workspace/externals/skia" rev-parse "refs/heads/${HEAD_BRANCH}")
  if [[ "$parent_head" != "$parent_ref" || "$skia_head" != "$skia_ref" ]]; then
    sync_error "Checked-out commits do not match the branch refs that will be pushed."
    return 1
  fi

  replacements=$(git -C "$workspace" replace -l)
  hidden=$(git -C "$workspace" ls-files -v | grep -E '^([a-z]|S) ' || true)
  if [[ -n "$replacements" || -n "$hidden" ]]; then
    sync_error "Parent repository uses replacement refs or concealed index entries."
    return 1
  fi

  replacements=$(git -C "$workspace/externals/skia" replace -l)
  hidden=$(git -C "$workspace/externals/skia" ls-files -v | grep -E '^([a-z]|S) ' || true)
  if [[ -n "$replacements" || -n "$hidden" ]]; then
    sync_error "mono/skia uses replacement refs or concealed index entries."
    return 1
  fi

  status=$(git -C "$workspace" status --porcelain --untracked-files=all --ignore-submodules=dirty)
  if [[ -n "$status" ]]; then
    sync_error "Parent worktree has uncommitted files that are not represented by $HEAD_BRANCH:"
    printf '%s\n' "$status" >&2
    return 1
  fi

  status=$(git -C "$workspace/externals/skia" status --porcelain --untracked-files=all --ignore-submodules=dirty)
  if [[ -n "$status" ]]; then
    sync_error "mono/skia worktree has uncommitted files that are not represented by $HEAD_BRANCH:"
    printf '%s\n' "$status" >&2
    return 1
  fi

  gitlink=$(git -C "$workspace" ls-tree HEAD -- externals/skia | awk '{print $3}')
  if [[ -z "$gitlink" || "$gitlink" != "$skia_head" ]]; then
    sync_error "Parent gitlink ($gitlink) does not match the tested mono/skia commit ($skia_head)."
    return 1
  fi
}
