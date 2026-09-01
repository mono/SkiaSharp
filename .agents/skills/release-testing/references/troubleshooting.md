# Troubleshooting Guide

Quick reference for common errors and fixes.

## Collection and repair workflow

Do not investigate one failure so deeply that the rest of the approved matrix
never runs. During the collection pass, capture diagnostics, allow runner
cleanup, and continue. After all initial attempts:

1. Group failures as environment/prerequisite, transient infrastructure, or
   product/test failures.
2. Repair shared environment causes first.
3. Retry only items affected by a concrete repair.
4. Preserve both the initial failure and retry outcome in the final report.

Starting or restarting an already-installed service and restoring pinned local
tools are safe repairs. Ask before installing/upgrading software, changing
system permissions, or modifying user-owned devices. A product assertion or
rendering mismatch is not repairable by changing the test, expected image,
package version, or skip policy.

## Package Resolution Errors

### Packages appear missing after the release build

**Symptom:** The selected release build/BAR is complete, but the planner cannot
resolve or read its exact package feed.

**Cause:** Darc authentication failed, the version is outside `--max-age`, more
than one BAR produced it, the build has no NuGet feed location, or its package
family is incomplete.

**Fix:** Keep the exact version. Authenticate with `darc login`, increase
`--max-age` only when needed, or use the exact `--bar-id` reported by the
planner.

**`multiple BAR builds contain ...`** — select the BAR approved for release and
rerun with `--bar-id`.

**`BAR build ... has no NuGet feed locations`** — the build has not published a
testable Darc feed. Inspect the BAR/build rather than choosing another feed.

**`BAR feed package ... is unavailable`** — the per-build feed is incomplete.
Wait for indexing or repair that build; do not substitute another BAR.

**`CI package source metadata does not match`** — the selected BAR package
family is not coherent under the current policy. Do not approve it.

**`BAR build and package source metadata do not match`** — the feed does not
belong to the Darc-selected build. Do not execute the matrix.

**`does not pin one concrete HarfBuzzSharp dependency`** — the bridge package
does not pin one concrete dependency version. Do not infer one from another
feed.

| Wrong | Correct |
|-------|---------|
| Newest matching BAR | Exact release-approved BAR ID |
| Global feed alias | Per-build feed returned by Darc |
| Partial prefix search | Exact package ID and version |

---

## Build Errors

| Error | Cause | Fix |
|-------|-------|-----|
| Local `android` / `apple` tool is unavailable | Pinned manifest has not been restored | Run `pwsh -NoLogo -NoProfile -File .agents/skills/release-testing/scripts/prepare-test-run.ps1`; it performs `dotnet tool restore` |
| `the maui workload is not installed` | Missing workload | Record affected MAUI items, continue unrelated coverage, then ask whether to install `maui` or explicitly amend the matrix |
| `the wasm-tools workload is not installed` | Missing workload | Record Blazor as failed, continue unrelated coverage, then ask whether to install `wasm-tools` or explicitly amend the matrix |
| `SkiaSharpVersion must be the exact package version` | Missing version param | Add both exact SkiaSharp and HarfBuzzSharp versions emitted by the planner |
| `HarfBuzzSharpVersion must be the exact package version` | Missing version param | Use the distinct HarfBuzzSharp version emitted by the planner |
| Generated platform package cannot be restored | A satellite package such as `SkiaSharp.Views.Blazor`, `SkiaSharp.Views.Maui.Controls`, or `SkiaSharp.NativeAssets.Linux.NoDependencies` is unavailable at the exact version | Confirm the satellite package exists on the selected BAR feed and retry the same build; dependencies continue to resolve from dotnet-public |

## Appium Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `Appium is not installed or is not on PATH` | Appium is absent or npm global binaries are unavailable | Record affected MAUI items, continue unrelated coverage, then ask whether to install/configure Appium before retrying |
| `the Appium ... driver is not installed` | Required platform driver is absent | Record affected items, continue unrelated coverage, then ask whether to install that driver before retrying |
| `Appium ... is required; found ...` | Server or driver differs from the pinned release-test version | Record affected items, continue unrelated coverage, then ask whether to switch versions before retrying |
| Doctor reports `ANDROID_HOME` / `JAVA_HOME` missing | Pinned path discovery failed | Run `dotnet tool run android -- sdk find` and `jdk find` to diagnose the selected installations |
| Driver doctor fails after path resolution | Required platform environment is incomplete | Fix every remaining required doctor finding; optional recording/streaming tools are not needed |
| `Connection refused` | Port conflict | Appium auto-starts on 4723; check for conflicts |
| `Session creation timeout` | First run building WDA | Wait - WebDriverAgent builds on first iOS/Mac run |
| `Invalid bundle identifier` | Wrong bundleId | Tests extract from csproj automatically |

