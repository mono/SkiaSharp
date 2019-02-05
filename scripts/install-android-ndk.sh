#!/usr/bin/env bash
set -e

mkdir -p ~/android-ndk-temp
cd ~/android-ndk-temp

# detect platform
if [ "$(uname)" == "Darwin" ]; then
    platform="darwin-x86_64"
elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
    platform="linux-x86_64"
else
    exit 1
fi

# download ndk
curl -L -o "android-ndk.zip" "https://dl.google.com/android/repository/android-ndk-r15c-$platform.zip"

# unzip ndk
unzip android-ndk.zip
mv ./android-ndk-r15c ~/android-ndk
