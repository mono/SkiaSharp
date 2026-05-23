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

⚠️ **NO UNDO:** This is step 2 of 3. See [releasing.md](../../../documentation/dev/releasing.md) for full workflow.

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

Before testing, verify CI builds have completed.

### Pipeline Chain

Release builds flow through a **3-pipeline chain**, each triggered by completion of the previous:

```
SkiaSharp-Native (devdiv/DevDiv, ID 26493)
    ↓ triggers on completion
SkiaSharp (devdiv/DevDiv, ID 10789) — managed build, signing & publishing to internal feed
    ↓ triggers on completion
SkiaSharp-Tests (devdiv/DevDiv, ID 15756) — device & unit tests
```

All three must complete before packages are available on the internal feed.
Packages are published to the feed by the `SkiaSharp` pipeline (10789), not by `SkiaSharp-Tests`.

### Tracking Pipeline Status via GitHub

Check commit statuses on the release branch head:

```bash
gh api "repos/mono/SkiaSharp/commits/{sha}/statuses" --jq '.[] | "\(.context) | \(.state) | \(.description // "no desc") | \(.created_at)"'
```

⚠️ Only `SkiaSharp-Native` and `SkiaSharp (Public)` report back to GitHub. The downstream DevDiv
pipelines (`SkiaSharp`, `SkiaSharp-Tests`) do NOT post commit statuses — use `az pipelines` to track them.

### Tracking Pipeline Status via Azure DevOps CLI

The quickest way to check the full pipeline chain is the reusable script:

```bash
.agents/skills/release-branch/scripts/pipeline-status.sh release/{version}
# Or pass a SHA:
.agents/skills/release-branch/scripts/pipeline-status.sh {commit-sha}
```

This outputs all three pipelines with status, trigger relationships, and ADO links.

For manual queries, use `az pipelines` to query each pipeline individually:

```bash
# Check the SkiaSharp-Native build status (find latest run on the release branch)
az pipelines runs list --pipeline-ids 26493 --branch release/{version} \
  --org https://devdiv.visualstudio.com --project DevDiv \
  --query "[].{id:id, status:status, result:result, buildNumber:buildNumber}" --top 5

# Find the downstream SkiaSharp (managed) build triggered by the native build
az pipelines runs list --pipeline-ids 10789 --branch release/{version} \
  --org https://devdiv.visualstudio.com --project DevDiv \
  --query "[].{id:id, status:status, result:result, buildNumber:buildNumber}" --top 5

# Find the SkiaSharp-Tests build triggered by the managed build
az pipelines runs list --pipeline-ids 15756 --branch release/{version} \
  --org https://devdiv.visualstudio.com --project DevDiv \
  --query "[].{id:id, status:status, result:result, buildNumber:buildNumber}" --top 5
```

### Identifying the Correct Run

Multiple runs may exist on the same branch (retries, new commits). Use the `buildNumber` field
to match runs across the chain — all pipelines in the same chain share a version-based buildNumber:

```
buildNumber format: {base}-{label}.{build}+{branch-version}
Example:            3.119.4-stable.2+3.119.4
```

To find the correct run for a specific version:
1. Start with SkiaSharp-Native — find the successful run whose `buildNumber` matches your version
2. Use that run's `id` to confirm downstream pipelines via `triggerInfo.pipelineId`

```bash
# Filter runs by buildNumber to find the exact match
az pipelines runs list --pipeline-ids 10789 --branch release/{version} \
  --org https://devdiv.visualstudio.com --project DevDiv \
  --query "[?contains(buildNumber, '{version}')].{id:id, status:status, result:result, buildNumber:buildNumber}" --top 3
```

### Verifying Trigger Relationships

Each triggered pipeline has a `triggerInfo` field that proves which upstream build caused it:

```bash
az pipelines runs show --id {downstream-build-id} \
  --org https://devdiv.visualstudio.com --project DevDiv \
  --query "triggerInfo"
```

Example output:
```json
{
  "alias": "SkiaSharp",
  "artifactType": "Pipeline",
  "pipelineId": "14174467",
  "pipelineTriggerType": "PipelineCompletion",
  "source": "SkiaSharp-Native",
  "version": "3.119.4-stable.2+3.119.4"
}
```

