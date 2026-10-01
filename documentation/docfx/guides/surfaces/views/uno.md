---
title: "Render with SkiaSharp views in Uno Platform"
description: "Use SKXamlCanvas and SKSwapChainPanel in Uno Platform while accounting for target-specific GPU support."
---

# Render with SkiaSharp views in Uno Platform

The `SkiaSharp.Views.Uno.WinUI` package provides WinUI-shaped controls in the `SkiaSharp.Views.Windows` namespace:

| Control | Rendering path | Paint event |
| --- | --- | --- |
| `SKXamlCanvas` | CPU raster | `SKPaintSurfaceEventArgs` |
| `SKSwapChainPanel` | Ganesh where a GL or WebGL implementation is available | `SKPaintGLSurfaceEventArgs` |

Use `SKXamlCanvas` as the portable default. `SKSwapChainPanel` support depends on the Uno runtime, so do not select it solely because the type is present.

## Add a control

```xml
<Page
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:skia="using:SkiaSharp.Views.Windows">
    <skia:SKXamlCanvas PaintSurface="OnPaintSurface" />
</Page>
```

Call `Invalidate()` when the scene changes. `SKSwapChainPanel` also has an `EnableRenderLoop` property for continuous rendering.

## Handle runtime differences

| Target | `SKSwapChainPanel` implementation |
| --- | --- |
| Windows | The ANGLE-backed control from `SkiaSharp.Views.WinUI` |
| WebAssembly | Ganesh over WebGL |
| Android | Ganesh over OpenGL ES in an `SKGLTextureView` |
| iOS | Ganesh over OpenGL ES in an `SKGLView` |
| macOS | Ganesh over OpenGL in an `SKGLView` |
| Mac Catalyst | Unsupported |
| Uno Skia renderer | Unsupported |

The iOS implementation uses the native `SKGLView` compatibility path, which Apple obsoleted starting with iOS 12. Prefer `SKXamlCanvas` on iOS unless your application specifically needs that OpenGL ES path.

On Mac Catalyst and the Uno Skia-renderer runtime, the default `RaiseOnUnsupported` value causes construction or use of `SKSwapChainPanel` to throw `NotSupportedException`.

Setting `RaiseOnUnsupported` to `false` suppresses the exception but does not create a working GPU renderer. Choose `SKXamlCanvas` or another runtime-native drawing integration as the fallback.

Verify the GPU control on every Uno target. A control that works in the browser or on Android is not evidence that it works on another runtime.

## Related links

- [Choose a SkiaSharp view](index.md)
- [Windows views](windows.md)
- [Apple platform views](apple.md)
- [Blazor WebAssembly views](blazor.md)
- [Ganesh with OpenGL](../ganesh/opengl.md)
