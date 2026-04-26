#!/usr/bin/env bash
set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

EMSCRIPTEN_VERSION="${1:-3.1.56}"
FEATURES="${2:-_wasmeh,st}"

# Use linux/amd64 platform for Docker — older emscripten versions don't publish arm64 binaries
PLATFORM_ARGS=""
if [ "$(uname -m)" = "arm64" ] || [ "$(uname -m)" = "aarch64" ]; then
    PLATFORM_ARGS="--platform linux/amd64"
fi

(cd "$DIR" && docker build $PLATFORM_ARGS --tag skiasharp-wasm:$EMSCRIPTEN_VERSION --build-arg EMSCRIPTEN_VERSION=$EMSCRIPTEN_VERSION .)

FEATURES_ARG=""
if [ -n "$FEATURES" ]; then
    FEATURES_ARG="--emscriptenFeatures='$FEATURES'"
fi

(cd "$DIR/../../../" && \
    docker run --rm $PLATFORM_ARGS --name skiasharp-wasm-$EMSCRIPTEN_VERSION --volume "$(pwd)":/work skiasharp-wasm:$EMSCRIPTEN_VERSION /bin/bash -c "\
        dotnet tool restore ; \
        dotnet cake --target=externals-wasm --emscriptenVersion=$EMSCRIPTEN_VERSION $FEATURES_ARG")
