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

# --- Check for changes ---
cd docs
git add -A

if git diff --cached --quiet; then
    echo "No changes — nothing to push"
    exit 0
fi

# --- Safety check: revert any deleted MemberSignature/TypeSignature lines ---
DELETED_SIGS=$(git diff --cached --diff-filter=M -U0 -- '*.xml' | grep '^-.*<\(MemberSignature\|TypeSignature\)' | wc -l | tr -d ' ')
if [ "$DELETED_SIGS" -gt 0 ]; then
    echo "::warning::Agent deleted $DELETED_SIGS signature lines — restoring originals for affected files"
    # Get list of files with deleted signatures
    AFFECTED_FILES=$(git diff --cached --diff-filter=M -U0 -- '*.xml' | grep -B5 '^-.*<\(MemberSignature\|TypeSignature\)' | grep '^diff --git' | sed 's|diff --git a/||;s| b/.*||' | sort -u)
    for f in $AFFECTED_FILES; do
        # Restore signature lines from HEAD, keep docs changes
        git checkout HEAD -- "$f"
        git add "$f"
    done
    echo "::warning::Restored $DELETED_SIGS signature lines. Re-staging docs-only changes..."
    # Re-apply only Docs block changes by re-running the agent's edits would be complex,
    # so we just skip the damaged files this run. They'll be picked up next time.
    if git diff --cached --quiet; then
        echo "No changes remain after reverting signature damage — nothing to push"
        exit 0
    fi
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
