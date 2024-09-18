#!/usr/bin/env bash
set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

VERSION_ARGS=""
EMSCRIPTEN_VERSION=3.1.34
if [ "$1" ]; then
    VERSION_ARGS="--build-arg EMSCRIPTEN_VERSION=$1"
    EMSCRIPTEN_VERSION=$1
fi

(cd $DIR && docker build --tag skiasharp-wasm:$EMSCRIPTEN_VERSION $VERSION_ARGS .)
(cd $DIR/../../../ && \
    docker run --rm --name skiasharp-wasm-$EMSCRIPTEN_VERSION --volume $(pwd):/work skiasharp-wasm:$EMSCRIPTEN_VERSION /bin/bash -c "\
        dotnet tool restore ; \
        dotnet cake --target=externals-wasm --emscriptenVersion=$EMSCRIPTEN_VERSION")

# sudo chown -R $(id -u):$(id -g) .
# (cd samples/Basic/Uno/SkiaSharpSample.Wasm/bin/Debug/netstandard2.0/dist && python3 server.py)
