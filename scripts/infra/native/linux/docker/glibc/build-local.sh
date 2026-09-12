#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | riscv64 | loongarch64 | ppc64le | x86 | x64 ]
# $2+ - Additional arguments to pass to the cake script

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-arm64}"
shift 1 || true
EXTRA_ARGS="$@"

# Validate architecture (Dockerfile handles x64→amd64 image-tag mapping internally).
# x86 and ppc64le use separate self-contained Dockerfiles (glibc-x86/, glibc-ppc64le/) — see below.
case "$ARCH" in
  arm|arm64|x64|riscv64|loongarch64) ;;
  x86|ppc64le) ;;
  *) echo "Unsupported architecture: $ARCH"; exit 1 ;;
esac

# x86 and ppc64le have their own self-contained Dockerfiles (builds libc++ in stage 1) since
# the .NET team doesn't ship libc++ for x86 and ppc64le. All other arches share one Dockerfile.
if [ "$ARCH" = "x86" ]; then
  DOCKER_DIR="$DIR/../glibc-x86"
  BUILD_ARGS=""
elif [ "$ARCH" = "ppc64le" ]; then
  DOCKER_DIR="$DIR/../glibc-ppc64le"
  BUILD_ARGS=""
else
  DOCKER_DIR="$DIR"
  BUILD_ARGS="--build-arg BUILD_ARCH=$ARCH"
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
