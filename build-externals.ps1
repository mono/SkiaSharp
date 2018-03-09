Param (
    [string[]] $Platforms = $null
)

$ErrorActionPreference = 'Stop'

# Prepare the script itself
. "./build-common.ps1"

# Paths
$SKIA_PATH = Join-Path $PSScriptRoot 'externals/skia'
$DEPOT_PATH = Join-Path $PSScriptRoot 'externals/depot_tools'
$HARFBUZZ_PATH = Join-Path $PSScriptRoot 'externals/harfbuzz'
$ANGLE_PATH = Join-Path $PSScriptRoot 'externals/angle'
$ANDROID_NDK_HOME = $env:ANDROID_NDK_HOME

# Tools
$python = FindTool 'python'
$git_sync_deps = Join-Path $SKIA_PATH 'tools/git-sync-deps'
$gn = Join-Path $SKIA_PATH $(if ($IsMacOS -or $IsLinux) { 'bin/gn' } else { 'bin/gn.exe' })
$ninja = Join-Path $DEPOT_PATH $(if ($IsMacOS -or $IsLinux) { 'ninja' } else { 'ninja.exe' })
$xcodebuild = FindTool 'xcodebuild'
$strip = FindTool 'strip'
$codesign = FindTool 'codesign'
$lipo = FindTool 'lipo'
$tar = FindTool 'tar'
$bash = FindTool 'bash'
$git = FindTool 'git'
$ndkbuild = ''
$make = FindTool 'make'

# Get tool versions
$xcodebuildVersion = ''
$pythonVersion = ''
$makeVersion = ''

# Tool helpers

Function Lipo ([string] $dest, [string[]] $libs) {
    WriteLine "Creating fat file '$dest'..."

    $dir = Split-Path -path $dest
    $name = Split-Path -path $dest -leaf

    if (Test-Path $dest) {
        Remove-Item $dest -force -recurse
    }

    if ($name.EndsWith(".framework")) {
        Copy-Item -literalpath "$dir/$($libs[0])/$name" -destination "$dest" -force -recurse
        $name = "$name/$([System.IO.Path]::GetFileNameWithoutExtension($name))"
    }

    $libs = $libs | ForEach-Object { "$_/$name" }
    Exec $lipo -a "-create -output $name $libs" -wo $dir
}

Function GnNinja ([string] $out, [string] $skiaArgs) {
    Exec $gn -a " gen out/$out -q --args=""$skiaArgs"" " -wo $SKIA_PATH
    Exec $ninja -a " -C out/$out " -wo $SKIA_PATH
}

Function XCodeBuild ([string] $project, [string] $sdk, [string] $arch) {
    WriteLine "Building '$project' as $sdk|$arch..."
    $target = [System.IO.Path]::GetFileNameWithoutExtension($project)
    $xcodebuildArgs = "-project $project -target $target -sdk $sdk -arch $arch -configuration Release -quiet"
    Exec $xcodebuild -a $xcodebuildArgs
}

Function StripSign ([string] $target) {
    if ($target.EndsWith(".framework")) {
        $archive = "$target/$([System.IO.Path]::GetFileNameWithoutExtension($target))"
    } else {
        $archive = $target
    }
    WriteLine "Stripping and signing '$target'..."
    Exec $strip -a "-x -S $archive"
    Exec $codesign -a "--force --sign - --timestamp=none $target"
}

# The main script

