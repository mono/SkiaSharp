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
| `SkiaSharp` | ⚠️ May not exist publically | Builds managed code & publishes packages |
| `SkiaSharp-Tests` | ⚠️ May fail or not exist publically | Sometimes flaky on release branches - warn user but don't block |

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

Start Android emulator before running tests. See [setup.md](references/setup.md) for SDK location and emulator commands.

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

### Providing User Feedback

**CRITICAL:** Long-running tests need continuous feedback. Users should never wait more than 30 seconds without knowing what's happening.

- Update the TODO checklist at each phase transition
- When waiting with `read_bash`, note elapsed time: "⏳ Still building (~60s elapsed)"
- Tell users what's normal: "MAUI Release builds take 30-120s, silence is expected"

👉 **See [monitoring.md](references/monitoring.md)** for:
- Phase timing and expected durations
- Output indicators to detect which phase is active
- Feedback templates and example output
- Troubleshooting hangs and crashes

---

## Step 5: Verify & Report

### Release Criteria

Proceed to **release-publish** ONLY when:

- ✅ ALL tests pass (no failures)
- ✅ iOS tests pass on BOTH oldest and newest runtime
- ✅ Android tests pass on BOTH oldest (API 21-23) and newest (API 35-36)
- ✅ Screenshots exist in `output/logs/testlogs/integration/`

### Skip Policy

**Hardware skips only:**
- iOS/Mac tests on non-macOS → Skip (hardware unavailable)
- Windows tests on non-Windows → Skip (hardware unavailable)

**NOT valid skips:**
- "No Android emulator" → Create one
- "Android SDK not found" → Ask user for path
- "No iOS simulators" → Install via Xcode
- "Tool X not installed" → Install it

**If environment is broken, FIX IT. Do not skip tests.**

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
