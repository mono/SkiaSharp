---
name: release-branch
description: >
  Create a release branch for SkiaSharp.
  Use when user says "release X", "start release X", "create release branch for X",
  "I want to release", or "release now".
  This is the FIRST step of releasing - creates branch and pushes to trigger CI.
  Can auto-detect next preview version from main branch.
---

# Release Branch Skill

Create release branches for SkiaSharp versions.

⚠️ **NO UNDO:** This is step 1 of 3. See [releasing.md](../../../documentation/dev/releasing.md) for full workflow.

## ⚠️ Branch Protection (COMPLIANCE REQUIRED)

> **🛑 NEVER commit directly to `main` or `skiasharp` branches. This is a policy violation.**

| Repository | Protected Branches | Required Action |
|------------|-------------------|-----------------|
| SkiaSharp (parent) | `main` | Create `release/X.Y.Z` branch, never commit to main |
| externals/skia (submodule) | `main`, `skiasharp` | Must use feature branch if submodule changes needed |

**Release branches are created FROM main, but never modify main directly.**

---

## Step 1: Determine Version

### Auto-detect (user says "release now")

1. Fetch main and read `SKIASHARP_VERSION` from `scripts/azure-templates-variables.yml`
2. List existing branches: `git branch -r | grep "release/{version}-preview"`
3. Next preview = highest + 1 (or 1 if none)
4. **⚠️ Semver check:** Also verify no bare `release/{version}` branch exists — if it does, the stable release is already cut and you should NOT create another preview. Ask the user to confirm.
5. Confirm with user: "Next release will be `X.Y.Z-preview.N`. Proceed?"

### User provides version

Use the provided version directly.

---

## Step 2: Determine Release Type

⚠️ **Semver ordering:** A bare version `X.Y.Z` is ALWAYS newer than `X.Y.Z-preview.N`. When listing
branches to find the latest, remember that `release/3.119.2` > `release/3.119.2-preview.3`.
Do NOT use alphabetical sorting — it gives wrong results for semver.

| Version Format | Type | Base | PREVIEW_LABEL |
|----------------|------|------|---------------|
| `X.Y.Z-preview.N` | Preview | `main` | `preview.N` |
| `X.Y.Z` | Stable | `release/X.Y.Z-preview.{latest}` | `stable` |
| `X.Y.Z.F-preview.N` | Hotfix Preview | tag `vX.Y.Z` | `preview.N` |
| `X.Y.Z.F` | Hotfix Stable | `release/X.Y.Z.F-preview.{latest}` | `stable` |

For stable releases, find latest preview: `git branch -r | grep "release/X.Y.Z-preview" | sort -V | tail -1`

**NuGet version format by release type:**
- **Preview:** `{base}-{PREVIEW_LABEL}.{build}` (e.g., `3.119.2-preview.2.3`) — build number is part of the prerelease tag
- **Stable:** `{base}` only (e.g., `3.119.2`) — the build number is NEVER appended to stable versions. On the internal feed, stable builds appear as `{base}-stable.{build}` but the published version is just `{base}`.

---

## Step 3: Create Branch and Update PREVIEW_LABEL

1. Checkout the base (main, preview branch, or tag)
2. Create branch `release/{version}`
3. Edit `scripts/azure-templates-variables.yml`: set `PREVIEW_LABEL`
4. Commit: `git commit -m "Bump the version to {version}"`
5. Show diff summary to user and **confirm with `ask_user`** before pushing

---

## Step 4: Push Branch

```bash
git push -u origin release/{version}
```

This triggers the CI pipeline chain (2-4 hours total):

```
SkiaSharp-Native (devdiv/DevDiv, ID 26493)              ~60-90 min
    ↓ triggers on completion
SkiaSharp (devdiv/DevDiv, ID 10789)                     ~30-60 min  (managed build, signing & publishing)
    ↓ triggers on completion
SkiaSharp-Tests (devdiv/DevDiv, ID 15756)               ~15-30 min  (device & unit tests)
```

### Tracking Build Progress

Use the pipeline status script to check the full chain at once:

```bash
.agents/skills/release-branch/scripts/pipeline-status.sh release/{version}
```

For manual queries, use `az pipelines` to monitor individual pipelines:

```bash
# Find the native build for this branch
az pipelines runs list --pipeline-ids 26493 --branch release/{version} \
  --org https://devdiv.visualstudio.com --project DevDiv \
  --query "[].{id:id, status:status, result:result, buildNumber:buildNumber}" --top 3

# Check downstream builds and their trigger source
az pipelines runs show --id {build-id} \
  --org https://devdiv.visualstudio.com --project DevDiv \
  --query "{status:status, result:result, triggerInfo:triggerInfo}"
```

The `triggerInfo.pipelineId` field on each downstream build confirms which upstream build triggered it.
This is the definitive way to trace the pipeline chain (not just branch/time correlation).

---

## Step 5: Bump Version on Main (Preview from main only)

**Skip for stable and hotfix releases.**

1. Create branch `bump-version-{next}` from main

2. Edit `scripts/azure-templates-variables.yml`:
   - Update `SKIASHARP_VERSION` to next version
   - Reset `PREVIEW_LABEL` to `preview.0`

3. Edit `scripts/VERSIONS.txt`:
   - `SkiaSharp file` → `{next}.0`
   - All `SkiaSharp ... nuget` lines → `{next}`
   - `HarfBuzzSharp file` → increment 4th digit (e.g., `8.3.1.4` → `8.3.1.5`)
   - All `HarfBuzzSharp ... nuget` lines → same as file version

4. Commit: `git commit -m "Bump to the next version ({next}) after release"`

5. Show diff to user, then:
   ```bash
   git push -u origin bump-version-{next}
   gh pr create --title "Bump to the next version ({next}) after release" --body ""
   gh pr merge --merge --delete-branch
   ```

---

## Resources

- [releasing.md](../../../documentation/dev/releasing.md) — Version patterns, HarfBuzz versioning, workflow diagrams
