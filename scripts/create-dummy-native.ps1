function Dummy {
    param ([string] $RootPath, [string[]] $Platforms, [string[]] $FileNames)

    foreach ($platform in $Platforms) {
        foreach ($filename in $FileNames) {
            New-Item -ItemType File -Force -Path "$RootPath/$platform/$filename" | Out-Null
        }
    }
}

Dummy output/native/android @('arm64-v8a', 'armeabi-v7a', 'x86', 'x86_64') @(
    'libSkiaSharp.so',
    'libHarfBuzzSharp.so')

Dummy output/native/ios @('.') @(
    'libHarfBuzzSharp.a',
    'libSkiaSharp.framework/libSkiaSharp')

Dummy output/native/linux @('x64') @(
    'libSkiaSharp.so',
    'libHarfBuzzSharp.so')

Dummy output/native/osx @('.') @(
    'libHarfBuzzSharp.dylib',
    'libSkiaSharp.dylib')

Dummy output/native/tizen @('armel', 'i386') @(
    'libSkiaSharp.so',
    'libHarfBuzzSharp.so')

Dummy output/native/tvos @('.') @(
    'libHarfBuzzSharp.a',
    'libSkiaSharp.framework/libSkiaSharp')

Dummy output/native/uwp @('arm', 'x86', 'x64') @(
    'libSkiaSharp.dll',
    'libHarfBuzzSharp.dll',
    'SkiaSharp.Views.Interop.UWP.dll',
    'libEGL.dll',
    'libGLESv2.dll',
    'zlib1.dll')

Dummy output/native/watchos @('.') @(
    'libHarfBuzzSharp.a',
    'libSkiaSharp.framework/libSkiaSharp')

Dummy output/native/windows @('x86', 'x64') @(
    'libSkiaSharp.dll',
    'libHarfBuzzSharp.dll')
