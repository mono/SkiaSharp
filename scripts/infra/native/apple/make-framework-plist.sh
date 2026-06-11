#!/bin/bash
# Generate a framework Info.plist equivalent to the one Xcode's PROCESS_INFOPLIST
# build phase emitted for the hand-maintained libSkiaSharp/libHarfBuzzSharp
# .xcodeproj framework targets.
#
# A framework is just a Mach-O dylib + Info.plist + code signature in a bundle
# directory. GN/ninja produces the dylib; this reproduces what Xcode injected
# into the bundle's Info.plist, including the build-provenance keys
# (DT*/BuildMachineOSBuild) that clang/ld never emit but that Apple's App Store /
# notarization asset validation expects on embedded frameworks.
#
# Everything here uses first-party Apple command-line tools only:
#   - plutil      : create/populate the (binary1) plist
#   - xcrun       : SDK version/build (DTSDKName/DTSDKBuild/DTPlatformVersion)
#   - xcodebuild  : Xcode version/build (DTXcode/DTXcodeBuild)
#   - sw_vers     : build machine OS build (BuildMachineOSBuild)
# There is no public Apple CLI that auto-injects the DT* keys (Xcode does it via
# an internal, unexported build tool), so the values are queried from the tools
# above exactly as Xcode does.

set -euo pipefail

OUTPUT=""
NAME=""
IDENTIFIER=""
SDK=""
MIN_OS=""
CATALYST=0
SUPPORTED_PLATFORMS=()
DEVICE_FAMILY=()

while [[ $# -gt 0 ]]; do
    case "$1" in
        --output) OUTPUT="$2"; shift 2;;
        --name) NAME="$2"; shift 2;;
        --identifier) IDENTIFIER="$2"; shift 2;;
        --sdk) SDK="$2"; shift 2;;
        --min-os) MIN_OS="$2"; shift 2;;
        --catalyst) CATALYST=1; shift;;
        --supported-platforms)
            shift
            while [[ $# -gt 0 && "$1" != --* ]]; do SUPPORTED_PLATFORMS+=("$1"); shift; done;;
        --device-family)
            shift
            while [[ $# -gt 0 && "$1" != --* ]]; do DEVICE_FAMILY+=("$1"); shift; done;;
        *) echo "unknown argument: $1" >&2; exit 1;;
    esac
done

# Apple toolchain provenance, queried the same way Xcode populates these keys.
SDK_VERSION="$(xcrun --sdk "$SDK" --show-sdk-version)"
SDK_BUILD="$(xcrun --sdk "$SDK" --show-sdk-build-version)"
BUILD_MACHINE="$(sw_vers -buildVersion)"

# xcodebuild prints e.g. "Xcode 26.3" then "Build version 17C529".
XCODE_VERSION="$(xcodebuild -version | sed -n '1s/Xcode //p')"
DTXCODE_BUILD="$(xcodebuild -version | sed -n '2s/Build version //p')"
# Xcode encodes its version as MMmp (15.4.0 -> 1540, 26.3.0 -> 2630), zero-padded
# to at least four characters.
IFS='.' read -ra _xc <<< "$XCODE_VERSION"
XC_MAJOR="${_xc[0]:-0}"; XC_MINOR="${_xc[1]:-0}"; XC_PATCH="${_xc[2]:-0}"
DTXCODE="$(printf '%04d' $(( 10#$XC_MAJOR * 100 + 10#$XC_MINOR * 10 + 10#$XC_PATCH )))"

# JSON arrays for plutil.
platforms_json="[$(printf '"%s",' "${SUPPORTED_PLATFORMS[@]}" | sed 's/,$//')]"

rm -f "$OUTPUT"
plutil -create binary1 "$OUTPUT"

plutil -insert CFBundleDevelopmentRegion -string "en" "$OUTPUT"
plutil -insert CFBundleExecutable -string "$NAME" "$OUTPUT"
plutil -insert CFBundleIdentifier -string "$IDENTIFIER" "$OUTPUT"
plutil -insert CFBundleInfoDictionaryVersion -string "6.0" "$OUTPUT"
plutil -insert CFBundleName -string "$NAME" "$OUTPUT"
plutil -insert CFBundlePackageType -string "FMWK" "$OUTPUT"
plutil -insert CFBundleShortVersionString -string "1.0" "$OUTPUT"
plutil -insert CFBundleSignature -string "????" "$OUTPUT"
plutil -insert CFBundleSupportedPlatforms -json "$platforms_json" "$OUTPUT"
plutil -insert CFBundleVersion -string "1" "$OUTPUT"

# Build-provenance keys (what Xcode's PROCESS_INFOPLIST injects).
plutil -insert BuildMachineOSBuild -string "$BUILD_MACHINE" "$OUTPUT"
plutil -insert DTCompiler -string "com.apple.compilers.llvm.clang.1_0" "$OUTPUT"
plutil -insert DTPlatformBuild -string "$SDK_BUILD" "$OUTPUT"
plutil -insert DTPlatformName -string "$SDK" "$OUTPUT"
plutil -insert DTPlatformVersion -string "$SDK_VERSION" "$OUTPUT"
plutil -insert DTSDKBuild -string "$SDK_BUILD" "$OUTPUT"
plutil -insert DTSDKName -string "${SDK}${SDK_VERSION}" "$OUTPUT"
plutil -insert DTXcode -string "$DTXCODE" "$OUTPUT"
plutil -insert DTXcodeBuild -string "$DTXCODE_BUILD" "$OUTPUT"

if [[ "$CATALYST" == "1" ]]; then
    plutil -insert LSMinimumSystemVersion -string "$MIN_OS" "$OUTPUT"
else
    plutil -insert MinimumOSVersion -string "$MIN_OS" "$OUTPUT"
fi

if [[ ${#DEVICE_FAMILY[@]} -gt 0 ]]; then
    family_json="[$(printf '%s,' "${DEVICE_FAMILY[@]}" | sed 's/,$//')]"
    plutil -insert UIDeviceFamily -json "$family_json" "$OUTPUT"
fi
