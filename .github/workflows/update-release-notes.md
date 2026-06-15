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

Automation that keeps the website release notes current. This file owns only the
**prep and automation**. The actual work — running the script and polishing the
pages — is the **release-notes skill**
([`.agents/skills/release-notes/SKILL.md`](../../.agents/skills/release-notes/SKILL.md)).
Do not restate the skill's steps here.

## Prep

Release notes are generated from `origin/` refs, so run from a clean `main`
checkout. This avoids leaking release-branch-only files (e.g. `PREVIEW_LABEL`)
into the main-targeted PR:

```bash
git fetch origin main --quiet
git checkout -B main origin/main
```

## Do the work

Use the **release-notes skill** to regenerate and polish, running it in `--all`
mode (regenerates every branch idempotently and lists only changed files). Polish
exactly the files it reports. If it reports "(none — all files up to date)",
stop — make no PR.

## Automation

Open a single consolidated pull request on branch `bot/release-notes` targeting
`main`, so there is at most one open release-notes PR rather than one-per-version.
