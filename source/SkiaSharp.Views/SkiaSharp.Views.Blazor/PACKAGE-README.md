# SkiaSharp.Views.Blazor

[![NuGet](https://img.shields.io/nuget/v/SkiaSharp.Views.Blazor?style=flat-square&label=NuGet)](https://www.nuget.org/packages/SkiaSharp.Views.Blazor)
[![NuGet downloads](https://img.shields.io/nuget/dt/SkiaSharp.Views.Blazor?style=flat-square&label=Downloads)](https://www.nuget.org/packages/SkiaSharp.Views.Blazor)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://github.com/mono/SkiaSharp/blob/main/LICENSE.md)

**SkiaSharp.Views.Blazor** brings SkiaSharp to Blazor WebAssembly with ready-to-use Razor components. Draw with the same SkiaSharp API used by native .NET applications while the package handles the browser canvas, JavaScript interop, input events, and WebAssembly native assets.

## What you get

- `SKCanvasView` for CPU-rendered, on-demand drawing to an HTML canvas.
- `SKGLView` for WebGL-backed rendering and continuous animation.
- Pointer, wheel, and touch input events.
- Density-aware sizing and explicit invalidation.
- Transitive WebAssembly native assets and build integration.

## Get started

Install the package into a Blazor WebAssembly project:

```bash
dotnet add package SkiaSharp.Views.Blazor
```

Add the namespaces to `_Imports.razor`:

```razor
@using SkiaSharp
@using SkiaSharp.Views.Blazor
```

Add a canvas to a Razor component:

```razor
<SKCanvasView OnPaintSurface="OnPaintSurface"
              IgnorePixelScaling="true" />

@code {
    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);

        using var paint = new SKPaint
        {
            Color = SKColors.CornflowerBlue,
            IsAntialias = true,
        };

        canvas.DrawCircle(
            e.Info.Width / 2f,
            e.Info.Height / 2f,
            Math.Min(e.Info.Width, e.Info.Height) * 0.3f,
            paint);
    }
}
```

The package supplies its browser interop automatically. Use .NET 8 or later for a complete WebAssembly application with the supported native assets. For animated or GPU-heavy scenes, switch to `SKGLView` and enable its render loop.

## See it running

- [Live Blazor WebAssembly Gallery](https://mono.github.io/SkiaSharp/gallery/) - interactive SkiaSharp examples with no installation
- [Buildable Blazor WebAssembly sample](https://github.com/mono/SkiaSharp/tree/main/samples/Basic/BlazorWebAssembly)
- [SkiaFiddle](https://mono.github.io/SkiaSharp/fiddle/) - experiment with drawing code in your browser

## Documentation and resources

- [SkiaSharp documentation](https://mono.github.io/SkiaSharp/docs/)
- [Blazor API reference on Microsoft Learn](https://learn.microsoft.com/dotnet/api/skiasharp.views.blazor)
- [Core SkiaSharp API reference](https://learn.microsoft.com/dotnet/api/skiasharp)
- [WebAssembly package guidance](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md)

## Feedback and contributing

SkiaSharp is an open-source Microsoft project built with the .NET community. Use [GitHub Discussions](https://github.com/mono/SkiaSharp/discussions) for questions, file bugs and feature requests in the [issue tracker](https://github.com/mono/SkiaSharp/issues), and read the [contributing guide](https://github.com/mono/SkiaSharp/blob/main/CONTRIBUTING.md) to get involved.

This package is released under the [MIT license](https://github.com/mono/SkiaSharp/blob/main/LICENSE.md).
