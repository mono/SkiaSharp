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

1. Run `audit-release-state.ps1 -Version A.B -Json` for the requested
   `release/<identity>`. Require its exact-tip `skiasharp-package (1642)` build
   to have succeeded with one BAR, and the resource-triggered
   `skiasharp-tests (1630)` run to have succeeded for the same branch, commit
   and build number with a matching `triggerInfo.pipelineId`. Any
   `release-testing` approval must match the BAR. Use that **SkiaSharp**
   commit, not the maintenance tip or mono/skia SHA.
2. Check the complete public package set and existing
   `dotnet-maui-release (1445)` runs for that commit
   (`templateParameters.commitHash`). Monitor an existing run instead of
   queueing another; if fully public, verify provenance and use the Finish
   path only on a separate request. Stop on mismatched evidence.
3. Require the `dotnet-maui-release (1445)` pipeline to default to
   `refs/heads/main` and the internal MAUI mirror to contain the merged
   SkiaSharp release support.
   Wait if the mirror is behind; never use the old feature branch.

Queue `dotnet-maui-release (1445)` on its verified default MAUI `main` tip
only on explicit request:

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

The pipeline's default branch selects the **current MAUI main tip**;
`commitHash` remains the **exact SkiaSharp BAR commit**. The real run prepares
packages and pauses at `ManualValidation`, so no separate dry run is required.

After triggering:

1. Read back the run's MAUI ref/SHA and parameters. Require `refs/heads/main`,
   the requested SkiaSharp commit and flags, and a MAUI SHA containing release
   support; stop on a mismatch without retrying.
2. Match `NuGetReleaseAudit` BAR, repository, commit and selected/staged package
   identities to the release record. Present it for human `ManualValidation`;
   the approver confirms package ownership and quota. Preparation alone is
   not approval.
3. Monitor that run after approval and independently verify **every** staged
   shipping ID/version on NuGet.org. On failure or partial publication,
   preserve evidence; do not automatically requeue or run Finish.

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

### Local Finish fallback

Only when the GitHub workflow is unavailable or local execution is explicitly
requested, use the repository-owned Finish script:

```powershell
./scripts/infra/publishing/finish-release.ps1 -Version 4.153.0-preview.1 -Mode DryRun

# After reviewing the plan and receiving confirmation:
./scripts/infra/publishing/finish-release.ps1 -Version 4.153.0-preview.1 -Mode Push
```

Local Finish does **not** replace the protected MAUI package pipeline or its
human NuGet approval. Do not use it to bypass a failed workflow.
