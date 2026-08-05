# Setup Guide

Prerequisites and environment setup for integration tests.

## Safety Rule: Inspect Before Changing the Machine

Start with read-only inspection. Do **not** install or update .NET workloads, global npm packages,
Appium drivers, SDK images, emulators, browsers, Carthage, WinAppDriver, or other machine-level
dependencies without showing the user the exact proposed change and receiving explicit approval.

```bash
dotnet workload list
node --version
npm --version
appium --version
appium driver list --installed --updates
```

If a command is missing or a component is incompatible, report:

1. What is installed now.
2. What the tests require.
3. The exact install/update/replacement command proposed.
4. Whether the action replaces an existing component.

Then ask the user before running the command. A release test is not permission to upgrade a
developer machine.

## Supported Appium Strategy

As verified from official upstream metadata on 2026-08-05:

| Component | Current stable | Compatibility |
|-----------|----------------|---------------|
| Appium core | [3.6.0](https://github.com/appium/appium/releases/tag/appium%403.6.0) | Node `^20.19.0 \|\| ^22.12.0 \|\| >=24.0.0`, npm `>=10` |
| UiAutomator2 driver | [8.2.2](https://github.com/appium/appium-uiautomator2-driver/releases/tag/v8.2.2) | Appium 3; Android API 26+ |
| Windows driver | [6.1.0](https://github.com/appium/appium-windows-driver/releases/tag/v6.1.0) | Appium 3; official README documents Windows 10 host support |

Sources: [Appium requirements](https://appium.io/docs/en/latest/quickstart/requirements/),
[UiAutomator2 requirements](https://github.com/appium/appium-uiautomator2-driver#requirements),
and [Windows driver usage](https://github.com/appium/appium-windows-driver#usage).

- Keep an existing mutually compatible Appium 3 setup when it works; release testing is not an
  automatic upgrade event.
- Before any installation or replacement, re-check the official releases and compatibility
  metadata rather than assuming the versions above are still latest.
- Current UiAutomator2 requires API 26. Prefer API 26 as the old-device boundary instead of
  downgrading to Appium 2 or a legacy driver for API 21-25.
- Install only the drivers needed on the current host: `uiautomator2` for Android, `xcuitest` and
  `mac2` on macOS, and `windows` on Windows.
- Use `appium driver doctor uiautomator2` after inspection for Android diagnostics. Doctor output
  does not grant permission to install missing components.

### Windows Driver Strategy

The supported SkiaSharp path remains Appium's official `windows` driver because PR
[#3969](https://github.com/mono/SkiaSharp/pull/3969) validated that path. Preserve a working
installation rather than replacing it during a release.

- The Node.js driver wrapper is maintained, but Microsoft has not maintained the WinAppDriver
  backend since 2022. Appium documents this limitation in its
  [driver catalog](https://appium.io/docs/en/latest/ecosystem/drivers/#windows).
- Windows driver 3.0.0+ no longer installs WinAppDriver automatically. After user approval, the
  supported setup command is `appium driver run windows install-wad [optional-version]`.
- The upstream README explicitly documents Windows 10 hosts. PR #3969 also validated SkiaSharp's
  tests on Windows 11; record the actual host version in the release report.
- Appium lists NovaWindows as a community drop-in replacement. Treat switching to it as a
  separate, user-approved compatibility decision, not an automatic release setup step.

## Prerequisites

### Docker (Linux tests)

Docker Desktop must be installed and running. Verify with:

```bash
docker --version
docker info --format '{{.OSType}}'  # Should output "linux"
```

The `LinuxConsoleTests` use `SkiaSharp.NativeAssets.Linux.NoDependencies` (statically linked) to avoid system dependency issues in minimal containers.

### Playwright (Blazor tests)

```bash
cd tests/SkiaSharp.Tests.Integration
dotnet build -p:SkiaSharpVersion={skia-test-version} -p:HarfBuzzSharpVersion={harfbuzz-test-version}
pwsh bin/Debug/net8.0/playwright.ps1 install chromium
```

Inspect whether Chromium is already present first. Ask before running the Playwright installation,
which writes to the user's browser cache.

### macOS Additional

- Inspect Carthage with `carthage version`.
- Inspect the selected Xcode with `xcode-select -p` and `xcodebuild -version`.
- If Carthage or a different Xcode selection is required, show the exact command and ask before
  running `brew install carthage` or `sudo xcode-select ...`.

### Windows Additional

1. Verify the `windows` driver appears in `appium driver list --installed`.
2. Verify WinAppDriver is already installed and can start.
3. Verify Developer Mode is enabled in Settings.
4. If WinAppDriver is missing, ask before running `appium driver run windows install-wad`.
5. Do not replace a working WinAppDriver or switch to NovaWindows without explicit approval.

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
| Old | 26 | Minimum supported by current UiAutomator2 |
| New | 35-36 | Latest Android |

**To check existing AVDs:** List with `emulator -list-avds`, then check each AVD's `config.ini` for `image.sysdir` containing `android-XX` where XX is the API level.

**To create missing emulators (only after user approval):**

1. Install system image: `sdkmanager "system-images;android-{API};google_apis;arm64-v8a"`
2. Create AVD: `avdmanager create avd -n {name} -k "system-images;android-{API};google_apis;arm64-v8a" -d pixel`

For API 36+, use `google_apis_playstore` instead of `google_apis`.

### Starting Emulators

⚠️ **CRITICAL:** Always use `-wipe-data` to ensure a clean emulator state.

1. Start with wipe: `emulator -avd {name} -wipe-data -no-snapshot -no-audio`
2. Wait for boot: poll `adb shell getprop sys.boot_completed` until it returns `1`
   - Fresh wipe takes **60-120 seconds** to boot (vs 15-30s without wipe)
3. Verify API level: `adb shell getprop ro.build.version.sdk`

**Note:** If `emulator` or `adb` are not in PATH, locate them via the SDK path documented above (e.g., `$ANDROID_HOME/emulator/emulator`).

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
2. **Old Android emulator exists** — AVD with API 26
3. **New Android emulator exists** — AVD with API 35-36
4. **iOS runtimes available** — at least 2 different versions
5. **Appium compatible** — version and Node/npm satisfy the current Appium requirements
6. **Required Appium drivers installed** — only the drivers needed for the selected host matrix
7. **Windows ready when applicable** — Windows driver, WinAppDriver, and Developer Mode verified
8. **Docker available** — `docker info` succeeds (for Linux console tests)

**If any check fails:** Report it and ask before changing the machine. Do not silently install,
replace, downgrade, or skip.

---

## Package Sources

Test project uses these feeds (pre-configured in `nuget.config`):
- SkiaSharp internal test packages: `https://pkgs.dev.azure.com/dnceng/public/_packaging/skiasharp/nuget/v3/index.json`
  (`https://aka.ms/skiasharp-eap/index.json` is the search alias used by the skill)
- dotnet-public: `https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json`
