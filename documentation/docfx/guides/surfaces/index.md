---
title: "Choose a SkiaSharp surface"
description: "Choose a raster, Ganesh, or Graphite SKSurface for CPU, GPU, offscreen, and headless rendering in SkiaSharp."
---

# Choose a SkiaSharp surface

You draw with an [`SKCanvas`](xref:SkiaSharp.SKCanvas). An [`SKSurface`](xref:SkiaSharp.SKSurface) manages a drawing destination and exposes a canvas for it. The surface decides where the pixels live: in system memory (a **raster** surface) or in a GPU texture (a **GPU-backed** surface).

Most of the other guides use an `SKCanvasView` and never create a surface directly — the view does that for you. This section is about the layer underneath: how to create a surface yourself for offscreen rendering, custom hosting, image processing pipelines, and server-side or headless rendering.

## Choose how the surface is managed

SkiaSharp exposes one CPU surface family and two GPU surface families.

- **Raster surfaces** live in CPU memory. They are always available and need no GPU. Use them for image generation, thumbnails, PDF/print pipelines, unit tests, and headless workloads. See [Raster surfaces](raster/index.md).

- **Ganesh GPU surfaces** are backed by a GPU texture through the classic Skia GPU backend, *Ganesh*. Create a [`GRContext`](xref:SkiaSharp.GRContext) for [OpenGL](ganesh/opengl.md), [Vulkan](ganesh/vulkan.md), [Metal](ganesh/metal.md), or [Direct3D](ganesh/direct3d.md), then create an offscreen surface or wrap an existing render target. See [Ganesh GPU surfaces](ganesh/index.md).

- **Graphite GPU surfaces** use Skia's newer GPU backend, *Graphite*, built on [Vulkan](graphite/vulkan.md), [Metal](graphite/metal.md), and [Dawn/WebGPU](graphite/dawn.md). Graphite records drawing into a recorder, snaps it into a recording, and submits that recording to the GPU. See [Graphite GPU surfaces](graphite/index.md).

For app UI, usually let a SkiaSharp view create and manage the surface. See [Rendering in SkiaSharp Views](../views/index.md).

## Ganesh or Graphite?

Both Ganesh and Graphite are GPU backends that render into an `SKSurface`, but the programming models differ:

| | Ganesh | Graphite |
| --- | --- | --- |
| Context type | [`GRContext`](xref:SkiaSharp.GRContext) | `SKGraphiteContext` |
| Backends | OpenGL, Vulkan, Metal, Direct3D | Vulkan, Metal, Dawn (WebGPU) — no Direct3D |
| Platforms | All | Metal on Apple; Vulkan on Linux/Android/Windows; Dawn on WASM |
| Drawing model | Draw on the canvas, then `Flush`/`Submit` the context | Draw on the canvas, then `Snap` a recording and `InsertRecording` + `Submit` it |
| Reading pixels back | Synchronous `SKSurface.ReadPixels` works | **Asynchronous only** — use `SKGraphiteContext.RequestReadPixels` |
| In the SkiaSharp Views | Yes (`SKGLView`, `SKMetalView`) | Not yet — offscreen only |

Ganesh is mature and is what the Views use today. Graphite is the direction Skia is moving in, and in SkiaSharp it is currently an **offscreen** path — you create the surface, draw, submit, and read back the result yourself.

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

## Related links

- [SkiaSharp APIs](xref:SkiaSharp)
- [Rendering in SkiaSharp Views](../views/index.md)
