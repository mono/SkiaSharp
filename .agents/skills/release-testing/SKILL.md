---
name: release-testing
description: >
  Smoke-test an exact public SkiaSharp release package set on the current host.
  Use when a maintainer asks to smoke test a NuGet.org version. This is optional
  human validation after public publication, not a release prerequisite.
---

# Release Smoke Testing

Use this skill for host/device validation that benefits from human inspection:
native loading, console use, Linux containers, Blazor rendering, Android, iOS,
Mac Catalyst, and Windows rendering.

The supported release path is documented in
[releasing.md](../../../documentation/dev/releasing.md). Branch creation,
package publication, tags, GitHub Releases, and milestones are outside this
skill.

## Contract

- Start from one exact public SkiaSharp version.
- The public-version planner verifies the three public anchor packages and
  their matching source metadata on NuGet.org before producing test commands.
- Runner commands pin those exact versions; the integration project restores
  them through dotnet-libraries with dependencies from dotnet-public.
- Obtain user approval for the exact host-appropriate matrix before preparation
  or execution.
- Execute every approved item once even when an earlier item fails. Continue
  collecting unrelated results after runner-owned cleanup finishes.
- Never turn a failure into a skip or substitute a version, branch, feed,
  runtime, image, simulator, device, or expected artifact.
- Each platform runner checks its own prerequisites and owns setup and cleanup.
  Do not manually duplicate its SDK, Appium, device, Docker, or test commands.
- Run mobile items sequentially. Runners must not delete user-owned devices.
- Product assertions and rendering differences remain failures. Do not change
  expectations, skips, or package pins to make them pass.
- Preserve every initial failure, environment repair, retry, and artifact
  review in the final report.
- A passing report is advisory and never gates or mutates publication.

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
| `scripts/plan-release-tests.py` | Verify the public receipt and emit the exact host matrix. |
| `scripts/prepare-test-run.py` | Restore pinned local tools and clear prior integration output once. |
| `scripts/run-host-tests.py` | Run smoke, console, Linux, Blazor, Mac Catalyst, and Windows items. |
| `scripts/run-android-tests.py` | Own Android setup, temporary/reused emulator, test, and cleanup. |
| `scripts/run-ios-tests.py` | Own fresh iOS simulator creation, test, and cleanup. |
| `scripts/release_test_common.py` | Share versions, heartbeat execution, validation, package arguments, and test invocation. |

Use [setup.md](references/setup.md) for prerequisites,
[monitoring.md](references/monitoring.md) for live progress, and
[troubleshooting.md](references/troubleshooting.md) only after failures.

## Workflow

### 1. Plan and approve

```bash
python3 .agents/skills/release-testing/scripts/plan-release-tests.py \
  {exact-public-skiasharp-version}
```

The planner:

1. downloads the exact `SkiaSharp` and `SkiaSharp.HarfBuzz` packages from
   NuGet.org;
2. derives and downloads the exact `HarfBuzzSharp` dependency;
3. requires all three packages to identify the same source branch and commit;
4. pins both package versions in every runner command; and
5. reports available and host-inapplicable coverage.

NuGet.org is the authority for public availability and package source metadata.
The runner feeds execute the verified plan; they do not independently prove
publication or payload identity.

Render:

```markdown
## Release smoke-test plan

**Version:** `{release.publicPackages.SkiaSharp}`
**Commit:** `{release.commit}`
**Packages:** SkiaSharp `{version}`, HarfBuzzSharp `{version}`
**Public verification:** `{packageSources.publicVerification}`
**Runner restore:** `{packageSources.runnerRestore}`
**Host:** `{host.os}` / `{host.architecture}`

| ID | Test | Target | Estimate |
|----|------|--------|----------|
| `{id}` | `{label}` | `{target}` | `{estimatedMinutes}` min |
```

Include every `missingCoverage[]` entry and release warning. Use `ask_user`:

1. `Run the full available matrix (Recommended)`
2. `Customize the matrix`
3. `Cancel release testing`

After customization, confirm the exact final item IDs.

### 2. Prepare once

```bash
python3 .agents/skills/release-testing/scripts/prepare-test-run.py
```

### 3. Collect every result

Run every approved matrix item's emitted `command` sequentially:

1. Show the exact command and the full pending/running/passed/failed table.
2. Run it in a visible terminal canvas; use an attached async shell only when
   the canvas is unavailable.
3. Relay new `[release-test]` output and refresh done/failed/remaining state
   every five seconds. Never launch a duplicate after a delayed read.
4. Record duration, failure phase, diagnostics, artifacts, and result.
5. Continue to the next item after a failure once runner cleanup finishes.

### 4. Repair and retry

After all initial attempts, present the complete failure inventory and group
shared root causes. Apply only concrete, safe environment repairs, then retry
affected failed items. Ask before installing or upgrading software, changing
permissions, or touching user-owned devices.

Preserve initial failures and retry outcomes. Do not alter product assertions,
expected images, skips, package pins, runtimes, or targets to produce a pass.

### 5. Report the advisory result

Review expected screenshots under `output/logs/testlogs/integration/`. The final
report must include:

- immutable public version, source branch and commit, and paired package
  versions;
- public verification and runner restore sources;
- every approved ID with initial, repair, retry, and final result;
- missing or intentionally omitted host coverage; and
- screenshot paths and review status.

State plainly that smoke testing is advisory and does not gate or mutate
publication.
