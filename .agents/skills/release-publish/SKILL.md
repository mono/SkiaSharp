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

Before triggering:

1. Run `audit-release-state.ps1 -Version A.B -Json`. Select the requested
   immutable `release/<identity>` branch and require its exact-tip
   `skiasharp-package` build (1642) to have succeeded with one BAR ID. Use
   the **SkiaSharp** release commit, never the maintenance tip or mono/skia SHA.
2. Use `az pipelines runs list/show` to require `skiasharp-tests` (1630)
   succeeded for the same branch, commit and build number, with
   `triggerInfo.pipelineId` matching that package build. Any optional
   `release-testing` approval must match the BAR.
3. Check the exact public version and existing MAUI runs (1445), comparing
   `templateParameters.commitHash` with that commit. Monitor an existing run
   rather than queueing a duplicate; if already public, verify provenance and
   proceed to Finish. Stop on ambiguous or mismatched evidence.

Queue `dotnet-maui-release` only on explicit request. **Temporary test ref:**
the feature branch below is for this unmerged skill. Before merging this
skill, merge dotnet/maui#38967 and replace the branch with
`refs/heads/main`. Never use the feature ref as the normal release path.

```bash
az pipelines run \
  --organization https://dev.azure.com/dnceng --project internal \
  --id 1445 \
  --branch refs/heads/mattleibow-skiasharp-release-support \
  --parameters ghOwner=mono ghRepo=SkiaSharp \
    commitHash=<exact-SkiaSharp-release-commit> \
    pushWorkloadSet=false pushNugetOrg=true pushPackages=true \
    nugetIncludeFilters=skip nugetExcludeFilters=skip \
  --output json
```

`--branch` uses the **current MAUI branch tip**; `commitHash` remains the
**exact SkiaSharp BAR commit**. The real run prepares packages and pauses
at `ManualValidation`, so no separate dry run is required.

After triggering:

1. Read back the run ID, resolved MAUI source ref/SHA and template parameters.
   Record the actual pipeline-code SHA and verify it contains the SkiaSharp
   release support; stop on a mismatch without blindly retrying.
2. Compare `NuGetReleaseAudit` BAR, repository, commit and selected/staged
   package identities with the release record. Show the audit to the human
   approver, who must confirm ownership and quota before resuming
   `ManualValidation`. Do not approve merely because preparation succeeded.
3. After approval, monitor the same run and independently verify **every**
   staged shipping ID/version on NuGet.org. On failure or partial publication,
   preserve the evidence; never automatically requeue or run Finish.

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
