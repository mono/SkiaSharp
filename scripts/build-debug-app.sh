#!/usr/bin/env bash

# git-sync-deps
(cd externals/skia && python tools/git-sync-deps)

# gn
(cd externals/skia && ./bin/gn gen "out/Debug")

# ninja
./externals/depot_tools/ninja 'dm' -C "externals/skia/out/Debug"
