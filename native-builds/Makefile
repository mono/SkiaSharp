
ANDROID_HOME ?= ${HOME}/Library/Developer/Xamarin/android-sdk-macosx

DEPOT_TOOLS_REPO=https://chromium.googlesource.com/chromium/tools/depot_tools.git
DEPOT_TOOLS_TAG=3add4b62b8fc420ba5a6cc0c8bddcf152c00a970

NATIVE_SKIA_ROOT=../skia

ANDROID_BUILD_ENV_VARS=PATH=${PATH}:${CURDIR}/depot_tools ANDROID_HOME=${ANDROID_HOME} ANDROID_SDK_ROOT=${ANDROID_HOME} BUILDTYPE=Release

all: ios android

ios: ios/libskia_core.a

android: android/x86/libskia_android.so android/x86_64/libskia_android.so \
	android/armeabi-v7a/libskia_android.so android/armeabi/libskia_android.so android/arm64-v8a/libskia_android.so

android/x86/libskia_android.so: depot_tools/README
	cd ${NATIVE_SKIA_ROOT} && \
		env ${ANDROID_BUILD_ENV_VARS} \
		./platform_tools/android/bin/android_ninja -d x86 "skia_lib"

	mkdir -p android/x86
	cp ${NATIVE_SKIA_ROOT}/out/config/android-x86/Release/lib/libskia_android.so $@

android/x86_64/libskia_android.so: depot_tools/README
	cd ${NATIVE_SKIA_ROOT} && \
		env ${ANDROID_BUILD_ENV_VARS} \
		./platform_tools/android/bin/android_ninja -d x86_64 "skia_lib"

	mkdir -p android/x86_64
	cp ${NATIVE_SKIA_ROOT}/out/config/android-x86_64/Release/lib/libskia_android.so $@

android/armeabi/libskia_android.so: depot_tools/README
	cd ${NATIVE_SKIA_ROOT} && \
		env ${ANDROID_BUILD_ENV_VARS} \
		./platform_tools/android/bin/android_ninja -d arm "skia_lib"

	mkdir -p android/armeabi
	cp ${NATIVE_SKIA_ROOT}/out/config/android-arm/Release/lib/libskia_android.so $@

android/armeabi-v7a/libskia_android.so: depot_tools/README
	cd ${NATIVE_SKIA_ROOT} && \
		env ${ANDROID_BUILD_ENV_VARS} \
		./platform_tools/android/bin/android_ninja -d arm_v7_neon "skia_lib"

	mkdir -p android/armeabi-v7a
	cp ${NATIVE_SKIA_ROOT}/out/config/android-arm_v7_neon/Release/lib/libskia_android.so $@

android/arm64-v8a/libskia_android.so: depot_tools/README
	cd ${NATIVE_SKIA_ROOT} && \
		env ${ANDROID_BUILD_ENV_VARS} \
		./platform_tools/android/bin/android_ninja -d arm64 "skia_lib"

	mkdir -p android/arm64-v8a
	cp ${NATIVE_SKIA_ROOT}/out/config/android-arm64/Release/lib/libskia_android.so $@


ios/libskia_core.a: depot_tools/README
	mkdir -p ios

	cd ${NATIVE_SKIA_ROOT} && \
		env PATH=${PATH}:${CURDIR}/depot_tools \
		GYP_DEFINES="skia_os='ios' skia_arch_type='arm' armv7=1 arm_neon=0" \
		python bin/sync-and-gyp

	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=core IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/core.xcodeproj IOS_LIB_NAME=libskia_core.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=libjpeg-turbo IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/libjpeg-turbo.xcodeproj IOS_LIB_NAME=libjpeg-turbo.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=libSkKTX IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/ktx.xcodeproj IOS_LIB_NAME=libSkKTX.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=libetc1 IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/etc1.xcodeproj IOS_LIB_NAME=libetc1.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=libpng_static IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/libpng.xcodeproj IOS_LIB_NAME=libpng_static.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=codec IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/codec.xcodeproj IOS_LIB_NAME=libskia_codec_android.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=codec_android IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/codec_android.xcodeproj IOS_LIB_NAME=libskia_codec_android.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=effects IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/effects.xcodeproj IOS_LIB_NAME=libskia_effects.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=images IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/images.xcodeproj IOS_LIB_NAME=libskia_images.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=opts IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/opts.xcodeproj IOS_LIB_NAME=libskia_opts.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=ports IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/ports.xcodeproj IOS_LIB_NAME=libskia_ports.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=sfnt IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/sfnt.xcodeproj IOS_LIB_NAME=libskia_sfnt.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=skgpu IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/gpu.xcodeproj IOS_LIB_NAME=libskia_skgpu.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=utils IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/utils.xcodeproj IOS_LIB_NAME=libskia_utils.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=libwebp IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/libwebp.xcodeproj IOS_LIB_NAME=libwebp_dec.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=libwebp_dsp_enc IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/libwebp.xcodeproj IOS_LIB_NAME=libwebp_dsp_enc.a
	${MAKE} -f MakeiOSProject.mk NATIVE_SKIA_ROOT=${NATIVE_SKIA_ROOT} IOS_TARGET=giflib IOS_PROJ=${NATIVE_SKIA_ROOT}/out/gyp/giflib.xcodeproj IOS_LIB_NAME=libgiflib.a

	for libfile in ios/i386/*; \
		do \
		lipo -create -output ios/$$(basename "$$libfile") \
		ios/i386/$$(basename "$$libfile") \
		ios/x86_64/$$(basename "$$libfile") \
		ios/armv7/$$(basename "$$libfile") \
		ios/arm64/$$(basename "$$libfile"); \
	done

depot_tools/README:
	git clone ${DEPOT_TOOLS_REPO}
	cd depot_tools && git checkout ${DEPOT_TOOLS_TAG}
	touch $@

clean :
	rm -rf depot_tools ios android

.PHONY: all clean ios android
