# Release Process Internals

Contracts, state ownership, and recovery for release automation. For operator
steps and workflow inputs, use the [Release Guide](releasing.md).

## Architecture and ownership

```text
Release - Prepare -> paired mono/skia and SkiaSharp release branches
  -> internal skiasharp-package -> signing -> BAR/default-channel promotion
  -> resource-triggered skiasharp-tests

Explicit publication request + exact Build/Tests/BAR evidence
  -> skiasharp-nuget-release -> isolated publisher / protected ESRP approval -> NuGet.org

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

The dedicated `skiasharp-nuget-release` definition 1715 in `dnceng/internal`
is the publication entry point, queued only on explicit request, not
automatically after Tests. Its source is `dotnet-release-workflows`,
`/eng/pipeline/entry/skiasharp-release-pipeline.yml`. The former MAUI publisher
is retired for SkiaSharp.

Three resources have distinct roles:

- publisher `self` ref/SHA selects the entry implementation;
- the `dotnet-release` repository resource ref/SHA selects production tooling;
- `resources.pipelines.skiaSharpBuild.version` selects the exact producer 1642
  **Build number**, not a numeric build ID, source SHA, or latest build.

Discover the live default/ref, inspect its exact entry YAML and tooling
resource, and verify current SkiaSharp production support before queueing.
Do not assume `main`, permanently pin a temporary production branch, or
return to MAUI when configuration is unavailable. Use the Runs API with the
verified refs/SHAs and explicit producer version; the sole entry parameter
`validateOnly` is boolean (`false` publishes, `true` validates only). There
are no MAUI repository, `commitHash`, push, or NuGet-filter inputs.

The pipeline consumes only the producer's `arcade_shipping_signed` artifact.
The classic `Container` `PackageArtifacts` can include unsigned/nonshipping
transport assets; neither it nor unsigned `nuget`/`arcade_shipping` is a
publication source. Derive expected shipping IDs and both product versions
from the shipment source and pack filter, not a hardcoded package count.

GET the admitted run to verify resolved publisher/tooling refs/SHAs,
`validateOnly`, and the selected producer's numeric ID, Build number, branch
and SHA against the release record. The immediate POST response may lack
resolved resources. Before dispatch, also inspect existing parents' resolved
producer resources; a success for another source is not equivalent.

The production entry invokes isolated publisher 1697 through
`dotnet-release`/ESRP. The current parent uses `requireApproval: false`, so
there is no parent `ManualValidation`; protected ESRP scan/release approval
remains. This is not permission to change access or bypass approval.

Monitor an existing publication rather than duplicating it. All exact
shipping IDs/versions and source provenance must be independently public
before a separately authorized Finish. The parent's "Require the first
attempt" guard prevents retrying failed parent jobs that may have queued
another publication. On failure or partial publication, preserve source,
producer/BAR, parent/isolated run, and ESRP IDs, audit every expected public
package, and report; never auto-requeue or switch publication paths.
Monitoring-only requests do not authorize CI repairs.
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
dedicated package publication. Standalone milestone repairs share that group.
Nested reusable calls use a run-specific child group to avoid deadlocking
their Finish caller.

Finish also computes a support PR: preview/RC adds the line to
`support.preview` without changing stable support. Stable releases add the line
to `support.stable` and remove only the same line from preview. All other
supported lines are preserved, with no automatic cap; only maintainers remove
lines when support ends. Patches do not duplicate a line, and already-correct
state is a no-op. Support follows SkiaSharp's own shipped releases, independent
of Chrome's current channels and the documentation regeneration floors in
`history_floor`.

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

Human output leads with a table combining public NuGet shipments, the unpublished
cut, that cut's exact-tip build/BAR, and the next release action. A second table
shows base-branch commits, open line PRs, exact-base build/BAR, and readiness
work. The third shows the upstream gap, base, sync PR, exact-sync build, and
sync action. These builds are not interchangeable.
It shows the latest public shipment using shared numeric version
ordering, retaining older unfinished/inconsistent shipments. Full history
remains in JSON. `openPullRequests` adds line-targeted open PR evidence;
`packageBuilds` also includes the incoming sync branch at its head SHA.
Sync CI is a source-branch package build, not PR merge validation.

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

It does not inspect BAR contents, dedicated publication runs, support metadata,
release notes, or milestone assignments. Those belong to their detailed
workflows. Exit codes are `0` no action, `1` pending work, `2` required remote
service/tool unavailable. `-Json` is undecorated; redirected text is plain.

## Recovery and local entry points

Matching branches, tags, Releases, PRs, and milestones are reused. Remote
writes are verified; transient gateway failures in idempotent repository
operations may be retried, semantic conflicts may not. After interruption,
inspect what persisted before resuming the owning operation with the same
identity. Publication is not an idempotent repository mutation: follow the
no-retry/reporting rules above rather than replaying a queue request or parent
job. Never repair by rewriting shipped state or substituting another BAR.

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
