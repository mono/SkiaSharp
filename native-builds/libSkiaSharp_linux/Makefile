arch = x64

src_root = ../src
obj_root = obj
bin_root = bin
objarch_root = ${obj_root}/${arch}
target_name = libSkiaSharp.so
target = ${bin_root}/${arch}/${target_name}

library_dirs = \
    ../../externals/skia/out/${arch}/Release         \
    ../../externals/skia/out/${arch}/Release/obj/gyp
include_dirs = \
    ../../externals/skia/src/c           \
    ../../externals/skia/include/c       \
    ../../externals/skia/include/core    \
    ../../externals/skia/include/codec   \
    ../../externals/skia/include/effects \
    ../../externals/skia/include/pathops \
    ../../externals/skia/include/gpu     \
    ../../externals/skia/include/config  \
    ../../externals/skia/include/utils   \
    ../../externals/skia/include/images  \
    ../../externals/skia/include/svg     \
    ../../externals/skia/include/xml
library_paths = \
    ../../externals/skia/out/${arch}/Release/libskia_core.a                     \
    ../../externals/skia/out/${arch}/Release/libskia_effects.a                  \
    ../../externals/skia/out/${arch}/Release/libskia_skgpu.a                    \
    ../../externals/skia/out/${arch}/Release/libskia_opts.a                     \
    ../../externals/skia/out/${arch}/Release/libskia_opts_sse41.a               \
    ../../externals/skia/out/${arch}/Release/libskia_opts_sse42.a               \
    ../../externals/skia/out/${arch}/Release/libskia_opts_ssse3.a               \
    ../../externals/skia/out/${arch}/Release/libskia_opts_avx.a                 \
    ../../externals/skia/out/${arch}/Release/libskia_opts_hsw.a                 \
    ../../externals/skia/out/${arch}/Release/libskia_utils.a                    \
    ../../externals/skia/out/${arch}/Release/libskia_ports.a                    \
    ../../externals/skia/out/${arch}/Release/libskia_images.a                   \
    ../../externals/skia/out/${arch}/Release/libsksl.a                          \
    ../../externals/skia/out/${arch}/Release/libskia_pdf.a                      \
    ../../externals/skia/out/${arch}/Release/libskia_codec.a                    \
    ../../externals/skia/out/${arch}/Release/libskia_codec_android.a            \
    ../../externals/skia/out/${arch}/Release/libpng_static.a                    \
    ../../externals/skia/out/${arch}/Release/libskia_sfnt.a                     \
    ../../externals/skia/out/${arch}/Release/libskia_svg.a                      \
    ../../externals/skia/out/${arch}/Release/libskia_xml.a                      \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libwebp_enc.a              \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libwebp_dec.a              \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libwebp_dsp.a              \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libwebp_utils.a            \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libwebp_demux.a            \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libwebp_dsp_enc.a          \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libjpeg-turbo.a            \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libzlib.a                  \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libetc1.a                  \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libsfntly.a                \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libicuuc.a                 \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libraw_codec.a             \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libSkKTX.a                 \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libdng_sdk.a               \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libpiex.a                  \
    ../../externals/skia/out/${arch}/Release/obj/gyp/libexpat_static.a          \
# ../../externals/skia/out/${arch}/Release/obj/gyp/libskia_opts_crc32.a       \
# -lfontconfig -lfreetype


defines = \
    -DSK_INTERNAL -DSK_GAMMA_APPLY_TO_A8 -DQT_NO_KEYWORDS             \
    -DSK_ALLOW_STATIC_GLOBAL_INITIALIZERS=1 -DSK_SUPPORT_GPU=1        \
    -DSK_FORCE_DISTANCE_FIELD_TEXT=0 -DSK_HAS_JPEG_LIBRARY            \
    -DSK_HAS_PNG_LIBRARY -DSK_HAS_WEBP_LIBRARY -DSKIA_C_DLL           \
    -DSKIA_IMPLEMENTATION=1 -DSK_CODEC_DECODES_RAW -DSK_SAMPLES_FOR_X \
    -DSK_BUILD_FOR_UNIX -DNDEBUG
