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

⚠️ **NO UNDO:** This is **Step 3 of 4** in the release pipeline. See [releasing.md](../../../documentation/dev/releasing.md) for full workflow.

**Pipeline:** [Step 1: release-branch](../release-branch/SKILL.md) → [Step 2: release-status](../release-status/SKILL.md) → **Step 3 (this skill)** → [Step 4: release-publish](../release-publish/SKILL.md)

## CRITICAL: ANY FAIL IS TOTAL FAIL

- Test fails → Release FAILS
- Test times out → Release FAILS  
- Screenshot doesn't match → Release FAILS

**Never rationalize failures.** Fix the issue before proceeding.

---

## ⚠️ CRITICAL: Semver Version Ordering

When identifying which release branch to test, you **MUST** use semver ordering, NOT alphabetical or `sort -V` ordering.

**In semver, a bare version is ALWAYS newer than its prerelease variants:**

```
3.119.2-preview.1 < 3.119.2-preview.2 < 3.119.2-preview.3 < 3.119.2 (FINAL)
```

`release/3.119.2` is the **stable release** and is NEWER than `release/3.119.2-preview.3`.

**To find the latest release branch:**

1. List all release branches: `git branch -r | grep "release/"`
2. Identify the highest base version (e.g., `3.119.2`)
3. Check if a **bare version branch** exists (e.g., `release/3.119.2`) — if so, that is the latest
4. If only preview branches exist, the highest preview number is the latest

**⚠️ Getting this wrong means testing the wrong version — wasting the entire process or shipping untested packages.**

---

## Step 1: Check CI Status

Before testing, verify CI builds have completed using the **release-status** skill:

```bash
python3 .agents/skills/release-status/scripts/pipeline-status.py release/{version}
```

**Prerequisite:** The `SkiaSharp` pipeline (ID 10789) must have completed successfully — this is
the pipeline that signs and publishes packages to the internal feed.

Record the selected successful `SkiaSharp` run's build ID and `buildNumber`. All package versions
used for testing must come from that exact run.

`SkiaSharp-Tests` (ID 15756) should pass but does not block testing/publishing.

See the [release-status skill](../release-status/SKILL.md) for full pipeline chain documentation,
manual queries, and troubleshooting.

### Extracting Package Versions

The selected `SkiaSharp` run has a `buildNumber` in format:
`{base}-{label}.{build}+{branch}`.

**Preview example:** `3.119.2-preview.2.3+3.119.2-preview.2`
- Test package version: `3.119.2-preview.2.3`
- Eventual public version: `3.119.2-preview.2.3` (same — build number is part of the prerelease tag)

**Stable example:** `3.119.2-stable.3+3.119.2`
- Test package version: `3.119.2-stable.3`
- Eventual public version: `3.119.2` (base only)

⚠️ **Stable release testing MUST use the exact `*-stable.{build}` package from the selected CI
build.** The bare version (for example, `3.119.2`) is only the eventual NuGet.org version and may
not exist before publication.

Every CI build — including a re-run of the same release branch — produces a distinct internal
package pair (`3.119.2-stable.1`, `3.119.2-stable.2`, etc.). If the selected run changes, repeat
release testing against the exact packages from the new run.

---

## Step 2: Resolve Package Versions

**DO NOT ask user for exact package versions.** Resolve them automatically from the selected CI
build:

