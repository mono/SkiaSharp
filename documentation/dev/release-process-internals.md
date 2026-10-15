# Release Process Internals

Contracts, state ownership, and recovery for release automation. For operator
steps and workflow inputs, use the [Release Guide](releasing.md).

## Architecture and ownership

```text
Release - Prepare -> paired mono/skia and SkiaSharp release branches
  -> internal skiasharp-package -> signing -> BAR/default-channel promotion
  -> resource-triggered skiasharp-tests

Explicit publication request + exact Build/Tests/BAR evidence
  -> MAUI release pipeline -> staged audit -> human approval -> NuGet.org

Public packages -> Release - Finish -> exact tag and GitHub Release
  -> support PR and documentation follow-ups
  -> Release - Milestones -> assignment reconciliation, dates, rollover, closure
```

Prepare does not publish packages. BAR promotion does not publish to NuGet.org.
Finish does not authorize package publication, and no workflow implicitly
merges the follow-up PRs.

| State | Authoritative source |
| --- | --- |
| SkiaSharp numeric version | `SKIASHARP_VERSION` in `scripts/azure-templates-variables.yml`, matching `scripts/VERSIONS.txt` |
| Channel | `PREVIEW_LABEL` in `scripts/azure-templates-variables.yml` |
| HarfBuzzSharp numeric version | HarfBuzzSharp entries in `scripts/VERSIONS.txt` |
| Native source | `externals/skia` gitlink at the selected SkiaSharp commit |
| Build revision | Arcade `OfficialBuildId` from Azure Pipelines |
| Built source | Repository metadata in package nuspecs |
| BAR and per-build feed | Maestro/Darc build metadata |
| Public shipment and tag target | Exact NuGet.org package version and its nuspec source commit |
| Shipped milestone boundaries | Immutable exact-version `v*` tags |
| Support tiers | `support.stable` / `support.preview` in `scripts/infra/docs/versions.json` |

Use the recorded identity rather than a nearby branch or newer build.
Release refs, published packages, and published GitHub
Releases are immutable; conflicting state stops mutation.

## Release identities and version coupling

Prepare accepts `X.Y.Z[.F]-preview.N`, `-rc.N`, or `-stable`.
Branches use `release/<identity>` with `-stable` removed. That suffix is a
Prepare-only sentinel: `PREVIEW_LABEL=stable` produces bare numeric package
versions; official prereleases append the Arcade build revision.

`set-build-variables.ps1` normalizes source identity and computes the package
build number. Exact stable builds require internal `release/*` provenance or
a downstream pipeline inheriting it. Later jobs reuse the original
`OfficialBuildId` and package build number across midnight. Resource-triggered
jobs inherit the upstream run name and special-package counter.

Label-only cuts keep both numeric product versions. A new four-part SkiaSharp
hotfix advances only one revision (`X.Y.Z` → `X.Y.Z.1` → `X.Y.Z.2`) and also
increments HarfBuzzSharp, avoiding reuse of an older BAR's package version.

HarfBuzzSharp's fourth component is a milestone bucket: the first Skia
milestone adopting native HarfBuzz `X.Y.Z` owns revisions 0–99; each later Skia
milestone using that native version adds 100. A native upgrade resets the
revision to zero and establishes a new base milestone. Prepare requires the
selected base to contain the correct bucket; it does not discover one.
See [Versioning](versioning.md) for the full scheme.

## Mutation boundaries

Prepare and Finish expose these script modes:

| Mode | Local writes | Remote writes |
| --- | --- | --- |
| `DryRun` | No | No |
| `Apply` | Yes | No |
| `Push` | Yes | Yes |

Their Actions workflows expose only `push`: unchecked maps to `DryRun`,
checked to `Push`. Prepare dispatches are restricted to the default branch;
Finish runs from `main`. Milestones likewise defaults to a read-only plan.
Review the plan and confirm before remote writes.

Local Prepare `Apply`/`Push` requires a clean SkiaSharp worktree and a clean
Skia submodule if initialized; otherwise it initializes Skia at the pinned
commit. Local script modes are recovery tools, not extra release gates.

## Branch preparation

`prepare-release.ps1` resolves `base` to one commit, validates version metadata,
reads its exact Skia gitlink, and computes the coupled product versions. It
creates or validates the deterministic SkiaSharp release version commit and
the matching mono/skia branch. Push order is mono/skia, then SkiaSharp.

Existing release branches are reused only when their metadata and gitlink
match the request. Conflicting local or remote branches stop preparation;
release branches are never force-updated.

