# Release Guide

This is the maintainer runbook for shipping SkiaSharp. It lists the workflows
to run, the values to enter, and the state to verify. For the design and
implementation of the automation, see
[Release process internals](release-process-internals.md).

## Process at a glance

| Step | Action | Result |
| --- | --- | --- |
| 1 | Run **Release - Prepare** | Creates the paired `mono/skia` and `mono/SkiaSharp` release branches |
| 2 | Wait for `skiasharp-package`, then `skiasharp-tests` | Produces the signed BAR and validates the exact Build pipeline resource |
| 3 | Optionally run `release-testing` | Adds host/device validation for the selected BAR |
| 4 | On request, queue the MAUI official release pipeline; approve after inspecting its audit | Publishes the selected BAR's shipping packages to NuGet.org |
| 5 | After all packages are public, review **Release - Finish** and confirm | Creates the tag and GitHub Release, then starts follow-up automation |
| 6 | Run **Release - Milestones** | Reconciles shipped work and advances release milestones |
| 7 | Merge the follow-up PRs | Lands any version bump, support update, and release notes |

## Safety

Release refs, package versions, tags, and published GitHub Releases are
immutable.

- Never force-update an existing release branch.
- Never move or delete a release tag.
- Never replace a published NuGet.org package version.
- Never substitute a different build, BAR, feed, or package after testing.
- Stop when existing remote state conflicts with the requested release.

## Before starting

Decide:

- the release identity, such as `4.153.0-preview.1`, `4.153.0-rc.1`, or
  `4.153.0-stable`;
- the exact source branch or commit to release; and
- whether a stable line needs a long-lived `release/X.Y.x` servicing branch.

> [!NOTE]
> The `-stable` label is a Prepare-only safety sentinel. It will not appear in
> package versions, branch names, tags, or GitHub Releases.

If a servicing branch is required, create `release/X.Y.x` from the intended
maintenance base before the stable Prepare `Push` run. Prepare does not create
that branch. When it exists, the post-stable version-bump PR targets it;
otherwise the PR targets `main`.

Prepare, Finish, and Milestones use the same two-dispatch pattern: first run
with `push` unchecked to review a read-only plan, then run again with identical
inputs and `push` checked. Package publication uses a different control: one
MAUI pipeline run prepares the packages and pauses at its human NuGet approval
gate. Neither Prepare nor a successful Build/Tests run queues publication
automatically.

## Audit a release line

Use the focused, read-only summary for one major/minor line:

```powershell
pwsh ./scripts/infra/publishing/audit-release-state.ps1 -Version 4.152
```

The command accepts exactly `A.B`, with optional `-Json`. It selects
`release/A.B.x` as maintenance when present; otherwise it uses `main` only when
its checked-in SkiaSharp version belongs to that line. It lists real specific
release branches and every exact public SkiaSharp package version, including
multiple prerelease builds. It checks each public shipment's NuGet provenance,
exact tag, and GitHub Release. It also checks the expected incoming Skia sync
branch and open pull request (`skia-sync/release-A.B.x` for a servicing line or
`skia-sync/mMILESTONE` for the current line on `main`). The separate
`skia-sync/main` branch follows the bleeding-edge Google Skia `main` tip and is
not the milestone sync used to decide release readiness. A non-draft incoming
milestone PR is shown as work to merge before cutting the next release; a draft
PR is visible but does not block the release recommendation.

The audit also mirrors the Skia sync detector's upstream work check. It compares
the exact `chrome/mMILESTONE` head with the existing mono/skia sync branch when
one exists, otherwise with `release/A.B.x` in mono/skia or `skiasharp` for the
current line. New upstream commits are reported with their count and suppress a
new release-cut recommendation until the milestone sync workflow is run and its
PRs are merged. Existing publication and Finish work remains actionable.

The immediately following line is also recognized before it becomes active on
`main`. For example, while `main` is still 4.154/m154, auditing `4.155` reports
an existing `skia-sync/m155` pull request or available `chrome/m155` work
instead of returning an empty line. A pending milestone PR must complete before
release preparation for that line can begin.

Invoke the `release-audit` skill rather than the script directly when auditing
several lines or deciding what to do next. The skill expands a range such as
`4.150-4.155` or discovers a prefix such as `4.15*`, runs the line audits in
parallel, and aggregates their findings. Each script invocation already checks
exact-tip health from the internal `skiasharp-package` pipeline and reports the
build ID and BAR ID. A release branch is publication-ready only when its
matching build succeeded and recorded one BAR ID; a new cut is ready only when
the maintenance tip has the same evidence and upstream Skia is current. Among
builds on the branch's current commit, the audit selects the newest succeeded
build with one BAR even if a later rerun failed or was canceled, and reports
those newer attempts. A partially succeeded build with a BAR requires review;
it is not automatically publication-ready. If the
internal pipeline cannot be reached, the script preserves the other evidence,
reports the unavailable build check, and exits `2` rather than guessing.

