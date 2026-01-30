
include $(CLEAR_VARS)

src_root = $(abspath ../../../externals/skia/third_party/externals/harfbuzz/src)
ext_root = $(abspath ../../../externals/skia/third_party/harfbuzz)

LOCAL_MODULE           := HarfBuzzSharp

LOCAL_C_INCLUDES       := . $(src_root) $(ext_root)

LOCAL_LDFLAGS          := -Wl,--gc-sections -Wl,-z,max-page-size=16384

LOCAL_CFLAGS           := -DNDEBUG                                                                  \
                          -DHAVE_CONFIG_OVERRIDE_H -DHAVE_OT -DHB_NO_FALLBACK_SHAPE                 \
                          -fno-rtti -fno-exceptions -fno-threadsafe-statics -fPIC                   \
                          -Os -ffunction-sections -fdata-sections -g -ggdb3

LOCAL_CPPFLAGS         := -std=c++11

LOCAL_SRC_FILES        := $(src_root)/harfbuzz-subset.cc

include $(BUILD_SHARED_LIBRARY)
