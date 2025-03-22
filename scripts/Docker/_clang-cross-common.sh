#!/usr/bin/env bash
set -ex

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
DOCKER_DIR="$1"

# the target architecture to build for
ARCH="${2:-arm}"

# the docker image architecture to use
MACHINE_ARCH="$(uname -m)"
[ "$MACHINE_ARCH" = "arm64" ] && MACHINE_ARCH=aarch64
IMAGE_ARCH="${3:-$([[ "$MACHINE_ARCH" == "aarch64" ]] && echo "arm64v8" || echo "amd64")}"

DISTRO_VERSION=$4
ABI=$5
VENDOR=$6

case $ARCH in
  arm)     TOOLCHAIN_ARCH=arm$VENDOR-linux-${ABI}eabihf ; TOOLCHAIN_ARCH_SHORT=armhf   ; TARGET_MACHINE_ARCH=armhf   ;;
  arm64)   TOOLCHAIN_ARCH=aarch64$VENDOR-linux-$ABI     ; TOOLCHAIN_ARCH_SHORT=arm64   ; TARGET_MACHINE_ARCH=aarch64 ;;
  riscv64) TOOLCHAIN_ARCH=riscv64$VENDOR-linux-$ABI     ; TOOLCHAIN_ARCH_SHORT=riscv64 ; TARGET_MACHINE_ARCH=riscv64 ;;
  x86)     TOOLCHAIN_ARCH=i686$VENDOR-linux-$ABI        ; TOOLCHAIN_ARCH_SHORT=i386    ; TARGET_MACHINE_ARCH=x86     ;;
  x64)     TOOLCHAIN_ARCH=x86-64$VENDOR-linux-$ABI      ; TOOLCHAIN_ARCH_SHORT=amd64   ; TARGET_MACHINE_ARCH=x86_64  ;;
  *) echo "Unsupported architecture: $ARCH" && exit 1 ;;
esac

(cd $DIR && docker build --tag skiasharp-linux-$ABI-cross-$ARCH \
    --build-arg TOOLCHAIN_ARCH=$TOOLCHAIN_ARCH                  \
    --build-arg TOOLCHAIN_ARCH_SHORT=$TOOLCHAIN_ARCH_SHORT      \
    --build-arg IMAGE_ARCH=$IMAGE_ARCH                          \
    --build-arg MACHINE_ARCH=$MACHINE_ARCH                      \
    --build-arg TARGET_MACHINE_ARCH=$TARGET_MACHINE_ARCH        \
    --build-arg DISTRO_VERSION=$DISTRO_VERSION                  \
    $DOCKER_DIR)

if [ "$VENDOR" = "-alpine" ]; then vendor=alpine; fi

(cd $DIR/../.. && 
    docker run --rm --name skiasharp-linux-$ABI-cross-$ARCH --volume $(pwd):/work skiasharp-linux-$ABI-cross-$ARCH /bin/bash -c "\
        dotnet tool restore &&
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=$ARCH --variant=$vendor")
