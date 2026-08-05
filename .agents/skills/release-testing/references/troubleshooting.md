# Troubleshooting Guide

Quick reference for common errors and fixes.

## Package Resolution Errors

### Packages appear missing but CI succeeded

**Symptom:** CI shows success, but package search seems to find wrong version or nothing matching your release.

**Cause:** Using `.latestVersion` from the JSON instead of `.version`. The feed contains multiple version streams (e.g., 3.119.2 AND 3.119.3), so `.latestVersion` returns the wrong one.

**Fix:** Use `.version` and filter by base version + label from the release branch. Pass the
selected exact internal version to the tests, including `-stable.{build}` for stable releases:

```bash
dotnet package search SkiaSharp \
  --source "https://aka.ms/skiasharp-eap/index.json" \
  --exact-match --prerelease --format json \
  | jq -r '.searchResult[].packages[] | select(.id == "SkiaSharp") | .version' \
  | grep "^3.119.2-preview.3\."
```

| ❌ Wrong | ✅ Correct |
|----------|-----------|
| `.latestVersion` | `.version` |
| No filtering | Filter by `{base}-{label}.*` |

---

## Build Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `MAUI workload is required` | Missing workload | Inspect `dotnet workload list`, then ask before proposing `dotnet workload install maui` |
| `wasm-tools workload is required` | Missing workload | Inspect `dotnet workload list`, then ask before proposing `dotnet workload install wasm-tools` |
| `SkiaSharpVersion must be specified` | Missing version param | Pass the resolved exact internal versions, e.g. `-p:SkiaSharpVersion=X.Y.Z-stable.B -p:HarfBuzzSharpVersion=X.Y.Z.N-stable.B` |

## Appium Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `Cannot start process 'appium'` | Appium missing or not on PATH | Inspect Node/npm/PATH, report the proposed compatible Appium install, and ask before `npm install -g appium` |
| Driver not found | Required platform driver missing | Inspect `appium driver list --installed` and ask before installing or replacing a driver |
| Appium/driver rejected by npm | Incompatible Appium, driver, Node, or npm versions | Compare against current official metadata; do not downgrade silently |
| `Mac2 driver requires Carthage` | Carthage missing | Inspect `carthage version`, then ask before proposing `brew install carthage` |
| `Connection refused` | Port conflict | Appium auto-starts on 4723; check for conflicts |
| `Session creation timeout` | First run building WDA | Wait - WebDriverAgent builds on first iOS/Mac run |
| `Invalid bundle identifier` | Wrong bundleId | Tests extract from csproj automatically |

Installation commands in this table are proposed remediations only. Obtain explicit approval
before running package-manager, workload, driver, SDK, or system setup commands.

### Windows driver or WinAppDriver missing

1. Inspect `appium --version` and `appium driver list --installed`.
2. Preserve a working Appium 3 + `windows` driver setup.
3. If WinAppDriver is missing, explain that Microsoft has not maintained it since 2022 and ask
   before running `appium driver run windows install-wad [optional-version]`.
4. Do not switch to NovaWindows or replace an existing driver during a release without a separate
   user-approved compatibility decision.

## Simulator/Emulator Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `No Android devices found` | No emulator running | Start emulator first |
| `Simulator not found` | Wrong device name | Check `xcrun simctl list devices available` |
| `iOS version not available` | Missing runtime | Report the missing runtime and ask before installing it via Xcode → Platforms |
| `System UI isn't responding` (Android) | Emulator unstable | Tests auto-retry with dialog dismissal |

## Android Crash Diagnostics

### Getting Crash Details

```bash
# Check if app crashed
adb logcat -d | grep -E "(Force removing|app died)" | tail -5

# Get stack trace
adb logcat -d | grep -E "(AndroidRuntime|FATAL EXCEPTION)" -A15 | head -30
```

### Common Crash Causes

| Log Message | Meaning | Action |
|-------------|---------|--------|
| `Force removing...app died` | App crashed | Get stack trace, investigate |
| `Killing...stop <package>` | Normal force-stop | Expected after test completes |
| `FATAL EXCEPTION` | Unhandled exception | **Bug - investigate** |
| `Native crash` | Native library issue | **Bug - investigate** |

### Minimum Android (API 26) Crashes

API 26 is the oldest Android version supported by current UiAutomator2. It may expose:
- Missing APIs that MAUI expects
- Different permission behavior
- Slower startup causing timeouts

If the crash occurs only on API 26, get the full stack trace and check for a MAUI/SkiaSharp issue.
Do not use API 21-25 with a legacy Appium/driver stack as a release-testing workaround.

## iOS Diagnostics

### Simulator Logs

```bash
xcrun simctl list devices booted  # get UDID
xcrun simctl spawn <UDID> log stream --predicate 'process == "YourApp"'
```

Or use Console.app → select simulator device.

### Common iOS Issues

| Symptom | Cause | Fix |
|---------|-------|-----|
| Simulator won't boot | Corrupt state | `xcrun simctl erase <UDID>` |
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
| `Executable doesn't exist` | Browsers not installed | Report the missing browser and ask before running `pwsh playwright.ps1 install chromium` |
| `Target page, context or browser has been closed` | Server crashed | Check app build output |
| `Timeout waiting for selector` | App didn't render | Check Blazor app console for errors |
| `Blazor server failed to start` | Env vars from parent | Fixed in test code (ClearDotNetEnvironmentVariables) |

## Docker Errors (Linux Console Tests)

| Error | Cause | Fix |
|-------|-------|-----|
| `Docker is not available` | Docker not installed/running | Inspect the installation; ask before installing Docker Desktop, or start it if already installed |
| `undefined symbol: uuid_generate_random` | Using `NativeAssets.Linux` instead of `NoDependencies` | Use `SkiaSharp.NativeAssets.Linux.NoDependencies` |
| `Fontconfig error: Cannot load default config file` | No fontconfig in container | Expected with `NoDependencies` — not an error |
| `Cannot connect to the Docker daemon` | Docker Desktop not running | Start Docker Desktop |
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
