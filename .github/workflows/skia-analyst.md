---
description: "Run the skia-analyst skill to produce a Skia API gap analysis report as a GitHub issue."
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
tools:
  github:
    toolsets: [issues]
    allowed-repos: ["mono/skiasharp", "mono/skia", "google/skia"]
    min-integrity: none
  bash: ["python3", "pip3", "gh", "git", "cat", "grep", "sort", "head", "tail", "cp", "mkdir", "echo", "sed"]
network:
  allowed:
    - defaults
    - python
    - raw.githubusercontent.com
safe-outputs:
  mentions: false
  allowed-github-references: []
  max-bot-mentions: 0
  create-issue:
    title-prefix: "Skia API Gap Analysis:"
    labels: [report, area/SkiaSharp]
    close-older-issues: true
    expires: 30
---

# Daily Skia Analyst Report

Run a full skia-analyst scan and publish the results as a GitHub issue.

## Step 1 — Run the skia-analyst skill

Read and follow the instructions in `.agents/skills/skia-analyst/SKILL.md` to run a **full scan**.

Complete all phases. Use `scanMode: full`. The skill will determine the current milestone,
fetch upstream Skia release notes, check binding status, and produce a validated JSON report.

After all phases complete, the report JSON will be at `skia-analyst-report.json`.

## Step 2 — Render the Markdown and publish

Render the report as GitHub-flavored Markdown:

```bash
python3 .agents/skills/skia-analyst/scripts/render-skia-analyst.py skia-analyst-report.json skia-analyst-report.md
```

Create a GitHub issue with the rendered Markdown as the body. The issue title **must** start with
`Skia API Gap Analysis:` (e.g. `Skia API Gap Analysis: m147 — 2025-01-15`) so `close-older-issues`
matches correctly. If the body exceeds ~60,000 characters, trim lower-priority sections.

## Step 3 — Upload artifacts and write step summary

```bash
cp skia-analyst-report.json /tmp/gh-aw/agent/
cp skia-analyst-report.md /tmp/gh-aw/agent/
cat skia-analyst-report.md >> /tmp/gh-aw/agent/step-summary.md
```

**IMPORTANT:** Write to the literal path `/tmp/gh-aw/agent/step-summary.md` — this file is symlinked
to the step summary. Do NOT use `$GITHUB_STEP_SUMMARY` as it resolves to an inaccessible path.
