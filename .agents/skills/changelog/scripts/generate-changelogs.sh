#!/usr/bin/env bash
#
# generate-changelogs.sh — Generate SkiaSharp API changelogs.
#
# WHY THIS SCRIPT EXISTS
# ----------------------
# All the changelog-generation logic lives here (not in the workflow) so it can
# be run locally exactly the way CI runs it. The api-diff.yml workflow is a thin
# wrapper that calls this script and then opens a PR. If you want to regenerate
# changelogs on your machine, just run this script — no GitHub Actions needed.
#
# WHAT IT DOES
# ------------
# Drives the Cake targets that wrap Mono.ApiTools.NuGetDiff to produce per-
# assembly API diff markdown under changelogs/{PackageId}/{version}/{Assembly}.md.
#
# Two modes:
#
#   past     Regenerate ALL historical changelogs from packages already
#            published to NuGet.org. Each version is diffed against its
#            predecessor; scripts/versions.json supplies supersession +
#            compare_to overrides (e.g. 4.148 compares to 3.119.4, skipping the
#            superseded 4.147 previews). This is the default mode and what the
#            scheduled/push-triggered workflow runs.
#
#   preview  Diff a not-yet-published build (CI packages for a branch/tag/SHA)
#            against its NuGet.org baseline. Answers "what would the API
#            changelog look like once <ref> ships?". Downloads CI packages and
#            overlays the ref's nuget versions onto scripts/VERSIONS.txt.
#            Output is for inspection (output/api-diff/) — not committed.
#
# USAGE
# -----
#   ./generate-changelogs.sh past [--prerelease true|false]
#   ./generate-changelogs.sh preview <ref> [--prerelease true|false]
#
#   <ref> is a branch (main, release/X.Y.Z), a tag (vX.Y.Z) or a 40-char SHA.
#   --prerelease defaults to true (prereleases are included in the diff set).
#
# PREREQUISITES
# -------------
#   * Run from the repository root (where build.cake / global.json live).
#   * .NET SDK matching global.json, with `dotnet tool restore` already done
#     (the script runs it for you).
#   * Network access to NuGet.org (past + preview) and the AzDO CI feed (preview).

set -euo pipefail

# ── Argument parsing ────────────────────────────────────────────────────────

MODE="${1:-past}"
shift || true

REF=""
PRERELEASE="true"

# For preview mode the next positional arg is the ref (unless it's a flag).
if [[ "$MODE" == "preview" ]]; then
  if [[ $# -gt 0 && "$1" != --* ]]; then
    REF="$1"
    shift
  else
    REF="main"
  fi
fi

while [[ $# -gt 0 ]]; do
  case "$1" in
    --prerelease)
      PRERELEASE="$2"
      shift 2
      ;;
    *)
      echo "::error::Unknown argument: $1" >&2
      exit 1
      ;;
  esac
done

if [[ "$MODE" != "past" && "$MODE" != "preview" ]]; then
  echo "::error::Mode must be 'past' or 'preview' (got '$MODE')" >&2
  exit 1
fi

# ── Tooling ─────────────────────────────────────────────────────────────────

echo "Restoring dotnet tools..."
dotnet tool restore

# ── Past mode ───────────────────────────────────────────────────────────────
# Regenerate every historical changelog straight from NuGet.org. The Cake
# target reads scripts/versions.json for compare_to / supersession overrides.

if [[ "$MODE" == "past" ]]; then
  echo "Generating historical API diffs (past mode, prerelease=$PRERELEASE)..."
  dotnet cake --target=docs-api-diff-past --nugetDiffPrerelease="$PRERELEASE"
  echo "Done. Changelogs written under changelogs/."
  exit 0
fi

# ── Preview mode ────────────────────────────────────────────────────────────
# Diff CI packages for <ref> against their NuGet.org baseline.

echo "Preview mode for ref '$REF' (prerelease=$PRERELEASE)..."

# Classify the ref so we know whether to drive Cake by branch name or by SHA.
REF_KIND=""
if [[ "$REF" =~ ^(main|release/[0-9]+\.[0-9]+\.[0-9a-z._-]+)$ ]]; then
  REF_KIND="branch"
elif [[ "$REF" =~ ^v[0-9]+\.[0-9]+\.[0-9][0-9a-z._+-]*$ ]]; then
  REF_KIND="tag"
elif [[ "$REF" =~ ^[0-9a-f]{40}$ ]]; then
  REF_KIND="sha"
else
  echo "::error::Invalid ref: '$REF' (expected main, release/X.Y.Z, vX.Y.Z, or a 40-char SHA)" >&2
  exit 1
fi

RESOLVED_SHA=""

# For any ref other than the checked-out main, overlay that ref's nuget package
# versions onto scripts/VERSIONS.txt so the download/diff targets pull the right
# packages. main needs no overlay — it's already the working tree.
if [[ "$REF" != "main" ]]; then
  echo "Fetching '$REF' to overlay its nuget versions..."
  git fetch --depth=1 origin "$REF"
  RESOLVED_SHA=$(git rev-parse FETCH_HEAD)

  # Replace the "# nuget versions" block in VERSIONS.txt with the ref's block.
  REF_NUGET_LINES=$(git show FETCH_HEAD:scripts/VERSIONS.txt | grep -E '[[:space:]]nuget[[:space:]]')
  grep -v -E '[[:space:]]nuget[[:space:]]' scripts/VERSIONS.txt | grep -v '^# nuget versions' > scripts/VERSIONS.tmp
  mv scripts/VERSIONS.tmp scripts/VERSIONS.txt
  printf '\n# nuget versions\n' >> scripts/VERSIONS.txt
  echo "$REF_NUGET_LINES" >> scripts/VERSIONS.txt
fi

# Download the CI build output (packages) for the ref.
echo "Downloading CI packages..."
if [[ "$REF_KIND" == "branch" ]]; then
  dotnet cake --target=docs-download-output --gitBranch="$REF"
else
  dotnet cake --target=docs-download-output --gitSha="$RESOLVED_SHA"
fi

# Diff the downloaded packages against the NuGet.org baseline.
echo "Generating API diff..."
if [[ "$REF_KIND" == "branch" ]]; then
  dotnet cake --target=docs-api-diff --gitBranch="$REF" --nugetDiffPrerelease="$PRERELEASE"
else
  dotnet cake --target=docs-api-diff --gitSha="$RESOLVED_SHA" --nugetDiffPrerelease="$PRERELEASE"
fi

echo "Done. Preview diff written under output/api-diff/ and changelogs/."
