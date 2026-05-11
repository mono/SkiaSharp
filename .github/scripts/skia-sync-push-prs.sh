#!/usr/bin/env bash
#
# skia-sync-push-prs.sh — Push branches and create/update PRs for skia-upstream-sync workflow.
#
# Expected env vars:
#   GH_TOKEN — PAT with write access to mono/skia and mono/SkiaSharp
#
# Expected files (written by the agent):
#   /tmp/gh-aw/agent/skia-sync-env.sh — TARGET and CURRENT vars
#   /tmp/gh-aw/agent/skia-sync-skia-summary.md — mono/skia PR body
#   /tmp/gh-aw/agent/skia-sync-skiasharp-summary.md — mono/SkiaSharp PR body
#

set -euo pipefail

if [ ! -f /tmp/gh-aw/agent/skia-sync-env.sh ]; then
    echo "No skia-sync-env.sh — agent determined no work needed"
    exit 0
fi
source /tmp/gh-aw/agent/skia-sync-env.sh

if [ -z "${TARGET:-}" ]; then
    echo "TARGET is empty — skipping"
    exit 0
fi

BRANCH="skia-sync/m${TARGET}"
SKIA_SUMMARY=""
SS_SUMMARY=""
[ -f /tmp/gh-aw/agent/skia-sync-skia-summary.md ] && SKIA_SUMMARY=$(cat /tmp/gh-aw/agent/skia-sync-skia-summary.md)
[ -f /tmp/gh-aw/agent/skia-sync-skiasharp-summary.md ] && SS_SUMMARY=$(cat /tmp/gh-aw/agent/skia-sync-skiasharp-summary.md)

# --- Determine PR titles based on same-milestone or milestone bump ---
if [ "$CURRENT" = "$TARGET" ]; then
    SS_TITLE="[skia-sync] Merge upstream chrome/m${TARGET} bug fixes"
    SS_BODY_INTRO="Automated upstream bug-fix sync for m${TARGET}."
else
    SS_TITLE="[skia-sync] Update skia to milestone ${TARGET}"
    SS_BODY_INTRO="Automated Skia milestone bump from m${CURRENT} to m${TARGET}."
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
            --head "$BRANCH" --base skiasharp \
            --title "[skia-sync] Merge upstream chrome/m${TARGET}" \
            --draft \
            --body "Automated upstream merge of \`chrome/m${TARGET}\`.

${SKIA_SUMMARY}

Created by ${WORKFLOW_LINK}." || echo "::warning::Failed to create mono/skia PR"
    else
        echo "Updating mono/skia PR #${pr}..."
        gh pr edit "$pr" --repo mono/skia \
            --body "Automated upstream merge of \`chrome/m${TARGET}\`.

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
            --head "$BRANCH" --base main \
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
