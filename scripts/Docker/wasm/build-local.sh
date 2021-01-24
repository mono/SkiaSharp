#!/usr/bin/env bash
set -e

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

(cd $DIR && docker build --tag skiasharp-wasm --build-arg EMSCRIPTEN_VERSION="$EMSCRIPTEN_VERSION" .)
(cd $DIR/../../../ && docker run --rm --name skiasharp-wasm --volume $(pwd):/work skiasharp-wasm /bin/bash ./bootstrapper.sh -t externals-wasm)
