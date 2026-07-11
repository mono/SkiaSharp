---
description: "Regenerate website release notes AND API diffs daily (and on every main push) — new tags, releases, and release-branch commits are discovered automatically. One pipeline, one PR."
# ENGINE — use the default Copilot model. The reworked pipeline hands the agent
# fully-structured facts (per-page data.json + companion files) and a deterministic
# renderer, so the Polish phase no longer needs a pinned stronger model (validated
# by an A/B run: the default model matched — and on one behavioural breaking change
# beat — the pinned Opus output). Recompile the .lock.yml after changing this.
engine:
  id: copilot
# TRIGGERS — main is the single source of truth for EVERY version/branch.
# Deliberately NOT triggered by `release/**` pushes or `v*` tags: a push/tag event
# runs the workflow copy that lives on THAT ref, not main's, so those triggers can
# only ever run a stale per-branch copy (exactly the duplicate-PR problem we removed).
# Instead, main's run walks every release/* ref and reads git tags itself, so a new
# stable tag or release-branch commit is picked up by the next daily run with no
# per-branch/tag workflow needed. Want it instant after tagging? Dispatch manually.
on:
  push:
    branches: [main]
    paths-ignore:
      - "documentation/docfx/releases/**"
  schedule:
    # Daily. Catches new stable tags (vX.Y.Z → page flips to "stable"), new
    # release-branch commits (unreleased deltas), and newly published NuGets
    # within ~24h. Quiet days are cheap: the generators are deterministic, so an
    # unchanged run yields an empty Prepare patch and the agent + PR are skipped
    # (see the `prepare` job's `has_changes` output and the top-level `if:`).
    - cron: "0 0 * * *"
  workflow_dispatch:
    inputs:
      source_branch:
        description: "Branch to generate from (its scripts + content). Defaults to main; override to validate a feature branch's pipeline before merge, or to refresh immediately after tagging instead of waiting for the daily run."
        required: false
        default: "main"
        type: string
      min_version:
        description: "Lower bound (inclusive) for generation, e.g. '3.116.0'. Empty = no lower bound. Combine with max_version to regenerate a range, or set both equal to regenerate a single version."
        required: false
        default: ""
        type: string
      max_version:
        description: "Upper bound (inclusive) for generation, e.g. '4.148.0'. Empty = no upper bound."
        required: false
        default: ""
        type: string
      force:
        description: "Force a total regeneration: rebuild every api diff and page even when unchanged (passes --force through to Cake + release-notes-data.py). Use to rebuild the whole back-catalogue after an api-diff-tool or page-format change."
        required: false
        default: false
        type: boolean
  skip-bots: [github-actions, copilot, dependabot]
concurrency:
  group: update-release-notes
  cancel-in-progress: true
# The agent only POLISHES prose now — the heavy, deterministic Prepare phase runs
# in its own `prepare` job (below), so the agent's own budget is modest.
timeout-minutes: 60
permissions:
  contents: read
# The agent replays the Prepare patch onto a clean main checkout and polishes; it
# needs main's history for create-pull-request's branch operations.
checkout:
  - fetch-depth: 0
