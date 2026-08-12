---
description: "Regenerate website release notes AND API diffs daily (and on every main push) — new tags, releases, and release-branch commits are discovered automatically. One pipeline, one PR."
# Pin the model used for large release-note contexts.
engine:
  id: copilot
model: gpt-5.6-sol
# Main discovers every release branch and tag; manual dispatch validates another source.
on:
  push:
    branches: [main]
    # Ignore generated output, but rerun for maintainer-authored notes sidecars.
    paths:
      - "**"
      - "!documentation/docfx/releases/**"
      - "documentation/docfx/releases/**/*.notes.md"
  schedule:
    - cron: "0 0 * * *"
  workflow_dispatch:
    inputs:
      source_branch:
        description: "Existing branch to generate from and target. The generated PR always uses bot/release-notes."
        required: false
        default: "main"
        type: string
      min_version:
        description: "Lower bound (inclusive) for generation, e.g. '4.150.0'. Empty = no lower bound. Combine with max_version to regenerate a range, or set both equal to regenerate a single version."
        required: false
        default: ""
        type: string
      max_version:
        description: "Upper bound (inclusive) for generation, e.g. '4.148.0'. Empty = no upper bound."
        required: false
        default: ""
        type: string
      force:
        description: "Force a total regeneration at or above the configured history floor: rebuild every selected api diff and page even when unchanged (passes --force through to Cake + release_notes/generate.py)."
        required: false
        default: false
        type: boolean
  skip-bots: [github-actions, copilot, dependabot]
concurrency:
  # Every source reuses the same automation branch, so runs must serialize globally.
  group: update-release-notes
  cancel-in-progress: false
timeout-minutes: 60
permissions:
  contents: read
checkout:
  - fetch-depth: 0
# Skip the agent and PR when deterministic Prepare output is unchanged.
if: needs.prepare.outputs.has_changes == 'true'
# Prepare runs the deterministic, disk-heavy generators on a separate runner.
jobs:
  prepare:
    name: Prepare (api diffs + release-notes raw data)
    runs-on: ubuntu-latest
    timeout-minutes: 120
    permissions:
      contents: read
      pull-requests: read
    outputs:
      has_changes: ${{ steps.package.outputs.has_changes }}
      output_base_branch: ${{ steps.route.outputs.output_base_branch }}
    steps:
      - name: Checkout
        uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1  # v7.0.1
        with:
          fetch-depth: 0
      - name: Validate output routing
        id: route
        env:
          GH_TOKEN: ${{ github.token }}
          REPOSITORY: ${{ github.repository }}
          SOURCE_BRANCH: ${{ inputs.source_branch || 'main' }}
        run: |
          set -euo pipefail
          fail() {
            echo "::error::$*"
            exit 1
          }
          git check-ref-format --branch "$SOURCE_BRANCH" >/dev/null 2>&1 ||
            fail "Invalid source branch name: $SOURCE_BRANCH"
          [[ "$SOURCE_BRANCH" =~ ^[A-Za-z0-9._/-]+$ ]] ||
            fail "source_branch may contain only letters, digits, dot, underscore, slash, and hyphen"
          [ "$SOURCE_BRANCH" != "bot/release-notes" ] ||
            fail "source_branch cannot be the generated bot/release-notes branch"
          git ls-remote --exit-code --heads origin \
            "refs/heads/$SOURCE_BRANCH" >/dev/null 2>&1 ||
            fail "Source branch does not exist on origin: $SOURCE_BRANCH"

          owner="${REPOSITORY%%/*}"
          open_bases="$(
            gh api --method GET "repos/$REPOSITORY/pulls" \
              -f state=open \
              -f "head=$owner:bot/release-notes" \
              -f per_page=100 \
              --jq '.[].base.ref'
          )" || fail "Could not inspect open pull requests for bot/release-notes"
          while IFS= read -r open_base; do
            [ -z "$open_base" ] && continue
            [ "$open_base" = "$SOURCE_BRANCH" ] ||
              fail "bot/release-notes already backs an open PR targeting $open_base; merge or close that PR before generating for $SOURCE_BRANCH"
          done <<< "$open_bases"

          echo "Validated release-notes routing:"
          echo "  source=$SOURCE_BRANCH"
          echo "  base=$SOURCE_BRANCH"
          echo "  head=bot/release-notes"
          echo "output_base_branch=$SOURCE_BRANCH" >> "$GITHUB_OUTPUT"
      - name: Free up disk space
        run: |
          set -euo pipefail
          echo "Disk before cleanup:"; df -h /
          # Remove unused toolchains so the feed-based API diff has room.
          sudo rm -rf /usr/local/lib/android /opt/ghc /usr/local/.ghcup \
                      /usr/share/swift /usr/share/dotnet/sdk 2>/dev/null || true
          sudo docker image prune --all --force 2>/dev/null || true
          echo "Disk after cleanup:"; df -h /
      - name: Start from a clean source tree
        env:
          SOURCE_BRANCH: ${{ inputs.source_branch || 'main' }}
        run: |
          set -euo pipefail
          # Generation walks all release branches and tags.
          git fetch origin --tags --force --quiet
          git fetch origin "$SOURCE_BRANCH" --quiet
          git checkout -B "$SOURCE_BRANCH" "origin/$SOURCE_BRANCH"
      - name: Setup .NET
        uses: actions/setup-dotnet@a98b56852c35b8e3190ac28c8c2271da59106c68  # v6.0.0
        with:
          global-json-file: global.json
      - name: Generate (verbose)
        env:
          GH_TOKEN: ${{ github.token }}
          GITHUB_TOKEN: ${{ github.token }}
          FORCE_REGEN: ${{ inputs.force }}
          MIN_VERSION: ${{ inputs.min_version }}
          MAX_VERSION: ${{ inputs.max_version }}
        run: |
          set -euo pipefail
          # prepare.sh owns API diffs, release facts/context, and index data.
          flags=()
          if [ "${FORCE_REGEN:-false}" = "true" ]; then flags+=(--force); fi
          if [ -n "${MIN_VERSION:-}" ]; then flags+=(--min-version "$MIN_VERSION"); fi
          if [ -n "${MAX_VERSION:-}" ]; then flags+=(--max-version "$MAX_VERSION"); fi
          bash .agents/skills/release-notes/scripts/prepare.sh "${flags[@]:+${flags[@]}}"
      - name: Package Prepare output
        id: package
        run: |
          set -euo pipefail
          mkdir -p "$RUNNER_TEMP/prepare-out"
          # Pass every deterministic working-tree change to the agent.
          git add -A
          git diff --cached --binary > "$RUNNER_TEMP/prepare-out/prepare.patch"
          git reset -q
          cp output/files-to-polish.txt "$RUNNER_TEMP/prepare-out/files-to-polish.txt"
          echo "Patch size: $(wc -c < "$RUNNER_TEMP/prepare-out/prepare.patch") bytes"
          echo "Files to polish:"; cat "$RUNNER_TEMP/prepare-out/files-to-polish.txt" || true
          if [ -s "$RUNNER_TEMP/prepare-out/prepare.patch" ]; then
            echo "has_changes=true" >> "$GITHUB_OUTPUT"
          else
            echo "has_changes=false" >> "$GITHUB_OUTPUT"
            echo "Prepare produced an empty patch — agent and PR will be skipped."
          fi
      - name: Upload Prepare output
        uses: actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a # v4.4.3
        with:
          name: release-notes-prepare
          path: ${{ runner.temp }}/prepare-out
          retention-days: 1
          if-no-files-found: error
