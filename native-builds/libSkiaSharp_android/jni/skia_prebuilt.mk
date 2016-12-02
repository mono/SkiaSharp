

ifeq ($(TARGET_ARCH_ABI),x86)
  SKIA_ANDROID_RELEASE=../../../externals/skia/out/config/android-x86/Release
else ifeq ($(TARGET_ARCH_ABI),x86_64)
  SKIA_ANDROID_RELEASE=../../../externals/skia/out/config/android-x86_64/Release
else ifeq ($(TARGET_ARCH_ABI),armeabi)
  SKIA_ANDROID_RELEASE=../../../externals/skia/out/config/android-arm/Release
else ifeq ($(TARGET_ARCH_ABI),armeabi-v7a)
  SKIA_ANDROID_RELEASE=../../../externals/skia/out/config/android-arm_v7_neon/Release
else ifeq ($(TARGET_ARCH_ABI),arm64-v8a)
  SKIA_ANDROID_RELEASE=../../../externals/skia/out/config/android-arm64/Release
endif

include $(CLEAR_VARS)
LOCAL_MODULE := libskia_core 
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_core.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libskia_effects
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_effects.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libskia_utils
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_utils.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libskia_ports
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_ports.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libskia_skgpu
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_skgpu.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libskia_images
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_images.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libskia_opts
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libfreetype_static
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libfreetype_static.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libpng_static
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libpng_static.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libexpat_static
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libexpat_static.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libSkKTX
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libSkKTX.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libetc1
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libetc1.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libskia_sfnt
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_sfnt.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libcpu_features
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libcpu_features.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libjpeg-turbo
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libjpeg-turbo.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libwebp_dec
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libwebp_dec.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libwebp_demux
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libwebp_demux.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libwebp_dsp
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libwebp_dsp.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libwebp_dsp_enc
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libwebp_dsp_enc.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libwebp_enc
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libwebp_enc.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libwebp_utils
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libwebp_utils.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libsksl
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libsksl.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libskia_pdf
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_pdf.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libsfntly
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libsfntly.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libicuuc
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libicuuc.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libzlib
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libzlib.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libskia_codec
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_codec.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libraw_codec
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libraw_codec.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libpiex
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libpiex.a
include $(PREBUILT_STATIC_LIBRARY)

include $(CLEAR_VARS)
LOCAL_MODULE := libdng_sdk
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libdng_sdk.a
include $(PREBUILT_STATIC_LIBRARY)


###
# platforms
###

ifeq ($(TARGET_ARCH_ABI),armeabi-v7a)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libwebp_dsp_neon
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libwebp_dsp_neon.a
  include $(PREBUILT_STATIC_LIBRARY)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_neon
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_neon.a
  include $(PREBUILT_STATIC_LIBRARY)
  
endif

ifeq ($(TARGET_ARCH_ABI),arm64-v8a)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libwebp_dsp_neon
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libwebp_dsp_neon.a
  include $(PREBUILT_STATIC_LIBRARY)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_crc32
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_crc32.a
  include $(PREBUILT_STATIC_LIBRARY)

endif

ifeq ($(TARGET_ARCH_ABI),x86)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_avx
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_avx.a
  include $(PREBUILT_STATIC_LIBRARY)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_ssse3
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_ssse3.a
  include $(PREBUILT_STATIC_LIBRARY)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_sse41
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_sse41.a
  include $(PREBUILT_STATIC_LIBRARY)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_sse42
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_sse42.a
  include $(PREBUILT_STATIC_LIBRARY)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_hsw
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_hsw.a
  include $(PREBUILT_STATIC_LIBRARY)

endif

ifeq ($(TARGET_ARCH_ABI),x86_64)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_avx
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_avx.a
  include $(PREBUILT_STATIC_LIBRARY)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_ssse3
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_ssse3.a
  include $(PREBUILT_STATIC_LIBRARY)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_sse41
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_sse41.a
  include $(PREBUILT_STATIC_LIBRARY)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_sse42
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_sse42.a
  include $(PREBUILT_STATIC_LIBRARY)

  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_hsw
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_hsw.a
  include $(PREBUILT_STATIC_LIBRARY)

endif
