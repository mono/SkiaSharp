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

### Android

```bash
# List available emulators
emulator -list-avds

# Start an emulator before running tests
emulator -avd <avd_name>

# Or verify a physical device is connected
adb devices
```

### iOS

```bash
# List available simulators
xcrun simctl list devices available | grep -i "iphone\|ipad"

# List iOS runtime versions
xcrun simctl list runtimes | grep -i ios

# Boot a simulator (optional - Appium will do this automatically)
xcrun simctl boot "iPhone 16 Pro"
```

## Package Sources

The test project uses these NuGet feeds (configured in `nuget.config`):
- **SkiaSharp Preview**: `https://aka.ms/skiasharp-eap/index.json`
- **dotnet-public**: `https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json`

These are already configured—no manual setup needed.

## Verification

Run smoke tests to verify setup:

```bash
cd tests/SkiaSharp.Tests.Integration
dotnet test --filter "FullyQualifiedName~SmokeTests" \
  -p:SkiaSharpVersion=X.Y.Z \
  -p:HarfBuzzSharpVersion=X.Y.Z.N
```

If smoke tests pass, the environment is correctly configured.
