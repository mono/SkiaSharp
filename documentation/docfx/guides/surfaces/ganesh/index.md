---
title: "Ganesh GPU surfaces"
description: "Create Ganesh GPU surfaces with OpenGL, Vulkan, Metal, or Direct3D, then render offscreen or wrap a render target or texture."
---

# Ganesh GPU surfaces

*Ganesh* is Skia's classic GPU backend. To draw on the GPU with Ganesh you create a [`GRContext`](xref:SkiaSharp.GRContext) — a handle to a live graphics API context — and then create an [`SKSurface`](xref:SkiaSharp.SKSurface) from it. There are two shapes of GPU surface:

- **Offscreen** surfaces, where Skia allocates and owns the GPU texture. This is the GPU equivalent of a [raster surface](../raster/index.md) and is the easiest way to render on the GPU.
- **Wrapped** surfaces, where you hand Skia an existing render target (a window's framebuffer) or texture that some other code created. This is how you draw SkiaSharp content into a swap-chain image you present to the screen.

Ganesh supports four graphics APIs: [OpenGL](opengl.md), [Vulkan](vulkan.md), [Metal](metal.md), and [Direct3D](direct3d.md). Context and backend-resource setup differ per API; everything after that — creating the surface, drawing, flushing, and reading back — is shared.

A `GRContext` and the resources created from it are not thread-safe, so use them from one thread at a time. OpenGL has an additional requirement: the GL context used to create the `GRContext` must be current whenever Skia makes GL calls, including during resource cleanup.

## Choose and create a backend

Start with the page for the graphics API your host already owns:

- [OpenGL](opengl.md) — make a WGL, GLX, EGL, CGL, or OpenGL ES context current, then let Ganesh resolve its functions.
- [Vulkan](vulkan.md) — provide the Vulkan instance, physical device, device, graphics queue, and function resolver; a typed Silk.NET adapter is available.
- [Metal](metal.md) — provide an `MTLDevice` and `MTLCommandQueue` on Apple platforms.
- [Direct3D](direct3d.md) — provide a DXGI adapter, D3D12 device, and command queue on Windows.

Each page creates the same [`GRContext`](xref:SkiaSharp.GRContext) abstraction. Return here after context creation for the shared surface, drawing, flushing, readback, and wrapping flow.

## Rendering offscreen

The simplest GPU surface is an offscreen one: describe the image with an [`SKImageInfo`](xref:SkiaSharp.SKImageInfo) and let Skia allocate the backing GPU texture. Pass `budgeted: true` so the texture counts against the context's resource budget and can be recycled.

```csharp
var info = new SKImageInfo(512, 512, SKColorType.Rgba8888, SKAlphaType.Premul);

using var surface = SKSurface.Create(context, budgeted: true, info)
    ?? throw new InvalidOperationException("Unable to create the Ganesh surface.");
using var paint = new SKPaint { Color = SKColors.CornflowerBlue };

surface.Canvas.Clear(SKColors.White);
surface.Canvas.DrawCircle(256, 256, 200, paint);

// push the recorded work to the GPU and wait for it to finish
context.Flush(submit: true, synchronous: true);
```

After flushing, you can read the pixels back synchronously with the same `ReadPixels` call used in the [raster case](../raster/index.md#getting-the-result-out):

```csharp
var pixels = new byte[info.BytesSize];
var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
try
{
    if (!surface.ReadPixels(info, handle.AddrOfPinnedObject(), info.RowBytes, 0, 0))
        throw new InvalidOperationException("Unable to read the surface pixels.");
}
finally
{
    handle.Free();
}
```

## Wrapping an existing render target

To draw SkiaSharp content into a render target that already exists — most often a window's framebuffer or a swap-chain image — describe it to Skia with a [`GRBackendRenderTarget`](xref:SkiaSharp.GRBackendRenderTarget) and wrap it with [`SKSurface.Create`](xref:SkiaSharp.SKSurface.Create*).

Construct the `GRBackendRenderTarget` from the API-specific descriptor shown on the [OpenGL](opengl.md#wrap-the-current-framebuffer), [Vulkan](vulkan.md#describe-vulkan-images), [Metal](metal.md#describe-metal-textures), or [Direct3D](direct3d.md#describe-direct3d-resources) page. Once you have it, the wrapping call is shared:

```csharp
using var surface = SKSurface.Create(context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType)
    ?? throw new InvalidOperationException("Unable to wrap the render target.");

surface.Canvas.Clear(SKColors.White);
// ... draw the frame ...

surface.Canvas.Flush();
context.Flush();
// then present/swap buffers with your windowing code
```

## Wrapping an existing texture

If instead of a render target you have a GPU **texture**, describe it with a [`GRBackendTexture`](xref:SkiaSharp.GRBackendTexture) and create a surface that renders into it:

```csharp
using var surface = SKSurface.Create(
    context, backendTexture, GRSurfaceOrigin.TopLeft, sampleCount: 0, colorType: colorType)
    ?? throw new InvalidOperationException("Unable to wrap the backend texture.");
```

You can also wrap a texture as a *sampling* [`SKImage`](xref:SkiaSharp.SKImage) with [`SKImage.FromTexture`](xref:SkiaSharp.SKImage.FromTexture*) when you want to draw an existing GPU texture *onto* a surface rather than *into* it.

## Cleaning up

Dispose your surfaces and the `GRContext` when you are done. If you use OpenGL, keep the GL context current while disposing the SkiaSharp objects that use it. Disposing the `GRContext` frees the GPU resources Skia allocated through it.

## Related links

- [SkiaSharp APIs](xref:SkiaSharp)
- [Surface overview](../index.md)
- [Raster surfaces](../raster/index.md)
- [Rendering in SkiaSharp Views](../views/index.md)
- [Graphite GPU surfaces](../graphite/index.md)
- [Skia canvas creation, GPU backend (skia.org)](https://skia.org/docs/user/api/skcanvas_creation/)
