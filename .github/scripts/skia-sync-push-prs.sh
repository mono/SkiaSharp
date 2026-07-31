#!/usr/bin/env bash
#
# Push the two agent-produced branches and create/update their draft PRs.
# The agent writes reports; this script owns complete PR templates and links.

set -euo pipefail

ARTIFACT_DIR="${SKIA_SYNC_ARTIFACT_DIR:-/tmp/gh-aw/agent}"
RUNTIME_DIR="/tmp/gh-aw"
ENV_FILE="$ARTIFACT_DIR/skia-sync-env.sh"
SKIA_SUMMARY_FILE="$ARTIFACT_DIR/skia-sync-skia-summary.md"
SS_SUMMARY_FILE="$ARTIFACT_DIR/skia-sync-skiasharp-summary.md"

if [[ ! -f "$ENV_FILE" ]]; then
  echo "::error::The detector found sync work, but the agent did not produce skia-sync-env.sh."
  exit 1
fi

required_file() {
  local path="$1"
  if [[ ! -s "$path" ]]; then
    echo "::error::Required sync artifact is missing or empty: $path"
    exit 1
  fi
}

required_file "$ENV_FILE"
required_file "$SKIA_SUMMARY_FILE"
required_file "$SS_SUMMARY_FILE"
required_file "$ARTIFACT_DIR/skia-breaking-change-analysis.md"
required_file "$ARTIFACT_DIR/skia-validation-review.md"
required_file "$ARTIFACT_DIR/skia-dependency-decisions.md"
required_file "$ARTIFACT_DIR/skia-fork-patch-audit.md"
required_file "$ARTIFACT_DIR/test-output.txt"
required_file "$ARTIFACT_DIR/test-exit-code.txt"

# Written by the trusted agent in this workflow and validated immediately below.
# shellcheck source=/dev/null
source "$ENV_FILE"

: "${TARGET:?TARGET is required}"
: "${CURRENT:?CURRENT is required}"
: "${UPSTREAM_REF:?UPSTREAM_REF is required}"
: "${BASE_BRANCH:?BASE_BRANCH is required}"
: "${SKIA_BASE_BRANCH:?SKIA_BASE_BRANCH is required}"
: "${SKIA_BASE_SHA:?SKIA_BASE_SHA is required}"
: "${HEAD_BRANCH:?HEAD_BRANCH is required}"
: "${BASE_UPSTREAM_SHA:?BASE_UPSTREAM_SHA is required}"
: "${TARGET_UPSTREAM_SHA:?TARGET_UPSTREAM_SHA is required}"
: "${GITHUB_WORKSPACE:?GITHUB_WORKSPACE is required}"
: "${GH_TOKEN:?GH_TOKEN is required}"

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

assert_resolved TARGET "$TARGET" SKIA_SYNC_TARGET "${SKIA_SYNC_TARGET:-}"
assert_resolved CURRENT "$CURRENT" SKIA_SYNC_CURRENT "${SKIA_SYNC_CURRENT:-}"
assert_resolved UPSTREAM_REF "$UPSTREAM_REF" SKIA_SYNC_UPSTREAM_REF "${SKIA_SYNC_UPSTREAM_REF:-}"
assert_resolved IS_RELEASE "$IS_RELEASE" SKIA_SYNC_IS_RELEASE "${SKIA_SYNC_IS_RELEASE:-}"
assert_resolved BASE_BRANCH "$BASE_BRANCH" SKIA_SYNC_BASE_BRANCH "${SKIA_SYNC_BASE_BRANCH:-}"
assert_resolved SKIA_BASE_BRANCH "$SKIA_BASE_BRANCH" SKIA_SYNC_SKIA_BASE_BRANCH "${SKIA_SYNC_SKIA_BASE_BRANCH:-}"
assert_resolved SKIA_BASE_SHA "$SKIA_BASE_SHA" SKIA_SYNC_SKIA_BASE_SHA "${SKIA_SYNC_SKIA_BASE_SHA:-}"
assert_resolved HEAD_BRANCH "$HEAD_BRANCH" SKIA_SYNC_HEAD_BRANCH "${SKIA_SYNC_HEAD_BRANCH:-}"
assert_resolved BASE_UPSTREAM_SHA "$BASE_UPSTREAM_SHA" SKIA_SYNC_BASE_UPSTREAM_SHA "${SKIA_SYNC_BASE_UPSTREAM_SHA:-}"
assert_resolved TARGET_UPSTREAM_SHA "$TARGET_UPSTREAM_SHA" SKIA_SYNC_TARGET_UPSTREAM_SHA "${SKIA_SYNC_TARGET_UPSTREAM_SHA:-}"

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

python3 "$RUNTIME_DIR/audit-fork-patches.py" \
  --skia-root "$GITHUB_WORKSPACE/externals/skia" \
  --old-upstream "$BASE_UPSTREAM_SHA" \
  --new-upstream "$TARGET_UPSTREAM_SHA" \
  --fork-base "$SKIA_BASE_SHA" \
  --merged-head "$BRANCH" \
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

require_branch() {
  local repo_dir="$1"
  if ! git -C "$repo_dir" rev-parse --verify "$BRANCH" >/dev/null 2>&1; then
    echo "::error::Required local branch '$BRANCH' is missing in $repo_dir."
    exit 1
  fi
}

push_branch() {
  local repo_dir="$1"
  local repo_url="$2"

  require_branch "$repo_dir"
  git -C "$repo_dir" remote set-url origin "https://x-access-token:${GH_TOKEN}@github.com/${repo_url}.git"
  local remote_sha
  local lease
  remote_sha=$(git -C "$repo_dir" ls-remote --heads origin "refs/heads/${BRANCH}" | awk '{print $1}')
  if [[ -n "$remote_sha" ]]; then
    lease="--force-with-lease=refs/heads/${BRANCH}:${remote_sha}"
  else
    lease="--force-with-lease=refs/heads/${BRANCH}:"
  fi
  git -C "$repo_dir" push origin \
    "refs/heads/${BRANCH}:refs/heads/${BRANCH}" \
    "$lease"
}

