---
title: "Ganesh GPU surfaces"
description: "Create Ganesh GPU surfaces with OpenGL, Vulkan, Metal, or Direct3D, then render offscreen or wrap a render target or texture."
---

# Ganesh GPU surfaces

*Ganesh* is Skia's classic GPU backend. To draw on the GPU with Ganesh you create a [`GRContext`](xref:SkiaSharp.GRContext) — a handle to a live graphics API context — and then create an [`SKSurface`](xref:SkiaSharp.SKSurface) from it. There are two shapes of GPU surface:

- **Offscreen** surfaces, where Skia allocates and owns the GPU texture. This is the GPU equivalent of a [raster surface](raster-surfaces.md) and is the easiest way to render on the GPU.
- **Wrapped** surfaces, where you hand Skia an existing render target (a window's framebuffer) or texture that some other code created. This is how you draw SkiaSharp content into a swap-chain image you present to the screen.

Ganesh supports four graphics APIs: **OpenGL**, **Vulkan**, **Metal**, and **Direct3D**. The context creation differs per API; everything after that — creating the surface, drawing, flushing, and reading back — is the same.

A `GRContext` and the resources created from it are not thread-safe, so use them from one thread at a time. OpenGL has an additional requirement: the GL context used to create the `GRContext` must be current whenever Skia makes GL calls, including during resource cleanup.

## Creating a context

### OpenGL

For OpenGL, a GL context must already be *current* on the calling thread — SkiaSharp does not create the GL context for you. Once it is current, create a [`GRGlInterface`](xref:SkiaSharp.GRGlInterface) (which resolves the GL entry points) and pass it to [`GRContext.CreateGl`](xref:SkiaSharp.GRContext.CreateGl*):

```csharp
// a platform GL context (WGL / GLX / EGL / CGL) is already current on this thread
using var glInterface = GRGlInterface.Create()
    ?? throw new InvalidOperationException("Unable to create the OpenGL interface.");
using var context = GRContext.CreateGl(glInterface)
    ?? throw new InvalidOperationException("Unable to create the Ganesh OpenGL context.");
```

`GRContext.CreateGl()` also has a parameterless overload that assembles the interface from the current context for you.

### Vulkan

For Vulkan you supply the objects Skia needs through a [`GRVkBackendContext`](xref:SkiaSharp.GRVkBackendContext): the instance, physical device, logical device, a graphics queue and its family index, and a `GetProcedureAddress` delegate that resolves Vulkan functions.

```csharp
using var backendContext = new GRVkBackendContext
{
    VkInstance = instanceHandle,
    VkPhysicalDevice = physicalDeviceHandle,
    VkDevice = deviceHandle,
    VkQueue = graphicsQueueHandle,
    GraphicsQueueIndex = graphicsFamilyIndex,
    GetProcedureAddress = (name, instance, device) => /* vkGetXxxProcAddr */,
};

using var context = GRContext.CreateVulkan(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Ganesh Vulkan context.");
```

For a typed binding that hands you `IntPtr`s to fill in, the recommended managed Vulkan binding is [Silk.NET](https://www.nuget.org/packages/Silk.NET.Vulkan). The **SkiaSharp.Vulkan.Silk.NET** package provides a typed `GRSilkNetBackendContext` that accepts Silk.NET objects directly, so you don't marshal handles yourself:

```csharp
using Silk.NET.Vulkan;

using var backendContext = new GRSilkNetBackendContext
{
    VkInstance = instance,               // Silk.NET.Vulkan.Instance
    VkPhysicalDevice = physicalDevice,   // PhysicalDevice
    VkDevice = device,                   // Device
    VkQueue = graphicsQueue,             // Queue
    GraphicsQueueIndex = graphicsFamily,
    MaxAPIVersion = apiVersion,
    GetProcedureAddress = getProc,       // (name, Instance, Device) => IntPtr
    VkPhysicalDeviceFeatures = features, // PhysicalDeviceFeatures
};

using var context = GRContext.CreateVulkan(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Ganesh Vulkan context.");
```

Silk.NET is the maintained, cross-platform binding and is the recommended choice for new Vulkan code. The legacy **SkiaSharp.Vulkan.SharpVk** package still provides `GRSharpVkBackendContext`, but new code should not take a dependency on the unmaintained SharpVk binding. Because `GRVkBackendContext` takes raw handles, you can also pair it with another binding or raw `libvulkan` P/Invoke.

### Metal

On Apple platforms, build a [`GRMtlBackendContext`](xref:SkiaSharp.GRMtlBackendContext) from an `MTLDevice` and an `MTLCommandQueue`. On the Apple target frameworks you can assign the typed `IMTLDevice`/`IMTLCommandQueue` objects; from other targets, assign their native handles:

```csharp
using var backendContext = new GRMtlBackendContext
{
    DeviceHandle = mtlDeviceHandle,
    QueueHandle = mtlCommandQueueHandle,
};

using var context = GRContext.CreateMetal(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Ganesh Metal context.");
```

### Direct3D

On Windows, build a [`GRD3DBackendContext`](xref:SkiaSharp.GRD3DBackendContext) from your DXGI adapter, D3D12 device, and command queue:

```csharp
using var backendContext = new GRD3DBackendContext
{
    Adapter = adapterHandle,
    Device = d3d12DeviceHandle,
    Queue = commandQueueHandle,
};

using var context = GRContext.CreateDirect3D(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Ganesh Direct3D context.");
```

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

After flushing, you can read the pixels back synchronously with the same `ReadPixels` call used in the [raster case](raster-surfaces.md#getting-the-result-out):

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

The whole offscreen loop looks the same regardless of which API you created the context with. For example, over OpenGL:

```csharp
using var glInterface = GRGlInterface.Create()
    ?? throw new InvalidOperationException("Unable to create the OpenGL interface.");
using var context = GRContext.CreateGl(glInterface)
    ?? throw new InvalidOperationException("Unable to create the Ganesh OpenGL context.");
using var surface = SKSurface.Create(context, budgeted: true, info)
    ?? throw new InvalidOperationException("Unable to create the Ganesh surface.");

surface.Canvas.Clear(SKColors.White);
// ... draw ...
context.Flush(submit: true, synchronous: true);
```

## Wrapping an existing render target

To draw SkiaSharp content into a render target that already exists — most often a window's framebuffer or a swap-chain image — describe it to Skia with a [`GRBackendRenderTarget`](xref:SkiaSharp.GRBackendRenderTarget) and wrap it with [`SKSurface.Create`](xref:SkiaSharp.SKSurface.Create*).

For OpenGL, you build the backend render target from the currently bound framebuffer. This is exactly what the built-in `SKGLView` controls do internally:

```csharp
// query the currently bound framebuffer, stencil bits, and sample count from GL,
// then describe it to Skia
var glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());
using var renderTarget = new GRBackendRenderTarget(width, height, samples, stencil, glInfo);

using var surface = SKSurface.Create(context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType)
    ?? throw new InvalidOperationException("Unable to wrap the render target.");

surface.Canvas.Clear(SKColors.White);
// ... draw the frame ...

surface.Canvas.Flush();
context.Flush();
// then present/swap buffers with your windowing code
```

`GRBackendRenderTarget` also has constructors for Vulkan (`GRVkImageInfo`), Metal (`GRMtlTextureInfo`), and Direct3D (`GRD3DTextureResourceInfo`), so you can wrap a swap-chain image from any of the supported APIs.

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
- [Raster surfaces](raster-surfaces.md)
- [Surfaces in the SkiaSharp Views](views-surfaces.md)
- [Graphite offscreen surfaces](graphite-surfaces.md)
- [Skia canvas creation, GPU backend (skia.org)](https://skia.org/docs/user/api/skcanvas_creation/)
