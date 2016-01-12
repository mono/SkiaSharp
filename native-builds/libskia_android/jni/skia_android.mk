
include jni/skia_prebuilt.mk

include $(CLEAR_VARS)

PRIVATE_STRIP = echo
LOCAL_WHOLE_STATIC_LIBRARIES := libskia_core libskia_effects libskia_utils 

LOCAL_STATIC_LIBRARIES := libskia_ports libskia_skgpu libskia_images libskia_opts        \
                          libfreetype_static libpng_static libexpat libSkKTX libetc1     \
                          libskia_sfnt libpng_static_neon libskia_opts_neon              \
                          libskia_opts_avx libskia_opts_ssse3 libskia_opts_sse41         \
                          libcpu_features

LOCAL_LDLIBS           := -llog -landroid -lEGL -lGLESv2 -lz

LOCAL_MODULE           := skia_android

LOCAL_C_INCLUDES       := ../../skia/src/c ../../skia/include/c ../../skia/include/core ../../skia/include/config

LOCAL_CFLAGS           := -DSK_INTERNAL -DSK_GAMMA_APPLY_TO_A8                                   \
                          -DSK_ALLOW_STATIC_GLOBAL_INITIALIZERS=0 -DSK_SUPPORT_GPU=1             \
                          -DSK_SUPPORT_OPENCL=0 -DSK_FORCE_DISTANCE_FIELD_TEXT=0                 \
                          -DSK_BUILD_FOR_ANDROID -DSK_GAMMA_EXPONENT=1.4 -DSK_GAMMA_CONTRAST=0.0 \
                          -DSKIA_DLL -DSKIA_IMPLEMENTATION=1 -DSK_SUPPORT_LEGACY_CLIPTOLAYERFLAG \
                          -DNDEBUG -DNEED_INIT_NEON                                              \
                          -fPIC -g -fno-exceptions -fstrict-aliasing -Wall -Wextra               \
                          -Winit-self -Wpointer-arith -Wsign-compare -Wno-unused-parameter       \
                          -Werror -fuse-ld=gold -O2
                          
LOCAL_CPPFLAGS         := -std=c++11 -fno-rtti -fno-threadsafe-statics -Wnon-virtual-dtor

LOCAL_SRC_FILES        := ../../src/sk_xamarin.cpp ../../src/SkiaKeeper.c ../../src/sk_managedstream.cpp ../../src/SkManagedStream.cpp

include $(BUILD_SHARED_LIBRARY)



