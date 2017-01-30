ARCH = x64
VERSION = 1.0.0.0

noop = 
space = ${noop} ${noop}

version_parts = $(subst ., ,${VERSION})
soname_version = $(word 2, ${version_parts})
file_version = $(subst ${space},.,$(wordlist 2, 4, ${version_parts}))

src_root = ../src
obj_root = obj
bin_root = bin
objarch_root = ${obj_root}/${ARCH}
target_name = libSkiaSharp.so
target = ${bin_root}/${ARCH}/${target_name}.${file_version}

library_dirs = \
	../../externals/skia/out/${ARCH}/Release         \
	../../externals/skia/out/${ARCH}/Release/obj/gyp
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
	../../externals/skia/out/${ARCH}/Release/libskia_core.a                     \
	../../externals/skia/out/${ARCH}/Release/libskia_effects.a                  \
	../../externals/skia/out/${ARCH}/Release/libskia_skgpu.a                    \
	../../externals/skia/out/${ARCH}/Release/libskia_opts.a                     \
	../../externals/skia/out/${ARCH}/Release/libskia_opts_sse41.a               \
	../../externals/skia/out/${ARCH}/Release/libskia_opts_sse42.a               \
	../../externals/skia/out/${ARCH}/Release/libskia_opts_ssse3.a               \
	../../externals/skia/out/${ARCH}/Release/libskia_opts_avx.a                 \
	../../externals/skia/out/${ARCH}/Release/libskia_opts_hsw.a                 \
	../../externals/skia/out/${ARCH}/Release/libskia_utils.a                    \
	../../externals/skia/out/${ARCH}/Release/libskia_ports.a                    \
	../../externals/skia/out/${ARCH}/Release/libskia_images.a                   \
	../../externals/skia/out/${ARCH}/Release/libsksl.a                          \
	../../externals/skia/out/${ARCH}/Release/libskia_pdf.a                      \
	../../externals/skia/out/${ARCH}/Release/libskia_codec.a                    \
	../../externals/skia/out/${ARCH}/Release/libskia_codec_android.a            \
	../../externals/skia/out/${ARCH}/Release/libpng_static.a                    \
	../../externals/skia/out/${ARCH}/Release/libskia_sfnt.a                     \
	../../externals/skia/out/${ARCH}/Release/libskia_svg.a                      \
	../../externals/skia/out/${ARCH}/Release/libskia_xml.a                      \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libwebp_enc.a              \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libwebp_dec.a              \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libwebp_dsp.a              \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libwebp_utils.a            \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libwebp_demux.a            \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libwebp_dsp_enc.a          \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libjpeg-turbo.a            \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libzlib.a                  \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libetc1.a                  \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libraw_codec.a             \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libSkKTX.a                 \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libdng_sdk.a               \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libpiex.a                  \
	../../externals/skia/out/${ARCH}/Release/obj/gyp/libexpat_static.a
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
	-Wno-unused-parameter -Werror -Os                              \
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
# link with skia
	mkdir -p $(dir $@)
	c++ -shared -rdynamic -s -o $@                                                \
		${defines} ${includes} ${cflags} ${cflags_cc}                             \
		-Wl,--start-group ${objarch_root}/*.o  ${library_paths} -Wl,--end-group   \
		${ldflags} -Wl,--gc-sections -Wl,--no-undefined                           \
		-Wl,-soname,libSkiaSharp.so.${soname_version}

all: ${target}

clean:
	rm -rf ${obj_root} ${bin_root}
