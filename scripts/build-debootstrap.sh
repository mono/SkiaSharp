#!/usr/bin/env bash
set -ex

# create the vars
SCRIPT=$(readlink -f "$0")
BASE_DIR=$(dirname "$SCRIPT")
NEW_ROOT=$(dirname "$BASE_DIR")/externals/debootstrap/armhf
SKIA_ROOT=$(dirname "$BASE_DIR")/externals/skia
BUILD_OUT=$SKIA_ROOT/out/linux/armhf
SONAME=$(grep "libSkiaSharp\W*soname\W*" VERSIONS.txt | sed 's/^libSkiaSharp\W*soname\W*\([\.0-9]*\).*$/\1/')

# git-sync-deps
(cd $SKIA_ROOT && python tools/git-sync-deps)

# gn
(cd $SKIA_ROOT && ./bin/gn gen "$BUILD_OUT" --args="
    is_official_build=true skia_enable_tools=false
    target_os=\"linux\" target_cpu=\"arm\"
    skia_use_icu=false skia_use_sfntly=false skia_use_piex=true
    skia_use_system_expat=false skia_use_system_freetype2=true skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
    skia_enable_gpu=true
    extra_cflags=[
        \"-g\",
        \"--target=armv7a-linux-gnueabihf\",
        \"-mfloat-abi=hard\",
        \"-mfpu=neon\",
        \"--sysroot=$NEW_ROOT\",
        \"-I$NEW_ROOT/usr/include/c++/4.9\",
        \"-I$NEW_ROOT/usr/include/arm-linux-gnueabihf\",
        \"-I$NEW_ROOT/usr/include/arm-linux-gnueabihf/c++/4.9\",
        \"-I$NEW_ROOT/usr/include/freetype2\",
        \"-DSKIA_C_DLL\"
    ]
    extra_asmflags=[
        \"-g\",
        \"--target=armv7a-linux-gnueabihf\",
        \"--sysroot=$NEW_ROOT\",
        \"-march=armv7-a\",
        \"-mfpu=neon\",
        \"-mthumb\"
    ]
    extra_ldflags=[
        \"--sysroot=$NEW_ROOT\",
        \"--target=armv7a-linux-gnueabihf\"
    ]
    cc=\"$NEW_ROOT/usr/bin/gcc\" cxx=\"$NEW_ROOT/usr/bin/g++\" ar=\"$NEW_ROOT/usr/bin/ar\"
    linux_soname_version=\"$SONAME\"")

# ninja
./externals/depot_tools/ninja 'SkiaSharp' -C "$SKIA_ROOT/$BUILD_OUT"
