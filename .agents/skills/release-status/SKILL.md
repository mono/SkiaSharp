---
name: release-status
description: >
  Check the current SkiaSharp release pipeline status. Use when the user asks
  how a release build is progressing, whether packages are ready, which build
  will be released, or what to do next. This is the second release step: resolve
  a release branch to its latest commit (or accept a commit directly), follow
  its latest connected pipeline chain, verify packages, and wait for tests.
---

# Release Status

This skill is **Step 2 of 5**:

[release-branch](../release-branch/SKILL.md) → **release-status** →
[release-testing](../release-testing/SKILL.md) →
[release-publish](../release-publish/SKILL.md) →
[release-milestones](../release-milestones/SKILL.md)

Use `pipeline-status.py` for all Azure DevOps and package-feed queries. The
agent's role is to choose the target, turn JSON into a concise human summary,
and follow the reported `nextAction`.

## Safety and selection rules

- This skill is read-only. It never queues, cancels, or retries a build.
- It fetches Git refs but never checks out or moves a branch.
- A release branch is a shortcut to its current tip commit.
- A commit SHA checks that exact commit.
- Only runs whose `sourceVersion` equals the resolved commit are considered.
- The newest `SkiaSharp-Native` run for that commit is authoritative.
- The selected `SkiaSharp` run must be a successful child of that native run.
- The selected `SkiaSharp-Tests` run must be a child of the selected managed
  run.
- Do not combine runs from different native retries.
- Do not proceed merely because managed packages were produced. Wait for the
  selected tests run to complete successfully by default.
- The user may explicitly override the test wait, but report that this departs
  from the release best practice and preserve the selected run/package metadata.

## Pipeline chain

```text
SkiaSharp-Native
  -> SkiaSharp
      -> SkiaSharp-Tests
```

| Pipeline | ID | Purpose |
|----------|----|---------|
| `SkiaSharp-Native` | 26493 | Build native binaries. |
| `SkiaSharp` | 10789 | Build managed code, sign, and publish internal packages. |
| `SkiaSharp-Tests` | 15756 | Run device and unit tests. |

## What the script guarantees

`pipeline-status.py` accepts an exact `release/{version}` branch or commit SHA.
For a branch, it resolves the current remote tip and then performs the same
commit-based query.

It:

- Selects the newest native run for the resolved commit.
- Selects the latest successful managed child of that native run.
- Selects the latest tests child of the managed run.
- Links downstream runs through `triggerInfo.pipelineId`.
- Reports failed/running/pending jobs when relevant.
- Reads release version inputs from the exact source commit.
- Derives exact SkiaSharp and HarfBuzzSharp test package versions.
- Separates stable internal test versions from eventual public versions.
- Verifies both exact test packages on the preview feed.

Script stdout is JSON for reliable agent parsing. Errors go to stderr and return
a nonzero exit code.

## Status and next actions

| `nextAction` | Meaning |
|--------------|---------|
| `wait-for-native` | No native run exists yet, or it is still running. |
| `retry-native` | The latest native run failed/canceled; tell the user to retry it. |
| `wait-for-managed-trigger` | Native succeeded, but managed has not started. |
| `wait-for-managed` | Managed is running. |
| `retry-managed` | No successful managed child exists; retry/investigate managed. |
| `wait-for-tests-trigger` | Managed succeeded, but tests have not started. |
| `wait-for-tests` | Tests are running; wait by default. |
| `retry-tests` | Tests failed/canceled; retry/investigate them. |
| `retry-package-check` | Package-feed verification failed; rerun status later. |
| `wait-for-packages` | One or both exact packages are not indexed yet. |
| `start-release-testing` | Native, managed, tests, and both packages are ready. |

Only `start-release-testing` is ready by default.

## Presenting status to the user

Never dump raw JSON. Render:

```markdown
## Release status

**Release:** `{branch}`
**Commit:** `{commit}`
**State:** `{state}`

### Pipeline

| Pipeline | Status | Run | Build number |
|----------|--------|-----|--------------|
| SkiaSharp-Native | `{nativeRun.state}` | [run `{nativeRun.runId}`]({nativeRun.url}) | `{nativeRun.buildNumber}` |
| SkiaSharp | `{managedRun.state}` | [run `{managedRun.runId}`]({managedRun.url}) | `{managedRun.buildNumber}` |
| SkiaSharp-Tests | `{testsRun.state}` | [run `{testsRun.runId}`]({testsRun.url}) | `{testsRun.buildNumber}` |

### Packages
- Test: SkiaSharp `{packageVersions.test.SkiaSharp}`,
  HarfBuzzSharp `{packageVersions.test.HarfBuzzSharp}`
- Feed: show each `packageFeed.packages` availability.
- Public: SkiaSharp `{packageVersions.public.SkiaSharp}`,
  HarfBuzzSharp `{packageVersions.public.HarfBuzzSharp}`

### Active or failed jobs
- Include failed/running/pending job names when present.

### Warnings
- Include every `warnings[]` entry.

### Next
- Translate `nextAction` using the table above.
```

Omit empty sections and missing run links. Highlight the latest native failure
even if an older managed build succeeded; the latest native attempt must have a
successful managed child before the release can advance.

When ready, retain the complete `managedRun`, `testsRun`, and
`packageVersions` objects for the release-testing handoff.

## Files

- [scripts/pipeline-status.py](scripts/pipeline-status.py) — latest connected
  chain and package-feed status.
- [scripts/tests/test_pipeline_status.py](scripts/tests/test_pipeline_status.py)
  — representative status scenarios.
- [releasing.md](../../../documentation/dev/releasing.md) — complete release
  process reference.

## Runbook

### 1. Choose the target

Prefer the exact release branch from release-branch. A commit SHA may be used to
inspect that exact commit.

### 2. Query status

```bash
python3 .agents/skills/release-status/scripts/pipeline-status.py \
  {release-branch-or-commit}
```

### 3. Present the result

Render the JSON using the summary above. Do not independently query or combine
other pipeline runs.

### 4. Follow `nextAction`

- For wait actions, report progress and stop.
- For retry actions, show the failed pipeline/jobs and Azure DevOps URL.
- Invoke [release-testing](../release-testing/SKILL.md) only for
  `start-release-testing`, unless the user explicitly overrides the test wait.
- Pass release-testing the `managedRun`, `testsRun`, and both exact test/public
  package-version pairs.
