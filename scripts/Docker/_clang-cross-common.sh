#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The directory containing the Dockerfile  [ clang-cross/10 | clang-cross ]
# $2 - The target architecture to build for     [ arm | arm64 | riscv64 | x86 | x64 | loongarch64 ]
# $3 - The ABI                                  [ gnu | musl ]
# $4 - The variant                              [ "" | alpine ]

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

# the directory containing the Dockerfile
DOCKER_DIR="$1"

# the target architecture to build for
ARCH="$2"

# the docker image architecture to use
MACHINE_ARCH="$(uname -m)"
case $MACHINE_ARCH in
  arm64) IMAGE_ARCH=arm64v8 ; MACHINE_ARCH=aarch64 ;;
  *)     IMAGE_ARCH=amd64   ;;
esac

# the ABI
ABI=$3

# the variant
VARIANT=$4

(cd $DIR && 
  docker build --tag skiasharp-linux-$ABI-cross-$ARCH \
    --build-arg BUILD_ARCH=$ARCH                      \
    --build-arg IMAGE_ARCH=$IMAGE_ARCH                \
    --build-arg MACHINE_ARCH=$MACHINE_ARCH            \
    $DOCKER_DIR)

[ -n "$VARIANT" ] && VARIANT="--variant=$VARIANT"

(cd $DIR/../.. && 
    docker run --rm --name skiasharp-linux-$ABI-cross-$ARCH --volume $(pwd):/work skiasharp-linux-$ABI-cross-$ARCH /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=$ARCH $VARIANT ")
