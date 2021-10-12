
include $(CLEAR_VARS)

cmd-strip = $(PRIVATE_STRIP) --strip-all $(call host-path,$1)

src_root = ../../../../externals/skia/third_party/externals/harfbuzz/src

LOCAL_MODULE           := HarfBuzzSharp

LOCAL_C_INCLUDES       := . $(src_root)

LOCAL_LDFLAGS          := -s -Wl,--gc-sections

LOCAL_CFLAGS           := -DHAVE_CONFIG_H -DNDEBUG                                                  \
                          -fno-rtti -fno-exceptions -fno-threadsafe-statics -fPIC                   \
                          -g -Os -ffunction-sections -fdata-sections

LOCAL_CPPFLAGS         := -std=c++11

LOCAL_SRC_FILES        := $(src_root)/hb-aat-layout.cc                          \
                          $(src_root)/hb-aat-map.cc                             \
                          $(src_root)/hb-blob.cc                                \
                          $(src_root)/hb-buffer-serialize.cc                    \
                          $(src_root)/hb-buffer.cc                              \
                          $(src_root)/hb-common.cc                              \
                          $(src_root)/hb-draw.cc                                \
                          $(src_root)/hb-face.cc                                \
                          $(src_root)/hb-fallback-shape.cc                      \
                          $(src_root)/hb-font.cc                                \
                          $(src_root)/hb-map.cc                                 \
                          $(src_root)/hb-number.cc                              \
                          $(src_root)/hb-ot-cff1-table.cc                       \
                          $(src_root)/hb-ot-cff2-table.cc                       \
                          $(src_root)/hb-ot-color.cc                            \
                          $(src_root)/hb-ot-face.cc                             \
                          $(src_root)/hb-ot-font.cc                             \
                          $(src_root)/hb-ot-layout.cc                           \
                          $(src_root)/hb-ot-map.cc                              \
                          $(src_root)/hb-ot-math.cc                             \
                          $(src_root)/hb-ot-meta.cc                             \
                          $(src_root)/hb-ot-metrics.cc                          \
                          $(src_root)/hb-ot-name.cc                             \
                          $(src_root)/hb-ot-shape-complex-arabic.cc             \
                          $(src_root)/hb-ot-shape-complex-default.cc            \
                          $(src_root)/hb-ot-shape-complex-hangul.cc             \
                          $(src_root)/hb-ot-shape-complex-hebrew.cc             \
                          $(src_root)/hb-ot-shape-complex-indic-table.cc        \
                          $(src_root)/hb-ot-shape-complex-indic.cc              \
                          $(src_root)/hb-ot-shape-complex-khmer.cc              \
                          $(src_root)/hb-ot-shape-complex-myanmar.cc            \
                          $(src_root)/hb-ot-shape-complex-syllabic.cc           \
                          $(src_root)/hb-ot-shape-complex-thai.cc               \
                          $(src_root)/hb-ot-shape-complex-use.cc                \
                          $(src_root)/hb-ot-shape-complex-vowel-constraints.cc  \
                          $(src_root)/hb-ot-shape-fallback.cc                   \
                          $(src_root)/hb-ot-shape-normalize.cc                  \
                          $(src_root)/hb-ot-shape.cc                            \
                          $(src_root)/hb-ot-tag.cc                              \
                          $(src_root)/hb-ot-var.cc                              \
                          $(src_root)/hb-set.cc                                 \
                          $(src_root)/hb-shape-plan.cc                          \
                          $(src_root)/hb-shape.cc                               \
                          $(src_root)/hb-shaper.cc                              \
                          $(src_root)/hb-static.cc                              \
                          $(src_root)/hb-style.cc                               \
                          $(src_root)/hb-subset-cff-common.cc                   \
                          $(src_root)/hb-subset-cff1.cc                         \
                          $(src_root)/hb-subset-cff2.cc                         \
                          $(src_root)/hb-subset-input.cc                        \
                          $(src_root)/hb-subset-plan.cc                         \
                          $(src_root)/hb-subset.cc                              \
                          $(src_root)/hb-ucd.cc                                 \
                          $(src_root)/hb-unicode.cc

include $(BUILD_SHARED_LIBRARY)
