#!/usr/bin/env bash
#
# skia-sync-prepare-skia.sh — Prepare the mono/skia checkout for the sync agent.
#
# The agent job checks out the workflow branch (usually main), so the submodule may
# sit at a different SHA than the base branch (`main` or a release line) expects. The
# submodule tracks `$skia_base_branch` in mono/skia (skiasharp for a main sync,
# release/<major>.<milestone>.x for a release sync), so the base-branch submodule SHA
# should be a commit on that branch.
#
# After aligning that exact gitlink, fetch the old and target google/skia commits needed
# for the agent's breaking-change analysis and export their exact SHAs to GITHUB_ENV.
#
# Requires (source skia-sync-detect.sh's output first):
#   base_branch        parent base branch (main or release/<major>.<ms>.x)
#   skia_base_branch   mono/skia base branch (skiasharp or release/<major>.<ms>.x)
#   current            milestone currently shipped by the base branch
#   upstream_ref       google/skia branch to merge
#   GITHUB_WORKSPACE   parent repository checkout
#   GITHUB_ENV         environment file for subsequent workflow steps

set -euo pipefail

BASE_BRANCH="${base_branch:?base_branch not set — source skia-sync-detect.sh first}"
SKIA_BASE_BRANCH="${skia_base_branch:?skia_base_branch not set — source skia-sync-detect.sh first}"
CURRENT="${current:?current not set — source skia-sync-detect.sh first}"
UPSTREAM_REF="${upstream_ref:?upstream_ref not set — source skia-sync-detect.sh first}"
WORKSPACE="${GITHUB_WORKSPACE:?GITHUB_WORKSPACE not set}"
ENV_FILE="${GITHUB_ENV:?GITHUB_ENV not set}"
SKIA_DIR="${WORKSPACE}/externals/skia"

echo "Aligning submodule to origin/${BASE_BRANCH} (mono/skia ${SKIA_BASE_BRANCH})"
git -C "$WORKSPACE" fetch origin "$BASE_BRANCH" 2>&1
BASE_SUB_SHA=$(git -C "$WORKSPACE" ls-tree "origin/${BASE_BRANCH}" -- externals/skia | awk '{print $3}')
if [ -z "$BASE_SUB_SHA" ]; then
  echo "::error::origin/${BASE_BRANCH} does not contain the externals/skia submodule."
  exit 1
fi
echo "origin/${BASE_BRANCH} submodule SHA: $BASE_SUB_SHA"
git -C "$SKIA_DIR" fetch origin "$SKIA_BASE_BRANCH" 2>&1
git -C "$SKIA_DIR" checkout "$BASE_SUB_SHA" 2>&1
echo "Verifying SHA is on ${SKIA_BASE_BRANCH} branch:"
if ! git -C "$SKIA_DIR" branch -r --contains "$BASE_SUB_SHA" | grep -q "origin/${SKIA_BASE_BRANCH}"; then
  echo "::error::The base submodule SHA is not on origin/${SKIA_BASE_BRANCH}."
  exit 1
fi
echo "  ✅ SHA is on origin/${SKIA_BASE_BRANCH}"

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
  git -C "$WORKSPACE" show "origin/${BASE_BRANCH}:cgmanifest.json" |
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
  echo "SKIA_SYNC_BASE_UPSTREAM_SHA=$BASE_UPSTREAM_SHA"
  echo "SKIA_SYNC_TARGET_UPSTREAM_SHA=$TARGET_UPSTREAM_SHA"
} >>"$ENV_FILE"

echo "Prepared upstream analysis range: ${BASE_UPSTREAM_SHA}..upstream/${UPSTREAM_REF}"
echo "Target upstream SHA: ${TARGET_UPSTREAM_SHA}"
