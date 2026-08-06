---
title: "Graphite with Vulkan"
description: "Create a Graphite Vulkan context, wrap Vulkan images with the required usage flags, and release backend textures safely."
---

# Use Graphite with Vulkan

Graphite Vulkan is available on Linux, Android, and Windows. Apple builds use [Metal](metal.md), not Vulkan. Before creating a context, confirm the backend is compiled into the current SkiaSharp native library:

```csharp
if (!SKGraphiteContext.IsBackendAvailable(SKGraphiteBackend.Vulkan))
    throw new PlatformNotSupportedException("Graphite Vulkan is unavailable.");
```

## Create the Graphite context

Fill `SKGraphiteVkBackendContext` with the Vulkan objects owned by your host:

```csharp
using var backendContext = new SKGraphiteVkBackendContext
{
    VkInstance = instanceHandle,
    VkPhysicalDevice = physicalDeviceHandle,
    VkDevice = deviceHandle,
    VkQueue = graphicsQueueHandle,
    GraphicsQueueIndex = graphicsFamilyIndex,
    MaxApiVersion = apiVersion,
    GetProcedureAddress = (name, instance, device) =>
        System.IntPtr.Zero, // TODO: Forward to vkGetInstanceProcAddr or vkGetDeviceProcAddr.
};

using var context = SKGraphiteContext.CreateVulkan(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Graphite Vulkan context.");
```

Replace the zero-returning placeholder with your `vkGetInstanceProcAddr` and `vkGetDeviceProcAddr` integration.

There is no typed Graphite-specific Vulkan adapter. With [Silk.NET](https://www.nuget.org/packages/Silk.NET.Vulkan), pass each object's `.Handle` value:

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

using var context = SKGraphiteContext.CreateVulkan(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Graphite Vulkan context.");
```

Use Silk.NET or raw `libvulkan` P/Invoke for new code. The legacy SharpVk binding is unmaintained.

## Wrap and release Vulkan images

Use `SKGraphiteBackendTexture.CreateVulkan` to describe an existing `VkImage`. A Vulkan image wrapped as a **renderable surface** must include both `VK_IMAGE_USAGE_COLOR_ATTACHMENT_BIT` (`0x10`) and `VK_IMAGE_USAGE_INPUT_ATTACHMENT_BIT` (`0x80`). Without them, `SKSurface.Create` returns `null`.

A typical renderable mask is `TRANSFER_SRC | TRANSFER_DST | SAMPLED | COLOR_ATTACHMENT | INPUT_ATTACHMENT` (`0x97`). An image used only for sampling needs `SAMPLED`, not `INPUT_ATTACHMENT`.

When Skia is done with a wrapped texture, the parameterless `SKGraphiteReleaseDelegate` fires after the wrapper is disposed and pending GPU work has drained. Disposing the wrapper alone does not mean the allocation is safe to delete.

### Release a wrapped texture

Starting with an initialized Vulkan `backendContext`, this sequence allocates a backend texture through the recorder, wraps it, submits work, waits for the release callback, and deletes the allocation while the recorder is still alive:

```csharp
const int width = 256;
const int height = 256;

using var context = SKGraphiteContext.CreateVulkan(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Graphite Vulkan context.");
using var recorder = context.CreateRecorder()
    ?? throw new InvalidOperationException("Unable to create the Graphite recorder.");

var vkInfo = new SKGraphiteVkTextureInfo
{
    Format = 37,           // VK_FORMAT_R8G8B8A8_UNORM
    ImageTiling = 0,       // VK_IMAGE_TILING_OPTIMAL
    SampleCount = 1,
    AspectMask = 1,        // VK_IMAGE_ASPECT_COLOR_BIT
    SharingMode = 0,       // VK_SHARING_MODE_EXCLUSIVE
    ImageUsageFlags = 0x1 | 0x2 | 0x4 | 0x10 | 0x80, // = 0x97
};
using var textureInfo = SKGraphiteTextureInfo.CreateVulkan(vkInfo)
    ?? throw new InvalidOperationException("Unable to create the Vulkan texture info.");
using var backendTexture = recorder.CreateBackendTexture(width, height, textureInfo)
    ?? throw new InvalidOperationException("Unable to create the backend texture.");

var released = false;
var wrapped = false;
var contextUsable = true;
SKSurface surface = null;
try
{
    surface = SKSurface.Create(
        recorder, backendTexture, SKColorType.Rgba8888,
        colorSpace: null, props: null,
        releaseProc: () => released = true);
    if (surface is null)
        throw new InvalidOperationException("Unable to wrap the backend texture.");

    wrapped = true;
    surface.Canvas.Clear(SKColors.Red);
    using var recording = recorder.Snap()
        ?? throw new InvalidOperationException("Graphite Snap did not succeed.");

    var status = context.InsertRecording(recording);
    if (status != SKGraphiteInsertStatus.Success)
    {
        contextUsable = false;
        throw new InvalidOperationException($"Graphite InsertRecording failed: {status}.");
    }

    if (!context.Submit(new SKGraphiteSubmitInfo { Sync = true }))
    {
        contextUsable = false;
        throw new InvalidOperationException("Graphite Submit did not succeed.");
    }
}
finally
{
    surface?.Dispose();

    if (wrapped && !released && contextUsable)
    {
        contextUsable = context.Submit(new SKGraphiteSubmitInfo { Sync = true });
        if (contextUsable)
        {
            for (var i = 0; i < 100 && !released; i++)
                context.CheckAsyncWorkCompletion();
            context.FreeGpuResources();
        }
    }

    if (!wrapped || released)
        recorder.DeleteBackendTexture(backendTexture);
}

if (!released)
    throw new InvalidOperationException(
        "Skia still has work using the backend texture; tear down the failed GPU context.");
```

The `finally` block also covers failed wrapping. If insertion or submission fails, the context might no longer be safe to submit or drain. Do not delete an allocation while Skia might still reference it; tear down and recreate the owning Graphite context and native Vulkan device instead.

For a `VkImage` allocated by your own Vulkan code, construct the wrapper with `SKGraphiteBackendTexture.CreateVulkan` and release the native image through Vulkan after the callback. `SKImage.FromTexture` uses the same callback timing.

## Next steps

Continue with the shared [Graphite recording, submission, readback, texture, and resource flow](index.md).

## Related links

- [Graphite GPU surfaces](index.md)
- [Ganesh with Vulkan](../ganesh/vulkan.md)
- [Migrate from Ganesh to Graphite](migrate-from-ganesh.md)
