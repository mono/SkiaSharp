---
title: "Choose a SkiaSharp view"
description: "Choose a raster or Ganesh-backed SkiaSharp view for .NET MAUI, native platforms, Uno Platform, or Blazor WebAssembly."
---

# Choose a SkiaSharp view

SkiaSharp view controls create the drawing surface, size it to the control, and provide its canvas through a paint callback. Use a view when drawing belongs in an app UI and you do not need to create, wrap, present, or dispose the [`SKSurface`](xref:SkiaSharp.SKSurface) yourself.

View controls use one of two rendering paths:

| View family | Typical controls | Rendering path | Use when |
| --- | --- | --- | --- |
| Raster | `SKCanvasView`, `SKXamlCanvas`, `SKElement`, `SKControl`, `SKDrawingArea` | CPU [raster surface](../raster/index.md), then platform presentation | You want the broadest support or do not need continuous GPU rendering |
| GPU | `SKGLView`, `SKMetalView`, `SKGLSurfaceView`, `SKGLTextureView`, `SKSwapChainPanel`, `SKGLElement`, `SKGLControl` | [Ganesh](../ganesh/index.md) over OpenGL, OpenGL ES, Metal, ANGLE, or WebGL | You render continuously or have a workload that benefits from a GPU-backed target |

> [!NOTE]
> The shipped view controls use raster surfaces or Ganesh. They do not drive [Graphite](../graphite/index.md). Graphite remains a manually managed rendering path.
>
> `SkiaSharp.Views.Tizen.NUI.SKGLSurfaceView` is an exception to the control naming pattern: it raises raster `SKPaintSurfaceEventArgs`, not Ganesh-backed `SKPaintGLSurfaceEventArgs`. See [Tizen views](tizen.md).

## Draw in the paint callback

Raster controls raise or invoke a callback with `SKPaintSurfaceEventArgs`. Draw on the provided surface and do not retain its canvas after the callback returns:

```csharp
void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    using var paint = new SKPaint
    {
        IsAntialias = true,
        Color = SKColors.CornflowerBlue,
    };

    canvas.Clear(SKColors.White);
    canvas.DrawCircle(e.Info.Width / 2f, e.Info.Height / 2f, 100, paint);
}
```

GL-, ANGLE-, and WebGL-backed controls use `SKPaintGLSurfaceEventArgs`. Native Apple Metal controls use `SKPaintMetalSurfaceEventArgs`. All of these event arguments expose an `SKSurface`; the drawing code still starts with `e.Surface.Canvas`.

The .NET MAUI `SKGLView` always exposes the MAUI `SKPaintGLSurfaceEventArgs`, including on Mac Catalyst where its handler uses a native Metal view.

## Choose a platform or integration

- [.NET MAUI](maui.md) - use cross-platform `SKCanvasView` and `SKGLView` controls and register their handlers.
- [Android](android.md) - use native raster, GL surface, or GL texture views.
- [Apple platforms](apple.md) - choose raster, OpenGL, or Metal controls on iOS, tvOS, macOS, and Mac Catalyst.
- [Windows](windows.md) - use WPF, Windows Forms, or WinUI controls.
- [Linux](linux.md) - use the GTK 3 or GTK 4 raster drawing area.
- [Tizen](tizen.md) - choose the ElmSharp or NUI control family.
- [Uno Platform](uno.md) - use WinUI-shaped raster and GPU controls with target-specific support.
- [Blazor WebAssembly](blazor.md) - use raster HTML canvas or WebGL Razor components.

## Choose raster or GPU

Start with the raster control for your UI framework. Move to a GPU control when profiling shows that raster presentation is the bottleneck, or when the app renders a continuously changing scene that benefits from GPU acceleration.

Changing the control does not require changing shared drawing helpers that accept `SKCanvas`. It does change the platform APIs, event argument type, invalidation model, and GPU availability, so verify the chosen control on every target your app ships.

## Related links

- [Choose a SkiaSharp drawing destination](../index.md)
- [Ganesh GPU surfaces](../ganesh/index.md)
- [Documents](../documents/index.md)
