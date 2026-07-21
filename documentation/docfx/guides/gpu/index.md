---
title: "GPU and Offscreen Surfaces"
description: "Learn how to create the different kinds of SKSurface that SkiaSharp can draw into — CPU raster surfaces, Ganesh GPU surfaces (OpenGL, Vulkan, Metal, Direct3D), the surfaces the SkiaSharp Views manage for you, and the new Graphite offscreen backend."
---

# GPU and Offscreen Surfaces

_Understand the different kinds of `SKSurface` and how to create them_

Everything you draw with SkiaSharp is drawn onto an [`SKCanvas`](xref:SkiaSharp.SKCanvas), and every canvas is backed by an [`SKSurface`](xref:SkiaSharp.SKSurface). The surface is what decides *where* the pixels actually live: in ordinary system memory (a **raster** surface), or in a texture owned by a GPU (a **GPU-backed** surface).

Most of the other guides use an `SKCanvasView` and never create a surface directly — the view does that for you. This section is about the layer underneath: how to create a surface yourself, which is what you need for offscreen rendering, custom hosting, image processing pipelines, and server-side or headless rendering.

## The three ways to get a surface

SkiaSharp has three families of surface, and this section has a page for each.

- **Raster surfaces** live in CPU memory. They are always available, work identically on every platform, and need no GPU. This is the right choice for image generation, thumbnails, PDF/print pipelines, unit tests, and any headless workload. See [Raster Surfaces](raster-surfaces.md).

- **Ganesh GPU surfaces** are backed by a GPU texture through the classic Skia GPU backend, *Ganesh*. You create a [`GRContext`](xref:SkiaSharp.GRContext) for an API — OpenGL, Vulkan, Metal, or Direct3D — and then create a surface from it, either fully offscreen or wrapping an existing render target. See [Ganesh GPU Surfaces](ganesh-surfaces.md).

- **Graphite offscreen surfaces** use Skia's newer GPU backend, *Graphite*, built on modern explicit APIs (Vulkan, Metal, and Dawn/WebGPU). Graphite records drawing into a *recorder*, snaps it into a *recording*, and submits that recording to the GPU. See [Graphite Offscreen Surfaces](graphite-surfaces.md).

Separately, if you are building an app UI you usually don't create any of these by hand — the SkiaSharp *Views* do it for you and hand you a ready-to-draw surface in a paint event. See [Surfaces in the SkiaSharp Views](views-surfaces.md).

## Ganesh or Graphite?

Both Ganesh and Graphite are GPU backends that render into an `SKSurface`, but the programming models differ:

| | Ganesh | Graphite |
| --- | --- | --- |
| Context type | [`GRContext`](xref:SkiaSharp.GRContext) | `SKGraphiteContext` |
| Backends | OpenGL, Vulkan, Metal, Direct3D | Vulkan, Metal, Dawn (WebGPU) |
| Drawing model | Draw on the canvas, then `Flush`/`Submit` the context | Draw on the canvas, then `Snap` a recording and `InsertRecording` + `Submit` it |
| Reading pixels back | Synchronous `SKSurface.ReadPixels` works | **Asynchronous only** — use `SKGraphiteContext.RequestReadPixels` |
| In the SkiaSharp Views | Yes (`SKGLView`, `SKMetalView`) | Not yet — offscreen only |

Ganesh is mature and is what the Views use today. Graphite is the direction Skia is moving in, and in SkiaSharp it is currently an **offscreen** path — you create the surface, draw, submit, and read back the result yourself.

If you already have Ganesh code and want to understand the equivalent Graphite calls, see [Migrating from Ganesh to Graphite](graphite-migration.md).

## In this section

## [Raster Surfaces](raster-surfaces.md)

Create CPU-backed surfaces with `SKSurface.Create`, draw into memory you own with a raster-direct surface, and read the result back.

## [Ganesh GPU Surfaces](ganesh-surfaces.md)

Create a `GRContext` for OpenGL, Vulkan, Metal, or Direct3D, then make an offscreen surface or wrap an existing render target or texture.

## [Surfaces in the SkiaSharp Views](views-surfaces.md)

See which view controls give you a raster surface and which give you a GPU surface, across SkiaSharp.Views, .NET MAUI, Uno Platform, and Blazor.

## [Graphite Offscreen Surfaces](graphite-surfaces.md)

Create an `SKGraphiteContext`, record and submit drawing, wrap external GPU textures, and read pixels back through the asynchronous readback path.

## [Migrating from Ganesh to Graphite](graphite-migration.md)

Map your existing Ganesh code — context creation, flushing, and readback — onto the Graphite recorder/recording model.

## Related Links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
