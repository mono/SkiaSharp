#!/usr/bin/env bash

# Headless Tizen Studio install for Linux containers.
#
# This is the Linux equivalent of install-tizen.ps1 (which targeted the
# build agents). It downloads the Tizen Studio CLI installer, installs it
# without the GUI/Java checks, and adds the native app development packages
# needed to cross-compile libSkiaSharp / libHarfBuzzSharp.
#
# Usage:
#   install-tizen.sh [VERSION] [DESTINATION] [PACKAGES]
#
#   VERSION      Tizen Studio version           (default: 10.0)
#   DESTINATION  install directory              (default: /opt/tizen-studio)
#   PACKAGES     comma-separated package list   (default: MOBILE-6.0 + TIZEN-8.0 native)

set -euo pipefail

VERSION="${1:-10.0}"
DESTINATION="${2:-/opt/tizen-studio}"
PACKAGES="${3:-MOBILE-6.0-NativeAppDevelopment,TIZEN-8.0-NativeAppDevelopment}"

PLATFORM="ubuntu-64"
EXT="bin"
URL="http://download.tizen.org/sdk/Installer/tizen-sdk_${VERSION}/web-cli_Tizen_SDK_${VERSION}_${PLATFORM}.${EXT}"

TEMP_DIR="$(mktemp -d)"
INSTALLER="${TEMP_DIR}/tizen-install.${EXT}"

cleanup() { rm -rf "${TEMP_DIR}"; }
trap cleanup EXIT

echo "Installing Tizen Studio ${VERSION} to '${DESTINATION}'..."

# Validate Java is available — the installer and package-manager need it.
echo "Validating Java install..."
java -version

# Download the CLI installer.
echo "Downloading SDK from '${URL}'..."
curl -fSL --retry 5 --retry-delay 10 -o "${INSTALLER}" "${URL}"
chmod +x "${INSTALLER}"

# Install. The CLI installer is built for Ubuntu but is glibc-based, so it
# runs on other glibc distros too. --no-java-check skips the JRE bitness probe.
echo "Installing SDK..."
bash "${INSTALLER}" --accept-license --no-java-check "${DESTINATION}"

# Add the native app development packages (rootstraps + llvm toolchain).
echo "Installing additional packages: '${PACKAGES}'..."
PACKAGE_MANAGER="${DESTINATION}/package-manager/package-manager-cli.${EXT}"
bash "${PACKAGE_MANAGER}" install --no-java-check --accept-license "${PACKAGES}"

# Validate the install actually produced the build CLI. The installer can exit
# non-zero / oddly even on success, so trust the artifact, not the exit code.
TIZEN_CLI="${DESTINATION}/tools/ide/bin/tizen"
if [ ! -x "${TIZEN_CLI}" ]; then
    echo "ERROR: Tizen CLI not found at '${TIZEN_CLI}' — install failed." >&2
    exit 1
fi

echo "Tizen Studio installed successfully at '${DESTINATION}'."
"${TIZEN_CLI}" version || true
