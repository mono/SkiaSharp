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
python .agents/skills/release-status/scripts/pipeline-status.py release/{version}
```

**Prerequisite:** The `SkiaSharp` pipeline (ID 10789) must have completed successfully — this is
the pipeline that signs and publishes packages to the internal feed.

`SkiaSharp-Tests` (ID 15756) should pass but does not block testing/publishing.

See the [release-status skill](../release-status/SKILL.md) for full pipeline chain documentation,
manual queries, and troubleshooting.

### Extracting Package Versions

The build description contains the exact internal package version in format:
`#{base}-{label}.{build}+{branch}`.

**Preview example:** `#3.119.2-preview.2.3+3.119.2-preview.2 succeeded`
- Internal test package: `3.119.2-preview.2.3`
- Public version if published: `3.119.2-preview.2.3`

**Stable example:** `#3.119.2-stable.3+3.119.2 succeeded`
- Internal test package: `3.119.2-stable.3`
- Final public version after publication: `3.119.2`

⚠️ **Integration tests run before public publication and MUST consume the exact internal package.**
For a stable release, pass `3.119.2-stable.3` to the tests. The bare `3.119.2` version does not
identify the prepublication build and is reserved for the final NuGet.org publication and tag.

---

## Step 2: Resolve Package Versions

**DO NOT ask the user for exact NuGet versions or select package versions independently.** Derive
both packages from the managed build number selected by release-status:

1. Fetch release branch and read version files:
   ```bash
   # Read base versions (format: "PackageName  nuget  version")
   SKIA_BASE=$(awk '$1 == "SkiaSharp" && $2 == "nuget" {print $3; exit}' scripts/VERSIONS.txt)
   HARFBUZZ_BASE=$(awk '$1 == "HarfBuzzSharp" && $2 == "nuget" {print $3; exit}' scripts/VERSIONS.txt)
   
   # Read preview label (remove surrounding quotes)
   PREVIEW_LABEL=$(grep "PREVIEW_LABEL:" scripts/azure-templates-variables.yml | awk '{print $2}' | tr -d "'")
   ```
   - `SkiaSharp ... nuget` line → base version (e.g., `3.119.2`)
   - `HarfBuzzSharp ... nuget` line → base version (e.g., `8.3.1.3`)
   - `PREVIEW_LABEL` → label (e.g., `preview.2` or `stable`)

2. Capture the exact managed build number reported for the selected `SkiaSharp` run (for example,
   `3.119.2-stable.3+3.119.2`). Derive and validate the package versions:

   - The exact SkiaSharp test package is the managed build-number text before `+`.
   - It must start with `{skia-base}-` and match `{skia-base}-{PREVIEW_LABEL}.{build}`. Stop if the
     base version or stable/preview/RC label does not match the release branch.
   - Remove `{skia-base}-` to obtain the complete suffix (for example, `stable.3`,
     `preview.3.1`, or `rc.1.2`).
   - The exact HarfBuzzSharp test package is `{harfbuzz-base}-{same-suffix}`. This shared suffix
     proves that both packages came from the same managed build.

   ```bash
   MANAGED_BUILD_NUMBER="{managed-build-number-from-release-status}"
   SKIA_TEST_VERSION="${MANAGED_BUILD_NUMBER%%+*}"
   EXPECTED_PREFIX="${SKIA_BASE}-${PREVIEW_LABEL}."

   case "$PREVIEW_LABEL" in
     stable) ;;
     preview.*|rc.*)
       LABEL_NUMBER="${PREVIEW_LABEL#*.}"
       case "$LABEL_NUMBER" in
         ''|*[!0-9]*) echo "PREVIEW_LABEL has an invalid release number." >&2; exit 1 ;;
       esac
       ;;
     *) echo "PREVIEW_LABEL is not stable, preview.N, or rc.N." >&2; exit 1 ;;
   esac
   case "$MANAGED_BUILD_NUMBER" in
     *+*) ;;
     *) echo "Managed build number has no + branch suffix." >&2; exit 1 ;;
   esac
   case "$SKIA_TEST_VERSION" in
     "$EXPECTED_PREFIX"*) ;;
     *) echo "Managed build does not match the SkiaSharp base and release label." >&2; exit 1 ;;
   esac

   BUILD_NUMBER="${SKIA_TEST_VERSION#"$EXPECTED_PREFIX"}"
   case "$BUILD_NUMBER" in
     ''|*[!0-9]*) echo "Managed build has an invalid numeric build suffix." >&2; exit 1 ;;
   esac

   PACKAGE_SUFFIX="${SKIA_TEST_VERSION#"$SKIA_BASE-"}"
   HARFBUZZ_TEST_VERSION="${HARFBUZZ_BASE}-${PACKAGE_SUFFIX}"
   ```

   | Managed build number | SkiaSharp test package | HarfBuzzSharp test package | Public versions |
   |----------------------|------------------------|----------------------------|-----------------|
   | `3.119.2-stable.3+3.119.2` | `3.119.2-stable.3` | `8.3.1.3-stable.3` | `3.119.2` / `8.3.1.3` |
   | `3.119.2-preview.3.1+3.119.2-preview.3` | `3.119.2-preview.3.1` | `8.3.1.3-preview.3.1` | Same as test packages |
   | `3.119.2-rc.1.2+3.119.2-rc.1` | `3.119.2-rc.1.2` | `8.3.1.3-rc.1.2` | Same as test packages |

