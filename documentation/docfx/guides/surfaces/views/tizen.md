---
title: "Render with SkiaSharp views on Tizen"
description: "Choose the ElmSharp or NUI SkiaSharp view controls and understand which Tizen path is GPU-backed."
---

# Render with SkiaSharp views on Tizen

The `SkiaSharp.Views` package contains separate control families for ElmSharp and Tizen NUI.

## ElmSharp controls

The `SkiaSharp.Views.Tizen` namespace provides:

| Control | Rendering path | Paint event |
| --- | --- | --- |
| `SKCanvasView` | CPU raster | `SKPaintSurfaceEventArgs` |
| `SKGLSurfaceView` | Ganesh over OpenGL ES | `SKPaintGLSurfaceEventArgs` |

The GPU control creates an Evas GL context, tries OpenGL ES 3 before falling back to OpenGL ES 2 or the platform default, and exposes its `GRContext`.

## NUI controls

The `SkiaSharp.Views.Tizen.NUI` namespace also provides `SKCanvasView` and `SKGLSurfaceView`. Both NUI controls expose `SKPaintSurfaceEventArgs` and draw through memory-backed `SKSurface` instances before presenting through NUI. The NUI `SKGLSurfaceView` is therefore not the same Ganesh API as the ElmSharp control with the same class name.

Choose the namespace that matches the application's Tizen UI framework, and use the event argument type supplied by that control rather than assuming it from the class name.

## Invalidate and dispose

Create Tizen controls on the main thread. Call `Invalidate()` to request a new frame after state changes. The control owns its surface and native presentation resources; drawing code owns and disposes the paints, paths, images, and other objects it creates.

The current `SkiaSharp.Views.Maui.Controls` package does not target Tizen. Use the native Tizen controls directly.

## Related links

- [Choose a SkiaSharp view](index.md)
- [Raster surfaces](../raster/index.md)
- [Ganesh with OpenGL](../ganesh/opengl.md)
