#!/usr/bin/env bash
# Free disk space on Linux CI agents by removing pre-installed Android SDK
# components. The CI pipeline reinstalls only the specific components it needs.

set -euo pipefail

ANDROID_SDK="${ANDROID_HOME:-/usr/local/lib/android/sdk}"

echo "Disk space before cleanup:"
df -h /

if [ -d "$ANDROID_SDK" ]; then
    echo "Removing pre-installed Android SDK components from $ANDROID_SDK..."
    sudo rm -rf "$ANDROID_SDK/ndk"
    sudo rm -rf "$ANDROID_SDK/cmake"
    sudo rm -rf "$ANDROID_SDK/build-tools"
    sudo rm -rf "$ANDROID_SDK/platforms"
    sudo rm -rf "$ANDROID_SDK/system-images"
    sudo rm -rf "$ANDROID_SDK/sources"
    sudo rm -rf "$ANDROID_SDK/extras"
    sudo rm -rf "$ANDROID_SDK/emulator"
else
    echo "Android SDK not found at $ANDROID_SDK, skipping."
fi

echo ""
echo "Disk space after cleanup:"
df -h /
