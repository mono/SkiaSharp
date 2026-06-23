#!/usr/bin/env bash
#
# skia-sync-detect.sh — Resolve which Skia milestone and branch line the upstream
# sync targets. This is the SINGLE source of truth for branch resolution, shared by:
#
#   - the pre_activation "Detect milestone" step  → run with --gate; writes the job
#     outputs consumed by the prompt and the activation gate.
#   - the agent job "Align submodule" step        → run without --gate and sourced,
#     because that job cannot read pre_activation's outputs and must recompute them.
#
# Contract:
#   - `key=value` lines (lowercase, matching the workflow's declared outputs) are
#     appended to the file named by $SKIA_SYNC_OUT, defaulting to $GITHUB_OUTPUT.
#   - All human-readable logs and ::error::/::notice:: workflow commands go to stdout
#     (so GitHub renders annotations and the machine output stays clean for sourcing).
#
# Inputs (env):
#   INPUT_MODE        github.event.inputs.mode      (current|next|latest, optional)
#   INPUT_MILESTONE   github.event.inputs.milestone (exact number, optional override)
#   SCHEDULE          github.event.schedule         (cron string, optional)
#   GITHUB_REPOSITORY owner/repo of this SkiaSharp checkout (default GitHub env)
#   GITHUB_REF        ref whose scripts/VERSIONS.txt gives main's milestone
#   GH_TOKEN          token for `gh api`
#
# Flags:
#   --gate   Also run the gating checks (does upstream exist? is it already merged?)
#            and emit `skip=true` / exit accordingly. Only pre_activation needs this.

set -euo pipefail

GATE=false
[ "${1:-}" = "--gate" ] && GATE=true

OUT="${SKIA_SYNC_OUT:-${GITHUB_OUTPUT:?SKIA_SYNC_OUT or GITHUB_OUTPUT must be set}}"
emit() { printf '%s=%s\n' "$1" "$2" >>"$OUT"; }

# Milestone number from scripts/VERSIONS.txt at the given ref (remote read, no checkout).
milestone_of() {
  gh api "repos/${GITHUB_REPOSITORY}/contents/scripts/VERSIONS.txt?ref=$1" \
    --jq '.content' | base64 -d | grep '^libSkiaSharp.*milestone' | awk '{print $NF}'
}

# -- Mode -------------------------------------------------------------
if [ -n "${INPUT_MODE:-}" ]; then
  MODE="$INPUT_MODE"
elif [ "${SCHEDULE:-}" = "0 7 * * *" ]; then
  MODE=current
elif [ "${SCHEDULE:-}" = "0 17 * * *" ]; then
  MODE=latest
else
  MODE=next
fi

MAIN_MS=$(milestone_of "$GITHUB_REF")
NEXT=$((MAIN_MS + 1))
LATEST=$(git ls-remote --heads https://github.com/google/skia.git 'refs/heads/chrome/m*' \
  | sed -n 's|.*refs/heads/chrome/m\([0-9]*\)$|\1|p' | sort -n | tail -1)

# -- Target -----------------------------------------------------------
if [ -n "${INPUT_MILESTONE:-}" ]; then
  TARGET="$INPUT_MILESTONE"; MODE="explicit"
elif [ "$MODE" = latest ]; then
  TARGET="$LATEST"
elif [ "$MODE" = current ]; then
  TARGET="$MAIN_MS"
else
  TARGET="$NEXT"
fi

# -- Target line: `main` (newest) vs a release/<major>.<TARGET>.x maintenance line.
# A maintenance line lives under the SAME branch name in both mono/SkiaSharp and
# mono/skia. We only look for one when TARGET is strictly OLDER than main's milestone;
# the newest line is always served by `main`. The release branch itself is owned by
# the release process (release-branch skill), NOT this sync.
IS_RELEASE=false
BASE_BRANCH=main
SKIA_BASE_BRANCH=skiasharp
HEAD_BRANCH="skia-sync/m${TARGET}"
RELEASE_BRANCH=""
# `2>/dev/null` swallows the "integer expression expected" noise for a non-numeric
# TARGET, which then falls through to the main line.
if [ "$TARGET" -lt "$MAIN_MS" ] 2>/dev/null; then
  RELEASE_BRANCH=$(git ls-remote --heads "https://github.com/${GITHUB_REPOSITORY}.git" \
      "refs/heads/release/*.${TARGET}.x" \
    | sed -n 's|.*refs/heads/\(release/[0-9][0-9.]*\.x\)$|\1|p' | sort -u)
  # The glob can match more than one major (e.g. release/4.148.x and a stray
  # release/14.148.x). Refuse to guess — fail so a human disambiguates.
  if [ "$(printf '%s' "$RELEASE_BRANCH" | grep -c . || true)" -gt 1 ]; then
    echo "::error::Multiple release branches match milestone ${TARGET}: $(echo $RELEASE_BRANCH) — cannot disambiguate."
    exit 1
  fi
