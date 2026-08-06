---
title: "Migrate from Ganesh to Graphite"
description: "Migrate Ganesh GPU code to Graphite by replacing context flushing, synchronous readback, and automatic CPU-image uploads."
---

# Migrate from Ganesh to Graphite

If you already render on the GPU with [Ganesh](../ganesh/index.md) — a [`GRContext`](xref:SkiaSharp.GRContext), an `SKSurface`, and `Flush` — this page shows the equivalent [Graphite](index.md) calls. The concepts line up closely. Two **behaviour** changes matter most, and they are the parts most likely to bite when you port working code:

1. **Reading pixels back is asynchronous.** Graphite has no synchronous `SKSurface.ReadPixels`; you use `context.RequestReadPixels(...)` and pump `CheckAsyncWorkCompletion()`.
2. **CPU images need an image provider.** Ganesh auto-uploads a raster `SKImage` when you draw it; Graphite does not — without a provider the draw is silently dropped.

There is also a **structural** shift: Ganesh records a draw stream on one `GRContext` and auto-flushes it, whereas Graphite is explicit — you record into a `Recorder`, `Snap` a `Recording`, `InsertRecording`, then `Submit`. The `Recorder` is **per-thread**, so a multi-threaded renderer gives each thread its own recorder.

> [!NOTE]
> Graphite is currently an **offscreen** path in SkiaSharp. If your Ganesh code renders into a view control's render target (`SKGLView`, `SKMetalView`, `SKSwapChainPanel`), there is no Graphite equivalent for that view yet — the [view controls](../views/index.md) still use Ganesh. This migration applies to offscreen rendering.

## Concept mapping

