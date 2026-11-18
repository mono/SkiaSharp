---
title: "Render with SkiaSharp views on Linux"
description: "Use the GTK 3 or GTK 4 SKDrawingArea control for raster SkiaSharp rendering on Linux."
---

# Render with SkiaSharp views on Linux

SkiaSharp provides one raster control for each supported GTK generation:

| GTK version | Package | Control | Paint event |
| --- | --- | --- | --- |
| GTK 3 | `SkiaSharp.Views.Gtk3` | `SkiaSharp.Views.Gtk.SKDrawingArea` | `SKPaintSurfaceEventArgs` |
| GTK 4 | `SkiaSharp.Views.Gtk4` | `SkiaSharp.Views.Gtk.SKDrawingArea` | `SKPaintSurfaceEventArgs` |

Both controls create a memory-backed `SKSurface`, invoke `PaintSurface`, and present the result through Cairo. The packages do not include a GTK GPU view.

## Add the drawing area

Create the control, subscribe to `PaintSurface`, and add it with the normal GTK container API:

```csharp
using SkiaSharp.Views.Gtk;

var skiaView = new SKDrawingArea();
skiaView.PaintSurface += OnPaintSurface;
```

Draw only during the event. The control owns the surface and its Cairo backing storage. Use GTK's `QueueDraw()` mechanism when application state changes.

If a Linux application needs a GPU-backed view, it must host an appropriate graphics context and wrap its render target manually. The packaged GTK controls do not provide that integration.

## Related links

- [Choose a SkiaSharp view](index.md)
- [Raster surfaces](../raster/index.md)
- [Ganesh GPU surfaces](../ganesh/index.md)
