# Monitoring Long-Running Tests

Proactive diagnostics and user feedback during test execution.

## Golden Rule

> **Users should never wait more than 30 seconds without knowing what's happening.**

Update the TODO checklist at each phase. When using `read_bash` during long operations, acknowledge progress even if there's no new output.

---

## Quick Reference

| Action | When |
|--------|------|
| Update TODO checklist | At each phase transition |
| Acknowledge progress | Every 30s during silent periods |
| Check device/build status | If no output for 60+ seconds |

---

## Test Phases and Timing

### Quick Tests (SmokeTests, ConsoleTests, BlazorTests)

| Phase | Duration | Output Indicator |
|-------|----------|------------------|
| Build test project | 5-10s | "Determining projects to restore..." |
| Run tests | 10-60s | "Starting test execution, please wait..." |
| Complete | - | "Passed! - Failed: 0, Passed: N" |

**Feedback:** These are fast enough that a single TODO update per test is sufficient.

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

### TODO Checklist Format

Use a detailed checklist showing sub-steps for MAUI tests:

```markdown
- [ ] MauiiOSTests (iOS 16.2 - oldest)
  - [x] Test project compiled
  - [x] Created temp MAUI project
  - [ ] Building iOS app (~60-90s)...
  - [ ] Deploying to simulator
  - [ ] Running test
```

### During Long Waits

When using `read_bash` to wait for a long operation:

1. **Before the wait:** Update checklist to show current phase
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
🔄 Running MauiiOSTests (iOS 16.2)
  ✅ Test project compiled
  ✅ Created MauiiOSSKCanvasView project
  ⏳ Building iOS app (~60-90s expected)...
     ⏳ Still building (~30s elapsed)
     ⏳ Still building (~60s elapsed)
  ✅ Build complete (~75s)
  ⏳ Deploying to iPhone 14 Pro simulator...
  ✅ App deployed, running test...
  ✅ Screenshot captured and verified
  ✅ MauiiOSTests (iOS 16.2) passed!
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
adb devices -l
adb logcat -d | grep -E "(FATAL|crash|died)" | tail -10
```

**iOS:**
```bash
xcrun simctl list devices booted
```

---

## When to Intervene

| Condition | Action |
|-----------|--------|
| Build running, < 2 minutes | Wait — builds are slow, this is normal |
| Build running, > 3 minutes | Something may be wrong, check logs |
| No build process, > 60s silence | Check for errors in test output |
| App crashes repeatedly | Stop, investigate (see [troubleshooting.md](troubleshooting.md)) |
| Device unresponsive | Kill device/emulator, restart, retry |
