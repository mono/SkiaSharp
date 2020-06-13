#!/bin/bash

cd $BUILD_SOURCESDIRECTORY/externals/skia

# Disable strict mode, as we don't use the output of CanvasKit and 
# it is not compatible with emscripten 1.39.11 as of m80
sed -i 's/STRICT=1/STRICT=0/g' modules/canvaskit/compile.sh

# Statically link SkiaSharp C helpers
sed -i 's/shared_library("SkiaSharp")/static_library("SkiaSharp")/g' BUILD.gn
sed -i 's/libsksg.a \\/libsksg.a libSkiaSharp.a \\/g' modules/canvaskit/compile.sh

# Restore CPU shaders
sed -i 's/-DSK_DISABLE_LEGACY_SHADERCONTEXT//g' modules/canvaskit/compile.sh
sed -i 's/SK_MaxS32 \* 0.25f/(float)SK_MaxS32 * 0.25f/g' src/shaders/SkImageShader.cpp

modules/canvaskit/compile.sh no_skottie no_managed_skottie no_particles no_canvas primitive_shaper no_paragraph

export ADDITIONAL_OBJ=out/canvaskit_wasm/obj/libAdditional

rm -fR  $ADDITIONAL_OBJ
mkdir -p $ADDITIONAL_OBJ

emcc -std=c++17 -I. modules/canvaskit/fonts/NotoMono-Regular.ttf.cpp -r -o $ADDITIONAL_OBJ/NotoMonoRegularttf.o
emcc -std=c++17 -I. src/xml/SkXMLWriter.cpp -r -o $ADDITIONAL_OBJ/SkXMLWriter.o
emcc -std=c++17 -I. src/svg/SkSVGCanvas.cpp -r -o $ADDITIONAL_OBJ/SkSVGCanvas.o
emcc -std=c++17 -I. src/svg/SkSVGDevice.cpp -r -o $ADDITIONAL_OBJ/SkSVGDevice.o

# Assembly all libaries to out/libSkiaSharp.a

pushd $ADDITIONAL_OBJ
for ARCHIVEFILE in $BUILD_SOURCESDIRECTORY/externals/skia/out/canvaskit_wasm/*.a
do
	ar x $ARCHIVEFILE
done
popd

ar -crs out/libSkiaSharp.a $ADDITIONAL_OBJ/*.o
mv out/libSkiaSharp.a out/canvaskit_wasm/libSkiaSharp.a