# Release Guide

SkiaSharp releases use two repository-owned GitHub Actions workflows around the
team-owned Azure DevOps publication pipeline:

```text
Release - Prepare -> dnceng build/test/publish -> Release - Finish
```

The GitHub workflows own repository state. Azure DevOps, Arcade, Darc, and
Maestro own package state.

## Irreversible operations

Release refs, tags, public package versions, and published GitHub Releases are
treated as immutable.

- Never force-update an existing release ref.
- Never move or delete a release tag.
- Never replace a conflicting published GitHub Release.
- NuGet.org packages cannot be replaced; publish a new version to recover.

Every GitHub write follows a read-only plan and a protected environment
approval. Rerunning a workflow reconciles matching partial state instead of
duplicating it.

The workflows build `utils/SkiaSharp.ReleaseTool` from the exact audited
checkout using `global.json` and locked NuGet dependencies. The standalone C#
CLI uses source-generated `System.Text.Json`, NuGet SDK APIs for public package
receipts and signatures, and Octokit for GitHub reads and writes. It does not
bootstrap or download native Skia artifacts and does not invoke the `gh` CLI.

## Required repository configuration

Configure these protected GitHub environments:

| Environment | Protects |
|-------------|----------|
| `release-branching` | Creating maintenance/exact release refs and bump PRs |
| `release-tag` | Pushing the immutable package-source tag and creating a draft |
| `release-publish` | Publishing the reviewed GitHub Release draft |

Each environment must:

- require named reviewers;
- prevent self-review;
- allow deployments from the default branch only.

Each protected job verifies this configuration before loading a write token.
GitHub otherwise auto-creates an unknown environment without protection, so a
missing or misconfigured environment is a hard failure.

Until the release GitHub App is available, approved write jobs use the
`SKIASHARP_AUTOBUMP_TOKEN` repository secret. The token can write branches in
both `mono/SkiaSharp` and `mono/skia`. It is an existing repository-level
credential and is not fully isolated by environments; the workflows merely
reference it only after approval for branch/tag/release writes. Closeout also
uses it without another approval so its workflow dispatch is attributed to a
maintainer rather than `github-actions[bot]`; that path first requires the exact
tag and published release and can only change milestones or dispatch convergent
workflows. Replacing the broad token with a narrowly scoped GitHub App remains
the required long-term hardening and must not change release logic.

Release checkouts never persist credentials. Approved branch/tag jobs expose
the PAT to a temporary noninteractive `GIT_ASKPASS` helper only while the C#
command runs, then remove the helper. Octokit uses the same approved PAT for the
cross-repository `mono/skia` Prepare ref and other GitHub writes; environment
checks use only job-scoped `github.token`.

GitHub returns draft releases only to callers with push access. The two
read-only Finish planning jobs therefore receive job-scoped `contents: write`
on `github.token` so they can discover and validate resumable drafts. Those
jobs contain no mutation commands; the broader cross-repository token remains
confined to environment-approved write jobs.

## System boundaries

| System | Responsibility |
|--------|----------------|
| GitHub `Release - Prepare` | Version planning, maintenance/exact refs, bump PR |
| dnceng Build 1642 | Native/managed build, signing, API Scan, BAR registration |
| dnceng Tests 1630 | Connected tests for the exact Build run |
| Arcade/Darc/Maestro | BAR validation and configured feed promotion |
| Team release pipeline | Protected publication to NuGet.org |
| GitHub `Release - Finish` | Public-package verification, tag, release, closeout |
| Release-notes workflow | Reviewed website prose and delayed GitHub summary |

Public CI definition 345 validates repository changes. It is not release
publication evidence.

## Step 1: Prepare

Run **Release - Prepare** from the default branch in the Actions tab.

Inputs:

| Input | Value |
|-------|-------|
| `target` | `main` or an exact maintenance branch such as `release/4.151.x` |
| `release_version` | Exact branch version, or empty to detect the next preview |

The read-only plan determines:

