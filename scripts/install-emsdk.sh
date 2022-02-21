#!/usr/bin/env bash
set -e

git clone --branch $@ https://github.com/emscripten-core/emsdk ~/emsdk
~/emsdk/emsdk install $@
~/emsdk/emsdk activate $@
