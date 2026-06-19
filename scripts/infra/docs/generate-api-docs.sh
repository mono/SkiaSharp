#!/usr/bin/env bash
#
# generate-api-docs.sh — Path 3: regenerate the mdoc XML reference docs.
#
# Produces the per-type/per-member XML under docs/SkiaSharpAPI/ (the docs submodule)
# by running mdoc over the built assemblies. This is the STUB-regeneration half of the
# auto-api-docs-writer workflow (docs/.github/workflows/auto-api-docs-writer.md); the
# AI placeholder-fill ("To be added." → prose) is the api-docs skill's separate job and
# is NOT done here.
#
# Steps (mirror the workflow's regenerate-stubs job):
#   1. dotnet tool restore
#   2. dotnet cake --target=docs-download-output   populate output/nugets from the CI feed
#   3. dotnet cake --target=update-docs            mdoc regenerates the XML (+ formats it)
#
# mdoc.exe is a .NET Framework executable; docs.cake runs it under mono on non-Windows
# hosts, so this whole path runs on any host. The auto-api-docs-writer regenerate-stubs
# job (Linux + mono) and the docs Docker image (scripts/infra/docs/docker/run.sh
# api-docs) both rely on that.
#
# Usage:
#   generate-api-docs.sh [--skip-download] [extra cake args for update-docs...]
#
#   --skip-download   Reuse the existing output/nugets instead of re-downloading (e.g.
#                     when a local package build already populated it). Errors if empty.
#
# Requires: dotnet; mono on non-Windows hosts; network access for the package download.
set -euo pipefail

# Repo root is a fixed three levels up from scripts/infra/docs/. Derive it from the
# script's own location rather than `git rev-parse` so this works everywhere the tree
# is checked out — including a git WORKTREE bind-mounted into the docs Docker image,
# where the repo's .git is a pointer file to a gitdir outside the mount.
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
cd "$REPO_ROOT"

skip_download=0
cake_args=()
while [ $# -gt 0 ]; do
    case "$1" in
        --skip-download) skip_download=1; shift ;;
        *)               cake_args+=("$1"); shift ;;
    esac
done

if ! command -v dotnet >/dev/null 2>&1; then
    echo "ERROR: the .NET SDK ('dotnet') is required for the API-docs generator but was not found." >&2
    echo "       Install the SDK pinned in global.json (or run via scripts/infra/docs/docker/run.sh) and retry." >&2
    exit 1
fi
case "$(uname -s)" in
    MINGW*|MSYS*|CYGWIN*|Windows_NT) ;;  # Windows: mdoc.exe runs natively
    *)
        if ! command -v mono >/dev/null 2>&1; then
            echo "ERROR: 'mono' is required to run mdoc.exe on non-Windows hosts but was not found." >&2
            echo "       Install mono (or run via scripts/infra/docs/docker/run.sh) and retry." >&2
            exit 1
        fi ;;
esac

echo "==> Path 3: mdoc XML docs — verbose"
dotnet tool restore

if [ "$skip_download" = 1 ]; then
    echo "==> [1/2] Skipping docs-download-output (--skip-download); reusing output/nugets."
else
    echo "==> [1/2] Downloading CI packages (cake docs-download-output)"
    dotnet cake --target=docs-download-output
fi

echo "==> [2/2] Regenerating XML docs (cake update-docs)"
dotnet cake --target=update-docs "${cake_args[@]}"
