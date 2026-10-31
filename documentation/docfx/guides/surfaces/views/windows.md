---
title: "Render with SkiaSharp views on Windows"
description: "Choose SkiaSharp raster or GPU controls for WPF, Windows Forms, and WinUI 3."
---

# Render with SkiaSharp views on Windows

SkiaSharp ships separate view packages for the major Windows UI frameworks:

| UI framework | Package | Raster control | GPU control |
| --- | --- | --- | --- |
| WPF | `SkiaSharp.Views.WPF` | `SKElement` | `SKGLElement` |
| Windows Forms | `SkiaSharp.Views.WindowsForms` | `SKControl` | `SKGLControl` |
| WinUI 3 | `SkiaSharp.Views.WinUI` | `SKXamlCanvas` | `SKSwapChainPanel` |

The raster controls raise `SKPaintSurfaceEventArgs`. The GPU controls raise `SKPaintGLSurfaceEventArgs`.

## WPF

Add the WPF namespace and choose `SKElement` or `SKGLElement`:

```xml
<Window
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:skia="clr-namespace:SkiaSharp.Views.WPF;assembly=SkiaSharp.Views.WPF">
    <skia:SKElement PaintSurface="OnPaintSurface" />
</Window>
```

`SKElement` renders through a `WriteableBitmap`. `SKGLElement` creates a Ganesh OpenGL context through OpenTK. Use `InvalidateVisual()` to request another WPF frame.

## Windows Forms

Create `SKControl` for raster drawing or `SKGLControl` for Ganesh OpenGL:

```csharp
using System.Windows.Forms;
using SkiaSharp.Views.Desktop;

var skiaControl = new SKControl { Dock = DockStyle.Fill };
skiaControl.PaintSurface += OnPaintSurface;
Controls.Add(skiaControl);
```

Use the normal Windows Forms invalidation and lifetime APIs. The control owns the per-frame surface; application code owns any drawing resources it creates.

## WinUI 3

The WinUI controls use the `SkiaSharp.Views.Windows` namespace:

```xml
<Page
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:skia="using:SkiaSharp.Views.Windows">
    <skia:SKXamlCanvas PaintSurface="OnPaintSurface" />
</Page>
```

`SKXamlCanvas` presents raster pixels through a `WriteableBitmap`. `SKSwapChainPanel` uses Ganesh over ANGLE/OpenGL ES. Call the control's `Invalidate()` method for on-demand rendering; enable its render loop only for continuous animation.

## Related links

- [Choose a SkiaSharp view](index.md)
- [.NET MAUI views](maui.md)
- [Uno Platform views](uno.md)
- [Ganesh with OpenGL](../ganesh/opengl.md)
