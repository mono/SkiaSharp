#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | riscv64 | loongarch64 | x86 | x64 ]
# $2+ - Additional arguments to pass to the cake script

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-arm64}"
shift 1 || true
EXTRA_ARGS="$@"

# Map architecture to .NET musl cross image tag and toolchain settings
case "$ARCH" in
  arm)
    BASE_TAG=azurelinux-3.0-net10.0-cross-arm-musl
    TOOLCHAIN_ARCH=armv7-alpine-linux-musleabihf
    TOOLCHAIN_ARCH_TARGET=armv7-alpine-linux-musleabihf
    APK_ARCH=armv7
    ROOTFS_DIR=/crossrootfs/arm
    ;;
  arm64)
    BASE_TAG=azurelinux-3.0-net10.0-cross-arm64-musl
    TOOLCHAIN_ARCH=aarch64-alpine-linux-musl
    TOOLCHAIN_ARCH_TARGET=aarch64-alpine-linux-musl
    APK_ARCH=aarch64
    ROOTFS_DIR=/crossrootfs/arm64
    ;;
  riscv64)
    BASE_TAG=azurelinux-3.0-net10.0-cross-riscv64-musl
    TOOLCHAIN_ARCH=riscv64-alpine-linux-musl
    TOOLCHAIN_ARCH_TARGET=riscv64-alpine-linux-musl
    APK_ARCH=riscv64
    ROOTFS_DIR=/crossrootfs/riscv64
    ;;
  loongarch64)
    BASE_TAG=azurelinux-3.0-net10.0-cross-loongarch64-musl
    TOOLCHAIN_ARCH=loongarch64-alpine-linux-musl
    TOOLCHAIN_ARCH_TARGET=loongarch64-alpine-linux-musl
    APK_ARCH=loongarch64
    ROOTFS_DIR=/crossrootfs/loongarch64
    ;;
  x86)
    BASE_TAG=azurelinux-3.0-net10.0-cross-x86-musl
    TOOLCHAIN_ARCH=i586-alpine-linux-musl
    TOOLCHAIN_ARCH_TARGET=i586-alpine-linux-musl
    APK_ARCH=x86
    ROOTFS_DIR=/crossrootfs/x86
    ;;
  x64)
    BASE_TAG=azurelinux-3.0-net10.0-cross-amd64-musl
    TOOLCHAIN_ARCH=x86_64-alpine-linux-musl
    TOOLCHAIN_ARCH_TARGET=x86_64-alpine-linux-musl
    APK_ARCH=x86_64
    ROOTFS_DIR=/crossrootfs/x64
    ;;
  *)
    echo "Unsupported architecture: $ARCH"
    exit 1
    ;;
esac

# Build the alpine image
(cd "$DIR" &&
  docker build --tag skiasharp-linux-musl-cross-$ARCH \
    --platform=linux/amd64                            \
    --build-arg BASE_TAG=$BASE_TAG                    \
    --build-arg BUILD_ARCH=$ARCH                      \
    --build-arg TOOLCHAIN_ARCH=$TOOLCHAIN_ARCH        \
    --build-arg TOOLCHAIN_ARCH_TARGET=$TOOLCHAIN_ARCH_TARGET \
    --build-arg APK_ARCH=$APK_ARCH                    \
    --build-arg ROOTFS_DIR=$ROOTFS_DIR                \
    .)

(cd "$DIR/../../../../../.." &&
    docker run --rm --name skiasharp-linux-musl-cross-$ARCH --volume $(pwd):/work skiasharp-linux-musl-cross-$ARCH /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=$ARCH --variant=alpine $EXTRA_ARGS ")
