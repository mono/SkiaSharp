---
title: "Choose a SkiaSharp drawing destination"
description: "Choose a manually managed surface, document canvas, or platform view for SkiaSharp drawing."
---

# Choose a SkiaSharp drawing destination

You draw with an [`SKCanvas`](xref:SkiaSharp.SKCanvas), but the API that provides the canvas depends on where the result should go:

| Destination | Who manages it | Use when |
| --- | --- | --- |
| [`SKSurface`](xref:SkiaSharp.SKSurface) | Your code | You need offscreen pixels, custom GPU hosting, image processing, or headless rendering |
| [Document canvas](documents/index.md) | `SKDocument` or `SKSvgCanvas` | Drawing should be serialized as PDF, SVG, or XPS |
| [Platform view](views/index.md) | A SkiaSharp view control | Drawing belongs in an app UI and the control should manage presentation |

Shared drawing helpers can accept `SKCanvas` and work with any of these destinations. Keep destination-specific creation, callbacks, finalization, and disposal in the caller.

## Create and manage a surface

An `SKSurface` manages a drawing destination and exposes its canvas. The surface decides whether pixels live in system memory or in a GPU resource. SkiaSharp exposes one CPU surface family and two GPU surface families:

- **Raster surfaces** live in CPU memory. They are always available and need no GPU. Use them for image generation, thumbnails, raster assets for document or print pipelines, unit tests, and headless workloads. See [Raster surfaces](raster/index.md).

- **Ganesh GPU surfaces** are backed by a GPU texture through the classic Skia GPU backend, *Ganesh*. Create a [`GRContext`](xref:SkiaSharp.GRContext) for [OpenGL](ganesh/opengl.md), [Vulkan](ganesh/vulkan.md), [Metal](ganesh/metal.md), or [Direct3D](ganesh/direct3d.md), then create an offscreen surface or wrap an existing render target. See [Ganesh GPU surfaces](ganesh/index.md).

- **Graphite GPU surfaces** use Skia's newer GPU backend, *Graphite*, built on [Vulkan](graphite/vulkan.md), [Metal](graphite/metal.md), and [Dawn/WebGPU](graphite/dawn.md). Graphite records drawing into a recorder, snaps it into a recording, and submits that recording to the GPU. See [Graphite GPU surfaces](graphite/index.md).

For app UI, usually let a [SkiaSharp view](views/index.md) create and manage the surface. For document output, use a [document canvas](documents/index.md) instead of rendering an intermediate surface unless the document needs a raster asset.

## Ganesh or Graphite?

Both Ganesh and Graphite are GPU backends that render into an `SKSurface`, but the programming models differ:

| | Ganesh | Graphite |
| --- | --- | --- |
| Context | [`GRContext`](xref:SkiaSharp.GRContext) | `SKGraphiteContext` |
| Backends | OpenGL, Vulkan, Metal, Direct3D | Vulkan, Metal, Dawn (WebGPU) |
| Platform fit | Broad desktop and mobile support | Metal on Apple; Vulkan elsewhere; Dawn in WebAssembly |
| Drawing | Draw, then flush or submit | Record, insert, then submit |
| Readback | Synchronous | Asynchronous |
| SkiaSharp Views | Ganesh-backed views available | Not available; offscreen only |

Ganesh is mature and is what the Views use today. It supports synchronous `SKSurface.ReadPixels`; Graphite uses `SKGraphiteContext.RequestReadPixels`. Graphite is the direction Skia is moving in, and in SkiaSharp it is currently an **offscreen** path — you create the surface, draw, submit, and read back the result yourself.

If you already have Ganesh code and want to understand the equivalent Graphite calls, see [Migrating from Ganesh to Graphite](graphite/migrate-from-ganesh.md).

## In this section

### [Raster surfaces](raster/index.md)

Create CPU-backed surfaces with `SKSurface.Create`, draw into memory you own with a raster-direct surface, and read the result back.

### [Ganesh GPU surfaces](ganesh/index.md)

Create a `GRContext` for OpenGL, Vulkan, Metal, or Direct3D, then make an offscreen surface or wrap an existing render target or texture.

### [Graphite GPU surfaces](graphite/index.md)

Create an `SKGraphiteContext`, record and submit drawing, wrap external GPU textures, and read pixels back through the asynchronous readback path.

### [Migrate from Ganesh to Graphite](graphite/migrate-from-ganesh.md)

Map your existing Ganesh code — context creation, flushing, and readback — onto the Graphite recorder/recording model.

### [Documents](documents/index.md)

Create and finalize PDF, SVG, or XPS output through a canvas backed by a document stream.

### [Rendering in SkiaSharp Views](views/index.md)

Choose a raster or Ganesh-backed control for .NET MAUI, native platforms, Uno Platform, or Blazor WebAssembly.

## Related links

- [SkiaSharp APIs](xref:SkiaSharp)
