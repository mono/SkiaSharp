#!/usr/bin/env bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

mkdir -p $DIR/source
cp -R $DIR/../source/* $DIR/source/
cp -R $DIR/NativeLibraryMiniTest.csproj $DIR/source/
cp -R $DIR/nuget.config $DIR/source/
cp -R $DIR/../../../output/native/linux/arm/libSkiaSharp.so $DIR/source/

(cd $DIR && docker build --tag skiasharp/minitest .)
(cd $DIR && docker run --rm skiasharp/minitest)
