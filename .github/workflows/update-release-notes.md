---
description: "Regenerate website release notes AND API diffs daily (and on every main push) — new tags, releases, and release-branch commits are discovered automatically. One pipeline, one PR."
# ENGINE — deterministic preparation and rendering bound the task, but the prose
# still requires nuanced API and breaking-change judgment, so use Sol.
engine:
  id: copilot
model: gpt-6.1-sol
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
    # Ignore the generated release-notes tree so the workflow never triggers on its
    # OWN committed output (the <version>.data.json / prose.json inputs and the
    # rendered <version>.md pages) — EXCEPT for the human-editable *.notes.md manual
    # additions sidecars. A maintainer can add or edit a sidecar in a standalone PR;
    # merging it must kick off a fresh run (the sidecar's sha256 folds into data.json,
    # which invalidates the page and forces a re-polish). Expressed as negated `paths`
    # (order matters — the last matching pattern wins) rather than `paths-ignore`,
    # because GitHub cannot re-include a subpath out of a `paths-ignore` rule.
    paths:
      - "**"
      - "!documentation/docfx/releases/**"
      - "documentation/docfx/releases/**/*.notes.md"
  # Daily. Catches new stable tags (vX.Y.Z → page flips to "stable"), new
  # release-branch commits (unreleased deltas), and newly published NuGets
  # within ~24h. Quiet days are cheap: the generators are deterministic, so an
  # unchanged run yields an empty Prepare patch and the agent + PR are skipped
  # (see the `prepare` job's `has_changes` output and the top-level `if:`).
  schedule: daily
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
  cancel-in-progress: false
  job-discriminator: ${{ github.run_id }}
# The agent only POLISHES prose now — the heavy, deterministic Prepare phase runs
# in its own `prepare` job (below), so the agent's own budget is modest.
timeout-minutes: 60
permissions:
  contents: read
# The host restores Prepare's selected source, then creates a main-rooted output
# branch before the agent starts. Full history is needed for that branch boundary.
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
        uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1  # v7.0.1
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
  - name: Prepare main-based documentation output branch
    run: bash .agents/skills/release-notes/scripts/prepare-pr-branch.sh
tools:
  # The agent reads the restored _sources/<version>.data.json sidecars, writes
  # _sources/<version>.prose.json, and runs release-notes-render.py (pure stdlib, no
  # network) to render each page and then `--all` to rebuild the TOC/index, then
  # commits and opens the PR. Use python3 for offline rendering and the docs regression
  # tests, not the heavy generators (they already ran in the prepare job). Keep an
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
  # mkdir + jq assemble the multiline repository-template body into the JSON
  # request consumed by the safeoutputs CLI; GitHub writes still remain isolated
  # behind create-pull-request.
  bash: ["cat", "grep", "sort", "head", "tail", "git", "python3", "mkdir", "jq", "set", "read", "basename", "wc", "printf"]
  edit:
# The agent has no network: it only polishes prose from already-generated files.
network: {}
safe-outputs:
  create-pull-request:
    title-prefix: "[docs] "
    labels: [area/Docs, partner/agentic-workflows]
    draft: false
    base-branch: main
    allowed-base-branches: [main]
    allowed-files: ["documentation/docfx/releases/**"]
    allowed-branches: [bot/release-notes]
    fallback-as-issue: false
    preserve-branch-name: true
    recreate-ref: true
---

# Write release notes and open the docs PR

Follow [the release-notes skill](../../.agents/skills/release-notes/SKILL.md) to turn
the collected evidence into consumer-facing prose. Validate the pages, then write
the PR description and publish one documentation update.

## Inputs and checkout are ready

The host has restored generated API diffs, page facts, index data, and
`output/files-to-polish.txt`. It has checked out `bot/release-notes` with history
rooted at `origin/main`; the selected source's skill and tools remain in the working tree.

- Work offline. Do not run `prepare.sh`, Cake, `release-notes-data.py`, or
  `release-notes-index.py`, or fetch new evidence.
- Do not check out, create, reset, or choose a branch.
- Do not commit source-only tools outside `documentation/docfx/releases/`.

## Write and validate

Re-author every listed page using the skill's complete-body reading, consumer
selection, companion, scope, and factual-review rules. The list may be empty;
in that case proceed directly to the final render.

Validate each page as the skill describes. Then finalize with:

```bash
python3 scripts/infra/docs/release-notes-render.py --all
```

Correct reported prose errors and rerun until it succeeds. Optional exact-tag
`release_summaries` are not checked by this renderer; a separate workflow validates
and publishes them later. Do not publish GitHub Release bodies in this run.

## Commit and submit

Inspect the final release-tree diff. If it is empty, exit without a commit or PR,
even when source-only tools elsewhere differ from main.

1. Write the PR description yourself from the repository template and actual diff,
   following the skill. Preserve sections/checklists, omit complete instructional
   HTML comments, and report only observed validation commands and results.
2. Run `git add -- documentation/docfx/releases` to stage the prepared facts, prose,
   rendered pages, API diffs, and navigation changes together.
3. Run `git commit -m "docs: regenerate API diffs and release notes"` as a standalone
   shell call. The safe-output tool requires a commit; it does not stage or commit for you.
   Do not chain cleanup or other commands onto this call.
4. Call the registered native `create_pull_request` safe-output with base `main`
   and branch `bot/release-notes`. Its schema is already supplied; do not probe a
   shell CLI with `command -v`, `--help`, or other discovery commands.

Keep temporary PR payloads in `/tmp/gh-aw/`, outside the commit. Do not delete the
payload before submission. Commit once after validation; do not hand-edit generated
Markdown or manually push output outside the safe-output tool.
