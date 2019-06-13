#!/usr/bin/env bash

echo "Starting package validation..."

rm -rf temp-tests
mkdir -p temp-tests

for f in $(find ./output/nugets -iname 'SkiaSharp.NativeAssets.Linux.*.nupkg'); do

    if [[ $f =~ "NoDependencies" ]]; then
        echo "Validating no-dependencies package $f..."
        unzip -joq $f runtimes/linux-x64/native/libSkiaSharp.so -d temp-tests
        ldd temp-tests/libSkiaSharp.so | grep -q fontconfig
        if [[ $? == 0 ]]; then
            echo "Package DID contain a dependency on fontconfig."
            exit 1
        fi
    else
        echo "Validating normal package $f..."
        unzip -joq $f runtimes/linux-x64/native/libSkiaSharp.so -d temp-tests
        ldd temp-tests/libSkiaSharp.so | grep -q fontconfig
        if [[ $? != 0 ]]; then
            echo "Package DID NOT contain a dependency on fontconfig."
            exit 1
        fi
    fi

done

rm -rf temp-tests

echo "All packages are valid."
