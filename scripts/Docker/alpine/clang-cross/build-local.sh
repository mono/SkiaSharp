#!/usr/bin/env bash
set -ex

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

# the target architecture to build for
ARCH="${1:-arm}"
case $ARCH in
  arm)   BUILD_ARGS="--build-arg TOOLCHAIN_ARCH=arm-none-eabi --build-arg TOOLCHAIN_ARCH_SHORT=armhf" ;;
  arm64) BUILD_ARGS="--build-arg TOOLCHAIN_ARCH=aarch64-none-elf   --build-arg TOOLCHAIN_ARCH_SHORT=aarch64" ;;
  *) echo "Unsupported architecture: $ARCH" && exit 1 ;;
esac

# the docker image architecture to use
IMAGE_ARCH="${2:-$([[ "$(uname -m)" == "arm64" ]] && echo "-arm64v8-local" || echo "")}"

(cd $DIR && docker build --tag skiasharp-alpine-cross-$ARCH $BUILD_ARGS --build-arg IMAGE_ARCH=$IMAGE_ARCH .)
(cd $DIR/../../../../ && 
    docker run --rm --name skiasharp-alpine-cross-$ARCH --volume $(pwd):/work skiasharp-alpine-cross-$ARCH /bin/bash -c "\
        dotnet tool restore ; \
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=$ARCH --variant=alpine")
