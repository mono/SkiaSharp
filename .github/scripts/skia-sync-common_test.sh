#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
# shellcheck source=/dev/null
source "$SCRIPT_DIR/skia-sync-common.sh"

ROOT=$(mktemp -d)
trap 'rm -rf "$ROOT"' EXIT
PARENT="$ROOT/parent"
SKIA_SOURCE="$ROOT/skia-source"
ENV_FILE="$ROOT/skia-sync-env.sh"
HEAD_BRANCH="skia-sync/test"

configure_repo() {
  git -C "$1" config user.email "sync-test@example.com"
  git -C "$1" config user.name "Sync Test"
}

git init -q "$SKIA_SOURCE"
configure_repo "$SKIA_SOURCE"
echo "skia" >"$SKIA_SOURCE/file.txt"
git -C "$SKIA_SOURCE" add file.txt
git -C "$SKIA_SOURCE" commit -q -m "skia base"
git -C "$SKIA_SOURCE" switch -q -c "$HEAD_BRANCH"

git init -q "$PARENT"
configure_repo "$PARENT"
echo "parent" >"$PARENT/parent.txt"
git -C "$PARENT" -c protocol.file.allow=always submodule add -q "$SKIA_SOURCE" externals/skia
configure_repo "$PARENT/externals/skia"
git -C "$PARENT/externals/skia" switch -q "$HEAD_BRANCH"
git -C "$PARENT" add .gitmodules externals/skia parent.txt
git -C "$PARENT" commit -q -m "parent base"
git -C "$PARENT" switch -q -c "$HEAD_BRANCH"

SKIA_SHA=$(git -C "$PARENT/externals/skia" rev-parse HEAD)
cat >"$ENV_FILE" <<EOF
TARGET=200
CURRENT=199
UPSTREAM_REF=chrome/m200
IS_RELEASE=false
BASE_BRANCH=main
SKIA_BASE_BRANCH=skiasharp
SKIA_BASE_SHA=$SKIA_SHA
HEAD_BRANCH=$HEAD_BRANCH
BASE_UPSTREAM_SHA=1111111111111111111111111111111111111111
TARGET_UPSTREAM_SHA=2222222222222222222222222222222222222222
PARENT_REMOTE_HEAD=none
SKIA_REMOTE_HEAD=none
EOF

load_sync_env "$ENV_FILE"
validate_sync_checkout "$PARENT"

MALICIOUS="$ROOT/malicious-env.sh"
MARKER="$ROOT/must-not-exist"
{
  printf 'TARGET=$(touch %s)\n' "$MARKER"
  tail -n +2 "$ENV_FILE"
} >"$MALICIOUS"
if (load_sync_env "$MALICIOUS"); then
  echo "Malicious environment unexpectedly validated." >&2
  exit 1
fi
if [[ -e "$MARKER" ]]; then
  echo "Environment parser executed agent-controlled content." >&2
  exit 1
fi

echo "dirty" >>"$PARENT/tracked.txt"
if (validate_sync_checkout "$PARENT"); then
  echo "Dirty checkout unexpectedly validated." >&2
  exit 1
fi
rm "$PARENT/tracked.txt"

git -C "$PARENT" update-index --skip-worktree parent.txt
if (validate_sync_checkout "$PARENT"); then
  echo "Concealed index entry unexpectedly validated." >&2
  exit 1
fi
git -C "$PARENT" update-index --no-skip-worktree parent.txt

echo "skia sync common tests passed"
