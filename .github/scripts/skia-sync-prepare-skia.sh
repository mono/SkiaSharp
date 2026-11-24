#!/usr/bin/env bash
#
# skia-sync-prepare-skia.sh — Prepare the mono/skia checkout for the sync agent.
#
# The agent job checks out the workflow branch (usually main), so the submodule may
# sit at a different SHA than the base branch (`main` or a release line) expects. The
# submodule tracks `$skia_base_branch` in mono/skia (skiasharp for a main sync,
# release/<major>.<milestone>.x for a release sync).
#
# Two native commits matter and are kept distinct:
#   - the parent GITLINK: what the parent base's metadata (cgmanifest, DEPS bookkeeping)
#     currently describes;
#   - the native BASE TIP: the current tip of origin/$skia_base_branch, where the sync
#     merge starts so the result contains everything already merged natively.
# The parent may lag the native branch (e.g. a native servicing PR merged before its parent
# repin); the gitlink must then be an ancestor of the tip. Nothing is silently dropped:
# a diverged or different-milestone native base is a hard error.
#
# Then fetch the old and target google/skia commits needed for the agent's
# breaking-change analysis and export their exact SHAs to GITHUB_ENV.
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

echo "Resolving native base: origin/${BASE_BRANCH} gitlink, mono/skia ${SKIA_BASE_BRANCH} tip"
git -C "$WORKSPACE" fetch origin "$BASE_BRANCH" 2>&1
PARENT_BASE_SHA=$(git -C "$WORKSPACE" rev-parse "origin/${BASE_BRANCH}^{commit}")
PARENT_SKIA_SHA=$(git -C "$WORKSPACE" ls-tree "origin/${BASE_BRANCH}" -- externals/skia | awk '{print $3}')
if [ -z "$PARENT_SKIA_SHA" ]; then
  echo "::error::origin/${BASE_BRANCH} does not contain the externals/skia submodule."
  exit 1
fi
echo "origin/${BASE_BRANCH} submodule gitlink: $PARENT_SKIA_SHA"
git -C "$SKIA_DIR" fetch origin "$SKIA_BASE_BRANCH" 2>&1
SKIA_BASE_SHA=$(git -C "$SKIA_DIR" rev-parse "origin/${SKIA_BASE_BRANCH}^{commit}")
echo "origin/${SKIA_BASE_BRANCH} tip: $SKIA_BASE_SHA"
if ! git -C "$SKIA_DIR" merge-base --is-ancestor "$PARENT_SKIA_SHA" "$SKIA_BASE_SHA"; then
  echo "::error::The parent gitlink ${PARENT_SKIA_SHA} is not contained in origin/${SKIA_BASE_BRANCH} (${SKIA_BASE_SHA}); the parent and native bases have diverged. Repair the base before syncing."
  exit 1
fi
git -C "$SKIA_DIR" checkout --detach "$SKIA_BASE_SHA" 2>&1
if [ "$SKIA_BASE_SHA" = "$PARENT_SKIA_SHA" ]; then
  echo "  ✅ parent gitlink is the tip of origin/${SKIA_BASE_BRANCH}"
else
  echo "::warning::Parent origin/${BASE_BRANCH} lags origin/${SKIA_BASE_BRANCH}; the sync will merge from the native tip ${SKIA_BASE_SHA}, not the parent gitlink."
fi

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

# Upstream commit already merged into the native base: the old side of the fork-patch audit,
# which must describe the same tree as the fork base. BASE_UPSTREAM_SHA stays the parent's
# recorded commit so the breaking-change range covers everything the parent has not seen.
if [ "$SKIA_BASE_SHA" = "$PARENT_SKIA_SHA" ]; then
  SKIA_BASE_UPSTREAM_SHA="$BASE_UPSTREAM_SHA"
else
  NATIVE_MILESTONE=$(git -C "$SKIA_DIR" show "${SKIA_BASE_SHA}:include/core/SkMilestone.h" |
    sed -n 's/^#define SK_MILESTONE[[:space:]]*\([0-9][0-9]*\).*/\1/p')
  if [ "$NATIVE_MILESTONE" != "$CURRENT" ]; then
    echo "::error::origin/${SKIA_BASE_BRANCH} is at milestone m${NATIVE_MILESTONE:-?} but parent origin/${BASE_BRANCH} ships m${CURRENT}. Repin the parent to the native base before syncing."
    exit 1
  fi
  if ! git -C "$SKIA_DIR" rev-parse --verify --quiet "upstream/chrome/m${CURRENT}^{commit}" >/dev/null; then
    echo "::error::upstream/chrome/m${CURRENT} is required to find the upstream commit merged into the lagging native base."
    exit 1
  fi
  SKIA_BASE_UPSTREAM_SHA=$(git -C "$SKIA_DIR" merge-base "$SKIA_BASE_SHA" "upstream/chrome/m${CURRENT}")
  if ! git -C "$SKIA_DIR" merge-base --is-ancestor "$BASE_UPSTREAM_SHA" "$SKIA_BASE_UPSTREAM_SHA"; then
    echo "::error::Upstream commit merged into origin/${SKIA_BASE_BRANCH} (${SKIA_BASE_UPSTREAM_SHA}) does not contain the parent's recorded ${BASE_UPSTREAM_SHA}."
    exit 1
  fi
fi
echo "Native base upstream commit: ${SKIA_BASE_UPSTREAM_SHA}"
TARGET_UPSTREAM_SHA=$(git -C "$SKIA_DIR" rev-parse "upstream/${UPSTREAM_REF}^{commit}")

{
  echo "SKIA_SYNC_PARENT_BASE_SHA=$PARENT_BASE_SHA"
  echo "SKIA_SYNC_PARENT_SKIA_SHA=$PARENT_SKIA_SHA"
  echo "SKIA_SYNC_SKIA_BASE_SHA=$SKIA_BASE_SHA"
  echo "SKIA_SYNC_SKIA_BASE_UPSTREAM_SHA=$SKIA_BASE_UPSTREAM_SHA"
  echo "SKIA_BASE_UPSTREAM_SHA=$BASE_UPSTREAM_SHA"
  echo "SKIA_TARGET_UPSTREAM_REF=upstream/$UPSTREAM_REF"
  echo "SKIA_SYNC_BASE_UPSTREAM_SHA=$BASE_UPSTREAM_SHA"
  echo "SKIA_SYNC_TARGET_UPSTREAM_SHA=$TARGET_UPSTREAM_SHA"
} >>"$ENV_FILE"

echo "Prepared upstream analysis range: ${BASE_UPSTREAM_SHA}..upstream/${UPSTREAM_REF}"
echo "Target upstream SHA: ${TARGET_UPSTREAM_SHA}"
