Param (
    [string[]] $Platforms = $Null,
    [string] $HarfBuzzVersion = "1.4.6",
    [string] $ANGLEVersion = "2.1.13"
)

$ErrorActionPreference = 'Stop'

Add-Type -assembly "System.IO.Compression.FileSystem"

# Paths
$SKIA_PATH = Join-Path $PSScriptRoot 'externals/skia'
$DEPOT_PATH = Join-Path $PSScriptRoot 'externals/depot_tools'
$HARFBUZZ_PATH = Join-Path $PSScriptRoot 'externals/harfbuzz'
$ANGLE_PATH = Join-Path $PSScriptRoot 'externals/angle'
$HOME_PATH = $(if ($IsMacOS -or $IsLinux) { $env:HOME } else { $env:USERPROFILE })
$ANDROID_NDK_HOME = $env:ANDROID_NDK_HOME
if (!$ANDROID_NDK_HOME -or $(Test-Path $ANDROID_NDK_HOME)) {
    $ANDROID_NDK_HOME = Join-Path $HOME_PATH 'Library/Developer/Xamarin/android-ndk'
}

# Tools
$where = $(if ($IsMacOS -or $IsLinux) { 'which' } else { "$env:SystemRoot\system32\where.exe" })
$python = & $where 'python'
$git_sync_deps = Join-Path $SKIA_PATH 'tools/git-sync-deps'
$gn = Join-Path $SKIA_PATH $(if ($IsMacOS -or $IsLinux) { 'bin/gn' } else { 'bin/gn.exe' })
$ninja = Join-Path $DEPOT_PATH $(if ($IsMacOS -or $IsLinux) { 'ninja' } else { 'ninja.exe' })
$xcodebuild = & $where 'xcodebuild'
$msbuild = & $where 'msbuild'
if (!$IsMacOS) {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    $msbuild = Join-Path $msbuild 'MSBuild\15.0\Bin\MSBuild.exe'
}
$strip = & $where 'strip'
$codesign = & $where 'codesign'
$lipo = & $where 'lipo'
$tar = & $where 'tar'
$bash = & $where 'bash'
$ndkbuild = Join-Path $ANDROID_NDK_HOME 'ndk-build'

# Get tool versions
$powershellVersion = "$($PSVersionTable.PSVersion.ToString()) ($($PSVersionTable.PSEdition.ToString()))"
$msbuildVersion = & $msbuild -version -nologo
$operatingSystem = if ($IsMacOS) { 'macOS' } elseif ($IsLinux) { 'Linux' } else { 'Windows' }
$xcodebuildVersion = if ($IsMacOS) { & $xcodebuild -version } else { 'not supported' }

# Utility functions

function Copy-Dir ([string] $src, [string] $dest) {
    New-Item $dest -itemtype "Directory" -force | Out-Null
    Get-ChildItem $src -Directory | ForEach-Object {
        Copy-Item -literalpath "$src/$_" $dest -force -recurse | Out-Null
    }
}

function Exec ([string] $file, [string[]] $a, [string] $wo) {
    if (!$wo) {
        $wo = "."
    }
    $process = Start-Process $file -args $a -wo $wo -nnw -wait -passthru
    if ($process.ExitCode -ne 0) {
        throw "Process '$file' exited with error code '$($process.ExitCode.ToString())'." 
    }
}

# Tool helpers

