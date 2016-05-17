# SkiaSharp

[![Gitter.im](https://img.shields.io/badge/gitter.im-xamarin%2FXamarinComponents-E60256.svg)](https://gitter.im/xamarin/XamarinComponents)  [![NuGet](https://img.shields.io/nuget/v/SkiaSharp.svg?maxAge=2592000)](https://www.nuget.org/packages/SkiaSharp)  [![NuGet Pre Release](https://img.shields.io/nuget/vpre/SkiaSharp.svg?maxAge=2592000)](https://www.nuget.org/packages/SkiaSharp)

SkiaSharp is a cross-platform 2D graphics API for .NET platforms based on Google's
Skia Graphics Library (https://skia.org/).   It provides a comprehensive 2D API that can
be used across mobile, server and desktop models to render images.

## What is Included

SkiaSharp provides a PCL and platform-specific bindings for:

 - Mac OS X
 - Xamarin.Android
 - Xamarin.iOS
 - Windows Desktop
 - Windows UWP
 - Mac Desktop

You can also build this on your particular variant of Unix
to create your native libraries.

## Using SkiaSharp

Check our getting [started guide](https://developer.xamarin.com/guides/cross-platform/drawing/)

## Building SkiaSharp

First clone the repository:

    $ git clone https://github.com/mono/SkiaSharp.git

Next, set up the submodules:

    $ cd SkiaSharp
    $ git submodule init && git submodule update
    
Then follow the platform-specific instructions below.

### Mac OS X

Run from Bash

    $ ./bootstrapper.sh -t libs

### Windows

You need Python 2.7 in `PATH` environment variable. Then you can build it:

    > .\bootstrapper.ps1 -Target libs

### Build Targets

There are several targets available:

 - `externals` - builds all the native libraries
   - [win] `externals-windows` - builds the native libraries for Windows
   - [win] `externals-uwp` - builds the native libraries for Windows UWP
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
 - `clean` - cleans everything
   - `clean-externals` - cleans externals only

## Compare Code

Here are some links to show the differences in our code as compared to Google's.

What version are we on? **m49**  
Are we up-to-date with Google? [Compare](https://github.com/google/skia/compare/chrome/m49...mono:xamarin-mobile-bindings)  
What have we added? [Compare](https://github.com/mono/skia/compare/xamarin-mobile-bindings...google:chrome/m49)  

## Where is Windows Phone 8 / Store 8 / tvOS
 
We are working to add binaries for these platforms, stay tuned for a future release
(or check the pull requests and branches, where we are working on those)
