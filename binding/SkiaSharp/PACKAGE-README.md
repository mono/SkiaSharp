# SkiaSharp

[![NuGet](https://img.shields.io/nuget/v/SkiaSharp?style=flat-square&label=NuGet)](https://www.nuget.org/packages/SkiaSharp)
[![NuGet downloads](https://img.shields.io/nuget/dt/SkiaSharp?style=flat-square&label=Downloads)](https://www.nuget.org/packages/SkiaSharp)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)]({{RepositoryUrl}}/blob/main/LICENSE.md)

**SkiaSharp** is a cross-platform 2D graphics API for .NET powered by Google's [Skia](https://skia.org/) graphics engine. Draw vector graphics, render text, process images, apply shaders and filters, and produce consistent output across mobile, desktop, server, and WebAssembly applications.

## What you can build

- **Rich 2D graphics** - paths, shapes, text, images, color spaces, shaders, filters, blend modes, and more.
- **Cross-platform** - Windows, macOS, Linux, Android, iOS, Mac Catalyst, tvOS, Tizen, and WebAssembly.
- **CPU and GPU rendering** - software rasterization plus OpenGL, Metal, Vulkan, and Direct3D integrations.
- **Broad output support** - render to pixels, encode common image formats, create PDFs, or generate SVG.
- **Native performance with an idiomatic C# API** - the same Skia engine used by Chrome, Android, Flutter, and many other products.
- **Open source** - developed in the open by Microsoft and the .NET community under the MIT license.

## Get started

Install the package:

```bash
dotnet add package SkiaSharp
```

Create an image and save it as a PNG:

```csharp
using SkiaSharp;

var info = new SKImageInfo(640, 360);
using var surface = SKSurface.Create(info);
var canvas = surface.Canvas;

canvas.Clear(new SKColor(0x0B, 0x10, 0x20));

using var shader = SKShader.CreateLinearGradient(
    new SKPoint(0, 0),
    new SKPoint(info.Width, info.Height),
    new[] { new SKColor(0x44, 0x88, 0xFF), new SKColor(0xB8, 0x44, 0xE8) },
    SKShaderTileMode.Clamp);
using var paint = new SKPaint
{
    IsAntialias = true,
    Shader = shader,
};

canvas.DrawCircle(info.Width / 2f, info.Height / 2f, 120, paint);

using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Png, 100);
using var output = File.Create("hello-skiasharp.png");
data.SaveTo(output);
```

> **Linux:** add [SkiaSharp.NativeAssets.Linux](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Linux) to the application project for fontconfig integration, or [SkiaSharp.NativeAssets.Linux.NoDependencies](https://www.nuget.org/packages/SkiaSharp.NativeAssets.Linux.NoDependencies) for minimal containers and explicit font loading. See the [package deployment guide]({{RepositoryUrl}}/blob/main/documentation/dev/packages.md) for details.

## Choose the right integration

| Scenario | Package |
| --- | --- |
| Draw into bitmaps, images, surfaces, PDFs, and SVG | `SkiaSharp` |
| .NET MAUI controls | [`SkiaSharp.Views.Maui.Controls`](https://www.nuget.org/packages/SkiaSharp.Views.Maui.Controls) |
| Blazor WebAssembly components | [`SkiaSharp.Views.Blazor`](https://www.nuget.org/packages/SkiaSharp.Views.Blazor) |
| Android, iOS, Mac Catalyst, macOS, tvOS, and Tizen native views | [`SkiaSharp.Views`](https://www.nuget.org/packages/SkiaSharp.Views) |
| WPF, Windows Forms, WinUI, or GTK controls | Use the matching `SkiaSharp.Views.*` package |
| Complex text shaping | [`SkiaSharp.HarfBuzz`](https://www.nuget.org/packages/SkiaSharp.HarfBuzz) |
| Lottie animation playback | [`SkiaSharp.Skottie`](https://www.nuget.org/packages/SkiaSharp.Skottie) |

## Learn and explore

- [SkiaSharp documentation]({{DocumentationUrl}}/docs/) - conceptual guides and tutorials
- [API reference on Microsoft Learn](https://learn.microsoft.com/dotnet/api/skiasharp)
- [Live SkiaSharp Gallery]({{DocumentationUrl}}/gallery/) - interactive samples running in Blazor WebAssembly
- [SkiaFiddle]({{DocumentationUrl}}/fiddle/) - experiment with SkiaSharp in your browser
- [Samples]({{RepositoryUrl}}/tree/main/samples) - buildable console, mobile, desktop, web, and UI examples
- [Release notes]({{DocumentationUrl}}/docs/releases/) - new features and API changes

## Feedback and contributing

SkiaSharp is built in the open. Use [GitHub Discussions]({{RepositoryUrl}}/discussions) for questions, file bugs and feature requests in the [issue tracker]({{RepositoryUrl}}/issues), and read the [contributing guide]({{RepositoryUrl}}/blob/main/CONTRIBUTING.md) to help improve the project.

SkiaSharp is released under the [MIT license]({{RepositoryUrl}}/blob/main/LICENSE.md).
