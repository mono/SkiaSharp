---
description: "Update website release notes when code changes land on main, release branches, or tags are pushed."
on:
  push:
    branches: [main, "release/**"]
    tags: ["v*"]
    paths-ignore:
      - "documentation/docfx/releases/**"
  workflow_dispatch:
  skip-bots: [github-actions, copilot, dependabot]
concurrency:
  group: update-release-notes
  cancel-in-progress: true
timeout-minutes: 15
permissions:
  contents: read
tools:
  bash: ["python3", "git", "cat", "grep", "sort", "head", "tail", "sed", "awk"]
  edit:
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

# Update Release Notes

Automatically update website release notes when code changes land on `main`,
`release/*` branches, or when release tags are pushed.

Uses `--all` mode to regenerate all versions (idempotent), skipping unchanged
files. Only genuinely-changed files are polished by AI.

## Step 1 — Fetch and generate raw data

**Important:** The script uses `origin/` remote refs for all git history queries.
Always run from a `main` checkout to avoid leaking release-branch-specific files
(like `PREVIEW_LABEL`) into the PR targeting main.

Ensure all remote refs are fetched and the working tree is on `main`:

```bash
git fetch origin main --quiet
git checkout -B main origin/main
```

Run the release notes script in `--all` mode. This processes every branch
(main + all release/*), skips files that haven't changed (same PR count and
diff range), and only outputs files that need polishing.

```bash
python3 .agents/skills/release-notes/scripts/generate-release-notes.py --all
```

## Step 2 — Polish changed files

Use the **release-notes** skill (`.agents/skills/release-notes/SKILL.md`) to polish
any files listed in the "Files to polish" output from Step 1.

If the output says "(none — all files up to date)", skip this step entirely —
there's nothing to polish and no PR needed.

The skill handles: reading the template, writing polished content, and regenerating
the TOC.

## Step 3 — Create or update the pull request

Use a single consolidated branch `bot/release-notes` for all release notes updates.
This ensures only one PR exists at a time (instead of per-version branches).

The PR targets `main` — release notes always live on main for the docs site.
