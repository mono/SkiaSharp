#!/usr/bin/env bash

mkdir ~/tizen-temp
cd ~/tizen-temp

# mac:   http://download.tizen.org/sdk/Installer/tizen-studio_2.3/web-cli_Tizen_Studio_2.3_macos-64.bin
# linux: http://download.tizen.org/sdk/Installer/tizen-studio_2.3/web-cli_Tizen_Studio_2.3_ubuntu-64.bin

curl -L -o "tizen-install.bin" http://download.tizen.org/sdk/Installer/tizen-studio_2.3/web-cli_Tizen_Studio_2.3_ubuntu-64.bin
chmod +x tizen-install.bin
./tizen-install.bin --accept-license ~/tizen-studio

ls -l ~/tizen-studio

cd ~/tizen-studio
./package-manager/package-manager-cli.bin install --accept-license MOBILE-4.0
