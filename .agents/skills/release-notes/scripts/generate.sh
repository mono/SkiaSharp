#!/usr/bin/env bash
#
# generate.sh — the Prepare phase: run the two deterministic
# release-notes/changelog generators in the required order (Cake -> Python),
# VERBOSE, and write the "Files to polish" list to a file.
#
# Output contract (spec §2.2/§2.3): this script is VERBOSE. Cake and Python
# stream their full progress straight to stdout/stderr (i.e. the CI job log), so
# a long NuGet download or a disk/timeout failure is visible AS IT HAPPENS — it
# is never hidden in a temp log. The machine-readable "Files to polish" list does
# NOT ride on stdout: the Python generator ALWAYS writes it to a file
# (output/files-to-polish.txt by default; pass --polish-list <path> to choose the
# location). One repo-relative path per line; an empty file = nothing changed. The
# Prepare job uploads that file as an artifact for the Polish agent to read.
#
# This is the single entry point for the Prepare phase. The release-notes skill
# and the update-release-notes workflow both call THIS, so neither has to know the
# individual commands. The AI Polish phase (spec §2.2) is deliberately NOT run
# here — that is the agent's job once this script finishes.
#
# Usage:
#   generate.sh [--api-only | --notes-only] [--polish-list <path>] \
#               [extra args for the Python script...]
#
#   (no scope args)      Full regeneration of everything: API changelogs (Cake)
#                        then release-notes raw data for every branch (Python --all).
#   --api-only           Run only the Cake API-changelog generator.
#   --notes-only         Run only the Python release-notes generator.
#   --polish-list <path> Forwarded to the Python generator: write the "Files to
#                        polish" list to <path> instead of the default
#                        output/files-to-polish.txt.
#   <extra args>         Forwarded verbatim to generate-release-notes.py, replacing
#                        the default `--all` (e.g. `--branch main`, a version, etc.).
#
# Exit status is non-zero (with a clear message) if a required tool is missing —
# the skill must then stop and ask the user to install it, never work around it.
# Any generator failure aborts immediately (set -e); the verbose log already shows
# why, so there is no separate failure dump.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(git -C "$SCRIPT_DIR" rev-parse --show-toplevel)"

# The two generation paths each have a single canonical script under
# scripts/infra/docs/ that local runs, CI, and the docs Docker wrapper all share. This
# orchestrator just calls them in the required order (Path 1 changelogs -> Path 2
# notes); it owns no commands of its own so nothing can drift between here and CI.
CHANGELOGS_SH="$REPO_ROOT/scripts/infra/docs/generate-changelogs.sh"
RELEASE_NOTES_SH="$REPO_ROOT/scripts/infra/docs/generate-release-notes.sh"

run_api=1
run_notes=1
polish_list=""
notes_args=()

while [ $# -gt 0 ]; do
  case "$1" in
    --api-only)        run_notes=0; shift ;;
    --notes-only)      run_api=0;   shift ;;
    --polish-list)     polish_list="${2:?--polish-list needs a path}"; shift 2 ;;
    --polish-list=*)   polish_list="${1#*=}"; shift ;;
    --)                shift; notes_args+=("$@"); break ;;
    *)                 notes_args+=("$1"); shift ;;
  esac
done

# Default scope: regenerate every branch.
if [ "${#notes_args[@]}" -eq 0 ]; then
  notes_args=(--all)
fi

cd "$REPO_ROOT"

if [ "$run_api" = 1 ]; then
  echo "==> Prepare [1/2]: API changelogs (Path 1) — verbose"
  "$CHANGELOGS_SH"
fi

if [ "$run_notes" = 1 ]; then
  py_args=("${notes_args[@]}")
  if [ -n "$polish_list" ]; then
    py_args+=(--polish-list "$polish_list")
    echo "==> Files-to-polish list -> $polish_list"
  fi
  echo "==> Prepare [2/2]: release-notes raw data (Path 2) — verbose"
  "$RELEASE_NOTES_SH" "${py_args[@]}"
fi
