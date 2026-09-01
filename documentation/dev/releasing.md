# Release Guide

SkiaSharp releases use a repository-owned PowerShell script to prepare release
branches, the dnceng pipelines to build and test them, and the team-owned
publication pipeline to publish packages.

```text
Release - Prepare -> dnceng Build/Tests -> team publication -> GitHub release/milestones
```

## Safety

Release refs, package versions, tags, and published GitHub Releases are treated
as immutable.

- Never force-update an existing release branch.
- Never move or delete a release tag.
- Never replace a published NuGet.org package version.
- Never replace conflicting remote state merely because a rerun expected
  something else.

The preparation script is convergent: matching state is reused and conflicting
state stops the run.

Before using the release workflows, configure required-reviewer protection on
the `release-branching`, `release-publish`, and `release-milestones`
environments. Store `SKIASHARP_AUTOBUMP_TOKEN` as an environment secret in the
first two. The read-only jobs fail closed when required-reviewer protection is
missing.

## 1. Prepare release branches

Use the **Release - Prepare** workflow from `main`. It requires:

| Input | Example |
|-------|---------|
| `base` | `main`, `release/4.152.x`, or an exact commit SHA |
| `release` | `4.153.0-preview.1`, `4.153.0-rc.1`, or `4.153.0-stable` |

The workflow:

1. resolves `base` to an exact commit;
2. runs `scripts/infra/publishing/prepare-release.ps1` read-only and shows every action;
3. waits at the protected `release-branching` environment;
4. reruns with the same base commit and `-Push`.

The script creates matching branches in `mono/SkiaSharp` and `mono/skia`. The
Skia branch points to the exact `externals/skia` gitlink used by the SkiaSharp
branch.

### Local modes

```powershell
# Read-only
./scripts/infra/publishing/prepare-release.ps1 `
  -Base main `
  -Release 4.153.0-preview.1

# Create and validate local branches and commits
./scripts/infra/publishing/prepare-release.ps1 `
  -Base main `
  -Release 4.153.0-preview.1 `
  -Apply

# Create locally, push both repositories, and create a stable bump PR
./scripts/infra/publishing/prepare-release.ps1 `
  -Base main `
  -Release 4.153.0-preview.1 `
  -Push
```

`-Push` implies the local Apply work. This makes the three modes:

| Mode | Local writes | Remote writes |
|------|--------------|---------------|
| no switch | No | No |
| `-Apply` | Yes | No |
| `-Push` | Yes | Yes |

Stable input uses the explicit `-stable` suffix but creates
`release/X.Y.Z`. For a three-part stable release, Prepare also:

- calculates the next SkiaSharp patch;
- increments HarfBuzzSharp within its current milestone bucket;
- creates `bump-version-X.Y.Z`;
- returns both families to `preview.0`; and
- opens a PR against a manually created `release/X.Y.x` servicing line when one
  exists, otherwise against `main`.

Release preparation never creates the optional `.x` line. The bump PR remains
human-owned and is never merged by the script.

## 2. Build, test, and publish packages

Pushing a `release/*` branch triggers the current dnceng release chain:

| Pipeline | ID | Responsibility |
|----------|----|----------------|
| `skiasharp-package` | 1642 | Build, signing, API Scan, BAR registration, packages |
| `skiasharp-tests` | 1630 | Tests consuming the exact Build pipeline resource |

The team-owned release process reviews the connected Build and Tests runs,
selects their exact BAR/packages, and publishes them to NuGet.org through its
protected publication pipeline. Repository automation does not query, queue, or
approve this internal boundary.

## 3. Create the public GitHub Release

Use the [release-publish](../../.agents/skills/release-publish/SKILL.md) skill
after the exact packages appear on NuGet.org. Use **Release - Finish** or run:

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
exact-version tag at that commit, publishes a marked GitHub Release, and
dispatches release-note generation. The GitHub Release summary workflow runs
automatically on publication and when reviewed release-note data later lands on
`main`.