1. Fetch release branch and read version files:
   ```bash
   git fetch origin
   RELEASE_REF="origin/release/{version}"

   # Read base versions (format: "PackageName  nuget  version")
   SKIA_BASE="$(git show "${RELEASE_REF}:scripts/VERSIONS.txt" | awk '$1 == "SkiaSharp" && $2 == "nuget" {print $3; exit}')"
   HB_BASE="$(git show "${RELEASE_REF}:scripts/VERSIONS.txt" | awk '$1 == "HarfBuzzSharp" && $2 == "nuget" {print $3; exit}')"

   # Read preview label (remove surrounding quotes)
   PREVIEW_LABEL="$(git show "${RELEASE_REF}:scripts/azure-templates-variables.yml" | awk '$1 == "PREVIEW_LABEL:" {print $2; exit}' | tr -d "'")"

   if [ -z "$SKIA_BASE" ] || [ -z "$HB_BASE" ] || [ -z "$PREVIEW_LABEL" ]; then
     echo "ERROR: Could not read release versions from $RELEASE_REF." >&2
     exit 1
   fi
   ```
   - `SkiaSharp ... nuget` line → base version (e.g., `3.119.2`)
   - `HarfBuzzSharp ... nuget` line → base version (e.g., `8.3.1.3`)
   - `PREVIEW_LABEL` → label (e.g., `preview.2` or `stable`)

2. Use the `SkiaSharp` pipeline run selected in Step 1. Its `buildNumber` identifies the exact
   package build to test; do not substitute the newest matching package from the feed.

   ```bash
   # Example selected buildNumber: 3.119.2-stable.3+3.119.2
   SELECTED_BUILD_ID="{selected SkiaSharp build ID}"
   BUILD_NUMBER="{selected SkiaSharp buildNumber}"

   SKIA_TEST_VERSION="${BUILD_NUMBER%%+*}"
   EXPECTED_PREFIX="${SKIA_BASE}-${PREVIEW_LABEL}."

   case "$SKIA_TEST_VERSION" in
     "$EXPECTED_PREFIX"*) ;;
     *)
       echo "ERROR: Selected buildNumber '$BUILD_NUMBER' does not match $RELEASE_REF ($SKIA_BASE, $PREVIEW_LABEL)." >&2
       exit 1
       ;;
   esac

   BUILD_SUFFIX="${SKIA_TEST_VERSION#*-}"
   HB_TEST_VERSION="${HB_BASE}-${BUILD_SUFFIX}"
   ```

   The suffix must match `{PREVIEW_LABEL}.{build}` from the selected build:
   - **Preview / RC:** `3.119.2-preview.3.1` and `8.3.1.3-preview.3.1`
   - **Stable:** `3.119.2-stable.3` and `8.3.1.3-stable.3`

3. **Verify both exact test package versions exist on the preview feed:**

   ```bash
   dotnet package search SkiaSharp \
     --source "https://aka.ms/skiasharp-eap/index.json" \
     --exact-match --prerelease --format json \
     | jq -r '.searchResult[].packages[] | select(.id == "SkiaSharp") | .version' \
     | grep -Fx "$SKIA_TEST_VERSION"

   dotnet package search HarfBuzzSharp \
     --source "https://aka.ms/skiasharp-eap/index.json" \
     --exact-match --prerelease --format json \
     | jq -r '.searchResult[].packages[] | select(.id == "HarfBuzzSharp") | .version' \
     | grep -Fx "$HB_TEST_VERSION"
   ```

   ⚠️ **CRITICAL:** Use `.version` to get ALL versions, NOT `.latestVersion` which only returns the newest.
   The feed contains multiple version streams and CI builds, so you MUST match the exact versions
   derived from the selected pipeline run.

   If either exact match returns nothing, list the candidates to see what the feed actually has:

   ```bash
   # Diagnostic only — never select the version to test from this list
   dotnet package search SkiaSharp \
     --source "https://aka.ms/skiasharp-eap/index.json" \
     --exact-match --prerelease --format json \
     | jq -r '.searchResult[].packages[] | select(.id == "SkiaSharp") | .version' \
     | grep -F "${SKIA_BASE}-${PREVIEW_LABEL}."

   dotnet package search HarfBuzzSharp \
     --source "https://aka.ms/skiasharp-eap/index.json" \
     --exact-match --prerelease --format json \
     | jq -r '.searchResult[].packages[] | select(.id == "HarfBuzzSharp") | .version' \
     | grep -F "${HB_BASE}-${PREVIEW_LABEL}."
   ```

