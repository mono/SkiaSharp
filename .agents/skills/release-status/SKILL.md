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
- All three pipelines with status icons (✅ ❌ 🔄 ⏳)
- Build IDs and build numbers
- Trigger relationships proving which upstream build caused each downstream run
- Direct ADO links for each build

---

## Step 2: Interpret Results

| Scenario | Meaning | Next Action |
|----------|---------|-------------|
| All ✅ | Packages are on the internal feed | Proceed to `release-testing` |
| Native ✅, SkiaSharp 🔄 | Managed build in progress | Wait |
| Native ✅, SkiaSharp ✅, Tests 🔄 | Tests running (packages already available) | Can start `release-testing` |
| Any ❌ | Pipeline failed | Investigate via ADO link, retry or fix |
| Native ⚠️ (partiallySucceeded) | Some native platforms had warnings | Usually OK — check which platforms |

---

## Step 3: Report to User

Present a summary table:

```
Pipeline Chain Status: release/3.119.4

| Pipeline | Status | Build | ADO Link |
|----------|--------|-------|----------|
| SkiaSharp-Native | ✅ partiallySucceeded | 3.119.4-stable.2 | [link] |
| SkiaSharp | 🔄 inProgress | 3.119.4-stable.2 | [link] |
| SkiaSharp-Tests | ⏳ not triggered | — | — |

Packages will be available after SkiaSharp (10789) completes.
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

## Extracting the NuGet Version

From the `buildNumber` in the script output:

| Release Type | buildNumber Example | NuGet Version |
|--------------|---------------------|---------------|
| Preview | `3.119.4-preview.1.1+3.119.4-preview.1` | `3.119.4-preview.1.1` |
| Stable | `3.119.4-stable.2+3.119.4` | `3.119.4` (no build suffix) |

For stable releases, the internal feed has `{base}-stable.{build}` but the published NuGet
version is always just the base (e.g., `3.119.4`).
