# Monitoring Long-Running Tests

Proactive diagnostics and user feedback during test execution.

## Golden Rule

> **Users should never wait more than 30 seconds without knowing what's happening.**

Update the progress table at each phase. When using `read_bash` during long operations, acknowledge progress even if there's no new output.

---

## Quick Reference

| Action | When |
|--------|------|
| Update progress table | At each phase transition |
| Acknowledge progress | Every 30s during silent periods |
| Check device/build status | If no output for 60+ seconds |
| Record and continue | When one matrix item fails |
| Diagnose and retry | After every approved item has an initial result |

---

## Test Phases and Timing

### Quick Tests (SmokeTests, ConsoleTests, BlazorTests)

| Phase | Duration | Output Indicator |
|-------|----------|------------------|
| Build test project | 5-10s | "Determining projects to restore..." |
| Run tests | 10-60s | "Starting test execution, please wait..." |
| Complete | - | "Passed! - Failed: 0, Passed: N" |

**Feedback:** These are fast enough that a single TODO update per test is sufficient.

### LinuxConsoleTests (Docker)

| Phase | Duration | Output Indicator |
|-------|----------|------------------|
| Build test project | 5-10s | "Determining projects to restore..." |
| Docker image build | 30-90s | "Building Docker image..." |
| Run in container | 5-10s | "Running in Docker container..." |
| Complete | - | "Passed! - Failed: 0, Passed: 2" |

**First run is slower** (~90s) due to Docker image layer caching. Subsequent runs use cached layers (~10s).
Docker tests require `SkiaSharp.NativeAssets.Linux.NoDependencies` which bundles all native deps statically.

### MAUI Platform Tests (iOS, Android, MacCatalyst)

| Phase | Duration | Output Indicator |
|-------|----------|------------------|
| Build test project | 5-10s | "Determining projects to restore..." |
| Test start | - | "Starting test execution, please wait..." |
| Create temp project | 3-5s | "The template \"...\" was created successfully" |
| Add NuGet packages | 2-5s | "PackageReference for package 'SkiaSharp..." |
| **Build MAUI app** | **30-120s** | **Silence** (this is the long part!) |
| Appium connect | 5s | "Connecting to Appium at port 4723..." |
| Driver session | 5-10s | "Driver connected, waiting for app..." |
| App deploy | 10-30s | Device/emulator shows app launching |
| Capture screenshot | 5s | "Full screenshot saved" |
| Verify | 1s | "✅ MAUI {Platform} passed" |
| Complete | - | "Passed! - Failed: 0, Passed: 2" |

---

## The Silent Build Phase

The **Build MAUI app** phase is the longest and provides **no output**. This is normal:

| Platform | Build Time |
|----------|------------|
| iOS | 60-90 seconds |
| Android | 90-120 seconds |
| MacCatalyst | 45-60 seconds |

**This silence is expected.** The user needs to know this.

### Verifying Build is Running

```bash
ps aux | grep -E "dotnet.*build" | grep -v grep
```

If you see a `dotnet build` process, the test is progressing normally.

---

## Providing User Feedback

### Progress Table Format

Use a flat table rather than nested checklist items:

```markdown
| Test | Phase | Status |
|------|-------|--------|
| MauiiOSTests (iOS 15.0) | Test project compiled | Done |
| MauiiOSTests (iOS 15.0) | Temp MAUI project created | Done |
| MauiiOSTests (iOS 15.0) | Building iOS app | Running (~60-90s) |
| MauiiOSTests (iOS 15.0) | Deploying to simulator | Pending |
| MauiiOSTests (iOS 15.0) | Running test | Pending |
```

Keep one summary row per matrix item as well. A failed row remains visible while
later items run; do not replace it with a generic stopped state.

### During Long Waits

When using `read_bash` to wait for a long operation:

1. **Before the wait:** Update the progress table to show the current phase
2. **After each read (if still running):** Add progress note with elapsed time
3. **On completion:** Mark step done, move to next

### Phase-Specific Feedback Messages

| Phase | What to Say |
|-------|-------------|
| Build test project | "Building test project..." |
| Create temp project | "Creating MAUI app from template..." |
| Build MAUI app | "Building for {platform} (~60-90s)..." |
| Still building (30s check) | "⏳ Still building (~30s elapsed)" |
| Still building (60s check) | "⏳ Still building (~60s elapsed)" |
| Build complete | "✅ Build complete" |
| Deploy | "Deploying app to {device}..." |
| Run test | "Running Appium test..." |
| Verify | "Verifying screenshot..." |
| Done | "✅ Passed" |

### Example: Full Test Feedback Flow

```
🔄 Running MauiiOSTests (iOS 15.0)
  ✅ Test project compiled
  ✅ Created MauiiOSSKCanvasView project
  ⏳ Building iOS app (~60-90s expected)...
     ⏳ Still building (~30s elapsed)
     ⏳ Still building (~60s elapsed)
  ✅ Build complete (~75s)
  ⏳ Deploying to iPhone 14 Pro simulator...
  ✅ App deployed, running test...
  ✅ Screenshot captured and verified
  ✅ MauiiOSTests (iOS 15.0) passed!
```

---

## Detecting Problems

| Symptom | Likely Cause | Action |
|---------|--------------|--------|
| Silent after "Starting test execution" | MAUI app building | **Normal** - wait, update user |
| No build process, 120+ seconds | Build stuck/failed | Check for errors |
| "Connecting to Appium" then silence | Driver creation failed | Check Appium logs |
| App appears then disappears | App crashed | Check device logs |
| "Could not find canvas element" | UI didn't render | Will auto-retry, wait |

## Checking Device Status

**Android:**
```bash
dotnet tool run android -- device list --format json
dotnet tool run android -- device logcat
```

**iOS:**
```bash
dotnet tool run apple -- simulator list --booted --format json
```

---

## When to Intervene

| Condition | Action |
|-----------|--------|
| Build running, < 2 minutes | Wait — builds are slow, this is normal |
| Build running, > 3 minutes | Something may be wrong, check logs |
| No build process, > 60s silence | Check for errors in test output |
| App crashes repeatedly | Preserve logs, mark the item failed, clean up, and continue; investigate in the repair pass |
| Device unresponsive | Clean up the runner-owned device, mark the item failed, and continue; restart/retry in the repair pass |

## Collection-pass failure handling

When a command fails:

1. Capture the command, exit code, failing phase, last useful output, and
   artifact/log paths.
2. Let `run-tests.py` finish its cleanup. Repair only leaked runner-owned
   resources that would prevent the next item from starting.
3. Mark that item failed in the progress table.
4. Start the next approved item, even when Docker, Appium, one runtime, or one
   device is unavailable.

After the final initial attempt, show the consolidated failures before beginning
the repair pass. Group repeated symptoms under one likely root cause.
