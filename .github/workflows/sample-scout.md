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
steps:
  - name: Redirect step summary into agent-writable directory
    run: |
      mkdir -p /tmp/gh-aw/agent
      touch /tmp/gh-aw/agent/step-summary.md
      rm -f /tmp/gh-aw/agent-step-summary.md
      ln -s /tmp/gh-aw/agent/step-summary.md /tmp/gh-aw/agent-step-summary.md
permissions:
  contents: read
  issues: read
tools:
  github:
    toolsets: [repos, issues]
    allowed-repos: ["mono/skiasharp", "mono/skia", "google/skia"]
    min-integrity: none
  bash: ["python3", "pip3", "gh", "git", "jq", "cat", "grep", "find", "sed", "sort", "head", "tail", "wc", "cp", "mkdir", "echo"]
network:
  allowed:
    - defaults
    - python
    - github
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

Scan all upstream Skia GM samples and publish a report of Gallery opportunities as a GitHub issue.

## Step 1 — Run the sample-scout skill

Read and follow the instructions in `.agents/skills/sample-scout/SKILL.md` to run a full scan.

Complete all phases (Setup → Analyze → Cross-Reference → Validate & Render).
Analyze all GM samples from `google/skia/gm/` — no filtering by milestone, always scan everything.

After Phase 4 completes, the outputs will be:
- `sample-scout-report.json` — the validated JSON report
- `sample-scout-report.md` — the rendered Markdown

## Step 2 — Publish as GitHub issue

Create a GitHub issue with the contents of `sample-scout-report.md` as the body.

The issue title **must** start with `Sample Scout:` followed by date and key metric
(e.g. `Sample Scout: 2025-01-15 (42 opportunities)`) so `close-older-issues` matches correctly.

## Step 3 — Upload artifacts and write step summary

```bash
cp sample-scout-report.json /tmp/gh-aw/agent/
cp sample-scout-report.md /tmp/gh-aw/agent/
cat sample-scout-report.md >> /tmp/gh-aw/agent/step-summary.md
```

**IMPORTANT:** Write to the literal path `/tmp/gh-aw/agent/step-summary.md` — this file is symlinked
to the step summary. Do NOT use `$GITHUB_STEP_SUMMARY` as it resolves to an inaccessible path.
