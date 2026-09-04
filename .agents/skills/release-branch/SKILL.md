---
name: release-branch
description: >
  Prepare SkiaSharp release branches with the repository-owned PowerShell
  script. Use when a maintainer asks to start, prepare, preview, RC, or cut a
  stable release branch.
---

# Release Branch

Use the **Release - Prepare** GitHub workflow for normal releases. Leave `push`
unchecked for the read-only plan. After reviewing that output, dispatch again
with `push` checked to create the remote release branches.

The script requires:

- `-Base`: a SkiaSharp branch or commit SHA;
- `-Release`: `X.Y.Z[-preview.N|-rc.N|-stable]`, or the corresponding
  four-part hotfix form `X.Y.Z.F[-preview.N|-rc.N|-stable]`.

Local modes:

```powershell
# Read-only
./scripts/infra/publishing/prepare-release.ps1 -Base main -Release 4.153.0-preview.1 -Mode DryRun

# Create and validate local branches and commits
./scripts/infra/publishing/prepare-release.ps1 -Base main -Release 4.153.0-preview.1 -Mode Apply

# Create locally, push paired Skia then SkiaSharp, and create a stable bump PR
./scripts/infra/publishing/prepare-release.ps1 -Base main -Release 4.153.0-preview.1 -Mode Push
```

Before `-Mode Push`, show the resolved base SHA and every planned ref to the user and
obtain confirmation. Never force-update a release branch. Existing matching
state is reused; conflicting state blocks the run.

Stable input deliberately uses the explicit `-stable` sentinel to prevent an
accidental stable cut, but creates the bare `release/X.Y.Z` branch. A three-part
stable release also prepares the next SkiaSharp patch and HarfBuzzSharp
revision on `bump-version-X.Y.Z`. Its PR targets a manually created
`release/X.Y.x` servicing line when one exists, otherwise `main`; release
preparation never creates the `.x` line.
