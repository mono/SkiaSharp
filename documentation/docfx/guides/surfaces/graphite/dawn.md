---
title: "Graphite with Dawn"
description: "Create a Graphite Dawn context for WebAssembly, submit without blocking, and drive WebGPU completion from the browser loop."
---

# Use Graphite with Dawn

Graphite Dawn is the WebGPU backend used by SkiaSharp in browser/WebAssembly hosts. Before creating a context, confirm the backend is compiled into the current native library:

```csharp
if (!SKGraphiteContext.IsBackendAvailable(SKGraphiteBackend.Dawn))
    throw new PlatformNotSupportedException("Graphite Dawn is unavailable.");
```

## Create the Graphite context

Supply the WebGPU instance, device, and queue handles:

```csharp
using var backendContext = new SKGraphiteDawnBackendContext
{
    WgpuInstance = instanceHandle,
    WgpuDevice = deviceHandle,
    WgpuQueue = queueHandle,
};

using var context = SKGraphiteContext.CreateDawn(backendContext)
    ?? throw new InvalidOperationException("Unable to create the Graphite Dawn context.");
```

With the emdawnwebgpu port, create a real `WGPUInstance` through `wgpuCreateInstance`. Register the device and queue under that instance as their event-source parent. A placeholder or mismatched instance can cause `SKGraphiteContext.CreateDawn` to wait indefinitely.

## Submit from the browser loop

The browser event loop cannot be pumped from inside a managed call, so synchronous submission is not allowed. Calling `Submit` with `Sync = true` throws `InvalidOperationException`.

Submit without blocking, then drive completion from the host render or event loop:

```csharp
if (context.InsertRecording(recording) != SKGraphiteInsertStatus.Success)
    throw new InvalidOperationException("Graphite InsertRecording did not succeed.");
if (!context.Submit(new SKGraphiteSubmitInfo { Sync = false }))
    throw new InvalidOperationException("Graphite Submit did not succeed.");

// Later, from the host loop:
context.CheckAsyncWorkCompletion();
```

The same rule applies to readback: request the pixels, submit asynchronously, and keep pumping `CheckAsyncWorkCompletion` until the callback runs.

## Wrap Dawn textures

Use `SKGraphiteBackendTexture.CreateDawn(wgpuTexture)` to describe an existing WebGPU texture, then pass it to the shared `SKSurface.Create(recorder, backendTexture, colorType)` overload. The native WebGPU texture remains owned by the code that created it; release it only after any supplied Graphite release callback fires.

## Next steps

Continue with the shared [Graphite recording, submission, readback, texture, and resource flow](index.md).

## Related links

- [Graphite GPU surfaces](index.md)
- [Migrate from Ganesh to Graphite](migrate-from-ganesh.md)
