
include $(CLEAR_VARS)

cmd-strip = $(PRIVATE_STRIP) --strip-all $(call host-path,$1)

# LOCAL_LDLIBS           := -llog -lEGL -lGLESv2

# LOCAL_LDFLAGS          := -s -Wl,--gc-sections

LOCAL_MODULE           := HarfBuzzSharp

LOCAL_C_INCLUDES       := .                                    \
                          ../../externals/harfbuzz/src/hb-ucdn \
                          ../../externals/harfbuzz/src         \
                          ../../externals/harfbuzz

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

LOCAL_CFLAGS           := -DHAVE_CONFIG_H -NDEBUG

LOCAL_SRC_FILES        := ../../../externals/harfbuzz/src/hb-ucdn/ucdn.c                            \
                          ../../../externals/harfbuzz/src/hb-ucdn.cc                                \
                          ../../../externals/harfbuzz/src/hb-aat-layout.cc                          \
                          ../../../externals/harfbuzz/src/hb-aat-map.cc                             \
                          ../../../externals/harfbuzz/src/hb-blob.cc                                \
                          ../../../externals/harfbuzz/src/hb-buffer-serialize.cc                    \
                          ../../../externals/harfbuzz/src/hb-buffer.cc                              \
                          ../../../externals/harfbuzz/src/hb-common.cc                              \
                          ../../../externals/harfbuzz/src/hb-face.cc                                \
                          ../../../externals/harfbuzz/src/hb-font.cc                                \
                          ../../../externals/harfbuzz/src/hb-map.cc                                 \
                          ../../../externals/harfbuzz/src/hb-ot-cff1-table.cc                       \
                          ../../../externals/harfbuzz/src/hb-ot-cff2-table.cc                       \
                          ../../../externals/harfbuzz/src/hb-ot-color.cc                            \
                          ../../../externals/harfbuzz/src/hb-ot-face.cc                             \
                          ../../../externals/harfbuzz/src/hb-ot-font.cc                             \
                          ../../../externals/harfbuzz/src/hb-ot-layout.cc                           \
                          ../../../externals/harfbuzz/src/hb-ot-map.cc                              \
                          ../../../externals/harfbuzz/src/hb-ot-math.cc                             \
                          ../../../externals/harfbuzz/src/hb-ot-name-language.cc                    \
                          ../../../externals/harfbuzz/src/hb-ot-name.cc                             \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-arabic.cc             \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-default.cc            \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-hangul.cc             \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-hebrew.cc             \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-indic-table.cc        \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-indic.cc              \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-khmer.cc              \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-myanmar.cc            \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-thai.cc               \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-use-table.cc          \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-use.cc                \
                          ../../../externals/harfbuzz/src/hb-ot-shape-complex-vowel-constraints.cc  \
                          ../../../externals/harfbuzz/src/hb-ot-shape-fallback.cc                   \
                          ../../../externals/harfbuzz/src/hb-ot-shape-normalize.cc                  \
                          ../../../externals/harfbuzz/src/hb-ot-shape.cc                            \
                          ../../../externals/harfbuzz/src/hb-ot-tag.cc                              \
                          ../../../externals/harfbuzz/src/hb-ot-var.cc                              \
                          ../../../externals/harfbuzz/src/hb-set.cc                                 \
                          ../../../externals/harfbuzz/src/hb-shape-plan.cc                          \
                          ../../../externals/harfbuzz/src/hb-shape.cc                               \
                          ../../../externals/harfbuzz/src/hb-shaper.cc                              \
                          ../../../externals/harfbuzz/src/hb-static.cc                              \
                          ../../../externals/harfbuzz/src/hb-unicode.cc                             \
                          ../../../externals/harfbuzz/src/hb-warning.cc                             \
                          ../../../externals/harfbuzz/src/hb-fallback-shape.cc

include $(BUILD_SHARED_LIBRARY)