# AGENT GATE — only polish + open a PR when Prepare actually changed something.
# The `prepare` job emits `has_changes` (its patch is non-empty); on a no-op run
# it is 'false', this `if:` is false, and the agent job is skipped — which also
# skips `safe_outputs`, so no spurious PR is opened and no agent runtime is spent.
# The agent still `needs:` activation + prepare, so a skipped need (e.g. activation
# gated off) also skips the agent the usual way.
if: needs.prepare.outputs.has_changes == 'true'
# ---------------------------------------------------------------------------
# PREPARE — a dedicated, disk-managed, VERBOSE job on its own runner.
#
# The deterministic generators (Cake API-diff over EVERY published NuGet of BOTH
# families incl. prereleases + Python raw data over every branch) are far too
# disk- and time-heavy to share the agent runner: that combination exhausted the
# hosted runner's disk on the first post-merge run (issue #4191). So Prepare gets
# its OWN job with its OWN timeout and a free-disk-space step, runs verbose (all
# progress visible in the job log), and hands its result to the agent as an
# artifact — a git patch of every working-tree change plus the files-to-polish
# list. See documentation/dev/release-notes-and-api-diffs.md §2.2/§2.3.
# ---------------------------------------------------------------------------
jobs:
  prepare:
    name: Prepare (api diffs + release-notes raw data)
    runs-on: ubuntu-latest
    timeout-minutes: 120
    permissions:
      contents: read
    outputs:
      # true iff Prepare's patch is non-empty (something actually changed). The
      # top-level `if:` uses this to skip the agent + PR on no-op runs.
      has_changes: ${{ steps.package.outputs.has_changes }}
    steps:
      - name: Checkout
        uses: actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd # v5.0.0
        with:
          fetch-depth: 0
      - name: Free up disk space
        run: |
          set -euo pipefail
          echo "Disk before cleanup:"; df -h /
          # Drop large preinstalled toolchains we never use here so the NuGet
          # diff (every package of both families) has room. setup-dotnet below
          # reinstalls the pinned SDK after we clear the preinstalled one.
          sudo rm -rf /usr/local/lib/android /opt/ghc /usr/local/.ghcup \
                      /usr/share/swift /usr/share/dotnet/sdk 2>/dev/null || true
          sudo docker image prune --all --force 2>/dev/null || true
          echo "Disk after cleanup:"; df -h /
      - name: Start from a clean source tree
        env:
          SOURCE_BRANCH: ${{ inputs.source_branch || 'main' }}
        run: |
          set -euo pipefail
          # Make EVERY release/* branch and tag available locally. The generators
          # walk all release/* refs (release-notes deltas, cross-minor rollups) and
          # read git tags to decide stable-vs-prerelease (_version_has_stable_tag):
          # a daily run is the moment a freshly-pushed vX.Y.Z tag flips its page to
          # "stable", so fetch tags explicitly (belt-and-suspenders over checkout's
          # fetch-depth:0) and force-update any moved refs.
          git fetch origin --tags --force --quiet
          # Release notes/api diffs target main, so push/schedule runs always
          # generate from main (a push to release/* is a content *source*, not the
          # PR base). A manual dispatch may point SOURCE_BRANCH at a feature branch
          # to validate its pipeline on CI before merge.
          git fetch origin "$SOURCE_BRANCH" --quiet
          git checkout -B "$SOURCE_BRANCH" "origin/$SOURCE_BRANCH"
      - name: Setup .NET
        uses: actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9 # v4.3.1
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
          # Single entry point: prepare.sh runs the API diffs (Cake) then the release-
          # notes data (release-notes-data.py) then the index (release-notes-index.py), all VERBOSE.
          # Every engine is incremental — an unforced run skips work whose output is
          # already current (a shipped api diff never changes), so a daily run is cheap
          # without any "notes-only" flag. A dispatch may bound to a version RANGE
          # (--min-version/--max-version, for a single version or a chunk) and/or force
          # a total rewrite (--force). The "Files to polish" list lands at
          # output/files-to-polish.txt.
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
          # Capture EVERY working-tree change (the releases tree, the co-release
          # map sidecar, the author cache, and any deletions/pruning) as one patch
          # so the agent can replay it onto a clean main checkout.
          git add -A
          git diff --cached --binary > "$RUNNER_TEMP/prepare-out/prepare.patch"
          git reset -q
          cp output/files-to-polish.txt "$RUNNER_TEMP/prepare-out/files-to-polish.txt"
          echo "Patch size: $(wc -c < "$RUNNER_TEMP/prepare-out/prepare.patch") bytes"
          echo "Files to polish:"; cat "$RUNNER_TEMP/prepare-out/files-to-polish.txt" || true
          # Drive the top-level agent `if:`: a non-empty patch means something
          # changed and the agent should polish + open the PR; an empty patch is a
          # no-op run, so skip the agent and the PR entirely.
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
# ---------------------------------------------------------------------------
# POLISH — the agent restores Prepare's output, then edits prose only.
# These steps run in the agent job after its checkout and before the engine.
# ---------------------------------------------------------------------------
pre-agent-steps:
  - name: Start from a clean source tree
    env:
      SOURCE_BRANCH: ${{ inputs.source_branch || 'main' }}
    run: |
      set -euo pipefail
      mkdir -p /tmp/gh-aw
      # Match the prepare job's source so its patch applies cleanly.
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
      # The skill always reads output/files-to-polish.txt — put it back at that same
      # path so the agent's Polish phase is identical to a manual run.
      mkdir -p output
      cp /tmp/gh-aw/prepare-in/files-to-polish.txt output/files-to-polish.txt
      # Replay Prepare's working-tree changes; an empty patch = nothing changed.
      if [ -s /tmp/gh-aw/prepare-in/prepare.patch ]; then
        git apply --3way --whitespace=nowarn /tmp/gh-aw/prepare-in/prepare.patch
        echo "Applied Prepare patch."
      else
        echo "Prepare produced no changes; nothing to apply."
      fi
