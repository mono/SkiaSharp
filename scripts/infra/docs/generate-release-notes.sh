#!/usr/bin/env bash
#
# generate-release-notes.sh — Path 2: regenerate the human release-notes prose.
#
# Runs the deterministic Python release-notes engine, which turns the committed
# changelog trees + co-release sidecar (produced by Path 1) into the per-line human
# pages and raw data under documentation/docfx/releases/. For a full refresh, run
# generate-changelogs.sh (Path 1) FIRST so the changelog inputs are current.
#
# Engine: python3 scripts/infra/docs/generate-release-notes.py
#   spec:  documentation/dev/release-notes-and-changelogs.md   (read this first)
#
# Single source of truth for Path 2: the release-notes skill's generate.sh, CI, the
# docs Docker wrapper (run.sh notes), and a human all call THIS.
#
# Usage:
#   generate-release-notes.sh [args...]          (default scope: --all)
#
#   <args> are forwarded verbatim to generate-release-notes.py, replacing the default
#   --all (e.g. --branch main, a version, --polish-list <path>, ...).
#
# Requires: python3. The generator uses the GitHub CLI ('gh') to resolve PR authors;
# export GITHUB_TOKEN or GH_TOKEN so it can authenticate (CI passes one through).
set -euo pipefail

# Repo root is a fixed three levels up from scripts/infra/docs/. Derive it from the
# script's own location rather than `git rev-parse` so this works everywhere the tree
# is checked out — including a git WORKTREE bind-mounted into the docs Docker image,
# where the repo's .git is a pointer file to a gitdir outside the mount. The Python
# generator DOES read git history (for PR authors), so a real .git must still be
# reachable when this path is run for real; deriving the root this way just removes a
# hard dependency on `git rev-parse` for locating the tree.
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
cd "$REPO_ROOT"

PY_GENERATOR="$SCRIPT_DIR/generate-release-notes.py"

if ! command -v python3 >/dev/null 2>&1; then
    echo "ERROR: 'python3' is required for the release-notes generator but was not found." >&2
    echo "       Install Python 3 (or run via scripts/infra/docs/docker/run.sh) and retry." >&2
    exit 1
fi
if [ ! -f "$PY_GENERATOR" ]; then
    echo "ERROR: release-notes generator not found at $PY_GENERATOR" >&2
    exit 1
fi

# Default scope: regenerate every branch.
if [ "$#" -eq 0 ]; then
    set -- --all
fi

echo "==> Path 2: release notes (generate-release-notes.py $*) — verbose"
python3 "$PY_GENERATOR" "$@"
