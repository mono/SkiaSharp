#!/usr/bin/env bash
#
# Publish two independently verified Git bundles and create/update their draft PRs.

set -euo pipefail

export PATH=/usr/bin:/bin
export GIT_NO_REPLACE_OBJECTS=1
unset BASH_ENV ENV CDPATH GIT_CONFIG_COUNT GIT_EXEC_PATH PYTHONHOME PYTHONPATH
unset DOTNET_STARTUP_HOOKS LD_PRELOAD

RUNTIME_DIR="${SKIA_SYNC_TRUSTED_DIR:-$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)}"
PACKAGE_DIR="${SKIA_SYNC_PACKAGE_DIR:?SKIA_SYNC_PACKAGE_DIR is required}"
ORIGINAL_PACKAGE_DIR="${SKIA_SYNC_ORIGINAL_PACKAGE_DIR:?SKIA_SYNC_ORIGINAL_PACKAGE_DIR is required}"
ORIGINAL_EXPECTED_ENV="${SKIA_SYNC_ORIGINAL_EXPECTED_ENV:?SKIA_SYNC_ORIGINAL_EXPECTED_ENV is required}"
ARTIFACT_DIR="${SKIA_SYNC_ARTIFACT_DIR:-${RUNNER_TEMP:-/tmp}/skia-sync-publish}"
ENV_FILE="$PACKAGE_DIR/skia-sync-expected.env"
SKIA_SUMMARY_FILE="$PACKAGE_DIR/skia-sync-skia-summary.md"
SS_SUMMARY_FILE="$PACKAGE_DIR/skia-sync-skiasharp-summary.md"
ATTESTATION="$PACKAGE_DIR/attestation.json"
PARENT_BUNDLE="$PACKAGE_DIR/skiasharp.bundle"
SKIA_BUNDLE="$PACKAGE_DIR/skia.bundle"
mkdir -p "$ARTIFACT_DIR"

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
required_file "$ATTESTATION"
required_file "$PARENT_BUNDLE"
required_file "$SKIA_BUNDLE"
required_file "$PACKAGE_DIR/skia-breaking-change-analysis.md"
required_file "$PACKAGE_DIR/skia-validation-review.md"
required_file "$PACKAGE_DIR/skia-dependency-decisions.md"
required_file "$ORIGINAL_EXPECTED_ENV"
required_file "$ORIGINAL_PACKAGE_DIR/attestation.json"
required_file "$ORIGINAL_PACKAGE_DIR/skiasharp.bundle"
required_file "$ORIGINAL_PACKAGE_DIR/skia.bundle"

if ! cmp -s "$ENV_FILE" "$ORIGINAL_EXPECTED_ENV" ||
   ! cmp -s "$PARENT_BUNDLE" "$ORIGINAL_PACKAGE_DIR/skiasharp.bundle" ||
   ! cmp -s "$SKIA_BUNDLE" "$ORIGINAL_PACKAGE_DIR/skia.bundle"; then
  echo "::error::Fresh verification output does not match the original source-run package." >&2
  exit 1
fi
for field in headBranch parentHead skiaHead gitlink parentBundleSha256 skiaBundleSha256; do
  if [[ "$(jq -er --arg field "$field" '.[$field]' "$ATTESTATION")" !=
        "$(jq -er --arg field "$field" '.[$field]' "$ORIGINAL_PACKAGE_DIR/attestation.json")" ]]; then
    echo "::error::Fresh verification changed the original attested $field." >&2
    exit 1
  fi
done

# shellcheck source=/dev/null
source "$RUNTIME_DIR/skia-sync-common.sh"
load_sync_env "$ENV_FILE"
export GIT_ASKPASS="$RUNTIME_DIR/skia-sync-git-askpass.sh"
export GIT_TERMINAL_PROMPT=0

: "${PARENT_TOKEN:?PARENT_TOKEN is required}"
: "${SKIA_TOKEN:?SKIA_TOKEN is required}"

