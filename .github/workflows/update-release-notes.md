---
description: "Regenerate website release notes AND API diffs when code lands on main, release branches, or tags — one pipeline, one PR."
on:
  push:
    branches: [main, "release/**"]
    tags: ["v*"]
    paths-ignore:
      - "documentation/docfx/releases/**"
  schedule:
    # Weekly safety net so a missed/failed push run still self-heals.
    - cron: "0 0 * * 0"
  workflow_dispatch:
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
      - name: Start from a clean main tree
        run: |
          set -euo pipefail
          # Release notes/api diffs always target main; a push to release/* is a
          # content *source*, not the PR base, so generate from origin/main.
          git fetch origin main --quiet
          git checkout -B main origin/main
      - name: Setup .NET
        uses: actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9 # v4.3.1
        with:
          global-json-file: global.json
      - name: Generate (verbose)
        env:
          GH_TOKEN: ${{ github.token }}
          GITHUB_TOKEN: ${{ github.token }}
        run: |
          set -euo pipefail
          # Single entry point: Cake (API diffs) then Python (raw data), both
          # VERBOSE. No args = --all; the "Files to polish" list lands at its
          # default location, output/files-to-polish.txt.
          bash .agents/skills/release-notes/scripts/generate.sh
      - name: Package Prepare output
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
  - name: Start from a clean main tree
    run: |
      set -euo pipefail
      mkdir -p /tmp/gh-aw
      git fetch origin main --quiet
      git checkout -B main origin/main
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
  # The agent only reads the restored files and rewrites prose — it must NOT run
  # the scripts (they already ran in the prepare job). No python3 here on purpose.
  bash: ["cat", "grep", "sort", "head", "tail", "sed", "awk", "git"]
  edit:
# The agent has no network: it only polishes prose from already-generated files.
network: {}
safe-outputs:
  create-pull-request:
    title-prefix: "[docs] "
    labels: [documentation]
    draft: false
    allowed-base-branches: [main]
    preserve-branch-name: true
    recreate-ref: true
---

# Update Release Notes & API Diffs

This is the single pipeline that keeps the website release notes **and** the API
api diffs current — there is no separate api-diff workflow. The deterministic
generators run in a dedicated **`prepare`** job, the agent then polishes prose,
and **one** pull request ships everything.

## What already ran: the Prepare phase (do NOT re-run)

Before you (the agent) started, a **separate `prepare` job** ran the skill's
**Prepare** phase on its own disk-managed runner — the single script
`.agents/skills/release-notes/scripts/generate.sh`, which in turn:

1. ran **Cake** (`docs-api-diff-past`) to regenerate the complete API-diff tree and
   `co-release-map.json` sidecar under `documentation/docfx/releases/`, then
2. ran **Python** (`generate-release-notes.py --all`) to regenerate every version
   page's raw-data block, write the deterministic page→API-diff links, and write
   the **"Files to polish"** list.

The `prepare` job uploaded its complete working-tree change as a patch plus that
list as an artifact, and a host step **already restored both** into this checkout:
the regenerated files are on disk, and the list is at
`output/files-to-polish.txt`. **Do not run `generate.sh`, `dotnet cake`, the
Python script, or `git commit`/`git push`.** Your job is the Polish phase only.

## Your job: the Polish phase

Use the **release-notes skill**
([`.agents/skills/release-notes/SKILL.md`](../../.agents/skills/release-notes/SKILL.md))
in its **unattended** mode: the Prepare phase already ran, so skip it and go straight
to **Polish**.

1. Read `output/files-to-polish.txt`. It lists exactly the pages under
   `documentation/docfx/releases/` whose raw data changed this run (one
   repo-relative path per line).
2. If it is **empty**, make **no edits** and exit — leave any existing PR
   untouched. No PR will be created.
3. Otherwise, for **each** listed file, follow the skill: rewrite only the human
   prose, keeping every script-owned region verbatim — the raw-data HTML comment
   block, the `> **API changes**` link line, and the `## Links` entries. Never
   hand-author API-diff or HarfBuzz links, never touch `TOC.yml`/`index.md`, and
   never create, rename, or delete pages.

## How the PR is made

The `create-pull-request` safe-output captures **all** working-tree changes —
the restored Prepare output (Cake tree + Python raw data) and your prose edits —
into one PR targeting `main` on the single `bot/release-notes` branch. You do not
git commit or push. If nothing changed (Prepare produced an empty patch and you
made no edits), no PR is opened or updated.
