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

## Backend platform support

Which Graphite backend you use is determined by the platform:

| Backend | Platforms |
| --- | --- |
| **Metal** | macOS, iOS (including the iOS Simulator on Apple Silicon), Mac Catalyst, tvOS |
| **Vulkan** | Linux, Android, Windows |
| **Dawn** (WebGPU) | WebAssembly / browser only |

A few consequences worth calling out:

- **Apple platforms use Metal, not Vulkan.** The native Skia build for Apple is not compiled with Vulkan, so on macOS/iOS/Mac Catalyst/tvOS the only Graphite backend is Metal.
- **On Windows, Graphite means Vulkan.** There is **no Direct3D Graphite backend** in SkiaSharp — a D3D path would only exist as Graphite→Dawn→D3D12, which is not exposed. If you need D3D specifically, use [Ganesh](ganesh-surfaces.md) with `GRContext.CreateDirect3D`.
- **Dawn is browser-only.** It is the WebAssembly path and carries an extra submission constraint — see [Dawn in the browser](#dawn-in-the-browser).

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

Graphite Vulkan is available on **Linux, Android, and Windows** (not Apple — see the [platform matrix](#backend-platform-support)). There is no typed Graphite-specific Vulkan wrapper: you fill in the binding-neutral `SKGraphiteVkBackendContext` with raw handles and a `GetProcedureAddress` delegate that resolves Vulkan functions.

```csharp
using var backendContext = new SKGraphiteVkBackendContext
{
    VkInstance = instanceHandle,
    VkPhysicalDevice = physicalDeviceHandle,
    VkDevice = deviceHandle,
    VkQueue = graphicsQueueHandle,
    GraphicsQueueIndex = graphicsFamilyIndex,
    MaxApiVersion = apiVersion,
    GetProcedureAddress = (name, instance, device) => /* vkGetInstance/DeviceProcAddr */,
};

using var context = SKGraphiteContext.CreateVulkan(backendContext);
```

Because the handles are raw `IntPtr`s, you can source them from any Vulkan binding. The recommended one is [Silk.NET](https://www.nuget.org/packages/Silk.NET.Vulkan) — feed its objects' `.Handle` values straight in:

```csharp
using Silk.NET.Vulkan;

using var backendContext = new SKGraphiteVkBackendContext
{
    VkInstance = instance.Handle,
    VkPhysicalDevice = physicalDevice.Handle,
    VkDevice = device.Handle,
    VkQueue = graphicsQueue.Handle,
    GraphicsQueueIndex = graphicsFamily,
    MaxApiVersion = apiVersion,
    GetProcedureAddress = getProc,
};

using var context = SKGraphiteContext.CreateVulkan(backendContext);
```

> [!NOTE]
> Steer new Vulkan code to Silk.NET (or raw `libvulkan` P/Invoke). The older SharpVk binding is unmaintained and Windows/Linux-only (it throws on Android), and there is no SharpVk wrapper for the Graphite path — Graphite always takes the raw handles above.

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

> [!NOTE]
> Graphite Metal works on the **iOS and tvOS Simulator on Apple Silicon** (it is backed by the host's Apple-Silicon GPU). Be aware that the simulator's `MTLDevice` under-reports its capabilities — it advertises only `Apple1`/`Apple2`/`Common1`, not `Apple7+`/`Mac2` — so a naive `supportsFamily:` capability gate would wrongly skip it even though rendering works. Don't gate simulator support on the reported GPU family.

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

The callback receives an `SKGraphiteAsyncReadResult` whose planes may be **row-padded**, so copy row-by-row using `GetPlaneRowBytes(0)` rather than assuming tightly packed pixels. If you don't need to see the padding handling spelled out, `SKGraphiteAsyncReadResult.CopyPlaneTo(planeIndex, destination, rowCount)` performs exactly this padding-aware copy for you; the manual loop above is shown to make the row padding explicit. A shorter `RequestReadPixels` overload uses default rescaling; a longer overload lets you pass an [`SKGraphiteRescaleGamma`](#status-and-enums) and [`SKGraphiteRescaleMode`](#status-and-enums) when you want the read to also rescale the image.

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

> [!IMPORTANT]
> **Vulkan surfaces need input-attachment usage.** When you wrap an externally-created Vulkan `VkImage` as a Graphite **surface** (a render target), the texture's `SKGraphiteVkTextureInfo.ImageUsageFlags` must include **both** `VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT` (`0x10`) **and** `VK_IMAGE_USAGE_INPUT_ATTACHMENT_BIT` (`0x80`). Skia Graphite requires input-attachment usage on every color-renderable Vulkan texture; without it, `SKSurface.Create` returns **null** (the texture is not considered renderable). A typical renderable usage mask is `TRANSFER_SRC | TRANSFER_DST | SAMPLED | COLOR_ATTACHMENT | INPUT_ATTACHMENT` = `0x97`. This applies to **surfaces only** — a texture you only *sample* from as an image (see [`SKImage.FromTexture`](#using-textures-as-images)) needs just `SAMPLED`.

### Releasing a wrapped texture

When Skia is done with a wrapped backend texture it can notify you so you can free the caller-owned native texture. The wrap overloads accept a parameterless `SKGraphiteReleaseDelegate`.

> [!IMPORTANT]
> The release callback fires when Skia destroys **its** reference to the wrapped texture — which is **after** the wrapping surface (or image) is disposed **and** the pending GPU work has drained. Disposing the surface alone is not enough. To force the callback to run, submit and pump completion, then free cached GPU resources:

```csharp
using var context  = SKGraphiteContext.CreateVulkan(backendContext);
using var recorder = context.CreateRecorder();

// A RENDERABLE backend texture needs COLOR_ATTACHMENT (0x10) + INPUT_ATTACHMENT (0x80).
var vkInfo = new SKGraphiteVkTextureInfo
{
    Format = 37,           // VK_FORMAT_R8G8B8A8_UNORM
    ImageTiling = 0,       // VK_IMAGE_TILING_OPTIMAL
    SampleCount = 1,
    AspectMask = 1,        // VK_IMAGE_ASPECT_COLOR_BIT
    SharingMode = 0,       // VK_SHARING_MODE_EXCLUSIVE
    ImageUsageFlags = 0x1 | 0x2 | 0x4 | 0x10 | 0x80, // = 0x97
};
using var texInfo = SKGraphiteTextureInfo.CreateVulkan(vkInfo);
using var backendTexture = recorder.CreateBackendTexture(width, height, texInfo);

var released = false;
using (var surface = SKSurface.Create(
           recorder, backendTexture, SKColorType.Rgba8888,
           colorSpace: null, props: null,
           releaseProc: () => released = true))
{
    surface.Canvas.Clear(SKColors.Red);
    using var recording = recorder.Snap();
    context.InsertRecording(recording);
    context.Submit(new SKGraphiteSubmitInfo { Sync = true });
} // surface disposed here — but the texture is not released yet

// Drain deferred GPU work so Skia actually destroys the wrapped texture → releaseProc fires
context.Submit(new SKGraphiteSubmitInfo { Sync = true });
for (var i = 0; i < 100; i++)
    context.CheckAsyncWorkCompletion();
context.FreeGpuResources();
// released == true

recorder.DeleteBackendTexture(backendTexture); // free the caller-owned texture
```

The example above uses `recorder.CreateBackendTexture` for brevity; to wrap a texture your own code allocated, build the `SKGraphiteBackendTexture` with `SKGraphiteBackendTexture.CreateVulkan/CreateMetal/CreateDawn` instead — the release flow is identical. `SKImage.FromTexture` has the same release-callback overload and fires the same way (on image dispose plus GPU drain); a sample-only image needs only `SAMPLED` usage, not `INPUT_ATTACHMENT`.

## Using textures as images

You can also move between GPU textures and [`SKImage`](xref:SkiaSharp.SKImage) objects on a recorder:

- [`SKImage.FromTexture`](xref:SkiaSharp.SKImage.FromTexture*) wraps a backend texture as a sampling image you can draw onto a surface:

  ```csharp
  using var image = SKImage.FromTexture(
      recorder, backendTexture, SKColorType.Rgba8888, SKAlphaType.Premul);
  ```

  A longer overload also takes a color space and a parameterless `SKGraphiteReleaseDelegate` that fires once when Skia releases the wrapped texture.

- [`ToTextureImage`](xref:SkiaSharp.SKImage.ToTextureImage*) uploads an existing image (for example, one decoded on the CPU) into a GPU-backed image on the recorder:

  ```csharp
  using var gpuImage = cpuImage.ToTextureImage(recorder);
  ```

## Drawing CPU images: the image provider

> [!IMPORTANT]
> Unlike Ganesh, Graphite does **not** automatically upload a non-Graphite `SKImage` to the GPU. If you draw a raster/CPU-backed `SKImage` — for example one you decoded with `SKImage.FromEncodedData` — onto a Graphite surface **without an image provider, the draw is silently dropped**: nothing appears and no error is raised.

There are two ways to handle this. You can upload each image yourself with [`ToTextureImage`](#using-textures-as-images) and draw the GPU-backed result. Or you can give the recorder an *image provider* callback that uploads CPU images on demand, so ordinary `DrawImage` calls just work.

Pass the callback to the `CreateRecorder` overload that accepts one. SkiaSharp ships a ready-made `SKGraphiteImageCache` whose `FindOrCreate` method implements the callback (uploading via `ToTextureImage`) and caches the results so repeated draws of the same image don't re-upload:

```csharp
var imageCache = new SKGraphiteImageCache();

using var recorder = context.CreateRecorder(
    recorderBudgetBytes: -1,
    findOrCreate: imageCache.FindOrCreate,     // uploads + caches CPU images on demand
    findOrCreateDispose: imageCache.Dispose);  // released with the recorder

using var surface = SKSurface.Create(recorder, info);
surface.Canvas.DrawImage(cpuImage, 0, 0);      // now uploaded through the provider
```

The callback has the signature `SKImage SKGraphiteFindOrCreateImageDelegate(SKGraphiteRecorder recorder, SKImage image, bool mipmapped)`, and returning `null` drops that image's draw. Provide your own delegate if you want custom upload or caching behaviour; otherwise `SKGraphiteImageCache` is the simplest correct default.

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

> [!NOTE]
> **Dawn bring-up on WASM.** When building the `SKGraphiteDawnBackendContext` in the browser (the emdawnwebgpu port), you must create a **real** `WGPUInstance` via `wgpuCreateInstance` and register the device and queue under *that* instance as their event-source parent. If the instance is a placeholder or the device/queue are registered under a different instance, `SKGraphiteContext.CreateDawn` deadlocks — emdawnwebgpu's event manager waits on a mismatched instance and never completes.

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
