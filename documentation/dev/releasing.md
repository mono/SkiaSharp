# Release Guide

SkiaSharp releases use a repository-owned PowerShell script to prepare release
branches, the dnceng pipelines to build and test them, and the team-owned
publication pipeline to publish packages.

```text
Release - Prepare -> dnceng Build/Tests + BAR -> release-testing approval
    -> team publication -> GitHub release/milestones
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


## 1. Prepare release branches

Use the **Release - Prepare** workflow from `main`. It requires:

| Input | Example |
| --- | --- |
| `base` | `main`, `release/4.152.x`, or an exact commit SHA |
| `release` | `4.153.0-preview.1`, `4.153.0-rc.1`, or `4.153.0-stable` |

The workflow:

1. resolves `base` and runs `scripts/infra/publishing/prepare-release.ps1`;
2. defaults to a read-only run when `apply` and `push` are disabled;
3. passes `-Apply` when `apply` is selected; or
4. passes `-Push` when `push` is selected.

Use separate workflow dispatches for preview and mutation. Review the dry-run
output before selecting `apply` or `push`.

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
| --- | --- | --- |
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
| --- | --- | --- |
| `skiasharp-package` | 1642 | Build, signing, API Scan, BAR registration, packages |
| `skiasharp-tests` | 1630 | Tests consuming the exact Build pipeline resource |

Arcade routes `IsShipping=true` packages to `dotnet-libraries` and
`IsShipping=false` build inputs to `dotnet-libraries-transport`.

Release-testing smoke-tests the exact selected CI artifacts on the approved
host/device targets before team publication. NuGet.org is not a planner or
runner source for this gate.

The team-owned release process selects the exact connected Build and Tests runs
and their BAR. Before publication, use
[release-testing](../../.agents/skills/release-testing/SKILL.md) with the exact
SkiaSharp CI package version:

```bash
python3 .agents/skills/release-testing/scripts/plan-release-tests.py 4.150.3
```

The planner asks Maestro which BAR produced that version. If more than one
build produced it, pass the release-approved BAR explicitly:

```bash
python3 .agents/skills/release-testing/scripts/plan-release-tests.py \
  4.150.3 --bar-id 329644
```

The planner resolves the BAR asset's GUID-backed per-build V3 and flat-container
feed through Darc. It downloads exact `SkiaSharp` and `SkiaSharp.HarfBuzz`
packages from that feed, derives the exact `HarfBuzzSharp` dependency, and
requires all three packages to report the selected build's source branch and
commit. Every runner receives the same package versions and GUID feed;
`dotnet-public` supplies dependencies.

The skill runs the approved host/device matrix and records the human
release-approval gate. Failed required coverage blocks approval, but the skill
does not publish packages or change BAR state. After approval, the team
publishes the selected BAR to NuGet.org through its protected publication
pipeline.

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
exact-version tag at that commit, publishes a GitHub-generated Release, opens or
updates a focused support-tier PR, and dispatches release-note generation. A
preview or RC adds its `major.minor` line to `support.preview`; a stable release
adds it to `support.stable` and removes only that same line from
`support.preview`. Ending support for any other line remains a maintainer policy
decision.

Package, tag, and release writes always have a separate dry-run and explicit
confirmation. The repository automation never approves the team pipeline's
protected publication stage.

## 4. Maintain milestones

Run **Release - Milestones** after publication. Reconciliation and milestone
updates are selected by default; `push` is disabled by default. Review one
read-only dispatch, then enable `push` in a separate dispatch if the plan is
correct.

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

**Sync - Issue Template Versions** runs daily in push mode and opens or
refreshes its owned pull request. Manual dispatches default to read-only and
also expose `apply` and `push`. The same script can be run locally:

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

## Version reference

| Release type | Prepare input | Branch |
| --- | --- | --- |
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
| --- | --- |
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
