#!/usr/bin/env bash
#
# skia-sync-align-submodule.sh — Point externals/skia at the SHA the base branch
# records, before the agent runs.
#
# The agent job checks out the workflow branch (usually main), so the submodule may
# sit at a different SHA than the base branch (`main` or a release line) expects. The
# submodule tracks `$skia_base_branch` in mono/skia (skiasharp for a main sync,
# release/<major>.<milestone>.x for a release sync), so the base-branch submodule SHA
# should be a commit on that branch.
#
# Requires (export these by sourcing skia-sync-detect.sh's output first):
#   base_branch        parent base branch (main or release/<major>.<ms>.x)
#   skia_base_branch   mono/skia base branch (skiasharp or release/<major>.<ms>.x)

set -euo pipefail

BASE_BRANCH="${base_branch:?base_branch not set — source skia-sync-detect.sh first}"
SKIA_BASE_BRANCH="${skia_base_branch:?skia_base_branch not set — source skia-sync-detect.sh first}"

echo "Aligning submodule to origin/${BASE_BRANCH} (mono/skia ${SKIA_BASE_BRANCH})"
git fetch origin "$BASE_BRANCH" 2>&1 || true
BASE_SUB_SHA=$(git ls-tree "origin/${BASE_BRANCH}" -- externals/skia | awk '{print $3}')
echo "origin/${BASE_BRANCH} submodule SHA: $BASE_SUB_SHA"
git -C externals/skia fetch origin "$SKIA_BASE_BRANCH" 2>&1
git -C externals/skia checkout "$BASE_SUB_SHA" 2>&1
echo "Verifying SHA is on ${SKIA_BASE_BRANCH} branch:"
git -C externals/skia branch -r --contains "$BASE_SUB_SHA" | grep -q "origin/${SKIA_BASE_BRANCH}" \
  && echo "  ✅ SHA is on origin/${SKIA_BASE_BRANCH}" \
  || echo "  ⚠️ SHA is NOT on origin/${SKIA_BASE_BRANCH} — submodule pointer may be stale"
