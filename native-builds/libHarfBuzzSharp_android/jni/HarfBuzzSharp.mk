
include $(CLEAR_VARS)

cmd-strip = $(PRIVATE_STRIP) --strip-all $(call host-path,$1)

# LOCAL_LDLIBS           := -llog -lEGL -lGLESv2

# LOCAL_LDFLAGS          := -s -Wl,--gc-sections

LOCAL_MODULE           := HarfBuzzSharp

LOCAL_C_INCLUDES       := .                                             \
                          ../../externals/harfbuzz/harfbuzz/src/hb-ucdn \
                          ../../externals/harfbuzz/harfbuzz/src         \
                          ../../externals/harfbuzz/harfbuzz

# LOCAL_CFLAGS           := -DSK_INTERNAL -DSK_GAMMA_APPLY_TO_A8                                   \
#                           -DSK_ALLOW_STATIC_GLOBAL_INITIALIZERS=0 -DSK_SUPPORT_GPU=1             \
#                           -DSK_SUPPORT_OPENCL=0 -DSK_FORCE_DISTANCE_FIELD_TEXT=0                 \
#                           -DSK_BUILD_FOR_ANDROID -DSK_GAMMA_EXPONENT=1.4 -DSK_GAMMA_CONTRAST=0.0 \
#                           -DSKIA_C_DLL -DSKIA_IMPLEMENTATION=1                                   \
#                           -DSK_SUPPORT_LEGACY_CLIPTOLAYERFLAG -DNDEBUG

# LOCAL_CFLAGS           += -fPIC -g -fno-exceptions -fstrict-aliasing -Wall -Wextra               \
#                           -Winit-self -Wpointer-arith -Wsign-compare -Wno-unused-parameter       \
#                           -Werror -fuse-ld=gold                                                  \
#                           -Os -ffunction-sections -fdata-sections -fno-rtti

# LOCAL_CPPFLAGS         := -std=c++11 -fno-rtti -fno-threadsafe-statics -Wnon-virtual-dtor        \
#                           -Os -ffunction-sections -fdata-sections

LOCAL_CFLAGS           := -DHAVE_CONFIG_H

LOCAL_SRC_FILES        := ../../../externals/harfbuzz/harfbuzz/src/hb-ucdn/ucdn.c                      \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-blob.cc                          \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-buffer-serialize.cc              \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-buffer.cc                        \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-common.cc                        \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-face.cc                          \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-fallback-shape.cc                \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-font.cc                          \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-font.cc                       \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-layout.cc                     \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-map.cc                        \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-math.cc                       \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-complex-arabic.cc       \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-complex-default.cc      \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-complex-hangul.cc       \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-complex-hebrew.cc       \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-complex-indic-table.cc  \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-complex-indic.cc        \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-complex-myanmar.cc      \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-complex-thai.cc         \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-complex-tibetan.cc      \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-complex-use-table.cc    \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-complex-use.cc          \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-fallback.cc             \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape-normalize.cc            \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-shape.cc                      \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-tag.cc                        \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ot-var.cc                        \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-set.cc                           \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-shape-plan.cc                    \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-shape.cc                         \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-shaper.cc                        \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-ucdn.cc                          \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-unicode.cc                       \
                          ../../../externals/harfbuzz/harfbuzz/src/hb-warning.cc

include $(BUILD_SHARED_LIBRARY)
