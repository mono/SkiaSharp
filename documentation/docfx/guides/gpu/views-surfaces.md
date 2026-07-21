---
title: "Surfaces in the SkiaSharp Views"
description: "Understand how the SkiaSharp view controls create and drive an SKSurface for you — which controls give you a CPU raster surface and which give you a GPU surface — across SkiaSharp.Views, .NET MAUI, Uno Platform, and Blazor."
---

# Surfaces in the SkiaSharp Views

_How the SkiaSharp view controls create and drive a surface for you_

The [Raster](raster-surfaces.md), [Ganesh](ganesh-surfaces.md), and [Graphite](graphite-surfaces.md) pages show how to create an [`SKSurface`](xref:SkiaSharp.SKSurface) by hand. When you are building an app UI you usually don't need to: the SkiaSharp **view controls** create the surface, size it to the control, and hand it to you in a paint event. Your job is just to draw.

There are two families of view control, and the difference between them is exactly the difference between the surface types:

- **Raster views** create a CPU [raster surface](raster-surfaces.md) each frame and blit the result into the control. They work everywhere and need no GPU.
- **GPU views** create and manage a GPU context and a surface that [wraps the control's render target](ganesh-surfaces.md#wrapping-an-existing-render-target), so your drawing goes straight to the GPU and is presented without a CPU copy.

> [!NOTE]
> The view controls are **raster + Ganesh only** today. **None of them drive Graphite yet** — in this release [Graphite](graphite-surfaces.md) is an offscreen-only path with no view control. Onscreen Graphite views are **not yet available and are under active investigation**, so treat this as "not wired up yet," not "impossible."

## The paint event

Whichever control you use, you draw in a `PaintSurface` event. The controls raise one of two event-argument types:

- [`SKPaintSurfaceEventArgs`](xref:SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs) — raised by **raster** views. It gives you the `Surface`, the `Info` describing it, and the `RawInfo`.
- `SKPaintGLSurfaceEventArgs` — raised by **GPU** views. In addition to the `Surface` it exposes the `BackendRenderTarget`, the `Origin`, and the `ColorType` of the target the view is drawing into.

In both cases you get an `SKSurface` and draw on its `Surface.Canvas`:

```csharp
void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    var info = e.Info;

    canvas.Clear(SKColors.White);
    canvas.DrawCircle(info.Width / 2f, info.Height / 2f, 100, new SKPaint
    {
        Color = SKColors.CornflowerBlue,
    });
}
```

The GPU views raise a different event-argument type, but the drawing code is identical — you still just draw on `e.Surface.Canvas`. A **GL view** (`SKGLView` / `SKGLSurfaceView`) raises `SKPaintGLSurfaceEventArgs`:

```csharp
void OnPaintGLSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    var info = e.Info;

    canvas.Clear(SKColors.White);
    canvas.DrawCircle(info.Width / 2f, info.Height / 2f, 100, new SKPaint
    {
        Color = SKColors.CornflowerBlue,
    });
}
```

On Apple platforms, a **Metal view** (`SKMetalView`) is the Metal-backed alternative and raises `SKPaintMetalSurfaceEventArgs` — again, the same drawing:

```csharp
void OnPaintMetalSurface(object sender, SKPaintMetalSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    canvas.Clear(SKColors.White);
    // draw exactly as with the raster and GL views
}
```

Choosing a GPU view means the same drawing runs on the GPU.

## .NET MAUI

The **SkiaSharp.Views.Maui.Controls** package provides two cross-platform controls that you can place in XAML or build in code:

| Control | Surface | Paint event |
| --- | --- | --- |
| `SKCanvasView` | Raster (CPU) | `PaintSurface` → `SKPaintSurfaceEventArgs` |
| `SKGLView` | GPU (Ganesh, OpenGL) | `PaintSurface` → `SKPaintGLSurfaceEventArgs` |

Under the hood, MAUI handlers map these controls to the per-platform SkiaSharp.Views controls below. `SKCanvasView` is the simplest starting point and is what the rest of these guides use; switch to `SKGLView` when you need GPU acceleration.

> [!IMPORTANT]
> In .NET MAUI you must initialize SkiaSharp by calling `UseSkiaSharp()` on the `MauiAppBuilder` in your `MauiProgram.cs`, with a `using` directive for `SkiaSharp.Views.Maui.Controls.Hosting`.

## SkiaSharp.Views (per platform)

The **SkiaSharp.Views** package contains the native controls that MAUI wraps, and that you can use directly in a non-MAUI app on each platform:

| Platform | Raster control | GPU control(s) |
| --- | --- | --- |
| iOS / macOS / tvOS | `SKCanvasView` | `SKGLView` (OpenGL ES), `SKMetalView` (Metal) |
| Android | `SKCanvasView` | `SKGLSurfaceView`, `SKGLTextureView` (OpenGL ES) |
| Tizen | `SKCanvasView` | `SKGLSurfaceView` (OpenGL ES) |
| Windows (WinUI / UWP) | `SKXamlCanvas` | `SKSwapChainPanel` (ANGLE / OpenGL ES) |

On Apple platforms, `SKMetalView` is a Metal-backed alternative to the OpenGL `SKGLView`; it raises an `SKPaintMetalSurfaceEventArgs`. The Windows `SKSwapChainPanel` is the GPU counterpart to the raster `SKXamlCanvas`.

Internally, the GPU controls do exactly what the [Ganesh wrapping example](ganesh-surfaces.md#wrapping-an-existing-render-target) shows: they create a `GRContext`, describe the control's framebuffer as a `GRBackendRenderTarget`, and call `SKSurface.Create(context, renderTarget, origin, colorType)` for you each frame.

## Uno Platform

The **SkiaSharp.Views.Uno** package brings the same idea to Uno Platform, mirroring the WinUI control names:

| Control | Surface | Paint event |
| --- | --- | --- |
| `SKXamlCanvas` | Raster (CPU) | `PaintSurface` → `SKPaintSurfaceEventArgs` |
| `SKSwapChainPanel` | GPU (Ganesh, OpenGL ES) | `PaintSurface` → `SKPaintGLSurfaceEventArgs` |

Because Uno runs the same controls across its targets (including WebAssembly), `SKXamlCanvas` is the portable raster choice and `SKSwapChainPanel` is the GPU-accelerated one.

## Blazor

The **SkiaSharp.Views.Blazor** package provides two Razor components for Blazor WebAssembly:

| Component | Surface | Backing technology |
| --- | --- | --- |
| `SKCanvasView` | Raster (CPU) | HTML 2D canvas |
| `SKGLView` | GPU (Ganesh) | WebGL |

`SKCanvasView` draws with a raster surface and copies the result to a 2D canvas; `SKGLView` renders through WebGL with a GPU surface. Use them like any other Razor component and handle their `OnPaintSurface` callback.

## Choosing a control

- Start with the **raster** control (`SKCanvasView` / `SKXamlCanvas`). It is the simplest, works everywhere, and is fast enough for most static and lightly-animated UI.
- Move to a **GPU** control (`SKGLView` / `SKMetalView` / `SKSwapChainPanel`) when you are animating continuously, drawing large or complex scenes, or compositing with other GPU content.
- The drawing code you write in the paint event is the **same** either way — only the control type changes.

## Related Links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
- [Integrating with .NET MAUI](../basics/integration.md)
- [Ganesh GPU Surfaces](ganesh-surfaces.md)
