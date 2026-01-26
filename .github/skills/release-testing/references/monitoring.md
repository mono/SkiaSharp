# Monitoring Long-Running Tests

Proactive diagnostics while tests are running. Don't just wait—actively check progress.

## Quick Reference

| Check | Command | When |
|-------|---------|------|
| What's running | `ps aux \| grep -E "dotnet.*build\|appium" \| grep -v grep` | Every 10-15s |
| Android crashes | `adb logcat -d \| grep -E "(FATAL\|crash\|died)" \| tail -10` | If no output 30s+ |
| App on device | `adb shell "dumpsys activity activities" \| grep -i skiasharp` | After app should launch |
| Emulator status | `adb devices -l` | If tests hang at start |

## Progress Indicators

### Normal Flow

1. **Test starts** → "Starting test execution"
2. **Appium starts** → "[Appium] Welcome to Appium"
3. **MAUI build** → `dotnet build Maui*` in process list (30-60s for Android)
4. **App installs** → Appium logs show "Installing app"
5. **Session created** → "Created the new session with id"
6. **Test runs** → App visible on device/simulator
7. **Screenshot** → Files appear in `output/logs/testlogs/integration/`

### Detecting Problems

| Symptom | Likely Cause | Check |
|---------|--------------|-------|
| No output after "Appium ready" | MAUI app building | `ps aux \| grep "dotnet.*build"` |
| Build running 60s+ | Normal for Android | Wait, Android builds are slow |
| No build, no progress | Appium session failed | Check Appium output for errors |
| App launches then disappears | App crashed | `adb logcat -d \| grep -E "died\|FATAL"` |
| "app died, no saved state" | App crashed on startup | Check crash details below |

## Android Crash Diagnostics

### Quick Crash Check

```bash
export PATH="$HOME/Library/Android/sdk/platform-tools:$PATH"

# Check if app was force-stopped or crashed
adb logcat -d | grep -E "(Force removing|app died|killing)" | tail -5

# Get crash stack trace (if any)
adb logcat -d | grep -E "(AndroidRuntime|FATAL EXCEPTION)" -A15 | head -30
```

### Common Android Crash Causes

| Log Message | Meaning | Action |
|-------------|---------|--------|
| `Force removing...app died` | App crashed | Get stack trace |
| `Killing...stop <package>` | Appium force-stopped app | Normal after test completes |
| `FATAL EXCEPTION` | Unhandled exception | **Bug - investigate** |
| `Native crash` | Native library issue | **Bug - investigate** |

### Old Android (API 21-23) Issues

Old Android versions may have:
- Missing APIs that MAUI expects
- Different behavior for permissions
- Slower startup times

If app crashes only on old Android:
1. Get full stack trace
2. Check if it's a known MAUI/SkiaSharp compatibility issue
3. Report to user before proceeding

## iOS Diagnostics

### Simulator Logs

```bash
# Get simulator UDID
xcrun simctl list devices booted

# Stream logs (Ctrl+C to stop)
xcrun simctl spawn <UDID> log stream --predicate 'process == "YourApp"'

# Or check Console.app → select simulator device
```

### Common iOS Issues

| Symptom | Cause | Fix |
|---------|-------|-----|
| Simulator won't boot | Corrupt state | `xcrun simctl erase <UDID>` |
| App won't install | Code signing | Check Appium logs for details |
| Black screen | App crashed on launch | Check simulator logs |

## Appium Session Diagnostics

### Check Appium Logs

Appium logs show session lifecycle:
- `POST /session` → Creating session
- `Installing app` → Deploying to device
- `Starting app` → Launching
- `DELETE /session` → Test completed

### Session Creation Failures

```bash
# Look for session errors in test output
grep -E "(Could not create|session.*failed|timeout)" test_output.log
```

Common session failures:
- **Device not found** → Emulator not running
- **App not found** → Build failed, check build output
- **Timeout** → Device too slow, increase timeout

## When to Intervene

| Condition | Action |
|-----------|--------|
| No output 30s, build running | Wait - builds are slow |
| No output 60s, no build | Check Appium logs, may need restart |
| App crashes repeatedly | Stop tests, investigate crash |
| Emulator unresponsive | Kill emulator, restart, retry |
| "Timed out" errors | Check [troubleshooting.md](troubleshooting.md) |

## Parallel Monitoring Commands

Run these in a separate terminal while tests execute:

```bash
# Combined watch command (runs every 5 seconds)
watch -n 5 '
echo "=== Processes ===" 
ps aux | grep -E "dotnet.*build|appium" | grep -v grep | head -3
echo ""
echo "=== Android ===" 
adb devices -l 2>/dev/null
echo ""
echo "=== Recent logcat ===" 
adb logcat -d -t 5 2>/dev/null | grep -v "^-" | tail -3
'
```

Or check manually every 10-15 seconds during test runs.
