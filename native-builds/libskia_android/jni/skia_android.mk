
include jni/skia_prebuilt.mk

include $(CLEAR_VARS)

PRIVATE_STRIP = echo
LOCAL_WHOLE_STATIC_LIBRARIES := libskia_core libskia_effects libskia_utils 

LOCAL_STATIC_LIBRARIES := libskia_ports libskia_skgpu   \
    libskia_images libskia_opts libfreetype_static libpng_static libexpat libSkKTX libetc1 \
    libskia_sfnt libpng_static_neon libskia_opts_neon libskia_opts_avx libskia_opts_ssse3 libskia_opts_sse41 libcpu_features

LOCAL_LDLIBS := -llog -landroid -lEGL -lGLESv2 -lz

LOCAL_MODULE           := skia_android
LOCAL_C_INCLUDES       := ../../skia/include/c
LOCAL_CFLAGS           += -DNEED_INIT_NEON
LOCAL_SRC_FILES        :=  skia_android/skia_android.cpp

include $(BUILD_SHARED_LIBRARY)



