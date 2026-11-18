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

Publication and Finish need separate explicit authorization. A maintainer can
authorize a conditional Finish beforehand, but must still confirm the reviewed
Finish plan. Neither follows automatically from Prepare or Build/Tests.

## Push the packages

Before triggering:

1. Run `audit-release-state.ps1 -Version A.B -Json` for the requested
   `release/<identity>`. Require its exact-tip `skiasharp-package (1642)` build
   to have succeeded with one BAR, and the resource-triggered
   `skiasharp-tests (1630)` run to have succeeded for the same branch, commit
   and build number with a matching `triggerInfo.pipelineId`. The producing
   Build/Tests runs and BAR are the release-validation evidence; do not require
   a separate local approval report. Never borrow `.x`, `main`, or PR evidence.
2. Derive the shipping SkiaSharp/HarfBuzzSharp ID/version set from the shipment's
   source `scripts/VERSIONS.txt` and pack filter. Check public availability,
   provenance, and existing publisher runs' resolved `skiaSharpBuild` resources.
   Monitor an existing publication of the same producer; do not duplicate it.
   If the complete set is already public, do not republish.
3. Follow
   [Release Guide, step 4](../../../documentation/dev/releasing.md#4-publish-the-packages)
   for discovery and the Runs API request. Use dedicated publisher
   `skiasharp-nuget-release (1715)`, the exact producer **Build number** in
   `resources.pipelines.skiaSharpBuild.version`, and only
   `arcade_shipping_signed`. Request boolean `validateOnly: false` for production.
   Do not use the retired MAUI publisher or omit the producer selection.

After triggering:

1. GET the admitted run; match publisher/tooling revisions and selected producer
   run ID/build number. Read that producer to verify definition 1642, release
   branch, source SHA and BAR. Normalize `validateOnly` when checking read-back:
   Azure can return the requested boolean as the string `"false"`. Stop on
   mismatched resources or parameters.
2. Check parent, isolated publisher and ESRP results, not aggregate status alone.
   Verify the live approval configuration; protected ESRP approval remains
   required. Preserve run/ESRP IDs and do not bypass approvals.
3. Verify every expected public ID/version and source provenance. Missing
   packages can be propagation delay: keep monitoring without requeueing.
   Report failed or stalled publication with its evidence; do not retry failed
   parent jobs or switch paths. Monitoring-only requests do not authorize repairs.

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
