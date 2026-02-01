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

⚠️ **NO UNDO:** This is step 1 of 3. See [releasing.md](../../../documentation/releasing.md) for full workflow.

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
4. Confirm with user: "Next release will be `X.Y.Z-preview.N`. Proceed?"

### User provides version

Use the provided version directly.

---

## Step 2: Determine Release Type

| Version Format | Type | Base | PREVIEW_LABEL |
|----------------|------|------|---------------|
| `X.Y.Z-preview.N` | Preview | `main` | `preview.N` |
| `X.Y.Z` | Stable | `release/X.Y.Z-preview.{latest}` | `stable` |
| `X.Y.Z.F-preview.N` | Hotfix Preview | tag `vX.Y.Z` | `preview.N` |
| `X.Y.Z.F` | Hotfix Stable | `release/X.Y.Z.F-preview.{latest}` | `stable` |

For stable releases, find latest preview: `git branch -r | grep "release/X.Y.Z-preview" | sort -V | tail -1`

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

This triggers CI build (2-4 hours).

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

- [releasing.md](../../../documentation/releasing.md) — Version patterns, HarfBuzz versioning, workflow diagrams
