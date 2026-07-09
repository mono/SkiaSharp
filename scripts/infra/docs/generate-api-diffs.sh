#!/usr/bin/env bash
#
# generate-api-diffs.sh — Path 1: regenerate the committed API-diff trees.
#
# Diffs every published version of each tracked package (SkiaSharp + HarfBuzz
# families) and writes the per-line, per-package markdown under
# documentation/docfx/releases/<line>/<pkg>/*.md (+ the harfbuzzsharp/ tree and the
# co-release sidecar the release-notes engine consumes).
#
# Engine: dotnet cake --target=docs-api-diff-past
#   code:  scripts/infra/docs/api-diff.cake
#   spec:  documentation/dev/release-notes-and-api-diffs.md   (read this first)
#
# This is the single source of truth for Path 1: the release-notes skill's
# generate.sh, the update-release-notes CI workflow, the docs Docker wrapper
# (scripts/infra/docs/docker/run.sh api-diffs), and a human running it by hand all
# call THIS so the command never drifts between local and CI.
#
# Usage:
#   generate-api-diffs.sh [extra cake args...]
#
# Requires: dotnet (the SDK pinned in global.json).
set -euo pipefail

# Repo root is a fixed three levels up from scripts/infra/docs/. Derive it from the
# script's own location rather than `git rev-parse` so this works everywhere the tree
# is checked out — including a git WORKTREE bind-mounted into the docs Docker image,
# where the repo's .git is a pointer file to a gitdir outside the mount (this path
# reads no git history, so it needs no working .git at all).
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
cd "$REPO_ROOT"

if ! command -v dotnet >/dev/null 2>&1; then
    echo "ERROR: the .NET SDK ('dotnet') is required for the API-diff generator but was not found." >&2
    echo "       Install the SDK pinned in global.json (or run via scripts/infra/docs/docker/run.sh) and retry." >&2
    exit 1
fi

echo "==> Path 1: API diffs (cake docs-api-diff-past) — verbose"
dotnet tool restore
# --nugetDiffPrerelease=true: enumerate prerelease packages so active dev lines that
# ship only as previews/rcs (e.g. 4.148/4.150) can be diffed (spec §5.1). Emission is
# still collapsed to one api diff per release line inside the target.
dotnet cake --target=docs-api-diff-past --nugetDiffPrerelease=true "$@"