cflags = \
    -msse2 -mfpmath=sse -g -fno-exceptions -fstrict-aliasing -Wall \
    -Wextra -Winit-self -Wpointer-arith -Wsign-compare -Wvla       \
    -Wno-unused-parameter -Werror -Os                         \
    -fPIC -fdata-sections -ffunction-sections
cflags_c = 
cflags_cc = -std=c++11 -fno-rtti -fno-threadsafe-statics -Wnon-virtual-dtor
ldflags = $(library_dirs:%=-L%) -lpthread -ldl -lfontconfig -lfreetype -lGL -lGLU -lX11
includes = $(include_dirs:%=-I%)
library_names = $(notdir ${library_paths})
libraries = $(library_names:lib%.a=-l%)

src = $(shell find ${src_root} -name *.cpp -or -name *.c)
src_names = $(notdir ${src})
objs = $(src_names:%=${objarch_root}/%.o)
deps = $(objs:.o=.d)

${objarch_root}/%.o: ${src_root}/%
# build the local source
	mkdir -p $(dir $@)
	c++ -MMD -MF $@.d ${defines} ${includes} ${cflags} ${cflags_cc} -c $< -o $@

${target}: ${objs}

# $(foreach path, $(abspath ${library_paths}), \
# 	$(shell mkdir -p ${objarch_root}/merge/$(notdir $(basename ${path})) && \
# 	cd ${objarch_root}/merge/$(notdir $(basename ${path})) && \
# 	ar x ${path}))

# link with skia
	mkdir -p $(dir $@)
	c++ -shared -rdynamic -o $@ \
		${defines} ${includes} ${cflags} ${cflags_cc} \
		-Wl,--start-group  ${objarch_root}/*.o  ${library_paths}  -Wl,--end-group \
		${ldflags} -Wl,--gc-sections -Wl,--no-undefined 

#	cd ${objarch_root} && ar r libSkiaSharp_merged.a merge/*/*.o
#	rm ${objarch_root}/merge/*/*.o

