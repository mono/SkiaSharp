Param (
    [string[]] $Platforms = $null,
    [bool] $SkipInit = $false,
    [bool] $SkipBuild = $false
)

$ErrorActionPreference = 'Stop'

# Prepare the script itself
. "./build-common.ps1"

# Paths
$SKIA_PATH = Join-Path $EXTERNALS_PATH 'skia'
$DEPOT_PATH = Join-Path $EXTERNALS_PATH 'depot_tools'
$HARFBUZZ_PATH = Join-Path $EXTERNALS_PATH 'harfbuzz'
$ANGLE_PATH = Join-Path $EXTERNALS_PATH 'angle'
$ANDROID_NDK_HOME = $env:ANDROID_NDK_HOME

# skia Tools
$git_sync_deps = Join-Path $SKIA_PATH 'tools/git-sync-deps'
$gn = Join-Path $SKIA_PATH $(if ($IsMacOS -or $IsLinux) { 'bin/gn' } else { 'bin/gn.exe' })
$ninja = Join-Path $DEPOT_PATH $(if ($IsMacOS -or $IsLinux) { 'ninja' } else { 'ninja.exe' })

# System Tools
$python = ''
$xcodebuild = ''
$strip = ''
$codesign = ''
$lipo = ''
$bash = ''
$git = ''
$make = ''
$cmake = ''

# Tool Versions
$xcodebuildVersion = ''
$pythonVersion = ''
$makeVersion = ''
$cmakeVersion = ''

################################################################################
# Tools

Function Lipo ([string] $dest, [string[]] $libs)
{
    WriteLine "Creating fat file '$dest'..."

    $dir = Split-Path -path $dest
    $name = Split-Path -path $dest -leaf

    if (Test-Path $dest) {
        Remove-Item $dest -force -recurse
    }

    if ($name -like "*.framework") {
        Copy-Item -literalpath "$dir/$($libs[0])/$name" -destination "$dest" -force -recurse
        $name = "$name/$([System.IO.Path]::GetFileNameWithoutExtension($name))"
    }

    $libs = $libs | ForEach-Object { "$_/$name" }
    Exec $lipo -a "-create -output $name $libs" -wo $dir
}

Function GnNinja ([string] $out, [string] $target = "SkiaSharp", [string] $skiaArgs)
{
    Exec $gn -a " gen out/$out --args=""$skiaArgs"" " -wo $SKIA_PATH
    Exec $ninja -a " $target -C out/$out " -wo $SKIA_PATH
}

Function XCodeBuild ([string] $project, [string] $sdk, [string] $arch)
{
    WriteLine "Building '$project' as $sdk|$arch..."
    $target = [System.IO.Path]::GetFileNameWithoutExtension($project)
    $xcodebuildArgs = "-project $project -target $target -sdk $sdk -arch $arch -configuration Release -quiet"
    Exec $xcodebuild -a $xcodebuildArgs
}

Function StripSign ([string] $target)
{
    if ($target -like "*.framework") {
        $archive = "$target/$([System.IO.Path]::GetFileNameWithoutExtension($target))"
    } else {
        $archive = $target
    }
    WriteLine "Stripping and signing '$target'..."
    Exec $strip -a "-x -S $archive"
    Exec $codesign -a "--force --sign - --timestamp=none $target"
}

################################################################################