# Restore Prepare output before the offline prose agent starts.
pre-agent-steps:
  - name: Start from a clean source tree
    env:
      SOURCE_BRANCH: ${{ inputs.source_branch || 'main' }}
    run: |
      set -euo pipefail
      mkdir -p /tmp/gh-aw
      git fetch origin "$SOURCE_BRANCH" --quiet
      git checkout -B "$SOURCE_BRANCH" "origin/$SOURCE_BRANCH"
  - name: Download Prepare output
    uses: actions/download-artifact@3e5f45b2cfb9172054b4087a40e8e0b5a5461e7c # v4.1.8
    with:
      name: release-notes-prepare
      path: /tmp/gh-aw/prepare-in
  - name: Restore Prepare output
    run: |
      set -euo pipefail
      mkdir -p output
      cp /tmp/gh-aw/prepare-in/files-to-polish.txt output/files-to-polish.txt
      if [ -s /tmp/gh-aw/prepare-in/prepare.patch ]; then
        git apply --3way --whitespace=nowarn /tmp/gh-aw/prepare-in/prepare.patch
        echo "Applied Prepare patch."
      else
        echo "Prepare produced no changes; nothing to apply."
      fi
tools:
  # Explicitly allow only offline reading, rendering, and commit operations.
  bash: ["cat", "grep", "sort", "head", "tail", "git", "python3"]
  edit:
network: {}
safe-outputs:
  needs: [prepare]
  create-pull-request:
    title-prefix: "[docs] "
    labels: [area/Docs, partner/agentic-workflows]
    draft: false
    base-branch: "${{ needs.prepare.outputs.output_base_branch }}"
    allowed-base-branches: ["${{ needs.prepare.outputs.output_base_branch }}"]
    allowed-branches: [bot/release-notes]
    allowed-files: ["documentation/docfx/releases/**"]
    max-patch-files: 100
    preserve-branch-name: true
    recreate-ref: true
---

# Sync - Release Notes & API Diffs

