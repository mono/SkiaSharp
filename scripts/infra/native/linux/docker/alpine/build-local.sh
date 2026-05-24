#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | riscv64 | x64 | loongarch64 ]
# $2+ - Additional arguments to pass to the cake script

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-arm64}"
shift 1 || true
EXTRA_ARGS="$@"

# the docker platform to use
MACHINE_ARCH="$(uname -m)"
case $MACHINE_ARCH in
  arm64) PLATFORM=linux/arm64 ; MACHINE_ARCH=aarch64 ;;
  *)     PLATFORM=linux/amd64 ;;
esac

# Build the alpine image
(cd "$DIR" &&
  docker build --tag skiasharp-linux-musl-cross-$ARCH \
    --platform=$PLATFORM                              \
    --build-arg BUILD_ARCH=$ARCH                      \
    --build-arg MACHINE_ARCH=$MACHINE_ARCH            \
    .)

(cd "$DIR/../../../../../.." &&
    docker run --rm --name skiasharp-linux-musl-cross-$ARCH --volume $(pwd):/work skiasharp-linux-musl-cross-$ARCH /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=$ARCH --variant=alpine $EXTRA_ARGS ")
