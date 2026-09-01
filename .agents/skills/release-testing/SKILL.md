---
name: release-testing
description: >
  Validate an exact SkiaSharp BAR package set on the current host. Use after the
  release Build and Tests pipelines finish, before approving their packages for
  team publication.
---

# Release Package Approval Testing

```text
dnceng Build/Tests + BAR -> release-testing -> team publication
```

This skill is the human approval gate for one completed BAR build. It resolves
that build's per-build Darc feed, verifies the package family, runs the approved
host/device matrix, and records the release decision. It never publishes
packages, changes BAR state, creates tags/releases, or merges code.

## Boundaries

- Start from the exact SkiaSharp package version selected for release. When
  Maestro finds more than one producing BAR, require its exact `--bar-id`.
- Verify `SkiaSharp`, `SkiaSharp.HarfBuzz`, and the bridge's concrete
  `HarfBuzzSharp` dependency from the resolved BAR feed.
- Require all three packages to identify the BAR's source branch and commit.
- Pin the package versions and resolved feed in every runner command. Never
  substitute another build, feed, runtime, image, device, or expected artifact.
- Obtain approval for the exact host matrix before setup or execution.
- Run every approved item once. A failure blocks release approval but does not
  stop collection of unrelated results.
- Platform runners own prerequisites and cleanup. Run mobile items sequentially
  and never delete user-owned devices.
- Product assertions and rendering differences remain failures. Do not change
  expectations, skips, targets, or package pins to manufacture a pass.
- Preserve every initial failure, repair, retry, and artifact review.

## Test matrix

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
support minimums. Exact mobile targets must already be installed. The approved
plan must state every host-inapplicable or intentionally omitted item.

## Runner ownership

| Script | Responsibility |
|--------|----------------|
| `scripts/plan-release-tests.py` | Resolve the BAR/feed, verify packages, and emit the host matrix. |
| `scripts/prepare-test-run.ps1` | Restore pinned local tools and clear prior integration output once. |
| `scripts/run-host-tests.py` | Run smoke, console, Docker/Linux, Blazor, Mac Catalyst, and Windows items. |
| `scripts/run-android-tests.py` | Own Android/Appium setup, temporary or reused emulator, test, and cleanup. |
| `scripts/run-ios-tests.py` | Own iOS/Appium setup, fresh simulator, test, and cleanup. |
| `scripts/release_test_common.py` | Share package arguments, restore sources, heartbeats, validation, and test invocation. |

Do not manually duplicate runner-owned SDK, Appium, device, Docker, or test
commands. Use [setup.md](references/setup.md) for prerequisites,
[monitoring.md](references/monitoring.md) for live progress, and
[troubleshooting.md](references/troubleshooting.md) after failures.

## Runbook

### 1. Resolve and verify the BAR package family

```bash
python3 .agents/skills/release-testing/scripts/plan-release-tests.py 4.150.3
```

The planner uses `darc get-asset` to find the producing BAR. If the version is
ambiguous, rerun with the exact ID reported by the planner:

```bash
python3 .agents/skills/release-testing/scripts/plan-release-tests.py \
  4.150.3 --bar-id 329644
```

The planner:

1. reads the BAR build, source branch/commit, build link, and Darc feed location;
2. resolves that feed's GUID-backed NuGet flat-container endpoint;
3. downloads the three anchor packages from that feed;
4. verifies package IDs, versions, source metadata, and bridge dependency;
5. requires package metadata to match the selected BAR; and
6. emits host-specific commands with the exact versions and feed pinned.

Render the plan:

```markdown
## Release package test plan

**BAR:** `{release.barBuildId}` / `{release.buildNumber}`
**Build:** `{release.buildLink}`
**Source:** `{release.branch}` @ `{release.commit}`
**Packages:** SkiaSharp `{release.ciPackages.SkiaSharp}`,
HarfBuzzSharp `{release.ciPackages.HarfBuzzSharp}`
**Verified anchors:** `{release.verifiedPackageCount}`
**Darc location:** `{packageSources.barLocation}`
**GUID feed:** `{packageSources.ciVerification}`
**Flat container:** `{packageSources.resolvedFlatContainer}`
**Host:** `{host.os}` / `{host.architecture}`

| ID | Test | Target | Estimate |
|----|------|--------|----------|
| `{id}` | `{label}` | `{target}` | `{estimatedMinutes}` min |
```

Include every `missingCoverage[]`.

### 2. Approve the exact matrix

Use `ask_user`:

1. `Run the full available matrix (Recommended)`
2. `Customize the matrix`
3. `Cancel release testing`

After customization, confirm the exact final item IDs.

### 3. Prepare once

```powershell
pwsh -NoLogo -NoProfile -File `
  .agents/skills/release-testing/scripts/prepare-test-run.ps1
```

Keep preparation after approval: it changes local tool state and clears prior
integration artifacts, while planning remains read-only.

### 4. Run every approved item

Run emitted commands sequentially:

1. Show the exact command and full pending/running/passed/failed table.
2. Use a visible terminal canvas; use attached async Bash only when unavailable.
3. Relay new `[release-test]` output and refresh the complete table every five
   seconds. Never duplicate a command after a delayed read.
4. Record duration, failing phase, diagnostics, `expectedArtifacts[]`, and result.
5. Continue after failure once runner-owned cleanup finishes.

### 5. Repair and retry

After every initial attempt, present the complete failure inventory and group
shared root causes. Apply only concrete, safe environment repairs, then retry
affected items. Ask before installing/upgrading software, changing permissions,
or touching user-owned devices.

Preserve initial and retry outcomes. Do not alter assertions, expected images,
skips, package pins, runtimes, or targets to produce a pass.

### 6. Report and decide

Review screenshots under `output/logs/testlogs/integration/`. Report:

- immutable BAR/build ID, build link, source branch/commit, and package feed;
- exact SkiaSharp and HarfBuzzSharp versions;
- every approved ID with initial, repair, retry, and final result;
- missing or intentionally omitted host coverage; and
- screenshot paths and review status.

Approve the exact BAR package family for team publication only when all required
results and artifact checks pass. Otherwise state that release approval is
blocked. This skill records the decision but never performs the publication.
