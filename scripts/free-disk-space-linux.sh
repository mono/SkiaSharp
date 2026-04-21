#!/usr/bin/env bash
# Free disk space on Linux CI agents by removing pre-installed Android SDK
# components that are not needed by CI builds.
#
# Components we KEEP:
#   cmdline-tools    - sdkmanager, used by install-android-package.ps1
#   platform-tools   - adb, used to communicate with the emulator
#   build-tools/NN.* - latest version only (used by .NET Android builds)
#   platforms/android-{21,35,36} - ANDROID_PLATFORM_VERSIONS from variables
#
# Components we REMOVE (large, never used or explicitly reinstalled):
#   ndk             - native builds use Docker or install-android-ndk.ps1
#   cmake           - native builds use Docker
#   system-images   - emulator tests reinstall the exact image they need
#   sources         - source code jars, never needed in CI
#   extras          - support libraries, not needed
#   emulator        - emulator tests reinstall this explicitly
#   old build-tools - only latest version needed
#   old platforms   - only 21, 35, 36 needed (ANDROID_PLATFORM_VERSIONS)

set -euo pipefail

ANDROID_SDK="${ANDROID_HOME:-/usr/local/lib/android/sdk}"

# Platforms to keep (must match ANDROID_PLATFORM_VERSIONS in variables.yml)
KEEP_PLATFORMS="21 35 36"

free_before=$(df --output=avail / | tail -1)

if [ -d "$ANDROID_SDK" ]; then
    echo "Removing unused Android SDK components from $ANDROID_SDK..."
    sudo rm -rf "$ANDROID_SDK/ndk"
    sudo rm -rf "$ANDROID_SDK/cmake"
    sudo rm -rf "$ANDROID_SDK/system-images"
    sudo rm -rf "$ANDROID_SDK/sources"
    sudo rm -rf "$ANDROID_SDK/extras"
    sudo rm -rf "$ANDROID_SDK/emulator"

    # Keep only the latest build-tools version
    if [ -d "$ANDROID_SDK/build-tools" ]; then
        latest_bt=$(ls -1 "$ANDROID_SDK/build-tools" | sort -V | tail -1)
        echo "Keeping build-tools/$latest_bt, removing older versions..."
        for bt in "$ANDROID_SDK/build-tools"/*/; do
            bt_name=$(basename "$bt")
            if [ "$bt_name" != "$latest_bt" ]; then
                echo "  Removing build-tools/$bt_name"
                sudo rm -rf "$bt"
            fi
        done
    fi

    # Keep only the platforms we need
    if [ -d "$ANDROID_SDK/platforms" ]; then
        echo "Keeping platforms: $KEEP_PLATFORMS, removing others..."
        for plat in "$ANDROID_SDK/platforms"/*/; do
            plat_name=$(basename "$plat")
            api_level="${plat_name#android-}"
            keep=false
            for k in $KEEP_PLATFORMS; do
                if [ "$api_level" = "$k" ]; then
                    keep=true
                    break
                fi
            done
            if [ "$keep" = false ]; then
                echo "  Removing platforms/$plat_name"
                sudo rm -rf "$plat"
            fi
        done
    fi
else
    echo "Android SDK not found at $ANDROID_SDK, skipping."
fi

free_after=$(df --output=avail / | tail -1)
freed_kb=$((free_after - free_before))
freed_mb=$((freed_kb / 1024))
echo "Freed ${freed_mb} MB of disk space"
df -h /
