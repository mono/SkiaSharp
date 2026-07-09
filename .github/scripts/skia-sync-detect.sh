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
# Args:
#   --target <""|main|number>      What to sync. There are only two shapes:
#                                    * empty  → ROTATE: pick one supported versions.json
#                                               line per run, round-robin (rotate_select).
#                                               This is the scheduled default.
#                                    * main   → the bleeding-edge tip: merge google/skia's
#                                               `main` HEAD (not a chrome/m<N> branch) into
#                                               the newest line. NOT a version bump — the
#                                               target milestone stays == main's milestone.
#                                    * number → an exact milestone (e.g. 151); merge
#                                               chrome/m<number>.
#   --gate                         Also run the gating checks (does upstream exist? is it
#                                  already merged?) and emit `skip=true` / exit
#                                  accordingly. Only pre_activation needs this.
#
# Inputs (env):
#   GITHUB_REPOSITORY  owner/repo of this SkiaSharp checkout (default GitHub env)
#   GITHUB_SHA         immutable triggering commit; main's config is read here so the two
#                      detector invocations of a run agree (falls back to GITHUB_REF)
#   GITHUB_REF         branch ref (fallback, and used for release-line reads)
#   GITHUB_RUN_NUMBER  per-workflow run counter; the rotation round-robin index
#   GH_TOKEN           token for `gh api`

set -euo pipefail

GATE=false
SYNC_TARGET=""
while [ $# -gt 0 ]; do
  case "$1" in
    --gate) GATE=true; shift ;;
    --target)
      [ $# -ge 2 ] || { echo "::error::skia-sync-detect.sh: --target requires a value"; exit 2; }
      SYNC_TARGET="$2"; shift 2 ;;
    *) echo "::error::skia-sync-detect.sh: unknown argument '$1'"; exit 2 ;;
  esac
done

OUT="${SKIA_SYNC_OUT:-${GITHUB_OUTPUT:?SKIA_SYNC_OUT or GITHUB_OUTPUT must be set}}"
emit() { printf '%s=%s\n' "$1" "$2" >>"$OUT"; }

# Milestone number from scripts/VERSIONS.txt at the given ref (remote read, no checkout).
milestone_of() {
  gh api "repos/${GITHUB_REPOSITORY}/contents/scripts/VERSIONS.txt?ref=$1" \
    --jq '.content' | base64 -d | grep '^libSkiaSharp.*milestone' | awk '{print $NF}'
}

# -- Milestone facts --------------------------------------------------
# SYNC_TARGET (the single dispatch input) is empty for scheduled/rotation runs, `main`
# for a bleeding-edge tip sync, or an exact milestone number. MAIN_MS is main's shipped
# milestone; NEXT (= MAIN_MS + 1) is the rotation's leading-edge fallback.
#
# Read main's config at the IMMUTABLE triggering commit ($GITHUB_SHA), not the branch
# ref: both the pre_activation gate and the agent-job align step re-run this script, and
# a branch (GITHUB_REF) could advance between them and make them disagree. (Release-line
# reads below intentionally use the branch name, to pick up that line's own tip.)
REF_IMMUTABLE="${GITHUB_SHA:-$GITHUB_REF}"
MAIN_MS=$(milestone_of "$REF_IMMUTABLE")
NEXT=$((MAIN_MS + 1))

# -- Rotation helpers -------------------------------------------------
# The rotation (used by every scheduled run) drives the sync set from the hand-maintained
# `support` block in scripts/infra/docs/versions.json — the single source of truth for
# which lines we actually ship — instead of a fixed cron→mode map.

