
include jni/skia_prebuilt.mk

include $(CLEAR_VARS)

PRIVATE_STRIP = echo
LOCAL_WHOLE_STATIC_LIBRARIES := libskia_core libskia_effects libskia_utils libskia_opts libskia_opts_neon

LOCAL_STATIC_LIBRARIES := libskia_ports libskia_skgpu libskia_images libskia_opts        \
                          libfreetype_static  libSkKTX libetc1                           \
                          libskia_sfnt libskia_opts_avx libskia_opts_ssse3               \
                          libskia_opts_sse41 libskia_codec libraw_codec libskia_pdf      \
                          libjpeg-turbo libcpu_features libgiflib libsfntly libicuuc     \
                          libpiex libwebp_enc libwebp_dec libwebp_dsp libwebp_utils      \
                          libwebp_demux libwebp_dsp_enc libwebp_dsp_neon                 \
                          libpng_static_neon libpng_static libdng_sdk libzlib            \
                          libskia_core libskia_effects libskia_utils libexpat_static     \
                          libskia_opts_crc32 libskia_opts_sse42 libskia_opts_hsw
                          
LOCAL_LDLIBS           := -llog -landroid -lEGL -lGLESv2 -lz

LOCAL_MODULE           := SkiaSharp

LOCAL_C_INCLUDES       := ../../externals/skia/src/c           \
                          ../../externals/skia/include/c       \
                          ../../externals/skia/include/core    \
                          ../../externals/skia/include/codec   \
                          ../../externals/skia/include/effects \
                          ../../externals/skia/include/pathops \
                          ../../externals/skia/include/gpu     \
                          ../../externals/skia/include/config  \
                          ../../externals/skia/include/utils   \
                          ../../externals/skia/include/images

LOCAL_CFLAGS           := -DSK_INTERNAL -DSK_GAMMA_APPLY_TO_A8                                   \
                          -DSK_ALLOW_STATIC_GLOBAL_INITIALIZERS=0 -DSK_SUPPORT_GPU=1             \
                          -DSK_SUPPORT_OPENCL=0 -DSK_FORCE_DISTANCE_FIELD_TEXT=0                 \
                          -DSK_BUILD_FOR_ANDROID -DSK_GAMMA_EXPONENT=1.4 -DSK_GAMMA_CONTRAST=0.0 \
                          -DSKIA_DLL -DSKIA_IMPLEMENTATION=1 -DSK_SUPPORT_LEGACY_CLIPTOLAYERFLAG \
                          -DNDEBUG                                                               

LOCAL_CFLAGS           += -fPIC -g -fno-exceptions -fstrict-aliasing -Wall -Wextra               \
                          -Winit-self -Wpointer-arith -Wsign-compare -Wno-unused-parameter       \
                          -Werror -fuse-ld=gold -O2
                          
LOCAL_CPPFLAGS         := -std=c++11 -fno-rtti -fno-threadsafe-statics -Wnon-virtual-dtor

LOCAL_SRC_FILES        := ../../src/sk_xamarin.cpp ../../src/SkiaKeeper.c ../../src/sk_managedstream.cpp ../../src/SkManagedStream.cpp

include $(BUILD_SHARED_LIBRARY)