- the exact release type and version;
- the integration/base commit;
- the Skia gitlink commit and matching `mono/skia` ref;
- the maintenance branch and exact release branch;
- version-file changes;
- any stable post-cut bump PR;
- operations already completed or blocked.

The plan records the exact tooling, base, and Skia commits. The
`release-branching` job waits for approval, checks out that exact tooling
revision, revalidates the refs, and then applies the plan. Every plan carries a
new `planId` GUID. The workflow exports it separately and supplies
`--expected-plan-id` to every write or derived-plan command, so an artifact from
another run cannot be substituted.

### Branch types

Integration targets:

```text
main
release/X.Y.x
```

Exact immutable release refs:

```text
release/X.Y.Z-preview.N
release/X.Y.Z-rc.N
release/X.Y.Z
release/X.Y.Z.F-preview.N
release/X.Y.Z.F-rc.N
release/X.Y.Z.F
```

The first prerelease for a new line creates `release/X.Y.x` from the audited
main commit before creating the exact release ref. Later prereleases and the
stable release use that maintenance line. A stable release never invents a
missing maintenance base silently.

Every exact SkiaSharp release ref has a matching `mono/skia` ref at the existing
Skia gitlink commit.

### Stable bump PR

A stable cut plans and opens a PR that advances the maintenance integration
branch to its next patch version and resets `PREVIEW_LABEL` to `preview.0`.
Branch protection and a maintainer own the merge; release automation never
auto-merges it.

### Prepare output

The final summary contains:

- the plan GUID and next recovery action;
- release and maintenance branches;
- exact SkiaSharp base and Skia commits;
- every planned or reconciled operation;
- stable bump PR state and URL when applicable;
- warnings.

For previews and RCs, the final public package version is not known until the
build revision is assigned. Prepare reports the base plus label, not a guessed
package version.

## Step 2: Build, test, and publish packages

Pushing the exact release ref starts the normal dnceng build and connected test
flow.

1. Build 1642 builds, signs, and registers one Arcade BAR.
2. Tests 1630 runs against that exact build.
3. Run and approve the team-owned release pipeline.
4. Wait for the public packages to appear on NuGet.org.
5. Optionally run the release smoke-testing skill against that exact public
   version before announcing it.

GitHub does not queue or approve the team pipeline. The protected team pipeline
remains the authority for package publication.

## Step 3: Finish

After publication, run **Release - Finish** from the default branch.

Input:

| Input | Value |
|-------|-------|
| `release_version` | Exact public SkiaSharp version on NuGet.org |

No private BAR or Azure run ID is required. NuGet.org is the public release
receipt.

### Public package verification

Finish:

1. Resolves the exact NuGet registration/catalog entry.
2. Requires the package to be listed.
3. Verifies catalog SHA512 metadata.
4. Downloads and signature-verifies the anchor packages.
5. Reads source repository metadata from the hash/signature-verified nuspec and
   version/dependency metadata from the public package records.
6. Fetches the SkiaSharp commit embedded in the public package.
7. Validates version files at that commit.
8. Verifies the complete public package family declared by `VERSIONS.txt` at
   that commit, so packages introduced on newer branches are not imposed on an
   older release.

The tag target is the source commit embedded in the published `SkiaSharp`
package. If the release branch has advanced, Finish reports the difference and
still plans to tag the package commit. If SkiaSharp-family packages disagree on
their source commit, finalization stops.

HarfBuzzSharp packages may come from an older SkiaSharp commit when their base
version was reused. Finish validates their expected version and dependency
relationship but does not require their source commit to equal the SkiaSharp
commit.

### Public version composition

`scripts/VERSIONS.txt` contains package base versions and
`scripts/azure-templates-variables.yml` contains `PREVIEW_LABEL`.

| Label | Public form |
|-------|-------------|
| `stable` | `X.Y.Z` or `X.Y.Z.F` |
| `preview.N` | `{base}-preview.N.{buildRevision}` |
| `rc.N` | `{base}-rc.N.{buildRevision}` |

The same label/build revision is applied to the SkiaSharp and HarfBuzzSharp base
versions. Finish validates this composition rather than comparing a prerelease
version directly with the base in `VERSIONS.txt`.

