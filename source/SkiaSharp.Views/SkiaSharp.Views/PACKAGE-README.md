# SkiaSharp.Views

[![NuGet](https://img.shields.io/nuget/v/SkiaSharp.Views?style=flat-square&label=NuGet)](https://www.nuget.org/packages/SkiaSharp.Views)
[![NuGet downloads](https://img.shields.io/nuget/dt/SkiaSharp.Views?style=flat-square&label=Downloads)](https://www.nuget.org/packages/SkiaSharp.Views)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg?style=flat-square)]({{RepositoryUrl}}/blob/main/LICENSE.md)

**SkiaSharp.Views** provides native SkiaSharp drawing views for Android, iOS, Mac Catalyst, macOS, tvOS, and Tizen applications. Each view owns the platform rendering surface and raises a familiar `PaintSurface` event so application code can draw with `SKCanvas`.

## What you get

- `SKCanvasView` for CPU-rendered, on-demand drawing.
- GPU-backed views using OpenGL, Metal, or the platform's accelerated rendering surface.
- Touch and input helpers, density-aware sizing, and explicit invalidation.
- Platform-native view types that fit directly into each UI framework.

## Get started

Install the package into a native platform application:

```bash
dotnet add package SkiaSharp.Views
```

Create the platform `SKCanvasView` and handle `PaintSurface`:

```csharp
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

The namespace and native base type vary by target:

| Target | Namespace | Primary view |
| --- | --- | --- |
| Android | `SkiaSharp.Views.Android` | `SKCanvasView` |
| iOS and Mac Catalyst | `SkiaSharp.Views.iOS` | `SKCanvasView` |
| macOS | `SkiaSharp.Views.Mac` | `SKCanvasView` |
| tvOS | `SkiaSharp.Views.tvOS` | `SKCanvasView` |
| Tizen | `SkiaSharp.Views.Tizen` | `SKCanvasView` |

## Using another UI framework?

Choose the package that matches the application framework:

| Framework | Package |
| --- | --- |
| .NET MAUI | [`SkiaSharp.Views.Maui.Controls`](https://www.nuget.org/packages/SkiaSharp.Views.Maui.Controls) |
| Blazor WebAssembly | [`SkiaSharp.Views.Blazor`](https://www.nuget.org/packages/SkiaSharp.Views.Blazor) |
| Windows Forms | [`SkiaSharp.Views.WindowsForms`](https://www.nuget.org/packages/SkiaSharp.Views.WindowsForms) |
| WPF | [`SkiaSharp.Views.WPF`](https://www.nuget.org/packages/SkiaSharp.Views.WPF) |
| WinUI 3 | [`SkiaSharp.Views.WinUI`](https://www.nuget.org/packages/SkiaSharp.Views.WinUI) |
| GTK 3 or GTK 4 | [`SkiaSharp.Views.Gtk3`](https://www.nuget.org/packages/SkiaSharp.Views.Gtk3) or [`SkiaSharp.Views.Gtk4`](https://www.nuget.org/packages/SkiaSharp.Views.Gtk4) |
| Uno Platform | [`SkiaSharp.Views.Uno.WinUI`](https://www.nuget.org/packages/SkiaSharp.Views.Uno.WinUI) |

## Documentation and resources

- [SkiaSharp documentation]({{DocumentationUrl}}/docs/)
- Platform API reference on Microsoft Learn: [Android](https://learn.microsoft.com/dotnet/api/skiasharp.views.android), [iOS and Mac Catalyst](https://learn.microsoft.com/dotnet/api/skiasharp.views.ios), [macOS](https://learn.microsoft.com/dotnet/api/skiasharp.views.mac), and [Tizen](https://learn.microsoft.com/dotnet/api/skiasharp.views.tizen)
- [Platform samples]({{RepositoryUrl}}/tree/main/samples/Basic)
- [Live SkiaSharp Gallery]({{DocumentationUrl}}/gallery/)
- [Package selection and deployment guide]({{RepositoryUrl}}/blob/main/documentation/dev/packages.md)

## Feedback and contributing

SkiaSharp is an open-source Microsoft project built with the .NET community. Use [GitHub Discussions]({{RepositoryUrl}}/discussions) for questions, file bugs and feature requests in the [issue tracker]({{RepositoryUrl}}/issues), and read the [contributing guide]({{RepositoryUrl}}/blob/main/CONTRIBUTING.md) to get involved.

This package is released under the [MIT license]({{RepositoryUrl}}/blob/main/LICENSE.md).