The report recommends only the next owner action: protected BAR-to-NuGet
publication, a Finish read-only plan, or a Prepare read-only plan. In chat,
use the `release-publish` or `release-branch` skill to dispatch the corresponding
pipeline or workflow; repository PowerShell scripts are the local fallback.
The audit intentionally does not inspect BAR details, release notes, support
metadata, milestone assignments, or milestone maintenance; those remain owned
by their detailed workflows. Exit `0` means no action is needed, `1` means
release work remains, and `2` means a required remote service or tool was
unavailable.

Interactive output uses PowerShell's aligned table formatting and host-aware
emphasis. Redirected output is plain text, and `-Json` remains undecorated.

## 1. Prepare the release branches

Use the `release-branch` skill in chat to dispatch
[Release - Prepare](https://github.com/mono/SkiaSharp/actions/workflows/release-prepare.yml)
on `main`; the Actions **Run workflow** button is also available.

| Input | Value |
| --- | --- |
| `base` | `main`, a servicing branch such as `release/4.152.x`, or an exact commit SHA |
| `release` | `X.Y.Z-preview.N`, `X.Y.Z-rc.N`, `X.Y.Z-stable`, or the equivalent four-part hotfix identity |
| `push` | Use the two-dispatch pattern above |

> [!NOTE]
> For a stable release, enter the explicit `X.Y.Z-stable` sentinel. The
> resulting branch, package version, tag, and GitHub Release use bare `X.Y.Z`.

Review the plan's base SHA, package versions, branch names, and remote writes.
After the push run, verify that both release branches exist at the expected
commits.

Both repositories use `release/<identity>`, with `-stable` removed. For example,
`4.153.0-preview.1` creates `release/4.153.0-preview.1`, while
`4.153.0-stable` creates `release/4.153.0`. Four-part hotfixes follow the same
rule.

The workflow pushes `mono/skia` first, then `mono/SkiaSharp`. A three-part
stable release also ensures that maintenance advances to the next SkiaSharp and
HarfBuzzSharp preview versions. It creates or reuses a human-owned bump PR
unless the target branch is already advanced; the workflow never merges it.

## 2. Wait for the release pipelines

Pushing the SkiaSharp `release/*` branch starts the internal pipeline chain
automatically. Do not manually queue a different build.

| Pipeline | What to verify |
| --- | --- |
| [`skiasharp-package` (1642)](https://dev.azure.com/dnceng/internal/_build?definitionId=1642) | Succeeded for the exact release branch and commit; record the Build run, exact package version, and BAR ID |
| [`skiasharp-tests` (1630)](https://dev.azure.com/dnceng/internal/_build?definitionId=1630) | Succeeded and was pipeline-triggered from that exact `skiasharp-package` run |

The public CI pipeline may also run for the branch. Its unsigned artifacts are
not the release BAR.

Do not continue if the Build and Tests runs disagree on branch, commit, build
number, or upstream pipeline resource.

## 3. Optional: approve the exact BAR package set

This extra package validation is optional. To run it, use the repository's
`release-testing` skill on each desired host with this copy-pasteable prompt:

```text
Use the release-testing skill to validate SkiaSharp {exact CI package version}
from BAR {BAR ID}. Run the full available matrix on this host and produce the
release approval report.
```

Add the resulting approval report, or the decision to skip this step, to the
release record below.

## 4. Publish the BAR to NuGet.org

When you say **"push the packages"** in chat, the `release-publish` skill
checks the release branch's exact-tip Build, the matching resource-triggered
Tests run, BAR, and any existing publication run. It then queues the
[`dotnet-maui-release` internal pipeline](https://dev.azure.com/dnceng/internal/_build?definitionId=1445)
on the **merged and mirrored MAUI `main`**, using that branch's current tip. This
requires the SkiaSharp support in `eng/pipelines/ci-official-release.yml` to
be available on that internal ref (introduced by
[dotnet/maui#38967](https://github.com/dotnet/maui/pull/38967)). If the
internal mirror is behind GitHub, wait for it to catch up. Confirm that
pipeline 1445's configured default branch is `refs/heads/main`; stop if it
is not. Do not queue a normal release from an unmerged feature branch.
An already-started run must be resumed and monitored, not duplicated; the
read-only SkiaSharp audit does not track MAUI publication runs.

The chat dispatch uses these template parameters (the example commit must be
replaced by the exact **SkiaSharp release branch** commit, not the mono/skia
submodule SHA):

```bash
az pipelines run \
  --organization https://dev.azure.com/dnceng --project internal --id 1445 \
  --parameters \
    ghOwner=mono \
    ghRepo=SkiaSharp \
    commitHash=<exact-SkiaSharp-release-commit> \
    pushWorkloadSet=false \
    pushNugetOrg=true \
    pushPackages=true \
    nugetIncludeFilters=skip \
    nugetExcludeFilters=skip \
  --output json
```

The pipeline's default branch selects the MAUI main tip; `commitHash` selects
the separate SkiaSharp BAR commit. The agent reads back the queued run's
resolved MAUI source ref/SHA and parameters, requires `refs/heads/main`,
verifies that SHA contains the SkiaSharp release support, then compares the
`NuGetReleaseAudit` artifact with the release record: BAR ID, repository,
commit, selected and staged shipping package identities. The pipeline does
not need a separate preparation-only run: the real run prepares its packages
and pauses at `ManualValidation`. Inspect the audit before a human resumes
the gate. The protected `1ES.PublishNuget` job and
`nuget.org (dotnetframework)` service connection cannot run until approval.
The approver must confirm package ownership and quota as well as the audit.
The agent must not approve merely because preparation succeeded.

After approval, monitor the **same run** and verify every exact staged
shipping ID/version on NuGet.org; partial visibility or a successful dispatch
does not constitute completion. If publication fails or is partial, preserve
the run and package evidence and do not automatically queue a second run.
Only after the full package set is public should Finish be planned, and
Finish still needs a separate confirmation.

## 5. Finish the public release

Use the `release-publish` skill in chat to dispatch
[Release - Finish](https://github.com/mono/SkiaSharp/actions/workflows/release-finish.yml)
on `main`, after verifying the full public package set. The Actions
**Run workflow** button is also available.

| Input | Value |
| --- | --- |
| `version` | Stable: `X.Y.Z[.F]`.<br>Prerelease: `X.Y.Z[.F]-preview.N` / `-rc.N`, optionally with the exact `.BUILD` suffix |
| `push` | Use the two-dispatch pattern above |

Both prerelease forms are valid. A short identity such as
`4.153.0-preview.1` is usually sufficient. If no public build matches, stop. If
multiple builds match, use the exact version, such as
`4.153.0-preview.1.26453.1`.

Review the plan's source branch, source commit, tag, release title, support
update, follow-up workflows, and planned milestone mutations. Ask for
confirmation **after** showing that plan; do not dispatch `push=true` just
because packages appeared. After the push run, verify:

- the immutable exact-version tag was created or verified at the package's
  source commit;
- the GitHub Release is published with the correct prerelease state and its
  generated changelog starts at the preceding exact shipment shown in the plan;
- the support state was already correct or the release-support PR was opened
  or updated; and
- release-note generation was dispatched; and
- milestone reconciliation plus date/rollover maintenance completed after
  verifying the immutable tag points to the public package's source commit.

Stable releases also dispatch the issue-template version update.

## 6. Repair milestones when needed

Release - Finish already reconciles and advances milestones for every completed
preview, RC, stable, patch, and hotfix. To inspect or repair milestone state
separately, open
[Release - Milestones](https://github.com/mono/SkiaSharp/actions/workflows/release-milestones.yml),
select **Run workflow**, and choose `main` as the workflow branch.

| Input | Value |
| --- | --- |
| `version` | Numeric release core when reconciling, such as `4.153.0` or `4.153.0.1` |
| `reconcile` | Select only when repairing shipped PR/issue assignments |
| `update` | Select only when repairing dates, rollover, or closures |
| `push` | Use the two-dispatch pattern above |

Missing exact shipped-release milestones are planned or created automatically
before assignments. Warnings about missing tags, release boundaries, or
ambiguous ownership still block safe mutation and must be resolved rather than
ignored.

The maintained cadence follows Chromium's overlapping two-week trains. This
M153/M154 example shows each offset from its Chromium branch point; the
Chromium marker appears before the corresponding SkiaSharp release:

| Date | M153 day | M153 | M154 day | M154 |
| --- | ---: | --- | ---: | --- |
| Aug 17 | 0 | Branch Point | | |
| Aug 19 | 2 | Earliest Beta → Preview 1 | | |
| Aug 25 | 8 | Early Stable Cut → Preview 2 | | |
| Aug 31 | | | 0 | Branch Point |
| Sep 1 | 15 | Stable Cut → RC 1 | | |
| Sep 2 | | | 2 | Earliest Beta → Preview 1 |
| Sep 8 | 22 | Stable Date → Stable | 8 | Early Stable Cut → Preview 2 |
| Sep 15 | | | 15 | Stable Cut → RC 1 |
| Sep 22 | | | 22 | Stable Date → Stable |

## 7. Complete the follow-up pull requests

Review and merge the automation PRs through the normal repository process:

- the post-stable version-bump PR, when one was created;
- the release-support PR, when one was needed; and
- the generated release-notes/API-diff PR, when one was opened or updated.

## Release record

Keep these values together for the whole release:

| Identity | Record |
| --- | --- |
| Requested Prepare identity | `X.Y.Z[.F]-preview.N`, `-rc.N`, or `-stable` |
| SkiaSharp release branch and commit | `release/...` at SHA |
| mono/skia release branch and commit | `release/...` at SHA |
| `skiasharp-package` run | Build ID and URL |
| `skiasharp-tests` run | Build ID and URL |
| BAR | BAR ID |
| Packages | Exact SkiaSharp and HarfBuzzSharp versions |
| Optional test approval | Combined report or recorded skip decision |
| Public release | NuGet version, tag, and GitHub Release URL |

## Related documentation

- [Release process internals](release-process-internals.md)
- [Versioning](versioning.md)
- [Packages](packages.md)
- [Release notes and API diffs](release-notes-and-api-diffs.md)
