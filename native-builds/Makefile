
ANDROID_HOME ?= ${HOME}/Library/Developer/Xamarin/android-sdk-macosx
XCODEBUILD ?= /Applications/Xcode.app/Contents/Developer/usr/bin/xcodebuild
LIPO ?= lipo

ANDROID_NDK_HOME ?= $(HOME)/Library/Developer/Xamarin/android-ndk
NDK_BUILD ?= $(ANDROID_NDK_HOME)/ndk-build

DEPOT_TOOLS_REPO=https://chromium.googlesource.com/chromium/tools/depot_tools.git
DEPOT_TOOLS_TAG=3add4b62b8fc420ba5a6cc0c8bddcf152c00a970

NATIVE_SKIA_ROOT=../skia

ANDROID_BUILD_ENV_VARS=PATH=${PATH}:${CURDIR}/depot_tools ANDROID_HOME=${ANDROID_HOME} ANDROID_SDK_ROOT=${ANDROID_HOME} BUILDTYPE=Release

all: ios android

ios: ios/libskia_ios.framework/libskia_ios


android: libskia_android/libs/armeabi-v7a/libskia_android.so

libskia_android/libs/armeabi-v7a/libskia_android.so: android/x86/libskia_android.so android/x86_64/libskia_android.so \
	android/armeabi-v7a/libskia_android.so android/armeabi/libskia_android.so android/arm64-v8a/libskia_android.so
	cd libskia_android && ${NDK_BUILD}

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

IOS_PROJ=libskia_ios/libskia_ios.xcodeproj
IOS_TARGET=libskia_ios
IOS_PROJ_BUILD=libskia_ios/build

ios/libskia_ios.framework/libskia_ios: ios/i386/libskia_ios.framework/libskia_ios ios/x86_64/libskia_ios.framework/libskia_ios \
	ios/armv7/libskia_ios.framework/libskia_ios ios/arm64/libskia_ios.framework/libskia_ios
	cp -R ios/armv7/libskia_ios.framework ios/
	rm ios/libskia_ios.framework/libskia_ios
	lipo -create -output $@ $^

${NATIVE_SKIA_ROOT}/out/gyp/core.xcodeproj: depot_tools/README
	cd ${NATIVE_SKIA_ROOT} && \
		env PATH=${PATH}:${CURDIR}/depot_tools \
		GYP_DEFINES="skia_os='ios' skia_arch_type='arm' armv7=1 arm_neon=0" \
		python bin/sync-and-gyp

ios/i386/libskia_ios.framework/libskia_ios: ${NATIVE_SKIA_ROOT}/out/gyp/core.xcodeproj
	mkdir -p ios/i386
	${XCODEBUILD} -project ${IOS_PROJ} -target ${IOS_TARGET} -sdk iphonesimulator -arch i386 -configuration Release clean build
	mv ${IOS_PROJ_BUILD}/Release-iphonesimulator/libskia_ios.framework ios/i386/

ios/x86_64/libskia_ios.framework/libskia_ios: ${NATIVE_SKIA_ROOT}/out/gyp/core.xcodeproj
	mkdir -p ios/x86_64
	${XCODEBUILD} -project ${IOS_PROJ} -target ${IOS_TARGET} -sdk iphonesimulator -arch x86_64 -configuration Release clean build
	mv ${IOS_PROJ_BUILD}/Release-iphonesimulator/libskia_ios.framework ios/x86_64/

ios/armv7/libskia_ios.framework/libskia_ios: ${NATIVE_SKIA_ROOT}/out/gyp/core.xcodeproj
	mkdir -p ios/armv7
	${XCODEBUILD} -project ${IOS_PROJ} -target ${IOS_TARGET} -sdk iphoneos -arch armv7 -configuration Release clean build
	mv ${IOS_PROJ_BUILD}/Release-iphoneos/libskia_ios.framework ios/armv7/

ios/arm64/libskia_ios.framework/libskia_ios: ${NATIVE_SKIA_ROOT}/out/gyp/core.xcodeproj
	mkdir -p ios/arm64
	${XCODEBUILD} -project ${IOS_PROJ} -target ${IOS_TARGET} -sdk iphoneos -arch arm64 -configuration Release clean build
	mv ${IOS_PROJ_BUILD}/Release-iphoneos/libskia_ios.framework ios/arm64/

depot_tools/README:
	git clone ${DEPOT_TOOLS_REPO}
	cd depot_tools && git checkout ${DEPOT_TOOLS_TAG}
	touch $@

clean :
	rm -rf depot_tools ios android libskia_android/obj libskia_android/libs

.PHONY: all clean ios android
