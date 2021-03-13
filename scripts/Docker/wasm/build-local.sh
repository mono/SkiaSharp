#!/usr/bin/env bash
set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

VERSION_ARGS=""
if [ "$1" ]; then
    VERSION_ARGS="--build-arg EMSCRIPTEN_VERSION=$1"
fi

(cd $DIR && docker build --tag skiasharp-wasm $VERSION_ARGS .)
(cd $DIR/../../../ && docker run --rm --name skiasharp-wasm --volume $(pwd):/work skiasharp-wasm /bin/bash ./bootstrapper.sh -t externals-wasm)
