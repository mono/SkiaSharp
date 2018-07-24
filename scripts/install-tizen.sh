#!/usr/bin/env bash
set -e

# download tizen
mkdir ~/tizen-temp
cd ~/tizen-temp
curl -L -o "tizen-install.bin" http://download.tizen.org/sdk/Installer/tizen-studio_2.4/web-cli_Tizen_Studio_2.4_ubuntu-64.bin

# install tizen
chmod +x tizen-install.bin
./tizen-install.bin --accept-license --no-java-check ~/tizen-studio

# install packages
cd ~/tizen-studio
./package-manager/package-manager-cli.bin install --no-java-check --accept-license MOBILE-4.0
