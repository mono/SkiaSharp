#!/usr/bin/env bash
set -ex

# Parameters:
# $1 - The target architecture to build for     [ x64 | arm64 ]

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

ARCH="${1:-x64}"

(cd $DIR && 
  docker build --tag skiasharp-bionic-$ARCH \
    --build-arg BUILD_ARCH=$ARCH            \
    .)

(cd $DIR/../../.. && 
    docker run --rm --name skiasharp-bionic-$ARCH --volume $(pwd):/work skiasharp-bionic-$ARCH /bin/bash -c " \
        dotnet tool restore ; \
        dotnet cake --target=externals-linux --configuration=Release --buildarch=$ARCH --variant=bionic ")
