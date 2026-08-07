---
name: release-testing
description: >
  Run integration tests against the exact SkiaSharp release packages before
  publishing. Use when the user asks to test or verify a release, run the
  release matrix, test packages on Android/iOS/Mac/Windows/Blazor/Linux, or
  continue after release-status. This is the third release step: plan the
  fullest host-appropriate matrix, obtain user approval, then execute it manually.
---

# Release Testing

This skill is **Step 3 of 5**:

[release-branch](../release-branch/SKILL.md) →
[release-status](../release-status/SKILL.md) → **release-testing** →
[release-publish](../release-publish/SKILL.md) →
[release-milestones](../release-milestones/SKILL.md)

Use the scripts for status/package handoff, host-specific matrix generation,
pinned-tool restoration, test setup/execution/cleanup, and output cleanup. The agent
presents the proposed matrix, obtains user approval, and executes the approved
items sequentially in a complete collection pass. It then reports all failures,
repairs safe environment problems, and retries only the affected items.

## Safety and ownership

- The user must approve the test matrix before any setup or test command runs.
- The default is the fullest platform matrix for the current host; runtime and
  tool prerequisites are checked only after approval.
- A failed test, timeout, crash, or screenshot mismatch blocks the release.
  Only a concrete environment repair followed by a successful rerun can clear
  an environment failure; product/test failures remain release failures.
- A failed matrix item does not stop the collection pass. Record it, preserve
  diagnostics, clean up its owned resources, and continue with every other
  approved item.
- Never convert a failure into a skip.
- Hardware/platform omissions must be explicit in the approved matrix.
- Do not use eventual bare stable versions before publication. Test the exact
  `*-stable.{build}` packages selected by release-status.
- Every command must use the same managed run, source commit, and package
  versions from the approved plan.
- Run mobile tests sequentially. Only one Android emulator should run at a
  time.
- Never hide a test behind one long blocking call. Prefer a dedicated visible
  terminal canvas so the user sees stdout live. If no terminal canvas is
  available, use an attached asynchronous shell job. Report new output plus the
  matrix state every five seconds until it exits.
- Each per-test script owns setup, test execution, and cleanup in one checked
  operation.
- Stop the matrix only when continuing would invalidate every result, such as
  losing the pinned source/package identity. A crashed Docker daemon, Appium
  failure, missing runtime, or broken device affects its item, not unrelated
  items.
- This skill never publishes packages, tags a release, or merges a PR.

## What is scripted

### Matrix planning

`plan-release-tests.py` is read-only. It:

- Runs release-status for the requested release branch or commit.
- Requires native, managed, tests, and both exact packages to be ready by
  default.
- Carries forward immutable managed/test run metadata and exact test/public
  package versions.
- Selects the default matrix for the current host OS.
- Generates one exact `run-tests.py` command per matrix item.
- Selects the fullest host-appropriate matrix by default.
- On macOS, reads the selected Xcode and installed simulator inventory, applies
  the Xcode/Appium compatibility policy, and pins exact minimum/maximum iOS
  runtimes plus compatible iPhone device types.

If CI tests are not ready, the planner returns no matrix. Use
`--allow-incomplete-ci` only after the user explicitly overrides the normal
release-status wait. Managed packages and both feed packages must still be
ready.

### Planner actions

| `nextAction` | Meaning |
|--------------|---------|
| `approve-test-matrix` | Render the host matrix and obtain user approval. |
| `wait-for-tests-trigger` / `wait-for-tests` | Return to release-status or explicitly override only this wait. |
| `retry-tests` | Investigate/retry the connected CI tests; failed CI tests cannot be overridden here. |
| Any other release-status action | Return to release-status; native/managed/package readiness cannot be overridden here. |

### Test-run preparation

`prepare-test-run.py` runs only after matrix approval. It restores the pinned
local .NET tools, verifies them, and safely clears
`output/logs/testlogs/integration/`.

### Manual execution

Test execution remains agent-driven because it requires progress reporting,
device observation, failure investigation, screenshot review, and user
decisions. Each selected matrix item has one host-quoted `command` that invokes a
`run-tests.py` subcommand with the two exact package versions. Run that command
from the repository root. The runner discovers its concrete device/runtime,
resolves safe process-local configuration, checks prerequisites, and performs
cleanup after failure. A nonzero exit records one failed item; it must not end
the agent's loop over the approved matrix.

`run-tests.py` emits machine-recognizable `[release-test]` records when the item
starts, whenever a child command starts, every five seconds while that command
is silent, when it exits, and when the item passes or fails. Run each matrix
command in a dedicated terminal canvas and read its rendered output every five
seconds. Reuse that terminal for sequential items. If the canvas is unavailable,
use an attached asynchronous Bash session and read that same shell ID. Do not
detach it, launch it through a background agent, rerun it while active, or wait
silently for completion.

