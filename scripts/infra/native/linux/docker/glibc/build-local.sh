#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | riscv64 | loongarch64 | x86 | x64 ]
# $2+ - Additional arguments to pass to the cake script

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-arm64}"
shift 1 || true
EXTRA_ARGS="$@"

# Validate architecture and map to .NET image tag arch
case "$ARCH" in
  arm|arm64|riscv64|loongarch64)
    IMAGE_ARCH="$ARCH" ;;
  x64)
    IMAGE_ARCH="amd64" ;;
  x86)
    IMAGE_ARCH="" ;;  # x86 uses a self-contained Dockerfile (glibc-x86/)
  *) echo "Unsupported architecture: $ARCH"; exit 1 ;;
esac

# x86 has its own self-contained Dockerfile (builds libc++ in stage 1) since
# the .NET team doesn't ship libc++ for x86. All other arches share one Dockerfile.
if [ "$ARCH" = "x86" ]; then
  DOCKER_DIR="$DIR/../glibc-x86"
  BUILD_ARGS=""
else
  DOCKER_DIR="$DIR"
  BUILD_ARGS="--build-arg BUILD_ARCH=$ARCH --build-arg IMAGE_ARCH=$IMAGE_ARCH"
fi

(cd "$DOCKER_DIR" &&
  docker build --tag skiasharp-linux-gnu-cross-$ARCH \
    --platform=linux/amd64                           \
    $BUILD_ARGS                                      \
    .)

(cd "$DIR/../../../../../.." &&
    docker run --rm --name skiasharp-linux-gnu-cross-$ARCH --volume $(pwd):/work skiasharp-linux-gnu-cross-$ARCH /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=$ARCH $EXTRA_ARGS ")
