---
name: release-branch
description: >
  Prepare SkiaSharp release branches with the repository-owned PowerShell
  script. Use when a maintainer asks to start, prepare, preview, RC, or cut a
  stable release branch.
---

# Release Branch

Use the **Release - Prepare** GitHub workflow for normal releases. In chat,
dispatch it on `main` with `push=false` first, inspect the read-only plan, and
obtain confirmation before dispatching the same inputs with `push=true`:

```bash
gh workflow run release-prepare.yml --repo mono/SkiaSharp --ref main \
  -f base=main -f release=4.153.0-preview.1 -f push=false

# After reviewing the plan and receiving confirmation:
gh workflow run release-prepare.yml --repo mono/SkiaSharp --ref main \
  -f base=main -f release=4.153.0-preview.1 -f push=true
```

Replace the example base and identity with the requested values. Locate and
inspect each dispatched run; never infer success from a successful dispatch.
Verify both release branches exist at the planned commits before proceeding.
The branch push starts the internal `skiasharp-package` and `skiasharp-tests`
chain. Package publication is a separate, explicitly requested
`release-publish` action; Prepare must not queue it.

The Prepare workflow accepts:

- `base`: a SkiaSharp branch or commit SHA;
- `release`: `X.Y.Z[-preview.N|-rc.N|-stable]`, or the corresponding
  four-part hotfix form `X.Y.Z.F[-preview.N|-rc.N|-stable]`.

## Local fallback

Use the local script only when the workflow is unavailable or the user
explicitly requests local execution. It does not replace workflow verification:

```powershell
# Read-only
./scripts/infra/publishing/prepare-release.ps1 -Base main -Release 4.153.0-preview.1 -Mode DryRun

# Create and validate local branches and commits
./scripts/infra/publishing/prepare-release.ps1 -Base main -Release 4.153.0-preview.1 -Mode Apply

# Create locally, push mono/skia then mono/SkiaSharp, and create a stable bump PR
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