changed_check() {
  local repo_dir="$1"
  local base="$2"
  shift 2
  local status
  if git -C "$repo_dir" diff --quiet "origin/${base}...${BRANCH}" -- "$@"; then
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

render_skia_body() {
  local companion_url="$1"
  local output="$2"
  local values="$ARTIFACT_DIR/skia-pr-values.json"

  jq -n \
    --arg BODY_INTRO "$SKIA_BODY_INTRO" \
    --arg WORKFLOW_LINK "$WORKFLOW_LINK" \
    --arg COMPANION_PR_URL "$companion_url" \
    --arg CAPI_CHECK "$(changed_check "$GITHUB_WORKSPACE/externals/skia" "$SKIA_BASE" include/c src/c)" \
    --arg DEPS_CHECK "$(changed_check "$GITHUB_WORKSPACE/externals/skia" "$SKIA_BASE" DEPS)" \
    --arg BUILD_CHECK "$(changed_check "$GITHUB_WORKSPACE/externals/skia" "$SKIA_BASE" BUILD.gn third_party)" \
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

  python3 "$RUNTIME_DIR/skia-sync-render-template.py" \
    "$RUNTIME_DIR/skia-sync-pr-skia.md" "$values" "$output"
}

render_skiasharp_body() {
  local companion_url="$1"
  local output="$2"
  local values="$ARTIFACT_DIR/skiasharp-pr-values.json"
  local generated_check

  generated_check=$(changed_check "$GITHUB_WORKSPACE" "$SS_BASE" ':(glob)**/*.generated.cs')
  jq -n \
    --arg BODY_INTRO "$SS_BODY_INTRO" \
    --arg WORKFLOW_LINK "$WORKFLOW_LINK" \
    --arg COMPANION_PR_URL "$companion_url" \
    --arg MANAGED_CHECK "$(changed_check "$GITHUB_WORKSPACE" "$SS_BASE" binding)" \
    --arg NATIVE_CHECK "$(changed_check "$GITHUB_WORKSPACE/externals/skia" "$SKIA_BASE" include/c src/c)" \
    --arg GENERATED_CHECK "$generated_check" \
    --arg INTEGRATIONS_CHECK "$(changed_check "$GITHUB_WORKSPACE" "$SS_BASE" views source)" \
    --arg RENDERING_CHECK " " \
    --arg TESTS_CHECK "$(changed_check "$GITHUB_WORKSPACE" "$SS_BASE" tests)" \
    --arg BUILD_CHECK "$(changed_check "$GITHUB_WORKSPACE" "$SS_BASE" native scripts .github)" \
    --arg DOCS_CHECK "$(changed_check "$GITHUB_WORKSPACE" "$SS_BASE" documentation samples)" \
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

  python3 "$RUNTIME_DIR/skia-sync-render-template.py" \
    "$RUNTIME_DIR/skia-sync-pr-skiasharp.md" "$values" "$output"
}

find_pr() {
  local repo="$1"
  gh pr list --repo "$repo" --head "$BRANCH" --state open --json number --jq '.[0].number // empty'
}

ensure_pr() {
  local repo="$1"
  local base="$2"
  local title="$3"
  local body_file="$4"
  local pr

  pr=$(find_pr "$repo")
  if [[ -z "$pr" ]]; then
    gh pr create --repo "$repo" \
      --head "$BRANCH" \
      --base "$base" \
      --title "$title" \
      --draft \
      --body-file "$body_file" >/dev/null
    pr=$(find_pr "$repo")
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
  gh api --method PATCH "repos/${repo}/pulls/${pr}" --input "$request" >/dev/null
}

apply_labels() {
  local repo="$1"
  local pr="$2"
  local request="$ARTIFACT_DIR/pr-labels.json"

  jq -n --argjson bump "$IS_MILESTONE_BUMP" \
    '{labels: (["type/milestone-sync", "partner/agentic-workflows"]
      + (if $bump then ["type/milestone-bump"] else [] end))}' >"$request"
  gh api --method POST "repos/${repo}/issues/${pr}/labels" --input "$request" >/dev/null
}

echo "Pushing $BRANCH to mono/skia and mono/SkiaSharp with guarded leases..."
git -C "$GITHUB_WORKSPACE/externals/skia" rev-parse --verify "origin/${SKIA_BASE}^{commit}" >/dev/null
git -C "$GITHUB_WORKSPACE" rev-parse --verify "origin/${SS_BASE}^{commit}" >/dev/null
push_branch "$GITHUB_WORKSPACE/externals/skia" mono/skia
push_branch "$GITHUB_WORKSPACE" mono/SkiaSharp

SKIA_BODY="$ARTIFACT_DIR/skia-pr-body.md"
SS_BODY="$ARTIFACT_DIR/skiasharp-pr-body.md"

render_skia_body "Pending companion PR creation in this workflow run." "$SKIA_BODY"
SKIA_PR=$(ensure_pr mono/skia "$SKIA_BASE" "$SKIA_TITLE" "$SKIA_BODY")
SKIA_PR_URL="https://github.com/mono/skia/pull/${SKIA_PR}"

render_skiasharp_body "$SKIA_PR_URL" "$SS_BODY"
SS_PR=$(ensure_pr mono/SkiaSharp "$SS_BASE" "$SS_TITLE" "$SS_BODY")
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
