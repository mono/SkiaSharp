---
name: release-tag
description: Tag and finalize a SkiaSharp release after packages are published. Use when user says "tag X", "finish release X", or "finalize X". This is the FINAL step - after packages are live. Tags cannot be deleted so only do this when release is confirmed good. Uses confirmation workflow.
---

# Release Tag Skill

Tag and finalize SkiaSharp releases after packages are published and verified.

**CRITICAL**: This skill uses a confirmation-based workflow. ALWAYS:
1. Create a detailed plan first
2. Ask user to confirm EACH step before executing
3. Never proceed without explicit approval

This skill is used AFTER:
1. **release-branch** skill created and pushed the branch
2. CI built and published packages
3. Packages are verified live on NuGet.org / preview feed

⚠️ **Tags cannot be deleted** — only proceed when release is confirmed successful.

## Workflow Overview

1. **Verify packages** — Confirm packages are live and working
2. **Get build number** — Ask user for the build number from CI
3. **Tag release** — Create and push version tag
4. **Create GitHub release** — Draft release with notes
5. **Close milestone** — (Stable releases only) Close GitHub milestone

## Confirmation Pattern

For EVERY step, use ask_user tool with choices: "Yes, proceed" / "Skip this step" / "Abort"

## Tag Format

| Type | Tag Format | Example |
|------|------------|---------|
| Preview | `v{version}-preview.{N}.{build}` | `v3.119.2-preview.3.1` |
| Stable | `v{version}` | `v3.119.2` |
| Hotfix | `v{version}.{fix}-preview.{N}.{build}` | `v3.119.2.1-preview.1.5` |

Build number comes from CI - ask user for the build number.

## Commands

### Step 1: Verify Packages

Ask user to confirm packages are live:
- Preview: Check `https://aka.ms/skiasharp-eap/index.json`
- Stable: Check NuGet.org

### Step 2: Get Build Number

Ask user for the build number from CI. This is visible in the Azure Pipelines build URL or artifacts.

### Step 3: Tag Release

```bash
# For preview release (e.g., 3.119.2-preview.3)
git fetch origin
git checkout release/{version}-preview.{N}
git pull origin release/{version}-preview.{N}
git tag v{version}-preview.{N}.{build}
git push origin v{version}-preview.{N}.{build}
```
```bash
# For stable release (e.g., 3.119.2)
git fetch origin
git checkout release/{version}
git pull origin release/{version}
git tag v{version}
git push origin v{version}
```
```bash
# For hotfix preview (e.g., 3.119.2.1-preview.1)
git fetch origin
git checkout release/{version}.{fix}-preview.{N}
git pull origin release/{version}.{fix}-preview.{N}
git tag v{version}.{fix}-preview.{N}.{build}
git push origin v{version}.{fix}-preview.{N}.{build}
```

### Step 4: Create GitHub Release

```bash
# Preview release
gh release create v{version}-preview.{N}.{build} \
  --title "Version {version} (Preview {N})" \
  --prerelease \
  --notes "Release notes here"
```
```bash
# Stable release  
gh release create v{version} \
  --title "Version {version}" \
  --notes "Release notes here"
```

### Step 5: Close Milestone (Stable only)

**Skip this step for preview releases.**

First, find the milestone number:
```bash
# List open milestones to find the number
gh api repos/:owner/:repo/milestones --jq '.[] | "\(.number): \(.title)"'
```

Then close the milestone:
```bash
gh api repos/:owner/:repo/milestones/{milestone_number} -X PATCH -f state=closed
```

Or close manually via GitHub UI: Repository → Milestones → Select milestone → Close.

## Resources

- [`/documentation/releasing.md`](../../../documentation/releasing.md) — Release checklist and reference
