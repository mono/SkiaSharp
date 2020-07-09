#!/usr/bin/env bash

DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

mkdir -p $DIR/bin
csc /out:$DIR/bin/Program.exe /unsafe $DIR/../source/Program.cs
cp $DIR/../../../output/native/linux/x64/libSkiaSharp.so $DIR/bin/

(cd $DIR/bin && mono Program.exe)
