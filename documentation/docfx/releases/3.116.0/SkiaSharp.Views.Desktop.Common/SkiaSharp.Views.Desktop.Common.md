# API diff: SkiaSharp.Views.Desktop.Common.dll

## SkiaSharp.Views.Desktop.Common.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Desktop

#### Type Changed: SkiaSharp.Views.Desktop.SKPaintGLSurfaceEventArgs

Removed constructors:

```csharp
[Obsolete ("Use SKPaintGLSurfaceEventArgs(SKSurface, GRBackendRenderTarget, SKColorType, GRSurfaceOrigin) instead.")]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);

[Obsolete ("Use SKPaintGLSurfaceEventArgs(SKSurface, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType) instead.")]
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType, SkiaSharp.GRGlFramebufferInfo glInfo);
```

Removed property:

```csharp
[Obsolete ("Use BackendRenderTarget instead.")]
public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
```


#### Removed Type SkiaSharp.Views.Desktop.Extensions

