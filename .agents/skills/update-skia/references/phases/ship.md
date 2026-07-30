# Phase 11: Ship

Phase 10 is a hard prerequisite. Never create a failing update PR.

## PR targets

| Repository | Head | Base |
|---|---|---|
| mono/skia | `{HEAD_BRANCH}` | `{SKIA_BASE_BRANCH}` |
| mono/SkiaSharp | `{HEAD_BRANCH}` | `{BASE_BRANCH}` |

Use the repository PR templates, link both PRs to each other, and include:

- Upstream ref and SHA.
- Conflict/fork-patch dispositions.
- Dependency decisions.
- C API and generated/managed API changes.
- Exact native, managed, and per-host test results.
- Cross-platform work not executed locally.

The automated workflow replaces this phase: it writes summary fragments and a handoff file,
then its deterministic post-step renders dedicated templates, pushes both branches, and creates
cross-linked draft PRs.

## Merge sequence

Do not merge without explicit approval.

1. Merge mono/skia first.
2. Fetch the resulting commit on `{SKIA_BASE_BRANCH}`.
3. Update the parent PR's submodule pointer to that branch commit.
4. Wait for parent CI.
5. Merge the parent PR.

Never leave the parent pointing at an orphaned PR-head commit.