Function InitializeTools ()
{
    WriteLine "$hr"
    WriteLine "Initializing tools..."
    WriteLine ""

    # try and find the tools
    if ($IsMacOS -or $IsLinux) {
        # find make
        $script:make = FindTool 'make'
        if (!$make -or !(Test-Path $make)) {
            throw 'Unable to locate "make". Make sure it exists in the "PATH" environment variable.'
        }

        # find bash
        $script:bash = FindTool 'bash'
        if (!$bash -or !(Test-Path $bash)) {
            throw 'Unable to locate "bash" on Unix...'
        }
    }

    if ($IsMacOS) {
        $script:strip = FindTool 'strip'
        $script:codesign = FindTool 'codesign'
        $script:lipo = FindTool 'lipo'
        $script:tar = FindTool 'tar'

        # find the Android NDK root
        if (!$ANDROID_NDK_HOME -or $(Test-Path $ANDROID_NDK_HOME)) {
            $script:ANDROID_NDK_HOME = Join-Path $HOME_PATH 'Library/Developer/Xamarin/android-ndk'
        }
        if (!(Test-Path $ANDROID_NDK_HOME)) {
            throw 'Unable to locate the "Android NDK home". Use the "ANDROID_NDK_HOME" environment variable.'
        }

        # find xcodebuild
        $script:xcodebuild = FindTool 'xcodebuild'
        if (!$xcodebuild -or !(Test-Path $xcodebuild)) {
            throw 'Unable to locate "xcodebuild". Make sure XCode and the command line tools are installed.'
        }
        $script:xcodebuildVersion = if ($xcodebuild) { & $xcodebuild -version }
    } elseif ($IsLinux) {
    } else {
        # find msbuild
        if (!$msbuild -or !(Test-Path $msbuild)) {
            throw 'Unable to locate "MSBuild.exe". Make sure Visual Studio 2017 is installed.'
        }
    }

    # find git
    $script:git = FindTool 'git'

    # find Python
    $script:python = FindTool 'python'

    # find cmake
    $script:cmake = FindTool 'cmake'
    if (!$cmake) {
        if ($IsMacOS -or $IsLinux) {
            # TODO: try find cmake
        } else {
            if ($vswhere) {
                $cmakeRoot = & $vswhere -latest -products * -requires Microsoft.VisualStudio.Component.VC.CMake.Project -property installationPath
                if ($cmakeRoot -and (Test-Path $cmakeRoot)) {
                    $script:cmake = Join-Path $cmakeRoot 'Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe'
                }
            }
        }
    }

    # verify that all the common tools exist
    if (!$python -or !(Test-Path $python)) {
        throw 'Unable to locate "python". Make sure it exists in the "PATH" environment variable.'
    }
    if (!$cmake -or !(Test-Path $cmake)) {
        throw 'Unable to locate "cmake". Make sure it exists in the "PATH" environment variable or Visual Studio 2017 is installed.'
    }

    # get the versions
    $script:cmakeVersion = if ($cmake) { & $cmake -version }
    $script:pythonVersion = if ($python) { & $python --version }
    $script:makeVersion = if ($make) { & $make -version }

    WriteLine "Tool initialization complete."
    WriteLine "$hr"
    WriteLine ""
}

################################################################################

Function WriteSystemInfo ()
{
    WriteLine "$hr"
    WriteLine "Current System:"
    WriteLine "  Operating system:    '$operatingSystem'"
    WriteLine "  PowerShell version:  '$powershellVersion'"
    WriteLine ""
    WriteLine "Tool Versions:"
    WriteLine "  cmake version:    '$cmakeVersion'"
    WriteLine "  make version:     '$makeVersion'"
    WriteLine "  MSBuild version:  '$msbuildVersion'"
    WriteLine "  Python version:   '$pythonVersion'"
    WriteLine "  XCode version:    '$xcodebuildVersion'"
    WriteLine ""
    WriteLine "Other Versions:"
    WriteLine "  skia version:      '$(GetVersion "skia" "release")'"
    WriteLine "  ANGLE version:     '$(GetVersion "ANGLE" "release")'"
    WriteLine "  HarfBuzz version:  '$(GetVersion "harfbuzz" "release")'"
    WriteLine ""
    WriteLine "Tool Paths:"
    WriteLine "  bash:           '$bash'"
    WriteLine "  cmake:          '$cmake'"
    WriteLine "  codesign:       '$codesign'"
    WriteLine "  git:            '$git'"
    WriteLine "  git-sync-deps:  '$git_sync_deps'"
    WriteLine "  gn:             '$gn'"
    WriteLine "  lipo:           '$lipo'"
    WriteLine "  make:           '$make'"
    WriteLine "  MSBuild:        '$msbuild'"
    WriteLine "  ninja:          '$ninja'"
    WriteLine "  Python:         '$python'"
    WriteLine "  strip:          '$strip'"
    WriteLine "  XCodeBuild:     '$xcodebuild'"
    WriteLine ""
    WriteLine "Other Paths:"
    WriteLine "  ANGLE_PATH:       '$ANGLE_PATH'"
    WriteLine "  DEPOT_PATH:       '$DEPOT_PATH'"
    WriteLine "  HARFBUZZ_PATH:    '$HARFBUZZ_PATH'"
    WriteLine "  SKIA_PATH:        '$SKIA_PATH'"
    WriteLine "  ANDROID_NDK_HOME: '$ANDROID_NDK_HOME'"
    WriteLine "$hr"
    WriteLine ""
}