4. Record the eventual public versions separately:
   - **Preview / RC:** Same exact prerelease versions as the test packages
   - **Stable:** Base versions from `VERSIONS.txt` (for example, `3.119.2` and `8.3.1.3`)

5. Report to user:

   **Preview / RC:**
   ```
   Resolved package versions:
     Selected CI run:
       Build ID:       {ado-build-id}
       buildNumber:    3.119.2-preview.3.1+3.119.2-preview.3
     Test packages:
       SkiaSharp:     3.119.2-preview.3.1
       HarfBuzzSharp: 8.3.1.3-preview.3.1
     Eventual public versions:
       SkiaSharp:     3.119.2-preview.3.1
       HarfBuzzSharp: 8.3.1.3-preview.3.1
     Package build:    1
     Feed check:       both exact test packages confirmed
   ```

   **Stable:**
   ```
   Resolved package versions:
     Selected CI run:
       Build ID:       {ado-build-id}
       buildNumber:    3.119.2-stable.3+3.119.2
     Test packages:
       SkiaSharp:     3.119.2-stable.3
       HarfBuzzSharp: 8.3.1.3-stable.3
     Eventual public versions:
       SkiaSharp:     3.119.2
       HarfBuzzSharp: 8.3.1.3
     Package build:    3
     Feed check:       both exact test packages confirmed
   ```

