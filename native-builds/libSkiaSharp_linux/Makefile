ARCH ?= x64
VERSION ?= 1.0.0.0
SUPPORT_GPU ?= 1
CC ?= gcc
CXX ?= g++
LDFLAGS ?= -lpthread -ldl -lfontconfig

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
	../../externals/skia/out/linux/${ARCH}
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
	../../externals/skia/include/ports   \
	../../externals/skia/include/images  \
	../../externals/skia/include/svg     \
	../../externals/skia/include/xml
library_paths = \
	../../externals/skia/out/linux/${ARCH}/libskia.a
defines = \
	-DSK_INTERNAL -DSK_GAMMA_APPLY_TO_A8 -DQT_NO_KEYWORDS                   \
	-DSK_ALLOW_STATIC_GLOBAL_INITIALIZERS=1 -DSK_SUPPORT_GPU=${SUPPORT_GPU} \
	-DSK_FORCE_DISTANCE_FIELD_TEXT=0 -DSK_HAS_JPEG_LIBRARY                  \
	-DSK_HAS_PNG_LIBRARY -DSK_HAS_WEBP_LIBRARY -DSKIA_C_DLL                 \
	-DSKIA_IMPLEMENTATION=1 -DSK_CODEC_DECODES_RAW -DSK_SAMPLES_FOR_X       \
	-DSK_BUILD_FOR_UNIX -DNDEBUG
cflags = \
	-msse2 -mfpmath=sse -g -fno-exceptions -fstrict-aliasing -Wall \
	-Wextra -Winit-self -Wpointer-arith -Wsign-compare -Wvla       \
	-Wno-unused-parameter -Werror -Os                              \
	-fPIC -fdata-sections -ffunction-sections
ifeq "${ARCH}" "x86"
  arch_cflags = -m32
else
  arch_cflags = 
endif
cflags_c = ${cflags} ${CFLAGS}
cflags_cc = -std=c++11 -fno-rtti -fno-threadsafe-statics -Wnon-virtual-dtor ${CXXFLAGS}
ldflags = $(library_dirs:%=-L%) ${LDFLAGS}
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
	$(CXX) -MMD -MF $@.d                                                 \
		${defines} ${includes} ${arch_cflags} ${cflags_c} ${cflags_cc}   \
		-c $< -o $@

${target}: ${objs}
# link with skia
	mkdir -p $(dir $@)
	$(CXX) -shared -rdynamic -s -o $@                                             \
		${defines} ${includes} ${arch_cflags} ${cflags_c} ${cflags_cc}            \
		-Wl,--start-group ${objarch_root}/*.o  ${library_paths} -Wl,--end-group   \
		${ldflags} -Wl,--gc-sections -Wl,--no-undefined                           \
		-Wl,-soname,libSkiaSharp.so.${soname_version}

all: ${target}

clean:
	rm -rf ${obj_root} ${bin_root}