The runner does not broadly provision workloads, Appium, Android SDK packages,
Playwright browsers, or Apple runtimes. It does restore repository-pinned
`.NET` tools. For every Android run, it resolves `ANDROID_HOME` and `JAVA_HOME`
with the pinned `dotnet android` tool and applies them only to the runner
process and its children.

Treat a missing deterministic configuration as something to resolve, not a
reason to end the collection pass. Restore pinned tools and discover SDK/JDK
paths before reporting a blocker. Ask the user only when an actual workload,
SDK image/runtime, Appium installation, or host capability is absent.

## Default matrix policy

| ID | Coverage | Default |
|----|----------|---------|
| `smoke` | Native library loading | Yes |
| `console` | Console apps and HarfBuzzSharp | Yes |
| `linux` | Linux container packages | Yes; runner checks Docker Linux |
| `blazor` | Native WASM app in preinstalled Chromium | Yes |
| `maccatalyst` | MAUI Mac Catalyst | On macOS |
| `android-26` | Exact Android 26 image (UiAutomator2 minimum) | Yes |
| `android-37.1` | Exact Android 37.1 image | Yes |
| `ios-{minimum}` | Oldest installed runtime in the Xcode-compatible minimum major | On macOS |
| `ios-{maximum}` | Newest installed iOS 26 runtime | On macOS |
| `windows` | MAUI Windows | On Windows |

Minimum/maximum mobile coverage must use distinct versions. Report missing
coverage rather than silently collapsing the matrix.

## Mobile tooling

Use the pinned local tools from `.config/dotnet-tools.json`:

- `dotnet tool run android -- sdk list`
- `dotnet tool run android -- avd create/start/delete`
- `dotnet tool run apple -- simulator create/list/boot/delete`

Mobile selectors are exact versions. Android targets are fixed; iOS targets are
resolved from the selected Xcode:

| Selected toolchain | Minimum iOS major | Maximum iOS major |
|--------------------|-------------------|-------------------|
| Xcode 26.x | iOS 15 | iOS 26 |
| Xcode 27.x or newer | iOS 18 | iOS 26 |

Within each major, planning chooses the oldest installed minimum runtime and
newest installed maximum runtime. Xcode 27 currently uses iOS 26 as its maximum
because the MAUI/Appium test app cannot launch reliably on iOS 27 simulators.
For example, this machine resolves to iOS 18.2 and iOS 26.5. This is release-test
coverage policy only; it does not change SkiaSharp's declared iOS product
minimum.

Use optional `--device {hardware-profile}` to override the default `pixel`
Android profile or the automatically selected compatible iPhone type. Use
Android-only `--device-id {serial}` to target an already connected
emulator/physical device.

The runner reads only installed Android packages and Apple simulators. It
requires the exact target, validates the selected iPhone type against that
runtime, creates a uniquely named temporary device, boots it with a wait and
timeout, and deletes it in `finally`. Android accepts Google APIs, Google Play,
and their 16 KB-page image variants for the host architecture.

If exactly one Android emulator is already running, the runner validates its API
and reuses it instead of demanding shutdown. With multiple devices, select one
using `--device-id`. User-owned devices are never deleted.

All Android SDK, AVD, device-list, and device-property operations go through the
pinned `dotnet android` tool. All simulator lifecycle operations go through
`dotnet apple`.

For MAUI items, the runner requires Appium and the platform driver to already be
installed at the pinned release-test versions, rejects an existing server on
port 4723, and runs
`appium driver doctor {driver}`. Before Android doctor checks, the runner
discovers SDK/JDK paths and exports them process-locally.

Do not replace these commands with raw `adb`, `sdkmanager`, `avdmanager`, `emulator`,
or `xcrun simctl` flows unless the pinned tool itself is broken and the user
approves the fallback.

## Presenting the proposed matrix

Never dump raw planner JSON. Render:

```markdown
## Release test plan

**Release:** `{release.branch}`
**Commit:** `{release.commit}`
**Managed run:** `{release.managedRunId}`
**Tests run:** `{release.testsRunId}`
**Packages:** SkiaSharp `{test version}`, HarfBuzzSharp `{test version}`
**Host:** `{host.os}` / `{host.architecture}`

| ID | Test | Target | Estimate |
|----|------|--------|----------|
| `{id}` | `{label}` | `{target}` | `{estimatedMinutes}` min |

| Kind | Detail |
|------|--------|
| Missing coverage | Include every `missingCoverage[]` entry |
| Warning | Include every `release.warnings[]` entry |
```

Mark IDs in `defaultSelection` as the recommended matrix. Explain that each
approved item restores pinned tools and resolves process-local paths, while
leaving broader SDK/runtime/workload installation under user control.

## Result policy

