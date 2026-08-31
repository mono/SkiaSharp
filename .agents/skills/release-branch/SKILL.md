---
name: release-branch
description: >
  Prepare SkiaSharp release branches with the repository-owned PowerShell
  script. Use when a maintainer asks to start, prepare, preview, RC, or cut a
  stable release branch.
---

# Release Branch

Use the **Release - Prepare** GitHub workflow for normal releases. It runs the
same script twice: a read-only preview, followed by an environment-approved
push using the exact resolved base commit.

The script requires:

- `-Base`: a SkiaSharp branch or commit SHA;
- `-Release`: `X.Y.Z-preview.N`, `X.Y.Z-rc.N`, or `X.Y.Z-stable`.

Local modes:

```powershell
# Read-only
./scripts/infra/publishing/prepare-release.ps1 -Base main -Release 4.153.0-preview.1

# Create and validate local branches and commits
./scripts/infra/publishing/prepare-release.ps1 -Base main -Release 4.153.0-preview.1 -Apply

# Create locally, push mono/skia then mono/SkiaSharp, and create a stable bump PR
./scripts/infra/publishing/prepare-release.ps1 -Base main -Release 4.153.0-preview.1 -Push
```

Before `-Push`, show the resolved base SHA and every planned ref to the user and
obtain confirmation. Never force-update a release branch. Existing matching
state is reused; conflicting state blocks the run.

Stable input uses the explicit `-stable` suffix but creates
`release/X.Y.Z`. A three-part stable release also prepares the next SkiaSharp
patch and HarfBuzzSharp revision on `bump-version-X.Y.Z` and opens a PR against
`release/X.Y.x`.
