#!/usr/bin/env bash
set -ex

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

# the target architecture to build for
ARCH="${1:-arm}"
case $ARCH in
  arm)     BUILD_ARGS="--build-arg TOOLCHAIN_ARCH=arm-linux-gnueabihf --build-arg TOOLCHAIN_ARCH_SHORT=armhf"   ;;
  arm64)   BUILD_ARGS="--build-arg TOOLCHAIN_ARCH=aarch64-linux-gnu   --build-arg TOOLCHAIN_ARCH_SHORT=arm64"   ;;
  riscv64) BUILD_ARGS="--build-arg TOOLCHAIN_ARCH=riscv64-linux-gnu   --build-arg TOOLCHAIN_ARCH_SHORT=riscv64" ;;
  x86)     BUILD_ARGS="--build-arg TOOLCHAIN_ARCH=i686-linux-gnu      --build-arg TOOLCHAIN_ARCH_SHORT=i386"    ;;
  x64)     BUILD_ARGS="--build-arg TOOLCHAIN_ARCH=x86-64-linux-gnu    --build-arg TOOLCHAIN_ARCH_SHORT=amd64"   ;;
  *) echo "Unsupported architecture: $ARCH" && exit 1 ;;
esac

# the docker image architecture to use
IMAGE_ARCH="${2:-$([[ "$(uname -m)" == "arm64" ]] && echo "arm64v8" || echo "amd64")}"

DEBIAN_VERSION=${3:-11}

(cd $DIR && docker build --tag skiasharp-linux-cross-$ARCH $BUILD_ARGS --build-arg IMAGE_ARCH=$IMAGE_ARCH $DEBIAN_VERSION)
(cd $DIR/../../../../ && 
    docker run --rm --name skiasharp-linux-cross-$ARCH --volume $(pwd):/work skiasharp-linux-cross-$ARCH /bin/bash -c "\
        dotnet tool restore ; \
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=$ARCH")
