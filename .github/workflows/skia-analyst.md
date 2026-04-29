---
description: "Run the skia-analyst skill to produce a Skia feature analysis report as a GitHub issue."
on:
  schedule: daily
  workflow_dispatch:
  skip-bots: [github-actions, copilot, dependabot]
concurrency:
  group: skia-analyst
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
    title-prefix: "Skia Analyst:"
    labels: [report, area/SkiaSharp]
    close-older-issues: true
    expires: 30
---

# Daily Skia Analyst Report

Run a full skia-analyst scan and publish the results as a GitHub issue.

## Step 1 — Run the skia-analyst skill

Read and follow the instructions in `.agents/skills/skia-analyst/SKILL.md` to run a **full scan**.

Complete all phases (Setup → Agents → Synthesize → Generate Outputs). Use `scanMode: full`.

After Phase 4 completes, the outputs will be:
- `skia-analyst-report.json` — the validated JSON report
- `skia-analyst-report.md` — the rendered Markdown

## Step 2 — Publish as GitHub issue

Create a GitHub issue with the contents of `skia-analyst-report.md` as the body.

The issue title **must** start with `Skia Analyst:` followed by the milestone and date
(e.g. `Skia Analyst: m147 (2025-01-15)`) so `close-older-issues` matches correctly.

## Step 3 — Upload artifacts and write step summary

```bash
cp skia-analyst-report.json /tmp/gh-aw/agent/
cp skia-analyst-report.md /tmp/gh-aw/agent/
cat skia-analyst-report.md >> /tmp/gh-aw/agent/step-summary.md
```

**IMPORTANT:** Write to the literal path `/tmp/gh-aw/agent/step-summary.md` — this file is symlinked
to the step summary. Do NOT use `$GITHUB_STEP_SUMMARY` as it resolves to an inaccessible path.
