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

Release notes are generated from `origin/` refs, so start from a clean `main`
checkout. This avoids leaking release-branch-only files (e.g. `PREVIEW_LABEL`)
into the main-targeted PR. The skill's `--all` script fetches the `release/*`
branches it needs itself, so this only has to put you on `main`:

```bash
git fetch origin main --quiet
git checkout -B main origin/main
```

## Do the work

Use the **release-notes skill** to regenerate and polish, running it in `--all`
mode (regenerates every branch idempotently and lists only changed files). Edit
exactly the files it lists under "Files to polish". If it reports
"(none — all files up to date)", **make no file edits** — leave any existing
`bot/release-notes` PR untouched and exit.

## Automation

The PR is created by the `create-pull-request` safe-output from whatever files
you edited — you do **not** git commit or push manually. It targets `main` on the
single consolidated branch `bot/release-notes`, so there is at most one open
release-notes PR rather than one-per-version. If you made no edits, no PR is
created or updated.