3. Verify both derived versions exist exactly on the internal test (EAP) feed. Use `.version`,
   never `.latestVersion`, and exact matching — do not choose the "highest" result:

   ```bash
   dotnet package search SkiaSharp \
     --source "https://aka.ms/skiasharp-eap/index.json" \
     --exact-match --prerelease --format json \
     | jq -r '.searchResult[].packages[] | select(.id == "SkiaSharp") | .version' \
     | grep -Fx -- "{skia-test-version}"

   dotnet package search HarfBuzzSharp \
     --source "https://aka.ms/skiasharp-eap/index.json" \
     --exact-match --prerelease --format json \
     | jq -r '.searchResult[].packages[] | select(.id == "HarfBuzzSharp") | .version' \
     | grep -Fx -- "{harfbuzz-test-version}"
   ```

   For preview and RC releases, each public version is its exact test-package version. For stable
   releases, the public versions are the two base versions from `scripts/VERSIONS.txt`.

4. Report to the user:

   **Preview:**
   ```
   Resolved versions:
     SkiaSharp test package:     3.119.2-preview.3.1
     HarfBuzzSharp test package: 8.3.1.3-preview.3.1
     Public versions if published: 3.119.2-preview.3.1 / 8.3.1.3-preview.3.1
     CI build number:             1
   ```

   **Stable:**
   ```
   Resolved versions:
     SkiaSharp test package:      3.119.2-stable.3
     HarfBuzzSharp test package:  8.3.1.3-stable.3
     Final public versions:       3.119.2 / 8.3.1.3
     CI build number:             3
   ```

