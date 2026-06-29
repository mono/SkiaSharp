#!/usr/bin/env bash
#
# skia-sync-push-prs.sh — Push branches and create/update PRs for skia-upstream-sync workflow.
#
# Expected env vars:
#   GH_TOKEN — PAT with write access to mono/skia and mono/SkiaSharp
#
# Expected files (written by the agent):
#   /tmp/gh-aw/agent/skia-sync-env.sh — TARGET, CURRENT, and (optionally) IS_RELEASE,
#       BASE_BRANCH, SKIA_BASE_BRANCH, HEAD_BRANCH vars
#   /tmp/gh-aw/agent/skia-sync-skia-summary.md — mono/skia PR body
#   /tmp/gh-aw/agent/skia-sync-skiasharp-summary.md — mono/SkiaSharp PR body
#

set -euo pipefail

if [ ! -f /tmp/gh-aw/agent/skia-sync-env.sh ]; then
    echo "No skia-sync-env.sh — agent determined no work needed"
    exit 0
fi
# Written by the agent at runtime; not resolvable at lint time.
# shellcheck source=/dev/null
source /tmp/gh-aw/agent/skia-sync-env.sh

if [ -z "${TARGET:-}" ]; then
    echo "TARGET is empty — skipping"
    exit 0
fi

# Branch targets. For a main sync these default to the historical values; for a
# release-line sync the agent provides explicit HEAD_BRANCH / BASE_BRANCH /
# SKIA_BASE_BRANCH that point at the matching release/<major>.<milestone>.x line.
BRANCH="${HEAD_BRANCH:-skia-sync/m${TARGET}}"
SS_BASE="${BASE_BRANCH:-main}"
SKIA_BASE="${SKIA_BASE_BRANCH:-skiasharp}"
IS_RELEASE="${IS_RELEASE:-false}"
# UPSTREAM_REF is the google/skia ref the sync merged FROM. For a `chrome/m<N>`
# milestone sync it defaults to that branch (backward compat with older env files);
# `main` denotes a bleeding-edge tip sync.
UPSTREAM_REF="${UPSTREAM_REF:-chrome/m${TARGET}}"
echo "Sync branch: $BRANCH | SkiaSharp base: $SS_BASE | mono/skia base: $SKIA_BASE | release: $IS_RELEASE | upstream: $UPSTREAM_REF"

SKIA_SUMMARY=""
SS_SUMMARY=""
[ -f /tmp/gh-aw/agent/skia-sync-skia-summary.md ] && SKIA_SUMMARY=$(cat /tmp/gh-aw/agent/skia-sync-skia-summary.md)
[ -f /tmp/gh-aw/agent/skia-sync-skiasharp-summary.md ] && SS_SUMMARY=$(cat /tmp/gh-aw/agent/skia-sync-skiasharp-summary.md)

# --- Determine PR titles based on sync kind (tip / same-milestone / milestone bump) ---
if [ "$UPSTREAM_REF" = "main" ]; then
    SS_TITLE="[skia-sync] Merge upstream Skia main (tip)"
    SS_BODY_INTRO="Automated bleeding-edge sync from the tip of upstream Skia (google/skia main)."
elif [ "$CURRENT" = "$TARGET" ]; then
    SS_TITLE="[skia-sync] Merge upstream chrome/m${TARGET} bug fixes"
    SS_BODY_INTRO="Automated upstream bug-fix sync for m${TARGET}."
else
    SS_TITLE="[skia-sync] Update skia to milestone ${TARGET}"
    SS_BODY_INTRO="Automated Skia milestone bump from m${CURRENT} to m${TARGET}."
fi
if [ "$IS_RELEASE" = "true" ]; then
    SS_BODY_INTRO="${SS_BODY_INTRO} Targeting release branch \`${SS_BASE}\` (mono/skia \`${SKIA_BASE}\`)."
fi

# --- mono/skia PR title + body intro (UPSTREAM_REF-aware) ---
if [ "$UPSTREAM_REF" = "main" ]; then
    SKIA_TITLE="[skia-sync] Merge upstream Skia main (tip)"
    SKIA_BODY_INTRO="Automated upstream merge of google/skia main (tip)."
