---
title: "Surfaces in the SkiaSharp Views"
description: "Learn which SkiaSharp view controls provide raster or Ganesh GPU surfaces across .NET MAUI, native platforms, Uno, and Blazor."
---

# Surfaces in the SkiaSharp Views

The [Raster](raster-surfaces.md), [Ganesh](ganesh-surfaces.md), and [Graphite](graphite-surfaces.md) pages show how to create an [`SKSurface`](xref:SkiaSharp.SKSurface) by hand. When you are building an app UI you usually don't need to: the SkiaSharp **view controls** create the surface, size it to the control, and hand it to you in a paint event. Your job is just to draw.

There are two families of view control, and the difference between them is exactly the difference between the surface types:

- **Raster views** create a CPU [raster surface](raster-surfaces.md) each frame and blit the result into the control. They provide the broadest platform support and need no GPU.
- **GPU views** create and manage a GPU context and a surface that [wraps the control's render target](ganesh-surfaces.md#wrapping-an-existing-render-target), so your drawing goes straight to the GPU and is presented without a CPU copy.

> [!NOTE]
> The view controls are **raster + Ganesh only** today. **None of them drive Graphite yet** — in this release [Graphite](graphite-surfaces.md) is an offscreen-only path with no view control. Onscreen Graphite views are **not yet available and are under active investigation**, so treat this as "not wired up yet," not "impossible."

## The paint event

Whichever control you use, you draw in a `PaintSurface` event. The controls raise one of three event-argument types:

- [`SKPaintSurfaceEventArgs`](xref:SkiaSharp.Views.Maui.SKPaintSurfaceEventArgs) — raised by **raster** views. It gives you the `Surface`, the `Info` describing it, and the `RawInfo`.
- `SKPaintGLSurfaceEventArgs` — raised by **GL- and ANGLE-backed GPU** views. In addition to the `Surface` it exposes the `BackendRenderTarget`, the `Origin`, and the `ColorType` of the target the view is drawing into.
- `SKPaintMetalSurfaceEventArgs` — raised by **Metal-backed GPU** views on Apple platforms.

In both cases you get an `SKSurface` and draw on its `Surface.Canvas`:

```csharp
void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    var info = e.Info;
    using var paint = new SKPaint { Color = SKColors.CornflowerBlue };

    canvas.Clear(SKColors.White);
    canvas.DrawCircle(info.Width / 2f, info.Height / 2f, 100, paint);
}
```

The GPU views raise a different event-argument type, but the drawing code is identical — you still just draw on `e.Surface.Canvas`. A **GL view** (`SKGLView` / `SKGLSurfaceView`) raises `SKPaintGLSurfaceEventArgs`:

```csharp
void OnPaintGLSurface(object sender, SKPaintGLSurfaceEventArgs e)
{
    var canvas = e.Surface.Canvas;
    var info = e.Info;
    using var paint = new SKPaint { Color = SKColors.CornflowerBlue };

    canvas.Clear(SKColors.White);
    canvas.DrawCircle(info.Width / 2f, info.Height / 2f, 100, paint);
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
| `SKGLView` | GPU (Ganesh; backend varies by platform) | `PaintSurface` → `SKPaintGLSurfaceEventArgs` |

Under the hood, MAUI handlers map `SKGLView` to OpenGL on Android and iOS, Metal on Mac Catalyst, and `SKSwapChainPanel` on Windows. The Tizen handler is not implemented and throws `PlatformNotSupportedException`. On iOS and tvOS, the OpenGL view is obsolete starting with version 12; prefer a Metal-backed path for new Apple-platform code.

`SKCanvasView` is the most portable starting point. Use `SKGLView` only after checking the handler and backend available on every target your app supports.

> [!IMPORTANT]
> In .NET MAUI you must initialize SkiaSharp by calling `UseSkiaSharp()` on the `MauiAppBuilder` in your `MauiProgram.cs`, with a `using` directive for `SkiaSharp.Views.Maui.Controls.Hosting`.

## SkiaSharp.Views (per platform)

The **SkiaSharp.Views** package contains the native controls that MAUI wraps, and that you can use directly in a non-MAUI app on each platform:

| Platform | Raster control | GPU control(s) |
| --- | --- | --- |
| iOS / tvOS | `SKCanvasView` | `SKGLView` (OpenGL ES, obsolete on version 12+), `SKMetalView` (Metal) |
| macOS | `SKCanvasView` | `SKGLView` (OpenGL), `SKMetalView` (Metal) |
| Mac Catalyst | `SKCanvasView` | `SKMetalView` (Metal) |
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

`SKXamlCanvas` is the portable raster choice. `SKSwapChainPanel` uses Ganesh over OpenGL ES or WebGL on its Android, iOS, macOS, and WebAssembly implementations. It is not supported on Mac Catalyst or on Uno's Skia-renderer targets, where the default `RaiseOnUnsupported` setting causes construction to throw `NotSupportedException`.

## Blazor

The **SkiaSharp.Views.Blazor** package provides two Razor components for Blazor WebAssembly:

| Component | Surface | Backing technology |
| --- | --- | --- |
| `SKCanvasView` | Raster (CPU) | HTML 2D canvas |
| `SKGLView` | GPU (Ganesh) | WebGL |

`SKCanvasView` draws with a raster surface and copies the result to a 2D canvas; `SKGLView` renders through WebGL with a GPU surface. Use them like any other Razor component and handle their `OnPaintSurface` callback.

## Choosing a control

- Start with the **raster** control available on your target (`SKCanvasView` / `SKXamlCanvas`). It needs no GPU and is often fast enough for static and lightly animated UI.
- Move to a **GPU** control (`SKGLView` / `SKMetalView` / `SKSwapChainPanel`) when you are animating continuously, drawing large or complex scenes, or compositing with other GPU content.
- The drawing code you write in the paint event is the **same** either way — only the control type changes.

## Related links

- [SkiaSharp APIs](xref:SkiaSharp)
- [Integrating with .NET MAUI](../basics/integration.md)
- [Ganesh GPU surfaces](ganesh-surfaces.md)