For a three-part stable cut, Prepare advances the SkiaSharp patch and
HarfBuzzSharp revision within its bucket, resets the channel to `preview.0`,
and creates or reuses `bump-version-NEXT_VERSION` and its PR unless the target
is already advanced. An existing remote `release/X.Y.x` is the PR base;
otherwise it uses `main`. Prepare never creates a servicing branch or merges
the PR. Four-part hotfix stable cuts do not trigger another automatic bump.

## Build, signing, and validation

| Pipeline | ID | Trigger / contract |
| --- | --- | --- |
| `skiasharp-package` | 1642 | CI on `main` and `release/*`; produces signed packages and one BAR |
| `skiasharp-tests` | 1630 | Completion trigger consuming the exact Build run through its `SkiaSharp` pipeline resource |

The package pipeline prepares sources, builds natives, assembles/scans
packages, signs shipping assets, registers the Arcade manifest/BAR, validates
packages/signing, and promotes the BAR to its configured default Maestro
channel. API Scan is scheduled on `main` or explicitly requested, not an
automatic release-branch stage.

Only `arcade_shipping` is signed into `arcade_shipping_signed`.
The manifest combines signed Shipping with NonShipping transport assets
marked `IsShipping="false"`. Service-side channel configuration owns feed
routing; repository promotion is not the final NuGet.org approval.
See [Packages](packages.md) for artifact topology and symbol handling.

Tests inherit the upstream run name, channel, and build number. Release
validation requires successful producing Build and resource-triggered Tests
runs with matching branch, commit, build number, and pipeline resource, plus
the BAR. Public CI has no internal signing/BAR stages. Local Cake, tests,
or visual goldens can diagnose failures but cannot substitute for this evidence.

## Protected publication

The internal MAUI `dotnet-maui-release` definition 1445 owns production
credentials and approval. It is queued only on an explicit publication
request, not automatically after Tests. Use merged, mirrored MAUI `main`,
with default branch `refs/heads/main` and SkiaSharp support at its current tip.

Two commits have distinct roles:

- the resolved MAUI source SHA selects the pipeline implementation;
- `commitHash` selects the **SkiaSharp** source/BAR, not the Skia gitlink.

The dispatcher reads back the resolved source ref/SHA and parameters. The
pipeline resolves the SkiaSharp commit against BAR, stages shipping assets,
and emits `NuGetReleaseAudit`. Human `ManualValidation` must verify repository,
source commit, BAR, selected/staged package identities, ownership, and quota
before `1ES.PublishNuget` can use the protected NuGet.org connection.

