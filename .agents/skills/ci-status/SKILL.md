---
name: ci-status
description: >
  Check the CI build health and automation status of SkiaSharp across main and
  recent release branches. Collects the last N builds from the AzDO pipeline chain
  (Public + Native → Managed → Tests) and all GitHub Actions workflows from
  mono/SkiaSharp and mono/SkiaSharp-API-docs, providing a daily dashboard view
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
  - Check GitHub Actions workflow health
  - See what automation is failing

  Triggers: "CI status", "build health", "is main green", "CI dashboard",
  "daily build status", "check builds", "are builds passing", "CI overview",
  "pipeline health", "branch build status", "CI report", "why is CI red",
  "what automation is failing", "GitHub Actions status".
---

# CI Status Skill

Provide a dashboard view of SkiaSharp CI health across main and recent release branches,
with AI-powered analysis to identify patterns, regressions, and actionable fixes.

Unlike the `release-status` skill (which tracks a single release through the pipeline chain),
this skill gives a **broad overview** of CI health across multiple branches simultaneously.

## Pipelines Tracked

### Prerequisites

The collector script requires:
- **`az` CLI** — authenticated with access to `xamarin/public` and `devdiv/DevDiv` orgs
- **`gh` CLI** — authenticated with read access to `mono/SkiaSharp` and `mono/SkiaSharp-API-docs`
- **Git remotes** — fetched recently so `git branch -r` returns up-to-date release branches

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

### GitHub Actions (mono/SkiaSharp and mono/SkiaSharp-API-docs)

| Workflow | Repository | Trigger | Why Track |
|----------|------------|---------|-----------|
| Pages - Deploy | mono/SkiaSharp | Push/PR to main | Docs site broken if failing |
| Pages - Go Live! | mono/SkiaSharp | Workflow dispatch | Docs don't publish if failing |
| Pages - PR Staging - Cleanup | mono/SkiaSharp | PR close events | Stale staging deploys accumulate |
| Pages - PR Staging - Sweep Stale | mono/SkiaSharp | Daily (06:00 UTC) | Stale staging deploys accumulate |
| Sync - Samples | mono/SkiaSharp | Push/PR to `samples/` | Sample projects broken if failing |
| API Diff | mono/SkiaSharp | Weekly (Sun 00:00 UTC) | API regression detection |
| Sync - Docs Submodule | mono/SkiaSharp | Daily (10:00 UTC) | API docs get out of sync |
| Sync - Release Notes & API Diffs | mono/SkiaSharp | Push to main/release/tags | Release notes stop auto-updating |
| Sync - Skia Upstream | mono/SkiaSharp | Daily (07:00 UTC) | Upstream tracking breaks |
| Nightly Fix Finder | mono/SkiaSharp | Nightly | Nightly automation health |
| Sync - Issue Triage | mono/SkiaSharp | Daily (04:05 UTC) + issue events | Triage automation stops |
| Sync - Agentic Data | mono/SkiaSharp | Push to main | AI workflow data lost |
| PR - Backport | mono/SkiaSharp | PR label/comment | Cherry-picks to release branches fail |
| PR - Rebase | mono/SkiaSharp | PR comment | PR rebase automation broken |
| PR - Artifacts Comment | mono/SkiaSharp | Workflow run events | Build links not posted to PRs |
| Auto API Docs Writer | mono/SkiaSharp-API-docs | Scheduled/dispatch | XML docs stop being written |
| Automerge Docs | mono/SkiaSharp-API-docs | PR events | Doc PRs won't auto-merge |
| Go Live | mono/SkiaSharp-API-docs | Workflow dispatch | Docs don't publish to live |

---

## Step 1: Collect Raw Data

Run the collector script to gather build status, issues, and commit info:

```bash
python3 .agents/skills/ci-status/scripts/ci-status.py \
  --json output/ai/ci-status-data.json
```

This produces `ci-status-data.json` containing raw pipeline runs, GitHub Actions statuses,
regression markers, issues/errors, and associated commits. The script also prints a console
summary for quick visual inspection.

### Options

