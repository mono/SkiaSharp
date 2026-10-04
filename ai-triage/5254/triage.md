# Issue Triage Report - #5254

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-10-04T05:55:45Z |
| Type | type/bug (0.99) |
| Area | area/Build (0.99) |
| Suggested action | ready-to-fix (0.98) |

**Issue Summary:** The Sync - Skia Upstream workflow failed because its staged create-pull-request output targeted a release branch that was not allowed as a base-branch override.

**Analysis:** The workflow resolves either a manually supplied base branch or a release branch and passes it to its staged pull-request completion signal. The safe-output declaration enables staged create-pull-request but does not configure allowed base branches, so the policy rejects that valid resolved target before the sync can be recorded.

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Tenets | tenet/reliability |
| Current labels | agentic-workflows |

## Evidence

### Reproduction

1. Run the Sync - Skia Upstream workflow for a release-line target.
2. Let the workflow resolve the release branch as the pull request base.
3. Observe that the staged create-pull-request safe output is rejected because that base branch is not allowlisted.

**Environment:** GitHub Actions Sync - Skia Upstream workflow run 37048491850 on the mono/SkiaSharp main branch.

**Related issues:** #5115, #5263

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/5115 - Previous occurrence of the same failure.
- https://github.com/mono/SkiaSharp/issues/5263 - Subsequent open recurrence for a release branch.
- https://github.com/mono/SkiaSharp/pull/5259 - Draft fix with regression coverage.

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Error type | build-error |
| Error message | create_pull_request: Base branch override is not allowed. Configure safe-outputs.create-pull-request.allowed-base-branches to allow per-run base overrides. |
| Repro quality | complete |

### Fix Status

Draft PR #5259 describes the allowlist fix and regression coverage, but remains open and has not yet changed the active workflow.

## Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `.github/workflows/auto-skia-sync.md` | 26-30, 146-153 | direct | Exposes a target_branch override and declares staged create-pull-request without an allowed-base-branches configuration. |
| `.github/scripts/skia-sync-detect.sh` | 190-258 | direct | Validates and resolves a supplied base branch, or derives a release branch, then emits base_branch for later workflow stages. |
| `.github/workflows/auto-skia-sync.lock.yml` | 1, 1110-1115 | direct | Compiled safe-output configuration has staged create_pull_request settings but no allowed-base-branches entry. |

**Recommended fix:** Merge the safe-output base-branch allowlist from PR #5259, then regenerate and verify the compiled workflow configuration.

## Automatable Actions

| Type | Risk | Confidence | Description |
|------|------|------------|-------------|
| update-labels | low | 0.99 | Apply the workflow bug, build area, and reliability labels. |
| link-related | low | 0.90 | Link the subsequent recurrence for consolidated workflow-failure tracking. |