################################################################################
# Initialize the repository
Function Initialize ()
{
    WriteLine "$hr"
    WriteLine "Initializing repository..."
    WriteLine ""
    if ($SkipInit) {
        WriteLine "Skipping initialization as requested."
    } else {
        InitializeSkia
        InitializeHarfBuzz
        InitializeANGLE
        WriteLine "Repository initialization complete."
    }
    WriteLine "$hr"
    WriteLine ""
}

Function InitializeSkia ()
{
    WriteLine "Initializing skia..."

    # sync skia dependencies
    Exec $git -a "submodule update --init --recursive"
    Exec $python -a $git_sync_deps -wo $SKIA_PATH
}

Function InitializeANGLE ()
{
    WriteLine "Initializing ANGLE..."

    # download ANGLE
    $version = GetVersion "ANGLE" "release"
    $angleRoot = Join-Path $ANGLE_PATH "uwp"
    $angleZip = Join-Path $angleRoot "ANGLE.WindowsStore.$version.zip"
    if (!(Test-Path $angleZip)) {
        WriteLine "Downloading ANGLE..."
        New-Item $angleRoot -itemtype "Directory" -force | Out-Null
        Invoke-WebRequest -Uri "https://www.nuget.org/api/v2/package/ANGLE.WindowsStore/$version" -OutFile $angleZip
    }
    
    # extract ANGLE
    if (!(Test-Path "$angleRoot/ANGLE.WindowsStore.nuspec")) {
        WriteLine "Extracting ANGLE..."
        Expand-Archive $angleZip $angleRoot
    }
}

Function InitializeHarfBuzz ()
{
    WriteLine "Initializing HarfBuzz..."

    # sync harfbuzz dependencies
    Exec $git -a "submodule update --init --recursive"
    if ($IsMacOS -or $IsLinux) {
        Exec $bash -a "autogen.sh" -wo $HARFBUZZ_PATH
        # ./configure is called automatically by ./autogen.sh
        # Exec $bash -a "configure" -wo $HARFBUZZ_PATH
    }
}

################################################################################
# Build-*-Arch

Function Build-Windows-Arch ([string] $arch)
{
    WriteLine "Building the Windows library for '$arch'..."

    if ($arch -eq "x86") {
        $cmakeArch = ""
    } elseif ($arch -eq "x64") {
        $cmakeArch = " Win64"
    } else {
        throw "Unknown Windows architecture: $arch"
    }

    # Build libSkiaSharp.dll
    GnNinja -out "win/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""win\"" target_cpu=\""$arch\""
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"", \""/MD\"", \""/EHsc\"" ]
        extra_ldflags=[ \""/DEBUG\"" ]
"@

    # Build harfbuzz.dll
    $harfbuzzOut = Join-Path $HARFBUZZ_PATH "out/win/$arch"
    New-Item $harfbuzzOut -itemtype "Directory" -force | Out-Null
    $config = " -T v140 -D""CMAKE_SYSTEM_VERSION=8.1"" -D""BUILD_SHARED_LIBS=1"" "
    Exec $cmake -a "-G ""Visual Studio 15 2017$cmakeArch"" $config ../../../" -wo $harfbuzzOut
    MSBuild -project "$harfbuzzOut/harfbuzz.vcxproj" -arch $arch

    # Copy the output to the output folder
    $out = "$OUTPUT_PATH/native/windows/$arch"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "$SKIA_PATH/out/win/$arch/libSkiaSharp.dll" $out -force
    Copy-Item "$HARFBUZZ_PATH/out/win/$arch/Release/harfbuzz.dll" $out -force
}

