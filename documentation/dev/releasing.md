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

If a servicing branch is required, create `release/X.Y.x` from the intended
maintenance base before the stable Prepare `Push` run. Prepare does not create
that branch. When it exists, the post-stable version-bump PR targets it;
otherwise the PR targets `main`.

## 1. Prepare the release branches

Open
[Release - Prepare](https://github.com/mono/SkiaSharp/actions/workflows/release-prepare.yml),
select **Run workflow**, and choose `main` as the workflow branch.

| Input | Value |
| --- | --- |
| `base` | `main`, a servicing branch such as `release/4.152.x`, or an exact commit SHA |
| `release` | `X.Y.Z-preview.N`, `X.Y.Z-rc.N`, `X.Y.Z-stable`, or the equivalent four-part hotfix identity |
| `mode` | `DryRun` first; `Push` only after reviewing the plan |

Run the workflow twice with identical `base` and `release` values:

1. Run `DryRun`. Verify the resolved base SHA, package versions, branch names,
   and every planned remote write.
2. Run `Push`. Verify that both release branches were created and point to the
   expected commits.

`Apply` performs and validates local changes on the temporary GitHub runner but
does not push or retain them. It is optional between `DryRun` and `Push`; it is
mainly useful when running the script in a persistent local checkout.

| Release input | Branch created in both repositories |
| --- | --- |
| `X.Y.Z-preview.N` | `release/X.Y.Z-preview.N` |
| `X.Y.Z-rc.N` | `release/X.Y.Z-rc.N` |
| `X.Y.Z-stable` | `release/X.Y.Z` |
| `X.Y.Z.F-preview.N` | `release/X.Y.Z.F-preview.N` |
| `X.Y.Z.F-rc.N` | `release/X.Y.Z.F-rc.N` |
| `X.Y.Z.F-stable` | `release/X.Y.Z.F` |

The `-stable` suffix is deliberately required only for the Prepare input. It is
an explicit confirmation that prevents an accidental stable cut. The workflow
removes it from the branch identity and uses the internal
`PREVIEW_LABEL=stable` switch; stable packages, tags, and GitHub Releases keep
the bare numeric version.

The workflow pushes `mono/skia` first, then `mono/SkiaSharp`. A three-part
stable release also opens a human-owned PR that advances SkiaSharp and
HarfBuzzSharp to the next preview versions. Review and merge that PR normally;
the workflow does not merge it.

## 2. Wait for the release pipelines

Pushing the SkiaSharp `release/*` branch starts the internal pipeline chain
automatically. Do not manually queue a different build.

| Pipeline | What to verify |
| --- | --- |
| [`skiasharp-package` (1642)](https://dev.azure.com/dnceng/internal/_build?definitionId=1642) | Succeeded for the exact release branch and commit; record the Build run, exact package version, and BAR ID |
| [`skiasharp-tests` (1630)](https://dev.azure.com/dnceng/internal/_build?definitionId=1630) | Succeeded and was pipeline-triggered from that exact `skiasharp-package` run |

The public CI pipeline may also run for the branch. Its unsigned artifacts are
not the release BAR. API Scan is not automatically enabled for a `release/*`
build, so it is not an expected release-branch stage.

Do not continue if the Build and Tests runs disagree on branch, commit, build
number, or upstream pipeline resource.

## 3. Optional: approve the exact BAR package set

This extra package validation is optional. To run it, use the repository's
`release-testing` skill on each desired host with this copy-pasteable prompt:

```text
Use the release-testing skill to validate SkiaSharp {exact CI package version} from BAR {BAR ID}. Run the full available matrix on this host and produce the release approval report.
```

If you run this gate, keep its report with the BAR ID, Build link, source branch
and commit, exact package versions, feed, and test results. Record the decision
if this optional step is skipped.

## 4. Publish the BAR to NuGet.org

> **TODO:** Document the exact internal Maestro page, button, fields, required
> permissions, and approval sequence used to publish the selected BAR to
> NuGet.org.

Until that UI is documented, use the current team-owned protected publication
procedure. Confirm the selected BAR ID, Build run, source branch and commit,
and package versions. If optional release-testing was run, require them to
match its approval report exactly.

After publication completes, verify that the exact SkiaSharp package version
and its expected shipping package family are visible on NuGet.org. Do not run
Release - Finish before that verification.

## 5. Finish the public release

Open
[Release - Finish](https://github.com/mono/SkiaSharp/actions/workflows/release-finish.yml),
select **Run workflow**, and choose `main` as the workflow branch.

| Input | Value |
| --- | --- |
| `version` | Stable: `X.Y.Z[.F]`. Prerelease: either `X.Y.Z[.F]-preview.N` / `-rc.N`, or the exact public version with its appended `.BUILD` |
| `mode` | `DryRun` first, then `Push` with the same version |

A short prerelease identity and an exact public prerelease version are equally
valid. Most releases have only one matching public build, so
`4.153.0-preview.1` is usually sufficient. When there are zero or multiple
matches, provide the exact version such as `4.153.0-preview.1.26453.1`.

Review the `DryRun` source branch, source commit, tag, release title, support
update, and follow-up workflows. Then run `Push` and verify:

- the immutable exact-version tag points to the package's source commit;
- the GitHub Release is published with the correct prerelease state;
- the support state was already correct or the release-support PR was opened
  or updated; and
- release-note generation was dispatched.

Stable releases also dispatch the issue-template version update.

## 6. Reconcile and advance milestones

After Release - Finish has created the shipped tag, open
[Release - Milestones](https://github.com/mono/SkiaSharp/actions/workflows/release-milestones.yml),
select **Run workflow**, and choose `main` as the workflow branch.

| Input | First run | Apply run |
| --- | --- | --- |
| `version` | Numeric release core, such as `4.153.0` or `4.153.0.1` | Same value |
| `reconcile` | `true` | `true` |
| `update` | `true` | `true` |
| `push` | `false` | `true` |

Run this after previews and RCs as well as stable releases. Review the read-only
plan before the `push: true` run. Warnings about missing tags, milestones, or
release boundaries block safe mutation and must be resolved rather than
ignored.

## 7. Complete the follow-up pull requests

Review and merge the automation PRs through the normal repository process:

- the post-stable version-bump PR, when one was created;
- the release-support PR, when one was needed; and
- the generated release-notes/API-diff PR, when one was opened or updated.

The issue-template version workflow also runs daily, so a stable release's
manual dispatch is convergent with the scheduled update.

## Release record

Keep these values together for the whole release:

| Identity | Record |
| --- | --- |
| Requested release identity | `X.Y.Z[.F][-channel.N]` |
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
