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

# Parse signal file safely — do NOT source it (it's agent-written and we hold a PAT)
DOCS_BRANCH=$(grep -oP '^DOCS_BRANCH=\K.+' /tmp/gh-aw/agent/api-docs-env.sh || true)

if [ -z "${DOCS_BRANCH:-}" ]; then
    echo "DOCS_BRANCH is empty — skipping"
    exit 0
fi

# Validate branch name
if [[ ! "$DOCS_BRANCH" =~ ^automation/write-api-docs$ ]]; then
    echo "::error::Unexpected DOCS_BRANCH value: $DOCS_BRANCH"
    exit 1
fi

# --- Skip push on pull_request events (used for CI validation only) ---
if [ "${GITHUB_EVENT_NAME:-}" = "pull_request" ]; then
    echo "Skipping push — pull_request event is for CI validation only"
    exit 0
fi

SUMMARY=""
[ -f /tmp/gh-aw/agent/api-docs-summary.md ] && SUMMARY=$(cat /tmp/gh-aw/agent/api-docs-summary.md)

WORKFLOW_LINK="[auto-api-docs-writer](https://github.com/${GITHUB_REPOSITORY:-mono/SkiaSharp}/actions/workflows/auto-api-docs-writer.lock.yml)"

# --- Push the docs branch ---
cd docs
git remote set-url origin "https://x-access-token:${GH_TOKEN}@github.com/mono/SkiaSharp-API-docs.git"

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
