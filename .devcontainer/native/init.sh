#!/usr/bin/env bash
set -e

# ---- Parallel prep (all independent) ----
(
    sudo apt update
    sudo apt install -y \
        python3 ninja-build clang lld build-essential \
        libfontconfig1-dev libfontconfig1 \
        fonts-ancient-scripts software-properties-common
    sudo add-apt-repository -y multiverse
    sudo apt update
    echo ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true | sudo debconf-set-selections
    sudo apt install -y ttf-mscorefonts-installer
) &

git submodule update --init --recursive &

TMPDIR=/var/tmp dotnet workload install android &

pwsh scripts/install-android-sdk.ps1 &

dotnet tool restore &

# ---- Wait for all prep ----
wait

# ---- Sequential: native build → C# build ----
dotnet cake --target externals-linux --arch=x64
dotnet build tests/SkiaSharp.Tests.Console.sln
