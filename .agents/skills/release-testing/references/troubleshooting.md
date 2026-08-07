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

### Packages appear missing but CI succeeded

**Symptom:** CI shows success, but package search seems to find wrong version or nothing matching your release.

**Cause:** Using `.latestVersion` from the JSON instead of `.version`, or choosing the newest
matching feed package instead of the exact package from the selected CI build. The feed contains
multiple version streams (for example, 3.119.2 and 3.119.3) and CI builds, so either approach can
return the wrong one.

**Fix:** Rerun `release-status`. It verifies both exact package versions against
the preview feed and reports `wait-for-packages` until both are indexed. Do not
select a replacement version from the feed.

**`ERROR: Could not resolve build metadata for run ...`** — confirm the selected `SkiaSharp`
pipeline run ID and Azure CLI authentication.

**`ERROR: Selected run came from ...`** — the selected run is not from the requested release
branch. Return to release-status and select the correct run.

**`ERROR: Selected source commit ... is not available locally`** — fetch the named release branch
again. Do not checkout the branch; the remote-tracking fetch should make the commit available.

**`ERROR: Selected source commit ... does not belong to ...`** — the run and release branch do not
match. Re-check the selected run rather than reading version files from the branch's current tip.

**`ERROR: Could not read release versions from ...`** — confirm the selected source commit contains
both version files. Keep `HEAD` unchanged and inspect them with `git show {source-sha}:{path}`.

**`ERROR: Selected buildNumber ... does not match ...`** — the selected run is not from that
release branch, or its source commit contains different version values. Re-check the run selected
by release-status and its `sourceVersion`.

| ❌ Wrong | ✅ Correct |
|----------|-----------|
| `.latestVersion` | `.version` |
| Newest matching build | Exact selected CI build |
| Prefix filtering to select a version | Exact version match |

---

## Build Errors

| Error | Cause | Fix |
|-------|-------|-----|
| Local `android` / `apple` tool is unavailable | Pinned manifest has not been restored | Run `python3 .agents/skills/release-testing/scripts/prepare-test-run.py`; it performs `dotnet tool restore` |
| `the maui workload is not installed` | Missing workload | Record affected MAUI items, continue unrelated coverage, then ask whether to install `maui` or explicitly amend the matrix |
| `the wasm-tools workload is not installed` | Missing workload | Record Blazor as failed, continue unrelated coverage, then ask whether to install `wasm-tools` or explicitly amend the matrix |
| `SkiaSharpVersion must be the exact package version from the selected CI build` | Missing version param | Add `-p:SkiaSharpVersion={skia-test-version} -p:HarfBuzzSharpVersion={hb-test-version}` to `dotnet test` |
| `HarfBuzzSharpVersion must be the exact package version from the selected CI build` | Missing version param | Add `-p:SkiaSharpVersion={skia-test-version} -p:HarfBuzzSharpVersion={hb-test-version}` to `dotnet test` |
| Stable `X.Y.Z` package cannot be restored | Eventual public version was passed before publication | Use the exact `X.Y.Z-stable.{build}` and matching HarfBuzzSharp test packages |

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
| Temporary simulator creation fails | Device type is incompatible with the selected runtime | Omit `--device` or choose a compatible iPhone device type |
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

This is a macOS accessibility permissions issue. The WebDriverAgentMac process needs accessibility permissions to automate apps.

**Fixes to try (in order):**
1. Reset accessibility permissions: `tccutil reset Accessibility`
2. System Settings → Privacy & Security → Accessibility → Add Terminal.app (or your IDE)
3. Restart Terminal/IDE after granting permissions
4. If still failing, try running test in isolation (not after other tests)

The test includes retry logic (3 attempts) with recovery actions that reset TCC and kill stale processes. If it still fails after retries, it's likely a deeper macOS configuration issue.

## Retry Logic

Tests include automatic retry for transient failures:
- **Android**: 3 retries, 10s delay, recovery includes dialog dismissal
- **iOS**: 3 retries, 10s delay
- **Mac Catalyst**: 3 retries, 30s delay, recovery includes TCC reset and process cleanup
- **Blazor**: 3 retries for server startup

Retryable errors include:
- Device not found
- Driver crashed
- Connection refused
- Session creation failed
- Element not found (might be blocked by dialog)
