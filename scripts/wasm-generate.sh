#!/bin/bash

cd $BUILD_SOURCESDIRECTORY

# Disable strict mode, as we don't use the output of CanvasKit
sed -i 's/STRICT=1/STRICT=0/g' modules/canvaskit/compile.sh

modules/canvaskit/compile.sh

export ADDITIONAL_OBJ=out/canvaskit_wasm/obj/libAdditional

md $ADDITIONAL_OBJ
emcc -std=c++17 -I. modules/canvaskit/fonts/NotoMono-Regular.ttf.cpp -r -o $ADDITIONAL_OBJ/NotoMonoRegularttf.o
emcc -std=c++17 -I. src/xml/SkXMLWriter.cpp -r -o $ADDITIONAL_OBJ/SkXMLWriter.o
emcc -std=c++17 -I. src/svg/SkSVGCanvas.cpp -r -o $ADDITIONAL_OBJ/SkSVGCanvas.o
emcc -std=c++17 -I. src/svg/SkSVGDevice.cpp -r -o $ADDITIONAL_OBJ/SkSVGDevice.o

ar -crs out/canvaskit_wasm/libSkiaSharp.a $ADDITIONAL_OBJ/*.o