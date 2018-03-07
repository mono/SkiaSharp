$ErrorActionPreference = 'Stop'

# Paths
$SKIA_PATH = Join-Path $PSScriptRoot 'externals/skia'
$DEPOT_PATH = Join-Path $PSScriptRoot 'externals/depot_tools'

# Tools
$where = $(if ($IsMacOS -or $IsLinux) { 'which' } else { 'where' })
$python = & $where 'python'
$git_sync_deps = Join-Path $SKIA_PATH 'tools/git-sync-deps'
$gn = Join-Path $SKIA_PATH $(if ($IsMacOS -or $IsLinux) { 'bin/gn' } else { 'bin/gn.exe' })
$ninja = Join-Path $DEPOT_PATH $(if ($IsMacOS -or $IsLinux) { 'ninja' } else { 'ninja.exe' })
$xcodebuild = & $where 'xcodebuild'
$msbuild = & $where 'msbuild'
if (!$IsMacOS) {
    $vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    $msbuild = Join-Path $msbuild 'MSBuild\15.0\Bin\MSBuild.exe'
}
$strip = & $where 'strip'
$codesign = & $where 'codesign'
$lipo = & $where 'lipo'

# Get tool versions
$powershellVersion = "$($PSVersionTable.PSVersion.ToString()) ($($PSVersionTable.PSEdition.ToString()))"
$msbuildVersion = & $msbuild -version -nologo
$operatingSystem = if ($IsMacOS) { 'macOS' } elseif ($IsLinux) { 'Linux' } else { 'Windows' }
$xcodebuildVersion = if ($IsMacOS) { & $xcodebuild -version } else { 'not supported' }

function WriteSystemInfo () {
    $fc = $host.UI.RawUI.ForegroundColor
    $host.UI.RawUI.ForegroundColor = "Cyan"

    Write-Host ""
    Write-Host "Current System:"
    Write-Host "  Operating system: '$operatingSystem'"
    Write-Host "  PowerShell version: '$powershellVersion'"
    Write-Host ""
    Write-Host "Tool Versions:"
    Write-Host "  MSBuild version: '$msbuildVersion'"
    Write-Host "  XCode version: '$xcodebuildVersion'"
    Write-Host ""
    Write-Host "Tool Paths:"
    Write-Host "  Python: '$python'"
    Write-Host "  git-sync-deps: '$git_sync_deps'"
    Write-Host "  gn: '$gn'"
    Write-Host "  ninja: '$ninja'"
    Write-Host "  XCodeBuild: '$xcodebuild'"
    Write-Host "  MSBuild: '$msbuild'"
    Write-Host "  strip: '$strip'"
    Write-Host "  codesign: '$codesign'"
    Write-Host "  lipo: '$lipo'"
    Write-Host ""
    Write-Host "Other Paths:"
    Write-Host "  SKIA_PATH: '$SKIA_PATH'"
    Write-Host "  DEPOT_PATH: '$DEPOT_PATH'"
    Write-Host ""

    $host.UI.RawUI.ForegroundColor = $fc
}

function Lipo ([string] $dest, [string[]] $libs) {
    $name = Split-Path -path $dest -leaf
    if ($name.EndsWith(".framework")) {
        $name = "$name/$([System.IO.Path]::GetFileNameWithoutExtension($name)))"
    }
    $dir = Split-Path -path $dest
    $libs = $libs | ForEach-Object { "$_/$name" }

    Write-Output "Creating fat file '$name' from $libs..."
    if ($name.EndsWith(".framework")) {
        Copy-Item $libs[0] $dest -force -recurse
    }
    if (Test-Path $dest) {
        Remove-Item $dest -force -recurse
    }
    Start-Process $lipo -args "-create -output $name $libs" -wo $dir -nnw -wait
}

function GnNinja ([string] $out, [string] $args) {
    Write-Output "Building the native library to '$out'..."
    Start-Process $gn -args " gen out/$out -q --args=""$args"" " -wo $SKIA_PATH -nnw -wait
    Start-Process $ninja -args " -C out/$out " -wo $SKIA_PATH -nnw -wait
}

function XCodeBuild ([string] $project, [string] $sdk, [string] $arch) {
    Write-Output "Building '$project' as $sdk|$arch..."
    $target = [System.IO.Path]::GetFileNameWithoutExtension($project)
    $xcodebuildArgs = "-project $project -target $target -sdk $sdk -arch $arch -configuration Release -quiet"
    Start-Process $xcodebuild -args $xcodebuildArgs -nnw -wait
}

