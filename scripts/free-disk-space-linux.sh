#!/usr/bin/env bash
# Free disk space on Linux CI agents by removing tools and SDK components
# that are not needed by SkiaSharp CI builds.
#
# The hosted agents ship with many pre-installed toolchains (Swift, Haskell,
# Julia, Miniconda, AWS CLI, etc.) that SkiaSharp never uses. Removing them
# before the build starts prevents the agent from running out of disk space.
#
# Android SDK — components we KEEP:
#   cmdline-tools    - sdkmanager, used by install-android-package.ps1
#   platform-tools   - adb, used to communicate with the emulator
#   build-tools/NN.* - latest version only (used by .NET Android builds)
#   platforms/android-{21,35,36} - ANDROID_PLATFORM_VERSIONS from variables
#
# Android SDK — components we REMOVE:
#   ndk, cmake, system-images, sources, extras, emulator, old build-tools,
#   and platforms not in ANDROID_PLATFORM_VERSIONS.

set -euo pipefail

ANDROID_SDK="${ANDROID_HOME:-/usr/local/lib/android/sdk}"

# Platforms to keep (from ANDROID_PLATFORM_VERSIONS pipeline variable, comma-separated)
KEEP_PLATFORMS="${ANDROID_PLATFORM_VERSIONS:-21,35,36}"

free_before=$(df --output=avail / | tail -1)

# -----------------------------------------------------------------------
# Remove pre-installed toolchains that SkiaSharp never uses
# -----------------------------------------------------------------------
echo "Removing unused toolchains..."
sudo rm -rf /usr/share/dotnet              # ~4.6 GB - pre-installed .NET (we install our own via UseDotNet@2)
sudo rm -rf /usr/share/swift               # ~3.3 GB - Swift toolchain
sudo rm -rf /usr/local/.ghcup              # ~3.7 GB - Haskell toolchain
sudo rm -rf /usr/local/julia*              # ~1.0 GB - Julia
sudo rm -rf /usr/share/miniconda           # ~858 MB - Conda
sudo rm -rf /usr/local/share/powershell    # ~178 MB - PowerShell modules (binary is at /opt/microsoft/powershell)
sudo rm -rf /usr/local/share/chromium      #          - Chromium browser
sudo rm -rf /usr/local/aws-cli             # ~255 MB - AWS CLI
sudo rm -rf /usr/local/aws-sam-cli         # ~260 MB - AWS SAM CLI
sudo rm -rf /opt/hostedtoolcache/go        # ~1.1 GB - Go
sudo rm -rf /opt/hostedtoolcache/CodeQL    # ~1.7 GB - CodeQL
sudo rm -rf /opt/hostedtoolcache/Python    # ~1.7 GB - Python (native builds use Docker)
sudo rm -rf /opt/hostedtoolcache/PyPy      # ~524 MB - PyPy
sudo rm -rf /opt/hostedtoolcache/Ruby      # ~312 MB - Ruby

# -----------------------------------------------------------------------
# Clean up Android SDK
# -----------------------------------------------------------------------
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
        IFS=',' read -ra keep_array <<< "$KEEP_PLATFORMS"
        for plat in "$ANDROID_SDK/platforms"/*/; do
            plat_name=$(basename "$plat")
            api_level="${plat_name#android-}"
            keep=false
            for k in "${keep_array[@]}"; do
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