ATTESTED_BRANCH=$(jq -er '.headBranch' "$ATTESTATION")
ATTESTED_PARENT_HEAD=$(jq -er '.parentHead' "$ATTESTATION")
ATTESTED_SKIA_HEAD=$(jq -er '.skiaHead' "$ATTESTATION")
ATTESTED_GITLINK=$(jq -er '.gitlink' "$ATTESTATION")
ATTESTED_PARENT_BUNDLE_SHA=$(jq -er '.parentBundleSha256' "$ATTESTATION")
ATTESTED_SKIA_BUNDLE_SHA=$(jq -er '.skiaBundleSha256' "$ATTESTATION")
if [[ "$ATTESTED_BRANCH" != "$HEAD_BRANCH" || "$ATTESTED_GITLINK" != "$ATTESTED_SKIA_HEAD" ]]; then
  sync_error "Verification attestation does not match the trusted branch/gitlink."
  exit 1
fi
if [[ "$(sha256sum "$PARENT_BUNDLE" | awk '{print $1}')" != "$ATTESTED_PARENT_BUNDLE_SHA" ||
      "$(sha256sum "$SKIA_BUNDLE" | awk '{print $1}')" != "$ATTESTED_SKIA_BUNDLE_SHA" ]]; then
  sync_error "Verified Git bundle hash does not match the attestation."
  exit 1
fi

BRANCH="$HEAD_BRANCH"
SS_BASE="$BASE_BRANCH"
SKIA_BASE="$SKIA_BASE_BRANCH"
IS_RELEASE="${IS_RELEASE:-false}"
UPDATED_AT=$(date -u +%Y-%m-%dT%H:%M:%SZ)
WORKFLOW_LINK="[skia-upstream-sync](https://github.com/${GITHUB_REPOSITORY:-mono/SkiaSharp}/actions/workflows/auto-skia-sync.lock.yml)"

git_safe() {
  git \
    -c core.hooksPath=/dev/null \
    -c core.fsmonitor=false \
    -c credential.helper= \
    "$@"
}

REPO_DIR=$(mktemp -d)
trap 'rm -rf "$REPO_DIR"' EXIT
SS_REPO="$REPO_DIR/skiasharp.git"
SKIA_REPO="$REPO_DIR/skia.git"
git_safe init --bare -q "$SS_REPO"
git_safe init --bare -q "$SKIA_REPO"
git_safe -C "$SS_REPO" fetch -q "$PARENT_BUNDLE" "refs/heads/${BRANCH}:refs/heads/${BRANCH}"
git_safe -C "$SKIA_REPO" fetch -q "$SKIA_BUNDLE" "refs/heads/${BRANCH}:refs/heads/${BRANCH}"

CURRENT_PARENT_HEAD=$(git_safe -C "$SS_REPO" rev-parse "refs/heads/${BRANCH}")
CURRENT_SKIA_HEAD=$(git_safe -C "$SKIA_REPO" rev-parse "refs/heads/${BRANCH}")
if [[ "$CURRENT_PARENT_HEAD" != "$ATTESTED_PARENT_HEAD" || "$CURRENT_SKIA_HEAD" != "$ATTESTED_SKIA_HEAD" ]]; then
  sync_error "Verified bundles do not contain the attested branch commits."
  exit 1
fi

git_safe -C "$SS_REPO" fetch -q --no-tags \
  "https://github.com/mono/SkiaSharp.git" \
  "refs/heads/${SS_BASE}:refs/remotes/origin/${SS_BASE}"
git_safe -C "$SKIA_REPO" fetch -q --no-tags \
  "https://github.com/mono/skia.git" \
  "refs/heads/${SKIA_BASE}:refs/remotes/origin/${SKIA_BASE}"

reject_sensitive_changes() {
  local repo_dir="$1"
  local base="$2"
  shift 2
  local changed

  changed=$(git_safe -C "$repo_dir" diff --name-only --no-ext-diff "origin/${base}...${BRANCH}" -- "$@")
  if [[ -n "$changed" ]]; then
    sync_error "Automated sync changes secret-bearing workflow inputs and requires manual publication:"
    printf '%s\n' "$changed" >&2
    exit 1
  fi
}

