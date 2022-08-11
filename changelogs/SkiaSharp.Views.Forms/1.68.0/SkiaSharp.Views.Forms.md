# API diff: SkiaSharp.Views.Forms.dll

## SkiaSharp.Views.Forms.dll

> Assembly Version Changed: 1.68.0.0 vs 1.60.0.0

### Namespace SkiaSharp.Views.Forms

#### Type Changed: SkiaSharp.Views.Forms.SKPaintGLSurfaceEventArgs

Obsoleted constructors:

```diff
 [Obsolete ()]
 public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```

Added constructors:

```csharp
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget);
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType);
```

Obsoleted properties:

```diff
 [Obsolete ()]
 public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
```

Added properties:

```csharp
public SkiaSharp.GRBackendRenderTarget BackendRenderTarget { get; }
public SkiaSharp.SKColorType ColorType { get; }
public SkiaSharp.GRSurfaceOrigin Origin { get; }
```



