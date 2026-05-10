#!/usr/bin/env bash
#
# api-docs-push-pr.sh — Commit agent changes, push, and create/update a PR in mono/SkiaSharp-API-docs.
#
# Expected env vars:
#   GH_TOKEN — PAT with write access to mono/SkiaSharp-API-docs
#

set -euo pipefail

DOCS_BRANCH="automation/write-api-docs"
WORKFLOW_LINK="[auto-api-docs-writer](https://github.com/${GITHUB_REPOSITORY:-mono/SkiaSharp}/actions/workflows/auto-api-docs-writer.lock.yml)"

# --- Skip on pull_request events (CI validation only) ---
if [ "${GITHUB_EVENT_NAME:-}" = "pull_request" ]; then
    echo "Skipping push — pull_request event is for CI validation only"
    exit 0
fi

# --- Check for changes ---
cd docs
git add -A

if git diff --cached --quiet; then
    echo "No changes — nothing to push"
    exit 0
fi

# --- Commit ---
git config user.name "github-actions[bot]"
git config user.email "41898282+github-actions[bot]@users.noreply.github.com"
git commit -m "Fill API documentation placeholders

AI-generated documentation for XML API docs.
Follows .NET API documentation guidelines."

# --- Push ---
echo "Pushing $DOCS_BRANCH to mono/SkiaSharp-API-docs..."
git remote set-url origin "https://x-access-token:${GH_TOKEN}@github.com/mono/SkiaSharp-API-docs.git"
git push origin "$DOCS_BRANCH" --force

# --- Create or update PR ---
pr=$(gh pr list --repo mono/SkiaSharp-API-docs --head "$DOCS_BRANCH" --state open --json number --jq '.[0].number' 2>/dev/null || echo "")
if [ -z "$pr" ]; then
    echo "Creating mono/SkiaSharp-API-docs PR..."
    gh pr create --repo mono/SkiaSharp-API-docs \
        --head "$DOCS_BRANCH" --base main \
        --title "Fill API documentation placeholders" \
        --body "Automated AI-generated documentation for XML API docs with 'To be added.' placeholders.

Created by ${WORKFLOW_LINK}." || echo "::warning::Failed to create mono/SkiaSharp-API-docs PR"
else
    echo "PR #$pr already exists, force-pushed update"
fi

echo "Done!"