Function Build-UWP-Arch ([string] $arch)
{
    WriteLine "Building the UWP library for '$arch'..."

    if ($arch -eq "x86") {
        $cmakeArch = ""
    } elseif ($arch -eq "x64") {
        $cmakeArch = " Win64"
    } elseif ($arch -eq "arm") {
        $cmakeArch = " ARM"
    } else {
        throw "Unknown UWP architecture: $arch"
    }

    # Build libSkiaSharp.dll
    GnNinja -out "uwp/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""winrt\"" target_cpu=\""$arch\""
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[
          \""-DSKIA_C_DLL\"", \""/MD\"", \""/EHsc\"",
          \""-DWINAPI_FAMILY=WINAPI_FAMILY_APP\"", \""-DSK_BUILD_FOR_WINRT\"", \""-DSK_HAS_DWRITE_1_H\"", \""-DNO_GETENV\"", \""-DNO_GETENV\""
        ]
        extra_ldflags=[ \""/DEBUG\"",  \""/APPCONTAINER\"",  \""WindowsApp.lib\"" ]
"@

    # Build harfbuzz.dll
    $harfbuzzOut = Join-Path $HARFBUZZ_PATH "out/uwp/$arch"
    New-Item $harfbuzzOut -itemtype "Directory" -force | Out-Null
    $config = " -T v141 -D""CMAKE_SYSTEM_VERSION=10.0.10240.0"" -D""CMAKE_SYSTEM_NAME=WindowsStore"" -D""BUILD_SHARED_LIBS=1"" "
    Exec $cmake -a "-G ""Visual Studio 15 2017$cmakeArch"" $config ../../../" -wo $harfbuzzOut
    MSBuild -project "$harfbuzzOut/harfbuzz.vcxproj" -arch $arch

    # Copy the output to the output folder
    $out = "$OUTPUT_PATH/native/uwp/$arch"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "$SKIA_PATH/out/uwp/$arch/libSkiaSharp.dll" $out -force
    Copy-Item "$HARFBUZZ_PATH/out/uwp/$arch/Release/harfbuzz.dll" $out -force
}

Function Build-MacOS-Arch ([string] $arch)
{
    WriteLine "Building the macOS library for '$arch'..."

    if ($arch -eq "x86") {
        $xcodeArch = "i386"
    } elseif ($arch -eq "x64") {
        $xcodeArch = "x86_64"
    } else {
        throw "Unknown macOS architecture: $arch"
    }

    # Build skia.a
    GnNinja -out "mac/$xcodeArch" -target "skia" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""mac\"" target_cpu=\""$arch\""
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"", \""-mmacosx-version-min=10.9\"" ]
        extra_ldflags=[ \""-Wl,macosx_version_min=10.9\"" ]
