---
name: release-testing
description: >
  Validate an exact SkiaSharp CI release package set on the current host. Use
  after the release build and tests finish, before approving its BAR packages
  for publication.
---

# Release Package Testing

Use this skill for host/device validation that benefits from human inspection:
native loading, console use, Linux containers, Blazor rendering, Android, iOS,
Mac Catalyst, and Windows rendering.

This is the human release-approval gate between the completed dnceng
Build/Tests chain and the team-owned publication pipeline. The supported
release path is documented in
[releasing.md](../../../documentation/dev/releasing.md). This skill never
publishes packages or changes BAR state.

## Contract

- Start from one exact SkiaSharp CI package version selected from the completed
  release build/BAR.
- The planner verifies the three anchor packages and their matching source
  metadata on dotnet-libraries before producing test commands.
- Runner commands pin those exact versions; the integration project restores
  them through dotnet-libraries with dependencies from dotnet-public.
- Test stable releases with exact `*-stable.{build}` packages, never the future
  bare public version.
- Obtain user approval for the matrix before preparation or execution.
- Execute every approved item once even when earlier items fail. A failed item
  blocks release approval but does not stop collection of unrelated results.
- Never turn a failure into a skip or silently substitute a runtime, image,
  device, package version, feed, branch, or expected artifact.
- Each platform runner checks its own prerequisites and owns setup/cleanup.
  Do not manually duplicate its SDK, Appium, device, Docker, or test commands.
- Run mobile items sequentially. Runners must not delete user-owned devices.
- Product assertions and rendering differences remain failures. Do not change
  expectations, skips, or package pins to make them pass.
- Preserve every initial failure, environment repair, retry, and artifact
  review in the final report.
- This skill never publishes packages, changes BAR state, creates tags/releases,
  or merges code.

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
| `scripts/plan-release-tests.py` | Read-only CI package verification and exact host matrix. |
| `scripts/prepare-test-run.py` | Restore pinned local tools and clear prior integration output once. |
| `scripts/run-host-tests.py` | Smoke, console, Docker/Linux, Blazor, Mac Catalyst, and Windows host items. |
| `scripts/run-android-tests.py` | Android environment, Appium, temporary/reused emulator, test, and cleanup. |
| `scripts/run-ios-tests.py` | Fresh iOS simulator, Appium test, and cleanup. |
| `scripts/release_test_common.py` | Shared versions, heartbeat execution, validation, package arguments, and test invocation. |

Use [setup.md](references/setup.md) for prerequisites,
[monitoring.md](references/monitoring.md) for live progress, and
[troubleshooting.md](references/troubleshooting.md) only after failures.

## Workflow

### 1. Plan and approve

```bash
python3 .agents/skills/release-testing/scripts/plan-release-tests.py \
  {exact-ci-skiasharp-version}
```

The planner:

1. downloads the exact `SkiaSharp` and `SkiaSharp.HarfBuzz` packages from
   dotnet-libraries;
2. derives and downloads the exact `HarfBuzzSharp` dependency;
3. requires all three packages to identify the same source branch and commit;
4. pins both package versions in every runner command; and
5. reports available and host-inapplicable coverage.

dotnet-libraries is the authority for the CI package family under test.
dotnet-public supplies dependencies during runner restore. Never substitute
NuGet.org or a different package version to make a test pass.

Render:

```markdown
## Release package test plan

**CI version:** `{release.ciPackages.SkiaSharp}`
**Commit:** `{release.commit}`
**Packages:** SkiaSharp `{version}`, HarfBuzzSharp `{version}`
**CI verification:** `{packageSources.ciVerification}`
**Runner restore:** `{packageSources.runnerRestore}`
**Host:** `{host.os}` / `{host.architecture}`

| ID | Test | Target | Estimate |
|----|------|--------|----------|
| `{id}` | `{label}` | `{target}` | `{estimatedMinutes}` min |
```

Include every `missingCoverage[]`. Use `ask_user`:

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
expectations, skips, package pins, runtimes, or targets to make them pass.

### 5. Report the release-approval gate

Review expected screenshots under `output/logs/testlogs/integration/`. The final
report must include:

- immutable CI package version, source branch and commit, and paired package
  versions;
- CI verification and runner restore sources;
- every approved ID with initial, repair, retry, and final result;
- missing or intentionally omitted host coverage; and
- screenshot paths and review status.

Approve the exact package family for the team publication pipeline only when
all required results and artifact checks pass. Otherwise report that release
approval is blocked. This skill records the decision but never publishes or
changes BAR state.