tools:
  # The agent reads the restored _sources/<version>.data.json sidecars, writes
  # _sources/<version>.prose.json, and runs release-notes-render.py (pure stdlib, no
  # network) to render each page and then `--all` to rebuild the TOC/index, then
  # commits and opens the PR. python3 is allowed ONLY for release-notes-render.py — it must NOT
  # re-run the heavy generators (they already ran in the prepare job). Keep an
  # explicit allowlist: it is the only thing that stops the agent shelling out to
  # anything else. Dropping the bash block entirely makes gh-aw compile to
  # `--allow-all-tools` (strictly worse). No sed/awk: they rewrite files in place,
  # bypassing the edit tool.
  #
  # git is REQUIRED, not optional: the create-pull-request safe-output is
  # commit-based — it errors with "No changes to commit" unless the agent has run
  # `git add` + `git commit` first (and gh-aw force-injects the git suite anyway).
  # The agent MUST commit its work before calling create_pull_request — see the
  # "How the PR is made" section. The earlier 2000+-file blow-up was the OPPOSITE
  # mistake: the agent created a branch but never committed, so gh-aw's patch
  # generator fell back to diffing months of history and exceeded the PR file cap.
  bash: ["cat", "grep", "sort", "head", "tail", "git", "python3"]
  edit:
# The agent has no network: it only polishes prose from already-generated files.
network: {}
safe-outputs:
  create-pull-request:
    title-prefix: "[docs] "
    labels: [documentation, partner/agentic-workflows]
    draft: false
    allowed-base-branches: [main]
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
`.agents/skills/release-notes/scripts/prepare.sh` (API diffs via Cake, then the
per-page `_sources/<version>.data.json` facts, then the network-sourced
`_sources/index.json` and the **"Files to polish"** list). See the skill's "Running
the full pipeline" section and `documentation/dev/release-notes-and-api-diffs.md` §2
for exactly what it produces.

The `prepare` job uploaded its complete working-tree change as a patch plus that
list as an artifact, and a host step **already restored both** into this checkout:
the regenerated files (every `_sources/<version>.data.json` and `_sources/index.json`)
are on disk, and the list is at `output/files-to-polish.txt`. **You have no network —
do not re-run `prepare.sh`, `dotnet cake`, `release-notes-data.py`, or `release-notes-index.py`.**
Your job is to write the prose and render the pages (below), then commit and open the PR.

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
the per-page `release-notes-render.py` validation, and the "never hand-edit the page" rules all
live there. The renderer owns every heading, table, banner, `@handle`, ❤️, and PR link,
so you only ever write prose.

This run's **CI-specific deltas** on top of the skill:

1. Your page list is `output/files-to-polish.txt` (one repo-relative
   `documentation/docfx/releases/<version>.md` per line). It **may be empty** — that
   means no page needs prose, but there is still deterministic work to materialize
   (a rebuilt no-changes HarfBuzz section, a refreshed API diff, or the TOC/index), so
   do **not** exit early; go straight to the final render.
2. You have **no network**, and Prepare already ran — never re-run it (above).
3. Because the tool allowlist permits `python3` but **not** `render.sh`, finalize by
   running the renderer directly: `release-notes-render.py` per page to validate as you go
   (per the skill), then **once** at the end
   `python3 scripts/infra/docs/release-notes-render.py --all`
   to rebuild every page + the `TOC.yml`/`index.md` aggregates (offline, from the
   committed JSON). If `--all` exits non-zero, fix the reported prose and re-run.
4. Commit and open the PR (below). If, after `--all`, `git status` shows the working
   tree is genuinely unchanged, make no commit and exit; otherwise commit everything.

## How the PR is made

`create-pull-request` is a **commit-based** safe-output: it turns your commit into
the PR. It does **not** stage or commit for you. If you call it with an uncommitted
working tree it returns *"No changes to commit"*, and gh-aw then falls back to
diffing months of history (2000+ files, exceeding the PR file cap and failing the
run). So once every page renders cleanly you **must** commit everything yourself,
then create the PR:

1. `git checkout -b bot/release-notes` — create the PR branch from the current HEAD.
2. `git add -A` — stage **all** working-tree changes: the restored Prepare output
   (Cake API-diff tree + the `_sources/*.data.json` + `_sources/index.json`) **and**
   the `_sources/*.prose.json` you wrote **and** the rendered `<version>.md` pages
   **and** the regenerated `TOC.yml` + `index.md`.
3. `git commit -m "docs: regenerate API diffs and release notes"`.
4. Call the `create_pull_request` safe-output. It opens one PR targeting `main` from
   the `bot/release-notes` branch.


Commit **once**, at the very end, after every edit is done — not incrementally.
Because the job only starts when Prepare produced changes, the working tree is
never completely clean here, so you will always commit and open exactly one PR.
(As a defensive fallback only: if `git status` somehow shows no changes at all,
make no edits, run no git, and exit — no PR is created.)
