#!/usr/bin/env bash
#
# prepare.sh — the release-notes PREPARE phase (network).
#
# One entrypoint that produces every deterministic input the Polish AI needs, in the
# required order:
#   1. API diffs        (Cake docs-api-diff)  — the committed releases/<line>/ trees
#                                                + the co-release map sidecar.
#   2. Page facts        (build-data.py)      — _sources/<version>.data.json + the
#                                                "Files to polish" list.
#   3. Index data        (build-index.py)     — _sources/index.json (Chrome schedule
#                                                + live-head set) for the offline render.
#
# All three engines are INCREMENTAL: with no flags they skip work whose output is
# already current, so a routine run is cheap. The three flags below are forwarded to
# each engine; there are no other options (the old --api-only/--notes-only are gone —
# an unforced run already skips unchanged api diffs).
#
#   --force              Rebuild even when the output already exists / is unchanged.
#   --min-version X      Lower bound (inclusive), e.g. 4.151.0.
#   --max-version Y      Upper bound (inclusive); set equal to --min-version for a
#                        single version.
#
# After this finishes, the AI writes _sources/<version>.prose.json for each page in
# output/files-to-polish.txt, then render.sh builds the Markdown. See SKILL.md.
#
# Requires: dotnet (Cake), python3, git history, and gh (PR authors). Any missing
# tool aborts with a clear message — do not work around it.

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../../.." && pwd)"
cd "$REPO_ROOT"

BUILD_DATA_PY="$SCRIPT_DIR/build-data.py"
BUILD_INDEX_PY="$SCRIPT_DIR/build-index.py"

FORCE=""
MIN=""
MAX=""
while [ $# -gt 0 ]; do
  case "$1" in
    --force)           FORCE=1; shift ;;
    --min-version)     MIN="${2:?--min-version needs a value}"; shift 2 ;;
    --min-version=*)   MIN="${1#*=}"; shift ;;
    --max-version)     MAX="${2:?--max-version needs a value}"; shift 2 ;;
    --max-version=*)   MAX="${1#*=}"; shift ;;
    *) echo "prepare.sh: unknown argument '$1' (only --force/--min-version/--max-version)" >&2; exit 2 ;;
  esac
done

# --- 1. API diffs (Cake). The shared contract is the `docs-api-diff` Cake target
#        (in build.cake, which isolates the heavy api-diff addins via RunCake).
#        --nugetDiffPrerelease=true enumerates prereleases so active dev lines that
#        ship only as previews/rcs can be diffed. Translate the shared flags to
#        Cake's argument syntax. ---
cake_args=(--target=docs-api-diff --nugetDiffPrerelease=true)
[ -n "$FORCE" ] && cake_args+=(--force=true)
[ -n "$MIN" ]   && cake_args+=(--minVersion="$MIN")
[ -n "$MAX" ]   && cake_args+=(--maxVersion="$MAX")
echo "==> Prepare [1/3]: API diffs (Cake docs-api-diff) — verbose"
dotnet tool restore
dotnet cake "${cake_args[@]}"

# --- 2. Page facts (build-data.py). Same flags, Python syntax. ---
py_args=()
[ -n "$FORCE" ] && py_args+=(--force)
[ -n "$MIN" ]   && py_args+=(--min-version "$MIN")
[ -n "$MAX" ]   && py_args+=(--max-version "$MAX")
echo "==> Prepare [2/3]: page facts (build-data.py) — verbose"
python3 "$BUILD_DATA_PY" "${py_args[@]:+${py_args[@]}}" --polish-list output/files-to-polish.txt

# --- 3. Index data (build-index.py). Network-sourced; no scope (it is repo-global). ---
echo "==> Prepare [3/3]: index data (build-index.py) — verbose"
python3 "$BUILD_INDEX_PY"

echo "==> Prepare complete. Files to polish:"
cat output/files-to-polish.txt || true
