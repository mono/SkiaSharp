---
name: release-status
description: >
  Check the current SkiaSharp release pipeline status. Use when the user asks
  how a release build is progressing, whether packages are ready, which build
  will be released, or what to do next. This is the second release step: resolve
  one exact commit, follow its connected native/managed/tests chain, verify both
  exact packages, and report the next action.
---

# Release Status

This skill is **Step 2 of 5**:

[release-branch](../release-branch/SKILL.md) → **release-status** →
[release-testing](../release-testing/SKILL.md) →
[release-publish](../release-publish/SKILL.md) →
[release-milestones](../release-milestones/SKILL.md)

## Contract

- Use `pipeline-status.py` for all Git, Azure DevOps, and package-feed queries.
- This skill is read-only: it may fetch refs but never checks out a branch or
  queues, cancels, or retries a build.
- Resolve a branch to its current remote tip; preserve a supplied commit exactly.
- Select one connected chain for that commit. The newest native run is
  authoritative; managed and tests must descend from it.
- Never combine downstream runs from different native attempts.
- Native, managed, tests, and both exact preview-feed packages must be ready
  before the default testing handoff.
- The user may explicitly override only the wait for incomplete CI tests.
  Preserve all selected run/package metadata and report the override.

## Pipeline chain

| Pipeline | ID | Role |
|----------|----|------|
| `SkiaSharp-Native` | 26493 | Native binaries |
| `SkiaSharp` | 10789 | Managed build/sign/internal packages |
| `SkiaSharp-Tests` | 15756 | Connected tests |

The script links downstream runs through Azure trigger metadata and derives
SkiaSharp/HarfBuzzSharp test and eventual public versions from the exact source
commit.

## Actions

| `nextAction` | Response |
|--------------|----------|
| `wait-for-native` | Report that native has not started or is running. |
| `retry-native` | Show the authoritative failed/canceled native run. |
| `wait-for-managed-trigger` | Native passed; managed has not started. |
| `wait-for-managed` | Report managed progress. |
| `retry-managed` | Show managed failure/missing successful child. |
| `wait-for-tests-trigger` | Managed passed; tests have not started. |
| `wait-for-tests` | Report tests progress; wait by default. |
| `retry-tests` | Show failed/canceled tests and jobs. |
| `retry-package-check` | Report package-feed query failure. |
| `wait-for-packages` | Report which exact package is not indexed. |
| `start-release-testing` | Hand the immutable chain and packages to release-testing. |

Only `start-release-testing` is ready by default.

## Workflow

Run:

```bash
python3 .agents/skills/release-status/scripts/pipeline-status.py \
  {release-branch-or-commit}
```

Render:

```markdown
## Release status

**Release:** `{branch}`
**Commit:** `{commit}`
**State:** `{state}`

| Pipeline | Status | Run | Build |
|----------|--------|-----|-------|
| Native | `{nativeRun.state}` | [run `{nativeRun.runId}`]({nativeRun.url}) | `{nativeRun.buildNumber}` |
| Managed | `{managedRun.state}` | [run `{managedRun.runId}`]({managedRun.url}) | `{managedRun.buildNumber}` |
| Tests | `{testsRun.state}` | [run `{testsRun.runId}`]({testsRun.url}) | `{testsRun.buildNumber}` |

**Test packages:** SkiaSharp `{packageVersions.test.SkiaSharp}`,
HarfBuzzSharp `{packageVersions.test.HarfBuzzSharp}`
**Public versions:** SkiaSharp `{packageVersions.public.SkiaSharp}`,
HarfBuzzSharp `{packageVersions.public.HarfBuzzSharp}`
**Next:** translate `{nextAction}`
```

Include package availability, active/failed jobs, and every warning. Omit missing
run rows/links. Do not independently query or replace the script-selected chain.

For `start-release-testing`, invoke
[release-testing](../release-testing/SKILL.md) with the complete `managedRun`,
`testsRun`, and exact test/public package pairs. For wait/retry actions, report
the relevant run URL and stop.

See [releasing.md](../../../documentation/dev/releasing.md) for the complete
release process.
