---
name: release-testing
description: >
  Run integration tests against the exact SkiaSharp release packages before
  publishing. Use when the user asks to test or verify a release, run the
  release matrix, test packages on Android/iOS/Mac/Windows/Blazor/Linux, or
  continue after release-status. This is the third release step: plan the
  host-appropriate matrix, obtain approval, execute every item, repair
  environment failures, and report the final release gate.
---

# Release Testing

This skill is **Step 3 of 5**:

[release-branch](../release-branch/SKILL.md) →
[release-status](../release-status/SKILL.md) → **release-testing** →
[release-publish](../release-publish/SKILL.md) →
[release-milestones](../release-milestones/SKILL.md)

## Contract

- Run the read-only planner first and preserve its exact source commit, managed
  run, tests run, and paired package versions throughout testing.
- Obtain user approval for the matrix before preparation or execution.
- Test stable releases with exact `*-stable.{build}` packages, never the future
  bare public version.
- Execute every approved item once even when earlier items fail. A failed item
  blocks publication but does not stop collection of unrelated results.
- Never turn a failure into a skip or silently substitute a runtime, image,
  device, package version, or expected artifact.
- Each platform runner checks its own prerequisites and owns setup/cleanup.
  Do not manually duplicate its SDK, Appium, device, Docker, or test commands.
- Run mobile items sequentially. Runners must not delete user-owned devices.
- Invoke release-publish only after every approved item and artifact check has a
  final passing result.
- This skill never publishes packages, creates tags/releases, or merges code.

## Fixed matrix

| ID | Coverage | Host |
|----|----------|------|
| `smoke` | Native loading | All |
| `console` | Console and HarfBuzzSharp | All |
| `linux` | Linux packages in Docker | All |
| `blazor` | Native WASM in Chromium | All |
| `android-26` | Minimum Android test target | All |
| `android-37.1` | Maximum Android test target | All |
| `maccatalyst` | Mac Catalyst rendering | macOS |
| `ios-18.6` | Minimum iOS test target | macOS |
| `ios-26.5` | Maximum iOS test target | macOS |
| `windows` | MAUI Windows rendering | Windows |

iOS 18.6 and Android 26 are minimum **release-test targets**, not product
support minimums. Exact mobile targets must already be installed. Missing or
host-inapplicable coverage must be explicit in the approved plan.

## Script contract

| Script | Responsibility |
|--------|----------------|
| `scripts/plan-release-tests.py` | Read-only release-status handoff and exact host matrix. |
| `scripts/prepare-test-run.py` | Restore pinned local tools and clear prior integration output once. |
| `scripts/run-host-tests.py` | Smoke, console, Docker/Linux, Blazor, and Windows items. |
| `scripts/run-android-tests.py` | Android environment, Appium, temporary/reused emulator, test, and cleanup. |
| `scripts/run-apple-tests.py` | Fresh iOS simulator or Mac Catalyst Appium test and cleanup. |
| `scripts/release_test_common.py` | Shared versions, heartbeat execution, validation, package arguments, and test invocation. |

Planner actions:

| `nextAction` | Response |
|--------------|----------|
| `approve-test-matrix` | Present and obtain approval. |
| `wait-for-tests-trigger` / `wait-for-tests` | Return to release-status, unless the user explicitly overrides only this wait with `--allow-incomplete-ci`. |
| `retry-tests` | Investigate/retry failed CI tests; never override them here. |
| Anything else | Return to release-status. |

Use [setup.md](references/setup.md) for prerequisites,
[monitoring.md](references/monitoring.md) for live progress, and
[troubleshooting.md](references/troubleshooting.md) only after failures.

## Workflow

### 1. Plan and approve

```bash
python3 .agents/skills/release-testing/scripts/plan-release-tests.py \
  {release-branch-or-commit}
```

If `readyToPlan` is false, report `nextAction` and stop. Otherwise render:

```markdown
## Release test plan

**Release:** `{release.branch}`
**Commit:** `{release.commit}`
**Managed/tests runs:** `{release.managedRunId}` / `{release.testsRunId}`
**Packages:** SkiaSharp `{test version}`, HarfBuzzSharp `{test version}`
**Host:** `{host.os}` / `{host.architecture}`

| ID | Test | Target | Estimate |
|----|------|--------|----------|
| `{id}` | `{label}` | `{target}` | `{estimatedMinutes}` min |
```

Include every `missingCoverage[]` and release warning. Use `ask_user`:

1. `Run the full available matrix (Recommended)`
2. `Customize the matrix`
3. `Cancel release testing`

Confirm the exact final IDs after customization.

### 2. Prepare once

```bash
python3 .agents/skills/release-testing/scripts/prepare-test-run.py
```

### 3. Collect every result

For each approved item, run its emitted `command` sequentially:

1. Show the exact command and the full pending/running/passed/failed table.
2. Run it in a visible terminal canvas; use an attached async shell only when
   the canvas is unavailable.
3. Relay new `[release-test]` output and refresh done/failed/remaining state
   every five seconds. Never launch a duplicate command after a delayed read.
4. Record duration, failure phase, diagnostics, artifacts, and result.
5. Continue after failure once runner-owned cleanup finishes.

### 4. Repair and retry

After all initial attempts, present the complete failure inventory and group
shared root causes. Apply only concrete, safe environment repairs, then retry
affected failed items. Ask before installing/upgrading software, changing
permissions, or touching user-owned devices. Preserve initial failures and all
retry outcomes.

Product assertions and rendering differences remain failures; do not alter
expectations, skips, or package pins to make them pass.

### 5. Report the gate

Review expected screenshots under `output/logs/testlogs/integration/`. The final
report must include:

- Immutable release/run/package identity.
- Every approved ID with initial, repair, retry, and final result.
- Missing or intentionally omitted coverage.
- Screenshot paths and review status.

Proceed to [release-publish](../release-publish/SKILL.md) only when all final
results and artifact checks pass.
