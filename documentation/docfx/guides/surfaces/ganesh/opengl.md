---
title: "Ganesh with OpenGL"
description: "Create a Ganesh context from a current OpenGL context and wrap an existing framebuffer as a SkiaSharp GPU surface."
---

# Use Ganesh with OpenGL

Use OpenGL when your host already owns a WGL, GLX, EGL, CGL, or OpenGL ES context. SkiaSharp does not create that platform context for you, and it must be current on the calling thread whenever Ganesh makes GL calls.

## Create the Ganesh context

Once the platform GL context is current, create a [`GRGlInterface`](xref:SkiaSharp.GRGlInterface) to resolve its entry points and pass it to [`GRContext.CreateGl`](xref:SkiaSharp.GRContext.CreateGl*):

```csharp
// A platform GL context is already current on this thread.
using var glInterface = GRGlInterface.Create()
    ?? throw new InvalidOperationException("Unable to create the OpenGL interface.");
using var context = GRContext.CreateGl(glInterface)
    ?? throw new InvalidOperationException("Unable to create the Ganesh OpenGL context.");
```

`GRContext.CreateGl()` also has a parameterless overload that assembles the interface from the current context.

## Wrap the current framebuffer

To draw into a window framebuffer, query its identifier, stencil bits, and sample count from OpenGL, then describe it with [`GRGlFramebufferInfo`](xref:SkiaSharp.GRGlFramebufferInfo):

```csharp
var glInfo = new GRGlFramebufferInfo((uint)framebuffer, colorType.ToGlSizedFormat());
using var renderTarget = new GRBackendRenderTarget(
    width, height, sampleCount, stencilBits, glInfo);

using var surface = SKSurface.Create(
    context, renderTarget, GRSurfaceOrigin.BottomLeft, colorType)
    ?? throw new InvalidOperationException("Unable to wrap the OpenGL framebuffer.");
```

Draw and flush the surface as described in the [shared Ganesh surface flow](index.md#rendering-offscreen), then present or swap buffers with your windowing API.

## Keep OpenGL current during cleanup

A `GRContext` and its resources are not thread-safe. Keep the same platform GL context current while disposing surfaces, backend targets, and the `GRContext`; cleanup can make GL calls.

## Related links

- [Ganesh GPU surfaces](index.md)
- [Rendering in SkiaSharp Views](../../views/index.md)
- [Skia canvas creation, GPU backend (skia.org)](https://skia.org/docs/user/api/skcanvas_creation/)