function StripSign ([string] $target) {
    if ($target.EndsWith(".framework")) {
        $archive = "$target/$([System.IO.Path]::GetFileNameWithoutExtension($target)))"
    } else {
        $archive = $target
    }
    Write-Output "Signing '$target' or '$archive'..."
    Start-Process $strip -args "-x -S $archive" -nnw -wait
    Start-Process $codesign -args "--force --sign - --timestamp=none $target" -nnw -wait
}

# Initialize the repository
function Initialize () {
    Write-Output "Initializing repository..."
    Start-Process $python -args $git_sync_deps -wo $SKIA_PATH -nnw -wait
}

Function BuildWindows ([string] $arch, [string] $skiaArch, [string] $dir) {
    Write-Output "Building the Windows library for '$dir'..."

    # Build skia.lib
    $args =
        " is_official_build=true skia_enable_tools=false " + 
        " target_os=\""win\"" target_cpu=\""$skiaArch\"" " +
        " skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true " +
        " skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
        " extra_cflags=[ \""-DSKIA_C_DLL\"", \""/MD\"", \""/EHsc\"", \""/Zi\"" ] " +
        " extra_ldflags=[ \""/DEBUG\"" ] "
    Start-Process $gn -args " gen out/win/$arch --args=""$args"" " -wo $SKIA_PATH -nnw -wait
    Start-Process $ninja -args " -C out/win/$arch " -wo $SKIA_PATH -nnw -wait

    # Build libSkiaSharp.dll
    $msbuildArgs = 
        " native-builds/libSkiaSharp_windows/libSkiaSharp.sln " +
        " /p:Configuration=Release /p:Platform=$arch /v:minimal "
    Start-Process $msbuild -args $msbuildArgs -nnw -wait

    # Build libHarfBuzzSharp.dll
    $msbuildArgs = 
        " native-builds/libHarfBuzzSharp_windows/libHarfBuzzSharp.sln " +
        " /p:Configuration=Release /p:Platform=$arch /v:minimal "
    Start-Process $msbuild -args $msbuildArgs -nnw -wait

    # Copy the output to the output folder
    $out = "output/native/windows/$dir"
    New-Item $out -ItemType "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_windows/bin/$arch/Release/*" $out -force
    Copy-Item "native-builds/libHarfBuzzSharp_windows/bin/$arch/Release/*" $out -force
}

Function BuildUWP ([string] $arch, [string] $skiaArch, [string] $dir) {
    Write-Output "Building the UWP library for '$dir'..."

    # Build skia.lib
    $args =
        " is_official_build=true skia_enable_tools=false " + 
        " target_os=\""winrt\"" target_cpu=\""$skiaArch\"" " +
        " skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true " +
        " skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
        " extra_cflags=[ " +
        "   \""-DSKIA_C_DLL\"", \""/MD\"", \""/EHsc\"", \""/Zi\"", " + 
        "   \""-DWINAPI_FAMILY=WINAPI_FAMILY_APP\"", \""-DSK_BUILD_FOR_WINRT\"", \""-DSK_HAS_DWRITE_1_H\"", \""-DNO_GETENV\"", \""-DNO_GETENV\"" " +
        "   ] " +
        " extra_ldflags=[ \""/APPCONTAINER\"",  \""/DEBUG\"" ] "
    Start-Process $gn -args " gen out/winrt/$arch --args=""$args"" " -wo $SKIA_PATH -nnw -wait
    Start-Process $ninja -args " -C out/winrt/$arch " -wo $SKIA_PATH -nnw -wait

    # Build libSkiaSharp.dll
    $msbuildArgs = 
        " native-builds/libSkiaSharp_uwp/libSkiaSharp.sln " +
        " /p:Configuration=Release /p:Platform=$arch /v:minimal "
    Start-Process $msbuild -args $msbuildArgs -nnw -wait

    # Build libHarfBuzzSharp.dll
    $msbuildArgs = 
        " native-builds/libHarfBuzzSharp_uwp/libHarfBuzzSharp.sln " +
        " /p:Configuration=Release /p:Platform=$arch /v:minimal "
    Start-Process $msbuild -args $msbuildArgs -nnw -wait

    # Copy the output to the output folder
    $out = "output/native/uwp/$dir"
    New-Item $out -ItemType "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_uwp/bin/$arch/Release/*" $out -force
    Copy-Item "native-builds/libHarfBuzzSharp_uwp/bin/$arch/Release/*" $out -force
}

