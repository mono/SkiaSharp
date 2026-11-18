#!/bin/bash -e -o pipefail

XCODE_VERSION=$1
XCODE_PATH="/Applications/Xcode_$XCODE_VERSION.app"

echo "Xcode versions:"
ls -ld /Applications/Xcode*.app

# GoldenGate names its Xcode 27.0 installation Xcode_27.app.
if [[ ! -d "$XCODE_PATH" && "$XCODE_VERSION" == *.0 ]]; then
    XCODE_PATH="/Applications/Xcode_${XCODE_VERSION%.0}.app"
fi

INSTALLED_VERSION=$(/usr/libexec/PlistBuddy -c 'Print :CFBundleShortVersionString' "$XCODE_PATH/Contents/version.plist")
if [[ "$INSTALLED_VERSION" != "$XCODE_VERSION" ]]; then
    echo "Expected Xcode $XCODE_VERSION at $XCODE_PATH, found $INSTALLED_VERSION." >&2
    exit 1
fi

xcode-select --switch "$XCODE_PATH/Contents/Developer"
xcodebuild -version
echo "##vso[task.setvariable variable=MD_APPLE_SDK_ROOT;]$XCODE_PATH"
