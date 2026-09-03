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
| 4 | Use the protected internal publication process | Publishes the selected BAR's shipping packages to NuGet.org |
| 5 | Run **Release - Finish** | Creates the tag and GitHub Release, then starts follow-up automation |
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
inputs and `push` checked.

## 1. Prepare the release branches

Open
[Release - Prepare](https://github.com/mono/SkiaSharp/actions/workflows/release-prepare.yml),
select **Run workflow**, and choose `main` as the workflow branch.

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

> **TODO:** Document the exact internal Maestro page, button, fields, required
> permissions, and approval sequence used to publish the selected BAR to
> NuGet.org.

Until that UI is documented, use the current team-owned protected publication
procedure. Confirm the BAR, Build run, source branch and commit, and package
versions against the release record. If optional release-testing was run, they
must match its approval report exactly.

After publication completes, verify that the exact SkiaSharp package version
and its expected shipping package family are visible on NuGet.org. Do not run
Release - Finish before that verification.

## 5. Finish the public release

Open
[Release - Finish](https://github.com/mono/SkiaSharp/actions/workflows/release-finish.yml),
select **Run workflow**, and choose `main` as the workflow branch.

| Input | Value |
| --- | --- |
| `version` | Stable: `X.Y.Z[.F]`.<br>Prerelease: `X.Y.Z[.F]-preview.N` / `-rc.N`, optionally with the exact `.BUILD` suffix |
| `push` | Use the two-dispatch pattern above |

Both prerelease forms are valid. A short identity such as
`4.153.0-preview.1` is usually sufficient. If no public build matches, stop. If
multiple builds match, use the exact version, such as
`4.153.0-preview.1.26453.1`.

Review the plan's source branch, source commit, tag, release title, support
update, and follow-up workflows. After the push run, verify:

- the immutable exact-version tag was created or verified at the package's
  source commit;
- the GitHub Release is published with the correct prerelease state;
- the support state was already correct or the release-support PR was opened
  or updated; and
- release-note generation was dispatched.

Stable releases also dispatch the issue-template version update.

## 6. Reconcile and advance milestones

After Release - Finish has created or verified the shipped tag, open
[Release - Milestones](https://github.com/mono/SkiaSharp/actions/workflows/release-milestones.yml),
select **Run workflow**, and choose `main` as the workflow branch.

| Input | Value |
| --- | --- |
| `version` | Numeric release core, such as `4.153.0` or `4.153.0.1` |
| `reconcile` | Checked |
| `update` | Checked |
| `push` | Use the two-dispatch pattern above |

Run this after previews and RCs as well as stable releases. Warnings about
missing tags, milestones, or release boundaries block safe mutation and must
be resolved rather than ignored.

The maintained cadence follows Chromium's overlapping two-week trains:

| Stage | Target date |
| --- | --- |
| Preview 1 | One day after Chromium branch point |
| RC 1 | Chromium stable cut |
| Stable | One day after Chromium Stable |

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