Function InitializeTools () {
    WriteLine "$hr"
    WriteLine "Initializing tools..."
    WriteLine ""

    # 7zip
    DownloadNuGet "7Zip4Powershell" (GetVersion "7Zip4Powershell" "release")
    Import-Module "./externals/7Zip4PowerShell/tools/7Zip4PowerShell.psd1"

    # try and find the tools
    if ($IsMacOS) {
        # find the Android NDK root
        if (!$ANDROID_NDK_HOME -or $(Test-Path $ANDROID_NDK_HOME)) {
            $script:ANDROID_NDK_HOME = Join-Path $HOME_PATH 'Library/Developer/Xamarin/android-ndk'
        }
        if (!(Test-Path $ANDROID_NDK_HOME)) {
            throw 'Unable to locate the "Android NDK home". Use the "ANDROID_NDK_HOME" environment variable.'
        }

        # find ndk-build
        $script:ndkbuild = Join-Path $ANDROID_NDK_HOME 'ndk-build'
        if (!(Test-Path $ndkbuild)) {
            throw 'Unable to locate "ndk-build". Use the "ANDROID_NDK_HOME" environment variable.'
        }

        # find xcodebuild
        if (!$xcodebuild -or !(Test-Path $xcodebuild)) {
            throw 'Unable to locate "xcodebuild". Make sure XCode and the command line tools are installed.'
        }
    } elseif ($IsLinux) {
        if (!$script:make -or !(Test-Path $script:make)) {
            throw 'Unable to locate "make". Make sure it exists in the "PATH" environment variable.'
        }
    } else {
        if (!$script:msbuild -or !(Test-Path $script:msbuild)) {
            throw 'Unable to locate "MSBuild.exe". Make sure Visual Studio 2017 is installed.'
        }
    }

    # verify that all the common tools exist
    if (!$python -or !(Test-Path $python)) {
        throw 'Unable to locate "Python". Make sure it exists in the "PATH" environment variable.'
    }

    # get the versions
    $script:pythonVersion = if ($python) { & $python --version }
    $script:xcodebuildVersion = if ($xcodebuild) { & $xcodebuild -version }
    $script:makeVersion = if ($make) { & $make -version }

    WriteLine "Tool initialization complete."
    WriteLine "$hr"
    WriteLine ""
}