reject_sensitive_changes \
  "$SS_REPO" "$SS_BASE" \
  .github/workflows .github/actions scripts/infra/package/manage-nuget-feed.ps1
reject_sensitive_changes \
  "$SKIA_REPO" "$SKIA_BASE" \
  .github/workflows .github/actions

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
  if ! git_safe -C "$repo_dir" rev-parse --verify "$BRANCH" >/dev/null 2>&1; then
    echo "::error::Required local branch '$BRANCH' is missing in $repo_dir."
    exit 1
  fi
}

push_branch() {
  local repo_dir="$1"
  local repo_url="$2"
  local expected_sha="$3"
  local observed_remote_sha="$4"
  local token="$5"
  local authenticated_url="https://github.com/${repo_url}.git"

  require_branch "$repo_dir"
  if [[ "$(git_safe -C "$repo_dir" rev-parse "refs/heads/${BRANCH}")" != "$expected_sha" ]]; then
    sync_error "$repo_url branch changed after validation; refusing to push."
    exit 1
  fi
  local remote_sha
  local lease
  remote_sha=$(
    GIT_AUTH_TOKEN="$token" \
      git_safe -C "$repo_dir" ls-remote --heads "$authenticated_url" "refs/heads/${BRANCH}" \
      | awk '{print $1}'
  )
  if [[ "$remote_sha" == "$expected_sha" ]]; then
    echo "$repo_url already has the attested $BRANCH commit."
    return
  fi
  if [[ "$observed_remote_sha" == "none" ]]; then
    if [[ -n "$remote_sha" ]]; then
      sync_error "$repo_url $BRANCH was created after sync activation; refusing stale publication."
      exit 1
    fi
    lease="--force-with-lease=refs/heads/${BRANCH}:"
  else
    if [[ "$remote_sha" != "$observed_remote_sha" ]]; then
      sync_error "$repo_url $BRANCH changed after sync activation; refusing stale publication."
      exit 1
    fi
    lease="--force-with-lease=refs/heads/${BRANCH}:${observed_remote_sha}"
  fi
  GIT_AUTH_TOKEN="$token" git_safe -C "$repo_dir" push "$authenticated_url" \
    "refs/heads/${BRANCH}:refs/heads/${BRANCH}" \
    "$lease"
}

changed_check() {
  local repo_dir="$1"
  local base="$2"
  shift 2
  local status
  if git_safe -C "$repo_dir" diff --quiet --no-ext-diff "origin/${base}...${BRANCH}" -- "$@"; then
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
    --arg CAPI_CHECK "$(changed_check "$SKIA_REPO" "$SKIA_BASE" include/c src/c)" \
    --arg DEPS_CHECK "$(changed_check "$SKIA_REPO" "$SKIA_BASE" DEPS)" \
    --arg BUILD_CHECK "$(changed_check "$SKIA_REPO" "$SKIA_BASE" BUILD.gn third_party)" \
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

  generated_check=$(changed_check "$SS_REPO" "$SS_BASE" ':(glob)**/*.generated.cs')
  jq -n \
    --arg BODY_INTRO "$SS_BODY_INTRO" \
    --arg WORKFLOW_LINK "$WORKFLOW_LINK" \
    --arg COMPANION_PR_URL "$companion_url" \
    --arg MANAGED_CHECK "$(changed_check "$SS_REPO" "$SS_BASE" binding)" \
    --arg NATIVE_CHECK "$(changed_check "$SKIA_REPO" "$SKIA_BASE" include/c src/c)" \
    --arg GENERATED_CHECK "$generated_check" \
    --arg INTEGRATIONS_CHECK "$(changed_check "$SS_REPO" "$SS_BASE" views source)" \
    --arg RENDERING_CHECK " " \
    --arg TESTS_CHECK "$(changed_check "$SS_REPO" "$SS_BASE" tests)" \
    --arg BUILD_CHECK "$(changed_check "$SS_REPO" "$SS_BASE" native scripts .github)" \
    --arg DOCS_CHECK "$(changed_check "$SS_REPO" "$SS_BASE" documentation samples)" \
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
  local token="$2"
  GH_TOKEN="$token" gh pr list --repo "$repo" --head "$BRANCH" --state open --json number --jq '.[0].number // empty'
}

