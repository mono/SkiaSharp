# Release Guide

Maintainer runbook for shipping SkiaSharp. Follow the steps in order; use
[Release process internals](release-process-internals.md) for automation
contracts and recovery details.

## Quick steps

1. [Audit](#1-audit-the-release-line) the line and resolve outstanding work.
2. [Prepare](#2-prepare-the-release-branches) the paired release branches.
3. [Verify](#3-verify-the-release-build-and-tests) the producing Build, Tests, and BAR.
4. [Publish](#4-publish-the-packages) through the dedicated SkiaSharp publisher and protected ESRP approval, then verify NuGet.org.
5. [Finish](#5-finish-the-public-release) to publish the tag and GitHub Release and update milestones.
6. [Complete follow-ups](#6-complete-follow-ups) by reviewing and merging the automation PRs.

## Safety and prerequisites

- Release branches, tags, package versions, and published GitHub Releases are
  immutable. Never force-update, move, delete, or replace them.
- Keep the exact source commit, producing Build/Tests runs, BAR, and package
  versions together. Do not substitute artifacts after testing.
- Prepare, Finish, and Milestones require a read-only plan first (`push`
  unchecked), then confirmation before a second dispatch with identical inputs
  and `push` checked. Stop on conflicting remote state.
- Package publication uses the dedicated internal SkiaSharp publisher and
  protected ESRP scan/release approval. A successful build does not authorize
  publication; public packages do not authorize Finish.

Choose the source branch or commit and the release identity before Prepare.
Use `X.Y.Z-preview.N`, `X.Y.Z-rc.N`, or `X.Y.Z-stable`; four-part hotfixes
use `X.Y.Z.F` with the same suffixes. `-stable` is a Prepare-only sentinel:
the resulting branch, package, tag, and GitHub Release use the bare version.
See [Versioning](versioning.md) for version and HarfBuzzSharp coupling rules.

If the stable line needs a servicing branch, create `release/X.Y.x` from the
intended maintenance base **before** Prepare's push run. Prepare never creates
it. The post-stable bump PR targets that branch when present, otherwise `main`.

## 1. Audit the release line

```powershell
pwsh ./scripts/infra/publishing/audit-release-state.ps1 -Version 4.152
```

Use `-Check` to select the data to fetch:

| Check | Scope |
| --- | --- |
| `All` (default) | Full release, branch readiness, and upstream audit |
| `Upstream` | Upstream commits and sync PRs/builds |
| `Prepare` | Branch readiness, including publication and upstream blockers |
| `Finish` | Package completeness and tag/GitHub Release state |
| `NuGet` | Package completeness; source tags/builds used only to identify shipments |

`-Version` accepts `A.B` or an inclusive same-major range:

```powershell
pwsh ./scripts/infra/publishing/audit-release-state.ps1 -Version 4.150-4.156 -Check Upstream
```

`4.150..4.156` is equivalent. With `-Json`, a line returns one object and a range
returns an array. Failed lines do not stop the range; the highest exit code wins.
Unrequested facts are null. Only `All` and `Prepare` check every gate for a new cut.

Package checks use `scripts/VERSIONS.txt` and the shipping pack filter at the
shipment's source commit, with separate SkiaSharp/HarfBuzzSharp versions.
JSON `Publication` reports `complete`, `partial`, `missing`, or `unavailable`,
plus counts and missing/unavailable ID/version pairs. Reused public versions
count as available; a partial set alone does not establish why packages are missing.

The text report combines shipping status in one table and keeps sync work separate:

| Section | Meaning |
| --- | --- |
| Current release | Public NuGet version, cut awaiting publication, that cut's build/BAR, and the release action |
| Branch readiness | The `.x` or `main` base for Prepare, additional merged commits, open line PRs, its own build/BAR, and readiness action |
| Upstream / sync | Target base, upstream gap, incoming sync PR, its own sync-branch build, and sync action |

The release table's next step is a release operation, not an instruction to
sync Skia. Full release SHA/BAR evidence and actionable workflow inputs follow
the table. Finish completes public packages' tags and GitHub Releases.

The version checked into a source branch is neither a public package version
nor proof a release has been prepared. A succeeded base build does not replace
the prepared release branch's build or BAR. Queued commits are already merged
into the base; open backport PRs are listed separately and not counted.
Base readiness does not override current-shipment or sync gates. The sync
build is the exact sync-source `skiasharp-package` build, not PR merge CI;
missing/unavailable evidence is explicit. An up-to-date sync branch is
not proof its PR has merged. Upstream work does not block publication or Finish
of an already-cut release.

The release table includes the latest complete and partial shipments and older
unpublished stable cuts. Older gaps require review, not automatic publication.
Full history, including unused preview/RC branches, remains in `-Json`.
A partial package build with a BAR still requires review; even a
succeeded release build requires the detailed publication checks below.

| Recommendation | Next step |
| --- | --- |
| Merge incoming milestone sync | Complete the paired Skia sync PRs before a new cut |
| Prepare | Review the branch plan in step 2 |
| Publish | Verify Build/Tests evidence, then use step 4 |
| Complete existing shipment | Investigate the existing BAR/publication run and missing ID/version pairs; do not prepare a replacement cut or Finish |
| Finish | Verify all shipping packages are public, then use step 5 |
| Required check unavailable | Restore access and rerun; do not infer readiness |

Exit codes: `0` no action needed, `1` release work remains, `2` a required
service or tool was unavailable. The audit does not replace the detailed
Build/Tests, BAR, publication-run, support, notes, or milestone checks below.
For several lines or a range, use the `release-audit` skill.

## 2. Prepare the release branches

Run [Release - Prepare](https://github.com/mono/SkiaSharp/actions/workflows/release-prepare.yml)
on `main`, through Actions or the `release-branch` skill.

| Input | Value |
| --- | --- |
| `base` | `main`, `release/X.Y.x`, or an exact commit SHA |
| `release` | `X.Y.Z[.F]-preview.N`, `-rc.N`, or `-stable` |
| `push` | Unchecked for the plan; checked only after confirmation |

Review the resolved base SHA, coupled package versions, branch names, and
remote writes. The push run creates or validates `release/<identity>` in both
repositories, omitting `-stable`, and pushes `mono/skia` before
`mono/SkiaSharp`. Verify both branches at the planned commits.

A three-part stable cut also creates or reuses the next-preview version-bump
PR unless maintenance is already advanced. Four-part hotfix stable cuts do
not create another automatic bump. No PR is merged automatically.

## 3. Verify the release build and tests

Pushing the SkiaSharp release branch starts the internal pipeline chain.
Do not manually queue a different build.

| Pipeline | Required evidence |
| --- | --- |
| [`skiasharp-package`](https://dev.azure.com/dnceng/internal/_build?definitionId=1642) | Succeeded at the exact release branch and commit; record Build ID, run number, package versions, and BAR ID |
| [`skiasharp-tests`](https://dev.azure.com/dnceng/internal/_build?definitionId=1630) | Succeeded, triggered by that exact Build run through its `SkiaSharp` pipeline resource |

Branch, commit, build number, and upstream pipeline resource must agree.
Public CI artifacts are unsigned and are not the release BAR. Local tests
help diagnose failures but do not replace the producing Build/Tests evidence.

## 4. Publish the packages

Verify the Build/Tests/BAR evidence and check for existing publication parents
and their resolved `skiaSharpBuild` resources. Match the exact producer numeric
ID, build number, branch, and SHA, not just another release's successful run.
Monitor an existing publication; do not queue a duplicate. The
`release-publish` skill can perform these checks and queue the pipeline.

Publication uses
[`skiasharp-nuget-release`](https://dev.azure.com/dnceng/internal/_build?definitionId=1715)
in `dnceng/internal`. Its repository is `dotnet-release-workflows`, with entry
YAML `/eng/pipeline/entry/skiasharp-release-pipeline.yml`. The former MAUI
publisher is retired for SkiaSharp; do not use it as a fallback.

Before dispatch, discover the live publisher default/ref and current production
`dotnet-release` tooling resource. Inspect the exact YAML and verify both
refs/SHAs support SkiaSharp publication. For example, read the live definition:

```bash
az pipelines show \
  --organization https://dev.azure.com/dnceng --project internal --id 1715 \
  --output json
```

Verify the executable configuration and production support, not stale rollout
comments. Do not assume `main` or hardcode a temporary production branch.
If support cannot be verified, stop and report.

On an explicit publication request, prepare a JSON Runs API body
(`publication.json` below), replacing every placeholder with verified values.
The resource version is the exact producing SkiaSharp **Build number**
(the full run name, including any `+...` suffix), **not the numeric build ID**.
Never omit this resource selection and accept latest.

```json
{
  "resources": {
    "repositories": {
      "self": {
        "refName": "<verified-publisher-production-ref>",
        "version": "<verified-publisher-commit-sha>"
      },
      "dotnet-release": {
        "refName": "<verified-tooling-production-ref>",
        "version": "<verified-tooling-commit-sha>"
      }
    },
    "pipelines": {
      "skiaSharpBuild": {
        "version": "<exact-producing-SkiaSharp-build-number>"
      }
    }
  },
  "templateParameters": {
    "validateOnly": false
  }
}
```

Use the tooling repository resource alias declared by the verified entry YAML
(`dotnet-release` above). Boolean `validateOnly: false` is real NuGet.org
production; `true` only validates and publishes nothing. This entry has no
MAUI `ghOwner`, `ghRepo`, `commitHash`, `pushNugetOrg`, `pushPackages`, or
NuGet-filter parameters: the pipeline resource selects the source.

```bash
az devops invoke \
  --organization https://dev.azure.com/dnceng \
  --area pipelines --resource runs --http-method POST \
  --route-parameters project=internal pipelineId=1715 \
  --api-version 7.1 --in-file publication.json --output json
```

Read back the admitted run, even if the immediate POST response lacks resolved
resources:

```bash
az devops invoke \
  --organization https://dev.azure.com/dnceng \
  --area pipelines --resource runs --http-method GET \
  --route-parameters project=internal pipelineId=1715 runId=<publisher-run-id> \
  --api-version 7.1 --output json
```

Match resolved publisher/tooling refs/SHAs and the selected producer Build
number. Read `resources.pipelines.skiaSharpBuild.pipeline.id` as the **producer
run ID**, not definition 1642 or the publisher run ID. Fetch that build:

```bash
az pipelines build show \
  --organization https://dev.azure.com/dnceng --project internal \
  --id <producer-run-id> --output json
```

Verify its definition is `skiasharp-package` (1642) and its Build number,
release branch, source SHA and BAR match the release record. Runs read-back
does not supply the producer branch/SHA. Also check `validateOnly`
semantically: the request is a JSON boolean, but read-back can be the string
`"false"`. Stop on mismatched resources or parameters.

Publish only `arcade_shipping_signed`. Unsigned `nuget`/`arcade_shipping`
and classic `Container` `PackageArtifacts` are not release inputs; the latter
also includes nonshipping transport packages. Derive expected
SkiaSharp/HarfBuzzSharp IDs and versions from the shipment source and shipping
filter, not a hardcoded package count.

Verify approval settings in the live entry and tooling. The current
configuration invokes isolated publisher 1697 through `dotnet-release`/ESRP
with parent `requireApproval: false`: no parent `ManualValidation`, but
protected ESRP scan/release approval still applies. These settings can change;
do not bypass approvals or change permissions.

Check parent, isolated publisher and ESRP task results, not aggregate status
alone. Independently verify every expected public ID/version and source
provenance. NuGet propagation can leave an incomplete public set after a
successful upload; continue monitoring the missing IDs without requeueing.
On failed or stalled publication, preserve source, producer/BAR,
parent/isolated run and ESRP IDs and report the missing packages. Do not retry
failed parent jobs that may already have queued an upload or switch publish
paths. A monitoring-only request does not authorize repairs.

Finish requires the complete public set and separate explicit authorization.
That authorization may be given beforehand for a conditional follow-up; it
does not remove the Finish plan and confirmation gates below.

## 5. Finish the public release

Run [Release - Finish](https://github.com/mono/SkiaSharp/actions/workflows/release-finish.yml)
on **`main`**, through Actions or the `release-publish` skill.

| Input | Value |
| --- | --- |
| `version` | Stable: `X.Y.Z[.F]`; preview/RC: `X.Y.Z[.F]-preview.N` / `-rc.N`, optionally with the exact `.BUILD` suffix |
| `push` | Unchecked for the plan; checked only after a separate confirmation |

A short prerelease identity must resolve to exactly one public version.
If none match, stop. If several match, provide the exact public version,
such as `4.153.0-preview.1.26453.1`.

Review the package-derived source branch/commit, exact tag, preceding shipment
for the changelog, release metadata, support update, follow-ups, and milestone
mutations. Confirm this plan before dispatching the push run.

Afterward, verify:

- the exact-version tag points to the public package's source commit;
- the GitHub Release is published with the correct prerelease state and
  changelog boundary;
- support state was already correct or a support PR was opened/updated;
- release-note generation was dispatched, and stable releases also dispatched
  the issue-template version update; and
- milestone reconciliation and date/rollover maintenance completed.

The Finish dry run plans milestones against a **virtual shipment**, without
creating a tag. Its push run requires the real tag to match the package commit
before milestone writes. A failed required milestone workflow fails Finish;
inspect and resume rather than replacing shipped state.

## 6. Complete follow-ups

Review and merge the version-bump, support, and release-notes/API-diff PRs
through the normal repository process. Automation does not merge them.

Finish normally reconciles milestones after every shipment. For independent
inspection or repair, run
[Release - Milestones](https://github.com/mono/SkiaSharp/actions/workflows/release-milestones.yml)
on `main`:

| Input | Value |
| --- | --- |
| `version` | Numeric core, such as `4.153.0` or `4.153.0.1`, for reconciliation |
| `reconcile` | Repair shipped PR/issue assignments |
| `update` | Repair dates, rollover, or closures |
| `push` | Unchecked for the plan; checked only after confirmation |

Resolve unsafe boundary/ownership warnings before writing. Missing exact
shipment milestones are planned or created before assignments. Issue discovery
and cadence rules are described in [the internals](release-process-internals.md#milestone-maintenance).

## Release record

Keep these values together until verification and follow-ups are complete:

| Identity | Record |
| --- | --- |
| Prepare | Requested identity and resolved base SHA |
| Source | SkiaSharp and mono/skia release branches and commits |
| Validation | Producing Build ID/URL/run number and matching Tests ID/URL |
| Artifacts | BAR ID and exact SkiaSharp/HarfBuzzSharp package versions |
| Publication | Dedicated parent/isolated run IDs/URLs, ESRP IDs, resolved publisher/tooling refs/SHAs, producer resource, `validateOnly`, and exact shipping set |
| Public release | Verified package set, exact tag, and GitHub Release URL |

## References

- [Release process internals](release-process-internals.md)
- [Versioning](versioning.md) and [Packages](packages.md)
- [Release notes and API diffs](release-notes-and-api-diffs.md)
