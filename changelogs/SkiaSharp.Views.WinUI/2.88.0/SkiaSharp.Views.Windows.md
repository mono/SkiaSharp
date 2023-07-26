# API diff: SkiaSharp.Views.Windows.dll

## SkiaSharp.Views.Windows.dll

> Assembly Version Changed: 2.88.0.0 vs 2.80.0.0

### Namespace SkiaSharp.Views.Windows

#### Type Changed: SkiaSharp.Views.Windows.SKPaintGLSurfaceEventArgs

Obsoleted constructors:

```diff
 [Obsolete ()]
 public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType, SkiaSharp.GRGlFramebufferInfo glInfo);
```

Added constructors:

```csharp
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info);
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
```

Added properties:

```csharp
public SkiaSharp.SKImageInfo Info { get; }
public SkiaSharp.SKImageInfo RawInfo { get; }
```


#### Type Changed: SkiaSharp.Views.Windows.SKPaintSurfaceEventArgs

Added constructor:

```csharp
public SKPaintSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
```

Added property:

```csharp
public SkiaSharp.SKImageInfo RawInfo { get; }
```



