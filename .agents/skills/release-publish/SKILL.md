---
name: release-publish
description: >
  Publish SkiaSharp packages and finalize the release. Use when the user says
  "publish X", "finalize X", "tag X", "finish release X", or says release
  testing passed. This is the fourth release step: detect the exact tested
  handoff, audit and publish its packages, prepare the customer teaser, then
  audit and publish the immutable tag and GitHub Release.
---

# Release Publish

This skill is **Step 4 of 5**:

[release-branch](../release-branch/SKILL.md) →
[release-status](../release-status/SKILL.md) →
[release-testing](../release-testing/SKILL.md) → **release-publish** →
[release-milestones](../release-milestones/SKILL.md)

## Contract

- Start only from a passing release-testing handoff with the exact branch,
  source SHA, managed run ID, tests run ID, and paired package versions.
- Use scripts for detection, Azure publication, NuGet verification, tag/notes/
  release reconciliation, and sample-workflow observation.
- Package publication, tag push, and GitHub Release publication are
  irreversible. Present the corresponding dry-run and obtain approval first.
- Preserve the detector's source SHA and run IDs; never select newer packages or
  pipeline runs.
- Keep the checkout unchanged. Finalization pushes a lightweight tag directly
  to the tested SHA.
- Never delete or move a published tag/release to recover.
- The Azure package stage retains its own human approval.
- The agent owns only customer-teaser classification; scripts assemble and
  validate the final release body.

## Script contract

| Script | Responsibility |
|--------|----------------|
| `scripts/detect-release-publish.py` | Read-only exact release/testing/package handoff. |
| `scripts/push-release-packages.py` | Audit or execute pipeline 25298, wait for Azure, and verify both NuGet.org packages. |
| `scripts/finalize-release.py` | Audit or execute tag, website notes, GitHub Release, and sample synchronization. |
| `scripts/release_publish.py` | Shared clients and validation; not a user command. |

Write scripts audit with `--dry-run` and execute without it. The detector emits
the pinned audit commands; every confirmation report emits its exact
`executionCommand`.

## Actions

| Source | `nextAction` | Response |
|--------|--------------|----------|
| Detector | `audit-package-publication` | Run `pushAuditCommand`. |
| Packages | `confirm-publish-packages` | Approve and run package execution. |
| Packages | `approve-or-wait-for-publish` | Verify/approve the reported Azure run; execution continues waiting. |
| Packages | `wait-for-nuget` | Wait for both exact versions to index. |
| Packages | `start-release-finalization` | Run `finalizeAuditCommand`. |
| Finalization | `select-previous-tag` | Choose a candidate and rerun with `--previous-tag`. |
| Finalization | `write-release-teaser` | Fill `teaser.md`, then rerun the dry-run. |
| Finalization | `confirm-finalize-release` | Approve and run finalization. |
| Finalization | `wait-for-samples` | Continue observing sample synchronization. |
| Finalization | `investigate-samples` | Report failed/canceled sample automation. |
| Finalization | `start-release-milestones` | Hand off the emitted milestone audit command. |

## Workflow

### 1. Detect

```bash
python3 .agents/skills/release-publish/scripts/detect-release-publish.py \
  {release-branch-or-tested-sha}
```

Preserve all returned release/run/package pins.

### 2. Publish packages

Run `pushAuditCommand`. Render:

```markdown
## Package publication audit

**Release:** `{release.version}` ({release.type})
**Commit:** `{release.sourceSha}`
**Managed/tests runs:** `{release.managedRunId}` / `{release.testsRunId}`
**Public packages:** SkiaSharp `{release.publicPackages.SkiaSharp}`,
HarfBuzzSharp `{release.publicPackages.HarfBuzzSharp}`

| Operation | Status | Detail |
|-----------|--------|--------|
| `{operations[].id}` | `{operations[].status}` | `{operations[].detail}` |
```

For `confirm-publish-packages`, obtain approval and run `executionCommand`.
Verify that Azure selected the exact managed resource/run and the Stable or
Preview push stage. The script returns only after both exact public packages are
available on NuGet.org or a clear failure/timeout occurs.

### 3. Prepare finalization

Run `finalizeAuditCommand`. For `select-previous-tag`, show ordered candidates:
prefer the latest same-version preview/RC; ask for maintainer intent when the
first preview/hotfix boundary is ambiguous.

Rerun with the exact choice:

```bash
python3 .agents/skills/release-publish/scripts/finalize-release.py \
  {pinned arguments} \
  --previous-tag {tag} \
  --dry-run
```

The dry-run creates ignored local artifacts under `output/release/{release-tag}`:

| File | Ownership |
|------|-----------|
| `generated-log.md` | Script-generated PR log; classification input only. |
| `teaser.md` | Agent edits customer-facing sections. |
| `release-body.md` | Script-assembled final body. |

For `write-release-teaser`, follow
[github-release-teaser.md](references/github-release-teaser.md), edit only
`teaser.md`, preserve exactly one `<!-- RELEASE_LINKS -->` marker, and rerun the
dry-run with `--teaser-file`.

### 4. Publish the release

For `confirm-finalize-release`, present the exact tag/title/prerelease state,
expected body SHA, teaser, and operation table. Obtain approval and run
`executionCommand`.

One resumable execution pushes the exact tag, dispatches targeted website notes,
publishes the GitHub Release, and observes the release-triggered sample workflow.
Rerun the dry-run after interruption; validated completed operations are skipped.

### 5. Hand off milestones

For `start-release-milestones`, invoke the emitted `milestonesCommand` with
[release-milestones](../release-milestones/SKILL.md), complete its Audit path,
then run its normal Sync path. Include the sample-workflow result in the handoff.

## Reporting

Never dump raw JSON. Include every warning and link any operation URL. Ask for
approval only for `confirm-*` actions.

Use the manual
[Azure release coordinator](../../../scripts/azure-pipelines-release-coordinator.yml)
for button-driven start/complete execution over the same script contracts.
See [releasing.md](../../../documentation/dev/releasing.md) for the complete
release process.
