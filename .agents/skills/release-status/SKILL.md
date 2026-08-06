---
name: release-status
description: >
  Check the status of a SkiaSharp release build pipeline chain.
  
  Use when user asks to:
  - Check release build status
  - See where the release pipeline is at
  - Track CI progress for a release
  - Find out if packages are ready
  - Get ADO build links for a release
  
  Triggers: "check release status", "how is the build", "where is the release",
  "pipeline status", "is the build done", "check CI", "how is the run doing",
  "are packages ready", "build progress".
  
  This is Step 2 of 4 in the release pipeline — after release-branch creates the branch
  and before release-testing runs integration tests.
---

# Release Status Skill

Check the status of the SkiaSharp release pipeline chain on Azure DevOps.

⚠️ This is **Step 2 of 4** in the release pipeline. See [releasing.md](../../../documentation/dev/releasing.md) for full workflow.

**Pipeline:** [Step 1: release-branch](../release-branch/SKILL.md) → **Step 2 (this skill)** → [Step 3: release-testing](../release-testing/SKILL.md) → [Step 4: release-publish](../release-publish/SKILL.md)

## Pipeline Chain

Release builds flow through a **3-pipeline chain** on Azure DevOps (devdiv/DevDiv org):

| Order | Pipeline Name | Definition ID | Role |
|-------|---------------|---------------|------|
| 1 | `SkiaSharp-Native` | 26493 | Builds native binaries for all platforms |
| 2 | `SkiaSharp` | 10789 | Builds managed code, signs & publishes to internal feed |
| 3 | `SkiaSharp-Tests` | 15756 | Runs device & unit tests |

Each pipeline is triggered by completion of the previous via Azure DevOps pipeline resources.
Packages appear on the internal feed after pipeline #2 (`SkiaSharp`) completes.

---

## Step 1: Run the Status Script

```bash
python3 .agents/skills/release-status/scripts/pipeline-status.py release/{version}
# Or pass a commit SHA:
python3 .agents/skills/release-status/scripts/pipeline-status.py {commit-sha}
```

This outputs:
- All three pipelines with status markers (`[OK]`, `[WARN]`, `[FAIL]`, `[RUNNING]`, `[WAITING]`)
- Build IDs and build numbers
- Trigger relationships proving which upstream build caused each downstream run
- Direct ADO links for each build

---

## Step 2: Interpret Results

| Scenario | Meaning | Next Action |
|----------|---------|-------------|
| All `[OK]` | Packages are on the internal feed | Proceed to `release-testing` |
| Native `[OK]`, SkiaSharp `[RUNNING]` | Managed build in progress | Wait |
| Native `[OK]`, SkiaSharp `[OK]`, Tests `[RUNNING]` | Tests running (packages already available) | Can start `release-testing` |
| Any `[FAIL]` | Pipeline failed | Investigate via ADO link, retry or fix |
| Native `[WARN]` (partiallySucceeded) | Some native platforms had warnings | Usually OK — check which platforms |

### Job-Level Details (In-Progress Builds)

When a pipeline is `inProgress`, the script queries the ADO timeline API and shows job-level
breakdown below the pipeline entry:

```
+- SkiaSharp-Native (ID 26493) - native binaries
|  [RUNNING] id=14361035    inProgress    pending               4.148.0-rc.1.1+4.148.0-rc.1
|
|  Jobs: 35 [OK] completed | 2 [FAIL] failed | 8 [RUNNING] running | 3 [WAITING] pending
|  Failed: Job_Name_1, Job_Name_2
|  Running: Win32 x64, Win32 arm64, iOS, macOS, Mac Catalyst, ...
|  Pending: Wasm, Linux ARM, Linux ARM64
```

**Reading job status:**
- **Completed count** — jobs that finished successfully (or with warnings)
- **Failed list** — jobs that failed (names shown so you can investigate)
- **Running list** — jobs actively executing (tells you what's left)
- **Pending list** — jobs not yet started (queued or waiting for agents)

---

## Step 3: Report to User

Present a summary table:

```
Pipeline Chain Status: release/3.119.4

| Pipeline | Status | Run ID | buildNumber | ADO Link |
|----------|--------|--------|-------------|----------|
| SkiaSharp-Native | [WARN] partiallySucceeded | 14361035 | 3.119.4-stable.2+3.119.4 | [link] |
| SkiaSharp | [OK] succeeded | 14361102 | 3.119.4-stable.2+3.119.4 | [link] |
| SkiaSharp-Tests | [RUNNING] inProgress | 14361241 | 3.119.4-stable.2+3.119.4 | [link] |

Packages are available from the selected SkiaSharp (10789) run.

Selected SkiaSharp run: 14361102 (3.119.4-stable.2+3.119.4)
Selected SkiaSharp test package: 3.119.4-stable.2
Eventual public SkiaSharp version: 3.119.4
```

---

## Manual Queries

If the script is unavailable, query pipelines individually:

```bash
# Check any pipeline by ID and branch
az pipelines runs list --pipeline-ids {id} --branch release/{version} \
  --org https://devdiv.visualstudio.com --project DevDiv \
  --query "[].{id:id, status:status, result:result, buildNumber:buildNumber}" --top 5

# Verify trigger relationship (proves which build triggered this one)
az pipelines runs show --id {build-id} \
  --org https://devdiv.visualstudio.com --project DevDiv \
  --query "triggerInfo"
```

### GitHub Commit Statuses

Only `SkiaSharp-Native` reports back to GitHub:

```bash
gh api "repos/mono/SkiaSharp/commits/release/{version}/statuses" \
  --jq '.[] | "\(.context) | \(.state) | \(.description // "")"'
```

---

## Identifying the Correct Run

Multiple runs may exist on the same branch (retries, new commits). Match by `buildNumber`:

```
buildNumber format: {base}-{label}.{build}+{branch-version}
Example:            3.119.4-stable.2+3.119.4
```

All pipelines in the same chain share the same buildNumber. The script traces trigger
relationships via `triggerInfo.pipelineId` to confirm the chain is connected.

---

## Extracting Package Versions

From the selected `SkiaSharp` run's `buildNumber` in the script output:

| Release Type | buildNumber Example | Test Package Version | Eventual Public Version |
|--------------|---------------------|----------------------|-------------------------|
| Preview / RC | `3.119.4-preview.1.1+3.119.4-preview.1` | `3.119.4-preview.1.1` | `3.119.4-preview.1.1` |
| Stable | `3.119.4-stable.2+3.119.4` | `3.119.4-stable.2` | `3.119.4` |

Pass the exact test package version to `release-testing`. For HarfBuzzSharp, combine its base
version from `VERSIONS.txt` with the same label and build suffix. Stable release testing must not
use the bare eventual public version.