"@

    # Build libSkiaSharp.dylib
    XCodeBuild -project "native-builds/libSkiaSharp_osx/libSkiaSharp.xcodeproj" -sdk "macosx" -arch $xcodeArch

    # Build libharfbuzz.dylib
    $harfbuzzOut = Join-Path $HARFBUZZ_PATH "out/mac/$arch"
    New-Item $harfbuzzOut -itemtype "Directory" -force | Out-Null
    $config = " -D""BUILD_SHARED_LIBS=1"" -D""CMAKE_OSX_ARCHITECTURES=$xcodeArch"" -D""CMAKE_OSX_DEPLOYMENT_TARGET=10.7"" "
    Exec $cmake -a "-G ""Unix Makefiles"" $config ../../../" -wo $harfbuzzOut
    Exec $make -a "harfbuzz" -wo $harfbuzzOut

    # Copy the output to the output folder
    $out = "$OUTPUT_PATH/native/osx/$arch"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_osx/build/Release/*" $out -force
    Copy-Item "$HARFBUZZ_PATH/out/mac/$arch/libharfbuzz.dylib" $out -force

    # Strip anything we can and resign with an empty key
    StripSign -target "$out/libSkiaSharp.dylib"
    StripSign -target "$out/libharfbuzz.dylib"
}

Function Build-iOS-Arch ([string] $arch)
{
    WriteLine "Building the iOS library for '$arch'..."

    if ($arch -eq "x86") {
        $xcodeArch = "i386"
    } elseif ($arch -eq "x64") {
        $xcodeArch = "x86_64"
    } elseif ($arch -eq "arm") {
        $xcodeArch = "armv7"
    } elseif ($arch -eq "arm64") {
        $xcodeArch = "arm64"
    } else {
        throw "Unknown iOS architecture: $arch"
    }

    $sdk = $(if ($arch -like "arm*") { "iphoneos" } else { "iphonesimulator" })
    $extrasFlags = $(if ($arch -eq "arm") { ", \""-Wno-over-aligned\""" } else { "" } )

    # Build skia.a
    GnNinja -out "ios/$xcodeArch" -target "skia" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""ios\"" target_cpu=\""$arch\""
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"", \""-mios-version-min=8.0\"" $extrasFlags ]
        extra_ldflags=[ \""-Wl,ios_version_min=8.0\"" ]
"@

    # Build libSkiaSharp.framework
    XCodeBuild -project "native-builds/libSkiaSharp_ios/libSkiaSharp.xcodeproj" -sdk $sdk -arch $xcodeArch

    # Build libharfbuzz.a
    $harfbuzzOut = Join-Path $HARFBUZZ_PATH "out/ios/$arch"
    New-Item $harfbuzzOut -itemtype "Directory" -force | Out-Null
    $config = " -D""CMAKE_OSX_ARCHITECTURES=$xcodeArch"" -D""CMAKE_OSX_SYSROOT=$sdk"" -D""CMAKE_C_FLAGS=-mios-version-min=8.0"" -D""CMAKE_CXX_FLAGS=-mios-version-min=8.0"" "
    Exec $cmake -a "-G ""Unix Makefiles"" $config ../../../" -wo $harfbuzzOut
    Exec $make -a "harfbuzz" -wo $harfbuzzOut

    # Copy the output to the output folder
    $out = "$OUTPUT_PATH/native/ios/$arch"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_ios/build/Release-$sdk/libSkiaSharp.framework" $out -force -recurse
    Copy-Item "$HARFBUZZ_PATH/out/ios/$arch/libharfbuzz.a" $out -force

    # Strip anything we can and resign with an empty key
    StripSign -target "$out/libSkiaSharp.framework"
    StripSign -target "$out/libharfbuzz.a"
}

Function Build-TVOS-Arch ([string] $arch)
{
    WriteLine "Building the tvOS library for '$arch'..."

    if ($arch -eq "x64") {
        $xcodeArch = "x86_64"
    } elseif ($arch -eq "arm64") {
        $xcodeArch = "arm64"
    } else {
        throw "Unknown tvOS architecture: $arch"
    }

    $sdk = $(if ($arch -like "arm*") { "appletvos" } else { "appletvsimulator" })

    # Build skia.a
    GnNinja -out "tvos/$xcodeArch" -target "skia" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""tvos\"" target_cpu=\""$arch\""
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"", \""-DSK_BUILD_FOR_TVOS\"", \""-mtvos-version-min=9.0\"" ]
        extra_ldflags=[ \""-Wl,tvos_version_min=9.0\"" ]
"@

    # Build libSkiaSharp.framework
    XCodeBuild -project "native-builds/libSkiaSharp_tvos/libSkiaSharp.xcodeproj" -sdk $sdk -arch $xcodeArch

    # Build libharfbuzz.a
    $harfbuzzOut = Join-Path $HARFBUZZ_PATH "out/tvos/$arch"
    New-Item $harfbuzzOut -itemtype "Directory" -force | Out-Null
    $config = " -D""CMAKE_OSX_ARCHITECTURES=$xcodeArch"" -D""CMAKE_OSX_SYSROOT=$sdk"" -D""CMAKE_C_FLAGS=-mtvos-version-min=9.0"" -D""CMAKE_CXX_FLAGS=-mtvos-version-min=9.0"" "
    Exec $cmake -a "-G ""Unix Makefiles"" $config ../../../" -wo $harfbuzzOut
    Exec $make -a "harfbuzz" -wo $harfbuzzOut

    # Copy the output to the output folder
    $out = "$OUTPUT_PATH/native/tvos/$arch"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_tvos/build/Release-$sdk/libSkiaSharp.framework" $out -force -recurse
    Copy-Item "$HARFBUZZ_PATH/out/tvos/$arch/libharfbuzz.a" $out -force

    # Strip anything we can and resign with an empty key
    StripSign -target "$out/libSkiaSharp.framework"
    StripSign -target "$out/libharfbuzz.a"
}

Function Build-WatchOS-Arch ([string] $arch)
{
    WriteLine "Building the watchOS library for '$arch'..."

    if ($arch -eq "x86") {
        $xcodeArch = "i386"
    } elseif ($arch -eq "arm") {
        $xcodeArch = "armv7k"
    } else {
        throw "Unknown watchOS architecture: $arch"
    }

    $sdk = $(if ($arch -like "arm*") { "watchos" } else { "watchsimulator" })
    $extrasFlags = $(if ($arch -eq "arm") { ", \""-Wno-over-aligned\""" } else { "" } )

    # Build skia.a
    GnNinja -out "watchos/$xcodeArch" -target "skia" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""watchos\"" target_cpu=\""$arch\""
        skia_enable_gpu=false
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSK_BUILD_FOR_WATCHOS\"", \""-DSKIA_C_DLL\"", \""-mwatchos-version-min=2.0\"" $extrasFlags ]
        extra_ldflags=[ \""-Wl,watchos_version_min=2.0\"" ]
"@

    # Build libSkiaSharp.framework
    XCodeBuild -project "native-builds/libSkiaSharp_watchos/libSkiaSharp.xcodeproj" -sdk $sdk -arch $xcodeArch

    # Build libharfbuzz.a
    $harfbuzzOut = Join-Path $HARFBUZZ_PATH "out/watchos/$arch"
    New-Item $harfbuzzOut -itemtype "Directory" -force | Out-Null
    $config = " -D""CMAKE_OSX_ARCHITECTURES=$xcodeArch"" -D""CMAKE_OSX_SYSROOT=$sdk"" -D""CMAKE_C_FLAGS=-mwatchos-version-min=2.0"" -D""CMAKE_CXX_FLAGS=-mwatchos-version-min=2.0"" "
    Exec $cmake -a "-G ""Unix Makefiles"" $config ../../../" -wo $harfbuzzOut
    Exec $make -a "harfbuzz" -wo $harfbuzzOut

    # Copy the output to the output folder
    $out = "$OUTPUT_PATH/native/watchos/$arch"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_watchos/build/Release-$sdk/libSkiaSharp.framework" $out -force -recurse
    Copy-Item "$HARFBUZZ_PATH/out/watchos/$arch/libharfbuzz.a" $out -force

    # Strip anything we can and resign with an empty key
    StripSign -target "$out/libSkiaSharp.framework"
    StripSign -target "$out/libharfbuzz.a"
}

Function Build-Android-Arch ([string] $arch)
{
    WriteLine "Building the Android library for '$arch'..."

    if ($arch -eq "x86") {
        $androidArch = "x86"
    } elseif ($arch -eq "x64") {
        $androidArch = "x86_64"
    } elseif ($arch -eq "arm") {
        $androidArch = "armeabi-v7a"
    } elseif ($arch -eq "arm64") {
        $androidArch = "arm64-v8a"
    } else {
        throw "Unknown Android architecture: $arch"
    }

    $ndk_api = $(if ($arch -like "*64") { "21" } else { "9" } )

    # Build skia.a
    GnNinja -out "android/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""android\"" target_cpu=\""$arch\""
        skia_use_system_freetype2=false
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"" ]
        ndk=\""$ANDROID_NDK_HOME\""
        ndk_api=$ndk_api
"@

    # Build libharfbuzz.so
    $harfbuzzOut = Join-Path $HARFBUZZ_PATH "out/android/$arch"
    New-Item $harfbuzzOut -itemtype "Directory" -force | Out-Null
    $config = " -D""CMAKE_SYSTEM_VERSION=$ndk_api"" -D""CMAKE_ANDROID_NDK=$ANDROID_NDK_HOME"" -D""CMAKE_ANDROID_ARCH_ABI=$androidArch"" -D""CMAKE_ANDROID_STL_TYPE=gnustl_static"" "
    $config = " -D""BUILD_SHARED_LIBS=1"" -D""CMAKE_SYSTEM_NAME=Android"" $config "
    Exec $cmake -a "-G ""Unix Makefiles"" $config ../../../" -wo $harfbuzzOut
    Exec $make -a "harfbuzz" -wo $harfbuzzOut

    # Copy the output to the output folder
    $out = "$OUTPUT_PATH/native/android/$arch"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "$SKIA_PATH/out/android/$arch/libSkiaSharp.so" $out -force
    Copy-Item "$HARFBUZZ_PATH/out/android/$arch/libharfbuzz.so" $out -force
}

Function Build-Linux-Arch-SkiaSharp (
    [string] $arch,
    [string] $name = "linux",
    [bool] $gpu = $true, 
    [string] $cc = $null,
    [string] $cxx = $null,
    [string] $ar = $null,
    [string[]] $flags_asm = $null,
    [string[]] $flags_cc = $null,
    [string[]] $flags_cxx = $null,
    [string[]] $flags_ld = $null)
{
    # support for custom flags
    # include GPU support (the API still exists, but everything returns null)
    $enable_gpu = if ($gpu) { "true" } else { "false" }
    # we need the SKIA_C_DLL flag in order to keep our C API public
    $flags_c = @("-DSKIA_C_DLL")
    $flags_cc += $flags_c
    $flags_cxx += $flags_c
    # we like this option, but others might not...
    if ($flags_ld -eq $null) { $flags_ld = @("-static-libstdc++") }
    # we always want this when linking
    $flags_ld += @("-Wl,--no-undefined")
    
    # format the flags for gn/ninja
    # custom tooling
    if ($cc) { $cc = "cc=\""$cc\""" }
    if ($cxx) { $cxx = "cxx=\""$cxx\""" }
    if ($ar) { $ar = "ar=\""$ar\""" }
    # custom flags
    $flags_asm = ($flags_asm | ForEach-Object { " \""$_\"" " }) -join ', '
    $flags_cc = ($flags_cc | ForEach-Object { " \""$_\"" " }) -join ', '
    $flags_cxx = ($flags_cxx | ForEach-Object { " \""$_\"" " }) -join ', '
    $flags_ld = ($flags_ld | ForEach-Object { " \""$_\"" " }) -join ', '

    # Build libSkiaSharp.so
    GnNinja -out "$name/$arch" -skiaArgs @"
        $ar $cc $cxx
        is_official_build=true skia_enable_tools=false
        target_os=\""linux\"" target_cpu=\""$arch\""
        linux_soname_version=\""$(GetVersion "libSkiaSharp" "soname")\""
        skia_enable_gpu=$enable_gpu
        skia_use_system_freetype2=false
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_asmflags=[ $flags_asm ]
        extra_cflags=[ ]
        extra_cflags_c=[ $flags_cc ]
        extra_cflags_cc=[ $flags_cxx ]
        extra_ldflags=[ $flags_ld ]
"@

    # Copy the output to the output folder
    $out = "$OUTPUT_PATH/native/$name/$arch"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "$SKIA_PATH/out/$name/$arch/libSkiaSharp.so.$(GetVersion "libSkiaSharp" "soname")" $out -force
    Copy-Item "$SKIA_PATH/out/$name/$arch/libSkiaSharp.so.$(GetVersion "libSkiaSharp" "soname")" "$out/libSkiaSharp.so" -force
}

Function Build-Linux-Arch ([string] $arch)
{
    WriteLine "Building the Linux library for '$arch'..."

    # Build libSkiaSharp.so
    Build-Linux-Arch-SkiaSharp -arch $arch

    # Build libharfbuzz.so
    $harfbuzzOut = Join-Path $HARFBUZZ_PATH "out/linux/$arch"
    New-Item $harfbuzzOut -itemtype "Directory" -force | Out-Null
    $config = " -D""BUILD_SHARED_LIBS=1"" "
    Exec $cmake -a "-G ""Unix Makefiles"" $config ../../../" -wo $harfbuzzOut
    Exec $make -a "harfbuzz" -wo $harfbuzzOut

    # Copy the output to the output folder
    $out = "$OUTPUT_PATH/native/linux/$arch"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "$HARFBUZZ_PATH/out/linux/$arch/libharfbuzz.so" $out -force
}

################################################################################
# Script starts here...

# Initialize the tooling
InitializeTools

# Output some useful information to the screen
WriteSystemInfo

# Initialize the repositories
Initialize

# these are all the platforms supported
$AllPlatforms = @("android", "ios", "linux", "macos", "tvos", "uwp", "watchos", "windows")
if ($IsMacOS) {
    $SupportedPlatforms = @("android", "ios", "macos", "tvos", "watchos")
} elseif ($IsLinux) {
    $SupportedPlatforms = @("linux")
} else {
    $SupportedPlatforms = @("uwp", "windows")
}

# filter and clean

$Platforms = 
    $(if ($Platforms) { $Platforms } else { $AllPlatforms }) | 
    ForEach-Object {
        $plat = $_.ToLower()
        if (($plat -eq "osx") -or ($plat -eq "mac")) {
            $plat = "macos"
        } elseif (($plat -eq "win") -or ($plat -eq "win32")) {
            $plat = "windows"
        }
        return $plat
    } | 
    Select-Object -uniq  | 
    ForEach-Object { if ($_ -in $SupportedPlatforms) { $_ } }

# Build the libraries
WriteLine "$hr"
WriteLine "Building the native libraries for:"
$Platforms | ForEach-Object { WriteLine " - $_" }
WriteLine ""

if ($SkipBuild) {
    WriteLine "Skipping the native build as requested."
} else {
    # Build for macOS
    if ("macos" -in $Platforms) {
        Build-MacOS-Arch -arch "x86"
        Build-MacOS-Arch -arch "x64"
        # Create the fat files
        Lipo -dest "$OUTPUT_PATH/native/osx/libSkiaSharp.dylib" -libs @("x86", "x64")
        Lipo -dest "$OUTPUT_PATH/native/osx/libharfbuzz.dylib" -libs @("x86", "x64")
    }

    # Build for iOS
    if ("ios" -in $Platforms) {
        Build-iOS-Arch -arch "x64"
        Build-iOS-Arch -arch "x86"
        Build-iOS-Arch -arch "arm"
        Build-iOS-Arch -arch "arm64"
        # Create the fat files
        Lipo -dest "$OUTPUT_PATH/native/ios/libSkiaSharp.framework" -libs @("arm", "arm64", "x86", "x64")
        Lipo -dest "$OUTPUT_PATH/native/ios/libharfbuzz.a" -libs @("arm", "arm64", "x86", "x64")
    }

    # Build for tvOS
    if ("tvos" -in $Platforms) {
        Build-TVOS-Arch -arch "x64"
        Build-TVOS-Arch -arch "arm64"
        # Create the fat files
        Lipo -dest "$OUTPUT_PATH/native/tvos/libSkiaSharp.framework" -libs @("arm64", "x64")
        Lipo -dest "$OUTPUT_PATH/native/tvos/libharfbuzz.a" -libs @("arm64", "x64")
    }

    # Build for watchOS
    if ("watchos" -in $Platforms) {
        Build-WatchOS-Arch -arch "x86"
        Build-WatchOS-Arch -arch "arm"
        # Create the fat files
        Lipo -dest "$OUTPUT_PATH/native/watchos/libSkiaSharp.framework" -libs @("arm", "x86")
        Lipo -dest "$OUTPUT_PATH/native/watchos/libharfbuzz.a" -libs @("arm", "x86")
    }

    # Build for Android
    if ("android" -in $Platforms) {
        Build-Android-Arch -arch "x86"
        Build-Android-Arch -arch "x64"
        Build-Android-Arch -arch "arm"
        Build-Android-Arch -arch "arm64"
    }

    # Build for Linux
    if ("linux" -in $Platforms) {
        Build-Linux-Arch -arch "x64"
    }

    # Build for Windows (Win32)
    if ("windows" -in $Platforms) {
        Build-Windows-Arch -arch "x86"
        Build-Windows-Arch -arch "x64"
    }

    # Build for UWP
    if ("uwp" -in $Platforms) {
        Build-UWP-Arch -arch "x86"
        Build-UWP-Arch -arch "x64"
        Build-UWP-Arch -arch "arm"

        # copy ANGLE to output folder
        Copy-Item "$ANGLE_PATH/uwp/bin/UAP/ARM/*.dll" "$OUTPUT_PATH/native/uwp/arm"
        Copy-Item "$ANGLE_PATH/uwp/bin/UAP/Win32/*.dll" "$OUTPUT_PATH/native/uwp/x86"
        Copy-Item "$ANGLE_PATH/uwp/bin/UAP/x64/*.dll" "$OUTPUT_PATH/native/uwp/x64"
    }
    WriteLine "Build complete for the native libraries."
}

WriteLine "$hr"
WriteLine ""