function Lipo ([string] $dest, [string[]] $libs) {
    Write-Output "Creating fat file '$dest'..."

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

function GnNinja ([string] $out, [string] $skiaArgs) {
    Write-Output "Building the native library to '$out'..."
    Exec $gn -a " gen out/$out -q --args=""$skiaArgs"" " -wo $SKIA_PATH
    Exec $ninja -a " -C out/$out " -wo $SKIA_PATH
}

function XCodeBuild ([string] $project, [string] $sdk, [string] $arch) {
    Write-Output "Building '$project' as $sdk|$arch..."
    $target = [System.IO.Path]::GetFileNameWithoutExtension($project)
    $xcodebuildArgs = "-project $project -target $target -sdk $sdk -arch $arch -configuration Release -quiet"
    Exec $xcodebuild -a $xcodebuildArgs
}

function StripSign ([string] $target) {
    if ($target.EndsWith(".framework")) {
        $archive = "$target/$([System.IO.Path]::GetFileNameWithoutExtension($target))"
    } else {
        $archive = $target
    }
    Write-Output "Stripping and signing '$target'..."
    Exec $strip -a "-x -S $archive"
    Exec $codesign -a "--force --sign - --timestamp=none $target"
}

# The main script

function WriteSystemInfo () {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = "Cyan"

    Write-Output ""
    Write-Output "Current System:"
    Write-Output "  Operating system:    '$operatingSystem'"
    Write-Output "  PowerShell version:  '$powershellVersion'"
    Write-Output ""
    Write-Output "Tool Versions:"
    Write-Output "  MSBuild version:  '$msbuildVersion'"
    Write-Output "  XCode version:    '$xcodebuildVersion'"
    Write-Output ""
    Write-Output "Other Versions:"
    Write-Output "  ANGLE version:     '$ANGLEVersion'"
    Write-Output "  HarfBuzz version:  '$HarfBuzzVersion'"
    Write-Output ""
    Write-Output "Tool Paths:"
    Write-Output "  bash:           '$bash'"
    Write-Output "  codesign:       '$codesign'"
    Write-Output "  git-sync-deps:  '$git_sync_deps'"
    Write-Output "  gn:             '$gn'"
    Write-Output "  lipo:           '$lipo'"
    Write-Output "  MSBuild:        '$msbuild'"
    Write-Output "  ndk-build:      '$ndkbuild'"
    Write-Output "  ninja:          '$ninja'"
    Write-Output "  Python:         '$python'"
    Write-Output "  strip:          '$strip'"
    Write-Output "  tar:            '$tar'"
    Write-Output "  where/which:    '$where'"
    Write-Output "  XCodeBuild:     '$xcodebuild'"
    Write-Output ""
    Write-Output "Other Paths:"
    Write-Output "  ANGLE_PATH:       '$ANGLE_PATH'"
    Write-Output "  DEPOT_PATH:       '$DEPOT_PATH'"
    Write-Output "  HARFBUZZ_PATH:    '$HARFBUZZ_PATH'"
    Write-Output "  SKIA_PATH:        '$SKIA_PATH'"
    Write-Output "  ANDROID_NDK_HOME: '$ANDROID_NDK_HOME'"
    Write-Output ""

    $host.UI.RawUI.ForegroundColor = $fc
}

# Initialize the repository
function Initialize () {
    Write-Output "Initializing repository..."
    InitializeSkia
    InitializeHarfBuzz
    InitializeANGLE
    Write-Output "Repository initialization complete."
    Write-Output ""
}

function InitializeSkia () {
    Write-Output "Initializing skia..."

    # sync skia dependencies
    Exec $python -a $git_sync_deps -wo $SKIA_PATH
}

function InitializeANGLE () {
    Write-Output "Initializing ANGLE..."

    # download ANGLE
    $angleRoot = Join-Path $ANGLE_PATH "uwp"
    $angleZip = Join-Path $angleRoot "ANGLE.WindowsStore.$ANGLEVersion.nupkg"
    if (!(Test-Path $angleZip)) {
        Write-Output "Downloading ANGLE..."
        New-Item $angleRoot -itemtype "Directory" -force | Out-Null
        Invoke-WebRequest -Uri "https://www.nuget.org/api/v2/package/ANGLE.WindowsStore/$ANGLEVersion" -OutFile $angleZip
    }
    
    # extract ANGLE
    if (!(Test-Path "$angleRoot/ANGLE.WindowsStore.nuspec")) {
        Write-Output "Extracting ANGLE..."
        [System.IO.Compression.ZipFile]::ExtractToDirectory($angleZip, $angleRoot)
    }
}

function InitializeHarfBuzz () {
    Write-Output "Initializing HarfBuzz..."

    # download harfbuzz
    $harfbuzzZip = Join-Path $HARFBUZZ_PATH "harfbuzz-$HarfBuzzVersion.tar.bz2"
    if (!(Test-Path $harfbuzzZip)) {
        Write-Output "Downloading HarfBuzz..."
        New-Item $HARFBUZZ_PATH -itemtype "Directory" -force | Out-Null
        Invoke-WebRequest -Uri "https://github.com/behdad/harfbuzz/releases/download/$HarfBuzzVersion/harfbuzz-$HarfBuzzVersion.tar.bz2" -OutFile $harfbuzzZip
    }
    
    # extract harfbuzz
    $harfbuzzSource = Join-Path $HARFBUZZ_PATH "harfbuzz"
    if (!(Test-Path "$harfbuzzSource/README")) {
        Write-Output "Extracting HarfBuzz..."
        if ($IsMacOS -or $IsLinux) {
            Exec $tar -a "-xjf $harfbuzzZip -C $HARFBUZZ_PATH"
        } else {
            throw 'TODO: Unzipping .tar.bz2 needs to be implemented.'
        }
        Move-Item "$HARFBUZZ_PATH/harfbuzz-$HarfBuzzVersion" "$harfbuzzSource"
    }

    # configure harfbuzz
    if ($IsMacOS -or $IsLinux) {
        if (!(Test-Path "$harfbuzzSource/config.h")) {
            # run ./configure
            Write-Output "Configuring HarfBuzz..."
            Exec $bash -a "configure -q" -wo $harfbuzzSource
        }
    } else {
        if (!(Test-Path "$harfbuzzSource/win32/config.h")) {
            # copy the default config header file
            Write-Output "Configuring HarfBuzz..."
            Copy-Item "$harfbuzzSource/win32/config.h.win32" "$harfbuzzSource/win32/config.h"
        }
    }
}

Function Build-Windows-Arch ([string] $arch, [string] $skiaArch, [string] $dir) {
    Write-Output "Building the Windows library for '$dir'..."

    # Build skia.lib
    GnNinja -out "win/$arch" -skiaArgs @"
        is_official_build=true skia_enable_tools=false
        target_os=\""win\"" target_cpu=\""$skiaArch\""
        skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true
        skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false
        extra_cflags=[ \""-DSKIA_C_DLL\"", \""/MD\"", \""/EHsc\"", \""/Zi\"" ]
        extra_ldflags=[ \""/DEBUG\"" ]
"@

    # Build libSkiaSharp.dll
    Exec $msbuild -a @"
        native-builds/libSkiaSharp_windows/libSkiaSharp.sln
        /p:Configuration=Release /p:Platform=$arch /v:minimal
"@

    # Build libHarfBuzzSharp.dll
    Exec $msbuild -a @"
        native-builds/libHarfBuzzSharp_windows/libHarfBuzzSharp.sln
        /p:Configuration=Release /p:Platform=$arch /v:minimal
"@

    # Copy the output to the output folder
    $out = "output/native/windows/$dir"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_windows/bin/$arch/Release/*" $out -force
    Copy-Item "native-builds/libHarfBuzzSharp_windows/bin/$arch/Release/*" $out -force
}

Function Build-UWP-Arch ([string] $arch, [string] $skiaArch, [string] $dir) {
    Write-Output "Building the UWP library for '$dir'..."

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

    # Build libSkiaSharp.dll
    Exec $msbuild -a @"
        native-builds/libSkiaSharp_uwp/libSkiaSharp.sln
        /p:Configuration=Release /p:Platform=$arch /v:minimal
"@

    # Build libHarfBuzzSharp.dll
    Exec $msbuild -a @"
        native-builds/libHarfBuzzSharp_uwp/libHarfBuzzSharp.sln
        /p:Configuration=Release /p:Platform=$arch /v:minimal
"@

    # Copy the output to the output folder
    $out = "output/native/uwp/$dir"
    New-Item $out -itemtype "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_uwp/bin/$arch/Release/*" $out -force
    Copy-Item "native-builds/libHarfBuzzSharp_uwp/bin/$arch/Release/*" $out -force
}

Function Build-MacOS-Arch ([string] $arch, [string] $skiaArch, [string] $dir) {
    Write-Output "Building the macOS library for '$dir'..."

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
    Write-Output "Building the iOS library for '$dir'..."

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
    Write-Output "Building the tvOS library for '$dir'..."

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
    Write-Output "Building the watchOS library for '$dir'..."

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
    Write-Output "Building the Android library for '$dir'..."

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
} else {
    $Platforms = $Platforms | ForEach-Object { $_.ToLowerInvariant() }
}

# Build the libraries
if ($IsMacOS) {
    # Build for macOS
    if ($Platforms.Contains("macos") -or $Platforms.Contains("osx") -or $Platforms.Contains("mac")) {
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
        Copy-Dir "native-builds/libSkiaSharp_android/libs" "output/native/android"
        # build and copy libHarfBuzzSharp
        Exec $ndkbuild -a "-C native-builds/libHarfBuzzSharp_android"
        Copy-Dir "native-builds/libHarfBuzzSharp_android/libs" "output/native/android"
    }
} elseif ($IsLinux) {
    # Build for Linux
} else {
    # Build for Windows (Win32)
    if ($Platforms.Contains("windows") -or $Platforms.Contains("win")) {
        Build-Windows-Arch -arch "Win32" -skiaArch "x86" -dir "x86"
        Build-Windows-Arch -arch "x64" -skiaArch "x64" -dir "x64"
    }

    # Build for UWP
    if ($Platforms.Contains("uwp")) {
        Build-UWP-Arch -arch "Win32" -skiaArch "x86" -dir "x86"
        Build-UWP-Arch -arch "x64" -skiaArch "x64" -dir "x64"
        Build-UWP-Arch -arch "ARM" -skiaArch "arm" -dir "ARM"
    }
}
