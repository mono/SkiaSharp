#!/bin/bash -e -o pipefail

XCODE_VERSION=$1

echo "Xcode versions:"
ls -ld /Applications/Xcode*.app

echo "##vso[task.setvariable variable=MD_APPLE_SDK_ROOT;]/Applications/Xcode_$XCODE_VERSION.app"

xcode-select --switch "/Applications/Xcode_$XCODE_VERSION.app/Contents/Developer"
