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

Run the **release-notes** skill in its automated `--all` flow. The skill is the
single source of truth — follow **"Automated workflow mode"** in
[`.agents/skills/release-notes/SKILL.md`](../../.agents/skills/release-notes/SKILL.md),
which lists the exact steps (prep main checkout → run `--all` → polish changed
files → open one PR). Do not restate or renumber those steps here.

This workflow only specifies the CI-specific values the skill defers to it:

- **PR branch:** single consolidated `bot/release-notes` (one PR at a time, not
  per-version).
- **Base branch:** `main` (release notes always live on main for the docs site).
- **No-op:** if the script reports "(none — all files up to date)", make no PR.
