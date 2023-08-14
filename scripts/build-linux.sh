#!/usr/bin/env bash
set -e

########################################
# 0. assumptions

#
#  * We assume our working directory is the root of SkiaSharp repository
#
#  * We assume the tools are installed,
#    see https://github.com/mono/SkiaSharp/wiki/Building-on-Linux
#


########################################
# 1. set variables for convenience

SONAME=$(grep "libSkiaSharp\W*soname\W*" scripts/VERSIONS.txt | sed 's/^libSkiaSharp\W*soname\W*\([\.0-9]*\).*$/\1/')
SKIA_ROOT=externals/skia
ARCH=x64

# output folders
[[ -z $PLATFORM_DIR ]] && PLATFORM_DIR="linux"
BUILD_OUT=out/$PLATFORM_DIR/$ARCH
ROOT_OUT=output/native/$PLATFORM_DIR/$ARCH

# compiler options
CUSTOM_COMPILERS=
[[ ! -z $CC ]] && CUSTOM_COMPILERS="$CUSTOM_COMPILERS cc=\"$CC\""
[[ ! -z $CXX ]] && CUSTOM_COMPILERS="$CUSTOM_COMPILERS cxx=\"$CXX\""
[[ ! -z $AR ]] && CUSTOM_COMPILERS="$CUSTOM_COMPILERS ar=\"$AR\""

########################################
# 2. sync dependencies

# git-sync-deps
(cd $SKIA_ROOT && python tools/git-sync-deps)


########################################
# 3. build libSkiaSharp.so

# gn
(cd $SKIA_ROOT && ./bin/gn gen "$BUILD_OUT" --args="
    is_official_build=true skia_enable_tools=false
    target_os=\"linux\" target_cpu=\"$ARCH\"
    skia_use_icu=false skia_use_sfntly=false skia_use_piex=true
    skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
    skia_enable_ganesh=true
    extra_cflags=[ \"-DSKIA_C_DLL\" ]
    extra_ldflags=[ \"-static-libstdc++\", \"-static-libgcc\" ]
    $CUSTOM_COMPILERS
    linux_soname_version=\"$SONAME\"")

# ninja
./externals/depot_tools/ninja 'SkiaSharp' -C "$SKIA_ROOT/$BUILD_OUT"


########################################
# 4. copy output

# copy inside skia
cp $SKIA_ROOT/$BUILD_OUT/libSkiaSharp.so.$SONAME $SKIA_ROOT/$BUILD_OUT/libSkiaSharp.so

# copy to root output
mkdir -p $ROOT_OUT
cp $SKIA_ROOT/$BUILD_OUT/libSkiaSharp.so.$SONAME $ROOT_OUT/libSkiaSharp.so.$SONAME
cp $SKIA_ROOT/$BUILD_OUT/libSkiaSharp.so.$SONAME $ROOT_OUT/libSkiaSharp.so
