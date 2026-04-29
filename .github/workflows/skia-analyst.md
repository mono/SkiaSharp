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
checkout:
  submodules: recursive
  fetch-depth: 1
steps:
  - name: Prepare external data
    env:
      GH_TOKEN: ${{ github.token }}
    run: |
      set -euo pipefail
      mkdir -p /tmp/gh-aw/agent
      touch /tmp/gh-aw/agent/step-summary.md
      rm -f /tmp/gh-aw/agent-step-summary.md
      ln -s /tmp/gh-aw/agent/step-summary.md /tmp/gh-aw/agent-step-summary.md

      # Pre-fetch upstream release notes (different repo, not in checkout)
      gh api repos/google/skia/contents/RELEASE_NOTES.md \
        -H "Accept: application/vnd.github.raw" > /tmp/gh-aw/agent/skia-release-notes.md
      echo "Release notes: $(wc -l < /tmp/gh-aw/agent/skia-release-notes.md) lines"
permissions:
  contents: read
  issues: read
tools:
  github:
    toolsets: [repos]
    allowed-repos: ["mono/skiasharp", "mono/skia", "google/skia"]
    min-integrity: none
  bash: ["python3", "cat", "grep", "find", "jq", "head", "tail", "wc", "sort", "sed", "cp", "mkdir", "echo"]
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

Run the skia-analyst skill and publish the results as a GitHub issue.

## Step 1 — Run the skia-analyst skill

Read and follow `.agents/skills/skia-analyst/SKILL.md` for a **full scan**.

The upstream Skia release notes have been pre-fetched to `/tmp/gh-aw/agent/skia-release-notes.md`.
The submodule is checked out so all headers and binding code are on disk.

## Step 2 — Publish as GitHub issue

**Use the `create_issue` safe output tool** to publish the report. This is the primary deliverable.

Read `skia-analyst-report.md` and pass it as the issue body. If larger than 65000 characters,
generate a condensed summary with top gaps and quick wins instead.

The issue title **must** start with `Skia Analyst:` followed by the milestone and date
(e.g. `Skia Analyst: m147 (2025-01-15)`).

## Step 3 — Upload artifacts

```bash
cp skia-analyst-report.json /tmp/gh-aw/agent/
cp skia-analyst-report.md /tmp/gh-aw/agent/
cat skia-analyst-report.md >> /tmp/gh-aw/agent/step-summary.md
```

Write to `/tmp/gh-aw/agent/step-summary.md` (symlinked to step summary).

## ⚠️ MANDATORY — Safe Output Required

**You MUST call the `create_issue` safe output tool before finishing.** If the full report
could not be generated, still call `create_issue` with a partial summary. Never exit without
calling at least one safe output tool.