| Ganesh | Graphite |
| --- | --- |
| `GRContext` | `SKGraphiteContext` |
| `GRContext.CreateGl` / `CreateVulkan` / `CreateMetal` / `CreateDirect3D` | `SKGraphiteContext.CreateVulkan` / `CreateMetal` / `CreateDawn` |
| `GRVkBackendContext` / `GRMtlBackendContext` | `SKGraphiteVkBackendContext` / `SKGraphiteMtlBackendContext` / `SKGraphiteDawnBackendContext` |
| `GRSilkNetBackendContext` (typed Vulkan, Silk.NET) — or legacy `GRSharpVkBackendContext` | No typed Graphite wrapper — fill `SKGraphiteVkBackendContext` with raw handles (e.g. Silk.NET `.Handle` values) |
| `SKSurface.Create(context, budgeted, info)` | `context.CreateRecorder()` + `SKSurface.Create(recorder, info)` |
| Draw on `surface.Canvas` | Draw on `surface.Canvas` (unchanged) |
| `context.Flush(submit: true, synchronous: true)` | `recorder.Snap()` + `context.InsertRecording(recording)` + `context.Submit(new SKGraphiteSubmitInfo { Sync = true })` |
| `surface.ReadPixels(...)` (synchronous) | `context.RequestReadPixels(...)` + `context.CheckAsyncWorkCompletion()` (asynchronous) |
| Draw a CPU `SKImage` (auto-uploaded) | Draw a CPU `SKImage` (needs an [image provider](index.md#drawing-cpu-images-the-image-provider)) |
| `GRBackendTexture` / `SKSurface.Create(context, texture, ...)` | `SKGraphiteBackendTexture` / `SKSurface.Create(recorder, backendTexture, colorType)` |
| `SKImage.FromTexture(context, texture, ...)` | `SKImage.FromTexture(recorder, backendTexture, ...)` |
| `image.ToTextureImage(context)` | `image.ToTextureImage(recorder)` |

Notice the pattern: wherever Ganesh takes the **context**, Graphite's per-surface and per-image APIs take the **recorder** instead. OpenGL and Direct3D have no Graphite backend — Graphite targets Vulkan, Metal, and Dawn (WebGPU).

## Before and after

These snippets compare the core offscreen render and readback flow in each backend. The Graphite version
calls the `ReadPixelsFromGraphite` helper from [Reading pixels back](index.md#reading-pixels-back);
that helper is omitted here so the migration steps stay focused on the lifecycle differences.

### Ganesh

```csharp
using System.Runtime.InteropServices;

var info = new SKImageInfo(512, 512, SKColorType.Rgba8888, SKAlphaType.Premul);

using var context = GRContext.CreateMetal(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Ganesh Metal context.");
using var surface = SKSurface.Create(context, budgeted: true, info)
    ?? throw new InvalidOperationException("Unable to create the Ganesh surface.");
using var paint = new SKPaint { Color = SKColors.CornflowerBlue };

surface.Canvas.Clear(SKColors.White);
surface.Canvas.DrawCircle(256, 256, 200, paint);

context.Flush(submit: true, synchronous: true);

// synchronous readback
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

### Graphite

```csharp
var info = new SKImageInfo(512, 512, SKColorType.Rgba8888, SKAlphaType.Premul);

using var context = SKGraphiteContext.CreateMetal(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Graphite Metal context.");
using var recorder = context.CreateRecorder()
    ?? throw new InvalidOperationException("Unable to create the Graphite recorder.");
using var surface = SKSurface.Create(recorder, info)
    ?? throw new InvalidOperationException("Unable to create the Graphite surface.");
using var paint = new SKPaint { Color = SKColors.CornflowerBlue };

surface.Canvas.Clear(SKColors.White);
surface.Canvas.DrawCircle(256, 256, 200, paint);

using (var recording = recorder.Snap()
    ?? throw new InvalidOperationException("Graphite Snap did not succeed."))
{
    if (context.InsertRecording(recording) != SKGraphiteInsertStatus.Success)
        throw new InvalidOperationException("InsertRecording did not succeed.");
}
if (!context.Submit(new SKGraphiteSubmitInfo { Sync = true }))
    throw new InvalidOperationException("Graphite Submit did not succeed.");

// asynchronous readback — helper defined in the Graphite GPU surfaces guide
var pixels = ReadPixelsFromGraphite(context, surface, info);
```

The drawing calls are identical. What changes is the plumbing around them.

## The changes to make

### 1. Replace `Flush` with snap + insert + submit

Ganesh flushes the context directly. Graphite splits this into three steps: `recorder.Snap()` captures the recorded commands into an `SKGraphiteRecording`, `context.InsertRecording(recording)` hands them to the context, and `context.Submit(new SKGraphiteSubmitInfo { Sync = true })` sends them to the GPU and (with `Sync = true`) waits.

Always check that `InsertRecording` returns `SKGraphiteInsertStatus.Success`, and note that `Snap` **resets** the recorder for the next frame. Because the recorder is per-thread, a multi-threaded renderer creates one recorder per thread and submits their recordings to the shared context.

### 2. Replace synchronous `ReadPixels` with asynchronous readback

This is the most important change. Graphite surfaces do **not** support synchronous [`SKSurface.ReadPixels`](xref:SkiaSharp.SKSurface.ReadPixels*) in shipping builds — it returns `false`. Replace it with `RequestReadPixels`, then drive the request to completion with `Submit` and repeated `CheckAsyncWorkCompletion` calls. The callback receives a backend-neutral `SKImageReadPixelsResult`; call `ToArray()`, `ToBitmap()`, or `CopyPlaneTo(...)` on it to get tightly-packed pixels (row padding is stripped for you). See [Reading pixels back](index.md#reading-pixels-back) for the complete helper.

### 3. Give the recorder an image provider for CPU images

Ganesh silently uploads a raster `SKImage` to the GPU the first time you draw it. Graphite does **not** — drawing a non-Graphite image without an *image provider* drops the draw with no error. If your Ganesh code draws decoded/CPU images, create the recorder with an image provider (the ready-made `SKGraphiteImageCache` is the simplest option), or upload each image yourself with `ToTextureImage` first. See [Drawing CPU images](index.md#drawing-cpu-images-the-image-provider).

### 4. Pass the recorder where you used to pass the context

Per-surface and per-image creation moves from the context to the recorder:

- `SKSurface.Create(context, budgeted, info)` → `SKSurface.Create(recorder, info)`
- `SKSurface.Create(context, backendTexture, ...)` → `SKSurface.Create(recorder, backendTexture, colorType)`
- `SKImage.FromTexture(context, ...)` → `SKImage.FromTexture(recorder, ...)`
- `image.ToTextureImage(context)` → `image.ToTextureImage(recorder)`

## Watch out for

- **No OpenGL or Direct3D.** Graphite targets Vulkan, Metal, and Dawn. There is no Direct3D Graphite backend — on Windows, Graphite means Vulkan. If your Ganesh code uses GL or D3D, there is no direct Graphite equivalent; keep using Ganesh, or move to Vulkan/Metal/Dawn.
- **Apple uses Metal, not Vulkan.** On macOS/iOS/Mac Catalyst/tvOS the only Graphite backend is Metal; Vulkan Graphite is Linux/Android/Windows. See the [platform matrix](index.md#backend-platform-support).
- **New Vulkan code should use Silk.NET.** For both Ganesh and Graphite, prefer Silk.NET or raw `libvulkan` over the unmaintained SharpVk binding. Graphite has no typed wrapper, so pass raw handles to `SKGraphiteVkBackendContext`.
- **CPU images need a provider.** A raster `SKImage` drawn without an image provider does not appear. See [Drawing CPU images](index.md#drawing-cpu-images-the-image-provider).
- **Browser (Dawn/WebGPU) can't submit synchronously.** In a WebAssembly host, `Submit(Sync = true)` throws. Submit without syncing and pump `CheckAsyncWorkCompletion`. See [Graphite with Dawn](dawn.md#submit-from-the-browser-loop).
- **Check backend availability.** Use `SKGraphiteContext.IsBackendAvailable` before creating a context, since not every build includes every backend.
- **The recorder is per-thread — and that's a feature.** A single `SKGraphiteRecorder` and its surfaces are single-threaded, but unlike a single-threaded Ganesh `GRContext`, Graphite is built for parallel recording: give each rendering thread its own recorder, then submit their recordings to the shared context (serializing the `InsertRecording`/`Submit` calls). See the threading note in [Graphite GPU surfaces](index.md).

## Related links

- [SkiaSharp APIs](xref:SkiaSharp)
- [Ganesh GPU surfaces](../ganesh/index.md)
- [Graphite GPU surfaces](index.md)