### Tag and draft approval

The first Finish plan shows:

- every required public package and version;
- the package source commit and release branch;
- the exact tag and previous release tag;
- tag/draft operations and conflicts.

The `release-tag` environment then gates:

- pushing the tag directly to the public package commit;
- creating or reconciling a GitHub draft;
- preserving GitHub-generated notes inside managed markers.

An existing matching tag/draft is success. An existing tag or release pointing
to another commit is a blocking conflict.

### Publication approval

After draft creation, a second read-only job downloads the actual remote draft
and reports its URL, body hash, tag target, prerelease state, and closeout
effects.

The `release-publish` environment gates publication of that exact draft.
The approved publication artifact contains the remote draft body hash; the write
job fails if the draft body or managed-marker state changed while approval was
pending. It also carries a distinct `publicationPlanId` GUID; publication
requires both `--expected-plan-id` and `--expected-publication-plan-id`.

### Automatic closeout

After publication, Finish:

- creates or updates the next three preview/RC/stable milestones from the
  Chromium/Skia schedule;
- reconciles shipped PRs and linked issues;
- moves open milestone items to the next unshipped milestone;
- closes the shipped milestone after propagation checks;
- dispatches targeted release-note generation;
- refreshes issue-template release choices;
- reports, but never merges, the stable bump PR.

Closeout is idempotent. Ambiguous milestone boundaries stop without changing
unrelated milestones.

## Release summaries

The initial GitHub Release is published with GitHub-generated notes and an empty
managed summary region. This intentionally avoids waiting for AI prose on the
release critical path.

The **Sync - Release Notes & API Diffs** workflow later:

1. deterministically regenerates release facts;
2. asks the release-notes skill to write consumer-facing prose;
3. opens a normal review PR.

After that PR merges, **Update GitHub Release summaries** replaces only the
managed summary region. It never rewrites GitHub-generated notes or updates an
unmarked historical release.

## Smoke testing

Release smoke testing remains an optional skill because it involves host/device
setup, screenshots, and human judgment.

It consumes an exact public NuGet version.

It never selects a newer package implicitly. If smoke testing becomes a
mandatory machine-enforced gate, move its result into CI or the team pipeline
rather than relying on local skill state.

## Recovery

Both workflows are reconciliation based:

- rerun Prepare with the same target/version to recover partial branch or PR
  creation;
- rerun Finish with the same public version after NuGet indexing completes;
- matching branches, PRs, tags, drafts, releases, summaries, and milestone state
  are reported as done;
- conflicting immutable state is blocked.

The C# CLI exposes the same plan/apply operations for diagnostics. Build the
exact checkout first:

```text
dotnet restore utils/SkiaSharp.ReleaseTool/SkiaSharp.ReleaseTool.csproj --locked-mode
dotnet build utils/SkiaSharp.ReleaseTool/SkiaSharp.ReleaseTool.csproj --configuration Release --no-restore
dotnet run --no-build --no-restore --configuration Release \
  --project utils/SkiaSharp.ReleaseTool/SkiaSharp.ReleaseTool.csproj -- \
  finish plan --version 4.152.0 --output finish-plan.json \
  --summary finish-plan.md
```

Producing commands accept `--summary <path>` and write their typed Markdown
summary directly. Never edit artifact JSON; regenerate it from current state.
Recovery commands that consume a plan require both its exact `planId` and the
lowercase SHA256 of its raw JSON bytes via `--expected-plan-sha256`.
Publication additionally requires the exact `publicationPlanId` and
`--expected-publication-sha256`. The workflows calculate these digests before
upload and propagate them independently of the artifacts.

The two JSON files retained under `scripts/infra/release/` are runtime policy
inputs for the C# receipt verifier: the public package anchors and trusted
signing certificates. They are not Python release tooling.

## Related documentation

- [Versioning](versioning.md)
- [NuGet packages](packages.md)
- [Release notes and API diffs](release-notes-and-api-diffs.md)
- [Memory management](memory-management.md)
