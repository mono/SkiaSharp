#!/usr/bin/env bash
set -ex

# Build loongarch64 glibc cross-compilation image and run the build.
# Uses the glibc Dockerfile with loongarch64-specific BASE_TAG.
#
# Parameters:
# $@  - Additional arguments to pass to the cake script

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

EXTRA_ARGS="$@"

# Build using the glibc Dockerfile with loongarch64 args
(cd "$DIR/../glibc" &&
  docker build --tag skiasharp-linux-gnu-cross-loongarch64 \
    --platform=linux/amd64                                  \
    --build-arg BASE_TAG=azurelinux-3.0-net10.0-cross-loongarch64 \
    --build-arg BUILD_ARCH=loongarch64                      \
    --build-arg TOOLCHAIN_ARCH=loongarch64-linux-gnu        \
    --build-arg TOOLCHAIN_ARCH_SHORT=loong64                \
    --build-arg TOOLCHAIN_ARCH_TARGET=loongarch64-linux-gnu \
    --build-arg ROOTFS_DIR=/crossrootfs/loongarch64         \
    --build-arg FC_DISTRO=sid                               \
    .)

(cd "$DIR/../../../../../.." &&
    docker run --rm --name skiasharp-linux-gnu-cross-loongarch64 --volume $(pwd):/work skiasharp-linux-gnu-cross-loongarch64 /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=loongarch64 $EXTRA_ARGS ")