This is the single pipeline that keeps the website release notes **and** the API
api diffs current — there is no separate api-diff workflow. The deterministic
generators run in a dedicated **`prepare`** job, the agent then polishes prose,
and **one** pull request ships everything.

## What already ran: the Prepare phase (do NOT re-run)

Before you (the agent) started, a **separate `prepare` job** ran the skill's
**Prepare** phase on its own disk-managed runner — the single script
`.agents/skills/release-notes/scripts/prepare.sh` (API diffs via Cake, then atomic
per-page `_sources/<version>.data.json` + `.context.md`, then the network-sourced
`_sources/index.json` and the **Files-to-polish context list**). See the skill's "Running
the full pipeline" section and `documentation/dev/release-notes-and-api-diffs.md` §2
for exactly what it produces.

The `prepare` job uploaded its complete working-tree change as a patch plus that
manifest as an artifact, and a host step **already restored both** into this checkout:
the regenerated files (every changed `_sources/<version>.data.json` +
`.context.md`, and `_sources/index.json`) are on disk, and the context list is at
`output/files-to-polish.txt`. **You have no network —
do not re-run `prepare.sh`, `dotnet cake`, `release_notes/generate.py`, or `release_notes/index.py`.**
Your job is to write the prose and render the pages (below), then commit and open the PR.

The restored Prepare output is authoritative and immutable to you. The only files
you may author by hand are the exact `_sources/<version>.prose.json` paths named by
`output/files-to-polish.txt`; `release_notes/render.py` owns the rendered release
pages and aggregate navigation. Never create, edit, normalize, or repair an API-diff
file, `data.json`, `context.md`, co-release map, or index-data file yourself. If a
deterministic artifact is missing or invalid, report it with the `missing_data`
safe-output and stop without committing or opening a PR.

> This agent job is gated on Prepare having actually changed something
> (`prepare.outputs.has_changes`). A no-op run — where the deterministic
> generators reproduced the existing tree byte-for-byte — is skipped *before* you
> start, so when you are running there is always at least the regenerated Prepare
> output on disk to commit.

## Your job: write the prose and render each page

Follow the **release-notes skill**
([`.agents/skills/release-notes/SKILL.md`](../../.agents/skills/release-notes/SKILL.md))
for **how** to write each page's prose and render it — the prose slots, the six
categories, the breaking-change sources (`*.breaking.md` + `_sources/<version>.notes.md`),
the per-page `release_notes/render.py` validation, and the "never hand-edit the page" rules all
live there. The renderer owns every heading, table, banner, `@handle`, ❤️, and PR link,
so you only ever write prose.

This run's **CI-specific deltas** on top of the skill:

0. Obey the validated output routing exactly:
   - Output PR base: `${{ needs.prepare.outputs.output_base_branch }}`
   - Output PR head: `bot/release-notes`
   The base was validated before Prepare. Do not substitute another branch.
1. Read `output/files-to-polish.txt`, one repo-relative
   `_sources/<version>.context.md` path per line. Process the list sequentially:
   read one context from beginning to end, recreate only its frontmatter-named
   `_sources/<version>.prose.json`, and render that page successfully before reading
   the next context. Never author multiple prose files in one edit.
   The list **may be empty**
   — that means no prose needs
   authoring, but there is
   still deterministic work to materialize (a refreshed API diff or the
   TOC/index), so do **not** exit early; go straight to the
   final render.
2. You have **no network**, and Prepare already ran — never re-run it (above).
3. Because the tool allowlist permits `python3` but **not** `render.sh`, finalize by
   running the renderer directly: `release_notes/render.py` per page to validate as you go
   (per the skill), then **once** at the end
   `python3 scripts/infra/docs/release_notes/render.py --all \
   --min-version="${{ inputs.min_version }}" \
   --max-version="${{ inputs.max_version }}"`
   to rebuild the selected pages + `TOC.yml`/`index.md` (offline, from committed
   JSON). Empty production bounds render and prune globally. If `--all` reports a
   prose error in one of the listed files, fix it and re-run. Any other failure is a
   deterministic Prepare defect: call `missing_data` and stop.
4. Commit and open the PR (below). If, after `--all`, `git status` shows the working
   tree is genuinely unchanged, make no commit and exit; otherwise commit everything.

## How the PR is made

`create-pull-request` consumes a commit; it does not stage or commit the working
tree. After every page and the final render pass:

1. `git checkout -b "bot/release-notes"`.
2. `git add -A` to stage Prepare output, authored prose, rendered pages, and
   aggregate navigation.
3. `git commit -m "docs: regenerate API diffs and release notes"`.
4. Call the `create_pull_request` **MCP safe-output tool directly** with
   `base: "${{ needs.prepare.outputs.output_base_branch }}"` and
   `branch: "bot/release-notes"`.

Commit once, after every edit. If the working tree is unexpectedly unchanged,
exit without creating a PR.
