#!/usr/bin/env bash
set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="arm"
BUILD_ARGS="--build-arg TOOLCHAIN_ARCH=arm-linux-gnueabihf --build-arg TOOLCHAIN_ARCH_SHORT=armhf"
if [ "$1" == "arm64" ]; then
    ARCH="arm64"
    BUILD_ARGS="--build-arg TOOLCHAIN_ARCH=aarch64-linux-gnu --build-arg TOOLCHAIN_ARCH_SHORT=arm64"
fi

(cd $DIR && docker build --tag skiasharp-$ARCH $BUILD_ARGS .)
(cd $DIR/../../../../ && docker run --rm --name skiasharp-$ARCH --volume $(pwd):/work skiasharp-$ARCH /bin/bash dotnet tool restore && dotnet cake -t externals-linux-clang-cross -c Release --buildarch=$ARCH)
