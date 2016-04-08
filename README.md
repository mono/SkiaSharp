# SkiaSharp

SkiaSharp is a cross-platform 2D graphics API for .NET platforms based on Google's
Skia Graphics Library (https://skia.org/).   It provides a comprehensive 2D API that can
be used across mobile, server and desktop models to render images.

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

You need Python 2.7 in `PATH` environment variable. Then you need to run following commands from `skia` directory:

    > ..\depot_tools\gclient.bat config --unmanaged https://github.com/mono/skia.git
    > ..\depot_tools\gclient.bat sync

The process will take some time while gclient downloads Skia build dependencies.

Then you can finally build it:

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
 - `clean` - cleans everything
   - `clean-externals` - cleans externals only

## Where is Windows Phone / Store / tvOS
 
We are working to add binaries for these platforms, stay tuned for a future release
(or check the pull requests and branches, where we are working on those)
