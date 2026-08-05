---
title: "Ganesh with Vulkan"
description: "Create a Ganesh Vulkan context from raw handles or Silk.NET objects and describe Vulkan images for SkiaSharp GPU surfaces."
---

# Use Ganesh with Vulkan

Use Vulkan when your host owns the Vulkan instance, physical device, logical device, and graphics queue. Ganesh can consume raw handles through [`GRVkBackendContext`](xref:SkiaSharp.GRVkBackendContext) or typed Silk.NET objects through `GRSilkNetBackendContext`.

## Create a context from raw handles

Supply the queue-family index and a delegate that resolves instance and device functions:

```csharp
using var backendContext = new GRVkBackendContext
{
    VkInstance = instanceHandle,
    VkPhysicalDevice = physicalDeviceHandle,
    VkDevice = deviceHandle,
    VkQueue = graphicsQueueHandle,
    GraphicsQueueIndex = graphicsFamilyIndex,
    GetProcedureAddress = (name, instance, device) =>
        throw new System.NotImplementedException("Configure Vulkan function lookup."),
};

using var context = GRContext.CreateVulkan(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Ganesh Vulkan context.");
```

Replace the `NotImplementedException` with your `vkGetInstanceProcAddr` and `vkGetDeviceProcAddr` integration.

## Use the Silk.NET adapter

For new managed Vulkan code, [Silk.NET](https://www.nuget.org/packages/Silk.NET.Vulkan) is the recommended binding. The **SkiaSharp.Vulkan.Silk.NET** package provides a typed adapter:

```csharp
using Silk.NET.Vulkan;

using var backendContext = new GRSilkNetBackendContext
{
    VkInstance = instance,
    VkPhysicalDevice = physicalDevice,
    VkDevice = device,
    VkQueue = graphicsQueue,
    GraphicsQueueIndex = graphicsFamily,
    MaxAPIVersion = apiVersion,
    GetProcedureAddress = getProc,
    VkPhysicalDeviceFeatures = features,
};

using var context = GRContext.CreateVulkan(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Ganesh Vulkan context.");
```

The legacy **SkiaSharp.Vulkan.SharpVk** package still exposes `GRSharpVkBackendContext`, but SharpVk is unmaintained and should not be a new dependency.

## Describe Vulkan images

Use `GRVkImageInfo` when constructing a [`GRBackendRenderTarget`](xref:SkiaSharp.GRBackendRenderTarget) or [`GRBackendTexture`](xref:SkiaSharp.GRBackendTexture) for an existing Vulkan image. Your Vulkan host remains responsible for allocating the image, synchronizing access, and presenting or releasing it.

After constructing the backend target or texture, return to [wrapping an existing Ganesh resource](index.md#wrapping-an-existing-render-target) for the shared `SKSurface.Create` calls.

## Related links

- [Ganesh GPU surfaces](index.md)
- [Graphite with Vulkan](../graphite/vulkan.md)
- [Skia canvas creation, GPU backend (skia.org)](https://skia.org/docs/user/api/skcanvas_creation/)
