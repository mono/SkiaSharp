#!/usr/bin/env bash
#
# generate.sh — the Prepare phase: run the two deterministic
# release-notes/changelog generators in the required order, then print the
# "Files to polish" list.
#
# Output contract: STDOUT carries ONLY that list (forwarded verbatim from
# generate-release-notes.py). All verbose Cake + Python progress is redirected to
# a temp log whose path is announced on STDERR; on failure the log tail is shown
# on STDERR. So `generate.sh | …` (or capturing its stdout) yields just the list.
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

# Keep STDOUT clean: the only thing this script emits to stdout is the
# machine-readable "Files to polish" list (generate-release-notes.py's stdout),
# so the Polish phase / workflow can consume it without wading through progress.
# All verbose Cake + Python progress is redirected to a log; its path is announced
# on stderr, and on failure its tail is surfaced there too.
PREPARE_LOG="$(mktemp "${TMPDIR:-/tmp}/release-notes-prepare.XXXXXX")"
echo "==> Prepare: verbose progress -> $PREPARE_LOG" >&2

if [ "$run_api" = 1 ]; then
  if ! command -v dotnet >/dev/null 2>&1; then
    echo "ERROR: the .NET SDK ('dotnet') is required for the API-changelog generator but was not found." >&2
    echo "       Install the SDK pinned in global.json and retry, or pass --notes-only to skip it." >&2
    exit 1
  fi
  echo "==> Prepare [1/2]: API changelogs (Cake: docs-api-diff-past)" >&2
  { dotnet tool restore && \
    dotnet cake --target=docs-api-diff-past --nugetDiffPrerelease=true ; } >>"$PREPARE_LOG" 2>&1 \
    || { echo "ERROR: API-changelog generation failed — last 50 log lines:" >&2; tail -50 "$PREPARE_LOG" >&2; exit 1; }
fi

if [ "$run_notes" = 1 ]; then
  if ! command -v python3 >/dev/null 2>&1; then
    echo "ERROR: 'python3' is required for the release-notes generator but was not found." >&2
    echo "       Install Python 3 and retry, or pass --api-only to skip it." >&2
    exit 1
  fi
  echo "==> Prepare [2/2]: release-notes raw data (generate-release-notes.py ${notes_args[*]})" >&2
  # The Python generator writes the clean "Files to polish" list to stdout and all
  # progress to stderr; send progress to the log and let the list flow through to
  # THIS script's stdout unchanged.
  python3 "$SCRIPT_DIR/generate-release-notes.py" "${notes_args[@]}" 2>>"$PREPARE_LOG" \
    || { echo "ERROR: release-notes generation failed — last 50 log lines:" >&2; tail -50 "$PREPARE_LOG" >&2; exit 1; }
fi
