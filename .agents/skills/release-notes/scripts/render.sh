#!/usr/bin/env bash
#
# render.sh — the release-notes POLISH-FINALIZE phase (offline).
#
# After the AI has written _sources/<version>.prose.json for each page, this builds
# ALL the Markdown from the committed JSON, deterministically and without a network:
#   * every <version>.md (from its data.json + prose.json), and the deterministic
#     no-changes pages (from data.json alone);
#   * TOC.yml + index.md, rebuilt from the finished page set + the committed schedule.
# It fails loudly (non-zero) if any committed prose.json is invalid, so a bad page can
# never ship.
#
# This is just render-notes.py --all under one banner — the single command the AI (and
# a human) run to finalize. The three flags are accepted for a uniform interface with
# prepare.sh; --min-version/--max-version additionally render those pages individually
# first (targeted validation output), and --force is a no-op (the render is idempotent —
# it always rewrites from the current JSON).
#
# Requires: python3 only (pure stdlib; reads _sources/index.json for the schedule).

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../../.." && pwd)"
cd "$REPO_ROOT"

RENDER_PY="$SCRIPT_DIR/render-notes.py"
RELEASES_DIR="documentation/docfx/releases"

MIN=""
MAX=""
while [ $# -gt 0 ]; do
  case "$1" in
    --force)           shift ;;                      # idempotent render; no-op
    --min-version)     MIN="${2:?--min-version needs a value}"; shift 2 ;;
    --min-version=*)   MIN="${1#*=}"; shift ;;
    --max-version)     MAX="${2:?--max-version needs a value}"; shift 2 ;;
    --max-version=*)   MAX="${1#*=}"; shift ;;
    *) echo "render.sh: unknown argument '$1' (only --force/--min-version/--max-version)" >&2; exit 2 ;;
  esac
done

# Optional: when scoped, render the in-range pages individually first so their
# validation errors surface with a clear per-page message before the --all pass.
if [ -n "$MIN" ] || [ -n "$MAX" ]; then
  echo "==> Render: scoped per-page pass (${MIN:-min}..${MAX:-max})"
  for dj in "$RELEASES_DIR"/_sources/*.data.json; do
    [ -e "$dj" ] || continue
    v="$(basename "$dj" .data.json)"
    core="${v%-unreleased}"
    if [ -n "$MIN" ] && [ "$(printf '%s\n%s\n' "$MIN" "$core" | sort -V | head -1)" != "$MIN" ]; then continue; fi
    if [ -n "$MAX" ] && [ "$(printf '%s\n%s\n' "$core" "$MAX" | sort -V | head -1)" != "$core" ]; then continue; fi
    pj="$RELEASES_DIR/_sources/$v.prose.json"
    [ -f "$pj" ] || { echo "   (no prose.json for $v yet — skipping)"; continue; }
    python3 "$RENDER_PY" "$dj" "$pj" "$RELEASES_DIR/$v.md"
  done
fi

# The authoritative pass: render every page from committed JSON + rebuild TOC/index.
echo "==> Render: full pass (every page + TOC.yml + index.md)"
python3 "$RENDER_PY" --all
