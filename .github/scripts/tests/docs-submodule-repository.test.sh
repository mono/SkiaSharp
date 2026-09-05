#!/usr/bin/env bash

set -euo pipefail

SCRIPT_DIR=$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)
PARSER="${SCRIPT_DIR}/../docs-submodule-repository.sh"
TMP_DIR=$(mktemp -d)
trap 'rm -rf "$TMP_DIR"' EXIT

fail() {
  echo "FAIL: $*" >&2
  exit 1
}

run_success_case() {
  local name="$1"
  local configured_url="$2"
  local expected_repository="$3"
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

run_failure_case() {
  local gitmodules="${TMP_DIR}/unsupported.gitmodules"
  local log="${TMP_DIR}/unsupported.log"

  git config -f "$gitmodules" submodule.docs.url \
    "git@github.com:mono/SkiaSharp-API-docs.git"

  if bash "$PARSER" "$gitmodules" >"$log" 2>&1; then
    fail "unsupported URL unexpectedly succeeded"
  fi

  grep -Fq "expected an HTTPS GitHub URL" "$log" ||
    fail "unsupported URL did not produce an actionable error"

  echo "PASS: unsupported-url"
}

run_success_case \
  current-mono \
  "https://github.com/mono/SkiaSharp-API-docs" \
  "mono/SkiaSharp-API-docs"
run_success_case \
  future-dotnet \
  "https://github.com/dotnet/SkiaSharp-API-docs" \
  "dotnet/SkiaSharp-API-docs"
run_success_case \
  optional-git-suffix \
  "https://github.com/dotnet/SkiaSharp-API-docs.git" \
  "dotnet/SkiaSharp-API-docs"
run_failure_case

echo "All docs submodule repository tests passed."
