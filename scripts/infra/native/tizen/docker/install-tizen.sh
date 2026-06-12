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
# Use the CLI ("-CLI") native app development packages, not the IDE ones. They
# install the identical build toolchain (NativeToolchain-Gcc-9.2, the tizen-8.0
# rootstraps, llvm-10 and the `tizen build-native` CLI) but skip the GUI-only
# NativeIDE / Certificate-Manager components. NativeIDE has a post-install step
# that hangs indefinitely in a headless container (no display), so the IDE
# packages must not be used here. The produced toolchain — and therefore the
# built libraries — is the same as the old agent install.
PACKAGES="${3:-MOBILE-6.0-NativeAppDevelopment-CLI,TIZEN-8.0-NativeAppDevelopment-CLI}"

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
#
# The installer and package-manager can exit non-zero even on an otherwise
# successful install, so do not let `set -e` abort here — the artifact-based
# validation below is the source of truth. We still capture and log the exit
# codes for diagnostics.
echo "Installing SDK..."
installer_rc=0
bash "${INSTALLER}" --accept-license --no-java-check "${DESTINATION}" || installer_rc=$?
echo "Installer exited with ${installer_rc}."

# Add the native app development packages (rootstraps + llvm toolchain).
echo "Installing additional packages: '${PACKAGES}'..."
PACKAGE_MANAGER="${DESTINATION}/package-manager/package-manager-cli.${EXT}"
packages_rc=0
bash "${PACKAGE_MANAGER}" install --no-java-check --accept-license "${PACKAGES}" || packages_rc=$?
echo "Package manager exited with ${packages_rc}."

# Validate the install actually produced the build CLI. The installer can exit
# non-zero / oddly even on success, so trust the artifact, not the exit code.
TIZEN_CLI="${DESTINATION}/tools/ide/bin/tizen"
if [ ! -x "${TIZEN_CLI}" ]; then
    echo "ERROR: Tizen CLI not found at '${TIZEN_CLI}' — install failed." >&2
    exit 1
fi

# The package-manager can exit 0 without fully installing the native packages.
# Assert the exact rootstraps the build consumes (native/tizen/build.cake) and a
# cross llvm toolchain are present, so a partial install fails here at image
# build time rather than confusingly later at `tizen build-native`.
for rootstrap in tizen-8.0-emulator64.core tizen-8.0-device64.core; do
    if ! ls -d "${DESTINATION}"/platforms/*/*/rootstraps/"${rootstrap}" >/dev/null 2>&1; then
        echo "ERROR: required rootstrap '${rootstrap}' not found — package install incomplete." >&2
        exit 1
    fi
done
if ! ls -d "${DESTINATION}"/tools/llvm-* >/dev/null 2>&1; then
    echo "ERROR: Tizen llvm toolchain not found under '${DESTINATION}/tools' — package install incomplete." >&2
    exit 1
fi

echo "Tizen Studio installed successfully at '${DESTINATION}'."
"${TIZEN_CLI}" version || true
