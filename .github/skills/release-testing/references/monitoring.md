# Monitoring Long-Running Tests

Proactive diagnostics during test execution.

## Quick Reference

| Check | When |
|-------|------|
| What's building/running | Every 10-15s during silent periods |
| Android logcat for crashes | If no output for 30+ seconds |
| Emulator/simulator status | If tests hang at start |

## Normal Test Flow

1. "Starting test execution" → Tests discovered
2. "[Appium] Welcome to Appium" → Appium server started
3. MAUI build running (30-60s for Android) → App being compiled
4. "Created the new session" → Device session established
5. App visible on device → Test executing
6. Files in `output/logs/testlogs/integration/` → Screenshots captured

## Detecting Problems

| Symptom | Likely Cause | Action |
|---------|--------------|--------|
| Silent after "Appium ready" | MAUI app building | Check for `dotnet build` process, wait |
| No build process, no progress | Session creation failed | Check Appium output for errors |
| App appears then disappears | App crashed | Check device logs for crash |
| "app died, no saved state" | Crash on startup | Get crash stack trace |

## Checking Progress

**What's running:**
```bash
ps aux | grep -E "dotnet.*build|appium" | grep -v grep
```

**Android device status:**
```bash
adb devices -l
adb logcat -d | grep -E "(FATAL|crash|died)" | tail -10
```

**iOS simulator status:**
```bash
xcrun simctl list devices booted
```

## When to Intervene

| Condition | Action |
|-----------|--------|
| Build running, no other issues | Wait — Android builds are slow |
| No progress for 60s, no build | Check Appium logs, restart if needed |
| App crashes repeatedly | Stop, investigate crash (see [troubleshooting.md](troubleshooting.md)) |
| Device unresponsive | Kill device/emulator, restart, retry |
