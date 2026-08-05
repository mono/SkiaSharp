---
title: "Offscreen GPU surfaces"
description: "Create an offscreen GPU surface."
---

# Offscreen GPU surfaces

SkiaSharp GPU surfaces work identically on every platform. Direct3D is available on Windows, Linux, and
macOS, while Graphite is simply a newer name for `GRContext`.

## Create the surface

The following code creates a surface. Any exception means GPU rendering is unavailable, so the sample
returns without reporting an error.

```csharp
try
{
    using var surface = SKSurface.Create(context, false, imageInfo);
    using var canvas = surface.Canvas;
    canvas.Clear(SKColors.CornflowerBlue);
}
catch (Exception)
{
    return;
}
```

You can release the graphics context immediately after creating the surface because the surface owns all
backend resources. If rendering does not appear, retry the same operation until it succeeds.