fi
if [ -n "$RELEASE_BRANCH" ]; then
  IS_RELEASE=true
  BASE_BRANCH="$RELEASE_BRANCH"
  SKIA_BASE_BRANCH="$RELEASE_BRANCH"
  HEAD_BRANCH="skia-sync/${RELEASE_BRANCH//\//-}"
  # The matching mono/skia release branch MUST already exist.
  if [ -z "$(git ls-remote --heads https://github.com/mono/skia.git "refs/heads/${SKIA_BASE_BRANCH}" | awk '{print $1}')" ]; then
    echo "::error::mono/skia branch '${SKIA_BASE_BRANCH}' does not exist. Release branches are owned by the release process (release-branch skill) — create it before running a release sync for m${TARGET}."
    exit 1
  fi
fi

# `current` = milestone of the BASE branch we sync INTO:
#   - main line → main's milestone
#   - release   → that line's milestone (== TARGET ⇒ a bug-fix-only sync)
if [ "$IS_RELEASE" = true ]; then
  CURRENT=$(milestone_of "$BASE_BRANCH")
else
  CURRENT="$MAIN_MS"
fi

emit mode "$MODE"
emit next "$NEXT"
emit latest "$LATEST"
emit target "$TARGET"
emit is_release "$IS_RELEASE"
emit base_branch "$BASE_BRANCH"
emit skia_base_branch "$SKIA_BASE_BRANCH"
emit head_branch "$HEAD_BRANCH"
emit current "$CURRENT"

echo "Resolved: m${TARGET} → ${BASE_BRANCH} (mode=${MODE}, base milestone=m${CURRENT}, latest=m${LATEST}, head=${HEAD_BRANCH}, release=${IS_RELEASE})"

# Branch derivation is all the agent job needs; gating is pre_activation-only.
$GATE || exit 0

# -- Gate: only spin up the (expensive) agent when there is new upstream work ----
UPSTREAM_SHA=$(git ls-remote https://github.com/google/skia.git "refs/heads/chrome/m${TARGET}" | awk '{print $1}')
if [ -z "$UPSTREAM_SHA" ]; then
  echo "::notice::upstream/chrome/m${TARGET} does not exist yet"
  [ "$MODE" = explicit ] && exit 1
  emit skip true
  exit 0
fi

# Compare upstream HEAD against the existing sync branch if present; otherwise, for a
# release line, against the release base branch. A main milestone bump has no sync
# branch yet and always has work, so it proceeds.
SYNC_SHA=$(git ls-remote https://github.com/mono/skia.git "refs/heads/${HEAD_BRANCH}" | awk '{print $1}')
COMPARE_REF=""
if [ -n "$SYNC_SHA" ]; then
  COMPARE_REF="$HEAD_BRANCH"
elif [ "$IS_RELEASE" = true ]; then
  COMPARE_REF="$SKIA_BASE_BRANCH"
fi
if [ -n "$COMPARE_REF" ]; then
  BEHIND=$(gh api "repos/mono/skia/compare/${UPSTREAM_SHA}...${COMPARE_REF}" --jq '.behind_by' 2>/dev/null || echo unknown)
  if [ "$BEHIND" = 0 ]; then
    echo "::notice::chrome/m${TARGET} already fully merged into ${COMPARE_REF} (upstream HEAD: ${UPSTREAM_SHA:0:12}) — skipping"
    emit skip true
    exit 0
  fi
  echo "${COMPARE_REF} exists but is ${BEHIND} commits behind upstream"
fi
