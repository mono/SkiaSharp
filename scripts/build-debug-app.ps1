$errorActionPreference = 'Stop'

try {
    cd externals/skia

    # git-sync-deps
    python tools/git-sync-deps

    # gn
    ./bin/gn gen "out/Debug"

    # ninja
    ../../externals/depot_tools/ninja "dm" -C "out/Debug"
} finally {
    cd ../../
}

exit $LASTEXITCODE
