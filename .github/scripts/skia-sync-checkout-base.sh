#!/usr/bin/env bash
#
# Materialize the exact selected parent base and all recursive submodule gitlinks
# in an isolated workspace. The workflow checkout remains the automation source.

set -euo pipefail

SOURCE_WORKSPACE="${GITHUB_WORKSPACE:?GITHUB_WORKSPACE not set}"
TARGET_WORKSPACE="${SKIA_SYNC_WORKSPACE:?SKIA_SYNC_WORKSPACE not set}"
BASE_BRANCH="${base_branch:?base_branch not set; source skia-sync-detect.sh first}"

if [[ "$SOURCE_WORKSPACE" == "$TARGET_WORKSPACE" ]]; then
  echo "::error::SKIA_SYNC_WORKSPACE must be isolated from GITHUB_WORKSPACE."
  exit 1
fi
if [[ -e "$TARGET_WORKSPACE" ]]; then
  echo "::error::The isolated sync workspace already exists: $TARGET_WORKSPACE"
  exit 1
fi

git -C "$SOURCE_WORKSPACE" fetch --no-tags origin \
  "+refs/heads/${BASE_BRANCH}:refs/remotes/origin/${BASE_BRANCH}"
PARENT_BASE_SHA=$(git -C "$SOURCE_WORKSPACE" rev-parse "refs/remotes/origin/${BASE_BRANCH}^{commit}")
ORIGIN_URL=$(git -C "$SOURCE_WORKSPACE" remote get-url origin)

git clone --no-checkout --no-hardlinks "$SOURCE_WORKSPACE" "$TARGET_WORKSPACE"
git -C "$TARGET_WORKSPACE" remote set-url origin "$ORIGIN_URL"
git -C "$TARGET_WORKSPACE" fetch --no-tags origin \
  "+refs/heads/${BASE_BRANCH}:refs/remotes/origin/${BASE_BRANCH}"
git -C "$TARGET_WORKSPACE" checkout --detach "$PARENT_BASE_SHA"
git -C "$TARGET_WORKSPACE" submodule sync --recursive
git -C "$TARGET_WORKSPACE" -c protocol.file.allow=always \
  submodule update --init --recursive

ACTUAL_PARENT_SHA=$(git -C "$TARGET_WORKSPACE" rev-parse HEAD)
if [[ "$ACTUAL_PARENT_SHA" != "$PARENT_BASE_SHA" ]]; then
  echo "::error::Isolated checkout resolved $ACTUAL_PARENT_SHA instead of $PARENT_BASE_SHA."
  exit 1
fi

SUBMODULE_STATUS=$(git -C "$TARGET_WORKSPACE" submodule status --recursive)
if grep -Eq '^[+-U]' <<<"$SUBMODULE_STATUS"; then
  echo "::error::A recursive submodule does not match the selected parent tree:"
  printf '%s\n' "$SUBMODULE_STATUS"
  exit 1
fi

echo "Prepared isolated parent base ${PARENT_BASE_SHA} at ${TARGET_WORKSPACE}"
printf '%s\n' "$SUBMODULE_STATUS"
