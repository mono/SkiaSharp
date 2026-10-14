#!/bin/bash
set -euo pipefail

XCODE_VERSION=${1:?Pass the exact installed Xcode version.}
if [[ ! "$XCODE_VERSION" =~ ^[0-9]+(\.[0-9]+){1,2}$ ]]; then
    echo "Invalid Xcode version: $XCODE_VERSION. Pass an exact numeric version." >&2
    exit 1
fi

XCODE_ROOT="/Applications/Xcode_$XCODE_VERSION.app"
export DEVELOPER_DIR="$XCODE_ROOT/Contents/Developer"
if [[ ! -d "$DEVELOPER_DIR" || ! -x "$DEVELOPER_DIR/usr/bin/xcodebuild" ]]; then
    echo "Required Xcode is not installed at $XCODE_ROOT. Provision this exact Xcode before running the job." >&2
    exit 1
fi

"$DEVELOPER_DIR/usr/bin/xcodebuild" -version

echo "##vso[task.setvariable variable=DEVELOPER_DIR;]$DEVELOPER_DIR"
echo "##vso[task.setvariable variable=MD_APPLE_SDK_ROOT;]$XCODE_ROOT"
