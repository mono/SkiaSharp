---
title: "Ganesh with Vulkan"
description: "Create a Ganesh Vulkan context from raw handles or Silk.NET objects and describe Vulkan images for SkiaSharp GPU surfaces."
---

# Use Ganesh with Vulkan

Use Vulkan when your host owns the Vulkan instance, physical device, logical device, and graphics queue. Ganesh can consume raw handles through [`GRVkBackendContext`](xref:SkiaSharp.GRVkBackendContext) or typed Silk.NET objects through `GRSilkNetBackendContext`.

## Create a context from raw handles

Supply the queue-family index and a delegate that resolves instance and device functions:

```csharp
GRVkGetProcedureAddressDelegate getProc = (name, instance, device) =>
    System.IntPtr.Zero; // TODO: Forward to vkGetInstanceProcAddr or vkGetDeviceProcAddr.

using var extensions = new GRVkExtensions();
extensions.Initialize(getProc, instanceHandle, physicalDeviceHandle);

using var backendContext = new GRVkBackendContext
{
    VkInstance = instanceHandle,
    VkPhysicalDevice = physicalDeviceHandle,
    VkDevice = deviceHandle,
    VkQueue = graphicsQueueHandle,
    GraphicsQueueIndex = graphicsFamilyIndex,
    MaxAPIVersion = apiVersion,
    Extensions = extensions,
    GetProcedureAddress = getProc,
};

using var context = GRContext.CreateVulkan(backendContext);
```

Replace the zero-returning placeholder with your `vkGetInstanceProcAddr` and `vkGetDeviceProcAddr` integration.

Initialize `GRVkExtensions` with the same function resolver before creating the context. If `Extensions` is `null`, Skia uses an empty extension set and cannot detect extension-dependent capabilities. `MaxAPIVersion` should describe the maximum Vulkan API version the host enabled; if it remains `0`, Skia falls back to the instance version it queries.

## Use the Silk.NET adapter

For new managed Vulkan code, [Silk.NET](https://www.nuget.org/packages/Silk.NET.Vulkan) is the recommended binding. The **SkiaSharp.Vulkan.Silk.NET** package provides a typed adapter:

```csharp
using Silk.NET.Vulkan;

using var extensions = new GRVkExtensions();
extensions.Initialize(getProc, instance, physicalDevice);

using var backendContext = new GRSilkNetBackendContext
{
    VkInstance = instance,
    VkPhysicalDevice = physicalDevice,
    VkDevice = device,
    VkQueue = graphicsQueue,
    GraphicsQueueIndex = graphicsFamily,
    MaxAPIVersion = apiVersion,
    Extensions = extensions,
    GetProcedureAddress = getProc,
    VkPhysicalDeviceFeatures = features,
};

using var context = GRContext.CreateVulkan(backendContext);
```

The legacy **SkiaSharp.Vulkan.SharpVk** package still exposes `GRSharpVkBackendContext`, but SharpVk is unmaintained and should not be a new dependency.

## Describe Vulkan images

Use `GRVkImageInfo` when constructing a [`GRBackendRenderTarget`](xref:SkiaSharp.GRBackendRenderTarget) or [`GRBackendTexture`](xref:SkiaSharp.GRBackendTexture) for an existing Vulkan image. Your Vulkan host remains responsible for allocating the image, synchronizing access, and presenting or releasing it.

After constructing the backend target or texture, return to [wrapping an existing Ganesh resource](index.md#wrapping-an-existing-render-target) for the shared `SKSurface.Create` calls.

## Related links

- [Ganesh GPU surfaces](index.md)
- [Graphite with Vulkan](../graphite/vulkan.md)
- [Skia canvas creation, GPU backend (skia.org)](https://skia.org/docs/user/api/skcanvas_creation/)
