# Setup Guide

Prerequisites and environment setup for integration tests.

## Prerequisites

### All Platforms

```bash
dotnet workload install maui wasm-tools
npm install -g appium
appium driver install mac2 uiautomator2 xcuitest
```

### Playwright (Blazor tests)

```bash
cd tests/SkiaSharp.Tests.Integration
dotnet build -p:SkiaSharpVersion=X.Y.Z -p:HarfBuzzSharpVersion=X.Y.Z.N
pwsh bin/Debug/net9.0/playwright.ps1 install chromium
```

### macOS Additional

- Install Carthage: `brew install carthage`
- Select Xcode: `sudo xcode-select -s /Applications/Xcode.app`

### Windows Additional

1. Install [WinAppDriver](https://github.com/microsoft/WinAppDriver/releases)
2. Enable Developer Mode in Settings

---

## Android Setup

### Locating Android SDK

Check these common locations in order:

1. `$ANDROID_HOME` or `$ANDROID_SDK_ROOT` (if set)
2. `$HOME/Library/Android/sdk` (macOS default)
3. `$HOME/Android/Sdk` (Linux default)
4. `C:\Users\<user>\AppData\Local\Android\Sdk` (Windows default)

**If not found:** Ask user for path, then verify `platform-tools/adb` and `emulator/emulator` exist.

### Required Tools

Once SDK is located, verify:
- `adb version` works
- `emulator -list-avds` returns output

### Required Emulators

| Type | API Level | Purpose |
|------|-----------|---------|
| Old | 21-23 | Minimum supported Android |
| New | 35-36 | Latest Android |

**To check existing AVDs:** List with `emulator -list-avds`, then check each AVD's `config.ini` for `image.sysdir` containing `android-XX` where XX is the API level.

**To create missing emulators:**

1. Install system image: `sdkmanager "system-images;android-{API};google_apis;arm64-v8a"`
2. Create AVD: `avdmanager create avd -n {name} -k "system-images;android-{API};google_apis;arm64-v8a" -d pixel`

For API 36+, use `google_apis_playstore` instead of `google_apis`.

### Starting Emulators

1. Start: `emulator -avd {name} -no-snapshot -no-audio &`
2. Wait for boot: poll `adb shell getprop sys.boot_completed` until it returns `1`
3. Verify version: `adb shell getprop ro.build.version.sdk`

---

## iOS Setup

### List Available Runtimes

```bash
xcrun simctl list runtimes | grep -i ios
```

Tests require **oldest and newest** available runtimes.

### Device Selection

- **Prefer iPhone** over iPad (more common)
- **Any model works** — tests validate rendering, not device features
- **Be consistent** — use same device type for old and new runtimes

To find devices for a runtime: `xcrun simctl list devices available | grep -A10 "iOS {version}"`

---

## Pre-Flight Verification

Before running release tests, verify:

1. **Android SDK found** — `adb version` works
2. **Old Android emulator exists** — AVD with API 21-23
3. **New Android emulator exists** — AVD with API 35-36
4. **iOS runtimes available** — at least 2 different versions
5. **Appium installed** — `which appium` returns path
6. **Appium drivers installed** — `appium driver list --installed` shows uiautomator2, xcuitest

**If any check fails:** Fix before proceeding. Do not skip tests.

---

## Package Sources

Test project uses these feeds (pre-configured in `nuget.config`):
- SkiaSharp Preview: `https://aka.ms/skiasharp-eap/index.json`
- dotnet-public: `https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json`
