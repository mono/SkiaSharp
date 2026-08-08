---
name: release-publish
description: >
  Publish SkiaSharp packages and finalize the release. Use when the user says
  "publish X", "finalize X", "tag X", "finish release X", says release testing
  passed, or explicitly overrides the testing gate. This is the fourth release
  step: detect the exact testing handoff, publish its packages, create the
  immutable tag and generated-notes draft, prepare the customer teaser, then
  publish the approved draft.
---

# Release Publish

This skill is **Step 4 of 5**:

[release-branch](../release-branch/SKILL.md) →
[release-status](../release-status/SKILL.md) →
[release-testing](../release-testing/SKILL.md) → **release-publish** →
[release-milestones](../release-milestones/SKILL.md)

## Contract

- Start from a passing release-testing handoff unless the user explicitly
  overrides that gate. Preserve the exact branch, source SHA, managed run ID,
  tests run ID, and paired package versions.
- Use scripts for detection, Azure publication, NuGet verification, draft
  creation, and final publication.
- Package publication, tag push, and GitHub Release publication are
  irreversible. Present the corresponding dry-run and obtain approval first.
- Preserve the detector's source SHA and run IDs; never select newer packages or
  pipeline runs.
- Keep the checkout unchanged. Draft creation pushes a lightweight tag directly
  to the tested SHA.
- Never delete or move a published tag/release to recover.
- Approval of the queue command authorizes only queueing pipeline 25298. Its
  protected push stage then waits for a human to review the exact versions and
  destination. The agent never approves that downstream gate.
- The agent owns customer-teaser classification between draft creation and
  publication; scripts assemble and validate the final release body.

## Script contract

| Script | Responsibility |
|--------|----------------|
| `scripts/detect-release-publish.py` | Read-only exact release/testing/package handoff. |
| `scripts/push-release-packages.py` | Audit, queue/recover one exact pipeline 25298 run, and optionally wait through NuGet verification. |
| `scripts/create-release-draft.py` | Audit or create the exact tag and generated-notes GitHub draft. |
| `scripts/publish-release.py` | Validate the teaser and publish the draft. |
| `scripts/release_github.py` | Shared GitHub release and body helpers; not a user command. |
| `scripts/release_publish.py` | Shared clients and validation; not a user command. |

Write scripts audit with `--dry-run` and execute without it. The detector emits
the pinned audit commands; every confirmation report emits its exact
`executionCommand`.

Package verification defaults to `--verification nuget`, which waits for both
exact package versions to be indexed. Automated coordination may explicitly use
`--verification azure` to continue after the protected publication run
succeeds; that mode reports that NuGet.org indexing was not verified.

## Actions

| Source | `nextAction` | Response |
|--------|--------------|----------|
| Detector | `audit-package-publication` | Run `pushAuditCommand`. |
| Packages | `confirm-publish-packages` | Approve and run package execution. |
| Packages | `approve-publish-run` | Show `publishRun.url` and stop for human review/approval. |
| Packages | `wait-for-nuget` | Continue the pinned resume command until both versions index. |
| Packages | `retry-publish-run` | Show the failed exact run and return to package audit. |
| Packages | `start-release-draft` | Run `draftAuditCommand`. |
| Draft | `confirm-create-release-draft` | Approve and create the tag/draft. |
| Draft | `write-release-teaser` | Classify `generated-log.md` and fill `teaser.md`. |
| Draft | `audit-release-publication` | Release already exists; run `publishAuditCommand`. |
| Publication | `confirm-publish-release` | Approve and publish the completed draft. |
| Publication | `start-release-milestones` | Hand off the emitted milestone reconciliation command. |

## Workflow

### 1. Detect

```bash
python3 .agents/skills/release-publish/scripts/detect-release-publish.py \
  {release-branch-or-tested-sha}
```

Preserve all returned release/run/package pins. Detection is also the recovery
entry point after lost context: rerun it from only `release/{version}` to
reconstruct every `--expect-*` value and pinned audit command. Never reconstruct
those values manually.

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

The emitted audit command already includes `--dry-run`. Use it whenever status
must be read or recovered without queueing anything; it detects an exact
queued/running/succeeded publication and returns its URL/resume command.

For `confirm-publish-packages`, obtain approval and run `executionCommand`.
Verify that Azure selected the exact managed resource/run and the Stable or
Preview destination. The queue command returns immediately: show
`publishRun.runId` and `publishRun.url`, then stop so a human can review the
versions/destination and approve the protected stage.

After the user confirms that decision, run the emitted `resumeCommand`. It is
the same script with `--wait --publish-run {id}`, waits for completion, and
verifies both exact public packages on NuGet.org.

If Azure succeeds but indexing exceeds the wait window, treat the returned
`wait-for-nuget` report as resumable status, not a publication failure. Show
`wait.missingPackages` and reuse `resumeCommand`.

For unattended automation, invoke the approved execution command with `--wait`.
It queues or recovers the exact run and prints its URL immediately. Keep the
default `--verification nuget` to wait through indexing, or explicitly use
`--verification azure` to stop waiting when the protected run succeeds.

### 3. Create the generated-notes draft

Run `draftAuditCommand`. It parses all release tags using SkiaSharp's
NuGet-compatible ordering (including four-part hotfixes) and selects the greatest
tag below the current release as `previousTag`.

```bash
python3 .agents/skills/release-publish/scripts/create-release-draft.py \
  {pinned arguments} \
  --dry-run
```

For `confirm-create-release-draft`, present the exact tag, source SHA, title,
prerelease state, previous tag, and operation table. Obtain approval and run
`executionCommand`. It pushes the tag, creates a GitHub draft containing the
exact generated notes, then downloads that body into ignored local artifacts:

| File | Ownership |
|------|-----------|
| `generated-log.md` | Body downloaded from the GitHub draft; classification input only. |
| `teaser.md` | Agent edits customer-facing sections. |

Rerunning this dry-run detects an existing remote tag/draft and re-downloads
`generated-log.md`.

### 4. Prepare the teaser

For `write-release-teaser`, follow
[github-release-teaser.md](references/github-release-teaser.md), edit only
`teaser.md`, and preserve exactly one `<!-- RELEASE_LINKS -->` marker.

This step is editorial and local only. It does not audit, modify, or publish the
GitHub Release. Unsaved edits to `teaser.md` are the only publication state that
cannot be recovered remotely.

### 5. Finish the release

Run the draft result's emitted `publishAuditCommand`. The publication dry-run
consumes `teaser.md`, creates `release-body.md`, and validates its exact SHA.

| File | Ownership |
|------|-----------|
| `release-body.md` | Script-assembled final body uploaded to the draft after approval. |

For `confirm-publish-release`, present the draft URL, expected body SHA, teaser,
and operation table. Obtain approval and run `executionCommand`. One execution
dispatches targeted website notes, uploads the approved body, and publishes the
draft.

### 6. Hand off milestones

For `start-release-milestones`, invoke the emitted `milestonesCommand` with
[release-milestones](../release-milestones/SKILL.md), complete its Reconcile
path, then run its normal Advance path.

## Reporting

Never dump raw JSON. Include every warning and link any operation URL. Ask for
approval only for `confirm-*` actions.

See [releasing.md](../../../documentation/dev/releasing.md) for the complete
release process.
