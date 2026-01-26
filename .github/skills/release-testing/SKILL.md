---
name: release-testing
description: >
  Run integration tests to verify SkiaSharp NuGet packages work correctly before publishing.
  
  Use when user asks to:
  - Test/verify packages before release
  - Run integration tests
  - Test on specific device (iPad, iPhone, Android emulator, Mac, Windows)
  - Verify SkiaSharp rendering works
  - Check if packages are ready for publishing
  - Run smoke/console/blazor/maui tests
  - Continue with release
  - Test version X
  
  Triggers: "test the release", "verify packages", "run tests on iPad", "check ios tests",
  "test mac catalyst", "run android tests", "continue", "test 3.119.2-preview.2".
---

# Release Testing Skill

Verify SkiaSharp packages work correctly before publishing.

⚠️ **NO UNDO:** This is step 2 of 3. See [releasing.md](../../../documentation/releasing.md) for full workflow.

## CRITICAL: ANY FAIL IS TOTAL FAIL

- Test fails → Release FAILS
- Test times out → Release FAILS  
- Screenshot doesn't match → Release FAILS

**Never rationalize failures.** Fix the issue before proceeding.

---

## Step 1: Check CI Status

Before testing, verify CI builds have completed. Check commit statuses on the release branch head:

```bash
gh api "repos/mono/SkiaSharp/commits/{sha}/statuses" --jq '.[] | "\(.context) | \(.state) | \(.description // "no desc") | \(.created_at)"'
```

### Required Pipelines

| Pipeline | Required | Notes |
|----------|----------|-------|
| `SkiaSharp-Native` | ✅ Must pass | Builds native binaries |
| `SkiaSharp` | ✅ Must pass | Builds managed code & publishes packages |
| `SkiaSharp-Tests` | ⚠️ May fail | Sometimes flaky on release branches - warn user but don't block |

**Ignore:** `SkiaSharp (Public)` — public CI, not used for releases.

### Understanding Multiple Statuses

The API returns ALL statuses chronologically. A pipeline may have multiple entries due to retries/rebuilds. Always use the **most recent** status (newest timestamp) for each pipeline.

### Extracting NuGet Version

The build description contains the version in format: `#{version}-{label}.{build}+{branch}`

Example: `#3.119.2-preview.2.3+3.119.2-preview.2 succeeded`
- Version: `3.119.2-preview.2.3`
- Label: `preview.2`
- Build: `3`

---

## Step 2: Resolve Package Versions

**DO NOT ask user for exact NuGet versions.** Resolve automatically:

1. Fetch release branch and read `scripts/VERSIONS.txt`:
   - `SkiaSharp nuget` line → base version (e.g., `3.119.2`)
   - `HarfBuzzSharp nuget` line → base version (e.g., `8.3.1.4`)

2. Read `PREVIEW_LABEL` from `scripts/azure-templates-variables.yml` (e.g., `preview.2` or `stable`)

3. Search preview feed:
   ```bash
   dotnet package search SkiaSharp --source "https://aka.ms/skiasharp-eap/index.json" --exact-match --prerelease --format json
   ```

4. Filter versions matching `{base}-{preview-label}.{build}`, pick latest

5. Report to user:
   ```
   Resolved versions:
     SkiaSharp:     3.119.2-preview.2.3
     HarfBuzzSharp: 8.3.1.4-preview.2.3
     Build number:  3
   ```

**No packages found?** CI build hasn't completed. Check CI status, wait 2-4 hours.

---

## Step 3: Confirm Test Matrix

**Before running tests**, determine and confirm the test matrix with the user.

### Device Requirements

| Platform | Old Version | New Version |
|----------|-------------|-------------|
| Android | API 21-23 (5.0-6.0) | API 35-36 (15-16) |
| iOS | Oldest available runtime | Newest available runtime |

👉 **See [setup.md](references/setup.md)** for device selection details and emulator creation.

### Confirm with User

