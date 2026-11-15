#!/usr/bin/env bash

set -e
echo 'KERNEL=="kvm", GROUP="kvm", MODE="0666", OPTIONS+="static_node=kvm"' | sudo tee /etc/udev/rules.d/99-kvm4all.rules
sudo udevadm control --reload-rules
sudo udevadm trigger --name-match=kvm

mkdir -p "$HOME/.android/avd"
echo "##vso[task.setvariable variable=ANDROID_AVD_HOME]$HOME/.android/avd"
echo "##vso[task.setvariable variable=ANDROID_EMULATOR_HOME]$HOME/.android"