Use `triggerInfo.pipelineId` to confirm which upstream build triggered a given run. This is
essential when multiple runs exist on the same branch (e.g., retries or concurrent pushes to main).

### Required Pipelines

| Pipeline Name | Definition ID | Required | Notes |
|---------------|---------------|----------|-------|
| `SkiaSharp-Native` | 26493 | ✅ Must pass | Builds native binaries, reports to GitHub |
| `SkiaSharp` | 10789 | ✅ Must pass | Builds managed code, signs & publishes to internal feed, triggered by Native |
| `SkiaSharp-Tests` | 15756 | ⚠️ Should pass | Device & unit tests, triggered by SkiaSharp — warn user if fails but don't block |

**Ignore:** `SkiaSharp (Public)` — public CI, not used for releases.

### Understanding Multiple Statuses

The GitHub API returns ALL statuses chronologically. A pipeline may have multiple entries due to retries/rebuilds. Always use the **most recent** status (newest timestamp) for each pipeline.

### Extracting NuGet Version

The build description contains the internal version in format: `#{base}-{label}.{build}+{branch}`

**Preview example:** `#3.119.2-preview.2.3+3.119.2-preview.2 succeeded`
- Internal version: `3.119.2-preview.2.3`
- NuGet version: `3.119.2-preview.2.3` (same — build number is part of the prerelease tag)

**Stable example:** `#3.119.2-stable.3+3.119.2 succeeded`
- Internal version: `3.119.2-stable.3`
- NuGet version: `3.119.2` (base only — build number is NEVER appended to stable versions)

⚠️ **Stable versions never include a build number.** Each CI build of a stable release produces a different internal package (`3.119.2-stable.1`, `3.119.2-stable.2`, etc.) but the published NuGet version is always just `3.119.2`.

---

## Step 2: Resolve Package Versions

**DO NOT ask user for exact NuGet versions.** Resolve automatically:

1. Fetch release branch and read version files:
   ```bash
   # Read base versions (format: "PackageName  nuget  version")
   grep "^SkiaSharp\s" scripts/VERSIONS.txt | grep "nuget" | awk '{print $3}'
   grep "^HarfBuzzSharp\s" scripts/VERSIONS.txt | grep "nuget" | awk '{print $3}'
   
   # Read preview label (remove surrounding quotes)
   grep "PREVIEW_LABEL:" scripts/azure-templates-variables.yml | awk '{print $2}' | tr -d "'"
   ```
   - `SkiaSharp ... nuget` line → base version (e.g., `3.119.2`)
   - `HarfBuzzSharp ... nuget` line → base version (e.g., `8.3.1.3`)
   - `PREVIEW_LABEL` → label (e.g., `preview.2` or `stable`)

2. **Search and filter for the SPECIFIC version:**

   **For preview releases** (`PREVIEW_LABEL` is NOT `stable`):

   ```bash
   # Get ALL versions, then filter to match {base}-{label}.*
   dotnet package search SkiaSharp \
     --source "https://aka.ms/skiasharp-eap/index.json" \
     --exact-match --prerelease --format json \
     | jq -r '.searchResult[].packages[] | select(.id == "SkiaSharp") | .version' \
     | grep "^{base}-{label}\."
   
   # Example: Find 3.119.2-preview.3.* versions
   ... | grep "^3.119.2-preview.3\."
   ```

   Pick the highest build number (e.g., `3.119.2-preview.3.1`). This IS the NuGet version.

   **For stable releases** (`PREVIEW_LABEL` is `stable`):

   ```bash
   # Verify a stable build exists on the internal feed
   dotnet package search SkiaSharp \
     --source "https://aka.ms/skiasharp-eap/index.json" \
     --exact-match --prerelease --format json \
     | jq -r '.searchResult[].packages[] | select(.id == "SkiaSharp") | .version' \
     | grep "^{base}-stable\."
   
   # Example: Find 3.119.2-stable.* internal packages
   ... | grep "^3.119.2-stable\."
   ```

   The internal feed has `{base}-stable.{build}` packages (e.g., `3.119.2-stable.3`), but the **NuGet version is just `{base}`** (e.g., `3.119.2`). The build number is never appended to stable versions.

   ⚠️ **CRITICAL:** Use `.version` to get ALL versions, NOT `.latestVersion` which only returns the newest.
   The feed contains multiple version streams (e.g., 3.119.2 AND 3.119.3), so you MUST filter
   by the base version and preview label from the release branch.

