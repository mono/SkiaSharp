#!/usr/bin/env bash
set -e

# Install FontConfigas that is a dependency of libSkiaSharp.so
sudo apt install -y libfontconfig1

# Install the Emoji fonts
sudo apt install -y ttf-ancient-fonts

# Install the Microsoft fonts - like Times New Roman
echo ttf-mscorefonts-installer msttcorefonts/accepted-mscorefonts-eula select true | sudo debconf-set-selections
sudo apt install -y ttf-mscorefonts-installer
