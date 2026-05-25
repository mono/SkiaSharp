#!/usr/bin/env bash
set -ex

# Build loongarch64 glibc cross-compilation image and run the build.
# No architecture parameter — this image is single-arch.
#
# Parameters:
# $@  - Additional arguments to pass to the cake script

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

EXTRA_ARGS="$@"

# the docker platform to use
MACHINE_ARCH="$(uname -m)"
case $MACHINE_ARCH in
  arm64) PLATFORM=linux/arm64 ; MACHINE_ARCH=aarch64 ;;
  *)     PLATFORM=linux/amd64 ;;
esac

# Build the loongarch64 image
(cd "$DIR" &&
  docker build --tag skiasharp-linux-gnu-cross-loongarch64 \
    --platform=$PLATFORM                                   \
    --build-arg MACHINE_ARCH=$MACHINE_ARCH                 \
    .)

(cd "$DIR/../../../../../.." &&
    docker run --rm --name skiasharp-linux-gnu-cross-loongarch64 --volume $(pwd):/work skiasharp-linux-gnu-cross-loongarch64 /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=loongarch64 --verifyGlibcMax=2.38 $EXTRA_ARGS ")
