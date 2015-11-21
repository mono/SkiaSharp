
XCODEBUILD ?= /Applications/Xcode.app/Contents/Developer/usr/bin/xcodebuild
LIPO ?= lipo

NATIVE_SKIA_ROOT?=../skia
IOS_TARGET ?= core
IOS_PROJ ?= skia/out/gyp/core.xcodeproj
IOS_LIB_NAME ?= libskia_core.a

all: ios/i386/${IOS_LIB_NAME} ios/x86_64/${IOS_LIB_NAME} ios/armv7/${IOS_LIB_NAME} ios/arm64/${IOS_LIB_NAME}

ios/i386/${IOS_LIB_NAME}:
	${XCODEBUILD} -project ${IOS_PROJ} -target ${IOS_TARGET} -sdk iphonesimulator -arch i386 -configuration Release clean build
	mkdir -p ios/i386
	mv ${NATIVE_SKIA_ROOT}/xcodebuild/Release-iphonesimulator/*.a ios/i386/

ios/x86_64/${IOS_LIB_NAME}:
	${XCODEBUILD} -project ${IOS_PROJ} -target ${IOS_TARGET} -sdk iphonesimulator -arch x86_64 -configuration Release clean build
	mkdir -p ios/x86_64
	mv ${NATIVE_SKIA_ROOT}/xcodebuild/Release-iphonesimulator/*.a ios/x86_64/

ios/armv7/${IOS_LIB_NAME}:
	$(XCODEBUILD) -project ${IOS_PROJ} -target ${IOS_TARGET} -sdk iphoneos -arch armv7 -configuration Release clean build
	mkdir -p ios/armv7
	mv ${NATIVE_SKIA_ROOT}/xcodebuild/Release-iphoneos/*.a ios/armv7/

ios/arm64/${IOS_LIB_NAME}:
	$(XCODEBUILD) -project ${IOS_PROJ} -target ${IOS_TARGET} -sdk iphoneos -arch arm64 -configuration Release clean build
	mkdir -p ios/arm64
	mv ${NATIVE_SKIA_ROOT}/xcodebuild/Release-iphoneos/*.a ios/arm64/