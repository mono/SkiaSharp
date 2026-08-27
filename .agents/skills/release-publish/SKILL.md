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
  overrides that gate. Preserve the exact branch, source SHA, Build run ID,
  tests run ID, BAR build ID, and paired package versions.
- Require release-status to verify `.NET Libraries` channel 1648 and exact
  approved-feed routing for the tested Build/Tests/BAR handoff.
- Use scripts for detection, NuGet verification, draft creation, and final
  publication.
- Package publication, tag push, and GitHub Release publication are
  irreversible. Present the corresponding dry-run and obtain approval first.
- Preserve the detector's source SHA and run IDs; never select newer packages
  or pipeline runs.
- Keep the checkout unchanged. Draft creation pushes a lightweight tag directly
  to the tested SHA.
- Never delete or move a published tag/release to recover.
- The Build pipeline's standard Darc default-channel promotion is upstream of
  this skill. Use BAR locations when present, otherwise verify the BAR's exact
  versions on the approved feeds; never select or promote a release by mutable
  channel name. Darc promotion is not NuGet.org publication.
- NuGet.org publication is currently started manually. Provide the tested
  repository owner (`mono`), repository name (`SkiaSharp`), and exact source
  commit SHA, then stop for the maintainer.
- The agent owns customer-teaser classification between draft creation and
  publication; scripts assemble and validate the final release body.

## Script contract

| Script | Responsibility |
|--------|----------------|
| `scripts/detect-release-publish.py` | Read-only exact release/Build/tests/BAR/package handoff. |
| `scripts/create-release-draft.py` | Audit or create the exact tag and generated-notes GitHub draft. |
| `scripts/publish-release.py` | Validate the teaser and publish the draft. |
| `scripts/release_github.py` | Shared GitHub release and body helpers; not a user command. |
| `scripts/release_publish.py` | Shared clients and validation; not a user command. |

Write scripts audit with `--dry-run` and execute without it. The detector emits
the pinned audit commands; every confirmation report emits its exact
`executionCommand`.

## Actions

| Source | `nextAction` | Response |
|--------|--------------|----------|
| Detector | `manual-package-publication` | Show the exact owner, repository, and commit SHA for the human-run publisher; stop until publication completes. |
| Detector | `start-release-draft` | Both exact package versions are on NuGet.org; run `draftAuditCommand`. |
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

### 2. Publish packages manually

Render:

```markdown
## Package publication audit

**Release:** `{releaseBranch}`
**Commit:** `{sourceSha}`
**Build/tests runs:** `{buildRunId}` / `{testsRunId}`
**BAR build:** `{barBuildId}`
**BAR routing evidence:** `{barAssets}`
**Public packages:** SkiaSharp `{publicPackages.SkiaSharp}`,
HarfBuzzSharp `{publicPackages.HarfBuzzSharp}`
**NuGet.org state:** `{nuget.state}` / `{nuget.packages}`
**Publisher inputs:** owner `{manualPublication.repositoryOwner}`, repository
`{manualPublication.repositoryName}`, commit `{manualPublication.commitSha}`
```

For `manual-package-publication`, stop and give those three values to the
maintainer. The current publisher is run manually and may ask for other
interactive choices; do not infer or automate them. After the maintainer says
publication completed, rerun detection. It verifies the exact SkiaSharp and
HarfBuzzSharp versions on NuGet.org and returns `start-release-draft` only when
both are indexed.

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
