---
name: release-publish
description: >
  Publish SkiaSharp packages and finalize the release. Use when user says "publish X", "finalize X",
  "tag X", or "finish release X". This is the FINAL step - after release-testing passes.
  Publishes to NuGet.org, creates tag, GitHub release, and closes milestone.
  Uses confirmation workflow.
---

# Release Publish Skill

Publish packages to NuGet.org and finalize SkiaSharp releases.

**CRITICAL**: This skill uses a confirmation-based workflow. ALWAYS:
1. Create a detailed plan first
2. Ask user to confirm EACH step before executing
3. Never proceed without explicit approval

## Release Workflow Context

This skill is the final step of the release process:

1. **release-branch** → creates release branch, triggers CI build
2. **CI builds packages** → 2-4 hours to complete
3. **release-testing** → verify packages work on all platforms
4. **release-publish** (this skill) → publish to NuGet.org, tag, finalize

⚠️ **Only proceed when `release-testing` has passed.** Tags cannot be deleted.

## Workflow Overview

1. **Get version and build number** — Ask user for version and CI build number
2. **Publish to NuGet.org** — Trigger publish pipeline (preview optional, stable required)
3. **Verify packages live** — Confirm packages appear on NuGet.org
4. **Tag release** — Create and push version tag
5. **Create GitHub release** — Draft release with notes
6. **Close milestone** — (Stable releases only)

## Confirmation Pattern

For EVERY step, use ask_user tool with choices: "Yes, proceed" / "Skip this step" / "Abort"

## Commands

### Step 1: Get Version and Build Number

Ask user for:
- **Version**: e.g., `3.119.2` (stable) or `3.119.2-preview.3` (preview)
- **Build number**: from CI (visible in Azure Pipelines build URL or artifacts)

### Step 2: Publish to NuGet.org

| Release Type | Action |
|--------------|--------|
| Preview | Run [publish pipeline](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298) with preview flag |
| Stable | Run [publish pipeline](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298) with stable flag |

Ask user to trigger the pipeline and wait for completion.

### Step 3: Verify Packages Live

Confirm the specific version is visible on NuGet.org:

```bash
# Check for specific version
dotnet package search SkiaSharp --source https://api.nuget.org/v3/index.json | grep {version}
```

Or check directly:
- `https://www.nuget.org/packages/SkiaSharp/{version}`
- `https://www.nuget.org/packages/HarfBuzzSharp/{version}`

### Step 4: Tag Release

| Type | Tag Format | Example |
|------|------------|---------|
| Preview | `v{version}-preview.{N}.{build}` | `v3.119.2-preview.3.1` |
| Stable | `v{version}` | `v3.119.2` |
| Hotfix Preview | `v{version}.{fix}-preview.{N}.{build}` | `v3.119.2.1-preview.1.5` |
| Hotfix Stable | `v{version}.{fix}` | `v3.119.2.1` |

```bash
# Preview release
git fetch origin
git checkout release/{version}-preview.{N}
git pull origin release/{version}-preview.{N}
git tag v{version}-preview.{N}.{build}
git push origin v{version}-preview.{N}.{build}
```
```bash
# Stable release
git fetch origin
git checkout release/{version}
git pull origin release/{version}
git tag v{version}
git push origin v{version}
```
```bash
# Hotfix preview
git fetch origin
git checkout release/{version}.{fix}-preview.{N}
git pull origin release/{version}.{fix}-preview.{N}
git tag v{version}.{fix}-preview.{N}.{build}
git push origin v{version}.{fix}-preview.{N}.{build}
```
```bash
# Hotfix stable
git fetch origin
git checkout release/{version}.{fix}
git pull origin release/{version}.{fix}
git tag v{version}.{fix}
git push origin v{version}.{fix}
```

### Step 5: Create GitHub Release

```bash
# Preview release
gh release create v{version}-preview.{N}.{build} \
  --title "Version {version} (Preview {N})" \
  --prerelease \
  --notes "Release notes here"
```
```bash
# Stable release (attach samples.zip if available)
gh release create v{version} \
  --title "Version {version}" \
  --notes "Release notes here"
# Then: gh release upload v{version} samples.zip
```

### Step 6: Close Milestone (Stable only)

**Skip for preview releases.**

```bash
# List milestones to find the number
gh api repos/:owner/:repo/milestones --jq '.[] | "\(.number): \(.title)"'

# Close milestone
gh api repos/:owner/:repo/milestones/{milestone_number} -X PATCH -f state=closed
```

## Resources

- [`/documentation/releasing.md`](../../../documentation/releasing.md) — Release checklist and reference
