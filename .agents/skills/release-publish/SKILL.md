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

After the exact packages appear on NuGet.org, use **Release - Finish**. Leave
`push` unchecked for the read-only plan, then dispatch again with `push`
checked after approval. The equivalent local commands are:

```powershell
# Read-only
./scripts/infra/publishing/finish-release.ps1 `
  -Version 4.153.0-preview.1 `
  -Mode DryRun

# Apply the support-tier file update locally without publishing
./scripts/infra/publishing/finish-release.ps1 `
  -Version 4.153.0-preview.1 `
  -Mode Apply

# Publish the tag and GitHub Release
./scripts/infra/publishing/finish-release.ps1 `
  -Version 4.153.0-preview.1 `
  -Mode Push
```

An abbreviated prerelease identity must resolve to exactly one public SkiaSharp
package version. The `Push` run reads that package's source commit, creates the
immutable exact NuGet version tag at that commit, publishes a GitHub-generated
Release whose generated notes start at the immediately preceding exact shipment
in global release topology, opens or updates the released line's support-tier
PR, dispatches release-note generation, then requires milestone reconciliation
and date/rollover maintenance to complete. The dry-run exposes that same
milestone plan using an explicit virtual shipment (the planned tag and package
source commit); the push run verifies the real tag still points to that commit
before any milestone mutation.

Before planning or publishing, Finish requires the package's nuspec branch to
match the inferred release branch and its source commit to be reachable from
that branch. A package from another line is a blocking provenance error, not a
warning.

Milestone reconciliation plans or creates a missing exact shipped-release
milestone before assigning pull requests and linked issues, so a valid Finish
does not require manual milestone setup.

Always present the dry-run and obtain confirmation before `-Mode Push`. Never move
or delete a tag, replace a published release, or substitute a newer package.

Use **Release - Milestones** separately only for diagnostics or repairs, where
its independent reconciliation and update toggles remain available.