An existing run is resumed/monitored, not duplicated. After approval, all exact
staged shipping IDs/versions must be independently public before Finish.
Partial or failed publication does not authorize automatic retry.
See [publication inputs](releasing.md#4-publish-the-packages).

## Finalization and concurrency

`finish-release.ps1` resolves an exact stable version as-is. An abbreviated
preview/RC must match exactly one public version in the NuGet.org index.
The public SkiaSharp nuspec supplies the repository branch and 40-character
source SHA: its branch must match the release identity, and its commit must
be reachable from that exact remote release branch.

Finish creates or verifies `vPUBLIC_VERSION` at that commit. Prerelease tags
include the build revision; stable tags are bare numeric versions. The
GitHub-generated changelog starts at the preceding exact shipment in global
semantic release topology, including parallel lines and all release channels.
Existing conflicting tag or Release metadata is rejected, never replaced.

The resolved version, numeric core, tag, and source commit are passed to the
required reusable Milestones workflow:

- `DryRun` supplies a virtual shipment so the plan includes prospective
  milestone changes without pretending the tag exists.
- `Push` verifies the real immutable tag against that commit before any
  milestone mutation.

A downstream milestone failure fails Finish. The global `release-state`
concurrency group covers **Finish through milestone completion**, not earlier
MAUI package publication. Standalone milestone repairs share that group.
Nested reusable calls use a run-specific child group to avoid deadlocking
their Finish caller.

Finish also computes a support PR: preview/RC adds the line to
`support.preview`; stable adds it to `support.stable` and removes only the
same line from preview. Already-correct state is a no-op. Retirement of another
line always requires a maintainer decision.

After the GitHub Release, Finish dispatches release-note/API-diff generation;
stable releases also dispatch issue-template version updates. These workflows
converge on current public state, with scheduled runs as fallback.
See [Release notes and API diffs](release-notes-and-api-diffs.md).

## Milestone maintenance

`reconcile-release-assignments.ps1` discovers matching release branches and
uses exact shipped tags as range boundaries. It extracts merged PR numbers
from commit subjects across the selected Git reachability ranges and assigns
PRs/issues where they first shipped.
Unshipped intermediate branches roll into the next shipped boundary.
Missing exact shipment milestones are planned/created before assignments;
unsafe tag, boundary, or ownership warnings block mutation.

Linked issues combine GitHub closing references with visible PR-body closing
directives. Body scanning excludes HTML comments, inline code, and fenced
code. Body-only candidates are validated through the issues endpoint:
PR targets and missing issues (HTTP 404) are skipped with explicit warnings.
Authentication, network, and server errors fail reconciliation.

`update-release-milestones.ps1` reads the current Skia identity and Chromium
schedule to maintain preview.1, preview.2, RC 1, and stable milestones.
The cadence follows overlapping two-week trains: earliest beta, early stable
cut, stable cut, and stable date respectively. It rolls remaining open work
to the next unshipped milestone and closes shipped milestones.

Both scripts are idempotent and verify writes. Neither creates release
branches, tags, packages, or GitHub Releases.

## Release audit

`audit-release-state.ps1` is a read-only next-action summary for one `A.B`
line, not an approval or milestone-repair tool.

- Maintenance is `release/A.B.x` when present; otherwise `main` qualifies
  only when its checked-in version belongs to the line.
- Exact public versions are checked against package provenance, tags, and
  GitHub Releases; specific release branches are checked at their current tip.
- For that tip, the newest succeeded package build with one BAR is selected
  even if a later attempt failed/canceled. Later attempts remain visible;
  partial success is not automatic publication readiness.
- Incoming milestone sync uses `skia-sync/release-A.B.x` for servicing or
  `skia-sync/mMILESTONE` on `main`, not bleeding-edge `skia-sync/main`.
  It compares `chrome/mMILESTONE` with the existing Skia sync branch, or the
  maintenance/native base when absent. Non-draft incoming PRs and new upstream
  work defer a new cut; existing publication/Finish work remains actionable.
- The immediately following milestone is recognized before `main` advances,
  so pending sync work is visible rather than treated as an empty line.

It does not inspect BAR contents, MAUI publication runs, support metadata,
release notes, or milestone assignments. Those belong to their detailed
workflows. Exit codes are `0` no action, `1` pending work, `2` required remote
service/tool unavailable. `-Json` is undecorated; redirected text is plain.

## Recovery and local entry points

Matching branches, tags, Releases, PRs, and milestones are reused. Remote
writes are verified; transient gateway failures may be retried, semantic
conflicts may not. After interruption, inspect what persisted and rerun the
owning operation with the same identity. Never repair by rewriting shipped
state or substituting another BAR.

Workflows are the normal entry points. Local read-only equivalents:

```powershell
./scripts/infra/publishing/prepare-release.ps1 -Base main -Release 4.153.0-preview.1 -Mode DryRun
./scripts/infra/publishing/finish-release.ps1 -Version 4.153.0-preview.1 -Mode DryRun
./scripts/infra/publishing/reconcile-release-assignments.ps1 -Version 4.153.0
./scripts/infra/publishing/update-release-milestones.ps1
./scripts/infra/publishing/update-bug-template.ps1 -Mode DryRun
```

After reviewing and confirming the plan, Prepare/Finish/template scripts
accept `-Mode Push`; milestone scripts accept `-Push`. `Apply` on
Prepare/Finish/template scripts changes local state only. Running Finish
locally does not replace the Actions workflow's required milestone call.

## Implementation map

| Concern | Primary files |
| --- | --- |
| Workflow orchestration | `.github/workflows/release-{prepare,finish,milestones}.yml` |
| Release contracts and safety | `scripts/infra/publishing/Publishing.Common.psm1`, `Git.Common.psm1`, `GitHub.Common.psm1` |
| Audit / preparation / finalization | `scripts/infra/publishing/{audit-release-state,prepare-release,finish-release}.ps1` |
| Build identity | `scripts/infra/shared/set-build-variables.ps1` |
| Build / Tests | `scripts/azure-pipelines-{package,tests}.yml`, `scripts/azure-templates-stages*.yml` |
| Signing / manifests | `eng/Signing.props`, `eng/Publishing.props` |
| Milestones | `scripts/infra/publishing/{reconcile-release-assignments,update-release-milestones}.ps1` |
| Release notes | `.github/workflows/update-release-notes.md`, `.agents/skills/release-notes/` |
| Issue-template versions | `.github/workflows/auto-update-issue-template-versions.yml`, `scripts/infra/publishing/update-bug-template.ps1` |
