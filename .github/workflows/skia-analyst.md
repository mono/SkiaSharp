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

## Step 1 — Determine current milestone

The submodule is not checked out in the workflow environment. Fetch the milestone header
from `mono/skia` via the GitHub MCP tool:

```
Use the github tool to get file contents from mono/skia, path: include/core/SkMilestone.h
```

Extract the `SK_MILESTONE` number from the `#define SK_MILESTONE` line.

## Step 2 — Run the skia-analyst skill

Read and follow `.agents/skills/skia-analyst/SKILL.md` to run a **full scan**.

Key parameters:
- `scanMode`: full
- `currentMilestone`: from Step 1
- Fetch Skia release notes from `https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md`
- Check binding status by grepping `binding/SkiaSharp/SkiaApi.generated.cs` for P/Invoke externs
  and `binding/SkiaSharp/*.cs` for C# wrappers
- For hidden API scan, fetch upstream C++ headers from `google/skia` via the GitHub MCP tool

Save the JSON report to `skia-analyst-report.json` in the working directory.

## Step 3 — Validate the report

```bash
pip3 install -r .agents/skills/skia-analyst/scripts/requirements.txt --quiet
python3 .agents/skills/skia-analyst/scripts/validate-skia-analyst.py skia-analyst-report.json
```

If validation fails, fix the JSON and re-validate. Do not proceed until it passes.

## Step 4 — Render the Markdown

```bash
python3 .agents/skills/skia-analyst/scripts/render-skia-analyst.py skia-analyst-report.json skia-analyst-report.md
```

Read the rendered `skia-analyst-report.md`. If it exceeds ~60,000 characters, trim lower-priority
sections — the full JSON is preserved in the step summary as a fallback.

## Step 5 — Publish the report

Create a GitHub issue with the rendered Markdown as the body. The `create-issue` safe output will
automatically close any older issues with the same title prefix, so only the latest report is active.

The issue title **must** start with `Skia API Gap Analysis:` (e.g. `Skia API Gap Analysis: m147 — 2025-01-15`)
so `close-older-issues` matches correctly.

## Step 6 — Write step summary

```bash
mkdir -p /tmp/gh-aw/agent
cp skia-analyst-report.json /tmp/gh-aw/agent/
cp skia-analyst-report.md /tmp/gh-aw/agent/
cat skia-analyst-report.md >> /tmp/gh-aw/agent/step-summary.md
```
