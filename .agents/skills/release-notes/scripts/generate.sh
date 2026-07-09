#!/usr/bin/env bash
#
# generate.sh — the Prepare phase: run the two deterministic
# release-notes/api diff generators in the required order (Cake -> Python),
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
#   (no scope args)      Full regeneration of everything: API diffs (Cake)
#                        then release-notes raw data for every branch (Python default).
#   --api-only           Run only the Cake API-diff generator.
#   --notes-only         Run only the Python release-notes generator.
#   --polish-list <path> Forwarded to the Python generator: write the "Files to
#                        polish" list to <path> instead of the default
#                        output/files-to-polish.txt.
#   <extra args>         Forwarded verbatim to build-data.py, replacing
#                        the default full regen (e.g. `--force`, `--min-version X`, etc.).
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
# orchestrator just calls them in the required order (Path 1 api diffs -> Path 2
# notes); it owns no commands of its own so nothing can drift between here and CI.
API_DIFFS_SH="$REPO_ROOT/scripts/infra/docs/generate-api-diffs.sh"
RELEASE_NOTES_SH="$SCRIPT_DIR/build-data.sh"
BUILD_INDEX_PY="$SCRIPT_DIR/build-index.py"

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

# Default scope: regenerate every branch (the generator's default when passed no
# scope flags). Any extra args collected above (e.g. --force, --min-version) are
# forwarded verbatim.
cd "$REPO_ROOT"

if [ "$run_api" = 1 ]; then
  echo "==> Prepare [1/3]: API diffs (Path 1) — verbose"
  "$API_DIFFS_SH"
fi

if [ "$run_notes" = 1 ]; then
  # Build the python arg list. Guard every array expansion with ":+}" so an empty
  # array is safe under `set -u` on bash 3.2 (macOS) — with no scope flags the
  # generator regenerates every branch by default.
  py_args=("${notes_args[@]:+${notes_args[@]}}")
  if [ -n "$polish_list" ]; then
    py_args+=(--polish-list "$polish_list")
    echo "==> Files-to-polish list -> $polish_list"
  fi
  echo "==> Prepare [2/3]: release-notes page data (build-data.py) — verbose"
  "$RELEASE_NOTES_SH" "${py_args[@]:+${py_args[@]}}"

  # Prepare [3/3]: the network-sourced index data (Chrome schedule) -> index.json,
  # plus pruning of stale -unreleased pages. Must run here, in the network-capable
  # Prepare phase, so the offline render (render-notes.py --all) can build the
  # TOC/index without a network. Only relevant to the notes path.
  echo "==> Prepare [3/3]: index data (build-index.py) — verbose"
  python3 "$BUILD_INDEX_PY"
fi