**Either exact package missing?** Stop: the selected managed build is not fully available on the
internal test (EAP) feed. See
[troubleshooting.md](references/troubleshooting.md#package-resolution-errors).

---

## Step 3: Confirm Test Matrix

**Before running tests**, determine and confirm the test matrix with the user.

### Inspect the Environment First

Follow [setup.md](references/setup.md) and run its read-only preflight before proposing changes.
If a workload, Appium component, driver, SDK image, emulator, browser, or system prerequisite is
missing or incompatible:

1. Report the installed and required versions.
2. Show the exact proposed install, update, or replacement.
3. Ask the user for explicit approval.

Do not turn a release-test request into permission to mutate the machine.

### Device Requirements

| Platform | Old Version | New Version |
|----------|-------------|-------------|
| Android | API 26 (Android 8/Oreo) | API 35-36 (Android 15-16) |
| iOS | Oldest available runtime | Newest available runtime |
| Mac Catalyst | Current macOS host | - |
| Windows | Current Windows host | - |

Current UiAutomator2 releases support API 26 or newer. Do not downgrade Appium or install a
legacy driver to retain API 21-25 coverage; API 26 is the supported old-device boundary.

👉 **See [setup.md](references/setup.md)** for device selection details and emulator creation.

### Confirm with User

```
Planned test matrix:
  - iOS (old):     [device] ([oldest available iOS runtime])
  - iOS (new):     [device] ([newest available iOS runtime])
  - Android (old): [device] (Android 8.0 / API 26)
  - Android (new): [device] (Android 16 / API 36)
  - Mac Catalyst:  Current macOS
  - Windows:       Current Windows host
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
dotnet test -p:SkiaSharpVersion={skia-test-version} -p:HarfBuzzSharpVersion={harfbuzz-test-version}
```

### Test Commands

> **Note:** This project uses **Microsoft.Testing.Platform (MTP)** with xUnit v3 (since #4143).
> The legacy VSTest `--filter "FullyQualifiedName~..."` syntax is **silently ignored** under MTP
> and runs ALL tests. Use the MTP filter args after the `--` separator instead:
> `--filter-class`, `--filter-method`, `--filter-namespace` (and `--filter-not-class`, etc.),
> with `*` wildcards. MSBuild `-p:` properties (e.g. `-p:SkiaSharpVersion=`, `-p:iOSDevice=`)
> must stay BEFORE the `--`; only the test-platform filter args go AFTER it.

```bash
# Run by category
# In every command, "..." means:
# -p:SkiaSharpVersion={skia-test-version} -p:HarfBuzzSharpVersion={harfbuzz-test-version}
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
| `SkiaSharpVersion` | SkiaSharp package version under test (**required**) |
| `HarfBuzzSharpVersion` | HarfBuzzSharp package version under test (**required**) |
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
     -p:HarfBuzzSharpVersion={harfbuzz-test-version} \
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

1. **Inspect the Playwright browser, then install only with approval if missing:**
   ```bash
   cd tests/SkiaSharp.Tests.Integration
   dotnet build -p:SkiaSharpVersion={skia-test-version} -p:HarfBuzzSharpVersion={harfbuzz-test-version}
   pwsh bin/Debug/*/playwright.ps1 install chromium
   ```

   The install writes to the user's browser cache. Do not run it until inspection shows Chromium
   is missing and the user approves the exact command.

2. **Run on the default .NET band:**
   ```bash
   dotnet test \
     -p:SkiaSharpVersion={skia-test-version} \
     -p:HarfBuzzSharpVersion={harfbuzz-test-version} \
     -- --filter-class "*BlazorTests"
   ```

3. **Run on another band** (e.g. a preview) — change `BaseFramework` + `SdkVersion` (see [Test Properties](#test-properties)):
   ```bash
   dotnet test \
     -p:SkiaSharpVersion={skia-test-version} \
     -p:HarfBuzzSharpVersion={harfbuzz-test-version} \
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
- ✅ Android tests pass on BOTH oldest supported (API 26) and newest (API 35-36)
- ✅ Windows tests pass on Windows hardware, or are explicitly reported as a hardware skip on non-Windows hosts
- ✅ Screenshots exist in `output/logs/testlogs/integration/`

### Skip Policy

**Hardware skips only:**
- iOS/Mac tests on non-macOS → Skip (hardware unavailable)
- Windows tests on non-Windows → Skip (hardware unavailable)

**NOT valid skips:**
- "No Android emulator" → Report the required AVD and ask before creating it
- "Android SDK not found" → Ask user for path
- "No iOS simulators" → Report the missing runtime and ask before installing via Xcode
- "Tool X not installed" → Report the exact proposed machine change and ask before installing it

**If the environment is incomplete, do not infer a skip and do not mutate the machine
automatically. Report the gap, ask for approval for the exact setup change, then retry.**

### Final Report Format

```
✅ Release Testing Complete

Packages tested:
  SkiaSharp:      3.119.2-stable.3
  HarfBuzzSharp:  8.3.1.3-stable.3
Final public versions if approved: 3.119.2 / 8.3.1.3

| Test | Platform | Version | Status |
|------|----------|---------|--------|
| SmokeTests | .NET | - | ✅ Passed |
| ConsoleTests | .NET | - | ✅ Passed |
| LinuxConsoleTests | Docker Linux | - | ✅ Passed |
| BlazorTests | Chromium | - | ✅ Passed |
| MauiMacCatalystTests | macOS | - | ✅ Passed |
| MauiWindowsTests | Windows | Current host | ✅ Passed |
| MauiiOSTests | iOS 16.2 (oldest) | iPhone 14 Pro | ✅ Passed |
| MauiiOSTests | iOS 18.5 (newest) | iPhone 16 Pro | ✅ Passed |
| MauiAndroidTests | Android 8.0 (API 26) | Pixel_API_26 | ✅ Passed |
| MauiAndroidTests | Android 16 (API 36) | Pixel_API_36 | ✅ Passed |

Ready for publishing.
```

On a non-Windows host, report `MauiWindowsTests` as a hardware skip with the host reason rather
than omitting the row. Apply the same rule to iOS/Mac tests on non-macOS hosts.

---

## References

- **Setup & device selection:** [references/setup.md](references/setup.md)
- **Monitoring long-running tests:** [references/monitoring.md](references/monitoring.md)
- **Troubleshooting errors:** [references/troubleshooting.md](references/troubleshooting.md)
