#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | riscv64 | x86 | x64 ]
# $2+ - Additional arguments to pass to the cake script

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-arm64}"
shift 1 || true
EXTRA_ARGS="$@"

# Determine sysroot release based on architecture
case "$ARCH" in
  riscv64) SYSROOT_RELEASE=trixie ;;
  *)       SYSROOT_RELEASE=bullseye ;;
esac

# the docker platform to use
MACHINE_ARCH="$(uname -m)"
case $MACHINE_ARCH in
  arm64) PLATFORM=linux/arm64 ; MACHINE_ARCH=aarch64 ;;
  *)     PLATFORM=linux/amd64 ;;
esac

# Build the glibc image
(cd "$DIR" &&
  docker build --tag skiasharp-linux-gnu-cross-$ARCH \
    --platform=$PLATFORM                             \
    --build-arg BUILD_ARCH=$ARCH                     \
    --build-arg MACHINE_ARCH=$MACHINE_ARCH           \
    --build-arg SYSROOT_RELEASE=$SYSROOT_RELEASE     \
    .)

# GLIBC verification based on sysroot release
ADDITIONAL_ARGS=""
case "$SYSROOT_RELEASE" in
  bullseye) ADDITIONAL_ARGS="--verifyGlibcMax=2.31" ;;
  trixie)   ADDITIONAL_ARGS="--verifyGlibcMax=2.38" ;;
esac

(cd "$DIR/../../../../../.." &&
    docker run --rm --name skiasharp-linux-gnu-cross-$ARCH --volume $(pwd):/work skiasharp-linux-gnu-cross-$ARCH /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=$ARCH $ADDITIONAL_ARGS $EXTRA_ARGS ")
