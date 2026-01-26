# Setup Guide

Complete setup instructions for running integration tests.

## Prerequisites

### All Platforms

```bash
# Required .NET workloads (requires .NET 10 SDK)
dotnet workload install maui
dotnet workload install wasm-tools

# Appium server and drivers (required for MAUI tests)
npm install -g appium
appium driver install mac2          # Mac Catalyst
appium driver install uiautomator2  # Android
appium driver install xcuitest      # iOS

# Playwright browsers (required for Blazor tests)
cd tests/SkiaSharp.Tests.Integration
dotnet build -p:SkiaSharpVersion=X.Y.Z -p:HarfBuzzSharpVersion=X.Y.Z.N
pwsh bin/Debug/net9.0/playwright.ps1 install chromium
```

### macOS Additional

```bash
# Carthage (required for Mac2 driver)
brew install carthage

# Ensure Xcode is selected
sudo xcode-select -s /Applications/Xcode.app
```

### Windows Additional

1. Download WinAppDriver from https://github.com/microsoft/WinAppDriver/releases
2. Install and run as Administrator
3. Enable Developer Mode: Settings → Privacy & Security → For developers → Developer Mode

---

## Android Setup

### Environment

```bash
# Set PATH (add to shell profile for persistence)
export PATH="$HOME/Library/Android/sdk/platform-tools:$HOME/Library/Android/sdk/emulator:$PATH"

# Verify tools work
adb version
emulator -list-avds
```

### Required Emulators

Tests must run on **both old and new** Android versions:

| Type | API Level | Android Version | Purpose |
|------|-----------|-----------------|---------|
| Old | 21-23 | 5.0-6.0 | Minimum supported version |
| New | 35-36 | 15-16 | Latest stable/preview |

#### Check Existing AVDs

```bash
# List AVDs with their API levels
for avd in $(emulator -list-avds); do
  api=$(grep "image.sysdir" ~/.android/avd/${avd}.avd/config.ini 2>/dev/null | grep -oE "android-[0-9]+" | grep -oE "[0-9]+")
  echo "$avd: API $api"
done
```

#### Create Missing Emulators

**Old emulator (API 21) — if none exist with API 21-23:**
```bash
sdkmanager "system-images;android-21;google_apis;arm64-v8a"
avdmanager create avd -n Pixel_API_21 -k "system-images;android-21;google_apis;arm64-v8a" -d pixel
```

**New emulator (API 36) — if none exist with API 35-36:**
```bash
sdkmanager "system-images;android-36;google_apis_playstore;arm64-v8a"
avdmanager create avd -n Pixel_API_36 -k "system-images;android-36;google_apis_playstore;arm64-v8a" -d pixel_9
```

### Starting Emulators

```bash
# Start emulator (use -no-snapshot for clean state)
nohup emulator -avd Pixel_API_21 -no-snapshot -no-audio > /tmp/emu.log 2>&1 &

# Wait for boot completion
while [ "$(adb shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')" != "1" ]; do sleep 2; done

# Verify Android version
adb shell getprop ro.build.version.release
adb shell getprop ro.build.version.sdk
```

---

## iOS Setup

Tests must run on **both oldest and newest** available iOS runtimes.

### List Available Runtimes

```bash
xcrun simctl list runtimes | grep -i ios

# Example output:
# iOS 16.2 (16.2 - 20C52) - oldest
# iOS 18.5 (18.5 - 22F77)
# iOS 26.2 (26.2 - 23C54) - newest
```

### Find Devices for Each Runtime

```bash
# List all devices grouped by runtime
xcrun simctl list devices available

# Find devices for specific runtime
xcrun simctl list devices available | grep -A15 "iOS 16.2"  # oldest
xcrun simctl list devices available | grep -A15 "iOS 26"    # newest
```

### Device Selection Guidelines

- **Prefer iPhone** over iPad (more common user device)
- **Any model works** — tests validate SkiaSharp rendering, not device-specific features
- **Pick consistently** — use same device type for old and new (e.g., both iPhone Pro models)

Example selection:
- Old: iPhone 14 Pro (iOS 16.2)
- New: iPhone 16 Pro (iOS 26.2)

### Booting Simulators (Optional)

Appium boots simulators automatically, but pre-booting speeds up tests:

```bash
xcrun simctl boot "iPhone 14 Pro"
xcrun simctl list devices booted  # verify
```

---

## Pre-Flight Verification

**Run these checks before starting release tests** to catch setup issues early.

### Checklist

```bash
# 1. Verify Android tools
export PATH="$HOME/Library/Android/sdk/platform-tools:$HOME/Library/Android/sdk/emulator:$PATH"
adb version && echo "✓ adb works"
emulator -list-avds | head -1 && echo "✓ emulator works"

# 2. Check for old Android emulator (API 21-23)
emulator -list-avds | while read avd; do
  api=$(grep "image.sysdir" ~/.android/avd/${avd}.avd/config.ini 2>/dev/null | grep -oE "android-[0-9]+" | grep -oE "[0-9]+")
  [ "$api" -ge 21 ] && [ "$api" -le 23 ] && echo "✓ Old Android: $avd (API $api)"
done

# 3. Check for new Android emulator (API 35-36)
emulator -list-avds | while read avd; do
  api=$(grep "image.sysdir" ~/.android/avd/${avd}.avd/config.ini 2>/dev/null | grep -oE "android-[0-9]+" | grep -oE "[0-9]+")
  [ "$api" -ge 35 ] && echo "✓ New Android: $avd (API $api)"
done

# 4. Check iOS runtimes
xcrun simctl list runtimes | grep -i ios | head -1  # oldest
xcrun simctl list runtimes | grep -i ios | tail -1  # newest

# 5. Verify Appium
which appium && echo "✓ appium installed"
appium driver list --installed 2>/dev/null | grep -E "(uiautomator2|xcuitest)" && echo "✓ drivers installed"
```

If any check fails, fix before proceeding with tests.

---

## Package Sources

The test project uses these NuGet feeds (configured in `nuget.config`):
- **SkiaSharp Preview**: `https://aka.ms/skiasharp-eap/index.json`
- **dotnet-public**: `https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json`

These are already configured—no manual setup needed.

## Quick Verification

Run smoke tests to verify setup is complete:

```bash
cd tests/SkiaSharp.Tests.Integration
dotnet test --filter "FullyQualifiedName~SmokeTests" \
  -p:SkiaSharpVersion=X.Y.Z \
  -p:HarfBuzzSharpVersion=X.Y.Z.N
```

If smoke tests pass, the environment is correctly configured.
