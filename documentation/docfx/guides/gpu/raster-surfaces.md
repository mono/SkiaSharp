---
title: "Raster Surfaces"
description: "Create CPU-backed SKSurface objects for offscreen and headless rendering with SkiaSharp, draw into memory you own with a raster-direct surface, and read the result back as an SKImage or encoded bytes."
---

# Raster Surfaces

_Draw into CPU memory with `SKSurface.Create`_

A **raster** surface keeps its pixels in ordinary system (CPU) memory. It is the simplest kind of surface, it needs no GPU, and it behaves identically on every platform SkiaSharp supports. Raster surfaces are the right choice whenever you are rendering offscreen: generating images or thumbnails, building a PDF or print pipeline, rendering on a server, or drawing in a unit test.

## Creating a raster surface

The most common way to create a raster surface is to describe the image you want with an [`SKImageInfo`](xref:SkiaSharp.SKImageInfo) and let SkiaSharp allocate the pixel buffer for you:

```csharp
var info = new SKImageInfo(256, 256, SKColorType.Rgba8888, SKAlphaType.Premul);

using var surface = SKSurface.Create(info);
var canvas = surface.Canvas;

canvas.Clear(SKColors.White);
canvas.DrawCircle(128, 128, 100, new SKPaint { Color = SKColors.CornflowerBlue });
```

`SKImageInfo` describes the width, height, color type, and alpha type of the surface. `SKColorType.Rgba8888` with `SKAlphaType.Premul` is a common, portable choice, but you can pick whatever format your pipeline needs.

The [`SKSurface.Create(SKImageInfo)`](xref:SkiaSharp.SKSurface.Create(SkiaSharp.SKImageInfo)) overload returns `null` if the surface could not be created (for example, if the dimensions are invalid), so it's good practice to check the result before using it.

## Getting the result out

Once you've finished drawing, there are two common ways to get the pixels back.

The simplest is to take an immutable snapshot as an [`SKImage`](xref:SkiaSharp.SKImage), which you can then encode to PNG, JPEG, or another format:

```csharp
using var image = surface.Snapshot();
using var data = image.Encode(SKEncodedImageFormat.Png, 100);

using var stream = File.OpenWrite("output.png");
data.SaveTo(stream);
```

If you need the raw pixels rather than an encoded image, read them back into a buffer with [`ReadPixels`](xref:SkiaSharp.SKSurface.ReadPixels*). Because a raster surface already lives in CPU memory, this read is synchronous and cheap:

```csharp
var info = new SKImageInfo(256, 256, SKColorType.Rgba8888, SKAlphaType.Premul);
var pixels = new byte[info.BytesSize];

var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
try
{
    surface.ReadPixels(info, handle.AddrOfPinnedObject(), info.RowBytes, 0, 0);
}
finally
{
    handle.Free();
}
```

> [!NOTE]
> Synchronous `ReadPixels` is a raster and Ganesh convenience. Graphite surfaces do **not** support it — see [Graphite Offscreen Surfaces](graphite-surfaces.md#reading-pixels-back).

## Raster-direct: drawing into memory you own

The `SKSurface.Create(SKImageInfo)` overload lets Skia allocate the pixel buffer. Sometimes you already have a buffer — a `byte[]`, a native allocation, or the pixels of an [`SKBitmap`](xref:SkiaSharp.SKBitmap) — and you want Skia to draw *directly* into it with no extra copy. That is a **raster-direct** surface.

Pass the address of your buffer along with the info and row stride:

```csharp
var info = new SKImageInfo(256, 256, SKColorType.Rgba8888, SKAlphaType.Premul);
var pixels = new byte[info.BytesSize];
var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
try
{
    using var surface = SKSurface.Create(info, handle.AddrOfPinnedObject(), info.RowBytes);

    // every draw call writes straight into `pixels`
    surface.Canvas.Clear(SKColors.White);
    surface.Canvas.DrawCircle(128, 128, 100, new SKPaint { Color = SKColors.Red });
    surface.Canvas.Flush();
}
finally
{
    handle.Free();
}
```

> [!IMPORTANT]
> The memory you pass must stay alive and pinned for as long as the surface uses it. If you pin a managed array with `GCHandle`, keep the handle allocated until you are done drawing and have flushed the canvas; freeing it too early lets the garbage collector move the buffer out from under Skia.

You can also wrap an [`SKPixmap`](xref:SkiaSharp.SKPixmap) — which already bundles an `SKImageInfo` with a pixel pointer — using the [`SKSurface.Create(SKPixmap)`](xref:SkiaSharp.SKSurface.Create(SkiaSharp.SKPixmap)) overload:

```csharp
using var bitmap = new SKBitmap(info);
using var pixmap = bitmap.PeekPixels();
using var surface = SKSurface.Create(pixmap);

surface.Canvas.Clear(SKColors.White);
// ... draw ...
// the pixels are now visible directly in `bitmap`
```

This is a convenient way to draw straight into an `SKBitmap` you already have.

## When to use a raster surface

Reach for a raster surface when:

- You are rendering **offscreen** or **headless** — no window, no GPU context.
- You want **deterministic, portable** output that is identical across platforms.
- You are producing images to save, stream, or process further (thumbnails, tiles, reports).
- You need to draw directly into a buffer you already own (raster-direct).

If you need GPU acceleration — because you are rendering many frames per second, compositing with other GPU content, or drawing very large scenes — use a GPU-backed surface instead. See [Ganesh GPU Surfaces](ganesh-surfaces.md) and [Graphite Offscreen Surfaces](graphite-surfaces.md).

## Related Links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
- [Creating and Drawing on Bitmaps](../bitmaps/drawing.md)
- [Skia canvas creation, Raster backend (skia.org)](https://skia.org/docs/user/api/skcanvas_creation/)
