#!/usr/bin/env bash
set -e

# detect platform
if [ "$(uname)" == "Darwin" ]; then
    platform="macos-64"
elif [ "$(expr substr $(uname -s) 1 5)" == "Linux" ]; then
    platform="ubuntu-64"
else
    exit 1
fi

url=http://download.tizen.org/sdk/Installer/tizen-studio_2.4/web-cli_Tizen_Studio_2.4_$platform.bin
packages=MOBILE-4.0,MOBILE-4.0-NativeAppDevelopment
bin=~/tizen-temp/tizen-install.bin

# download tizen
mkdir -p ~/tizen-temp
curl -L -o "$bin" "$url"

# install tizen
chmod +x $bin
bash $bin --accept-license --no-java-check ~/tizen-studio

# install packages
bash ~/tizen-studio/package-manager/package-manager-cli.bin install --no-java-check --accept-license $packages