Record each approved item as `pending`, `passed`, or `failed`, including its
target, duration, failure phase, diagnostic artifact paths, and every attempt.

Use two passes:

1. **Collection pass:** Run every approved item once. Do not perform deep repair
   between items; only preserve diagnostics and restore isolation needed by the
   next item.
2. **Repair pass:** Present the complete failure inventory, group failures by
   root cause, make safe deterministic environment repairs, and retry only
   affected failed items.

Safe repairs include restoring pinned tools, rediscovering process-local paths,
starting or restarting an already-installed service such as Docker, and
cleaning resources created by the runner. Ask before installing/upgrading
workloads, SDKs, runtimes, Appium, or system software, changing permissions, or
touching user-owned devices.

Do not blindly retry. Each retry must follow a concrete diagnosis or repair.
Keep the first failure in the report even when a retry passes. Product failures,
assertion failures, and rendering differences stay failed; never change
versions, expectations, or skips to make the matrix green.

Proceed to release-publish only when:

- Every approved item has a final `passed` result.
- Both minimum/maximum Android items passed when available.
- Both minimum/maximum iOS items passed when available.
- Expected screenshots were produced and visually reviewed.
- Any omitted platform or unavailable coverage is explicitly recorded.

The final report must include:

- Managed and tests run IDs, build number, source branch, and source commit.
- Exact test package versions and eventual public versions.
- The approved matrix, initial result, repair action, retry result, and final
  result for each item.
- A consolidated failure table from the collection pass.
- Missing/omitted coverage.
- Screenshot paths for visual tests.

## Files

- [scripts/plan-release-tests.py](scripts/plan-release-tests.py) — read-only
  status handoff and host-specific matrix planning.
- [scripts/prepare-test-run.py](scripts/prepare-test-run.py) — verify pinned
  tools and reset test output.
- [scripts/run-tests.py](scripts/run-tests.py) — checked setup, execution, and
  cleanup subcommands for every matrix item.
- [scripts/tests/](scripts/tests/) — planner, preparation, and runner tests.
- [references/setup.md](references/setup.md) — prerequisite details.
- [references/monitoring.md](references/monitoring.md) — long-running test
  progress and diagnostics.
- [references/troubleshooting.md](references/troubleshooting.md) — failure
  investigation.

## Runbook

### 1. Plan the matrix

```bash
python3 .agents/skills/release-testing/scripts/plan-release-tests.py \
  {release-branch-or-commit}
```

If `readyToPlan` is false, report `nextAction` and stop. If the user explicitly
overrides only the CI tests wait, rerun with
`--allow-incomplete-ci`.

### 2. Present and approve

Render the proposed matrix using the table above. Use `ask_user`:

1. `Run the full available matrix (Recommended)`
2. `Customize the matrix`
3. `Cancel release testing`

For customization, ask which matrix IDs to include or omit. Confirm the final
list before running anything.

### 3. Prepare the approved run

Run the preparation script directly:

```bash
python3 .agents/skills/release-testing/scripts/prepare-test-run.py
```

It restores/verifies pinned tools and clears old integration-test artifacts.

### 4. Run approved items sequentially

For each approved matrix item:

1. Show the full matrix table with completed, failed, running, and remaining
   items.
2. Announce the item, target, exact command, and estimate.
3. Start its single `command` in a visible terminal canvas. Keep its instance ID
   for output reads and later matrix items. Fall back to an attached
   asynchronous Bash job only when the canvas is unavailable.
4. Every five seconds, read newly rendered terminal output (or output from the
   fallback shell ID) and report:
   current item/phase, elapsed time, completed count, failures, and remaining
   item IDs. Do not repeat old log lines.
5. On exit, show the command result and immediately refresh the matrix table.
6. Record the result, failure phase, diagnostics, duration, and artifact paths.
7. On failure, perform only cleanup/isolation needed for subsequent items, then
   continue to the next approved item.

Follow [monitoring.md](references/monitoring.md) for progress during silent MAUI
builds. The runner heartbeat means five seconds without either runner output or
an agent update is itself a monitoring problem. Do not let a nonzero command
exit terminate the collection pass.

### 5. Diagnose, repair, and retry failures

After every approved item has an initial result:

1. Present all failures together.
2. Group shared causes so one repair can cover every affected item.
3. Apply safe repairs, asking only for changes that require user approval.
4. Retry only the affected failed items.
5. Repeat only when new evidence identifies another concrete repair.

Use [troubleshooting.md](references/troubleshooting.md) to distinguish host
infrastructure failures from package/product failures.

### 6. Verify and report

Review screenshots under `output/logs/testlogs/integration/`, ensure every
approved item has a result, and present the final report described above.

Invoke [release-publish](../release-publish/SKILL.md) only when all approved
tests and artifact checks pass.
