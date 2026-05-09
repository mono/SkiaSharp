#!/usr/bin/env bash
#
# api-docs-push-pr.sh — Push the docs branch and create/update a PR in mono/SkiaSharp-API-docs.
#
# Expected env vars:
#   GH_TOKEN — PAT with write access to mono/SkiaSharp-API-docs
#
# Expected files (written by the agent):
#   /tmp/gh-aw/agent/api-docs-env.sh — DOCS_BRANCH var
#   /tmp/gh-aw/agent/api-docs-summary.md — PR body content
#

set -euo pipefail

if [ ! -f /tmp/gh-aw/agent/api-docs-env.sh ]; then
    echo "No api-docs-env.sh — agent determined no work needed"
    exit 0
fi
source /tmp/gh-aw/agent/api-docs-env.sh

if [ -z "${DOCS_BRANCH:-}" ]; then
    echo "DOCS_BRANCH is empty — skipping"
    exit 0
fi

SUMMARY=""
[ -f /tmp/gh-aw/agent/api-docs-summary.md ] && SUMMARY=$(cat /tmp/gh-aw/agent/api-docs-summary.md)

WORKFLOW_LINK="[auto-api-docs-writer](https://github.com/${GITHUB_REPOSITORY:-mono/SkiaSharp}/actions/workflows/auto-api-docs-writer.lock.yml)"

# --- Set up docs repo with auth ---
cd docs
git remote set-url origin "https://x-access-token:${GH_TOKEN}@github.com/mono/SkiaSharp-API-docs.git"
git config user.name "github-actions[bot]"
git config user.email "41898282+github-actions[bot]@users.noreply.github.com"

# --- Rebase onto latest main so PR is always up to date ---
git fetch origin main
CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)

if [ "$CURRENT_BRANCH" = "HEAD" ]; then
    # Detached HEAD — create branch from current state
    git checkout -B "$DOCS_BRANCH"
fi

# Rebase the agent's work onto latest docs main
git rebase origin/main || {
    echo "::warning::Rebase onto main failed — force-resetting to main + agent commit"
    git rebase --abort
    # Save the tree (all file contents) from current HEAD
    TREE=$(git log -1 --format=%T HEAD)
    # Create a new commit with that tree on top of origin/main
    COMMIT=$(git commit-tree "$TREE" -p origin/main -m "Fill API documentation placeholders

AI-generated documentation for XML API docs.
Follows .NET API documentation guidelines.")
    git checkout -B "$DOCS_BRANCH" "$COMMIT"
}

echo "Pushing $DOCS_BRANCH to mono/SkiaSharp-API-docs..."
git push origin "$DOCS_BRANCH" --force

# --- Create or update PR ---
pr=$(gh pr list --repo mono/SkiaSharp-API-docs --head "$DOCS_BRANCH" --state open --json number --jq '.[0].number' 2>/dev/null || echo "")
if [ -z "$pr" ]; then
    echo "Creating mono/SkiaSharp-API-docs PR..."
    gh pr create --repo mono/SkiaSharp-API-docs \
        --head "$DOCS_BRANCH" --base main \
        --title "Fill API documentation placeholders" \
        --body "Automated AI-generated documentation for XML API docs with 'To be added.' placeholders.

${SUMMARY}

Created by ${WORKFLOW_LINK}." || echo "::warning::Failed to create mono/SkiaSharp-API-docs PR"
else
    echo "Updating mono/SkiaSharp-API-docs PR #${pr}..."
    gh pr edit "$pr" --repo mono/SkiaSharp-API-docs \
        --body "Automated AI-generated documentation for XML API docs with 'To be added.' placeholders.

${SUMMARY}

Updated: $(date -u +%Y-%m-%dT%H:%M:%SZ)
Created by ${WORKFLOW_LINK}." || true
fi

echo "Done!"
