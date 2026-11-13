---
title: "Render with SkiaSharp views on Apple platforms"
description: "Choose raster, OpenGL, or Metal SkiaSharp views on iOS, tvOS, macOS, and Mac Catalyst."
---

# Render with SkiaSharp views on Apple platforms

The `SkiaSharp.Views` package supplies native controls for iOS, tvOS, macOS, and Mac Catalyst. The namespace depends on the target:

- iOS and Mac Catalyst: `SkiaSharp.Views.iOS`
- tvOS: `SkiaSharp.Views.tvOS`
- macOS: `SkiaSharp.Views.Mac`

## Choose a control

| Target | Raster | OpenGL | Metal |
| --- | --- | --- | --- |
| iOS | `SKCanvasView` | `SKGLView` | `SKMetalView` |
| tvOS | `SKCanvasView` | `SKGLView` | `SKMetalView` |
| macOS | `SKCanvasView` | `SKGLView` | `SKMetalView` |
| Mac Catalyst | `SKCanvasView` | Not available | `SKMetalView` |

Raster views raise `SKPaintSurfaceEventArgs`. OpenGL views raise `SKPaintGLSurfaceEventArgs`, and `SKMetalView` raises `SKPaintMetalSurfaceEventArgs`.

Use the same canvas drawing code for each event type:

```csharp
void OnPaintMetalSurface(object? sender, SKPaintMetalSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    canvas.Clear(SKColors.White);
    // Draw the frame without retaining the surface or canvas.
}
```

## Prefer Metal for new GPU paths

`SKGLView` on iOS and tvOS is marked obsolete starting with operating-system version 12. Use `SKMetalView` for new native GPU rendering on those targets.

Mac Catalyst compiles the raster and Apple Metal controls but excludes `SKGLView`. On macOS, both OpenGL and Metal controls remain available; choose Metal for a modern GPU path and verify behavior on the macOS versions your app supports.

`SKMetalView` creates a Ganesh Metal context. It does not use Graphite.

## Redraw safely

Use the platform view invalidation mechanism rather than drawing outside the paint callback. For on-demand Metal rendering, configure the `MTKView` to pause its loop and request display when state changes. For animation, use the control's render-loop behavior and stop it when the view is no longer visible.

The view owns its render target and the surface supplied to the callback. Dispose only resources your drawing code creates, such as paints, paths, images, and shaders.

## Related links

- [Choose a SkiaSharp view](index.md)
- [.NET MAUI views](maui.md)
- [Ganesh with Metal](../ganesh/metal.md)
