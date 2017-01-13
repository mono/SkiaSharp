# SkiaSharp

[![SkiaSharp](https://img.shields.io/nuget/vpre/SkiaSharp.svg?maxAge=2592000&label=SkiaSharp%20nuget)](https://www.nuget.org/packages/SkiaSharp)  [![SkiaSharp.Views](https://img.shields.io/nuget/vpre/SkiaSharp.Views.svg?maxAge=2592000&label=SkiaSharp.Views%20nuget)](https://www.nuget.org/packages/SkiaSharp.Views)  [![SkiaSharp.Views.Forms](https://img.shields.io/nuget/vpre/SkiaSharp.Views.Forms.svg?maxAge=2592000&label=SkiaSharp.Views.Forms%20nuget)](https://www.nuget.org/packages/SkiaSharp.Views.Forms)  
[![Gitter.im](https://img.shields.io/badge/gitter.im-xamarin%2FXamarinComponents-E60256.svg)](https://gitter.im/xamarin/XamarinComponents)  [![Xamarin Forums](https://img.shields.io/badge/forums-Graphics%20%26%20Games%2FSkiaSharp-1faece.svg)](https://forums.xamarin.com/categories/skiasharp)  
[![API Docs](https://img.shields.io/badge/docs-api-1faece.svg)](https://developer.xamarin.com/api/root/SkiaSharp/)  [![API Docs](https://img.shields.io/badge/docs-guides-1faece.svg)](https://developer.xamarin.com/guides/cross-platform/drawing/)  

SkiaSharp is a cross-platform 2D graphics API for .NET platforms based on Google's
Skia Graphics Library (https://skia.org/).   It provides a comprehensive 2D API that can
be used across mobile, server and desktop models to render images.

## What is Included

SkiaSharp provides a PCL and platform-specific bindings for:

 - Mac (Console or using the Xamarin.Mac)
 - Xamarin.Android
 - Xamarin.iOS
 - Xamarin.tvOS
 - Xamarin.Mac
 - Windows Desktop (Windows.Forms / WPF)
 - Windows UWP

You can also build this on your particular variant of Unix
to create your native libraries.

## Using SkiaSharp

SkiaSharp is available as a convenience NuGet package, to use install the package like this:

```
nuget install SkiaSharp
```

Our [getting started guide](https://developer.xamarin.com/guides/cross-platform/drawing/) will walk you 
through both the basic setup as well as the platform specific capabilties.

The [API Documentation](https://developer.xamarin.com/api/namespace/SkiaSharp/) is also available on the
web to browse.

### Prerequisites

Make sure the [Microsoft Visual C++ 2015 Redistributable](https://www.microsoft.com/en-us/download/details.aspx?id=52982) is installed if this error occurs: 
 > Unable to load DLL 'libSkiaSharp.dll': The specified module could not be found.

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

This runs the build process by using the `libs` build target.

### Windows

You need Python 2.7 in `PATH` environment variable. Then you can build it:

    > .\bootstrapper.ps1 -Target libs

This runs the build process by using the `libs` build target.

### Build Targets

There are several targets available, you can specify the target as the argument to the `-t` command line
option in the bootstrapper script.

 - `Everything` - builds everything for the current platform
 - `externals` - builds all the native libraries
   - [win] `externals-windows` - builds the native libraries for Windows
   - [win] `externals-uwp` - builds the native libraries for Windows UWP
   - [mac] `externals-osx` - builds the native libraries for Mac OS X
   - [mac] `externals-ios` - builds the native libraries for iOS
   - [mac] `externals-tvos` - builds the native libraries for tvOS
   - [mac] `externals-andoid` - builds the native libraries for Android
 - `libs` - builds all the managed libraries
   - [win] `libs-windows` - builds the managed libraries that can be built on Windows
   - [mac] `libs-osx` - builds the managed libraries that can be built on Mac OS X
 - `tests` - builds and runs the tests
 - `samples` - builds the samples available for the current platform
 - `docs` - updates the mdoc files
 - `nuget` - packages the libraries into a NuGet
 - `clean` - cleans everything
   - `clean-externals` - cleans externals only
   - `clean-managed` - cleans managed libraries/samples only

## New Skia Features Roadmap

Google has created a [nice doc with a collection of high level items](https://docs.google.com/document/d/1C9w8qpPpdgNGThqmgNnTToLZ5UYK4TsUGl5X3B_q6oM/edit)
they have on tap the next 6-12 months. Note it is a living document that changes based on the requirements of the library's users.

## Compare Code

Here are some links to show the differences in our code as compared to Google's.

What version are we on? [**m56**](https://github.com/google/skia/tree/chrome/m56)  
Are we up-to-date with Google? [Compare](https://github.com/mono/skia/compare/xamarin-mobile-bindings...google:chrome/m56)  
What have we added? [Compare](https://github.com/google/skia/compare/chrome/m56...mono:xamarin-mobile-bindings)  

## Where is Windows Phone 8 / Store 8
 
We are working to add binaries for these platforms, stay tuned for a future release
(or check the pull requests and branches, where we are working on those)
