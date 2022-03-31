#!/bin/bash -e -o pipefail

XCODE_VERSION=$1

ls -l /Applications/Xcode*

echo "##vso[task.setvariable variable=MD_APPLE_SDK_ROOT;]/Applications/Xcode_$XCODE_VERSION.app"

xcode-select --switch "/Applications/Xcode_$XCODE_VERSION.app/Contents/Developer"
