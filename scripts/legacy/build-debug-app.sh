#!/usr/bin/env bash
set -e

# git-sync-deps
(cd externals/skia && python tools/git-sync-deps)

# compiler options
CUSTOM_COMPILERS=
[[ ! -z $CC ]] && CUSTOM_COMPILERS="$CUSTOM_COMPILERS cc=\"$CC\""
[[ ! -z $CXX ]] && CUSTOM_COMPILERS="$CUSTOM_COMPILERS cxx=\"$CXX\""
[[ ! -z $AR ]] && CUSTOM_COMPILERS="$CUSTOM_COMPILERS ar=\"$AR\""

# gn
(cd externals/skia && ./bin/gn gen "out/Debug" --args="$CUSTOM_COMPILERS")

# ninja
./externals/depot_tools/ninja 'dm' -C "externals/skia/out/Debug"
