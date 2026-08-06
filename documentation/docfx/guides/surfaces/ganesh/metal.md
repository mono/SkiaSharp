---
title: "Ganesh with Metal"
description: "Create a Ganesh Metal context from an Apple Metal device and command queue, then wrap Metal textures as SkiaSharp surfaces."
---

# Use Ganesh with Metal

Use Metal for GPU rendering on Apple platforms. Build a [`GRMtlBackendContext`](xref:SkiaSharp.GRMtlBackendContext) from an `MTLDevice` and an `MTLCommandQueue`.

## Create the Ganesh context

On Apple target frameworks you can assign typed `IMTLDevice` and `IMTLCommandQueue` objects. From other targets, assign their native handles:

```csharp
using var backendContext = new GRMtlBackendContext
{
    DeviceHandle = mtlDeviceHandle,
    QueueHandle = mtlCommandQueueHandle,
};

using var context = GRContext.CreateMetal(backendContext);
```

## Describe Metal textures

Use `GRMtlTextureInfo` when constructing a [`GRBackendRenderTarget`](xref:SkiaSharp.GRBackendRenderTarget) or [`GRBackendTexture`](xref:SkiaSharp.GRBackendTexture) for an existing `MTLTexture`. Your Metal host remains responsible for allocating and eventually releasing that texture.

After constructing the backend target or texture, return to [wrapping an existing Ganesh resource](index.md#wrapping-an-existing-render-target) for the shared `SKSurface.Create` calls.

## Related links

- [Ganesh GPU surfaces](index.md)
- [Graphite with Metal](../graphite/metal.md)
- [Choose a SkiaSharp view](../views/index.md)