else
    SKIA_TITLE="[skia-sync] Merge upstream chrome/m${TARGET}"
    SKIA_BODY_INTRO="Automated upstream merge of \`chrome/m${TARGET}\`."
fi

WORKFLOW_LINK="[skia-upstream-sync](https://github.com/${GITHUB_REPOSITORY:-mono/SkiaSharp}/actions/workflows/auto-skia-sync.lock.yml)"

# --- mono/skia: push submodule branch and create/update PR ---
push_skia() {
    cd externals/skia
    if ! git rev-parse --verify "$BRANCH" &>/dev/null; then
        echo "No submodule branch $BRANCH — skipping mono/skia"
        return
    fi

    echo "Pushing $BRANCH to mono/skia..."
    git remote set-url origin "https://x-access-token:${GH_TOKEN}@github.com/mono/skia.git"
    git push origin "$BRANCH" --force-with-lease 2>/dev/null || git push origin "$BRANCH" --force

    local pr
    pr=$(gh pr list --repo mono/skia --head "$BRANCH" --state open --json number --jq '.[0].number' 2>/dev/null || echo "")
    if [ -z "$pr" ]; then
        echo "Creating mono/skia PR..."
        gh pr create --repo mono/skia \
            --head "$BRANCH" --base "$SKIA_BASE" \
            --title "$SKIA_TITLE" \
            --draft \
            --body "${SKIA_BODY_INTRO}

${SKIA_SUMMARY}

Created by ${WORKFLOW_LINK}." || echo "::warning::Failed to create mono/skia PR"
    else
        echo "Updating mono/skia PR #${pr}..."
        gh pr edit "$pr" --repo mono/skia \
            --body "${SKIA_BODY_INTRO}

${SKIA_SUMMARY}

Updated: $(date -u +%Y-%m-%dT%H:%M:%SZ)" || true
    fi
}

# --- mono/SkiaSharp: push parent branch and create/update PR ---
push_skiasharp() {
    cd "$GITHUB_WORKSPACE"
    if ! git rev-parse --verify "$BRANCH" &>/dev/null; then
        echo "No SkiaSharp branch $BRANCH — skipping"
        return
    fi

    echo "Pushing $BRANCH to mono/SkiaSharp..."
    git remote set-url origin "https://x-access-token:${GH_TOKEN}@github.com/mono/SkiaSharp.git"
    git push origin "$BRANCH" --force-with-lease 2>/dev/null || git push origin "$BRANCH" --force

    local skia_pr skia_pr_link ss_pr
    skia_pr=$(gh pr list --repo mono/skia --head "$BRANCH" --state open --json number --jq '.[0].number' 2>/dev/null || echo "")
    skia_pr_link=""
    [ -n "$skia_pr" ] && skia_pr_link="**Companion skia PR:** https://github.com/mono/skia/pull/$skia_pr"

    ss_pr=$(gh pr list --repo mono/SkiaSharp --head "$BRANCH" --state open --json number --jq '.[0].number' 2>/dev/null || echo "")
    if [ -z "$ss_pr" ]; then
        echo "Creating mono/SkiaSharp PR..."
        gh pr create --repo mono/SkiaSharp \
            --head "$BRANCH" --base "$SS_BASE" \
            --title "$SS_TITLE" \
            --draft \
            --body "${SS_BODY_INTRO}

$skia_pr_link

${SS_SUMMARY}

Created by ${WORKFLOW_LINK}." || echo "::warning::Failed to create mono/SkiaSharp PR"
    else
        echo "Updating mono/SkiaSharp PR #${ss_pr}..."
        gh pr edit "$ss_pr" --repo mono/SkiaSharp \
            --body "${SS_BODY_INTRO}

$skia_pr_link

${SS_SUMMARY}

Updated: $(date -u +%Y-%m-%dT%H:%M:%SZ)" || true
    fi
}

push_skia
push_skiasharp
