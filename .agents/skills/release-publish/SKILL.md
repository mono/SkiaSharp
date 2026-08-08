---
name: release-publish
description: >
  Publish SkiaSharp packages and finalize the release. Use when the user says
  "publish X", "finalize X", "tag X", "finish release X", says release testing
  passed, or explicitly overrides the testing gate. This is the fourth release
  step: detect the exact testing handoff, publish its packages, create the
  immutable tag and marked generated-notes draft, then publish the approved
  draft and dispatch reviewed release notes.
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
- The initial and published GitHub Release body is the useful GitHub-generated
  notes wrapped in explicit managed markers. Never publish an empty placeholder.
- Customer teaser prose is owned by the later agentic release-notes PR. A
  deterministic workflow applies reviewed exact-tag teaser entries after merge
  without reconstructing or rewriting GitHub's generated payload.

## Script contract

| Script | Responsibility |
|--------|----------------|
| `scripts/detect-release-publish.py` | Read-only exact release/testing/package handoff. |
| `scripts/push-release-packages.py` | Audit, queue/recover one exact pipeline 25298 run, and optionally wait through NuGet verification. |
| `scripts/create-release-draft.py` | Audit or create the exact tag and generated-notes GitHub draft. |
| `scripts/publish-release.py` | Publish the marked generated-notes draft, then dispatch release notes. |
| `scripts/release_github.py` | Shared GitHub release and body helpers; not a user command. |
| `scripts/release_publish.py` | Shared clients and validation; not a user command. |

Write scripts audit with `--dry-run` and execute without it. The detector emits
the pinned audit commands; every confirmation report emits its exact
`executionCommand`.

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
| Draft | `audit-release-publication` | Release already exists; run `publishAuditCommand`. |
| Draft | `confirm-publish-release` | Approve and publish the useful generated-notes draft. |
| Publication | `dispatch-release-notes` | Retry the idempotent docs dispatch for an already-published release. |
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
It queues or recovers the exact run, prints its URL immediately, then waits
through protected approval and NuGet indexing in one process.

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
`executionCommand`. It pushes the tag and creates a GitHub draft containing the
exact generated notes inside managed markers. The generated payload is frozen:
later automation may update only the separate managed teaser region above it.

| File | Ownership |
|------|-----------|
| `generated-release-body.md` | Exact marked body used to create or audit the GitHub draft. |

Rerunning this dry-run detects an existing remote tag/draft and verifies its
managed markers.

### 4. Finish the release

Run the draft result's emitted `publishAuditCommand`. For
`confirm-publish-release`, present the draft URL, current body SHA, and operation
table. Obtain approval and run `executionCommand`. One execution publishes the
marked generated-notes body unchanged and then dispatches the targeted agentic
release-notes workflow.

If publication succeeds but dispatch fails, rerun the audit. An already-published
release reports `dispatch-release-notes`; run its `executionCommand` without a new
approval. The dispatch is safe to retry and successful execution advances to
milestones without republishing or rewriting the release.

Publication does not wait for teaser review. Continue immediately to milestones.
The release-notes workflow adds the exact-tag teaser entry to the line's single
`prose.json`; after that PR is reviewed and merged, the zero-AI updater changes
only the managed teaser region. Manual content outside managed markers and the
original generated payload remain byte-for-byte intact.

### 5. Hand off milestones

For `start-release-milestones`, invoke the emitted `milestonesCommand` with
[release-milestones](../release-milestones/SKILL.md), complete its Reconcile
path, then run its normal Advance path.

## Reporting

Never dump raw JSON. Include every warning and link any operation URL. Ask for
approval only for `confirm-*` actions.

See [releasing.md](../../../documentation/dev/releasing.md) for the complete
release process.
