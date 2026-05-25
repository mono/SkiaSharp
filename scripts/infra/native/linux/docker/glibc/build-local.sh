#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | riscv64 | x86 | x64 ]
# $2+ - Additional arguments to pass to the cake script

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-arm64}"
shift 1 || true
EXTRA_ARGS="$@"

# Map architecture to .NET cross image tag and toolchain settings
case "$ARCH" in
  arm)
    BASE_TAG=azurelinux-3.0-net10.0-cross-arm
    TOOLCHAIN_ARCH=arm-linux-gnueabihf
    TOOLCHAIN_ARCH_SHORT=armhf
    TOOLCHAIN_ARCH_TARGET=armv7a-linux-gnueabihf
    ROOTFS_DIR=/crossrootfs/arm
    VERIFY_GLIBC_MAX=2.35
    FC_DISTRO=jammy
    FC_VERSION=2.13.1-4.4ubuntu0.1
    FC_DEV_SHA256=""
    FC_LIB_SHA256=""
    ;;
  arm64)
    BASE_TAG=azurelinux-3.0-net10.0-cross-arm64
    TOOLCHAIN_ARCH=aarch64-linux-gnu
    TOOLCHAIN_ARCH_SHORT=arm64
    TOOLCHAIN_ARCH_TARGET=aarch64-linux-gnu
    ROOTFS_DIR=/crossrootfs/arm64
    VERIFY_GLIBC_MAX=2.27
    FC_DISTRO=bionic
    FC_VERSION=2.12.6-0ubuntu2
    FC_DEV_SHA256=""
    FC_LIB_SHA256=""
    ;;
  riscv64)
    BASE_TAG=azurelinux-3.0-net10.0-cross-riscv64
    TOOLCHAIN_ARCH=riscv64-linux-gnu
    TOOLCHAIN_ARCH_SHORT=riscv64
    TOOLCHAIN_ARCH_TARGET=riscv64-linux-gnu
    ROOTFS_DIR=/crossrootfs/riscv64
    VERIFY_GLIBC_MAX=2.39
    FC_DISTRO=noble
    FC_VERSION=2.15.0-1.1ubuntu2
    FC_DEV_SHA256=""
    FC_LIB_SHA256=""
    ;;
  x86)
    BASE_TAG=azurelinux-3.0-net10.0-cross-x86
    TOOLCHAIN_ARCH=i386-linux-gnu
    TOOLCHAIN_ARCH_SHORT=i386
    TOOLCHAIN_ARCH_TARGET=i686-linux-gnu
    ROOTFS_DIR=/crossrootfs/x86
    VERIFY_GLIBC_MAX=2.27
    FC_DISTRO=bionic
    FC_VERSION=2.12.6-0ubuntu2
    FC_DEV_SHA256=""
    FC_LIB_SHA256=""
    ;;
  x64)
    BASE_TAG=azurelinux-3.0-net10.0-cross-amd64
    TOOLCHAIN_ARCH=x86_64-linux-gnu
    TOOLCHAIN_ARCH_SHORT=amd64
    TOOLCHAIN_ARCH_TARGET=x86_64-linux-gnu
    ROOTFS_DIR=/crossrootfs/x64
    VERIFY_GLIBC_MAX=2.27
    FC_DISTRO=bionic
    FC_VERSION=2.12.6-0ubuntu2
    FC_DEV_SHA256=""
    FC_LIB_SHA256=""
    ;;
  *)
    echo "Unsupported architecture: $ARCH"
    echo "For loongarch64, use ../loongarch64/build-local.sh"
    exit 1
    ;;
esac

# Build the glibc image
(cd "$DIR" &&
  docker build --tag skiasharp-linux-gnu-cross-$ARCH \
    --platform=linux/amd64                           \
    --build-arg BASE_TAG=$BASE_TAG                   \
    --build-arg BUILD_ARCH=$ARCH                     \
    --build-arg TOOLCHAIN_ARCH=$TOOLCHAIN_ARCH       \
    --build-arg TOOLCHAIN_ARCH_SHORT=$TOOLCHAIN_ARCH_SHORT \
    --build-arg TOOLCHAIN_ARCH_TARGET=$TOOLCHAIN_ARCH_TARGET \
    --build-arg ROOTFS_DIR=$ROOTFS_DIR               \
    --build-arg FC_DISTRO=$FC_DISTRO                 \
    --build-arg FC_VERSION=$FC_VERSION               \
    --build-arg FC_DEV_SHA256=$FC_DEV_SHA256         \
    --build-arg FC_LIB_SHA256=$FC_LIB_SHA256         \
    .)

(cd "$DIR/../../../../../.." &&
    docker run --rm --name skiasharp-linux-gnu-cross-$ARCH --volume $(pwd):/work skiasharp-linux-gnu-cross-$ARCH /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=$ARCH --verifyGlibcMax=$VERIFY_GLIBC_MAX $EXTRA_ARGS ")
