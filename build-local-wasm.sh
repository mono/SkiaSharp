#!/bin/bash

cd scripts

export BUILD_SOURCESDIRECTORY=/workspace/Uno.SkiaSharp
export BUILD_ARTIFACTSTAGINGDIRECTORY=/workspace/artifacts

./wasm-uitest-run.sh