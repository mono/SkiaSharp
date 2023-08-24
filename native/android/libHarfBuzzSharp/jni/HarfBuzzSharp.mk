
include $(CLEAR_VARS)

cmd-strip = $(PRIVATE_STRIP) --strip-all $(call host-path,$1)

src_root = $(abspath ../../../externals/skia/third_party/externals/harfbuzz/src)
ext_root = $(abspath ../../../externals/skia/third_party/harfbuzz)

LOCAL_MODULE           := HarfBuzzSharp

LOCAL_C_INCLUDES       := . $(src_root) $(ext_root)

LOCAL_LDFLAGS          := -s -Wl,--gc-sections

LOCAL_CFLAGS           := -DNDEBUG                                                                  \
                          -DHAVE_CONFIG_OVERRIDE_H -DHAVE_OT -DHB_NO_FALLBACK_SHAPE                 \
                          -fno-rtti -fno-exceptions -fno-threadsafe-statics -fPIC                   \
                          -g -Os -ffunction-sections -fdata-sections

LOCAL_CPPFLAGS         := -std=c++11

LOCAL_SRC_FILES        := $(src_root)/harfbuzz-subset.cc

include $(BUILD_SHARED_LIBRARY)