**Exact package missing?** Verify the selected run and release branch first; CI may not have
completed, or the version inputs may not match that run. See
[troubleshooting.md](references/troubleshooting.md#package-resolution-errors).

---

## Step 3: Confirm Test Matrix

**Before running tests**, determine and confirm the test matrix with the user.

### Device Requirements

| Platform | Old Version | New Version |
|----------|-------------|-------------|
| Android | API 26 (8.0/Oreo) | API 35-36 (15-16) |
| iOS | Oldest available runtime | Newest available runtime |

> API 26 is the floor for the current release-test automation/UiAutomator2 stack, not the
> minimum Android version supported by SkiaSharp or MAUI.

👉 **See [setup.md](references/setup.md)** for device selection details and emulator creation.

### Confirm with User

```
Planned test matrix:
  - iOS (old):     [device] ([oldest available iOS runtime])
  - iOS (new):     [device] ([newest available iOS runtime])
  - Android (old): [device] (Android 8.0 / API 26)
  - Android (new): [device] (Android 16 / API 36)
  - Mac Catalyst:  Current macOS
  - Windows:       Current Windows
  - Blazor:        Chromium
  - Console:       .NET runtime
  - Linux (Docker): Docker container (mcr.microsoft.com/dotnet/sdk:8.0)

Proceed with this matrix?
```

---

## Step 4: Run Integration Tests

### Pre-Test Cleanup (REQUIRED)

⚠️ **CRITICAL:** These steps MUST be done before ANY integration tests:

```bash
# 1. Clear screenshot folder to ensure fresh results
rm -rf output/logs/testlogs/integration/*
mkdir -p output/logs/testlogs/integration

# 2. Kill any running Android emulators
adb devices | grep emulator | awk '{print $1}' | while read emu; do
  adb -s $emu emu kill 2>/dev/null
done
sleep 5

# 3. Verify clean state
adb devices -l  # Should show NO emulators
ls output/logs/testlogs/integration/  # Should be empty
```

### Run Tests

```bash
cd tests/SkiaSharp.Tests.Integration
dotnet test -p:SkiaSharpVersion={skia-test-version} -p:HarfBuzzSharpVersion={hb-test-version}
```

Always pass the exact test package versions resolved in Step 2. For stable releases, these include
the `-stable.{build}` suffix; never pass the eventual public bare versions before publication.

### Test Commands

> **Note:** This project uses **Microsoft.Testing.Platform (MTP)** with xUnit v3 (since #4143).
> The legacy VSTest `--filter "FullyQualifiedName~..."` syntax is **silently ignored** under MTP
> and runs ALL tests. Use the MTP filter args after the `--` separator instead:
> `--filter-class`, `--filter-method`, `--filter-namespace` (and `--filter-not-class`, etc.),
> with `*` wildcards. MSBuild `-p:` properties (e.g. `-p:SkiaSharpVersion=`, `-p:iOSDevice=`)
> must stay BEFORE the `--`; only the test-platform filter args go AFTER it.

```bash
# Run by category
dotnet test ... -- --filter-class "*SmokeTests"
dotnet test ... -- --filter-class "*ConsoleTests"
dotnet test ... -- --filter-class "*LinuxConsoleTests"
dotnet test ... -- --filter-class "*BlazorTests"
dotnet test -p:iOSDevice="iPhone 14 Pro" -p:iOSVersion="16.2" ... -- --filter-class "*MauiiOSTests"
dotnet test ... -- --filter-class "*MauiMacCatalystTests"
dotnet test ... -- --filter-class "*MauiWindowsTests"

# Android: specify device ID and expected API level for validation
dotnet test ... \
  -p:AndroidDeviceId="emulator-5554" \
  -p:AndroidApiLevel="26" \
  -- --filter-class "*MauiAndroidTests"
```

### Test Properties

MSBuild `-p:` properties the test project accepts (all go **before** the `--`):

| Property | What it's for |
|----------|---------------|
| `SkiaSharpVersion` | Exact SkiaSharp test package version from the selected CI build (**required**) |
| `HarfBuzzSharpVersion` | Exact HarfBuzzSharp test package version from the selected CI build (**required**) |
| `BaseFramework` | TFM the generated temp apps target (`dotnet new … -f`) |
| `SdkVersion` | SDK feature band pinned in the temp apps' `global.json` |
| `SdkAllowPrerelease` | allow a prerelease SDK for that band (`true` for previews) |
| `iOSDevice` / `iOSVersion` | simulator device name + iOS runtime for MAUI iOS tests |
| `AndroidDevice` / `AndroidVersion` | device name + Android version (display metadata) |
| `AndroidDeviceId` | target emulator/device UDID (e.g. `emulator-5554`) |
| `AndroidApiLevel` | expected API level, validated at runtime |

> **Testing a different .NET band (e.g. a preview):** change these two — `BaseFramework` to `netX.0`
> and `SdkVersion` to the matching SDK feature band — and add `-p:SdkAllowPrerelease=true` for a
> preview SDK. With no override they use the project defaults.

### Android Emulator Workflow

⚠️ **CRITICAL:** Run only ONE Android emulator at a time to avoid device confusion.

1. **Verify no emulators running:**
   ```bash
   adb devices -l  # Should show empty or only physical devices
   ```

2. **Start emulator with WIPE and boot verification:**
   ```bash
   # Start emulator with -wipe-data to ensure clean state (use mode="async" to keep it running)
   emulator -avd Pixel_API_26 -wipe-data -no-snapshot -no-audio
   
   # Wait for boot (check every 10s until returns "1")
   # This can take 60-120s for a fresh wipe
   adb shell getprop sys.boot_completed
   
   # Verify correct API level
   adb shell getprop ro.build.version.sdk  # Should match expected (e.g., "26")
   ```

   ⚠️ **The `-wipe-data` flag is REQUIRED** to ensure a clean emulator state. Without it,
   cached apps or system state from previous runs may interfere with tests.

3. **Run tests with device validation:**
   ```bash
   DEVICE_ID=$(adb devices | grep emulator | awk '{print $1}')
   API_LEVEL=$(adb -s $DEVICE_ID shell getprop ro.build.version.sdk | tr -d '\r')
   
   dotnet test \
     -p:AndroidDeviceId="$DEVICE_ID" \
     -p:AndroidApiLevel="$API_LEVEL" \
     -p:SkiaSharpVersion={skia-test-version} \
     -p:HarfBuzzSharpVersion={hb-test-version} \
     -- --filter-class "*MauiAndroidTests"
   ```

4. **Shut down emulator before next test:**
   ```bash
   adb -s $DEVICE_ID emu kill
   # Wait for it to stop
   sleep 5
   adb devices -l  # Verify empty
   ```

5. **Repeat for next API level** (start from step 1)

### WASM (Blazor) Workflow

`BlazorTests` builds a **real** WASM app (`-p:WasmBuildNative=true`, so it exercises the shipped
native `.a` libs), boots it headless in Playwright/Chromium, and screenshot-diffs the canvas. Run it
once per .NET band you're validating.

1. **Install the Playwright browser (one-time):**
   ```bash
   cd tests/SkiaSharp.Tests.Integration
   dotnet build -p:SkiaSharpVersion={skia-test-version} -p:HarfBuzzSharpVersion={hb-test-version}
   pwsh bin/Debug/*/playwright.ps1 install chromium
   ```

2. **Run on the default .NET band:**
   ```bash
   dotnet test \
     -p:SkiaSharpVersion={skia-test-version} \
     -p:HarfBuzzSharpVersion={hb-test-version} \
     -- --filter-class "*BlazorTests"
   ```

3. **Run on another band** (e.g. a preview) — change `BaseFramework` + `SdkVersion` (see [Test Properties](#test-properties)):
   ```bash
   dotnet test \
     -p:SkiaSharpVersion={skia-test-version} \
     -p:HarfBuzzSharpVersion={hb-test-version} \
     -p:BaseFramework=netX.0 \
     -p:SdkVersion=X.0.100 \
     -p:SdkAllowPrerelease=true \
     -- --filter-class "*BlazorTests"
   ```

   ⚠️ **The target band needs its own `wasm-tools` workload**, and Playwright must be new enough to
   boot that runtime (a too-old Chromium can't start preview-.NET WASM apps — they use the `exnref`
   exception-handling feature). The required Playwright version is pinned in the test project.

4. **Repeat step 3 for each additional band.**

### Test Execution Order

| Test | Run on Old | Run on New | Time |
|------|------------|------------|------|
| SmokeTests | Once | - | ~2s |
| ConsoleTests | Once | - | ~20s |
| LinuxConsoleTests | Once (Docker) | - | ~2min |
| BlazorTests | Once | - | ~2min |
| MauiMacCatalystTests | Once | - | ~2min |
| MauiWindowsTests | Once | - | ~2min |
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
- ✅ Android tests pass on BOTH oldest (API 26) and newest (API 35-36)
- ✅ Windows tests pass on Windows hosts
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

Selected CI run:
  Build ID:       {ado-build-id}
  buildNumber:    {buildNumber}

Test packages:
  SkiaSharp:     {skia-test-version}
  HarfBuzzSharp: {hb-test-version}

Eventual public versions:
  SkiaSharp:     {skia-public-version}
  HarfBuzzSharp: {hb-public-version}

Feed check: both exact test packages confirmed

| Test | Platform | Version | Status |
|------|----------|---------|--------|
| SmokeTests | .NET | - | ✅ Passed |
| ConsoleTests | .NET | - | ✅ Passed |
| LinuxConsoleTests | Docker Linux | - | ✅ Passed |
| BlazorTests | Chromium | - | ✅ Passed |
| MauiMacCatalystTests | macOS | - | ✅ Passed |
| MauiWindowsTests | Windows | - | ✅ Passed |
| MauiiOSTests | iOS 16.2 (oldest) | iPhone 14 Pro | ✅ Passed |
| MauiiOSTests | iOS 18.5 (newest) | iPhone 16 Pro | ✅ Passed |
| MauiAndroidTests | Android 8.0 (API 26) | Pixel_API_26 | ✅ Passed |
| MauiAndroidTests | Android 16 (API 36) | Pixel_API_36 | ✅ Passed |

Ready for publishing.
```

---

## References

- **Setup & device selection:** [references/setup.md](references/setup.md)
- **Monitoring long-running tests:** [references/monitoring.md](references/monitoring.md)
- **Troubleshooting errors:** [references/troubleshooting.md](references/troubleshooting.md)
