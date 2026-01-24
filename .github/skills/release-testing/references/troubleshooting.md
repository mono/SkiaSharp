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
