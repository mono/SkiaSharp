---
title: "Ganesh GPU Surfaces"
description: "Create GPU-backed SKSurface objects with the Ganesh backend in SkiaSharp. Build a GRContext for OpenGL, Vulkan, Metal, or Direct3D, then render fully offscreen or wrap an existing render target or texture."
---

# Ganesh GPU Surfaces

_Render on the GPU with a `GRContext` and the Ganesh backend_

*Ganesh* is Skia's classic GPU backend. To draw on the GPU with Ganesh you create a [`GRContext`](xref:SkiaSharp.GRContext) — a handle to a live graphics API context — and then create an [`SKSurface`](xref:SkiaSharp.SKSurface) from it. There are two shapes of GPU surface:

- **Offscreen** surfaces, where Skia allocates and owns the GPU texture. This is the GPU equivalent of a [raster surface](raster-surfaces.md) and is the easiest way to render on the GPU.
- **Wrapped** surfaces, where you hand Skia an existing render target (a window's framebuffer) or texture that some other code created. This is how you draw SkiaSharp content into a swap-chain image you present to the screen.

Ganesh supports four graphics APIs: **OpenGL**, **Vulkan**, **Metal**, and **Direct3D**. The context creation differs per API; everything after that — creating the surface, drawing, flushing, and reading back — is the same.

> [!NOTE]
> A `GRContext` is bound to the graphics context that was current when you created it, and neither it nor its surfaces are thread-safe. Create and use them on the thread that owns the graphics context.

## Creating a context

### OpenGL

For OpenGL, a GL context must already be *current* on the calling thread — SkiaSharp does not create the GL context for you. Once it is current, create a [`GRGlInterface`](xref:SkiaSharp.GRGlInterface) (which resolves the GL entry points) and pass it to [`GRContext.CreateGl`](xref:SkiaSharp.GRContext.CreateGl*):

```csharp
// a platform GL context (WGL / GLX / EGL / CGL) is already current on this thread
using var glInterface = GRGlInterface.Create();
using var context = GRContext.CreateGl(glInterface);
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

using var context = GRContext.CreateVulkan(backendContext);
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
    GetProcedureAddress = getProc,       // (name, Instance, Device) => IntPtr
    VkPhysicalDeviceFeatures = features, // PhysicalDeviceFeatures
};

using var context = GRContext.CreateVulkan(backendContext);
```

> [!NOTE]
> Silk.NET is the maintained, cross-platform binding and is the recommended choice for new Vulkan code. An older **SkiaSharp.Vulkan.SharpVk** package with a `GRSharpVkBackendContext` still exists, but SharpVk is effectively unmaintained and only works on Windows and Linux (it throws on Android). Because `GRVkBackendContext` takes raw handles, you can also pair it with any other binding — or raw `libvulkan` P/Invoke — without a wrapper package.

### Metal

On Apple platforms, build a [`GRMtlBackendContext`](xref:SkiaSharp.GRMtlBackendContext) from an `MTLDevice` and an `MTLCommandQueue`. On the Apple target frameworks you can assign the typed `IMTLDevice`/`IMTLCommandQueue` objects; from other targets, assign their native handles:

```csharp
using var backendContext = new GRMtlBackendContext
{
    DeviceHandle = mtlDeviceHandle,
    QueueHandle = mtlCommandQueueHandle,
};

using var context = GRContext.CreateMetal(backendContext);
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

using var context = GRContext.CreateDirect3D(backendContext);
```

## Rendering offscreen

The simplest GPU surface is an offscreen one: describe the image with an [`SKImageInfo`](xref:SkiaSharp.SKImageInfo) and let Skia allocate the backing GPU texture. Pass `budgeted: true` so the texture counts against the context's resource budget and can be recycled.

```csharp
var info = new SKImageInfo(512, 512, SKColorType.Rgba8888, SKAlphaType.Premul);

using var surface = SKSurface.Create(context, budgeted: true, info);

surface.Canvas.Clear(SKColors.White);
surface.Canvas.DrawCircle(256, 256, 200, new SKPaint { Color = SKColors.CornflowerBlue });

// push the recorded work to the GPU and wait for it to finish
context.Flush(submit: true, synchronous: true);
```

After flushing, you can read the pixels back synchronously — GPU readback with Ganesh works exactly like the [raster case](raster-surfaces.md#getting-the-result-out):

```csharp
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

The whole offscreen loop looks the same regardless of which API you created the context with. For example, over OpenGL:

```csharp
using var glInterface = GRGlInterface.Create();
using var context = GRContext.CreateGl(glInterface);
using var surface = SKSurface.Create(context, budgeted: true, info);

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

using var surface = SKSurface.Create(context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType);

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
    context, backendTexture, GRSurfaceOrigin.TopLeft, sampleCount: 0, colorType);
```

You can also wrap a texture as a *sampling* [`SKImage`](xref:SkiaSharp.SKImage) with [`SKImage.FromTexture`](xref:SkiaSharp.SKImage.FromTexture*) when you want to draw an existing GPU texture *onto* a surface rather than *into* it.

## Cleaning up

Dispose your surfaces and the `GRContext` when you are done, and make sure the graphics context they were created against is still current at disposal time. Disposing the `GRContext` frees all the GPU resources Skia allocated through it.

## Related Links

- [SkiaSharp APIs](/dotnet/api/skiasharp)
- [Raster Surfaces](raster-surfaces.md)
- [Surfaces in the SkiaSharp Views](views-surfaces.md)
- [Graphite Offscreen Surfaces](graphite-surfaces.md)
- [Skia canvas creation, GPU backend (skia.org)](https://skia.org/docs/user/api/skcanvas_creation/)