```
Planned test matrix:
  - iOS (old):     iPhone 14 Pro (iOS 16.2 - oldest available)
  - iOS (new):     iPhone 16 Pro (iOS 18.5 - newest available)
  - Android (old): Pixel_API_23 (Android 6.0 / API 23)
  - Android (new): Pixel_API_36 (Android 16 / API 36)
  - Mac Catalyst:  Current macOS
  - Blazor:        Chromium
  - Console:       .NET runtime

Proceed with this matrix?
```

---

## Step 4: Run Integration Tests

```bash
cd tests/SkiaSharp.Tests.Integration
dotnet test -p:SkiaSharpVersion={version} -p:HarfBuzzSharpVersion={hb-version}
```

### Test Commands

```bash
# Run by category
dotnet test --filter "FullyQualifiedName~SmokeTests" ...
dotnet test --filter "FullyQualifiedName~ConsoleTests" ...
dotnet test --filter "FullyQualifiedName~BlazorTests" ...
dotnet test --filter "FullyQualifiedName~MauiiOSTests" ... -p:iOSDevice="iPhone 14 Pro" -p:iOSVersion="16.2"
dotnet test --filter "FullyQualifiedName~MauiMacCatalystTests" ...
dotnet test --filter "FullyQualifiedName~MauiAndroidTests" ...
```

### Start Emulators First

```bash
# Android
export PATH="$HOME/Library/Android/sdk/platform-tools:$HOME/Library/Android/sdk/emulator:$PATH"
nohup emulator -avd Pixel_API_23 -no-snapshot -no-audio > /tmp/emu.log 2>&1 &
while [ "$(adb shell getprop sys.boot_completed 2>/dev/null | tr -d '\r')" != "1" ]; do sleep 2; done
```

### Test Execution Order

| Test | Run on Old | Run on New | Time |
|------|------------|------------|------|
| SmokeTests | Once | - | ~2s |
| ConsoleTests | Once | - | ~20s |
| BlazorTests | Once | - | ~2min |
| MauiMacCatalystTests | Once | - | ~2min |
| MauiiOSTests | ✅ Yes | ✅ Yes | ~2min each |
| MauiAndroidTests | ✅ Yes | ✅ Yes | ~2min each |

**iOS and Android run TWICE:** once on oldest, once on newest.

### Monitoring

👉 **See [monitoring.md](references/monitoring.md)** for:
- Progress checking commands
- Detecting hangs and crashes
- When to intervene

---

## Step 5: Verify & Report

### Release Criteria

Proceed to **release-publish** ONLY when:

- ✅ ALL tests pass (no failures, no skips except hardware)
- ✅ iOS tests pass on BOTH oldest and newest runtime
- ✅ Android tests pass on BOTH oldest (API 21-23) and newest (API 35-36)
- ✅ Screenshots exist in `output/logs/testlogs/integration/`

**Valid skips:** iOS/Mac on non-macOS, Windows on non-Windows.

**Invalid skips:** "No Android emulator" is NOT valid—create one.

### Final Report Format

```
✅ Release Testing Complete

| Test | Platform | Version | Status |
|------|----------|---------|--------|
| SmokeTests | .NET | - | ✅ Passed |
| ConsoleTests | .NET | - | ✅ Passed |
| BlazorTests | Chromium | - | ✅ Passed |
| MauiMacCatalystTests | macOS | - | ✅ Passed |
| MauiiOSTests | iOS 16.2 (oldest) | iPhone 14 Pro | ✅ Passed |
| MauiiOSTests | iOS 18.5 (newest) | iPhone 16 Pro | ✅ Passed |
| MauiAndroidTests | Android 6.0 (API 23) | Pixel_API_23 | ✅ Passed |
| MauiAndroidTests | Android 16 (API 36) | Pixel_API_36 | ✅ Passed |

Ready for publishing.
```

---

## References

- **Setup & device selection:** [references/setup.md](references/setup.md)
- **Monitoring long-running tests:** [references/monitoring.md](references/monitoring.md)
- **Troubleshooting errors:** [references/troubleshooting.md](references/troubleshooting.md)
