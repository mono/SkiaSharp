---
name: release-publish
description: >
  Finalize a SkiaSharp release after the team publication pipeline has pushed
  packages to NuGet.org. Use when the user says "finalize X", "tag X", "finish
  release X", or confirms that exact packages are public.
---

# Release Publish

The internal Build, Tests, signing, BAR, promotion, and NuGet.org publication
process is owned by the team pipeline and is outside this skill.

After the exact packages appear on NuGet.org, use **Release - Finish** or run:

```powershell
# Read-only
./scripts/infra/publishing/finish-release.ps1 `
  -Version 4.153.0-preview.1

# Publish the tag and GitHub Release
./scripts/infra/publishing/finish-release.ps1 `
  -Version 4.153.0-preview.1 `
  -Push
```

An abbreviated prerelease identity must resolve to exactly one public SkiaSharp
package version. The `-Push` run reads that package's source commit, creates the
immutable exact NuGet version tag at that commit, publishes a GitHub-generated
Release, and dispatches release-note generation. Run **Release - Milestones**
separately when milestone reconciliation is needed.

Always present the dry-run and obtain confirmation before `-Push`. Never move
or delete a tag, replace a published release, or substitute a newer package.
