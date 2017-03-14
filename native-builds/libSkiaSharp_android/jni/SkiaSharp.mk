
include jni/skia_prebuilt.mk

include $(CLEAR_VARS)

cmd-strip = $(PRIVATE_STRIP) --strip-all $(call host-path,$1)

# LOCAL_WHOLE_STATIC_LIBRARIES := libskia

LOCAL_STATIC_LIBRARIES := libskia

LOCAL_LDLIBS           := -llog -lEGL -lGLESv2

LOCAL_LDFLAGS          := -s -Wl,--gc-sections

LOCAL_MODULE           := SkiaSharp

LOCAL_C_INCLUDES       := ../../externals/skia/src/c           \
                          ../../externals/skia/include/c       \
                          ../../externals/skia/include/core    \
                          ../../externals/skia/include/codec   \
                          ../../externals/skia/include/effects \
                          ../../externals/skia/include/pathops \
                          ../../externals/skia/include/gpu     \
                          ../../externals/skia/include/config  \
                          ../../externals/skia/include/xml     \
                          ../../externals/skia/include/svg     \
                          ../../externals/skia/include/utils   \
                          ../../externals/skia/include/ports   \
                          ../../externals/skia/include/images

LOCAL_CFLAGS           := -DSK_INTERNAL -DSK_GAMMA_APPLY_TO_A8                                   \
                          -DSK_ALLOW_STATIC_GLOBAL_INITIALIZERS=0 -DSK_SUPPORT_GPU=1             \
                          -DSK_SUPPORT_OPENCL=0 -DSK_FORCE_DISTANCE_FIELD_TEXT=0                 \
                          -DSK_BUILD_FOR_ANDROID -DSK_GAMMA_EXPONENT=1.4 -DSK_GAMMA_CONTRAST=0.0 \
                          -DSKIA_C_DLL -DSKIA_IMPLEMENTATION=1                                   \
                          -DSK_SUPPORT_LEGACY_CLIPTOLAYERFLAG -DNDEBUG

LOCAL_CFLAGS           += -fPIC -g -fno-exceptions -fstrict-aliasing -Wall -Wextra               \
                          -Winit-self -Wpointer-arith -Wsign-compare -Wno-unused-parameter       \
                          -Werror -fuse-ld=gold                                                  \
                          -Os -ffunction-sections -fdata-sections -fno-rtti

LOCAL_CPPFLAGS         := -std=c++11 -fno-rtti -fno-threadsafe-statics -Wnon-virtual-dtor        \
                          -Os -ffunction-sections -fdata-sections

LOCAL_SRC_FILES        := ../../src/sk_xamarin.cpp               \
                          ../../src/SkiaKeeper.c                 \
                          ../../src/sk_managedstream.cpp         \
                          ../../src/SkManagedStream.cpp

include $(BUILD_SHARED_LIBRARY)



