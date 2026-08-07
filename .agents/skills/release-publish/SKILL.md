---
name: release-publish
description: >
  Publish SkiaSharp packages and finalize the release. Use when the user says
  "publish X", "finalize X", "tag X", "finish release X", or says release
  testing passed. This is the fourth release step: detect the exact tested
  handoff, dry-run and publish the exact packages, then dry-run and finalize the
  tag, GitHub release, website notes, and sample automation.
---

# Release Publish

This skill is **Step 4 of 5**:

[release-branch](../release-branch/SKILL.md) →
[release-status](../release-status/SKILL.md) →
[release-testing](../release-testing/SKILL.md) → **release-publish** →
[release-milestones](../release-milestones/SKILL.md)

The agent confirms manual release-testing passed, renders script JSON, obtains
approval, and runs exact commands. Scripts own deterministic mechanics. The
agent owns only customer-teaser classification.

## Safety and ownership

- Publishing packages, pushing a release tag, and publishing a GitHub Release
  are irreversible. Never infer approval.
- Ask before running any command without `--dry-run`.
- The Azure publish stage has its own human approval. Verify its run name,
  selected resource, and Stable/Preview stage before approving it.
- Never delete or move a remote release tag.
- Never checkout or move the current branch. Finalization pushes a lightweight
  tag directly to the exact tested source SHA.
- Never select a newer package or pipeline run. Every command is pinned to the
  detector's source SHA, managed run ID, and tests run ID.
- The GitHub Release is created only after teaser review.
- Publishing the GitHub Release triggers **Sync - Samples** automatically; no
  sample ZIP upload is needed.

## Command model

The workflow has three user-facing scripts:

| Script | Default behavior | `--dry-run` |
|--------|------------------|-------------|
| `detect-release-publish.py` | Read-only detection | Not needed; always read-only |
| `push-release-packages.py` | Queue/wait for exact Azure publish and both NuGet packages | Audit/preview only |
| `finalize-release.py` | Reconcile tag, docs, release, and samples | Audit plus ignored local teaser artifacts |

The publication/finalization scripts follow the release-branch convention:

| Invocation | Behavior |
|------------|----------|
| Omit `--dry-run` | Execute |
| Pass `--dry-run` | Audit |

## What is deterministic

### Detector

`detect-release-publish.py` accepts one exact `release/{version}` branch or
tested source commit. It runs release-status, resolves the exact release branch,
and emits:

- Tested source SHA.
- Exact managed and tests run IDs.
- Managed build number.
- Exact test and public package versions.
- Pinned package-push and finalization dry-run commands.

It never queues, tags, publishes, or writes files.

### Package publication

`push-release-packages.py --dry-run`:

- Revalidates release-status and the current remote release-branch tip.
- Revalidates the exact tested package versions on the preview feed.
- Compiles Azure pipeline 25298 in `previewRun` mode.
- Pins resource `SkiaSharp` to the exact managed run ID.
- Pins `selectedResource=SkiaSharp`, `pushPackages=true`, and the exact
  `pushStable` value.
- Reconciles any matching live run and exact NuGet.org package state.

After approval, run the same command without `--dry-run`. It queues only when
needed, waits while the Azure approval/run completes, then waits until both exact
public packages are indexed on NuGet.org. Default timeout is 60 minutes.
An unchanged package version may already be public; the pipeline uses
`--skip-duplicate`, so the exact remaining package can still be published safely.

### Release finalization

`finalize-release.py --dry-run`:

- Requires both exact public packages on NuGet.org.
- Revalidates the source SHA and selected CI chain.
- Validates an explicit previous tag and reports ordered candidates.
- Generates GitHub release notes from that exact previous tag.
- Reconciles the exact remote tag, GitHub Release, title, prerelease
  flag, and sample run.

With `--dry-run`, it also refreshes only the ignored local teaser artifacts
under `output/release/{tag}/`; it makes no remote changes.

Without `--dry-run`, one call performs all currently valid finalization work:

| Current state | Execution |
|---------------|-----------|
| Unpublished + approved teaser | Push exact tag, dispatch targeted website notes, and publish the GitHub Release. |
| Published release | Validate exact state and report sample automation. |

If a command fails, rerun the dry-run and then rerun the same execution command;
completed exact operations are skipped.

## Human-owned work

| Work | Reason |
|------|--------|
| Confirm release-testing passed | Manual matrix results are not stored in CI. |
| Select previous tag when candidates are ambiguous | First preview/hotfix boundaries may require maintainer intent. |
| Approve Azure push stage | Azure protected resource gate. |
| Classify customer teaser | Consumer relevance is editorial judgment. |

## Status values

| Status | Meaning |
|--------|---------|
| `done` | Exact state exists and validates. |
| `pending` | Approved execution can perform it. |
| `running` | External automation is queued/running/indexing. |
| `awaiting-user` | Human approval or decision is required. |
| `blocked` | A dependency or unsafe state prevents execution. |
| `failed` | External automation completed unsuccessfully and needs investigation. |

## Package-push actions

| `nextAction` | Meaning |
|--------------|---------|
| `audit-package-publication` | Detector completed; run `pushAuditCommand`. |
| `confirm-publish-packages` | Ask approval, then run `executionCommand`. |
| `approve-or-wait-for-publish` | Verify/approve the reported Azure run; the executing script continues waiting. |
| `wait-for-nuget` | Publish succeeded; wait for both package versions. |
| `start-release-finalization` | Both exact public packages are ready. |

## Finalization actions