## Simulator/Emulator Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `Android ... is not installed` | No installed image matches the exact version and host architecture | Record that item, continue, then inspect `dotnet tool run android -- sdk list --installed --format json` and ask whether to install it or explicitly amend the matrix |
| `Android SDK package ... is not installed` | Emulator or platform tools are absent | Record affected Android items, continue, then ask whether to install the missing package |
| `hvf is not enabled` / `mprotect failed: Permission denied` | Android emulator installation/acceleration is unhealthy despite host support | Check `emulator -accel-check`, reinstall/update the Android emulator package through `dotnet android`, then retry |
| Multiple Android emulators are running | Automatic target selection is ambiguous | Rerun with `--device-id <serial>` |
| `iOS ... is not installed` | The exact runtime has no available installed simulator | Record that item, continue, then ask whether to install it or explicitly amend the matrix |
| Temporary simulator creation fails | No compatible iPhone type is available for the exact runtime | Inspect `dotnet apple simulator list --runtime "iOS {version}"`; install a compatible device profile or explicitly choose one with `--device` |
| `System UI isn't responding` (Android) | Emulator unstable | Tests auto-retry with dialog dismissal |

## Android Crash Diagnostics

### Getting Crash Details

```bash
dotnet tool run android -- device logcat
```

### Common Crash Causes

| Log Message | Meaning | Action |
|-------------|---------|--------|
| `Force removing...app died` | App crashed | Get stack trace, investigate |
| `Killing...stop <package>` | Normal force-stop | Expected after test completes |
| `FATAL EXCEPTION` | Unhandled exception | **Bug - investigate** |
| `Native crash` | Native library issue | **Bug - investigate** |

### Minimum Android (API 26) Crashes

The minimum Android target may crash due to:
- Missing APIs that MAUI expects
- Different permission behavior
- Slower startup causing timeouts

If a crash occurs only on API 26, get the full stack trace and investigate it as
a compatibility regression.

## iOS Diagnostics

### Simulator Logs

```bash
dotnet tool run apple -- simulator list --booted --format json
dotnet tool run apple -- simulator logs <udid>
```

Or use Console.app → select simulator device.

### Common iOS Issues

| Symptom | Cause | Fix |
|---------|-------|-----|
| Simulator won't boot | Runtime/device creation problem | The runner deletes its temporary simulator; inspect Xcode/CoreSimulator logs before retrying |
| App won't install | Code signing | Check Appium logs |
| Black screen | App crashed | Check simulator logs |

## Screenshot Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `Image similarity too low` | Rendering mismatch | **INVESTIGATE - potential real bug** |
| `Screenshot is blank/black` | Rendering failed | **INVESTIGATE - potential real bug** |
| `Failed to decode image` | Corrupt screenshot | Check Appium logs for errors |
| `Resizing actual to match expected` | Size mismatch | Normal for different devices - comparison still works |

## Playwright Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `Executable doesn't exist` | Browsers not installed | `pwsh playwright.ps1 install chromium` |
| `Target page, context or browser has been closed` | Server crashed | Check app build output |
| `Timeout waiting for selector` | App didn't render | Check Blazor app console for errors |
| `Blazor server failed to start` | Env vars from parent | Fixed in test code (ClearDotNetEnvironmentVariables) |

## Docker Errors (Linux Console Tests)

| Error | Cause | Fix |
|-------|-------|-----|
| `Docker is not available` | Docker not installed/running | Record the Linux item failure and continue; in the repair pass start/restart an existing Docker Desktop, or ask before installation |
| `undefined symbol: uuid_generate_random` | Using `NativeAssets.Linux` instead of `NoDependencies` | Use `SkiaSharp.NativeAssets.Linux.NoDependencies` |
| `Fontconfig error: Cannot load default config file` | No fontconfig in container | Expected with `NoDependencies` — not an error |
| `Cannot connect to the Docker daemon` | Docker Desktop crashed or is not running | Continue unrelated items; restart Docker Desktop in the repair pass and retry only `linux` |
| Docker image build slow | No layer cache | Normal on first run (~90s), cached after |

## Platform-Specific Notes

### macOS /var symlink issue

If Blazor tests fail with path-related errors, the test infrastructure automatically resolves `/var` → `/private/var` in `PlatformTestBase.cs`.

### iOS Simulator Scale Factors

Scale factor calculated automatically from screenshot size vs window size:
- iPhone Pro/Max: 3x
- iPhone standard: 3x
- iPad: 2x

### Mac Catalyst

Mac Catalyst uses hardcoded 2x scale factor. Screenshot is full monitor size, element coordinates are app-relative.

**"Timed out while enabling automation mode" error:**

After WDA builds, XCTest may require the normal interactive **Allow UI
Automation** authorization. Keep authentication enabled and run from the
logged-in Aqua session.

**Fixes to try (in order):**
1. Approve the interactive authorization dialog.
2. Confirm `launchctl managername` reports `Aqua`.
3. Grant Xcode Helper and the launching Terminal/IDE Accessibility access.
4. Restart the Terminal/IDE and rerun the item in isolation.

Do not run `enable-automationmode-without-authentication` for interactive release
tests. Recovery kills only stale WebDriverAgentRunner processes and does not
reset TCC.

## Retry Logic

Tests include automatic retry for transient failures:
- **Android**: 3 retries, 10s delay, recovery includes dialog dismissal
- **iOS**: 3 retries, 10s delay
- **Mac Catalyst**: 3 retries, 30s delay, recovery kills stale WDA test processes
- **Blazor**: 3 retries for server startup

Retryable errors include:
- Device not found
- Driver crashed
- Connection refused
- Session creation failed
- Element not found (might be blocked by dialog)
