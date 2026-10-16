#!/usr/bin/env bash

# Headless Tizen Studio install for Linux containers.
#
# This is the Linux equivalent of install-tizen.ps1 (which targeted the build
# agents). It downloads the Tizen Studio CLI installer, installs it without the
# GUI/Java checks, and adds the native app development packages needed to
# cross-compile libSkiaSharp / libHarfBuzzSharp.
#
# Everything here is pinned to exact values on purpose: the whole point of
# building in a container is a reproducible toolchain, so the version, download
# URL, installer hash, install location and package set are all fixed constants
# rather than parameters. Bumping the toolchain is a deliberate edit to this
# file (and the pinned SHA-256), not a runtime choice.

set -euo pipefail

# Tizen Studio version + the exact installer to download.
VERSION="10.0"
URL="https://download.tizen.org/sdk/Installer/tizen-sdk_${VERSION}/web-cli_Tizen_SDK_${VERSION}_ubuntu-64.bin"

# Expected SHA-256 of the installer above. Tizen does not publish a per-build
# versioned URL (the "10.0" URL is a rolling pointer) nor a checksum file, so we
# pin the hash here. This both verifies download integrity and makes the
# toolchain reproducible: if Tizen ever republishes the installer the build
# fails loudly here and the hash is updated deliberately, rather than silently
# picking up a different toolchain.
INSTALLER_SHA256="65d2a569cbf0351dca4b94254f2c642d49195ac4964e08a76b4eaabc9ce4bed0"

# Install into the (unprivileged) installing user's home. The installer refuses
# to run as root and its Java backend validates that the destination is under
# the user's home directory. This must match TIZEN_STUDIO_HOME in the Dockerfile
# / startup.sh (and therefore what native/tizen/build.cake reads).
DESTINATION="${HOME}/tizen-studio"

# The official SDK catalog selects GCC 14.2 for the Tizen 10 CLI group.
# Specify the compiler package explicitly and assert both cross compilers
# below; installing an older toolchain must not silently satisfy this build.
PACKAGES="TIZEN-10.0-NativeAppDevelopment-CLI,NativeToolchain-Gcc-14.2"

TEMP_DIR="$(mktemp -d)"
INSTALLER="${TEMP_DIR}/tizen-install.bin"

cleanup() { rm -rf "${TEMP_DIR}"; }
trap cleanup EXIT

echo "Installing Tizen Studio ${VERSION} to '${DESTINATION}'..."

# Validate Java is available — the installer and package-manager need it.
echo "Validating Java install..."
java -version

# Download the CLI installer over HTTPS so it cannot be tampered with in transit.
echo "Downloading SDK from '${URL}'..."
curl -fSL --retry 5 --retry-delay 10 -o "${INSTALLER}" "${URL}"

# Verify the download integrity before executing this self-extracting binary.
echo "Verifying installer SHA-256..."
echo "${INSTALLER_SHA256}  ${INSTALLER}" | sha256sum --check --status || {
    echo "ERROR: installer SHA-256 mismatch for Tizen ${VERSION}." >&2
    echo "  expected: ${INSTALLER_SHA256}" >&2
    echo "  actual:   $(sha256sum "${INSTALLER}" | cut -d' ' -f1)" >&2
    echo "The published installer may have changed; update the pinned hash in" >&2
    echo "install-tizen.sh after confirming the new toolchain is correct." >&2
    exit 1
}
chmod +x "${INSTALLER}"

# Install. The CLI installer is built for Ubuntu but is glibc-based, so it runs
# on other glibc distros (e.g. Debian) too. --no-java-check skips the JRE
# bitness probe.
#
# Fail fast: if the installer or package-manager exits non-zero we abort the
# build, even though the original install-tizen.ps1 tolerated non-zero native
# exit codes (PowerShell's $ErrorActionPreference='Stop' did not trap them).
# We deliberately prefer failing a build that might have been fine over silently
# producing a broken toolchain and shipping bad native libraries to customers.
# The artifact validation further below is a second layer of defence for the
# cases where a tool exits 0 without actually installing everything.
echo "Installing SDK..."
if ! bash "${INSTALLER}" --accept-license --no-java-check "${DESTINATION}"; then
    echo "ERROR: Tizen Studio installer exited non-zero — aborting." >&2
    exit 1
fi

# Add the Tizen 10 native rootstraps and GCC 14.2 cross toolchains.
echo "Installing additional packages: '${PACKAGES}'..."
PACKAGE_MANAGER="${DESTINATION}/package-manager/package-manager-cli.bin"
if ! bash "${PACKAGE_MANAGER}" install --no-java-check --accept-license "${PACKAGES}"; then
    echo "ERROR: Tizen package install exited non-zero — aborting." >&2
    exit 1
fi

# Validate the install actually produced the build CLI. The exit-code checks
# above catch hard failures; this catches a tool that exits 0 without producing
# the expected artifacts.
TIZEN_CLI="${DESTINATION}/tools/ide/bin/tizen"
if [ ! -x "${TIZEN_CLI}" ]; then
    echo "ERROR: Tizen CLI not found at '${TIZEN_CLI}' — install failed." >&2
    exit 1
fi

# The package-manager can exit 0 without fully installing the native packages.
# Assert the exact rootstraps the build consumes (native/tizen/build.cake) are
# present, so a partial install fails here at image build time rather than
# confusingly later at `tizen build-native`.
for rootstrap in tizen-10.0-emulator64.core tizen-10.0-device64.core; do
    if ! ls -d "${DESTINATION}"/platforms/*/*/rootstraps/"${rootstrap}" >/dev/null 2>&1; then
        echo "ERROR: required rootstrap '${rootstrap}' not found — package install incomplete." >&2
        exit 1
    fi
done

# Fail at image build time if the exact cross compiler layout changes.
for arch in aarch64 x86_64; do
    gcc="${DESTINATION}/tools/${arch}-linux-gnu-gcc-14.2/bin/${arch}-linux-gnu-g++"
    if [ ! -x "${gcc}" ]; then
        echo "ERROR: required Tizen 10 GCC 14.2 compiler '${gcc}' not found." >&2
        exit 1
    fi
done

echo "Tizen Studio installed successfully at '${DESTINATION}'."
"${TIZEN_CLI}" version || true