Package, tag, and release writes always have a separate dry-run and explicit
confirmation. The repository automation never approves the team pipeline's
protected publication stage.

## 4. Maintain milestones

Run **Release - Milestones** after publication. Both workflow operations are
selected by default and can be run independently:

```powershell
# Reconcile shipped pull requests and linked issues
./scripts/infra/publishing/reconcile-release-assignments.ps1 `
  -Version 4.153.0

# Update milestone dates, rollover, and closure
./scripts/infra/publishing/update-release-milestones.ps1
```

Both scripts are read-only by default. Add `-Push` to the selected script after
reviewing its output:

```powershell
./scripts/infra/publishing/reconcile-release-assignments.ps1 `
  -Version 4.153.0 `
  -Push

./scripts/infra/publishing/update-release-milestones.ps1 -Push
```

These scripts never change release branches, tags, packages, or GitHub
Releases.

## Maintain issue-template versions

**Sync - Issue Template Versions** runs daily and opens or refreshes its owned
pull request. The same script can be run locally:

```powershell
# Read-only
./scripts/infra/publishing/update-bug-template.ps1

# Update only the local issue form
./scripts/infra/publishing/update-bug-template.ps1 -Apply

# Refresh the owned automation branch and pull request
./scripts/infra/publishing/update-bug-template.ps1 -Push
```

It derives both bug-report version dropdowns from published GitHub Releases and
preserves every unrelated line in the issue form.

## Optional public-package smoke testing

After an exact package version is public, use
[release-testing](../../.agents/skills/release-testing/SKILL.md) for
host/device smoke testing:

```bash
python3 .agents/skills/release-testing/scripts/plan-release-tests.py \
  4.153.0-preview.1.26431.1
```

This is advisory validation. It does not unlock or mutate publication state.

## Version reference

| Release type | Prepare input | Branch |
|--------------|---------------|--------|
| Preview | `X.Y.Z-preview.N` | `release/X.Y.Z-preview.N` |
| RC | `X.Y.Z-rc.N` | `release/X.Y.Z-rc.N` |
| Stable | `X.Y.Z-stable` | `release/X.Y.Z` |
| Hotfix preview | `X.Y.Z.F-preview.N` | `release/X.Y.Z.F-preview.N` |
| Hotfix stable | `X.Y.Z.F-stable` | `release/X.Y.Z.F` |

Hotfixes advance exactly one four-part revision from their base:
`X.Y.Z → X.Y.Z.1 → X.Y.Z.2`. Each hotfix also increments HarfBuzzSharp.

Prerelease public packages append the Arcade build revision. Stable packages use
the bare numeric version.

### HarfBuzzSharp milestone buckets

HarfBuzzSharp uses `X.Y.Z.N`. The Skia milestone that first adopts native
HarfBuzz `X.Y.Z` owns revisions 0-99. Each later Skia milestone using that same
native version adds 100:

Every promoted build that changes the SkiaSharp numeric version also needs a
unique HarfBuzzSharp version; BAR registrations cannot reuse an older package
version from another build.

| Milestone relative to adoption | Revision range |
|--------------------------------|----------------|
| Base milestone | 0-99 |
| Base + 1 | 100-199 |
| Base + 2 | 200-299 |

For example, M150 adopted HarfBuzz 14.2.1, so M151 uses `14.2.1.100-199`
and M152 uses `14.2.1.200-299`. Releases within one milestone increment by one.
A native HarfBuzz upgrade resets the revision to zero and establishes a new
base milestone.

## Related documentation

- [Versioning](versioning.md)
- [Packages](packages.md)
- [Release notes and API diffs](release-notes-and-api-diffs.md)
- [Release assignment script](../../scripts/infra/publishing/reconcile-release-assignments.ps1)
- [Release milestone script](../../scripts/infra/publishing/update-release-milestones.ps1)
- [Issue-template version script](../../scripts/infra/publishing/update-bug-template.ps1)
