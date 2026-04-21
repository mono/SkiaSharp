#!/usr/bin/env bash
# Free disk space on Linux CI agents by removing pre-installed Android SDK
# components that are not needed by CI builds.
#
# Components we KEEP (relied on by managed builds and the emulator tests):
#   cmdline-tools   - sdkmanager, used by install-android-package.ps1
#   platform-tools  - adb, used to communicate with the emulator
#   build-tools     - aapt/d8/etc, used to build .NET Android projects
#   platforms       - android.jar, used to build .NET Android projects
#
# Components we REMOVE (large, never used or explicitly reinstalled):
#   ndk             - native builds use Docker or install-android-ndk.ps1
#   cmake           - native builds use Docker
#   system-images   - emulator tests reinstall the exact image they need
#   sources         - source code jars, never needed in CI
#   extras          - support libraries, not needed
#   emulator        - emulator tests reinstall this explicitly

set -euo pipefail

ANDROID_SDK="${ANDROID_HOME:-/usr/local/lib/android/sdk}"

free_before=$(df --output=avail / | tail -1)

if [ -d "$ANDROID_SDK" ]; then
    echo "Removing unused Android SDK components from $ANDROID_SDK..."
    sudo rm -rf "$ANDROID_SDK/ndk"
    sudo rm -rf "$ANDROID_SDK/cmake"
    sudo rm -rf "$ANDROID_SDK/system-images"
    sudo rm -rf "$ANDROID_SDK/sources"
    sudo rm -rf "$ANDROID_SDK/extras"
    sudo rm -rf "$ANDROID_SDK/emulator"
else
    echo "Android SDK not found at $ANDROID_SDK, skipping."
fi

free_after=$(df --output=avail / | tail -1)
freed_kb=$((free_after - free_before))
freed_mb=$((freed_kb / 1024))
echo "Freed ${freed_mb} MB of disk space"
df -h /
