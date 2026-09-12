---
title: "Render with SkiaSharp in .NET MAUI"
description: "Register and use SkiaSharp.Views.Maui.Controls raster and GPU views across Android, iOS, Mac Catalyst, and Windows."
---

# Render with SkiaSharp in .NET MAUI

The `SkiaSharp.Views.Maui.Controls` package provides two cross-platform controls:

| Control | Surface | Paint event |
| --- | --- | --- |
| `SKCanvasView` | CPU raster | `PaintSurface` with `SKPaintSurfaceEventArgs` |
| `SKGLView` | Ganesh GPU | `PaintSurface` with `SKPaintGLSurfaceEventArgs` |

Start with `SKCanvasView`. Choose `SKGLView` only after checking the backend and support level on every target your app ships.

## Register the handlers

Call `UseSkiaSharp()` when creating the MAUI app. Without this call, MAUI does not register the handlers for the controls:

```csharp
using SkiaSharp.Views.Maui.Controls.Hosting;

public static MauiApp CreateMauiApp() =>
    MauiApp
        .CreateBuilder()
        .UseMauiApp<App>()
        .UseSkiaSharp()
        .Build();
```

## Add a view

Declare the controls from the `SkiaSharp.Views.Maui.Controls` namespace:

```xml
<ContentPage
    xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
    xmlns:skia="clr-namespace:SkiaSharp.Views.Maui.Controls;assembly=SkiaSharp.Views.Maui.Controls">
    <skia:SKCanvasView PaintSurface="OnPaintSurface" />
</ContentPage>
```

The handler receives `SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs`. Draw on `e.Surface.Canvas`, then let the control present the result.

Use `InvalidateSurface()` when application state changes and the control needs another frame. For `SKGLView`, set `HasRenderLoop` only while continuous rendering is required; otherwise invalidate on demand.

## Check the platform backend

The public MAUI controls remain the same, but their handlers use different native views:

| Target | `SKCanvasView` handler | `SKGLView` handler |
| --- | --- | --- |
| Android | Native raster `SKCanvasView` | `SKGLTextureView` with OpenGL ES |
| iOS | Native raster `SKCanvasView` | Native `SKGLView` with OpenGL ES |
| Mac Catalyst | Native raster `SKCanvasView` | Native `SKMetalView` with Metal |
| Windows | `SKXamlCanvas` | `SKSwapChainPanel` with ANGLE/OpenGL ES |

The current MAUI projects target Android, iOS, Mac Catalyst, and Windows. They do not target tvOS, macOS, or Tizen.

On iOS, the `SKGLView` handler is marked obsolete starting with iOS 12 because it uses OpenGL ES. The MAUI package does not expose a separate `SKMetalView` control for iOS. Use `SKCanvasView` or provide a native/custom Metal integration when an iOS GPU path is required.

On Mac Catalyst, the MAUI `SKGLView` handler uses Metal internally, but the callback type is still the MAUI `SKPaintGLSurfaceEventArgs`.

## Related links

- [Choose a SkiaSharp view](index.md)
- [Android views](android.md)
- [Apple platform views](apple.md)
- [Windows views](windows.md)
- [Integrating with .NET MAUI](../../basics/integration.md)
