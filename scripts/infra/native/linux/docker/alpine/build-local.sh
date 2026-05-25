#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ arm | arm64 | x64 | riscv64 | loongarch64 ]
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
  *) echo "Unsupported architecture: $ARCH"; exit 1 ;;
esac

# Build the alpine image — the Dockerfile handles all arch-specific config internally.
(cd "$DIR" &&
  docker build --tag skiasharp-linux-musl-cross-$ARCH \
    --platform=linux/amd64                            \
    --build-arg BUILD_ARCH=$ARCH                      \
    --build-arg IMAGE_ARCH=$IMAGE_ARCH                \
    .)

(cd "$DIR/../../../../../.." &&
    docker run --rm --name skiasharp-linux-musl-cross-$ARCH --volume $(pwd):/work skiasharp-linux-musl-cross-$ARCH /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=externals-linux-clang-cross --configuration=Release --buildarch=$ARCH --variant=alpine $EXTRA_ARGS ")
