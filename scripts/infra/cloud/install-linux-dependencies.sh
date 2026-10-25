#!/usr/bin/env bash

set -euo pipefail

if [[ "$(uname -s)" != "Linux" ]]; then
    echo "Cloud agent provisioning requires Linux." >&2
    exit 1
fi

if ! command -v apt-get >/dev/null 2>&1; then
    echo "Cloud agent provisioning requires apt-get." >&2
    exit 1
fi

packages=(
    # native/linux/build.cake uses Clang-only flags and links libc++.
    clang
    libc++-dev
    libc++abi-dev
    fontconfig
    libfontconfig1-dev
    ninja-build
    fonts-dejavu-core
    ttf-ancient-fonts
    fonts-noto-color-emoji
    xvfb
    mesa-utils
    libgl1-mesa-dri
    libglx-mesa0
    mesa-vulkan-drivers
    vulkan-tools
)

if [[ "$EUID" -eq 0 ]]; then
    apt-get update -qq
    DEBIAN_FRONTEND=noninteractive apt-get install -y "${packages[@]}"
else
    if ! command -v sudo >/dev/null 2>&1; then
        echo "Cloud agent provisioning requires root access or sudo." >&2
        exit 1
    fi

    sudo apt-get update -qq
    sudo env DEBIAN_FRONTEND=noninteractive apt-get install -y "${packages[@]}"
fi

fc-cache -f
