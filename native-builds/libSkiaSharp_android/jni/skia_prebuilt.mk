

ifeq ($(TARGET_ARCH_ABI),x86)
  SKIA_ANDROID_RELEASE=../../../skia/out/config/android-x86/Release
else ifeq ($(TARGET_ARCH_ABI),x86_64)
  SKIA_ANDROID_RELEASE=../../../skia/out/config/android-x86_64/Release
else ifeq ($(TARGET_ARCH_ABI),armeabi)
  SKIA_ANDROID_RELEASE=../../../skia/out/config/android-arm/Release
else ifeq ($(TARGET_ARCH_ABI),armeabi-v7a)
  SKIA_ANDROID_RELEASE=../../../skia/out/config/android-arm_v7_neon/Release
else ifeq ($(TARGET_ARCH_ABI),arm64-v8a)
  SKIA_ANDROID_RELEASE=../../../skia/out/config/android-arm64/Release
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
LOCAL_MODULE := libexpat
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libexpat.a
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

ifneq (,$(filter x86 x86_64,$(TARGET_ARCH_ABI)))
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
endif

ifeq ($(TARGET_ARCH_ABI),armeabi-v7a)
  include $(CLEAR_VARS)
  LOCAL_MODULE := libskia_opts_neon
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia_opts_neon.a
  include $(PREBUILT_STATIC_LIBRARY)
endif

ifneq (,$(filter armeabi-v7a arm64-v8a,$(TARGET_ARCH_ABI)))
  include $(CLEAR_VARS)
  LOCAL_MODULE := libpng_static_neon
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libpng_static_neon.a
  include $(PREBUILT_STATIC_LIBRARY)
  
  include $(CLEAR_VARS)
  LOCAL_MODULE := libwebp_dsp_neon
  LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libwebp_dsp_neon.a
  include $(PREBUILT_STATIC_LIBRARY)
endif


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
LOCAL_MODULE := libgiflib
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/obj/gyp/libgiflib.a
include $(PREBUILT_STATIC_LIBRARY)

