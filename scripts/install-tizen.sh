#!/usr/bin/env bash
set -e

mkdir -p ~/tizen-temp
cd ~/tizen-temp

# detect platform
if [ "$(uname)" == "Darwin" ]; then
    platform="macos-64"
elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
    platform="ubuntu-64"
else
    exit 1
fi

# download tizen
curl -L -o "tizen-install.bin" "http://download.tizen.org/sdk/Installer/tizen-studio_2.4/web-cli_Tizen_Studio_2.4_$platform.bin"

# install tizen
chmod +x tizen-install.bin
./tizen-install.bin --accept-license --no-java-check ~/tizen-studio

# install packages
cd ~/tizen-studio
./package-manager/package-manager-cli.bin install --no-java-check --accept-license MOBILE-4.0,MOBILE-4.0-NativeAppDevelopment