#c++ -rdynamic -m32 -pie -o SampleApp -Wl,--start-group 
#  obj/gm/SampleApp.gm.o obj/samplecode/SampleApp.SampleApp.o obj/samplecode/SampleApp.GMSampleView.o obj/samplecode/SampleApp.ClockFaceView.o obj/samplecode/SampleApp.OverView.o obj/samplecode/SampleApp.PerlinPatch.o obj/samplecode/SampleApp.Sample2PtRadial.o obj/samplecode/SampleApp.SampleAAClip.o obj/samplecode/SampleApp.SampleAAGeometry.o obj/samplecode/SampleApp.SampleAARects.o obj/samplecode/SampleApp.SampleAARectModes.o obj/samplecode/SampleApp.SampleAll.o obj/samplecode/SampleApp.SampleAndroidShadows.o obj/samplecode/SampleApp.SampleAnimatedText.o obj/samplecode/SampleApp.SampleAnimBlur.o obj/samplecode/SampleApp.SampleArc.o obj/samplecode/SampleApp.SampleAtlas.o obj/samplecode/SampleApp.SampleBevel.o obj/samplecode/SampleApp.SampleBigBlur.o obj/samplecode/SampleApp.SampleBigGradient.o obj/samplecode/SampleApp.SampleBitmapRect.o obj/samplecode/SampleApp.SampleBlur.o obj/samplecode/SampleApp.SampleCamera.o obj/samplecode/SampleApp.SampleChart.o obj/samplecode/SampleApp.SampleCircle.o obj/samplecode/SampleApp.SampleClip.o obj/samplecode/SampleApp.SampleClipDrawMatch.o obj/samplecode/SampleApp.SampleClock.o obj/samplecode/SampleApp.SampleCode.o obj/samplecode/SampleApp.SampleColorFilter.o obj/samplecode/SampleApp.SampleComplexClip.o obj/samplecode/SampleApp.SampleConcavePaths.o obj/samplecode/SampleApp.SampleDegenerateTwoPtRadials.o obj/samplecode/SampleApp.SampleDither.o obj/samplecode/SampleApp.SampleDitherBitmap.o obj/samplecode/SampleApp.SampleEffects.o obj/samplecode/SampleApp.SampleEmboss.o obj/samplecode/SampleApp.SampleFatBits.o obj/samplecode/SampleApp.SampleFillType.o obj/samplecode/SampleApp.SampleFilter.o obj/samplecode/SampleApp.SampleFilter2.o obj/samplecode/SampleApp.SampleFilterQuality.o obj/samplecode/SampleApp.SampleFilterFuzz.o obj/samplecode/SampleApp.SampleFontScalerTest.o obj/samplecode/SampleApp.SampleFuzz.o obj/samplecode/SampleApp.SampleGradients.o obj/samplecode/SampleApp.SampleHairCurves.o obj/samplecode/SampleApp.SampleHairline.o obj/samplecode/SampleApp.SampleHairModes.o obj/samplecode/SampleApp.SampleHT.o obj/samplecode/SampleApp.SampleIdentityScale.o obj/samplecode/SampleApp.SampleLayerMask.o obj/samplecode/SampleApp.SampleLayers.o obj/samplecode/SampleApp.SampleLCD.o obj/samplecode/SampleApp.SampleLighting.o obj/samplecode/SampleApp.SampleLines.o obj/samplecode/SampleApp.SampleLitAtlas.o obj/samplecode/SampleApp.SampleLua.o obj/samplecode/SampleApp.SampleManyRects.o obj/samplecode/SampleApp.SampleMeasure.o obj/samplecode/SampleApp.SampleMegaStroke.o obj/samplecode/SampleApp.SamplePatch.o obj/samplecode/SampleApp.SamplePath.o obj/samplecode/SampleApp.SamplePathClip.o obj/samplecode/SampleApp.SamplePathFuzz.o obj/samplecode/SampleApp.SamplePathEffects.o obj/samplecode/SampleApp.SamplePathOverstroke.o obj/samplecode/SampleApp.SamplePictFile.o obj/samplecode/SampleApp.SamplePoints.o obj/samplecode/SampleApp.SamplePolyToPoly.o obj/samplecode/SampleApp.SampleQuadStroker.o obj/samplecode/SampleApp.SampleRectanizer.o obj/samplecode/SampleApp.SampleRegion.o obj/samplecode/SampleApp.SampleRepeatTile.o obj/samplecode/SampleApp.SampleShaders.o obj/samplecode/SampleApp.SampleShaderText.o obj/samplecode/SampleApp.SampleShadowing.o obj/samplecode/SampleApp.SampleShip.o obj/samplecode/SampleApp.SampleSlides.o obj/samplecode/SampleApp.SampleStringArt.o obj/samplecode/SampleApp.SampleStrokePath.o obj/samplecode/SampleApp.SampleSubpixelTranslate.o obj/samplecode/SampleApp.SampleSVGPong.o obj/samplecode/SampleApp.SampleSVGFile.o obj/samplecode/SampleApp.SampleText.o obj/samplecode/SampleApp.SampleTextAlpha.o obj/samplecode/SampleApp.SampleTextBox.o obj/samplecode/SampleApp.SampleTextOnPath.o obj/samplecode/SampleApp.SampleTextureDomain.o obj/samplecode/SampleApp.SampleTiling.o obj/samplecode/SampleApp.SampleTinyBitmap.o obj/samplecode/SampleApp.SampleUnpremul.o obj/samplecode/SampleApp.SampleVertices.o obj/samplecode/SampleApp.SampleXfermodesBlur.o obj/samplecode/SampleApp.SampleXfer.o obj/experimental/SkPerlinNoiseShader2/SampleApp.SkPerlinNoiseShader2.o obj/src/utils/SampleApp.SkLuaCanvas.o obj/src/utils/SampleApp.SkLua.o obj/gm/SampleApp.OverStroke.o obj/gm/SampleApp.SkLinearBitmapPipelineGM.o obj/gm/SampleApp.aaa.o obj/gm/SampleApp.aaclip.o obj/gm/SampleApp.aarectmodes.o obj/gm/SampleApp.aaxfermodes.o obj/gm/SampleApp.addarc.o obj/gm/SampleApp.all_bitmap_configs.o obj/gm/SampleApp.alphagradients.o obj/gm/SampleApp.animatedGif.o obj/gm/SampleApp.anisotropic.o obj/gm/SampleApp.annotated_text.o obj/gm/SampleApp.arcofzorro.o obj/gm/SampleApp.arcto.o obj/gm/SampleApp.arithmode.o obj/gm/SampleApp.badpaint.o obj/gm/SampleApp.beziereffects.o obj/gm/SampleApp.beziers.o obj/gm/SampleApp.bigblurs.o obj/gm/SampleApp.bigmatrix.o obj/gm/SampleApp.bigrrectaaeffect.o obj/gm/SampleApp.bigtext.o obj/gm/SampleApp.bigtileimagefilter.o obj/gm/SampleApp.bitmapcopy.o obj/gm/SampleApp.bitmapfilters.o obj/gm/SampleApp.bitmapimage.o obj/gm/SampleApp.bitmappremul.o obj/gm/SampleApp.bitmaprect.o obj/gm/SampleApp.bitmaprecttest.o obj/gm/SampleApp.bitmapshader.o obj/gm/SampleApp.bleed.o obj/gm/SampleApp.blend.o obj/gm/SampleApp.blurcircles.o obj/gm/SampleApp.blurcircles2.o obj/gm/SampleApp.blurquickreject.o obj/gm/SampleApp.blurrect.o obj/gm/SampleApp.blurredclippedcircle.o obj/gm/SampleApp.blurroundrect.o obj/gm/SampleApp.blurs.o obj/gm/SampleApp.bmpfilterqualityrepeat.o obj/gm/SampleApp.bug5252.o obj/gm/SampleApp.bug530095.o obj/gm/SampleApp.bug615686.o obj/gm/SampleApp.cgm.o obj/gm/SampleApp.cgms.o obj/gm/SampleApp.circles.o obj/gm/SampleApp.circulararcs.o obj/gm/SampleApp.circularclips.o obj/gm/SampleApp.clip_error.o obj/gm/SampleApp.clip_strokerect.o obj/gm/SampleApp.clipdrawdraw.o obj/gm/SampleApp.clippedbitmapshaders.o obj/gm/SampleApp.color4f.o obj/gm/SampleApp.colorcube.o obj/gm/SampleApp.coloremoji.o obj/gm/SampleApp.colorfilteralpha8.o obj/gm/SampleApp.colorfilterimagefilter.o obj/gm/SampleApp.colorfilters.o obj/gm/SampleApp.colormatrix.o obj/gm/SampleApp.colorspacexform.o obj/gm/SampleApp.colortype.o obj/gm/SampleApp.colortypexfermode.o obj/gm/SampleApp.colorwheel.o obj/gm/SampleApp.complexclip.o obj/gm/SampleApp.complexclip2.o obj/gm/SampleApp.complexclip3.o obj/gm/SampleApp.complexclip_blur_tiled.o obj/gm/SampleApp.composeshader.o obj/gm/SampleApp.concavepaths.o obj/gm/SampleApp.conicpaths.o obj/gm/SampleApp.constcolorprocessor.o obj/gm/SampleApp.convex_all_line_paths.o obj/gm/SampleApp.convexpaths.o obj/gm/SampleApp.convexpolyclip.o obj/gm/SampleApp.convexpolyeffect.o obj/gm/SampleApp.copyTo4444.o obj/gm/SampleApp.croppedrects.o obj/gm/SampleApp.cubicpaths.o obj/gm/SampleApp.dashcircle.o obj/gm/SampleApp.dashcubics.o obj/gm/SampleApp.dashing.o obj/gm/SampleApp.deferredtextureimage.o obj/gm/SampleApp.degeneratesegments.o obj/gm/SampleApp.dftext.o obj/gm/SampleApp.discard.o obj/gm/SampleApp.displacement.o obj/gm/SampleApp.distantclip.o obj/gm/SampleApp.downsamplebitmap.o obj/gm/SampleApp.draw_bitmap_rect_skbug4374.o obj/gm/SampleApp.drawable.o obj/gm/SampleApp.drawatlas.o obj/gm/SampleApp.drawatlascolor.o obj/gm/SampleApp.drawbitmaprect.o obj/gm/SampleApp.drawfilter.o obj/gm/SampleApp.drawlooper.o obj/gm/SampleApp.drawminibitmaprect.o obj/gm/SampleApp.drawregion.o obj/gm/SampleApp.drawregionmodes.o obj/gm/SampleApp.dropshadowimagefilter.o obj/gm/SampleApp.drrect.o obj/gm/SampleApp.dstreadshuffle.o obj/gm/SampleApp.emboss.o obj/gm/SampleApp.emptypath.o 
#  obj/gm/SampleApp.encode-platform.o obj/gm/SampleApp.encode.o obj/gm/SampleApp.extractbitmap.o obj/gm/SampleApp.fadefilter.o obj/gm/SampleApp.fatpathfill.o obj/gm/SampleApp.filltypes.o obj/gm/SampleApp.filltypespersp.o obj/gm/SampleApp.filterbitmap.o obj/gm/SampleApp.filterfastbounds.o obj/gm/SampleApp.filterindiabox.o obj/gm/SampleApp.fontcache.o obj/gm/SampleApp.fontmgr.o obj/gm/SampleApp.fontscaler.o obj/gm/SampleApp.fontscalerdistortable.o obj/gm/SampleApp.gamma.o obj/gm/SampleApp.gammacolorfilter.o obj/gm/SampleApp.gammatext.o obj/gm/SampleApp.gamut.o obj/gm/SampleApp.gaussianedge.o obj/gm/SampleApp.getpostextpath.o obj/gm/SampleApp.giantbitmap.o obj/gm/SampleApp.glyph_pos.o obj/gm/SampleApp.glyph_pos_align.o obj/gm/SampleApp.gradientDirtyLaundry.o obj/gm/SampleApp.gradient_matrix.o obj/gm/SampleApp.gradients.o obj/gm/SampleApp.gradients_2pt_conical.o obj/gm/SampleApp.gradients_no_texture.o obj/gm/SampleApp.gradtext.o obj/gm/SampleApp.grayscalejpg.o obj/gm/SampleApp.hairlines.o obj/gm/SampleApp.hairmodes.o obj/gm/SampleApp.hardstop_gradients.o obj/gm/SampleApp.hittestpath.o obj/gm/SampleApp.image.o obj/gm/SampleApp.image_pict.o obj/gm/SampleApp.image_shader.o obj/gm/SampleApp.imagealphathreshold.o obj/gm/SampleApp.imageblur.o obj/gm/SampleApp.imageblur2.o obj/gm/SampleApp.imageblurtiled.o obj/gm/SampleApp.imagefilters.o obj/gm/SampleApp.imagefiltersbase.o obj/gm/SampleApp.imagefiltersclipped.o obj/gm/SampleApp.imagefilterscropexpand.o obj/gm/SampleApp.imagefilterscropped.o obj/gm/SampleApp.imagefiltersgraph.o obj/gm/SampleApp.imagefiltersscaled.o obj/gm/SampleApp.imagefiltersstroked.o obj/gm/SampleApp.imagefilterstransformed.o obj/gm/SampleApp.imagefromyuvtextures.o obj/gm/SampleApp.imagegeneratorexternal.o obj/gm/SampleApp.imagemagnifier.o obj/gm/SampleApp.imagemakewithfilter.o obj/gm/SampleApp.imagemasksubset.o obj/gm/SampleApp.imageresizetiled.o obj/gm/SampleApp.imagescalealigned.o obj/gm/SampleApp.imagesource.o obj/gm/SampleApp.imagesource2.o obj/gm/SampleApp.imagetoyuvplanes.o obj/gm/SampleApp.internal_links.o obj/gm/SampleApp.inversepaths.o obj/gm/SampleApp.labpcsdemo.o obj/gm/SampleApp.largeglyphblur.o obj/gm/SampleApp.lattice.o obj/gm/SampleApp.lcdblendmodes.o obj/gm/SampleApp.lcdoverlap.o obj/gm/SampleApp.lcdtext.o obj/gm/SampleApp.lighting.o obj/gm/SampleApp.lightingshader.o obj/gm/SampleApp.lightingshader2.o obj/gm/SampleApp.lightingshaderbevel.o obj/gm/SampleApp.linepaths.o obj/gm/SampleApp.localmatriximagefilter.o obj/gm/SampleApp.lumafilter.o obj/gm/SampleApp.matrixconvolution.o obj/gm/SampleApp.matriximagefilter.o obj/gm/SampleApp.megalooper.o obj/gm/SampleApp.mipmap.o obj/gm/SampleApp.mixedtextblobs.o obj/gm/SampleApp.modecolorfilters.o obj/gm/SampleApp.morphology.o obj/gm/SampleApp.multipicturedraw.o obj/gm/SampleApp.nested.o obj/gm/SampleApp.ninepatchstretch.o obj/gm/SampleApp.nonclosedpaths.o obj/gm/SampleApp.occludedrrectblur.o obj/gm/SampleApp.offsetimagefilter.o obj/gm/SampleApp.ovals.o obj/gm/SampleApp.patch.o obj/gm/SampleApp.patchgrid.o obj/gm/SampleApp.path_stroke_with_zero_length.o obj/gm/SampleApp.pathcontourstart.o obj/gm/SampleApp.patheffects.o obj/gm/SampleApp.pathfill.o obj/gm/SampleApp.pathinterior.o obj/gm/SampleApp.pathmaskcache.o obj/gm/SampleApp.pathopsinverse.o obj/gm/SampleApp.pathopsskpclip.o obj/gm/SampleApp.pathreverse.o obj/gm/SampleApp.pdf_never_embed.o obj/gm/SampleApp.perlinnoise.o obj/gm/SampleApp.perspshaders.o obj/gm/SampleApp.picture.o obj/gm/SampleApp.pictureimagefilter.o obj/gm/SampleApp.pictureimagegenerator.o obj/gm/SampleApp.pictureshader.o obj/gm/SampleApp.pictureshadertile.o obj/gm/SampleApp.pixelsnap.o obj/gm/SampleApp.plus.o obj/gm/SampleApp.points.o obj/gm/SampleApp.poly2poly.o obj/gm/SampleApp.polygons.o obj/gm/SampleApp.quadpaths.o obj/gm/SampleApp.recordopts.o obj/gm/SampleApp.rectangletexture.o obj/gm/SampleApp.rects.o obj/gm/SampleApp.repeated_bitmap.o obj/gm/SampleApp.resizeimagefilter.o obj/gm/SampleApp.reveal.o obj/gm/SampleApp.roundrects.o obj/gm/SampleApp.rrect.o obj/gm/SampleApp.rrectclipdrawpaint.o obj/gm/SampleApp.rrects.o obj/gm/SampleApp.samplerstress.o obj/gm/SampleApp.scaledstrokes.o obj/gm/SampleApp.shaderbounds.o obj/gm/SampleApp.shadertext.o obj/gm/SampleApp.shadertext2.o obj/gm/SampleApp.shadertext3.o obj/gm/SampleApp.shadowmaps.o obj/gm/SampleApp.shadows.o obj/gm/SampleApp.shallowgradient.o obj/gm/SampleApp.shapes.o obj/gm/SampleApp.showmiplevels.o obj/gm/SampleApp.simpleaaclip.o obj/gm/SampleApp.simplerect.o obj/gm/SampleApp.skbug1719.o obj/gm/SampleApp.skbug_257.o obj/gm/SampleApp.skbug_4868.o obj/gm/SampleApp.skbug_5321.o obj/gm/SampleApp.smallarc.o obj/gm/SampleApp.smallimage.o obj/gm/SampleApp.smallpaths.o obj/gm/SampleApp.spritebitmap.o obj/gm/SampleApp.srcmode.o obj/gm/SampleApp.stlouisarch.o obj/gm/SampleApp.stringart.o obj/gm/SampleApp.stroke_rect_shader.o obj/gm/SampleApp.strokedlines.o obj/gm/SampleApp.strokefill.o obj/gm/SampleApp.strokerect.o obj/gm/SampleApp.strokerects.o obj/gm/SampleApp.strokes.o obj/gm/SampleApp.stroketext.o obj/gm/SampleApp.subsetshader.o obj/gm/SampleApp.surface.o obj/gm/SampleApp.tablecolorfilter.o obj/gm/SampleApp.tallstretchedbitmaps.o obj/gm/SampleApp.texdata.o obj/gm/SampleApp.textblob.o obj/gm/SampleApp.textblobblockreordering.o obj/gm/SampleApp.textblobcolortrans.o obj/gm/SampleApp.textblobgeometrychange.o obj/gm/SampleApp.textbloblooper.o obj/gm/SampleApp.textblobmixedsizes.o obj/gm/SampleApp.textblobrandomfont.o obj/gm/SampleApp.textblobshader.o obj/gm/SampleApp.textblobtransforms.o obj/gm/SampleApp.textblobuseaftergpufree.o obj/gm/SampleApp.texteffects.o obj/gm/SampleApp.texturedomaineffect.o obj/gm/SampleApp.thinrects.o obj/gm/SampleApp.thinstrokedrects.o obj/gm/SampleApp.tiledscaledbitmap.o obj/gm/SampleApp.tileimagefilter.o obj/gm/SampleApp.tilemodes.o obj/gm/SampleApp.tilemodes_scaled.o obj/gm/SampleApp.tinybitmap.o obj/gm/SampleApp.transparency.o obj/gm/SampleApp.typeface.o obj/gm/SampleApp.variedtext.o obj/gm/SampleApp.vertices.o obj/gm/SampleApp.verttext.o obj/gm/SampleApp.verttext2.o obj/gm/SampleApp.verylargebitmap.o obj/gm/SampleApp.windowrectangles.o obj/gm/SampleApp.xfermodeimagefilter.o obj/gm/SampleApp.xfermodes.o obj/gm/SampleApp.xfermodes2.o obj/gm/SampleApp.xfermodes3.o obj/gm/SampleApp.yuvtorgbeffect.o obj/tools/debugger/SampleApp.SkDrawCommand.o obj/tools/debugger/SampleApp.SkDebugCanvas.o obj/tools/debugger/SampleApp.SkJsonWriteBuffer.o obj/tools/debugger/SampleApp.SkObjectParser.o 
#  obj/gyp/libetc1.a obj/gyp/libflags.a obj/gyp/libjsoncpp.a libskia_skgputest.a obj/gyp/libresources.a obj/gyp/libsk_tool_utils.a obj/gyp/libtimer.a obj/gyp/liburl_data_manager.a libskia_views.a obj/gyp/libexperimental.a obj/gyp/liblua.a libskia_pdf.a libsvgdom.a libskia_xml.a obj/gyp/libpicture_utils.a libskia_core.a libskia_codec.a obj/gyp/libjpeg-turbo.a libpng_static.a obj/gyp/libzlib.a obj/gyp/libwebp_dec.a obj/gyp/libwebp_demux.a obj/gyp/libwebp_dsp.a obj/gyp/libwebp_enc.a obj/gyp/libwebp_dsp_enc.a obj/gyp/libwebp_utils.a obj/gyp/libraw_codec.a obj/gyp/libdng_sdk.a obj/gyp/libpiex.a libskia_codec_android.a libskia_effects.a libskia_images.a obj/gyp/libSkKTX.a libskia_utils.a libskia_opts.a libskia_opts_ssse3.a libskia_opts_sse41.a libskia_opts_sse42.a libskia_opts_avx.a libskia_opts_hsw.a libskia_ports.a obj/gyp/libexpat_static.a libskia_sfnt.a libskia_skgpu.a libsksl.a obj/gyp/libflags_common.a obj/gyp/libsfntly.a obj/gyp/libicuuc.a
#  -Wl,--end-group  -lpthread -ldl -lfontconfig -lfreetype -lGL -lGLU -lX11


# cd ${objarch_root} && $(foreach i,$(shell echo ../../externals/skia/out/${arch}/Release/libskia_core.a),$(shell ar x $i))

#	cd ${objarch_root} && ar x ../../../../externals/skia/out/${arch}/Release/libskia_core.a

# rm *.o

# mkdir -p $(dir $@)
# c++ -shared -o ${target} ${defines} ${includes} ${cflags} ${cflags_cc} ${ldflags} -Wl,--start-group ${libraries} -Wl,--end-group $(src) -Wl,--no-undefined 
# -soname=$soname 
# $link_file_list

all: ${target}

clean:
	rm -rf ${obj_root} ${bin_root}
