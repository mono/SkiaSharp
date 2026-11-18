---
title: "Render with SkiaSharp views in Blazor WebAssembly"
description: "Use SkiaSharp.Views.Blazor raster HTML canvas and Ganesh WebGL components in a Blazor WebAssembly app."
---

# Render with SkiaSharp views in Blazor WebAssembly

The `SkiaSharp.Views.Blazor` package provides two Razor components for browser WebAssembly:

| Component | Rendering path | Callback |
| --- | --- | --- |
| `SKCanvasView` | CPU raster copied to an HTML 2D canvas | `OnPaintSurface` with `SKPaintSurfaceEventArgs` |
| `SKGLView` | Ganesh over WebGL | `OnPaintSurface` with `SKPaintGLSurfaceEventArgs` |

The package is browser-only. Its JavaScript interop rejects non-WebAssembly hosting.

## Add a raster component

```razor
@using SkiaSharp
@using SkiaSharp.Views.Blazor

<SKCanvasView OnPaintSurface="OnPaintSurface"
              IgnorePixelScaling="true" />

@code {
    private void OnPaintSurface(SKPaintSurfaceEventArgs e)
    {
        var canvas = e.Surface.Canvas;
        canvas.Clear(SKColors.White);
    }
}
```

Use `SKGLView` when the browser and workload benefit from WebGL. The drawing callback still receives an `SKSurface`; only the event argument type and backing target change.

## Control redraws

Call `Invalidate()` after application state changes. Set `EnableRenderLoop` only for animation, because it requests browser animation frames continuously.

`IgnorePixelScaling` changes whether the callback's logical size or device-pixel size is used for drawing. Compare `Info` and `RawInfo` when coordinating Skia drawing with CSS layout or pointer coordinates.

Both components own their event surfaces and JavaScript interop objects. The raster component also owns its pinned pixel buffer; the GPU component owns its WebGL-backed Ganesh context. Do not retain the event surface after the callback.

## Related links

- [Choose a SkiaSharp view](index.md)
- [Uno Platform views](uno.md)
- [Raster surfaces](../raster/index.md)
- [Ganesh with OpenGL](../ganesh/opengl.md)
