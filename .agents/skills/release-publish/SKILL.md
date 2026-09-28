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

The normal path is **two distinct, chat-triggered operations**: queue the
internal `dotnet-maui-release` pipeline to publish packages, then (after
publication and separate confirmation) dispatch SkiaSharp **Release - Finish**.
Neither operation starts merely because Prepare, a build, or the other
operation completed. Do not run the local Finish script by default.

## 1. Push the packages (only when requested)

1. Resolve the exact release identity, immutable `release/<identity>` branch
   and commit. Run `audit-release-state.ps1 -Version A.B -Json` and check the
   selected release branch's exact-tip `skiasharp-package` build succeeded
   with **one** BAR ID. Do not use the maintenance branch or the mono/skia SHA
   as the package source commit.
2. Verify `skiasharp-tests` (internal definition 1630) succeeded for the
   *same* branch, commit, and build number, and its pipeline resource
   `triggerInfo.pipelineId` points to that package build (definition 1642).
   Fail closed on missing, canceled, partial, or mismatched evidence. Preserve
   the exact version, branch, commit, Build, Tests, and BAR IDs together.
   Optional `release-testing` approval, if used, must match that same BAR.
   Read test runs with `az pipelines runs list --organization
   https://dev.azure.com/dnceng --project internal --pipeline-ids 1630
   --branch refs/heads/release/<identity> --output json`, and read each
   candidate's `triggerInfo` with `az pipelines runs show --id <test-run-id>`
   using the same organization and project. Do not accept an unrelated
   manually queued tests run.
3. Check public NuGet.org for the exact package version. If already public,
   do not queue another publication: verify its provenance and move to step 2
   below. Check for an existing active or completed MAUI release run for the
   same SkiaSharp commit and BAR; continue monitoring that run rather than
   queuing a duplicate. If the run state or package set is ambiguous, stop.
   Use `az pipelines runs list --organization
   https://dev.azure.com/dnceng --project internal --pipeline-ids 1445
   --output json` and read candidate runs' `templateParameters` with
   `az pipelines runs show`; the release audit does not track MAUI runs.
4. Verify the internal MAUI `main` ref has the merged SkiaSharp support in
   `eng/pipelines/ci-official-release.yml`. Pin that ref to its current exact
   internal SHA; do not use an unmerged feature ref for normal releases. If the
   support is not deployed to the internal mirror, stop and report the blocker.
   Resolve the internal SHA with `git ls-remote
   https://dev.azure.com/dnceng/internal/_git/dotnet-maui refs/heads/main`
   and verify the file at that revision includes the SkiaSharp release path.
5. Queue **`dotnet-maui-release` (1445)** with Azure CLI using the exact
   SkiaSharp BAR/source commit. This is a real preparation run, not a separate
   dry run: publication remains gated by its `ManualValidation`.

```bash
# After verifying the internal main ref and all release evidence above:
az pipelines run \
  --organization https://dev.azure.com/dnceng --project internal \
  --id 1445 --branch refs/heads/main --commit-id <exact-internal-MAUI-main-SHA> \
  --parameters ghOwner=mono ghRepo=SkiaSharp \
    commitHash=<exact-SkiaSharp-release-commit> \
    pushWorkloadSet=false pushNugetOrg=true pushPackages=true \
    nugetIncludeFilters=skip nugetExcludeFilters=skip \
  --output json
```

`--commit-id` pins the **MAUI pipeline code**; `commitHash` identifies the
**SkiaSharp BAR**. Capture the created run ID, then read it back with
`az pipelines runs show --organization https://dev.azure.com/dnceng --project
internal --id <run-id> --output json`. Require the source ref/SHA and all
stored `templateParameters` to match the intended inputs. If the dispatch
fails or read-back disagrees, stop; do not blindly retry.

6. Monitor **Prepare release artifacts** and **Gather and classify release
   packages**. Inspect `NuGetReleaseAudit/release-audit.json`: its BAR,
   repository, commit, selected IDs/versions and staged IDs/versions must
   match the evidence, with no missing or duplicate identities. At
   `ManualValidation`, present the run link and audit to the user. The
   protected publish job must not start before approval. Only a human with
   authority to approve package ownership, quota, and the NuGet service
   connection should resume that gate. Do not approve it merely because
   preparation succeeded.
7. After approval, monitor the same run and its post-publish verification.
   Independently verify **every** exact staged shipping ID/version on
   NuGet.org, not only `SkiaSharp`. A successful dispatch, approval, build,
   or partial package visibility is not publication completion. On failed or
   partial publication, preserve the run and package evidence; do not
   automatically queue another run or proceed to Finish.

## 2. Finish after the packages are public

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

Inspect the dispatched runs. Finish checks the public SkiaSharp nuspec's
release-branch and source-commit provenance, creates or verifies the immutable
exact-version tag, publishes the GitHub Release, opens or updates the support
PR, dispatches release-note generation, and requires milestone reconciliation.
Rerun the read-only release audit afterward. Never move/delete a tag or
replace a public package. Use `release-milestones` separately only for
diagnostics or repairs.

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
