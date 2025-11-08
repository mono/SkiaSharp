# SkiaSharp

[![SkiaSharp](https://img.shields.io/nuget/vpre/SkiaSharp.svg?cacheSeconds=3600&label=SkiaSharp%20nuget)](https://www.nuget.org/packages/SkiaSharp)
[![HarfBuzzSharp](https://img.shields.io/nuget/vpre/HarfBuzzSharp.svg?cacheSeconds=3600&label=HarfBuzzSharp%20nuget)](https://www.nuget.org/packages/HarfBuzzSharp)

[![SkiaSharp.Views](https://img.shields.io/nuget/vpre/SkiaSharp.Views.svg?cacheSeconds=3600&label=SkiaSharp.Views%20nuget)](https://www.nuget.org/packages/SkiaSharp.Views)
[![SkiaSharp.Views.Maui.Controls](https://img.shields.io/nuget/vpre/SkiaSharp.Views.Maui.Controls.svg?cacheSeconds=3600&label=SkiaSharp.Views.Maui.Controls%20nuget)](https://www.nuget.org/packages/SkiaSharp.Views.Maui.Controls)
[![SkiaSharp.Views.Uno.WinUI](https://img.shields.io/nuget/vpre/SkiaSharp.Views.Uno.WinUI.svg?cacheSeconds=3600&label=SkiaSharp.Views.Uno.WinUI%20nuget)](https://www.nuget.org/packages/SkiaSharp.Views.Uno.WinUI) 

[![discord](https://img.shields.io/badge/chat-.NET%20Discord-E60256.svg)](https://aka.ms/dotnet-discord)
[![SkiaSharp API Docs](https://img.shields.io/badge/docs-skiasharp-1faece.svg)](https://docs.microsoft.com/dotnet/api/SkiaSharp)
[![HarfBuzzSharp API Docs](https://img.shields.io/badge/docs-harfbuzzsharp-1faece.svg)](https://docs.microsoft.com/dotnet/api/SkiaSharp)
[![SkiaSharp Guides](https://img.shields.io/badge/docs-guides-1faece.svg)](https://docs.microsoft.com/xamarin/graphics-games/skiasharp/)

[![Build Status](https://dev.azure.com/devdiv/DevDiv/_apis/build/status/Xamarin/Components/SkiaSharp?branchName=main)](https://dev.azure.com/devdiv/DevDiv/_build/latest?definitionId=10789&branchName=main)
[![Build Status](https://dev.azure.com/xamarin/public/_apis/build/status/mono/SkiaSharp/SkiaSharp%20(Public)?branchName=main)](https://dev.azure.com/xamarin/public/_build/latest?definitionId=4&branchName=main)

SkiaSharp is a cross-platform 2D graphics API for .NET platforms based on Google's
Skia Graphics Library ([skia.org](https://skia.org/)). It provides a comprehensive 2D API that can
be used across mobile, server and desktop models to render images.

SkiaSharp provides cross-platform bindings for:

 - .NET Standard 1.3
 - .NET Core
 - .NET 6
 - Tizen
 - Android
 - iOS
 - tvOS
 - macOS
 - Mac Catalyst
 - WinUI 3 (Windows App SDK / Uno Platform)
 - Windows Classic Desktop (Windows.Forms / WPF)
 - Web Assembly (WASM)
 - Uno Platform (iOS / macOS / Android / WebAssembly)

The [API Documentation](https://docs.microsoft.com/en-us/dotnet/api/SkiaSharp/) is
available on the web to browse.

## Using SkiaSharp

SkiaSharp is available as a convenient NuGet package, to use install the package like this:

```
nuget install SkiaSharp
```

_Because there are multiple distros of Linux, we provide a separate NuGet package with supported binaries: [SkiaSharp.NativeAssets.Linux](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Linux). See [Linux Native Assets](documentation/user-guide/linux-native-assets.md) for more information._

There is also an early access feed for the latest previews:

```
https://aka.ms/skiasharp-eap/index.json
```

## Documentation

- **[Documentation Hub](documentation/)** - Complete documentation
- **[User Guide](documentation/user-guide/)** - Using SkiaSharp
- **[Building Guide](documentation/building/)** - Building from source
- **[Contributing Guide](documentation/contributing/)** - How to contribute
- **[Design Docs](design/)** - Architecture and design
- **[API Documentation](https://docs.microsoft.com/dotnet/api/SkiaSharp)** - Online API reference

## Building SkiaSharp

Building SkiaSharp is straightforward. The main challenge is the platform-specific dependencies, but these are well documented:

- **[Building SkiaSharp](documentation/building/building-skiasharp.md)** - Complete build guide
- **[Building on Linux](documentation/building/building-on-linux.md)** - Linux-specific instructions
- **[Quick Start](design/QUICKSTART.md)** - 10-minute developer tutorial

## Contributors

<a href="https://github.com/mono/SkiaSharp/graphs/contributors">
  <img src="https://contrib.rocks/image?repo=mono/SkiaSharp" />
</a>


Made with [contrib.rocks](https://contrib.rocks).

## Compare Code

Here are some links to show the differences in our code as compared to Google's code.

What version are we on? [**m118**](https://github.com/google/skia/tree/chrome/m118)  
Are we up-to-date with Google? [Compare](https://github.com/mono/skia/compare/skiasharp...google:chrome/m118)  
What have we added? [Compare](https://github.com/google/skia/compare/chrome/m118...mono:skiasharp)  
