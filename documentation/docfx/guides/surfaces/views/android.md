---
title: "Render with SkiaSharp views on Android"
description: "Choose a native Android raster, GLSurfaceView, or GLTextureView control and draw through its paint event."
---

# Render with SkiaSharp views on Android

The `SkiaSharp.Views` package includes native Android controls in the `SkiaSharp.Views.Android` namespace:

| Control | Android base | Rendering path | Paint event |
| --- | --- | --- | --- |
| `SKCanvasView` | `View` | CPU raster | `SKPaintSurfaceEventArgs` |
| `SKGLSurfaceView` | `GLSurfaceView` | Ganesh over OpenGL ES | `SKPaintGLSurfaceEventArgs` |
| `SKGLTextureView` | `TextureView` through SkiaSharp's `GLTextureView` | Ganesh over OpenGL ES | `SKPaintGLSurfaceEventArgs` |

Use `SKCanvasView` for the default raster path. The two GPU controls expose the same SkiaSharp drawing model but inherit from different Android view primitives. Choose between surface-view and texture-view behavior based on the composition needs of the Android layout.

## Create a view in code

This example installs a raster view as an activity's content:

```csharp
using SkiaSharp.Views.Android;

var skiaView = new SKCanvasView(this);
skiaView.PaintSurface += OnPaintSurface;
SetContentView(skiaView);
```

The view owns the surface passed to `OnPaintSurface`. Do not dispose the surface or retain its canvas after the callback.

For GPU rendering, create `SKGLSurfaceView` or `SKGLTextureView` and handle `SKPaintGLSurfaceEventArgs` instead. Both controls expose their managed `GRContext` while the GL context is valid.

## Request another frame

Call `Invalidate()` on `SKCanvasView` after state changes. The GL controls use their inherited render-request and render-mode APIs. Avoid running a continuous render loop when the scene is unchanged.

Android screen density can make the raw pixel size differ from the logical control size. Check `Info`, `RawInfo`, and the control's pixel-scaling option before mixing Skia coordinates with Android layout coordinates.

## Related links

- [Choose a SkiaSharp view](index.md)
- [.NET MAUI views](maui.md)
- [Ganesh with OpenGL](../ganesh/opengl.md)