ensure_pr() {
  local repo="$1"
  local base="$2"
  local title="$3"
  local body_file="$4"
  local token="$5"
  local pr

  pr=$(find_pr "$repo" "$token")
  if [[ -z "$pr" ]]; then
    GH_TOKEN="$token" gh pr create --repo "$repo" \
      --head "$BRANCH" \
      --base "$base" \
      --title "$title" \
      --draft \
      --body-file "$body_file" >/dev/null
    pr=$(find_pr "$repo" "$token")
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
  local token="$5"
  local request="$ARTIFACT_DIR/pr-patch.json"

  jq -n --arg title "$title" --rawfile body "$body_file" '{title: $title, body: $body}' >"$request"
  GH_TOKEN="$token" gh api --method PATCH "repos/${repo}/pulls/${pr}" --input "$request" >/dev/null
}

apply_labels() {
  local repo="$1"
  local pr="$2"
  local token="$3"
  local request="$ARTIFACT_DIR/pr-labels.json"

  jq -n --argjson bump "$IS_MILESTONE_BUMP" \
    '{labels: (["type/milestone-sync", "partner/agentic-workflows"]
      + (if $bump then ["type/milestone-bump"] else [] end))}' >"$request"
  GH_TOKEN="$token" gh api --method POST "repos/${repo}/issues/${pr}/labels" --input "$request" >/dev/null
}

echo "Pushing $BRANCH to mono/skia and mono/SkiaSharp with guarded leases..."
git_safe -C "$SKIA_REPO" rev-parse --verify "origin/${SKIA_BASE}^{commit}" >/dev/null
git_safe -C "$SS_REPO" rev-parse --verify "origin/${SS_BASE}^{commit}" >/dev/null
push_branch "$SKIA_REPO" mono/skia "$ATTESTED_SKIA_HEAD" "$SKIA_REMOTE_HEAD" "$SKIA_TOKEN"
push_branch "$SS_REPO" mono/SkiaSharp "$ATTESTED_PARENT_HEAD" "$PARENT_REMOTE_HEAD" "$PARENT_TOKEN"

SKIA_BODY="$ARTIFACT_DIR/skia-pr-body.md"
SS_BODY="$ARTIFACT_DIR/skiasharp-pr-body.md"

render_skia_body "Pending companion PR creation in this workflow run." "$SKIA_BODY"
SKIA_PR=$(ensure_pr mono/skia "$SKIA_BASE" "$SKIA_TITLE" "$SKIA_BODY" "$SKIA_TOKEN")
SKIA_PR_URL="https://github.com/mono/skia/pull/${SKIA_PR}"

render_skiasharp_body "$SKIA_PR_URL" "$SS_BODY"
SS_PR=$(ensure_pr mono/SkiaSharp "$SS_BASE" "$SS_TITLE" "$SS_BODY" "$PARENT_TOKEN")
SS_PR_URL="https://github.com/mono/SkiaSharp/pull/${SS_PR}"

render_skia_body "$SS_PR_URL" "$SKIA_BODY"
render_skiasharp_body "$SKIA_PR_URL" "$SS_BODY"
patch_pr mono/skia "$SKIA_PR" "$SKIA_TITLE" "$SKIA_BODY" "$SKIA_TOKEN"
patch_pr mono/SkiaSharp "$SS_PR" "$SS_TITLE" "$SS_BODY" "$PARENT_TOKEN"
apply_labels mono/skia "$SKIA_PR" "$SKIA_TOKEN"
apply_labels mono/SkiaSharp "$SS_PR" "$PARENT_TOKEN"

echo "Created or updated reciprocal draft PRs:"
echo "  $SKIA_PR_URL"
echo "  $SS_PR_URL"
