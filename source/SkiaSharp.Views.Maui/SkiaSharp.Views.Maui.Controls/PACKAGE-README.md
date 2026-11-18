# SkiaSharp.Views.Maui.Controls

[![NuGet](https://img.shields.io/nuget/v/SkiaSharp.Views.Maui.Controls?style=flat-square&label=NuGet)](https://www.nuget.org/packages/SkiaSharp.Views.Maui.Controls)
[![NuGet downloads](https://img.shields.io/nuget/dt/SkiaSharp.Views.Maui.Controls?style=flat-square&label=Downloads)](https://www.nuget.org/packages/SkiaSharp.Views.Maui.Controls)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)](https://github.com/mono/SkiaSharp/blob/main/LICENSE.md)

**SkiaSharp.Views.Maui.Controls** adds SkiaSharp rendering controls and image sources to .NET MAUI applications. Put a CPU- or GPU-backed canvas alongside standard MAUI controls, draw with the full SkiaSharp API, and share the same rendering code across Android, iOS, Mac Catalyst, and Windows.

## What you get

- `SKCanvasView` for CPU-rendered, on-demand drawing.
- `SKGaneshView` for engine-named GPU drawing on Android, iOS, Mac Catalyst and Windows.
- `SKGraphiteView` for explicit Graphite drawing on iOS, Mac Catalyst and compatible Android devices; Windows presentation is unsupported.
- The unchanged `SKGLView` for existing applications.
- `SKImageImageSource`, `SKBitmapImageSource`, `SKPixmapImageSource`, and `SKPictureImageSource` for MAUI images.
- XAML support, data binding, touch input, invalidation, and density-aware canvas sizing.
- Native handlers for Android, iOS, Mac Catalyst, and Windows.

## Get started

Install the package:

```bash
dotnet add package SkiaSharp.Views.Maui.Controls
```

Register SkiaSharp in `MauiProgram.cs`:

```csharp
using SkiaSharp.Views.Maui.Controls.Hosting;

public static MauiApp CreateMauiApp() =>
    MauiApp
        .CreateBuilder()
        .UseMauiApp<App>()
        .UseSkiaSharp()
        .Build();
```

Add a canvas to a XAML page:

```xml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:skia="clr-namespace:SkiaSharp.Views.Maui.Controls;assembly=SkiaSharp.Views.Maui.Controls">

    <skia:SKCanvasView PaintSurface="OnPaintSurface" />

</ContentPage>
```

Draw in the `PaintSurface` handler:

```csharp
using SkiaSharp;
using SkiaSharp.Views.Maui;

private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    canvas.Clear(SKColors.White);

    using var paint = new SKPaint
    {
        Color = SKColors.CornflowerBlue,
        IsAntialias = true,
    };

    canvas.DrawCircle(e.Info.Width / 2f, e.Info.Height / 2f, 120, paint);
}
```

Call `InvalidateSurface()` whenever state changes and the view should redraw. Use `SKGaneshView` for a GPU renderer with a replaceable private backend. Its typed paint metadata and Graphite's derive from `SKPaintSurfaceEventArgs`, so the drawing handler above works with either engine. Subscribe to context lifecycle and `RenderFailed` events; GPU resources are borrowed and tied to a context generation. See the repository's [engine-view ownership and threading guide](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/gpu-engine-views.md).

## Documentation and resources

- [SkiaSharp .NET MAUI guides](https://mono.github.io/SkiaSharp/docs/guides/)
- [API reference on Microsoft Learn](https://learn.microsoft.com/dotnet/api/skiasharp.views.maui.controls)
- [Buildable .NET MAUI sample](https://github.com/mono/SkiaSharp/tree/main/samples/Basic/Maui)
- [Live SkiaSharp Gallery](https://mono.github.io/SkiaSharp/gallery/) - explore the rendering API in your browser
- [SkiaSharp package and deployment guide](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md)

## Feedback and contributing

SkiaSharp is an open-source Microsoft project built with the .NET community. Use [GitHub Discussions](https://github.com/mono/SkiaSharp/discussions) for questions, file bugs and feature requests in the [issue tracker](https://github.com/mono/SkiaSharp/issues), and read the [contributing guide](https://github.com/mono/SkiaSharp/blob/main/CONTRIBUTING.md) to get involved.

This package is released under the [MIT license](https://github.com/mono/SkiaSharp/blob/main/LICENSE.md).
