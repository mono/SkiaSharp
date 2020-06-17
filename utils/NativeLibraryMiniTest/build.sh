#!/usr/bin/env bash

mkdir -p utils/NativeLibraryMiniTest/bin
csc /out:utils/NativeLibraryMiniTest/bin/Program.exe /unsafe utils/NativeLibraryMiniTest/Program.cs
cp output/native/linux/x64/libSkiaSharp.so utils/NativeLibraryMiniTest/bin/

(cd utils/NativeLibraryMiniTest/bin && mono Program.exe)
