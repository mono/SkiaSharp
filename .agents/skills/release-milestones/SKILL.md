---
name: release-milestones
description: >
  Audit, synchronize, and close SkiaSharp GitHub milestones. Use this skill
  whenever the user asks to audit milestone assignments, fix shipped PR/issue
  milestones, sync upcoming milestones with Chromium/Skia schedules, close a
  release milestone, check milestone hygiene, or continue after
  release-publish. This is the final release step and also a standalone
  maintenance workflow intended for regular use.
---

# Release Milestones

This skill is **Step 5 of 5**:

[release-branch](../release-branch/SKILL.md) →
[release-status](../release-status/SKILL.md) →
[release-testing](../release-testing/SKILL.md) →
[release-publish](../release-publish/SKILL.md) →
**release-milestones**

It also runs independently whenever milestone state needs review.

Use the Python scripts for all schedule calculation, Git/release-range
selection, GitHub queries, assignment updates, and closure. The agent renders
JSON and obtains approval for pending writes.

Choose one of two paths:

| Path | Use it for | Script |
|------|------------|--------|
| **Sync** | Synchronize upcoming dates and automatically roll/close milestones that have shipped tags. | `sync-milestones.py` |
| **Audit** | Correct shipped PR and linked-issue milestone assignments. | `audit-milestones.py` |

The release-publish handoff composes the same two paths: complete Audit starting
with its emitted dry-run command, then complete Sync.

## Safety and ownership

- Run `--dry-run` before every write command and show the exact operations.
- Omit `--dry-run` only after approval.
- Milestone assignment changes are reversible but affect project planning, so
  never infer approval.
- A remote release tag makes its matching milestone eligible for closure.
- Before closure, open issues move to the next unshipped milestone in release
  order; the dry-run shows that destination explicitly.
- Assignment auditing considers only releases that shipped with a tag.
  Unshipped preview/RC ranges roll forward to the next shipped release.
- Main-branch commits after the last release cut remain unassigned until a later
  release ships.
- These scripts never modify release branches, tags, packages, or GitHub
  Releases.

## Invocation model

Both paths follow the release workflow convention:

| Invocation | Behavior |
|------------|----------|
| Pass `--dry-run` | Read-only plan |
| Omit `--dry-run` | Execute approved operations |

## Sync path

`sync-milestones.py` reads the SkiaSharp major and current Skia milestone from
`scripts/VERSIONS.txt`, then maps Chromium dates:

| Chromium event | SkiaSharp milestone | Due date |
|----------------|----------------------|----------|
| Branch / Beta | `X.M.0-preview.1` | Earliest Beta |
| Early Stable Cut / Early Stable | `X.M.0-preview.2` | Early Stable |
| Early Stable / Stable Cut | `X.M.0-rc.1` | Stable Cut |
| Stable Cut / Stable | `X.M.0` | Stable |

It compares exact due dates and descriptions, reporting each milestone as:

| Status | Meaning |
|--------|---------|
| `done` | Existing milestone already matches. |
| `pending` | Create or update after approval. |
| `skipped` | Missing milestone is more than 30 days past due. |

It also examines every open release milestone:

1. A stable tag `vX.Y.Z` ships milestone `X.Y.Z`.
2. A prerelease tag `vX.Y.Z-preview.N.B` or `vX.Y.Z-rc.N.B` ships milestone
   `X.Y.Z-preview.N` or `X.Y.Z-rc.N`.
3. Open issues move to the next unshipped milestone in release order.
4. The shipped milestone closes after its open issue count reaches zero.

The dry-run emits `closureOperations[]` with the shipped tag, open issues, and
exact `moveTo` milestone.

## Audit path

`audit-milestones.py`:

1. Refreshes remote release branches.
2. Sorts regular and hotfix preview/RC/stable branches in shipping order.
3. Detects shipped releases from exact remote tags.
4. Computes first-parent commit ranges between release cut points.
5. Rolls unshipped ranges into the next shipped milestone.
6. Extracts merged PR numbers from commit subjects.
7. Resolves linked issues through GitHub closing references and closing keywords.
8. Compares every PR/issue's current milestone with the release it shipped in.

The JSON `operations[]` entries include kind, number, source milestone, target
milestone, and the PR that linked an issue.

## Actions

### Synchronization

| `nextAction` | Meaning |
|--------------|---------|
| `resolve-sync-warnings` | A tagged milestone has open issues but no future unshipped milestone. |
| `confirm-sync` | Show create/update/move/close operations and ask approval. |
| `complete` | The schedule matches and every tagged milestone is closed. |

### Assignment audit

| `nextAction` | Meaning |
|--------------|---------|
| `resolve-audit-warnings` | Missing milestone or release boundary requires investigation. |
| `confirm-apply` | Shipped assignment updates await approval. |
| `complete` | Shipped assignments are correct. |

## Presenting reports

Never dump raw JSON. For sync:

```markdown
## Milestone schedule

**Source:** SkiaSharp major `{source.majorVersion}`, Skia
`m{source.currentSkiaMilestone}`

| Milestone | Action | Due | Changes |
|-----------|--------|-----|---------|
| `{operations[].title}` | `{action}` | `{dueOn}` | summarize `changes[]` |

| Shipped milestone | Tag | Move open issues to | Status |
|-------------------|-----|---------------------|--------|
| `{closureOperations[].title}` | `{tag}` | `{moveTo}` | `{status}` |
```

For audit:

```markdown
## Milestone audit

**Version line:** `{version}`
**Previous boundary:** `{previousBoundary}`

| Item | Current | Shipped in |
|------|---------|------------|
| `{kind} #{number}` | `{fromMilestone}` | `{toMilestone}` |

```

Include every warning and each open issue URL/title that will move.

## Running the paths

### Sync

```bash
python3 .agents/skills/release-milestones/scripts/sync-milestones.py \
  --count 5 \
  --dry-run
```

For `confirm-sync`, obtain approval and run `executionCommand`. Rerun the
dry-run until `complete`.

### Audit

Use a specific numeric line when known:

```bash
python3 .agents/skills/release-milestones/scripts/audit-milestones.py \
  --version 4.152.0 \
  --dry-run
```

Without `--version`, the script uses the current line in `VERSIONS.txt`.

For `confirm-apply`, obtain approval and run `executionCommand`. This is safe to
run regularly; already-correct assignments are skipped.

## Recovery

Both scripts are idempotent:

1. Rerun the dry-run.
2. Review pending operations and warnings.
3. Run the emitted execution command after approval.
4. Rerun the dry-run to confirm `complete`.

## Files

- [scripts/sync-milestones.py](scripts/sync-milestones.py) — Chromium schedule
  synchronization and tag-driven milestone rollover/closure.
- [scripts/audit-milestones.py](scripts/audit-milestones.py) — shipped
  assignment auditing.
- [scripts/milestone_common.py](scripts/milestone_common.py) — shared GitHub,
  command, and version helpers.
- [scripts/tests/](scripts/tests/) — cadence, roll-forward, closure, and CLI
  tests.
- [releasing.md](../../../documentation/dev/releasing.md) — complete release
  workflow.
