---
title: "Graphite Offscreen Surfaces"
description: "Render on the GPU with SkiaSharp's new Graphite backend. Create an SKGraphiteContext for Vulkan, Metal, or Dawn (WebGPU), record and submit drawing through a recorder and recording, wrap external GPU textures, and read pixels back through the asynchronous readback path."
---

# Graphite Offscreen Surfaces

_Render on the GPU with the new Graphite backend_

*Graphite* is Skia's newer GPU backend, built on modern explicit graphics APIs. In SkiaSharp, Graphite is currently an **offscreen** rendering path: you create a context, record drawing into a surface, submit that recording to the GPU, and read the result back yourself. It does not yet drive any of the [view controls](views-surfaces.md).

Graphite differs from [Ganesh](ganesh-surfaces.md) in two important ways:

- **Drawing is recorded, not flushed.** You draw onto a surface's canvas as usual, but instead of flushing a context you *snap* a **recording** from a **recorder** and *insert* that recording into the context, then *submit* it.
- **Reading pixels back is asynchronous.** Graphite surfaces do not support the synchronous `SKSurface.ReadPixels` you use with raster and Ganesh surfaces. You request a readback and drive it to completion. This is the single most important thing to get right — see [Reading pixels back](#reading-pixels-back).

Graphite supports three backends: **Vulkan**, **Metal**, and **Dawn** (WebGPU).

> [!NOTE]
> Like Ganesh, an `SKGraphiteContext`, its recorders, and its surfaces are not thread-safe. Create and use them on a single thread that owns the underlying graphics device.

## Checking a backend is available

A given build of SkiaSharp may not include every Graphite backend. Before creating a context, you can check whether a backend is compiled in with `SKGraphiteContext.IsBackendAvailable`:

```csharp
if (SKGraphiteContext.IsBackendAvailable(SKGraphiteBackend.Metal))
{
    // safe to call SKGraphiteContext.CreateMetal
}
```

## Creating a context

You create a context from a backend context that carries the native device objects for your API. Each factory also has an overload that takes an [`SKGraphiteContextOptions`](#context-options).

### Vulkan

Fill in a `SKGraphiteVkBackendContext` with your instance, physical device, device, queue, and graphics-queue index, plus a `GetProcedureAddress` delegate that resolves Vulkan functions:

```csharp
using var backendContext = new SKGraphiteVkBackendContext
{
    VkInstance = instanceHandle,
    VkPhysicalDevice = physicalDeviceHandle,
    VkDevice = deviceHandle,
    VkQueue = graphicsQueueHandle,
    GraphicsQueueIndex = graphicsFamilyIndex,
    GetProcedureAddress = (name, instance, device) => /* vkGetXxxProcAddr */,
};

using var context = SKGraphiteContext.CreateVulkan(backendContext);
```

If you use the [SharpVk](https://www.nuget.org/packages/SharpVk) managed binding, the **SkiaSharp.Vulkan.SharpVk** package ships a typed `SKGraphiteSharpVkBackendContext` that accepts SharpVk objects directly:

```csharp
using var backendContext = new SKGraphiteSharpVkBackendContext
{
    VkInstance = instance,
    VkPhysicalDevice = physicalDevice,
    VkDevice = device,
    VkQueue = queue,
    GraphicsQueueIndex = graphicsFamily,
    GetProcedureAddress = (name, instance, device) => /* ... */,
};

using var context = SKGraphiteContext.CreateVulkan(backendContext);
```

### Metal

On Apple platforms, supply an `MTLDevice` and `MTLCommandQueue`. From the Apple target frameworks you can assign the typed `Device`/`Queue` (`IMTLDevice`/`IMTLCommandQueue`) properties; from other targets, assign the native handles:

```csharp
using var backendContext = new SKGraphiteMtlBackendContext
{
    MtlDevice = mtlDeviceHandle,
    MtlQueue = mtlCommandQueueHandle,
};

using var context = SKGraphiteContext.CreateMetal(backendContext);
```

### Dawn (WebGPU)

For Dawn, supply the WebGPU instance, device, and queue handles:

```csharp
using var backendContext = new SKGraphiteDawnBackendContext
{
    WgpuInstance = instanceHandle,
    WgpuDevice = deviceHandle,
    WgpuQueue = queueHandle,
};

using var context = SKGraphiteContext.CreateDawn(backendContext);
```

Dawn is the backend used in the browser (WebAssembly), which imposes an extra constraint on submission — see [Dawn in the browser](#dawn-in-the-browser).

## The render loop

Once you have a context, the Graphite drawing loop is: create a **recorder**, create a surface from it, draw, **snap** a recording, **insert** it, and **submit**.

```csharp
var info = new SKImageInfo(512, 512, SKColorType.Rgba8888, SKAlphaType.Premul);

using var recorder = context.CreateRecorder();
using var surface = SKSurface.Create(recorder, info);

// draw exactly as you would on any other surface
surface.Canvas.Clear(SKColors.White);
surface.Canvas.DrawCircle(256, 256, 200, new SKPaint { Color = SKColors.CornflowerBlue });

// capture everything recorded so far
using var recording = recorder.Snap();

// hand the recording to the context and submit it to the GPU
if (context.InsertRecording(recording) != SKGraphiteInsertStatus.Success)
    throw new InvalidOperationException("Graphite InsertRecording did not succeed.");

context.Submit(new SKGraphiteSubmitInfo { Sync = true });
```

A few things to note:

- `CreateRecorder` returns an `SKGraphiteRecorder`. A recorder is a reusable unit of work capture; you create the surface from it, not from the context directly.
- `Snap` produces an `SKGraphiteRecording` — an immutable list of GPU commands. Snapping resets the recorder so it can record the next frame.
- `InsertRecording` returns an [`SKGraphiteInsertStatus`](#status-and-enums); always confirm it is `Success`.
- `Submit(new SKGraphiteSubmitInfo { Sync = true })` flushes the work to the GPU and, with `Sync = true`, waits for it to finish. It returns `false` if submission failed.

## Reading pixels back

> [!IMPORTANT]
> Graphite surfaces do **not** support the synchronous [`SKSurface.ReadPixels`](xref:SkiaSharp.SKSurface.ReadPixels*) used with [raster](raster-surfaces.md) and [Ganesh](ganesh-surfaces.md) surfaces — it returns `false`. To get pixels off a Graphite surface you must use the **asynchronous** readback path. This is the number-one thing to get right when porting existing code.

Call `RequestReadPixels` with the surface, the destination `SKImageInfo`, the source rectangle, and a callback. Then drive the request to completion by submitting and repeatedly calling `CheckAsyncWorkCompletion` until the callback fires:

```csharp
var dstInfo = new SKImageInfo(info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);

byte[] pixels = null;
var done = false;

context.RequestReadPixels(
    surface,
    dstInfo,
    new SKRectI(0, 0, dstInfo.Width, dstInfo.Height),
    result =>
    {
        done = true;
        if (result is null || result.PlaneCount < 1)
            return;

        var src = result.GetPlaneData(0);
        if (src == IntPtr.Zero)
            return;

        // the returned plane may have per-row padding; copy row-by-row into a
        // tightly-packed buffer, dropping any padding
        var buffer = new byte[dstInfo.BytesSize];
        var srcRowBytes = result.GetPlaneRowBytes(0);
        var rowBytes = Math.Min(srcRowBytes, dstInfo.RowBytes);
        for (var y = 0; y < dstInfo.Height; y++)
            Marshal.Copy(src + (y * srcRowBytes), buffer, y * dstInfo.RowBytes, rowBytes);

        pixels = buffer;
    });

// flush the queued readback and wait, then pump the context until the callback runs
context.Submit(new SKGraphiteSubmitInfo { Sync = true });
for (var i = 0; i < 10_000 && !done; i++)
    context.CheckAsyncWorkCompletion();

if (!done || pixels is null)
    throw new InvalidOperationException("Graphite async readback did not complete.");
```

The callback receives an `SKGraphiteAsyncReadResult` whose planes may be **row-padded**, so copy row-by-row using `GetPlaneRowBytes(0)` rather than assuming tightly packed pixels. A shorter `RequestReadPixels` overload uses default rescaling; a longer overload lets you pass an [`SKGraphiteRescaleGamma`](#status-and-enums) and [`SKGraphiteRescaleMode`](#status-and-enums) when you want the read to also rescale the image.

## Wrapping an external GPU texture

Instead of letting Skia allocate the surface's texture, you can render into a GPU texture your own code created. Describe it with an `SKGraphiteBackendTexture` and create a surface that wraps it:

```csharp
// Metal example: wrap an existing MTLTexture handle
using var backendTexture = SKGraphiteBackendTexture.CreateMetal(width, height, mtlTextureHandle);
using var surface = SKSurface.Create(recorder, backendTexture, SKColorType.Rgba8888);

surface.Canvas.Clear(SKColors.White);
// ... draw, then Snap / InsertRecording / Submit as above ...
```

There are matching factory methods for each backend:

- `SKGraphiteBackendTexture.CreateVulkan(width, height, info, imageLayout, queueFamilyIndex, vkImage)`
- `SKGraphiteBackendTexture.CreateMetal(width, height, mtlTexture)`
- `SKGraphiteBackendTexture.CreateDawn(wgpuTexture)`

## Using textures as images

You can also move between GPU textures and [`SKImage`](xref:SkiaSharp.SKImage) objects on a recorder:

- [`SKImage.FromTexture`](xref:SkiaSharp.SKImage.FromTexture*) wraps a backend texture as a sampling image you can draw onto a surface:

  ```csharp
  using var image = SKImage.FromTexture(
      recorder, backendTexture, SKColorType.Rgba8888, SKAlphaType.Premul);
  ```

- [`ToTextureImage`](xref:SkiaSharp.SKImage.ToTextureImage*) uploads an existing image (for example, one decoded on the CPU) into a GPU-backed image on the recorder:

  ```csharp
  using var gpuImage = cpuImage.ToTextureImage(recorder);
  ```

## Dawn in the browser

When Graphite runs over Dawn in a browser/WebAssembly host, the Dawn event loop cannot be pumped from inside a managed call, so **synchronous submission is not allowed**. Calling `Submit` with `Sync = true` there throws an `InvalidOperationException`.

In that environment, submit without syncing and drive any readbacks with `CheckAsyncWorkCompletion`:

```csharp
// browser / WASM (non-yielding Dawn)
context.InsertRecording(recording);
context.Submit(new SKGraphiteSubmitInfo { Sync = false });

// later, pump completion instead of blocking
context.CheckAsyncWorkCompletion();
```

## Context options

The `Create*` factories accept an optional `SKGraphiteContextOptions`. The most commonly useful field is `InternalMultisampleCount` (the internal MSAA sample count), which must be `0` (use Skia's default) or one of `1`, `2`, `4`, `8`, or `16`; other values are rejected. Other options include a GPU byte budget and driver-workaround toggles.

```csharp
var options = new SKGraphiteContextOptions { InternalMultisampleCount = 4 };
using var context = SKGraphiteContext.CreateMetal(backendContext, options);
```

## Managing resources

An `SKGraphiteContext` exposes a few properties and methods for inspecting and managing GPU resources:

- `Backend`, `IsDeviceLost`, `MaxTextureSize`, and `SupportsProtectedContent` report the state of the underlying device.
- `MaxBudgetedBytes` gets or sets the GPU memory budget; `CurrentBudgetedBytes` reports current usage.
- `FreeGpuResources()` releases cached GPU resources; `PerformDeferredCleanup(TimeSpan)` purges resources unused for longer than the given duration.

Dispose recordings, surfaces, recorders, and the context when you are done. The context owns the GPU resources allocated through it.

## Status and enums

Graphite uses a handful of enums:

- `SKGraphiteBackend` — `Dawn`, `Metal`, `Vulkan`, or `Unknown`.
- `SKGraphiteInsertStatus` — the result of `InsertRecording`; `Success` plus failure reasons such as `InvalidRecording`, `AddCommandsFailed`, and `OutOfOrderRecording`.
- `SKGraphiteRescaleGamma` — `Src` or `Linear`, for the optional readback rescale.
- `SKGraphiteRescaleMode` — `Nearest`, `RepeatedLinear`, or `RepeatedCubic`, for the optional readback rescale.

## Related Links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
- [Ganesh GPU Surfaces](ganesh-surfaces.md)
- [Migrating from Ganesh to Graphite](graphite-migration.md)
