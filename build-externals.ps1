$ErrorActionPreference = 'Stop'

# Paths
$SKIA_PATH = Join-Path $PSScriptRoot 'externals/skia'
$DEPOT_PATH = Join-Path $PSScriptRoot 'externals/depot_tools'

# Tools
$python = 'python'
$git_sync_deps = Join-Path $SKIA_PATH 'tools/git-sync-deps'
$gn = Join-Path $SKIA_PATH 'bin/gn.exe'
$ninja = Join-Path $DEPOT_PATH 'ninja.exe'
$msbuild = 'msbuild'
if (!$IsMacOS) {
    $vswhere = 'C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe'
    $msbuild = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath
    $msbuild = Join-Path $msbuild 'MSBuild\15.0\Bin\MSBuild.exe'
}

# Get tool versions
$powershellVersion = $PSVersionTable.PSVersion.ToString();
$msbuildVersion = & $msbuild -version -nologo

# Initialize the repository
function Initialize () {
    Start-Process $python -args $git_sync_deps -wo $SKIA_PATH -nnw -wait
}

Function BuildWindows ([string] $arch, [string] $skiaArch, [string] $dir) {
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
    Copy-Item "native-builds/libSkiaSharp_windows/bin/$arch/Release/*" $out
    Copy-Item "native-builds/libHarfBuzzSharp_windows/bin/$arch/Release/*" $out
}

Function BuildUWP ([string] $arch, [string] $skiaArch, [string] $dir) {
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
    Copy-Item "native-builds/libSkiaSharp_uwp/bin/$arch/Release/*" $out
    Copy-Item "native-builds/libHarfBuzzSharp_uwp/bin/$arch/Release/*" $out
}

Write-Host "Running PowerShell version: '$powershellVersion'" -ForegroundColor Cyan
Write-Host "Running MSBuild version: '$msbuildVersion'" -ForegroundColor Cyan

if ($IsMacOS)
{
}
elseif ($IsLinux)
{
}
else
{
    BuildWindows -arch "Win32" -skiaArch "x86" -dir "x86"
    BuildWindows -arch "x64" -skiaArch "x64" -dir "x64"

    BuildUWP -arch "Win32" -skiaArch "x86" -dir "x86"
    BuildUWP -arch "x64" -skiaArch "x64" -dir "x64"
    BuildUWP -arch "ARM" -skiaArch "arm" -dir "ARM"
}