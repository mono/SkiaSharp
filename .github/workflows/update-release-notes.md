---
description: "Regenerate website release notes AND API changelogs when code lands on main, release branches, or tags — one pipeline, one PR."
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
# Cake regenerates the full historical API-diff tree from the NuGet feed and
# Python regenerates every branch's raw data, so this is much longer than a
# pure-AI run.
timeout-minutes: 90
permissions:
  contents: read
# Python --all fetches the release/* refs it needs; full history is required so
# it can walk each branch's commits.
checkout:
  - fetch-depth: 0
# The deterministic Prepare phase (the generate.sh script) runs on the HOST before
# the agent so the AI never waits on — or pays for — the long script runs.
# Everything these steps write lands in the working tree and is captured into the
# same PR as the agent's prose (Polish) edits.
pre-agent-steps:
  - name: Start from a clean main tree
    run: |
      set -euo pipefail
      # Release notes/changelogs always target main. A push to release/* is a
      # content *source*, not the PR base, so rebase onto origin/main first to
      # avoid leaking release-branch-only files into the main-targeted PR.
      mkdir -p /tmp/gh-aw
      git fetch origin main --quiet
      git checkout -B main origin/main
  - name: Setup .NET
    uses: actions/setup-dotnet@67a3573c9a986a3f9c594539f4ab511d57bb3ce9 # v4.3.1
    with:
      global-json-file: global.json
  - name: Generate changelogs + release-notes raw data
    env:
      GH_TOKEN: ${{ github.token }}
      GITHUB_TOKEN: ${{ github.token }}
    run: |
      set -euo pipefail
      # Single entry point: runs Cake (API changelogs) then Python (raw data) in
      # order and prints the "Files to polish" list. Tee it so the agent can read
      # exactly which pages to polish. No args = full regeneration (Python --all).
      bash .agents/skills/release-notes/scripts/generate.sh \
        | tee /tmp/gh-aw/files-to-polish.txt
tools:
  # The agent only reads the generated files and rewrites prose — it must NOT run
  # the scripts (they already ran above). No python3 here on purpose.
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

# Update Release Notes & API Changelogs

This is the single pipeline that keeps the website release notes **and** the API
changelogs current — there is no separate api-diff workflow. It runs the
deterministic generators on the host, then uses the AI to polish prose, and opens
**one** pull request with everything.

## What already ran: the Prepare phase (do NOT re-run)

Before you (the agent) started, a host step already ran the skill's **Prepare**
phase — the single script `.agents/skills/release-notes/scripts/generate.sh`, which
in turn:

1. ran **Cake** (`docs-api-diff-past`) to regenerate the complete API-diff tree and
   `co-release-map.json` sidecar under `documentation/docfx/releases/`, then
2. ran **Python** (`generate-release-notes.py --all`) to regenerate every version
   page's raw-data block, write the deterministic page→API-diff links, and print the
   **"Files to polish"** list (teed to `/tmp/gh-aw/files-to-polish.txt`).

These changes are already on disk. **Do not run `generate.sh`, `dotnet cake`, the
Python script, or `git commit`/`git push`.** Your job is the Polish phase only.

## Your job: the Polish phase

Use the **release-notes skill**
([`.agents/skills/release-notes/SKILL.md`](../../.agents/skills/release-notes/SKILL.md))
in its **unattended** mode: the Prepare phase already ran, so skip it and go straight
to **Polish**.

1. Read `/tmp/gh-aw/files-to-polish.txt`. It lists exactly the pages under
   `documentation/docfx/releases/` whose raw data changed this run.
2. If it says **"(none — all files up to date)"** or is empty, make **no edits**
   and exit — leave any existing PR untouched. No PR will be created.
3. Otherwise, for **each** listed file, follow the skill: rewrite only the human
   prose, keeping every script-owned region verbatim — the raw-data HTML comment
   block, the `> **API changes**` link line, and the `## Links` entries. Never
   hand-author API-diff or HarfBuzz links, never touch `TOC.yml`/`index.md`, and
   never create, rename, or delete pages.

## How the PR is made

The `create-pull-request` safe-output captures **all** working-tree changes —
the Cake tree, the Python raw data, and your prose edits — into one PR targeting
`main` on the single `bot/release-notes` branch. You do not git commit or push.
If nothing changed (scripts produced no diff and you made no edits), no PR is
opened or updated.
