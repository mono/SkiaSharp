#!/usr/bin/env bash
#
# Prepare the mono/skia checkout for the agent's breaking-change analysis.
# The detector output must be sourced before invoking this script.

set -euo pipefail

BASE_BRANCH="${base_branch:?base_branch not set}"
CURRENT="${current:?current not set}"
UPSTREAM_REF="${upstream_ref:?upstream_ref not set}"
SKIA_DIR="${GITHUB_WORKSPACE:?GITHUB_WORKSPACE not set}/externals/skia"

if git -C "$SKIA_DIR" remote get-url upstream >/dev/null 2>&1; then
  git -C "$SKIA_DIR" remote set-url upstream https://github.com/google/skia.git
else
  git -C "$SKIA_DIR" remote add upstream https://github.com/google/skia.git
fi

fetch_upstream_ref() {
  local ref="$1"
  git -C "$SKIA_DIR" fetch --no-tags upstream \
    "+refs/heads/${ref}:refs/remotes/upstream/${ref}"
}

fetch_upstream_ref "$UPSTREAM_REF"
if [[ "$UPSTREAM_REF" != "chrome/m${CURRENT}" ]]; then
  if ! fetch_upstream_ref "chrome/m${CURRENT}"; then
    echo "chrome/m${CURRENT} is unavailable; the recorded commit will be fetched directly."
  fi
fi

BASE_UPSTREAM_SHA=$(
  git -C "$GITHUB_WORKSPACE" show "origin/${BASE_BRANCH}:cgmanifest.json" |
    jq -er '.registrations[]
      | select(.component.type == "other" and .component.other.name == "skia")
      | .upstream_merge_commit'
)

if ! git -C "$SKIA_DIR" cat-file -e "${BASE_UPSTREAM_SHA}^{commit}" 2>/dev/null; then
  # The recorded commit should normally arrive with chrome/m<CURRENT>. Fetching it
  # explicitly also supports old maintenance lines whose branch history was pruned.
  git -C "$SKIA_DIR" fetch --no-tags upstream "$BASE_UPSTREAM_SHA"
fi

git -C "$SKIA_DIR" cat-file -e "${BASE_UPSTREAM_SHA}^{commit}"
TARGET_UPSTREAM_SHA=$(git -C "$SKIA_DIR" rev-parse "upstream/${UPSTREAM_REF}^{commit}")

{
  echo "SKIA_BASE_UPSTREAM_SHA=$BASE_UPSTREAM_SHA"
  echo "SKIA_TARGET_UPSTREAM_REF=upstream/$UPSTREAM_REF"
} >>"${GITHUB_ENV:?GITHUB_ENV not set}"

echo "Prepared upstream analysis range: ${BASE_UPSTREAM_SHA}..upstream/${UPSTREAM_REF}"
echo "Target upstream SHA: ${TARGET_UPSTREAM_SHA}"
