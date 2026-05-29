---
name: ci-status
description: >
  Check the CI build health of SkiaSharp across main and recent release branches.
  Collects the last N builds from the pipeline chain (Public + Native → Managed → Tests)
  for main and the most recent release/* branches, providing a daily dashboard view
  with AI-powered analysis of failures, regressions, and flakes.

  Use when user asks to:
  - Check overall CI health
  - See build status across branches
  - Get a daily CI summary
  - Check if main is green
  - See recent build failures
  - Monitor branch health
  - Identify flaky tests or chronic failures
  - Determine if a release branch is shippable

  Triggers: "CI status", "build health", "is main green", "CI dashboard",
  "daily build status", "check builds", "are builds passing", "CI overview",
  "pipeline health", "branch build status", "CI report", "why is CI red".
---

# CI Status Skill

Provide a dashboard view of SkiaSharp CI health across main and recent release branches,
with AI-powered analysis to identify patterns, regressions, and actionable fixes.

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

## Step 1: Collect Data

Run the collector script to gather build status, issues, and commit info:

```bash
python3 .agents/skills/ci-status/scripts/ci-status.py \
  --output output/ai/ci-status-report.md \
  --json output/ai/ci-status-data.json
```

### Options

| Flag | Default | Description |
|------|---------|-------------|
| `--branches N` | 3 | Number of most recent release/* branches to include |
| `--builds N` | 5 | Number of recent builds to show per pipeline per branch |
| `--no-issues` | off | Skip fetching errors/warnings (faster, less detail) |
| `--output PATH` | none | Write formatted markdown report |
| `--json PATH` | none | Write raw structured JSON (for AI analysis) |

### Examples

```bash
# Full report with all data
python3 .agents/skills/ci-status/scripts/ci-status.py --output output/ai/ci-status-report.md --json output/ai/ci-status-data.json

# Quick check (no timeline fetch)
python3 .agents/skills/ci-status/scripts/ci-status.py --no-issues

# Deep analysis window (10 builds, 5 branches)
python3 .agents/skills/ci-status/scripts/ci-status.py --branches 5 --builds 10 --output output/ai/ci-status-report.md --json output/ai/ci-status-data.json
```

---

## Step 2: AI Analysis

After the script runs, read the JSON data (`output/ai/ci-status-data.json`) and perform the following analysis. **All claims must reference actual build IDs and URLs from the data.**

### 2.1 Executive Summary (Verdict)

Classify overall health as one of:
- 🟢 **Healthy** — all branches green or only warnings
- 🟡 **Degraded** — some failures but main is green
- 🔴 **Broken** — main is red or a release branch is blocked

Write 1-2 sentences: what's broken, since when, and the top action.

### 2.2 Root Cause Clustering

Group all errors/warnings across all branches and pipelines by **normalized signature**:
1. Strip volatile fragments (build IDs, timestamps, GUIDs, file paths with random hashes, agent names)
2. Cluster on `(task_name, normalized_first_error_line)`
3. For each cluster, determine:
   - **Category**: one of `code regression`, `flake`, `infra/network`, `quota/resource`, `chain blockage`, `unknown`
   - **Footprint**: which branches × pipelines are affected
   - **First/last seen** within the window
   - **Sample error** (verbatim from data)

#### Classification Decision Tree

| Signal | Category |
|--------|----------|
| Message contains `network`, `timeout`, `EOF`, `connection`, `nuget.org`, `429`, `503` | infra/network |
| Message contains `No space left`, `OOM`, `killed`, `agent lost`, `pool` | quota/resource |
| Same build passes/fails with no code change (pass/fail/pass pattern) | flake |
| Failure appears on multiple unrelated branches simultaneously | infra (not code) |
| Failure appears at a green→red transition on one branch only | code regression |
| Downstream pipeline failed but upstream in same chain also failed | chain blockage (don't double-count) |
| None of the above | unknown |

### 2.3 Pipeline Chain Analysis (MANDATORY — always perform, even when failures look independent)

The Internal chain is sequential: **Native → Managed → Tests**. A red pipeline is NOT automatically an independent failure — it is often a downstream casualty of an upstream break.

For **every** branch that has ≥1 red internal pipeline, you MUST:
1. Find the earliest-in-chain failing pipeline (Native before Managed before Tests).
2. Decide whether each later red pipeline is an **independent failure** (its own distinct error) or a **cascade** (failed because the upstream artifact never built / a shared error).
3. **Collapse cascades** into the upstream root cause so a single break is not counted as 2–3 problems.

Emit one explicit sentence per affected branch, even if the answer is "no cascade":
- Cascade: `"release/X: 3 red internal pipelines — root-caused to {Native}; Managed+Tests were blocked downstream, not independently broken."`
- Independent: `"release/X: Native and Tests both red but with unrelated errors ({errA} vs {errB}) — two independent failures, not a cascade."`

⚠️ Do not skip this step or list internal pipelines as separate equal-weight failures without first stating the chain verdict. This is the most common analysis miss.

### 2.4 Regression Detection

For each branch × pipeline, find green→red transitions (the script pre-computes these in `regression` fields):
- Report the last green build and the first red build
- List the associated commits from `changes` — these are the regression suspects
- Provide link to the first red build for investigation

### 2.5 Flake Detection

Look for alternating pass/fail patterns within a single branch × pipeline:
- ✅ ❌ ✅ or ❌ ✅ ❌ pattern = likely flake
- Same error appearing intermittently (present in some builds, absent in others with no code change)

### 2.6 Cross-Branch Correlation

| Pattern | Interpretation |
|---------|---------------|
| Same error on ≥2 unrelated branches | Infrastructure issue (toolchain, agent pool, NuGet) |
| Error on exactly one branch | Code regression specific to that branch |
| Same error on release/* and main | Shared code issue (or infra) |
| Error only on release/X.Y.x but not release/X.Y.Z | Servicing-specific backport issue |

### 2.7 Release Risk Assessment

For each `release/*` branch:
- Is it shippable right now? (yes / no / blocked)
- Days since last full green chain
- Blockers (link to root cause clusters)
- Recommendation: `ship`, `wait`, `cherry-pick fix`, `investigate`

### 2.8 Top Recommendations

Provide **at most 5** prioritized actions, ordered by:
1. Blocks a release → highest priority
2. Breaks main → high
3. Chronic failure → medium
4. Flake → low
5. Warning → informational

Each recommendation should include:
- What to do (imperative sentence)
- Why (which branch/pipeline is affected)
- Link to the relevant build

---

## Step 3: Present to User

After analysis, present:

1. **Verdict** (1 sentence + emoji)
2. **Health matrix** (the table from the markdown report)
3. **Chain verdict** — for each branch with red internal pipelines, the one-line cascade-vs-independent statement from Step 2.3 (so the reader knows whether N red pipelines = N problems or 1 root cause)
4. **Top 3-5 actions** with build links (root-caused — never list a cascaded downstream pipeline as its own action)
5. **Offer follow-ups**: "Want me to investigate the regression on release/X? Open the failing build? Check if this is a known issue?"

### Example Output

```
🟡 CI is degraded — release/3.119.x is blocked by a Guardian TSA upload failure; main is green.

📊 Health:
  main                         ✅ Public | ✅ Native | ✅ Managed | ✅ Tests
  release/4.147.0-preview.3    ❌ Public | ⚠️ Native | ✅ Managed | ✅ Tests
  release/3.119.x              ❌ Public | ⚠️ Native | ⚠️ Managed | ❌ Tests

Top actions:
1. [release/3.119.x] Fix Guardian TSA upload — blocks Tests pipeline → build 14177772
2. [release/4.147.0-preview.3] Public CI failure — investigate CS0016 errors → build 157985
3. [infra] SkiaSharp-Native partial success on all release branches — Guardian warnings (non-blocking)

Full report: output/ai/ci-status-report.md
```

---

## Step 4: Follow-Up Investigations

If asked to dig deeper:
- Use the `hlx-azdo_*` tools to fetch specific build timelines, test results, or logs
- Use `hlx-azdo_build_analysis` for known-issue matching
- Use `hlx-azdo_test_results` to get specific failing test names
- Check if failures correlate with specific platforms (look at job names in timeline)
- For flakes, recommend looking at the last 20 builds to confirm the pattern

---

## When to Use This vs Other Skills

| Question | Use |
|----------|-----|
| "Is main green?" | **ci-status** |
| "How's the release/3.119.4 build doing?" | **release-status** |
| "Daily CI check" | **ci-status** |
| "Are packages ready for release X?" | **release-status** |
| "Any CI failures across the board?" | **ci-status** |
| "Trace the pipeline chain for branch X" | **release-status** |
| "Why is CI red?" | **ci-status** (with analysis) |
| "Is release/X shippable?" | **ci-status** (risk assessment) |

---

## Scheduling as a Daily Workflow

This skill works well as a daily scheduled workflow:

```
Prompt: "Run the ci-status skill and report the health of main and recent release branches. Generate a full report with AI analysis."
Schedule: Daily at 9:00 AM
```
