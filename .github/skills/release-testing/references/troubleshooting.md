# Troubleshooting Guide

Quick reference for common errors and fixes.

## Build Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `MAUI workload is required` | Missing workload | `dotnet workload install maui` |
| `wasm-tools workload is required` | Missing workload | `dotnet workload install wasm-tools` |
| `SkiaSharpVersion must be specified` | Missing version param | Add `-p:SkiaSharpVersion=X.Y.Z -p:HarfBuzzSharpVersion=X.Y.Z.N` |

## Appium Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `Cannot start process 'appium'` | Appium not installed | `npm install -g appium` |
| `Mac2 driver requires Carthage` | Carthage missing | `brew install carthage` |
| `Connection refused` | Port conflict | Appium auto-starts on 4723; check for conflicts |
| `Session creation timeout` | First run building WDA | Wait - WebDriverAgent builds on first iOS/Mac run |
| `Invalid bundle identifier` | Wrong bundleId | Tests extract from csproj automatically |

## Simulator/Emulator Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `No Android devices found` | No emulator running | `emulator -avd <name>` |
| `Simulator not found` | Wrong device name | Check `xcrun simctl list devices available` |
| `iOS version not available` | Missing runtime | Install via Xcode → Platforms |
| `System UI isn't responding` (Android) | Emulator unstable | Tests auto-retry with dialog dismissal |

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
