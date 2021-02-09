# SkiaSharp

[![SkiaSharp](https://img.shields.io/nuget/vpre/SkiaSharp.svg?maxAge=2592000&label=SkiaSharp%20nuget)](https://www.nuget.org/packages/SkiaSharp)  [![SkiaSharp.Views](https://img.shields.io/nuget/vpre/SkiaSharp.Views.svg?maxAge=2592000&label=SkiaSharp.Views%20nuget)](https://www.nuget.org/packages/SkiaSharp.Views)  [![SkiaSharp.Views.Forms](https://img.shields.io/nuget/vpre/SkiaSharp.Views.Forms.svg?maxAge=2592000&label=SkiaSharp.Views.Forms%20nuget)](https://www.nuget.org/packages/SkiaSharp.Views.Forms)  [![HarfBuzzSharp](https://img.shields.io/nuget/vpre/HarfBuzzSharp.svg?maxAge=2592000&label=HarfBuzzSharp%20nuget)](https://www.nuget.org/packages/HarfBuzzSharp)  [![SkiaSharp.Views.Uno](https://img.shields.io/nuget/vpre/SkiaSharp.Views.Uno.svg?maxAge=2592000&label=SkiaSharp.Views.Uno%20nuget)](https://www.nuget.org/packages/SkiaSharp.Views.Uno) 
[![chat](https://img.shields.io/badge/chat-xamarin%2FXamarinComponents-E60256.svg)](https://gitter.im/xamarin/XamarinComponents)  [![SkiaSharp API Docs](https://img.shields.io/badge/docs-skiasharp-1faece.svg)](https://docs.microsoft.com/dotnet/api/SkiaSharp)  [![HarfBuzzSharp API Docs](https://img.shields.io/badge/docs-harfbuzzsharp-1faece.svg)](https://docs.microsoft.com/dotnet/api/SkiaSharp)  [![SkiaSharp Guides](https://img.shields.io/badge/docs-guides-1faece.svg)](https://docs.microsoft.com/xamarin/graphics-games/skiasharp/)  
[![Build Status](https://dev.azure.com/devdiv/DevDiv/_apis/build/status/Xamarin/Components/SkiaSharp?branchName=main)](https://dev.azure.com/devdiv/DevDiv/_build/latest?definitionId=10789&branchName=main)  [![Build Status](https://dev.azure.com/xamarin/public/_apis/build/status/mono/SkiaSharp/SkiaSharp%20(Public)?branchName=main)](https://dev.azure.com/xamarin/public/_build/latest?definitionId=4&branchName=main)

SkiaSharp is a cross-platform 2D graphics API for .NET platforms based on Google's
Skia Graphics Library ([skia.org](https://skia.org/)). It provides a comprehensive 2D API that can
be used across mobile, server and desktop models to render images.

SkiaSharp provides cross-platform bindings for:

 - .NET Standard 1.3
 - .NET Core
 - Tizen
 - Xamarin.Android
 - Xamarin.iOS
 - Xamarin.tvOS
 - Xamarin.watchOS
 - Xamarin.Mac
 - Windows Classic Desktop (Windows.Forms / WPF)
 - Windows UWP (Desktop / Mobile / Xbox / HoloLens)
 - Web Assembly (WASM)
 - Uno Platform (iOS / macOS / Android / WebAssembly / UWP)

The [API Documentation](https://docs.microsoft.com/en-us/dotnet/api/SkiaSharp/) is
available on the web to browse.

## Using SkiaSharp

SkiaSharp is available as a convenient NuGet package, to use install the package like this:

```
nuget install SkiaSharp
```

_Because there are multiple distros of Linux, and we cannot possibly support them all, we have a separate NuGet package that will contain the supported binaries for a few distros: [SkiaSharp.NativeAssets.Linux](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Linux). ([distros](https://github.com/mono/SkiaSharp/issues/453)) ([more info](https://github.com/mono/SkiaSharp/issues/312))_

There is also a early access feed that you can use to get the latest and greatest, before it goes out to the public:

```
https://aka.ms/skiasharp-eap/index.json
```

## Building SkiaSharp

Building SkiaSharp is mostly straight forward. The main issue is the multiple dependencies for each platform.

However, these are easy to install as they are found on the various websites. If you are just working on managed code, it is even easier as there mays to skip all the native builds.

 - To get started building, [go here](https://github.com/mono/SkiaSharp/wiki/Building-SkiaSharp).
 - If you are just wanting a custom Linux build, [go here](https://github.com/mono/SkiaSharp/wiki/Building-on-Linux)

## Compare Code

Here are some links to show the differences in our code as compared to Google's code.

What version are we on? [**m80**](https://github.com/google/skia/tree/chrome/m80)  
Are we up-to-date with Google? [Compare](https://github.com/mono/skia/compare/xamarin-mobile-bindings...google:chrome/m80)  
What have we added? [Compare](https://github.com/google/skia/compare/chrome/m80...mono:xamarin-mobile-bindings)  
