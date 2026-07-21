---
title: "Migrating from Ganesh to Graphite"
description: "Map your existing SkiaSharp Ganesh GPU code onto the newer Graphite backend — context creation, the recorder and recording drawing model, submission, and the asynchronous pixel readback that replaces synchronous ReadPixels."
---

# Migrating from Ganesh to Graphite

_Map your existing Ganesh code onto the Graphite model_

If you already render on the GPU with [Ganesh](ganesh-surfaces.md) — a [`GRContext`](xref:SkiaSharp.GRContext), an `SKSurface`, and `Flush` — this page shows the equivalent [Graphite](graphite-surfaces.md) calls. The concepts line up closely. Two **behaviour** changes matter most, and they are the parts most likely to bite when you port working code:

1. **Reading pixels back is asynchronous.** Graphite has no synchronous `SKSurface.ReadPixels`; you use `context.RequestReadPixels(...)` and pump `CheckAsyncWorkCompletion()`.
2. **CPU images need an image provider.** Ganesh auto-uploads a raster `SKImage` when you draw it; Graphite does not — without a provider the draw is silently dropped.

There is also a **structural** shift: Ganesh records a draw stream on one `GRContext` and auto-flushes it, whereas Graphite is explicit — you record into a `Recorder`, `Snap` a `Recording`, `InsertRecording`, then `Submit`. The `Recorder` is **per-thread**, so a multi-threaded renderer gives each thread its own recorder.

> [!NOTE]
> Graphite is currently an **offscreen** path in SkiaSharp. If your Ganesh code renders into a view control's render target (`SKGLView`, `SKMetalView`, `SKSwapChainPanel`), there is no Graphite equivalent for that view yet — the [view controls](views-surfaces.md) still use Ganesh. This migration applies to offscreen rendering.

## Concept mapping

| Ganesh | Graphite |
| --- | --- |
| `GRContext` | `SKGraphiteContext` |
| `GRContext.CreateGl` / `CreateVulkan` / `CreateMetal` / `CreateDirect3D` | `SKGraphiteContext.CreateVulkan` / `CreateMetal` / `CreateDawn` |
| `GRVkBackendContext` / `GRMtlBackendContext` | `SKGraphiteVkBackendContext` / `SKGraphiteMtlBackendContext` / `SKGraphiteDawnBackendContext` |
| `GRSharpVkBackendContext` (typed Vulkan) | `SKGraphiteSharpVkBackendContext` (typed Vulkan) |
| `SKSurface.Create(context, budgeted, info)` | `context.CreateRecorder()` + `SKSurface.Create(recorder, info)` |
| Draw on `surface.Canvas` | Draw on `surface.Canvas` (unchanged) |
| `context.Flush(submit: true, synchronous: true)` | `recorder.Snap()` + `context.InsertRecording(recording)` + `context.Submit(new SKGraphiteSubmitInfo { Sync = true })` |
| `surface.ReadPixels(...)` (synchronous) | `context.RequestReadPixels(...)` + `context.CheckAsyncWorkCompletion()` (asynchronous) |
| Draw a CPU `SKImage` (auto-uploaded) | Draw a CPU `SKImage` (needs an image provider — see below) |
| `GRBackendTexture` / `SKSurface.Create(context, texture, ...)` | `SKGraphiteBackendTexture` / `SKSurface.Create(recorder, backendTexture, colorType)` |
| `SKImage.FromTexture(context, texture, ...)` | `SKImage.FromTexture(recorder, backendTexture, ...)` |
| `image.ToTextureImage(context)` | `image.ToTextureImage(recorder)` |

Notice the pattern: wherever Ganesh takes the **context**, Graphite's per-surface and per-image APIs take the **recorder** instead. OpenGL and Direct3D have no Graphite backend — Graphite targets Vulkan, Metal, and Dawn (WebGPU).

## Before and after

Here is a complete offscreen render + readback in each backend.

### Ganesh

```csharp
var info = new SKImageInfo(512, 512, SKColorType.Rgba8888, SKAlphaType.Premul);

using var context = GRContext.CreateMetal(backendContext);
using var surface = SKSurface.Create(context, budgeted: true, info);

surface.Canvas.Clear(SKColors.White);
surface.Canvas.DrawCircle(256, 256, 200, new SKPaint { Color = SKColors.CornflowerBlue });

context.Flush(submit: true, synchronous: true);

// synchronous readback
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

### Graphite

```csharp
var info = new SKImageInfo(512, 512, SKColorType.Rgba8888, SKAlphaType.Premul);

