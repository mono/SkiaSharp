#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
PARSER="${SCRIPT_DIR}/../docs-submodule-repository.sh"
WORKFLOW="${SCRIPT_DIR}/../../workflows/auto-docs-submodule-sync.yml"
TMP_DIR=$(mktemp -d)
trap 'rm -rf "$TMP_DIR"' EXIT

fail() {
  echo "FAIL: $*" >&2
  exit 1
}

assert_parse() {
  local name="${1%%|*}"
  local remainder="${1#*|}"
  local configured_url="${remainder%%|*}"
  local expected_repository="${remainder#*|}"
  local gitmodules="${TMP_DIR}/${name}.gitmodules"
  local actual

  git config -f "$gitmodules" submodule.docs.url "$configured_url"
  actual=$(bash "$PARSER" "$gitmodules")

  grep -qx "repository=$expected_repository" <<<"$actual" ||
    fail "$name: derived the wrong repository from $configured_url"
  grep -qx "url=https://github.com/$expected_repository" <<<"$actual" ||
    fail "$name: derived the wrong link from $configured_url"

  echo "PASS: $name"
}

assert_rejected() {
  local name="${1%%|*}"
  local configured_url="${1#*|}"
  local gitmodules="${TMP_DIR}/${name}.gitmodules"
  local log="${TMP_DIR}/${name}.log"

  git config -f "$gitmodules" submodule.docs.url "$configured_url"

  if bash "$PARSER" "$gitmodules" >"$log" 2>&1; then
    fail "$name: malformed URL unexpectedly succeeded: $configured_url"
  fi

  grep -q '^::error::' "$log" ||
    fail "$name: malformed URL did not produce an actionable error"

  echo "PASS: $name"
}

assert_workflow_contains() {
  grep -Fq -- "$1" "$WORKFLOW" ||
    fail "workflow is missing invariant: $1"
}

assert_workflow_line() {
  grep -Fxq -- "$1" "$WORKFLOW" ||
    fail "workflow line changed: $1"
}

valid_cases=(
  "current-mono|https://github.com/mono/SkiaSharp-API-docs|mono/SkiaSharp-API-docs"
  "future-dotnet|https://github.com/dotnet/SkiaSharp-API-docs|dotnet/SkiaSharp-API-docs"
  "optional-git-suffix|https://github.com/dotnet/SkiaSharp-API-docs.git|dotnet/SkiaSharp-API-docs"
  "repository-punctuation|https://github.com/owner/repo.name_with-parts|owner/repo.name_with-parts"
  "maximum-owner-length|https://github.com/abcdefghijklmnopqrstuvwxyzabcdefghijklm/repo|abcdefghijklmnopqrstuvwxyzabcdefghijklm/repo"
  "maximum-repository-length|https://github.com/owner/abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuv|owner/abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuv"
)

invalid_cases=(
  "dot-segments|https://github.com/./."
  "parent-owner|https://github.com/../repo"
  "leading-owner-hyphen|https://github.com/-owner/repo"
  "trailing-owner-hyphen|https://github.com/owner-/repo"
  "consecutive-owner-hyphens|https://github.com/own--er/repo"
  "invalid-owner-character|https://github.com/own_er/repo"
  "owner-too-long|https://github.com/abcdefghijklmnopqrstuvwxyzabcdefghijklmn/repo"
  "dot-repository|https://github.com/owner/."
  "parent-repository|https://github.com/owner/.."
  "invalid-repository-character|https://github.com/owner/repo~name"
  "repository-too-long|https://github.com/owner/abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvw"
  "empty-owner|https://github.com//repo"
  "empty-repository|https://github.com/owner/"
  "extra-path|https://github.com/owner/repo/extra"
  "query|https://github.com/owner/repo?ref=main"
  "fragment|https://github.com/owner/repo#readme"
  "non-github-host|https://example.com/owner/repo"
  "ssh|git@github.com:owner/repo.git"
  "leading-whitespace| https://github.com/owner/repo"
  "embedded-whitespace|https://github.com/owner/repo name"
  "trailing-whitespace|https://github.com/owner/repo "
  $'control-character|https://github.com/owner/repo\001name'
  $'trailing-control-character|https://github.com/owner/repo\n'
)

for test_case in "${valid_cases[@]}"; do
  assert_parse "$test_case"
done

for test_case in "${invalid_cases[@]}"; do
  assert_rejected "$test_case"
done

workflow_invariants=(
  '.github/scripts/docs-submodule-repository.sh >> "$GITHUB_OUTPUT"'
  'DOCS_REPOSITORY: ${{ steps.docs.outputs.repository }}'
  'DOCS_REPOSITORY_URL: ${{ steps.docs.outputs.url }}'
  '[\`$DOCS_REPOSITORY\`]($DOCS_REPOSITORY_URL) main.'
  '--title "Update docs submodule to latest main"'
)

for invariant in "${workflow_invariants[@]}"; do
  assert_workflow_contains "$invariant"
done

exact_workflow_lines=(
  '    - cron: "0 10 * * *"  # Daily at 10 AM UTC'
  '  workflow_dispatch:'
  '  contents: write'
  '  pull-requests: write'
  '          BRANCH="automation/update-docs-submodule"'
  '          REMOTE_SHA=$(git ls-remote origin "refs/heads/$BRANCH" | cut -f1)'
  '          git checkout -B "$BRANCH" origin/main'
  '          git push origin "$BRANCH" --force-with-lease="refs/heads/$BRANCH:$REMOTE_SHA"'
  '            gh pr create --base main --head "$BRANCH" \'
)

for line in "${exact_workflow_lines[@]}"; do
  assert_workflow_line "$line"
done

echo "PASS: workflow-integration"

echo "All docs submodule repository tests passed."
