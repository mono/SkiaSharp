---
name: release-publish
description: >
  Publish SkiaSharp release packages to NuGet.org from chat and finish public
  releases. Use when a maintainer says "push the packages", "publish the BAR",
  "resume publication", "finalize X", "tag X", or "finish release X". Queue the
  protected MAUI pipeline only on explicit request; after exact packages are
  public, use the SkiaSharp Release - Finish workflow with a separate
  confirmation.
---

# Release Publish

Use this skill for **two separate chat requests**: "push the packages" queues
the MAUI publication pipeline; "finish the release" dispatches the SkiaSharp
Finish workflow after the packages are public. Neither runs automatically
after Prepare, Build/Tests, or the other operation.

## Push the packages

Before queuing, run `audit-release-state.ps1 -Version A.B -Json` and select
the requested immutable `release/<identity>` branch. Require its exact-tip
`skiasharp-package` build (1642) to have succeeded with one BAR ID. Require
`skiasharp-tests` (1630) to have succeeded for the same branch, commit and
build number, with `triggerInfo.pipelineId` pointing to that package build.
Use `az pipelines runs list/show` to check those test details; do not accept
an unrelated manual test run. Any optional `release-testing` approval must
match the same BAR. Never substitute the maintenance tip or mono/skia SHA
for the SkiaSharp release commit.

Check the exact public version and existing MAUI release runs (1445) before
dispatch: use `az pipelines runs list/show` and compare their
`templateParameters.commitHash` with the selected SkiaSharp commit. If a
matching run already exists, monitor it instead of creating another; if the
packages are already public, verify provenance and proceed to Finish. Stop
on ambiguous or mismatched evidence.

For testing this unmerged skill, pin the MAUI pipeline to the verified feature
ref and SHA below. **Before merging this skill**, merge dotnet/maui#38967,
remove this temporary pin, and use the exact current SHA of the internal
MAUI `main` ref instead. Never treat the feature ref as the normal release path.

Queue `dotnet-maui-release` from chat only when explicitly requested:

```bash
az pipelines run \
  --organization https://dev.azure.com/dnceng --project internal \
  --id 1445 \
  --branch refs/heads/mattleibow-skiasharp-release-support \
  --commit-id d095037c93e035606d728b28ebd0d82dd94997b0 \
  --parameters ghOwner=mono ghRepo=SkiaSharp \
    commitHash=<exact-SkiaSharp-release-commit> \
    pushWorkloadSet=false pushNugetOrg=true pushPackages=true \
    nugetIncludeFilters=skip nugetExcludeFilters=skip \
  --output json
```

`--commit-id` pins the **MAUI pipeline code**; `commitHash` identifies the
**SkiaSharp BAR commit**, not the mono/skia SHA. Read back the created run's
source ref/SHA and template parameters; stop on any mismatch without blindly
retrying. The real run prepares packages and pauses at `ManualValidation`, so
no separate dry run is required. Compare its `NuGetReleaseAudit` BAR,
repository, commit, and selected/staged package identities with the release
record before asking a human to approve the protected NuGet push. The human
must also confirm ownership and quota. Do not approve merely because
preparation succeeded.

After approval, monitor the same run and independently verify **every**
staged shipping ID/version on NuGet.org. On failure or partial publication,
preserve the evidence and stop; never automatically requeue or run Finish.

## Finish after the packages are public

Once the complete exact package set is public, use **Release - Finish** on
`main`, first with `push=false` to inspect the read-only plan. Resolve an
abbreviated prerelease identity to one exact public version; stop if ambiguous.
Present the source commit, exact tag, release title, support updates and
milestone changes to the user and obtain **separate confirmation** before
dispatching the *same* inputs with `push=true`:

```bash
gh workflow run release-finish.yml --repo mono/SkiaSharp --ref main \
  -f version=4.153.0-preview.1 -f push=false

# After the plan is reviewed and separately approved:
gh workflow run release-finish.yml --repo mono/SkiaSharp --ref main \
  -f version=4.153.0-preview.1 -f push=true
```

Inspect both workflow runs; do not treat dispatch as success. Finish verifies
the public package's branch and commit, publishes the exact tag and GitHub
Release, and coordinates support, release notes, and milestones. Rerun the
read-only audit afterward. Never move a tag or replace a public package.

## Local fallback

Only when the GitHub workflow is unavailable or local execution is explicitly
requested, use the repository-owned Finish script:

```powershell
./scripts/infra/publishing/finish-release.ps1 -Version 4.153.0-preview.1 -Mode DryRun

# After reviewing the plan and receiving confirmation:
./scripts/infra/publishing/finish-release.ps1 -Version 4.153.0-preview.1 -Mode Push
```

Local Finish does **not** replace the protected MAUI package pipeline or its
human NuGet approval. Do not use it to bypass a failed workflow.