Function BuildMac ([string] $arch, [string] $skiaArch, [string] $dir) {
    Write-Output "Building the macOS library for '$dir'..."

    # Build skia.a
    $args =
        " is_official_build=true skia_enable_tools=false " + 
        " target_os=\""mac\"" target_cpu=\""$skiaArch\"" " +
        " skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true " +
        " skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
        " extra_cflags=[ \""-DSKIA_C_DLL\"", \""-mmacosx-version-min=10.9\"" ] " +
        " extra_ldflags=[ \""-Wl,macosx_version_min=10.9\"" ] "
    GnNinja -out "mac/$arch" -args $args

    # Build libSkiaSharp.dylib and libHarfBuzzSharp.dylib
    XCodeBuild -project "native-builds/libSkiaSharp_osx/libSkiaSharp.xcodeproj" -sdk "macosx" -arch $arch
    XCodeBuild -project "native-builds/libHarfBuzzSharp_osx/libHarfBuzzSharp.xcodeproj" -sdk "macosx" -arch $arch

    # Copy the output to the output folder
    $out = "output/native/osx/$dir"
    New-Item $out -ItemType "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_osx/build/Release/*" $out -force
    Copy-Item "native-builds/libHarfBuzzSharp_osx/build/Release/*" $out -force

    # Strip anything we can and resign with an empty key
    StripSign -target "$out/libSkiaSharp.dylib"
    StripSign -target "$out/libHarfBuzzSharp.dylib"
}

Function BuildIos ([string] $arch, [string] $skiaArch, [string] $dir) {
    Write-Output "Building the iOS library for '$dir'..."

    $sdk = $(if ($arch.Contains("arm")) { "iphoneos" } else { "iphonesimulator" })
    $extrasFlags = $(if ($arch.StartsWith("armv7")) { ", \""-Wno-over-aligned\""" } else { "" } )

    # Build skia.a
    $args =
        " is_official_build=true skia_enable_tools=false " + 
        " target_os=\""ios\"" target_cpu=\""$skiaArch\"" " +
        " skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true " +
        " skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
        " extra_cflags=[ \""-DSKIA_C_DLL\"", \""-mios-version-min=8.0\"" $extrasFlags ] " +
        " extra_ldflags=[ \""-Wl,ios_version_min=8.0\"" ] "
    GnNinja -out "ios/$arch" -args $args

    # Build libSkiaSharp.framework and libHarfBuzzSharp.a
    XCodeBuild -project "native-builds/libSkiaSharp_ios/libSkiaSharp.xcodeproj" -sdk $sdk -arch $arch
    XCodeBuild -project "native-builds/libHarfBuzzSharp_ios/libHarfBuzzSharp.xcodeproj" -sdk $sdk -arch $arch

    # Copy the output to the output folder
    $out = "output/native/ios/$dir"
    New-Item $out -ItemType "Directory" -force | Out-Null
    Copy-Item "native-builds/libSkiaSharp_ios/build/Release-$sdk/libSkiaSharp.framework" $out -force -recurse
    Copy-Item "native-builds/libHarfBuzzSharp_ios/build/Release-$sdk/*" $out -force

    # Strip anything we can and resign with an empty key
    StripSign -target "$out/libSkiaSharp.framework"
    StripSign -target "$out/libHarfBuzzSharp.a"
}

# Output some useful information to the screen
WriteSystemInfo

# Initialize the repositories
Initialize

# Build the libraries
if ($IsMacOS) {
    # Build for macOS
    BuildMac -arch "i386" -skiaArch "x86" -dir "x86"
    # BuildMac -arch "x86_64" -skiaArch "x64" -dir "x64"
    # # Create the fat files
    # Lipo -dest "output/native/osx/libSkiaSharp.dylib" -libs @("x86", "x64")
    # Lipo -dest "output/native/osx/libHarfBuzzSharp.dylib" -libs @("x86", "x64")

    # # Build for iOS
    # BuildIos -arch "x86_64" -skiaArch "x64" -dir "x64"
    # BuildIos -arch "i386" -skiaArch "x86" -dir "x86"
    # BuildIos -arch "armv7" -skiaArch "arm" -dir "arm"
    # BuildIos -arch "arm64" -skiaArch "arm64" -dir "arm64"
    # # Create the fat files
    # Lipo -dest "output/native/ios/libSkiaSharp.framework" -libs @("arm", "armv64", "x86", "x64")

    # # Build for tvOS

    # # Build for watchOS

    # # Build for Android
} elseif ($IsLinux) {
    # Build for Linux
} else {
    # Build for Windows (Win32)
    BuildWindows -arch "Win32" -skiaArch "x86" -dir "x86"
    BuildWindows -arch "x64" -skiaArch "x64" -dir "x64"

    # Build for UWP
    BuildUWP -arch "Win32" -skiaArch "x86" -dir "x86"
    BuildUWP -arch "x64" -skiaArch "x64" -dir "x64"
    BuildUWP -arch "ARM" -skiaArch "arm" -dir "ARM"
}
