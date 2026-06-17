#!/usr/bin/env bash
#
# generate.sh — run the two deterministic release-notes/changelog generators in
# the required order, then print the "Files to polish" list.
#
# generate.sh — the Prepare phase: run the two deterministic
# release-notes/changelog generators in the required order, then print the
# "Files to polish" list.
#
# This is the single entry point for the Prepare phase (Cake -> Python; spec
# §2.2). The release-notes skill and the update-release-notes workflow both call
# THIS, so neither has to know the individual commands. The AI Polish phase (spec
# §2.2) is deliberately NOT run here — that is the human/agent's job once this
# script finishes.
#
# Usage:
#   generate.sh [--notes-only | --api-only] [extra args for the Python script...]
#
#   (no args)            Full regeneration of everything: API changelogs (Cake)
#                        then release-notes raw data for every branch (Python --all).
#   --api-only           Run only the Cake API-changelog generator.
#   --notes-only         Run only the Python release-notes generator.
#   <extra args>         Forwarded verbatim to generate-release-notes.py, replacing
#                        the default `--all` (e.g. `--branch main`, a version, etc.).
#
# Exit status is non-zero (with a clear message) if a required tool is missing —
# the skill must then stop and ask the user to install it, never work around it.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"

run_api=1
run_notes=1
notes_args=()

while [ $# -gt 0 ]; do
  case "$1" in
    --api-only)   run_notes=0; shift ;;
    --notes-only) run_api=0;   shift ;;
    --)           shift; notes_args+=("$@"); break ;;
    *)            notes_args+=("$1"); shift ;;
  esac
done

# Default scope: regenerate every branch.
if [ "${#notes_args[@]}" -eq 0 ]; then
  notes_args=(--all)
fi

cd "$REPO_ROOT"

if [ "$run_api" = 1 ]; then
  if ! command -v dotnet >/dev/null 2>&1; then
    echo "ERROR: the .NET SDK ('dotnet') is required for the API-changelog generator but was not found." >&2
    echo "       Install the SDK pinned in global.json and retry, or pass --notes-only to skip it." >&2
    exit 1
  fi
  echo "==> Prepare [1/2]: API changelogs (Cake: docs-api-diff-past)"
  dotnet tool restore
  dotnet cake --target=docs-api-diff-past --nugetDiffPrerelease=true
fi

if [ "$run_notes" = 1 ]; then
  if ! command -v python3 >/dev/null 2>&1; then
    echo "ERROR: 'python3' is required for the release-notes generator but was not found." >&2
    echo "       Install Python 3 and retry, or pass --api-only to skip it." >&2
    exit 1
  fi
  echo "==> Prepare [2/2]: release-notes raw data (generate-release-notes.py ${notes_args[*]})"
  python3 "$SCRIPT_DIR/generate-release-notes.py" "${notes_args[@]}"
fi
