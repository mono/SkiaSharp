---
title: "Ganesh with Direct3D"
description: "Create a Ganesh Direct3D context from D3D12 objects and describe existing Direct3D resources for SkiaSharp GPU surfaces."
---

# Use Ganesh with Direct3D

Use the Direct3D backend on Windows when your host owns a DXGI adapter, D3D12 device, and command queue. Graphite has no direct Direct3D backend, so Ganesh is the SkiaSharp path when D3D12 is a requirement.

## Create the Ganesh context

Build a [`GRD3DBackendContext`](xref:SkiaSharp.GRD3DBackendContext) from the native handles:

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

## Describe Direct3D resources

Use `GRD3DTextureResourceInfo` when constructing a [`GRBackendRenderTarget`](xref:SkiaSharp.GRBackendRenderTarget) or [`GRBackendTexture`](xref:SkiaSharp.GRBackendTexture) for an existing D3D12 resource. Your host remains responsible for allocation, resource-state transitions, synchronization, presentation, and final release.

After constructing the backend target or texture, return to [wrapping an existing Ganesh resource](index.md#wrapping-an-existing-render-target) for the shared `SKSurface.Create` calls.

## Related links

- [Ganesh GPU surfaces](index.md)
- [Rendering in SkiaSharp Views](../views/index.md)
