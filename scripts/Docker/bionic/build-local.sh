#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ x64 | arm64 ]

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-x64}"

# Use linux/amd64 for Docker on Apple Silicon because the Android NDK only
# ships linux-x86_64 host tools.
PLATFORM_ARGS=""
if [ "$(uname -m)" = "arm64" ] || [ "$(uname -m)" = "aarch64" ]; then
    PLATFORM_ARGS="--platform linux/amd64"
fi

(cd "$DIR" && 
  docker build $PLATFORM_ARGS --tag "skiasharp-bionic-$ARCH" \
    --build-arg BUILD_ARCH=$ARCH            \
    .)

(cd "$DIR/../../.." && 
    docker run --rm $PLATFORM_ARGS --name "skiasharp-bionic-$ARCH" --volume "$(pwd)":/work "skiasharp-bionic-$ARCH" /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=externals-linux --configuration=Release --buildarch=$ARCH --variant=bionic --verifyExcluded=fontconfig ")
