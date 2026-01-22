---
name: release-branch
description: Create a release branch for SkiaSharp. Use when user says "release X", "start release X", or "create release branch for X". This is the FIRST step of releasing - creates branch and pushes to trigger CI. Uses confirmation workflow.
---

# Release Branch Skill

Create release branches for SkiaSharp versions with step-by-step confirmation.

**CRITICAL**: This skill uses a confirmation-based workflow. ALWAYS:
1. Create a detailed plan first
2. Ask user to confirm EACH step before executing
3. Never proceed without explicit approval

## Workflow Overview

1. **Plan** — Analyze version, determine release type, show plan
2. **Create branch** — Branch from correct base with proper naming
3. **Update PREVIEW_LABEL** — Set on release branch, commit and push
4. **Bump main** — (Preview from main only) Create PR to update version on main, merge immediately

After this skill completes:
- CI builds and publishes packages (2-4 hours)
- Main branch is ready for continued development
- Once packages are live and verified, use **release-tag** skill to finalize

## Confirmation Pattern

For EVERY step, use ask_user tool with choices: "Yes, proceed" / "Skip this step" / "Abort release"

## Determining Release Type

| User Says | Type | Base Branch | New Branch |
|-----------|------|-------------|------------|
| "X.Y.Z-preview.N" | Preview | `main` | `release/X.Y.Z-preview.N` |
| "X.Y.Z" (stable) | Stable | `release/X.Y.Z-preview.N` | `release/X.Y.Z` |
| "X.Y.Z.F-preview.N" | Hotfix | tag `vX.Y.Z` | `release/X.Y.Z.F-preview.N` |

## Commands

### Step 1: Create Release Branch

```bash
# For preview (from main)
# Note: {N} is typically 1 for first preview, 2 for second, etc.
git fetch origin
git checkout main
git pull origin main
git checkout -b release/{version}-preview.{N}
```
```bash
# For stable (from preview branch)
git fetch origin
git checkout release/{version}-preview.{N}
git pull origin release/{version}-preview.{N}
git checkout -b release/{version}
```
```bash
# For hotfix (from tag)
# Note: {fix} is typically 1 for first hotfix, 2 for second, etc.
# Note: {N} is typically 1 for first preview of a hotfix
git fetch origin --tags
git checkout v{version}
git checkout -b release/{version}.{fix}-preview.{N}
```

### Step 2: Update PREVIEW_LABEL and Push

Edit `scripts/azure-templates-variables.yml`:

| Type | PREVIEW_LABEL | Example |
|------|---------------|---------|
| Preview | `preview.{N}` | `preview.3` |
| Stable | `stable` | `stable` |
| Hotfix | `preview.{N}` | `preview.1` |

```bash
git add scripts/azure-templates-variables.yml
git commit -m "Bump the version to {version}"
git push -u origin HEAD  # Pushes current branch (release/{version} or release/{version}-preview.{N})
```

### Step 3: Wait for Push to Complete

After pushing, verify the branch appears on GitHub before proceeding to step 4.

### Step 4: Bump Version on Main (Preview from main only)

**Skip this step for stable and hotfix releases** — only preview releases from main need version bumping.

Edit these files:
- `scripts/azure-templates-variables.yml` — set `SKIASHARP_VERSION: {next-version}`
- `scripts/VERSIONS.txt` — update ALL version numbers:
  - `SkiaSharp file` version (e.g., `3.119.3.0`)
  - All SkiaSharp `nuget` versions (e.g., `3.119.3`)
  - `HarfBuzzSharp file` version — increment 4th digit (e.g., `8.3.1.3` → `8.3.1.4`)
  - All HarfBuzzSharp `nuget` versions — same as file version

> **HarfBuzzSharp versioning note:** The first 3 digits (`8.3.1`) correspond to the native HarfBuzz version. The 4th digit is incremented with each SkiaSharp release to keep packages in sync, even if there are no HarfBuzz changes. When native HarfBuzz is upgraded, reset to 3-digit version (e.g., `8.4.0`).

```bash
git checkout main
git pull origin main
git checkout -b bump-version-{next-version}
# Make edits to both files
git add scripts/azure-templates-variables.yml scripts/VERSIONS.txt
git commit -m "Bump to the next version ({next-version}) after release"
git push -u origin bump-version-{next-version}
gh pr create --title "Bump to the next version ({next-version}) after release" --body ""
gh pr merge --merge --delete-branch
```

## Resources

- [`/documentation/releasing.md`](../../../documentation/releasing.md) — Release checklist and reference
