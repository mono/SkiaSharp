
include $(CLEAR_VARS)

cmd-strip = $(PRIVATE_STRIP) --strip-all $(call host-path,$1)

LOCAL_MODULE           := HarfBuzzSharp

LOCAL_C_INCLUDES       := .                                    \
                          ../../externals/harfbuzz/src

LOCAL_LDFLAGS          := -s -Wl,--gc-sections

LOCAL_CFLAGS           := -DHAVE_CONFIG_H -DNDEBUG                                                  \
                          -fno-rtti -fno-exceptions -fno-threadsafe-statics -fPIC                   \
                          -g -Os -ffunction-sections -fdata-sections

LOCAL_CPPFLAGS         := -std=c++11

LOCAL_SRC_FILES        := ../../../externals/harfbuzz/src/hb-aat-layout.cc                          \
                          ../../../externals/harfbuzz/src/hb-aat-map.cc                             \
                          ../../../externals/harfbuzz/src/hb-blob.cc                                \
                          ../../../externals/harfbuzz/src/hb-buffer-serialize.cc                    \
                          ../../../externals/harfbuzz/src/hb-buffer.cc                              \
                          ../../../externals/harfbuzz/src/hb-common.cc                              \
                          ../../../externals/harfbuzz/src/hb-face.cc                                \
                          ../../../externals/harfbuzz/src/hb-fallback-shape.cc                      \
                          ../../../externals/harfbuzz/src/hb-font.cc                                \
                          ../../../externals/harfbuzz/src/hb-map.cc                                 \
                          ../../../externals/harfbuzz/src/hb-number.cc                              \
                          ../../../externals/harfbuzz/src/hb-ot-cff1-table.cc                       \
                          ../../../externals/harfbuzz/src/hb-ot-cff2-table.cc                       \
                          ../../../externals/harfbuzz/src/hb-ot-color.cc                            \
                          ../../../externals/harfbuzz/src/hb-ot-face.cc                             \
                          ../../../externals/harfbuzz/src/hb-ot-font.cc                             \
                          ../../../externals/harfbuzz/src/hb-ot-layout.cc                           \
                          ../../../externals/harfbuzz/src/hb-ot-map.cc                              \
                          ../../../externals/harfbuzz/src/hb-ot-math.cc                             \
                          ../../../externals/harfbuzz/src/hb-ot-meta.cc                             \
                          ../../../externals/harfbuzz/src/hb-ot-metrics.cc                          \
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
                          ../../../externals/harfbuzz/src/hb-subset-cff-common.cc                   \
                          ../../../externals/harfbuzz/src/hb-subset-cff1.cc                         \
                          ../../../externals/harfbuzz/src/hb-subset-cff2.cc                         \
                          ../../../externals/harfbuzz/src/hb-subset-input.cc                        \
                          ../../../externals/harfbuzz/src/hb-subset-plan.cc                         \
                          ../../../externals/harfbuzz/src/hb-subset.cc                              \
                          ../../../externals/harfbuzz/src/hb-ucd.cc                                 \
                          ../../../externals/harfbuzz/src/hb-unicode.cc                             \
                          ../../../externals/harfbuzz/src/hb-warning.cc

include $(BUILD_SHARED_LIBRARY)
