# API diff: SkiaSharp.Views.Tizen.dll

## SkiaSharp.Views.Tizen.dll

> Assembly Version Changed: 1.68.0.0 vs 1.60.0.0

### Namespace SkiaSharp.Views.Tizen

#### Type Changed: SkiaSharp.Views.Tizen.SKPaintGLSurfaceEventArgs

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


#### Type Changed: SkiaSharp.Views.Tizen.TizenExtensions

Added methods:

```csharp
public static SkiaSharp.SKSize ToSKSize (this ElmSharp.Size size);
public static SkiaSharp.SKSizeI ToSKSizeI (this ElmSharp.Size size);
public static ElmSharp.Size ToSize (this SkiaSharp.SKSize size);
public static ElmSharp.Size ToSize (this SkiaSharp.SKSizeI size);
```



