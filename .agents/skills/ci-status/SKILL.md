---
name: ci-status
description: >
  Check the CI build health of SkiaSharp across main and recent release branches.
  Collects the last N builds from the 3-pipeline chain (Native → Managed → Tests)
  for main and the most recent release/* branches, providing a daily dashboard view.

  Use when user asks to:
  - Check overall CI health
  - See build status across branches
  - Get a daily CI summary
  - Check if main is green
  - See recent build failures
  - Monitor branch health

  Triggers: "CI status", "build health", "is main green", "CI dashboard",
  "daily build status", "check builds", "are builds passing", "CI overview",
  "pipeline health", "branch build status".
---

# CI Status Skill

Provide a dashboard view of SkiaSharp CI health across main and recent release branches.

Unlike the `release-status` skill (which tracks a single release through the pipeline chain),
this skill gives a **broad overview** of CI health across multiple branches simultaneously.

## Pipelines Tracked

### Public CI (xamarin/public org — triggers on push/PR to main, develop, release/*)

| Pipeline Name | Org/Project | Definition ID | URL |
|---------------|-------------|---------------|-----|
| `SkiaSharp (Public)` | xamarin/public | 4 | [link](https://dev.azure.com/xamarin/public/_build?definitionId=4) |

### Internal Release Chain (devdiv/DevDiv org — triggers on release/* branches)

| Order | Pipeline Name | Definition ID | URL |
|-------|---------------|---------------|-----|
| 1 | `SkiaSharp-Native` | 26493 | [link](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=26493) |
| 2 | `SkiaSharp` | 10789 | [link](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=10789) |
| 3 | `SkiaSharp-Tests` | 15756 | [link](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=15756) |

---

## Step 1: Run the Status Script

```bash
python3 .agents/skills/ci-status/scripts/ci-status.py
```

### Options

| Flag | Default | Description |
|------|---------|-------------|
| `--branches N` | 3 | Number of most recent release/* branches to include |
| `--builds N` | 5 | Number of recent builds to show per pipeline per branch |
| `--no-issues` | off | Skip fetching errors/warnings (faster) |
| `--output PATH` | none | Write a formatted markdown report to the given file path |

Examples:

```bash
# Default: main + 3 recent release branches, 5 builds each
python3 .agents/skills/ci-status/scripts/ci-status.py

# More branches, fewer builds
python3 .agents/skills/ci-status/scripts/ci-status.py --branches 5 --builds 3

# Just main with last 10 builds
python3 .agents/skills/ci-status/scripts/ci-status.py --branches 0 --builds 10

# Generate a markdown report
python3 .agents/skills/ci-status/scripts/ci-status.py --output output/ai/ci-status.md

# Quick check without issue extraction + markdown
python3 .agents/skills/ci-status/scripts/ci-status.py --no-issues --output /tmp/report.md
```

---

## Step 2: Interpret Results

The script outputs:
1. **Per-branch breakdown** — last N builds for each pipeline on each branch
2. **Health summary** — one-line status per branch showing latest build from each pipeline

### Status Icons

| Icon | Meaning |
|------|---------|
| ✅ | Succeeded |
| ⚠️ | Partially succeeded (some platforms had warnings) |
| ❌ | Failed |
| 🚫 | Canceled |
| 🔄 | In progress |
| ⏳ | Not started / not triggered |

---

## Step 3: Report to User

Present a concise summary focusing on:

1. **Is main green?** — Are the most recent builds on main all passing?
2. **Any failing branches?** — Highlight branches with failures
3. **Trends** — Are recent builds improving or degrading?

Example report:

```
SkiaSharp CI Health — 2026-05-28 13:45 UTC

📊 Health Summary:
  main                    ✅ Native | ✅ Managed | ✅ Tests
  release/4.147.0-pre.3   ✅ Native | ✅ Managed | ⚠️ Tests
  release/4.147.0-pre.2   ✅ Native | ✅ Managed | ✅ Tests
  release/3.119.4          ✅ Native | ✅ Managed | ✅ Tests

Status: main is green ✅. release/4.147.0-preview.3 has test warnings.
```

If there are failures, include the ADO build link:
```
https://devdiv.visualstudio.com/DevDiv/_build/results?buildId={id}
```

---

## When to Use This vs release-status

| Question | Use |
|----------|-----|
| "Is main green?" | **ci-status** |
| "How's the release/3.119.4 build doing?" | **release-status** |
| "Daily CI check" | **ci-status** |
| "Are packages ready for release X?" | **release-status** |
| "Any CI failures across the board?" | **ci-status** |
| "Trace the pipeline chain for branch X" | **release-status** |

---

## Scheduling as a Daily Workflow

This skill works well as a daily scheduled workflow:

```
Prompt: "Run the ci-status skill and report the health of main and recent release branches"
Schedule: Daily at 9:00 AM
```
