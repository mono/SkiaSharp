#!/usr/bin/env bash
set -e

# Update APT package lists
sudo apt update

# Install FontConfig - this is a dependency of libSkiaSharp.so
sudo apt install -y libfontconfig1

# Install the Emoji fonts - for tests
sudo apt install -y ttf-ancient-fonts

# Install the Microsoft fonts (eg: Times New Roman) - for tests
echo ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true | sudo debconf-set-selections
sudo apt install -y ttf-mscorefonts-installer
