#!/usr/bin/env bash
set -e

# ---- Parallel prep (all independent) ----
(
    sudo apt update
    sudo apt install -y libfontconfig1 fonts-ancient-scripts software-properties-common
    sudo add-apt-repository -y multiverse
    sudo apt update
    echo ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true | sudo debconf-set-selections
    sudo apt install -y ttf-mscorefonts-installer
) &

TMPDIR=/var/tmp dotnet workload install android &

pwsh scripts/install-android-sdk.ps1 &

# ---- Sequential: tool restore → download natives ----
dotnet tool restore
dotnet cake --target externals-download

# ---- Wait for all parallel jobs before building ----
wait

# ---- Build tests ----
dotnet build tests/SkiaSharp.Tests.Console.sln