Function WriteSystemInfo () {
    WriteLine "$hr"
    WriteLine "Current System:"
    WriteLine "  Operating system:    '$operatingSystem'"
    WriteLine "  PowerShell version:  '$powershellVersion'"
    WriteLine ""
    WriteLine "Tool Versions:"
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
    WriteLine "  codesign:       '$codesign'"
    WriteLine "  git:            '$git'"
    WriteLine "  git-sync-deps:  '$git_sync_deps'"
    WriteLine "  gn:             '$gn'"
    WriteLine "  lipo:           '$lipo'"
    WriteLine "  make:           '$make'"
    WriteLine "  MSBuild:        '$msbuild'"
    WriteLine "  ndk-build:      '$ndkbuild'"
    WriteLine "  ninja:          '$ninja'"
    WriteLine "  Python:         '$python'"
    WriteLine "  strip:          '$strip'"
    WriteLine "  tar:            '$tar'"
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

# Initialize the repository
Function Initialize () {
    WriteLine "$hr"
    WriteLine "Initializing repository..."
    WriteLine ""
    InitializeSkia
    InitializeHarfBuzz
    InitializeANGLE
    WriteLine "Repository initialization complete."
    WriteLine "$hr"
    WriteLine ""
}

Function InitializeSkia () {
    WriteLine "Initializing skia..."

    # sync skia dependencies
    Exec $git -a "submodule update --init --recursive"
    Exec $python -a $git_sync_deps -wo $SKIA_PATH
}

Function InitializeANGLE () {
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

Function InitializeHarfBuzz () {
    WriteLine "Initializing HarfBuzz..."

    # download harfbuzz
    $version = GetVersion "harfbuzz" "release"
    $harfbuzzBzip = Join-Path $HARFBUZZ_PATH "harfbuzz-$version.tar.bz2"
    $harfbuzzTar = Join-Path $HARFBUZZ_PATH "harfbuzz-$version.tar"
    if (!(Test-Path $harfbuzzBzip)) {
        WriteLine "Downloading HarfBuzz..."
        New-Item $HARFBUZZ_PATH -itemtype "Directory" -force | Out-Null
        Invoke-WebRequest -Uri "https://github.com/behdad/harfbuzz/releases/download/$version/harfbuzz-$version.tar.bz2" -OutFile $harfbuzzBzip
    }

    # extract harfbuzz
    $harfbuzzSource = Join-Path $HARFBUZZ_PATH "harfbuzz"
    if (!(Test-Path "$harfbuzzSource/README")) {
        WriteLine "Extracting HarfBuzz..."
        if ($IsMacOS -or $IsLinux) {
            Exec $tar -a "-xjf $harfbuzzBzip -C $HARFBUZZ_PATH"
        } else {
            Expand-7Zip $harfbuzzBzip $HARFBUZZ_PATH
            Expand-7Zip $harfbuzzTar $HARFBUZZ_PATH
        }
        Move-Item "$HARFBUZZ_PATH/harfbuzz-$version" "$harfbuzzSource"
    }

    # configure harfbuzz
    if ($IsMacOS -or $IsLinux) {
        if (!(Test-Path "$harfbuzzSource/config.h")) {
            # run ./configure
            WriteLine "Configuring HarfBuzz..."
            Exec $bash -a "configure -q" -wo $harfbuzzSource
        }
    } else {
        if (!(Test-Path "$harfbuzzSource/win32/config.h")) {
            # copy the default config header file
            WriteLine "Configuring HarfBuzz..."
            Copy-Item "$harfbuzzSource/win32/config.h.win32" "$harfbuzzSource/win32/config.h"
        }
    }
}

Function Build-Windows-Arch ([string] $arch, [string] $skiaArch, [string] $dir) {
    WriteLine "Building the Windows library for '$dir'..."

    # Build skia.lib
    GnNinja -out "win/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""win\"" target_cpu=\""$skiaArch\""
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"", \""/MD\"", \""/EHsc\"", \""/Zi\"" ]
        extra_ldflags=[ \""/DEBUG\"" ]
"@

    # Build libSkiaSharp.dll and libHarfBuzzSharp.dll
    MSBuild -project "native-builds/libHarfBuzzSharp_windows/libHarfBuzzSharp.sln" -arch $arch

    # Copy the output to the output folder
    $out = "output/native/windows/$dir"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_windows/bin/$arch/Release/*" $out -force
    Copy-Item "native-builds/libHarfBuzzSharp_windows/bin/$arch/Release/*" $out -force
}

Function Build-UWP-Arch ([string] $arch, [string] $skiaArch, [string] $dir) {
    WriteLine "Building the UWP library for '$dir'..."

    # Build skia.lib
    GnNinja -out "winrt/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""winrt\"" target_cpu=\""$skiaArch\""
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[
          \""-DSKIA_C_DLL\"", \""/MD\"", \""/EHsc\"", \""/Zi\"",
          \""-DWINAPI_FAMILY=WINAPI_FAMILY_APP\"", \""-DSK_BUILD_FOR_WINRT\"", \""-DSK_HAS_DWRITE_1_H\"", \""-DNO_GETENV\"", \""-DNO_GETENV\""
        ]
        extra_ldflags=[ \""/APPCONTAINER\"",  \""/DEBUG\"" ]
"@

    # Build libSkiaSharp.dll and libHarfBuzzSharp.dll
    MSBuild -project "native-builds/libHarfBuzzSharp_uwp/libHarfBuzzSharp.sln" -arch $arch

    # Copy the output to the output folder
    $out = "output/native/uwp/$dir"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_uwp/bin/$arch/Release/*" $out -force
    Copy-Item "native-builds/libHarfBuzzSharp_uwp/bin/$arch/Release/*" $out -force
}

Function Build-MacOS-Arch ([string] $arch, [string] $skiaArch, [string] $dir) {
    WriteLine "Building the macOS library for '$dir'..."

    # Build skia.a
    GnNinja -out "mac/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""mac\"" target_cpu=\""$skiaArch\""
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"", \""-mmacosx-version-min=10.9\"" ]
        extra_ldflags=[ \""-Wl,macosx_version_min=10.9\"" ]
"@

    # Build libSkiaSharp.dylib and libHarfBuzzSharp.dylib
    XCodeBuild -project "native-builds/libSkiaSharp_osx/libSkiaSharp.xcodeproj" -sdk "macosx" -arch $arch
    XCodeBuild -project "native-builds/libHarfBuzzSharp_osx/libHarfBuzzSharp.xcodeproj" -sdk "macosx" -arch $arch

    # Copy the output to the output folder
    $out = "output/native/osx/$dir"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_osx/build/Release/*" $out -force
    Copy-Item "native-builds/libHarfBuzzSharp_osx/build/Release/*" $out -force

    # Strip anything we can and resign with an empty key
    StripSign -target "$out/libSkiaSharp.dylib"
    StripSign -target "$out/libHarfBuzzSharp.dylib"
}

Function Build-iOS-Arch ([string] $arch, [string] $skiaArch, [string] $dir) {
    WriteLine "Building the iOS library for '$dir'..."

    $sdk = $(if ($arch.Contains("arm")) { "iphoneos" } else { "iphonesimulator" })
    $extrasFlags = $(if ($arch.StartsWith("armv7")) { ", \""-Wno-over-aligned\""" } else { "" } )

    # Build skia.a
    GnNinja -out "ios/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""ios\"" target_cpu=\""$skiaArch\""
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"", \""-mios-version-min=8.0\"" $extrasFlags ]
        extra_ldflags=[ \""-Wl,ios_version_min=8.0\"" ]
"@

    # Build libSkiaSharp.framework and libHarfBuzzSharp.a
    XCodeBuild -project "native-builds/libSkiaSharp_ios/libSkiaSharp.xcodeproj" -sdk $sdk -arch $arch
    XCodeBuild -project "native-builds/libHarfBuzzSharp_ios/libHarfBuzzSharp.xcodeproj" -sdk $sdk -arch $arch

    # Copy the output to the output folder
    $out = "output/native/ios/$dir"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_ios/build/Release-$sdk/libSkiaSharp.framework" $out -force -recurse
    Copy-Item "native-builds/libHarfBuzzSharp_ios/build/Release-$sdk/*" $out -force

    # Strip anything we can and resign with an empty key
    StripSign -target "$out/libSkiaSharp.framework"
    StripSign -target "$out/libHarfBuzzSharp.a"
}

Function Build-TVOS-Arch ([string] $arch, [string] $skiaArch, [string] $dir) {
    WriteLine "Building the tvOS library for '$dir'..."

    $sdk = $(if ($arch.Contains("arm")) { "appletvos" } else { "appletvsimulator" })

    # Build skia.a
    GnNinja -out "tvos/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""tvos\"" target_cpu=\""$skiaArch\""
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"", \""-DSK_BUILD_FOR_TVOS\"", \""-mtvos-version-min=9.0\"" ]
        extra_ldflags=[ \""-Wl,tvos_version_min=9.0\"" ]
"@

    # Build libSkiaSharp.framework and libHarfBuzzSharp.a
    XCodeBuild -project "native-builds/libSkiaSharp_tvos/libSkiaSharp.xcodeproj" -sdk $sdk -arch $arch
    XCodeBuild -project "native-builds/libHarfBuzzSharp_tvos/libHarfBuzzSharp.xcodeproj" -sdk $sdk -arch $arch

    # Copy the output to the output folder
    $out = "output/native/tvos/$dir"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_tvos/build/Release-$sdk/libSkiaSharp.framework" $out -force -recurse
    Copy-Item "native-builds/libHarfBuzzSharp_tvos/build/Release-$sdk/*" $out -force

    # Strip anything we can and resign with an empty key
    StripSign -target "$out/libSkiaSharp.framework"
    StripSign -target "$out/libHarfBuzzSharp.a"
}

Function Build-WatchOS-Arch ([string] $arch, [string] $skiaArch, [string] $dir) {
    WriteLine "Building the watchOS library for '$dir'..."

    $sdk = $(if ($arch.Contains("arm")) { "watchos" } else { "watchsimulator" })
    $extrasFlags = $(if ($arch.StartsWith("armv7")) { ", \""-Wno-over-aligned\""" } else { "" } )

    # Build skia.a
    GnNinja -out "watchos/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""watchos\"" target_cpu=\""$skiaArch\""
        skia_enable_gpu=false
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSK_BUILD_FOR_WATCHOS\"", \""-DSKIA_C_DLL\"", \""-mwatchos-version-min=2.0\"" $extrasFlags ]
        extra_ldflags=[ \""-Wl,watchos_version_min=2.0\"" ]
"@

    # Build libSkiaSharp.framework and libHarfBuzzSharp.a
    XCodeBuild -project "native-builds/libSkiaSharp_watchos/libSkiaSharp.xcodeproj" -sdk $sdk -arch $arch
    XCodeBuild -project "native-builds/libHarfBuzzSharp_watchos/libHarfBuzzSharp.xcodeproj" -sdk $sdk -arch $arch

    # Copy the output to the output folder
    $out = "output/native/watchos/$dir"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_watchos/build/Release-$sdk/libSkiaSharp.framework" $out -force -recurse
    Copy-Item "native-builds/libHarfBuzzSharp_watchos/build/Release-$sdk/*" $out -force

    # Strip anything we can and resign with an empty key
    StripSign -target "$out/libSkiaSharp.framework"
    StripSign -target "$out/libHarfBuzzSharp.a"
}

Function Build-Android-Arch ([string] $arch, [string] $skiaArch, [string] $dir) {
    WriteLine "Building the Android library for '$dir'..."

    $ndk_api = $(if ($skiaArch.EndsWith("64")) { "21" } else { "9" } )

    # Build skia.a
    GnNinja -out "android/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""android\"" target_cpu=\""$skiaArch\""
        skia_use_system_freetype2=false
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"" ]
        extra_ldflags=[ \""-Wl,watchos_version_min=2.0\"" ]
        ndk=\""$ANDROID_NDK_HOME\"" ndk_api=$ndk_api
"@
}

Function Build-Linux-Arch ([string] $arch, [string] $skiaArch, [string] $dir) {
    WriteLine "Building the Linux library for '$dir'..."

    $enable_gpu = if ($gpu) { "true" } else { "false" }
    $support_gpu = if ($gpu) { "1" } else { "0" }

    # path to fontconfig

    # Build skia.a
    GnNinja -out "linux/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""linux\"" target_cpu=\""$skiaArch\""
        skia_enable_gpu=$enable_gpu
        skia_use_system_freetype2=false
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"" ]
        extra_ldflags=[  ]
"@

    # Build libSkiaSharp.so
    Exec $make -a "ARCH=$arch SUPPORT_GPU=$support_gpu" -wo "native-builds/libSkiaSharp_linux"

    $out = "output/native/linux/$dir"
    $so = "native-builds/libSkiaSharp_linux/bin/$arch/libSkiaSharp.so.$(GetVersion "SkiaSharp" "soname")"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item $so $out -force
    Copy-Item $so "$out/libSkiaSharp.so" -force
}


# var arches = EnvironmentVariable ("BUILD_ARCH") ?? (Environment.Is64BitOperatingSystem ? "x64" : "x86");  // x64, x86, ARM
# var BUILD_ARCH = arches.Split (',').Select (a => a.Trim ()).ToArray ();
# var SUPPORT_GPU = (EnvironmentVariable ("SUPPORT_GPU") ?? "1") == "1"; // 1 == true, 0 == false

# var buildArch = new Action<string> ((arch) => {

#     // copy libSkiaSharp to output
#     EnsureDirectoryExists ("native-builds/lib/linux/" + arch);
#     var so = "native-builds/libSkiaSharp_linux/bin/" + arch + "/libSkiaSharp.so." + VERSION_SONAME;
#     CopyFileToDirectory (so, "native-builds/lib/linux/" + arch);
#     CopyFile (so, "native-builds/lib/linux/" + arch + "/libSkiaSharp.so");
# });

# var buildHarfBuzzArch = new Action<string> ((arch) => {
#     // build libHarfBuzzSharp
#     // RunProcess ("make", new ProcessSettings {
#     //     Arguments = "clean",
#     //     WorkingDirectory = "native-builds/libHarfBuzzSharp_linux",
#     // });
#     RunProcess ("make", new ProcessSettings {
#         Arguments = "ARCH=" + arch + " VERSION=" + HARFBUZZ_VERSION_FILE,
#         WorkingDirectory = "native-builds/libHarfBuzzSharp_linux",
#     });

#     // copy libHarfBuzzSharp to output
#     EnsureDirectoryExists ("native-builds/lib/linux/" + arch);
#     var so = "native-builds/libHarfBuzzSharp_linux/bin/" + arch + "/libHarfBuzzSharp.so." + HARFBUZZ_VERSION_SONAME;
#     CopyFileToDirectory (so, "native-builds/lib/linux/" + arch);
#     CopyFile (so, "native-builds/lib/linux/" + arch + "/libHarfBuzzSharp.so");
# });

# foreach (var arch in BUILD_ARCH) {
#     buildArch (arch);
#     buildHarfBuzzArch (arch);
# }

# Initialize the tooling
InitializeTools

# Output some useful information to the screen
WriteSystemInfo

# Initialize the repositories
Initialize

if (!$Platforms) {
    $Platforms = @(
        "android",
        "ios",
        "linux"
        "macos",
        "tvos",
        "uwp",
        "watchos",
        "windows"
    )
}
$Platforms = $Platforms | ForEach-Object {
    $plat = $_.ToLowerInvariant()
    if (($plat -eq "osx") -or ($plat -eq "mac")) {
        $plat = "macos"
    } elseif (($plat -eq "win") -or ($plat -eq "win32")) {
        $plat = "windows"
    }
    return $plat
} | Select -uniq

# Build the libraries
WriteLine "$hr"
WriteLine "Building the native libraries for:"
$Platforms | ForEach-Object { WriteLine " - $_" }
WriteLine ""

if ($IsMacOS) {
    # Build for macOS
    if ($Platforms.Contains("macos")) {
        Build-MacOS-Arch -arch "i386" -skiaArch "x86" -dir "x86"
        Build-MacOS-Arch -arch "x86_64" -skiaArch "x64" -dir "x64"
        # Create the fat files
        Lipo -dest "output/native/osx/libSkiaSharp.dylib" -libs @("x86", "x64")
        Lipo -dest "output/native/osx/libHarfBuzzSharp.dylib" -libs @("x86", "x64")
    }

    # Build for iOS
    if ($Platforms.Contains("ios")) {
        Build-iOS-Arch -arch "x86_64" -skiaArch "x64" -dir "x64"
        Build-iOS-Arch -arch "i386" -skiaArch "x86" -dir "x86"
        Build-iOS-Arch -arch "armv7" -skiaArch "arm" -dir "arm"
        Build-iOS-Arch -arch "arm64" -skiaArch "arm64" -dir "arm64"
        # Create the fat files
        Lipo -dest "output/native/ios/libSkiaSharp.framework" -libs @("arm", "arm64", "x86", "x64")
        Lipo -dest "output/native/ios/libHarfBuzzSharp.a" -libs @("arm", "arm64", "x86", "x64")
    }

    # Build for tvOS
    if ($Platforms.Contains("tvos")) {
        Build-TVOS-Arch -arch "x86_64" -skiaArch "x64" -dir "x64"
        Build-TVOS-Arch -arch "arm64" -skiaArch "arm64" -dir "arm64"
        # Create the fat files
        Lipo -dest "output/native/tvos/libSkiaSharp.framework" -libs @("arm64", "x64")
        Lipo -dest "output/native/tvos/libHarfBuzzSharp.a" -libs @("arm64", "x64")
    }

    # Build for watchOS
    if ($Platforms.Contains("watchos")) {
        Build-WatchOS-Arch -arch "i386" -skiaArch "x86" -dir "x86"
        Build-WatchOS-Arch -arch "armv7k" -skiaArch "arm" -dir "arm"
        # Create the fat files
        Lipo -dest "output/native/watchos/libSkiaSharp.framework" -libs @("arm", "x86")
        Lipo -dest "output/native/watchos/libHarfBuzzSharp.a" -libs @("arm", "x86")
    }

    # Build for Android
    if ($Platforms.Contains("android")) {
        Build-Android-Arch -arch "x86" -skiaArch "x86" -dir "x86"
        Build-Android-Arch -arch "x86_64" -skiaArch "x64" -dir "x86_64"
        Build-Android-Arch -arch "armeabi-v7a" -skiaArch "arm" -dir "armeabi-v7a"
        Build-Android-Arch -arch "arm64-v8a" -skiaArch "arm64" -dir "arm64-v8a"
        # build and copy libSkiaSharp
        Exec $ndkbuild -a "-C native-builds/libSkiaSharp_android"
        CopyDirectoryContents "native-builds/libSkiaSharp_android/libs" "output/native/android"
        # build and copy libHarfBuzzSharp
        Exec $ndkbuild -a "-C native-builds/libHarfBuzzSharp_android"
        CopyDirectoryContents "native-builds/libHarfBuzzSharp_android/libs" "output/native/android"
    }
} elseif ($IsLinux) {
    # Build for Linux
    # TODO: BUILD_ARCH | SUPPORT_GPU
    Build-Linux-Arch  -arch "x64" -skiaArch "x64" -dir "x64"
} else {
    # Build for Windows (Win32)
    if ($Platforms.Contains("windows")) {
        Build-Windows-Arch -arch "Win32" -skiaArch "x86" -dir "x86"
        Build-Windows-Arch -arch "x64" -skiaArch "x64" -dir "x64"
    }

    # Build for UWP
    if ($Platforms.Contains("uwp")) {
        Build-UWP-Arch -arch "Win32" -skiaArch "x86" -dir "x86"
        Build-UWP-Arch -arch "x64" -skiaArch "x64" -dir "x64"
        Build-UWP-Arch -arch "ARM" -skiaArch "arm" -dir "arm"

        # copy ANGLE to output folder
        Copy-Item "$ANGLE_PATH/uwp/bin/UAP/ARM/*.dll" "output/native/uwp/arm"
        Copy-Item "$ANGLE_PATH/uwp/bin/UAP/Win32/*.dll" "output/native/uwp/x86"
        Copy-Item "$ANGLE_PATH/uwp/bin/UAP/x64/*.dll" "output/native/uwp/x64"
    }
}
WriteLine "Build complete for the native libraries."
WriteLine "$hr"
WriteLine ""
