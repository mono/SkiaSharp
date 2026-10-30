#!/usr/bin/env bash
set -euo pipefail

: "${SOURCE_BRANCH:?SOURCE_BRANCH must identify the checked-out source}"
: "${GITHUB_RUN_ID:?GITHUB_RUN_ID is required}"
: "${GITHUB_ENV:?GITHUB_ENV is required}"

if [[ ! "$GITHUB_RUN_ID" =~ ^[0-9]+$ ]]; then
  echo "GITHUB_RUN_ID must be numeric" >&2
  exit 1
fi

if [ "$SOURCE_BRANCH" = "main" ]; then
  branch="bot/release-notes"
else
  branch="bot/release-notes-test-$GITHUB_RUN_ID"
fi

git checkout -B "$branch"
# Retain the selected source's tools and prepared files, but publish only new docs commits on main.
git reset --mixed origin/main
printf 'DOCS_PR_BRANCH=%s\n' "$branch" >> "$GITHUB_ENV"