# versions.json `support` block, read remotely at the immutable triggering commit
# (pre_activation only sparse-checks out .github/scripts, so the file is not on disk).
support_json() {
  gh api "repos/${GITHUB_REPOSITORY}/contents/scripts/infra/docs/versions.json?ref=${REF_IMMUTABLE}" \
    --jq '.content' | base64 -d
}
# Has upstream cut the chrome/m<N> milestone branch yet? Fail hard on a lookup error so a
# transient git failure can't be mistaken for "branch absent" and silently fall back to main.
chrome_branch_exists() {
  local out
  if ! out=$(git ls-remote --heads https://github.com/google/skia.git "refs/heads/chrome/m$1"); then
    echo "::error::git ls-remote failed while checking upstream chrome/m$1"
    exit 1
  fi
  [ -n "$out" ]
}

# rotate_select: enumerate the supported lines (stables ascending, then previews
# ascending, then the single leading-edge `next`) and pick ONE per run, round-robin.
#
# The picker is GITHUB_RUN_NUMBER, which increments by exactly 1 per run of THIS workflow
# and is identical across every job/step of a run. So the round-robin is fully
# deterministic between the pre_activation gate and the agent-job align step (separate
# jobs that each re-run this script) — no wall-clock read, so no risk of the two steps
# landing on different targets near a time boundary. It advances one line per run; with a
# 6-hourly schedule that is one line every 6h, cycling through a list of ANY length.
#
# `next` = highest supported milestone + 1: if upstream has a chrome/m<N> branch it is
# a real milestone bump into main; otherwise it falls back to the bleeding-edge main tip
# (head skia-sync/main). The skip-gate no-ops any run that has nothing new, so a run
# landing on an up-to-date line is essentially free.
rotate_select() {
  local support next_ms specs len idx chosen rest m run_number
  local -a stable_ms preview_ms
  support=$(support_json)
  readarray -t stable_ms < <(printf '%s' "$support" \
    | jq -r '[.support.stable[]?  | (split(".")[1]|tonumber)] | sort | .[]')
  readarray -t preview_ms < <(printf '%s' "$support" \
    | jq -r '[.support.preview[]? | (split(".")[1]|tonumber)] | sort | .[]')

  specs=()
  for m in "${stable_ms[@]}";  do [ -n "$m" ] && specs+=("stable|$m|chrome/m$m"); done
  for m in "${preview_ms[@]}"; do [ -n "$m" ] && specs+=("preview|$m|chrome/m$m"); done

  next_ms=$(printf '%s' "$support" \
    | jq -r '[.support.stable[]?, .support.preview[]? | (split(".")[1]|tonumber)] | if length>0 then (max+1) else empty end')
  [ -n "$next_ms" ] || next_ms="$NEXT"
  if chrome_branch_exists "$next_ms"; then
    specs+=("next|$next_ms|chrome/m$next_ms")
  else
    specs+=("main|$MAIN_MS|main")
  fi

  len=${#specs[@]}
  if [ "$len" -eq 0 ]; then
    echo "::error::rotation produced no targets — versions.json 'support' block is empty."
    exit 1
  fi

  # Deterministic round-robin index — see the function header (GITHUB_RUN_NUMBER is stable
  # across both detector invocations of a run). Defaults to 0 outside Actions.
  run_number="${GITHUB_RUN_NUMBER:-0}"
  idx=$(( run_number % len ))
  chosen="${specs[$idx]}"
  MODE="${chosen%%|*}"
  rest="${chosen#*|}"
  TARGET="${rest%%|*}"
  UPSTREAM_REF="${rest#*|}"
  echo "Rotation: ${len} target(s) [$(IFS=' '; echo "${specs[*]}")]; run=${run_number} idx=${idx} → ${chosen}"
}

# -- Target -----------------------------------------------------------
# UPSTREAM_REF is the google/skia ref we merge FROM. Two shapes only (see --target):
# empty ⇒ rotate over versions.json; `main` ⇒ upstream tip; a number ⇒ chrome/m<number>.
INVALID=false
if [ -z "$SYNC_TARGET" ]; then
  rotate_select
elif [ "$SYNC_TARGET" = main ]; then
  MODE="main"; TARGET="$MAIN_MS"; UPSTREAM_REF="main"
else
  MODE="explicit"; TARGET="$SYNC_TARGET"; UPSTREAM_REF="chrome/m${TARGET}"
fi

# -- Target line: `main` (newest) vs a release/<major>.<TARGET>.x maintenance line.
# A maintenance line lives under the SAME branch name in both mono/SkiaSharp and
# mono/skia. We only look for one when TARGET is strictly OLDER than main's milestone;
# the newest line is always served by `main`. The release branch itself is owned by
# the release process (release-branch skill), NOT this sync.
#
# The `main` (tip) mode targets the newest line too (TARGET == MAIN_MS, so the
# release-detection block below stays skipped), but uses a DISTINCT head branch
# (`skia-sync/main`) so a bleeding-edge tip sync never collides with a same-milestone
# sync branch (`skia-sync/m${TARGET}`).
IS_RELEASE=false
BASE_BRANCH=main
SKIA_BASE_BRANCH=skiasharp
if [ "$MODE" = main ]; then
  HEAD_BRANCH="skia-sync/main"
else
  HEAD_BRANCH="skia-sync/m${TARGET}"
fi
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
    matches=$(echo "$RELEASE_BRANCH" | paste -sd' ' -)
    echo "::error::Multiple release branches match milestone ${TARGET}: ${matches} — cannot disambiguate."
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
elif [ "$TARGET" -lt "$MAIN_MS" ] 2>/dev/null; then
  # A supported/rotation line older than main but with NO release/<major>.<TARGET>.x
  # branch has no home — do NOT merge an older milestone into main. Flag it so the gate
  # skips (fix versions.json 'support' if this milestone should still be synced).
  INVALID=true
  echo "::notice::milestone m${TARGET} is older than main (m${MAIN_MS}) but has no release/*.${TARGET}.x branch — nothing to sync."
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
emit target "$TARGET"
emit upstream_ref "$UPSTREAM_REF"
emit is_release "$IS_RELEASE"
emit base_branch "$BASE_BRANCH"
emit skia_base_branch "$SKIA_BASE_BRANCH"
emit head_branch "$HEAD_BRANCH"
emit current "$CURRENT"

echo "Resolved: m${TARGET} → ${BASE_BRANCH} (mode=${MODE}, upstream=${UPSTREAM_REF}, base milestone=m${CURRENT}, head=${HEAD_BRANCH}, release=${IS_RELEASE}, invalid=${INVALID})"

# Branch derivation is all the agent job needs; gating is pre_activation-only.
$GATE || exit 0

# A supported/rotation line older than main with no release branch has nowhere to sync.
if [ "$INVALID" = true ]; then
  emit skip true
  exit 0
fi

# -- Gate: only spin up the (expensive) agent when there is new upstream work ----
UPSTREAM_SHA=$(git ls-remote https://github.com/google/skia.git "refs/heads/${UPSTREAM_REF}" | awk '{print $1}')
if [ -z "$UPSTREAM_SHA" ]; then
  echo "::notice::upstream/${UPSTREAM_REF} does not exist yet"
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
    echo "::notice::${UPSTREAM_REF} already fully merged into ${COMPARE_REF} (upstream HEAD: ${UPSTREAM_SHA:0:12}) — skipping"
    emit skip true
    exit 0
  fi
  echo "${COMPARE_REF} exists but is ${BEHIND} commits behind upstream"
fi