| `nextAction` | Meaning |
|--------------|---------|
| `select-previous-tag` | Select a candidate and rerun with `--previous-tag`. |
| `write-release-teaser` | Edit the generated `teaser.md`. |
| `confirm-finalize-release` | Teaser is ready for tag/docs/release finalization. |
| `wait-for-samples` | Release-triggered sample synchronization is pending/running. |
| `investigate-samples` | Sample synchronization failed or was canceled. |
| `start-release-milestones` | GitHub Release is published; run the emitted release-milestones dry-run. |

## Presenting audits

Never dump raw JSON. Render:

```markdown
## Publication audit

**Release:** `{release.version}` ({release.type})
**Commit:** `{release.sourceSha}`
**CI:** managed `{release.managedRunId}`, tests `{release.testsRunId}`
**Packages:** SkiaSharp `{release.publicPackages.SkiaSharp}`,
HarfBuzzSharp `{release.publicPackages.HarfBuzzSharp}`

| Operation | Status | Detail |
|-----------|--------|--------|
| `{operations[].id}` | `{operations[].status}` | `{operations[].detail}` |
```

For package publication, add:

```markdown
| Check | Result |
|-------|--------|
| Azure request | Valid, not required, or invalid |
| NuGet.org | `{nuget.state}` |
```

For finalization, add:

```markdown
| Check | Result |
|-------|--------|
| Tag | `{release.tag}` |
| Release | draft/published/not created |
| Body | expected body SHA/match |
| Samples | workflow state when present |
```

Include every warning and link operations that include a URL.

Ask for approval only when `nextAction` begins with `confirm-`, then run the exact
`executionCommand` (which intentionally has no `--dry-run`).

## Customer teaser

Finalization creates:

| Artifact | Purpose |
|----------|---------|
| `output/release/{tag}/generated-log.md` | GitHub's generated PR log; only classification input. |
| `output/release/{tag}/teaser.md` | Agent-owned teaser with one links marker. |
| `output/release/{tag}/release-body.md` | Script-assembled final body. |

Follow [github-release-teaser.md](references/github-release-teaser.md). Edit only
`teaser.md`; preserve exactly one `<!-- RELEASE_LINKS -->` marker. The script
adds exact links and folds the unedited generated log.

## Runbook

### 1. Confirm testing handoff

Do not start unless release-testing reported every approved item passed and
provided the exact branch, source SHA, managed run ID, and tests run ID.

### 2. Detect exact inputs

```bash
python3 .agents/skills/release-publish/scripts/detect-release-publish.py \
  release/{version}
```

Render the detector output.

### 3. Dry-run and publish packages

Run `pushAuditCommand`. For `confirm-publish-packages`, obtain approval and run
the returned `executionCommand`.

The script waits while the protected Azure stage is approved and executed. In
the Azure run, verify:

| Check | Expected |
|-------|----------|
| Run name | `SkiaSharp {buildNumber}` |
| Stage | `Push Stable` for stable; `Push Preview` otherwise |
| Resource | Exact managed run ID/build number from detector |

It returns only after both exact public package versions are on NuGet.org, or
fails/times out clearly.

### 4. Dry-run finalization and select previous tag

Run `finalizeAuditCommand`. If it reports `select-previous-tag`, show
`release.previousTagCandidates`.

Prefer the latest same-version preview/RC. If none exists, confirm the intended
prior stable tag with the user. Rerun with:

```bash
--previous-tag {exact-tag} --dry-run
```

### 5. Prepare and audit the teaser

The previous-tag dry-run writes `generated-log.md` and the `teaser.md` template
locally without changing remote release state.

Read `generated-log.md`, edit `teaser.md` according to the reference, then rerun:

```bash
python3 .agents/skills/release-publish/scripts/finalize-release.py \
  {pinned arguments} \
  --previous-tag {tag} \
  --teaser-file output/release/{release-tag}/teaser.md \
  --dry-run
```

Review the expected body SHA and all publication operations.

### 6. Final approval

For `confirm-finalize-release`, show:

- Exact tag, title, and prerelease state.
- Teaser path and expected body SHA.

Obtain approval, then run `executionCommand`. In one resumable call the script
pushes the tag, dispatches website notes, and publishes the GitHub Release.

### 7. Hand off release milestones

Once `nextAction` is `start-release-milestones`, invoke the emitted
`milestonesCommand` using
[release-milestones](../release-milestones/SKILL.md). The release-milestones
workflow completes its Audit path from that command, then runs its normal Sync
path to update upcoming milestones and automatically close tagged milestones
after moving open issues forward. Report the sample workflow state during the
handoff.

## Recovery

Both write scripts are resumable:

1. Rerun the detector.
2. Rerun the relevant dry-run.
3. Review exact operations and `nextAction`.
4. Run the returned execution command only after approval.

Never delete a tag or published release to recover. Correct mistakes with a new
release version.

## Files

- [scripts/detect-release-publish.py](scripts/detect-release-publish.py) —
  read-only tested-run handoff.
- [scripts/push-release-packages.py](scripts/push-release-packages.py) — exact
  Azure publish preview/queue/wait and NuGet verification.
- [scripts/finalize-release.py](scripts/finalize-release.py) — exact tag, docs,
  release, and sample reconciliation.
- [scripts/release_publish.py](scripts/release_publish.py) — shared deterministic
  clients and validation helpers; not a user-facing command.
- [scripts/tests/](scripts/tests/) — version, handoff, Azure request, CLI mode,
  tag, and body-assembly tests.
- [references/github-release-teaser.md](references/github-release-teaser.md) —
  human teaser classification rules.
- [Azure release coordinator](../../../scripts/azure-pipelines-release-coordinator.yml)
  — manual start/complete pipeline over the same script contracts.
- [releasing.md](../../../documentation/dev/releasing.md) — complete release
  pipeline reference.