using var context = SKGraphiteContext.CreateMetal(backendContext);
using var recorder = context.CreateRecorder();
using var surface = SKSurface.Create(recorder, info);

surface.Canvas.Clear(SKColors.White);
surface.Canvas.DrawCircle(256, 256, 200, new SKPaint { Color = SKColors.CornflowerBlue });

using (var recording = recorder.Snap())
{
    if (context.InsertRecording(recording) != SKGraphiteInsertStatus.Success)
        throw new InvalidOperationException("InsertRecording did not succeed.");
}
context.Submit(new SKGraphiteSubmitInfo { Sync = true });

// asynchronous readback — see the Graphite Offscreen Surfaces guide for the full helper
var pixels = ReadPixelsAsync(context, surface, info);
```

The drawing calls are identical. What changes is the plumbing around them.

## The changes to make

### 1. Replace `Flush` with snap + insert + submit

Ganesh flushes the context directly. Graphite splits this into three steps: `recorder.Snap()` captures the recorded commands into an `SKGraphiteRecording`, `context.InsertRecording(recording)` hands them to the context, and `context.Submit(new SKGraphiteSubmitInfo { Sync = true })` sends them to the GPU and (with `Sync = true`) waits.

Always check that `InsertRecording` returns `SKGraphiteInsertStatus.Success`, and note that `Snap` **resets** the recorder for the next frame. Because the recorder is per-thread, a multi-threaded renderer creates one recorder per thread and submits their recordings to the shared context.

### 2. Replace synchronous `ReadPixels` with asynchronous readback

This is the most important change. Graphite surfaces do **not** support synchronous [`SKSurface.ReadPixels`](xref:SkiaSharp.SKSurface.ReadPixels*) in shipping builds — it returns `false`. Replace it with `RequestReadPixels`, then drive the request to completion with `Submit` and repeated `CheckAsyncWorkCompletion` calls. The returned plane may be row-padded, so copy row-by-row. See [Reading pixels back](graphite-surfaces.md#reading-pixels-back) for the complete helper.

### 3. Give the recorder an image provider for CPU images

Ganesh silently uploads a raster `SKImage` to the GPU the first time you draw it. Graphite does **not** — drawing a non-Graphite image without an *image provider* drops the draw with no error. If your Ganesh code draws decoded/CPU images, create the recorder with an image provider (the ready-made `SKGraphiteImageCache` is the simplest option), or upload each image yourself with `ToTextureImage` first. See [Drawing CPU images](graphite-surfaces.md#drawing-cpu-images-the-image-provider).

### 4. Pass the recorder where you used to pass the context

Per-surface and per-image creation moves from the context to the recorder:

- `SKSurface.Create(context, budgeted, info)` → `SKSurface.Create(recorder, info)`
- `SKSurface.Create(context, backendTexture, ...)` → `SKSurface.Create(recorder, backendTexture, colorType)`
- `SKImage.FromTexture(context, ...)` → `SKImage.FromTexture(recorder, ...)`
- `image.ToTextureImage(context)` → `image.ToTextureImage(recorder)`

## Watch out for

- **No OpenGL or Direct3D.** Graphite targets Vulkan, Metal, and Dawn. If your Ganesh code uses GL or D3D, there is no direct Graphite equivalent; keep using Ganesh, or move to Vulkan/Metal/Dawn.
- **CPU images need a provider.** The single easiest thing to miss — a raster `SKImage` drawn without an image provider simply doesn't appear. See change 3 above.
- **Browser (Dawn/WebGPU) can't submit synchronously.** In a WebAssembly host, `Submit(Sync = true)` throws. Submit without syncing and pump `CheckAsyncWorkCompletion`. See [Dawn in the browser](graphite-surfaces.md#dawn-in-the-browser).
- **Check backend availability.** Use `SKGraphiteContext.IsBackendAvailable` before creating a context, since not every build includes every backend.
- **The recorder is per-thread.** As with Ganesh, the context, recorders, and surfaces are single-threaded — use a recorder only on the thread that created it, and give each rendering thread its own.

## Related Links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
- [Ganesh GPU Surfaces](ganesh-surfaces.md)
- [Graphite Offscreen Surfaces](graphite-surfaces.md)
