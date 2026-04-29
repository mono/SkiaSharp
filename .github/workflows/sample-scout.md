---
description: "Run the sample-scout skill to discover Skia GM samples worth porting to the SkiaSharp Gallery."
on:
  schedule: weekly
  workflow_dispatch:
  skip-bots: [github-actions, copilot, dependabot]
concurrency:
  group: sample-scout
  cancel-in-progress: true
timeout-minutes: 30
checkout:
  submodules: recursive
  fetch-depth: 1
steps:
  - name: Prepare working directory
    run: |
      mkdir -p /tmp/gh-aw/agent
      touch /tmp/gh-aw/agent/step-summary.md
      rm -f /tmp/gh-aw/agent-step-summary.md
      ln -s /tmp/gh-aw/agent/step-summary.md /tmp/gh-aw/agent-step-summary.md
permissions:
  contents: read
  issues: read
tools:
  bash: ["python3", "cat", "grep", "find", "jq", "head", "tail", "wc", "sort", "sed", "ls", "cp", "mkdir", "echo", "xargs", "basename"]
network:
  allowed:
    - defaults
    - python
safe-outputs:
  mentions: false
  allowed-github-references: []
  max-bot-mentions: 1
  create-issue:
    title-prefix: "Sample Scout:"
    labels: [report, area/Gallery]
    close-older-issues: true
    expires: 30
---

# Weekly Sample Scout Report

Scan Skia GM samples from the submodule and publish a report of Gallery opportunities
as a GitHub issue using the `create_issue` safe output tool.

## Step 1 — Run the sample-scout skill

Read `.agents/skills/sample-scout/SKILL.md` and follow its instructions for a full scan.

The GM files are at `externals/skia/gm/*.cpp` and binding code is in `binding/SkiaSharp/` —
read them directly from the checkout, no remote fetching needed.

## Step 2 — Publish as GitHub issue

**Use the `create_issue` safe output tool** to publish the report. This is the primary deliverable.

Read `sample-scout-report.md` and pass it as the issue body. If larger than 65000 characters,
generate a condensed summary with the top opportunities instead.

The issue title **must** start with `Sample Scout:` followed by date and key metric
(e.g. `Sample Scout: 2025-01-15 (42 opportunities)`).

## Step 3 — Upload artifacts

```bash
cp sample-scout-report.json /tmp/gh-aw/agent/
cp sample-scout-report.md /tmp/gh-aw/agent/
cat sample-scout-report.md >> /tmp/gh-aw/agent/step-summary.md
```

Write to `/tmp/gh-aw/agent/step-summary.md` (symlinked to step summary).

## ⚠️ MANDATORY — Safe Output Required

**You MUST call the `create_issue` safe output tool before finishing.** If the full report
could not be generated, still call `create_issue` with a partial summary. Never exit without
calling at least one safe output tool.
