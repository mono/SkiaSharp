
SKIA_ANDROID_RELEASE=../../../externals/skia/out/android/$(TARGET_ARCH_ABI)

include $(CLEAR_VARS)
LOCAL_MODULE := libskia
LOCAL_SRC_FILES := $(SKIA_ANDROID_RELEASE)/libskia.a
include $(PREBUILT_STATIC_LIBRARY)
