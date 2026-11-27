#!/bin/bash -e -o pipefail

# Installs an Xcode from an 'xcode-apple-silicon' Universal Package, as published by
# dotnet/macios to the internal 'macios-tools-internal' feed. The package holds
# xcode-metadata.json and the Apple-signed .xip it describes.
#
# Usage: install-xcode-package.sh <package-directory> <xcode-version>
# Installs to /Applications/Xcode_<xcode-version>.app so select-xcode.sh can switch to it.

PACKAGE_DIRECTORY=${1:?Usage: $0 <package-directory> <xcode-version>}
XCODE_VERSION=${2:?Usage: $0 <package-directory> <xcode-version>}

MANIFEST="$PACKAGE_DIRECTORY/xcode-metadata.json"
XCODE_APP="/Applications/Xcode_$XCODE_VERSION.app"

if [[ ! -f "$MANIFEST" ]]; then
  echo "The Xcode package metadata is missing: $MANIFEST" >&2
  exit 1
fi

BUILD_VERSION=$(jq -er '.xcode.buildVersion' "$MANIFEST")
ARCHIVE_NAME=$(jq -er '.artifacts[0].fileName' "$MANIFEST")
EXPECTED_SHA256=$(jq -er '.artifacts[0].sha256' "$MANIFEST")
ARCHIVE="$PACKAGE_DIRECTORY/$ARCHIVE_NAME"

if [[ ! "$ARCHIVE_NAME" =~ ^[A-Za-z0-9._-]+\.xip$ || ! -f "$ARCHIVE" ]]; then
  echo "The Xcode package archive is missing or has an unexpected name: $ARCHIVE_NAME" >&2
  exit 1
fi

ACTUAL_SHA256=$(shasum -a 256 "$ARCHIVE" | cut -d ' ' -f 1)
if [[ "$ACTUAL_SHA256" != "$EXPECTED_SHA256" ]]; then
  echo "SHA-256 mismatch for $ARCHIVE: expected $EXPECTED_SHA256, got $ACTUAL_SHA256." >&2
  exit 1
fi
if ! pkgutil --check-signature "$ARCHIVE" | grep -q 'signed Apple Software'; then
  echo "$ARCHIVE is not signed Apple Software." >&2
  exit 1
fi

echo "Disk space before expanding:"
df -h /

STAGING=$(mktemp -d "${AGENT_TEMPDIRECTORY:-${TMPDIR:-/tmp}}/xcode-expand.XXXXXX")
(cd "$STAGING" && xip --expand "$ARCHIVE")
rm -f "$ARCHIVE"

EXPANDED_APP=$(find "$STAGING" -maxdepth 1 -name 'Xcode*.app' -type d | head -n 1)
if [[ -z "$EXPANDED_APP" ]]; then
  echo "No Xcode application found in $ARCHIVE_NAME." >&2
  exit 1
fi

APP_VERSION=$(/usr/libexec/PlistBuddy -c 'Print :CFBundleShortVersionString' "$EXPANDED_APP/Contents/Info.plist")
APP_BUILD=$(/usr/libexec/PlistBuddy -c 'Print :ProductBuildVersion' "$EXPANDED_APP/Contents/version.plist")
if [[ "$APP_VERSION" != "$XCODE_VERSION" || "$APP_BUILD" != "$BUILD_VERSION" ]]; then
  echo "The package contains Xcode $APP_VERSION ($APP_BUILD); expected $XCODE_VERSION ($BUILD_VERSION)." >&2
  exit 1
fi

sudo rm -rf -- "$XCODE_APP"
sudo mv "$EXPANDED_APP" "$XCODE_APP"
rm -rf -- "$STAGING"

sudo "$XCODE_APP/Contents/Developer/usr/bin/xcodebuild" -license accept
sudo "$XCODE_APP/Contents/Developer/usr/bin/xcodebuild" -runFirstLaunch

echo "Installed Xcode $APP_VERSION ($APP_BUILD) at $XCODE_APP."
