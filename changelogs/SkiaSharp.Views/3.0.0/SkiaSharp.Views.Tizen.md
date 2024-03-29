# API diff: SkiaSharp.Views.Tizen.dll

## SkiaSharp.Views.Tizen.dll

> Assembly Version Changed: 3.0.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Tizen

#### Type Changed: SkiaSharp.Views.Tizen.SKPaintGLSurfaceEventArgs

Removed constructors:

```csharp
[Obsolete]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);

[Obsolete]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType, SkiaSharp.GRGlFramebufferInfo glInfo);
```

Removed property:

```csharp
[Obsolete]
public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
```


#### Type Changed: SkiaSharp.Views.Tizen.TizenExtensions

Added methods:

```csharp
public static SkiaSharp.SKColorF ToSKColorF (this Tizen.NUI.Color color);
public static SkiaSharp.SKPoint ToSKPoint (this Tizen.NUI.Position point);
public static SkiaSharp.SKPointI ToSKPointI (this Tizen.NUI.Position2D point);
public static SkiaSharp.SKRect ToSKRect (this Tizen.NUI.Rectangle rect);
public static SkiaSharp.SKRectI ToSKRectI (this Tizen.NUI.Rectangle rect);
public static SkiaSharp.SKSize ToSKSize (this Tizen.NUI.Size size);
public static SkiaSharp.SKSizeI ToSKSizeI (this Tizen.NUI.Size2D size);
```


#### Removed Type SkiaSharp.Views.Tizen.Extensions

### Namespace SkiaSharp.Views.Tizen.NUI

#### Type Changed: SkiaSharp.Views.Tizen.NUI.CustomRenderingView

Added property:

```csharp
public SkiaSharp.SKSize CanvasSize { get; }
```



