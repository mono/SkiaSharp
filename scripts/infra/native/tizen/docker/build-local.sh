#!/usr/bin/env bash
set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

# The Tizen toolchain version is fixed inside the image (see install-tizen.sh),
# so the only knob here is which architecture(s) to build.
BUILD_ARCH="${1:-all}"

# Tizen Studio only ships x86_64 (linux/amd64) tooling, so always build and run
# the image as linux/amd64 — even on Apple Silicon / arm64 hosts.
PLATFORM_ARGS="--platform linux/amd64"

(cd "$DIR" && docker build $PLATFORM_ARGS --tag skiasharp-tizen .)

BUILD_ARCH_ARG=""
if [ -n "$BUILD_ARCH" ] && [ "$BUILD_ARCH" != "all" ]; then
    BUILD_ARCH_ARG="--buildarch=$BUILD_ARCH"
fi

(cd "$DIR/../../../../.." && \
    docker run --rm $PLATFORM_ARGS --name skiasharp-tizen --volume "$(pwd)":/work skiasharp-tizen /bin/bash -c "\
        dotnet tool restore ; \
        dotnet cake --target=externals-tizen $BUILD_ARCH_ARG")
