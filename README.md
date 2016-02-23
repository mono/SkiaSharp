# SkiaSharp

SkiaSharp is a cross-platform, managed binding for the 
Skia Graphics Library (https://skia.org/)

## What is Included

SkiaSharp provides a PCL and platform-specific bindings for:

 - Mac OS X
 - Xamarin.Android
 - Xamarin.iOS
 - Windows Desktop
 - Mac Desktop

You can also build this on your particular variant of Unix
to create your native libraries.

## Using SkiaSharp

Check our getting [started guide](https://developer.xamarin.com/guides/cross-platform/drawing/)

## Building SkiaSharp

To build SkiaSharp on Mac OS X (Bash):

    $ ./bootstrapper.sh -t libs

Similarly on Windows (PowerShell):

    > .\bootstrapper.ps1 -Target libs

There are several targets available:

 - `externals` - builds all the native libraries
   - [win] `externals-windows` - builds the native libraries for Windows
   - [mac] `externals-osx` - builds the native libraries for Mac OS X
   - [mac] `externals-ios` - builds the native libraries for iOS
   - [mac] `externals-andoid` - builds the native libraries for Android
 - `libs` - builds all the managed libraries
   - [win] `libs-windows` - builds the managed libraries that can be built on Windows
   - [mac] `libs-osx` - builds the managed libraries that can be built on Mac OS X
 - `tests` - builds and runs the tests
 - `samples` - builds the samples available for the current platform
 - `docs` - updates the mdoc files
 - `nuget` - packages the libraries into a NuGet
 - `CI` - builds everything

## Where is Windows Phone / Store
 
At this time, Windows Phone and Windows Store apps are not 
supported. This is due to the native library not supporting 
those platforms: 

 - https://bugs.chromium.org/p/skia/issues/detail?id=2059
 - https://groups.google.com/forum/#!searchin/skia-discuss/windows$20phone/skia-discuss/VHRCLl-XV8E/YpUKZr4OVKgJ
 - https://groups.google.com/forum/#!searchin/skia-discuss/windows$208/skia-discuss/FF4-KzRRDp8/S0Uoy1f0waIJ
