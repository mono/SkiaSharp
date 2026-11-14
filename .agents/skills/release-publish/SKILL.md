---
name: release-publish
description: >
  Publish SkiaSharp release packages to NuGet.org from chat and finish public
  releases. Use when a maintainer says "push the packages", "publish the BAR",
  "resume publication", "finalize X", "tag X", or "finish release X". Queue the
  dedicated SkiaSharp publisher only on explicit request; after exact packages
  are public, use the SkiaSharp Release - Finish workflow with a separate
  confirmation.
---

# Release Publish

Use this skill for **two separate chat requests**: "push the packages" queues
the dedicated SkiaSharp publication pipeline; "finish the release" dispatches
the SkiaSharp Finish workflow after the packages are public. Neither runs
automatically after Prepare, Build/Tests, or the other operation.

## Push the packages

Before triggering:

1. Run `audit-release-state.ps1 -Version A.B -Json` for the requested
   `release/<identity>`. Require its exact-tip `skiasharp-package (1642)` build
   to have succeeded with one BAR, and the resource-triggered
   `skiasharp-tests (1630)` run to have succeeded for the same branch, commit
   and build number with a matching `triggerInfo.pipelineId`. The producing
   Build/Tests runs and BAR are the release-validation evidence; do not require
   a separate local approval report. Never borrow `.x`, `main`, or PR evidence.
2. Derive every expected shipping ID/version from the shipment's source
   `scripts/VERSIONS.txt` and pack filter, including both SkiaSharp and
   HarfBuzzSharp. Check public availability and provenance, and existing
   `skiasharp-nuget-release (1715)` parents **and their resolved
   `skiaSharpBuild` resources**. Match the exact producer's numeric ID,
   build number, branch and SHA; another source's successful publication is
   not equivalent. Monitor an existing publication instead of duplicating it.
   If fully public, use Finish only on a separately authorized request.
3. Discover pipeline 1715's live default/ref and YAML in
   `dotnet-release-workflows`, plus the current production `dotnet-release`
   tooling resource ref/SHA. Verify executable configuration and current
   owner-supported production evidence, not stale rollout comments alone.
   Do not assume `main`, pin a temporary production branch forever, or fall
   back to the retired MAUI publisher. If production configuration cannot be
   verified, stop and report.

Use the Runs API request in
[Release Guide, step 4](../../../documentation/dev/releasing.md#4-publish-the-packages)
only on an explicit publication request. Select
`resources.pipelines.skiaSharpBuild.version` using the **exact producing
Build number**, not its numeric build ID. Never omit it and accept latest.
The only entry parameter is boolean `templateParameters.validateOnly`:
`false` publishes to NuGet.org; `true` validates without publishing.
There are no MAUI `commitHash`, repository, push, or NuGet-filter parameters.

The publisher consumes only `arcade_shipping_signed` from producer 1642.
Do not use unsigned `nuget`/`arcade_shipping` or classic `Container`
`PackageArtifacts`, which can include nonshipping transport assets such as
`_DocsMedia`, `_NativeAssets*`, and `_NuGets*`. Derive the expected package
count from that shipment, not a permanent hardcoded total.

After triggering:

1. GET the admitted run and verify resolved publisher self and tooling
   refs/SHAs, `validateOnly`, and the selected producer numeric ID/build
   number/branch/SHA. An immediate POST response may lack resolved resources;
   dispatch is not verified until read-back agrees. Stop on a mismatch.
2. Preserve parent and isolated publisher run IDs and ESRP IDs. The production
   entry invokes isolated publisher 1697 through `dotnet-release`/ESRP.
   The current parent uses `requireApproval: false`, so it has no parent
   `ManualValidation`; protected ESRP scan/release approval remains. Do not
   change permissions or bypass approvals.
3. Monitor that publication and independently verify **every** expected
   shipping ID/version and source provenance on NuGet.org before Finish.
   The parent's "Require the first attempt" guard prohibits retrying failed
   parent jobs that may have queued another publication. On failure or
   partial publication, preserve source/run/ESRP IDs, audit every expected
   package and report; never auto-requeue, switch publish paths, or run Finish.
   If the user asked for monitoring/reporting only, do not repair CI.

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

Local Finish does **not** replace the dedicated publisher or protected ESRP
approval. Do not use it to bypass a failed workflow.