| Flag | Default | Description |
|------|---------|-------------|
| `--branches N` | 3 | Number of most recent release/* branches to include |
| `--builds N` | 5 | Number of recent builds to show per pipeline per branch |
| `--no-issues` | off | Skip fetching errors/warnings (faster, less detail) |
| `--json PATH` | none | Write raw structured JSON (for AI analysis) |

### Examples

```bash
# Standard collection
python3 .agents/skills/ci-status/scripts/ci-status.py --json output/ai/ci-status-data.json

# Quick check (no timeline fetch)
python3 .agents/skills/ci-status/scripts/ci-status.py --no-issues

# Deep analysis window (10 builds, 5 branches)
python3 .agents/skills/ci-status/scripts/ci-status.py --branches 5 --builds 10 --json output/ai/ci-status-data.json
```

### File Boundary

| File | Produced By | Consumed By |
|------|-------------|-------------|
| `output/ai/ci-status-data.json` | Collector script (Step 1) | AI analysis only (Step 2) |
| `output/ai/ci-status-YYYY-MM-DD.json` | AI (Step 3) | Validator + Renderers (Steps 4–5) |

> ⚠️ **Never pass `ci-status-data.json` to validate or render scripts.** Those scripts expect the
> augmented report JSON that the AI assembles in Step 3.

---

## Step 2: AI Analysis

After the script runs, read the JSON data (`output/ai/ci-status-data.json`) and perform the following analysis. **All claims must reference actual build IDs and URLs from the data.**

### 2.1 Executive Summary (Verdict)

Classify overall health as one of:
- 🟢 **Healthy** — all branches green or only warnings (both AzDO and GitHub Actions)
- 🟡 **Degraded** — some failures but main is green
- 🔴 **Broken** — main is red or a release branch is blocked

Write 1-2 sentences: what's broken, since when, and the top action.

### 2.2 Root Cause Clustering

Group all errors/warnings across all branches and pipelines by **normalized signature**:
1. Strip volatile fragments (build IDs, timestamps, GUIDs, file paths with random hashes, agent names)
2. Cluster on `(task_name, normalized_first_error_line)`
3. For each cluster, determine:
   - **Category**: one of `code_regression`, `flake`, `infra_network`, `quota_resource`, `chain_blockage`, `unknown`
   - **Footprint**: which branches × pipelines are affected
   - **First/last seen** within the window
   - **Sample error** (verbatim from data)

> ⚠️ **Important:** Finalize root-cause clusters only AFTER performing the Pipeline Chain Analysis (§2.3). Downstream cascade failures must be collapsed into the upstream root cause, not counted as independent clusters.

#### Classification Decision Tree

| Signal | Category |
|--------|----------|
| Message contains `network`, `timeout`, `EOF`, `connection`, `nuget.org`, `429`, `503` | infra_network |
| Message contains `No space left`, `OOM`, `killed`, `agent lost`, `pool` | quota_resource |
| Same build passes/fails with no code change (pass/fail/pass pattern) | flake |
| Failure appears on multiple unrelated branches simultaneously | infra_network |
| Failure appears at a green→red transition on one branch only | code_regression |
| Downstream pipeline failed but upstream in same chain also failed | chain_blockage |
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
- Recommendation: `ship`, `wait`, `cherry-pick`, `investigate`

### 2.8 GitHub Actions Health

For each tracked GitHub Actions workflow:
- Current status (pass/fail/skipped/no recent runs)
- Any failures or patterns (recurring failures, recent regressions)
- Whether failures are related to AzDO failures (same commit?) or independent
- Categorize by severity:
  - **High**: Pages - Deploy, Sync - Samples, Sync - Release Notes & API Diffs, Sync - Skia Upstream,
    Auto API Docs Writer (broken = user-facing impact or release process blocked)
  - **Medium**: Sync - Docs Submodule, Nightly Fix Finder, Sync - Issue Triage,
    PR - Backport, Pages - Go Live! (broken = automation degraded, manual workaround exists)
  - **Low**: Pages - PR Staging - Cleanup, Pages - PR Staging - Sweep Stale, PR - Rebase, PR - Artifacts Comment,
    Sync - Agentic Data (broken = cosmetic/housekeeping)

GitHub Actions failures don't block releases directly (AzDO owns that), but they
indicate broken automation that accumulates tech debt if ignored. Flag all failures
as actionable — the goal is "what automation is failing" not just "can we release".

### 2.9 Top Recommendations

Provide **at most 5** prioritized actions, ordered by:
1. Blocks a release → highest priority
2. Breaks main → high
3. High-severity GitHub Actions failure → high
4. Chronic failure → medium
5. Medium-severity GitHub Actions failure → medium
6. Flake → low
7. Warning / low-severity GitHub Actions → informational

Each recommendation should include:
- What to do (imperative sentence)
- Why (which branch/pipeline/workflow is affected)
- Link to the relevant build or run

---

## Step 3: Assemble Report JSON

After completing the analysis, assemble a **single JSON file** that combines the raw data
with your analysis. This JSON is the source of truth for rendering.

Write the JSON to: `output/ai/ci-status-YYYY-MM-DD.json`

The schema is documented in `references/report-schema.md`. Top-level keys:

```json
{
  "meta": { "date", "timestamp", "schemaVersion": "1.0", "window", "branches" },
  "verdict": { "status", "emoji", "summary" },
  "azdoHealth": { "branches": [...], "regressions": [...] },
  "chainAnalysis": [ { "branch", "verdict", "summary", "rootPipeline", "cascadedPipelines" } ],
  "rootCauses": [ { "id", "title", "category", "severity", "footprint", "sampleError", ... } ],
  "githubActions": { "workflows": [...], "summary": { "total", "healthy", "failing", ... } },
  "flakes": [ { "branch", "pipeline", "pattern", "confidence", "description" } ],
  "releaseRisk": [ { "branch", "shippable", "daysSinceGreen", "blockers", "recommendation" } ],
  "recommendations": [ { "priority", "severity", "action", "reason", "target", "buildUrl" } ]
}
```

**Rules for assembling the JSON:**
1. The `azdoHealth` section contains the raw pipeline data from `ci-status-data.json`, restructured to match the schema
2. The `githubActions` section combines raw workflow data with your severity/status classifications
3. All other sections (`verdict`, `chainAnalysis`, `rootCauses`, `flakes`, `releaseRisk`, `recommendations`) are your AI analysis
4. Every `buildUrl` and `buildEvidence` must reference real builds from the collected data
5. Maximum 5 recommendations, sorted by priority

---

## Step 4: Validate Report

Run the validator to check the assembled JSON:

```bash
python3 .agents/skills/ci-status/scripts/validate-ci-status.py output/ai/ci-status-YYYY-MM-DD.json
```

If validation fails, fix the JSON and re-validate. Common issues:
- Missing required keys
- Invalid enum values (e.g., verdict.status must be healthy/degraded/broken)
- rootCauseId references that don't exist
- recommendations not sorted by priority

---

## Step 5: Render Reports

Generate HTML and Markdown reports from the validated JSON:

```bash
# HTML dashboard (self-contained, opens in browser)
python3 .agents/skills/ci-status/scripts/render-ci-status.py output/ai/ci-status-YYYY-MM-DD.json

# Markdown report (for AI consumption and downstream agents)
python3 .agents/skills/ci-status/scripts/render-ci-status-md.py output/ai/ci-status-YYYY-MM-DD.json
```

This produces:
- `output/ai/ci-status-YYYY-MM-DD.html` — Bootstrap 5 dashboard viewable in any browser
- `output/ai/ci-status-YYYY-MM-DD.md` — Comprehensive markdown for AI agents

---

## Step 6: Present to User

After rendering, present a brief summary in chat and point to the files:

1. **Verdict** (1 sentence + emoji)
2. **AzDO health matrix** (compact table)
3. **Chain verdict** — one-line cascade-vs-independent statement per affected branch
4. **GitHub Actions health** — one-line summary per workflow, grouped by severity
5. **Top 3-5 actions** with build links
6. **File locations** — point to the JSON, HTML, and MD files
7. **Offer follow-ups**: "Want me to investigate the regression on release/X? Open the failing build?"

### Example Output

```
🟡 CI is degraded — release/3.119.x is blocked by a Guardian TSA upload failure; main is green.

📊 AzDO Health:
  main                         ✅ Public | ✅ Native | ✅ Managed | ✅ Tests
  release/4.147.0-preview.3    ❌ Public | ⚠️ Native | ✅ Managed | ✅ Tests
  release/3.119.x              ❌ Public | ⚠️ Native | ⚠️ Managed | ❌ Tests

🔗 Chain verdict:
  release/3.119.x: Tests red independently (Guardian TSA); Native/Managed warnings only — no cascade.
  release/4.147.0-preview.3: Public CI red (CS0016 errors); internal chain unaffected.

🐙 GitHub Actions:
  🟠 High:    Pages - Deploy ✅ | Sync - Samples ✅ | Auto API Docs Writer ❌
  🟡 Medium:  PR - Backport ✅ | Pages - Go Live! ✅ | Sync - Issue Triage ✅
  ⚪ Low:     All passing

Top actions:
1. [release/3.119.x] Fix Guardian TSA upload — blocks Tests → build 14177772
2. [release/4.147.0-preview.3] Investigate CS0016 errors → build 157985
3. [GitHub Actions] Auto API Docs Writer failing → run 26651087356

📁 Reports:
  JSON: output/ai/ci-status-2026-05-29.json
  HTML: output/ai/ci-status-2026-05-29.html
  MD:   output/ai/ci-status-2026-05-29.md
```

---

## Step 7: Follow-Up Investigations

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
| "What automation is failing?" | **ci-status** |
| "Trace the pipeline chain for branch X" | **release-status** |
| "Why is CI red?" | **ci-status** (with analysis) |
| "Is release/X shippable?" | **ci-status** (risk assessment) |
| "GitHub Actions status?" | **ci-status** |

---

## Scheduling as a Daily Workflow

This skill works well as a daily scheduled workflow:

```
Prompt: "Run the ci-status skill and report the health of main and recent release branches. Generate a full report with AI analysis."
Schedule: Daily at 9:00 AM
```