3. Pick the NuGet version:
   - **Preview:** Highest build number from matching versions (e.g., `3.119.2-preview.3.1`)
   - **Stable:** Just the base version (e.g., `3.119.2`) — no build number appended

4. Report to user:

   **Preview:**
   ```
   Resolved versions:
     SkiaSharp:     3.119.2-preview.3.1
     HarfBuzzSharp: 8.3.1.3-preview.3.1
     Build number:  1
   ```

   **Stable:**
   ```
   Resolved versions:
     SkiaSharp:     3.119.2
     HarfBuzzSharp: 8.3.1.3
     Internal build: 3.119.2-stable.3 (on feed)
   ```

**No packages found?** CI build hasn't completed. See [troubleshooting.md](references/troubleshooting.md#package-resolution-errors).

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
  - iOS (old):     [device] ([oldest available iOS runtime])
  - iOS (new):     [device] ([newest available iOS runtime])
  - Android (old): [device] (Android 6.0 / API 23)
  - Android (new): [device] (Android 16 / API 36)
  - Mac Catalyst:  Current macOS
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
dotnet test -p:SkiaSharpVersion={version} -p:HarfBuzzSharpVersion={hb-version}
```

### Test Commands

```bash
# Run by category
dotnet test --filter "FullyQualifiedName~SmokeTests" ...
dotnet test --filter "FullyQualifiedName~ConsoleTests" ...
dotnet test --filter "FullyQualifiedName~LinuxConsoleTests" ...
dotnet test --filter "FullyQualifiedName~BlazorTests" ...
dotnet test --filter "FullyQualifiedName~MauiiOSTests" ... -p:iOSDevice="iPhone 14 Pro" -p:iOSVersion="16.2"
dotnet test --filter "FullyQualifiedName~MauiMacCatalystTests" ...

# Android: specify device ID and expected API level for validation
dotnet test --filter "FullyQualifiedName~MauiAndroidTests" ... \
  -p:AndroidDeviceId="emulator-5554" \
  -p:AndroidApiLevel="23"
```

### Android Emulator Workflow

⚠️ **CRITICAL:** Run only ONE Android emulator at a time to avoid device confusion.

1. **Verify no emulators running:**
   ```bash
   adb devices -l  # Should show empty or only physical devices
   ```

2. **Start emulator with WIPE and boot verification:**
   ```bash
   # Start emulator with -wipe-data to ensure clean state (use mode="async" to keep it running)
   emulator -avd Pixel_API_23 -wipe-data -no-snapshot -no-audio
   
   # Wait for boot (check every 10s until returns "1")
   # This can take 60-120s for a fresh wipe
   adb shell getprop sys.boot_completed
   
   # Verify correct API level
   adb shell getprop ro.build.version.sdk  # Should match expected (e.g., "23")
   ```

   ⚠️ **The `-wipe-data` flag is REQUIRED** to ensure a clean emulator state. Without it,
   cached apps or system state from previous runs may interfere with tests.

3. **Run tests with device validation:**
   ```bash
   DEVICE_ID=$(adb devices | grep emulator | awk '{print $1}')
   API_LEVEL=$(adb -s $DEVICE_ID shell getprop ro.build.version.sdk | tr -d '\r')
   
   dotnet test --filter "FullyQualifiedName~MauiAndroidTests" \
     -p:AndroidDeviceId="$DEVICE_ID" \
     -p:AndroidApiLevel="$API_LEVEL" \
     -p:SkiaSharpVersion={version} \
     -p:HarfBuzzSharpVersion={hb-version}
   ```

4. **Shut down emulator before next test:**
   ```bash
   adb -s $DEVICE_ID emu kill
   # Wait for it to stop
   sleep 5
   adb devices -l  # Verify empty
   ```

5. **Repeat for next API level** (start from step 1)

### Test Execution Order

| Test | Run on Old | Run on New | Time |
|------|------------|------------|------|
| SmokeTests | Once | - | ~2s |
| ConsoleTests | Once | - | ~20s |
| LinuxConsoleTests | Once (Docker) | - | ~2min |
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
| LinuxConsoleTests | Docker Linux | - | ✅ Passed |
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
