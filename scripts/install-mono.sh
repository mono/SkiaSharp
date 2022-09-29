#!/usr/bin/env bash
set -e

sudo apt remove -y mono-complete msbuild
sudo apt autoremove -y
sudo rm /etc/mono/config
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb https://download.mono-project.com/repo/ubuntu $@ main" | sudo tee /etc/apt/sources.list.d/mono-official-stable.list
sudo apt update
sudo apt install -y mono-complete msbuild
mono --version
