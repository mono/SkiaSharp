---
name: release-milestones
description: >
  Reconcile and advance SkiaSharp GitHub milestones. Use whenever the user asks
  to correct shipped PR/issue assignments, maintain upcoming Chromium/Skia
  milestone dates, roll open work forward, close released milestones, check
  milestone hygiene, or continue after release-publish. This is the final
  release step and a standalone maintenance workflow.
---

# Release Milestones

This skill is **Step 5 of 5**:

[release-branch](../release-branch/SKILL.md) →
[release-status](../release-status/SKILL.md) →
[release-testing](../release-testing/SKILL.md) →
[release-publish](../release-publish/SKILL.md) →
**release-milestones**

## Contract

- Use scripts for schedule calculation, release ranges, GitHub queries,
  assignments, issue rollover, and closure.
- Run the selected path with `--dry-run`, present every operation/warning, and
  obtain approval before executing its emitted command.
- A real remote release tag makes its matching milestone eligible for closure.
- Advance moves open issues and pull requests to the next unshipped milestone
  before closing.
- After moving items, Advance waits for GitHub to reflect those exact moves
  before closure; a newly added item still blocks closure.
- Reconcile assigns merged PRs and linked issues to the release where they
  shipped; unshipped preview/RC ranges roll into the next shipped release.
- Main commits after the last release cut remain unassigned.
- These scripts never modify release branches, tags, packages, or GitHub
  Releases.

## Paths

| Path | Script | Responsibility |
|------|--------|----------------|
| **Reconcile** | `scripts/reconcile-release-assignments.py` | Reconcile shipped PR/linked-issue assignments for a numeric release line. |
| **Advance** | `scripts/advance-release-milestones.py` | Maintain upcoming dates, move open issues/PRs, and close tagged milestones. |

The release-publish handoff runs Reconcile first from its emitted command, then
runs Advance. Either path can also run independently.

## Actions

| Path | `nextAction` | Response |
|------|--------------|----------|
| Reconcile | `resolve-reconciliation-warnings` | Investigate a missing milestone/release boundary. |
| Reconcile | `confirm-reconcile-assignments` | Approve and run assignment updates. |
| Reconcile | `complete` | Assignments are correct. |
| Advance | `resolve-advance-warnings` | Create/identify a future unshipped destination. |
| Advance | `confirm-advance-milestones` | Approve schedule, item-move, and closure operations. |
| Advance | `complete` | Schedule matches and all tagged milestones are closed. |

## Workflow

### Reconcile assignments

Use the numeric release line from release-publish when available:

```bash
python3 .agents/skills/release-milestones/scripts/reconcile-release-assignments.py \
  --version {X.Y.Z} \
  --dry-run
```

Without `--version`, the script uses `scripts/VERSIONS.txt`.

Render:

```markdown
## Milestone assignment reconciliation

**Version:** `{version}`
**Previous boundary:** `{previousBoundary}`

| Item | Current | Shipped in |
|------|---------|------------|
| `{kind} #{number}` | `{fromMilestone}` | `{toMilestone}` |
```

### Advance milestones

```bash
python3 .agents/skills/release-milestones/scripts/advance-release-milestones.py \
  --count 3 \
  --dry-run
```

Render:

```markdown
## Milestone schedule

**Source:** SkiaSharp `{source.majorVersion}`, Skia
`m{source.currentSkiaMilestone}`

| Milestone | Action | Due | Changes |
|-----------|--------|-----|---------|
| `{operations[].title}` | `{action}` | `{dueOn}` | summarize `changes[]` |

| Shipped milestone | Tag | Move open items to | Status |
|-------------------|-----|--------------------|--------|
| `{closureOperations[].title}` | `{tag}` | `{moveTo}` | `{status}` |
```

For either path:

1. Include every warning and each open item kind, URL, and title that will move.
2. For a confirmation action, obtain approval and run `executionCommand`.
3. Rerun the same dry-run until it reports `complete`.

See [releasing.md](../../../documentation/dev/releasing.md) for the complete
release process.
