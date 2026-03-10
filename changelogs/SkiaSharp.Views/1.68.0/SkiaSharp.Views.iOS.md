# API diff: SkiaSharp.Views.iOS.dll

## SkiaSharp.Views.iOS.dll

> Assembly Version Changed: 1.68.0.0 vs 1.60.0.0

### Namespace SkiaSharp.Views.iOS

#### Type Changed: SkiaSharp.Views.iOS.SKCanvasLayer

Obsoleted properties:

```diff
 [Obsolete ("Use PaintSurface instead.")]
 public ISKCanvasLayerDelegate SKDelegate { get; set; }
```

Obsoleted methods:

```diff
 [Obsolete ("Use OnPaintSurface(SKPaintSurfaceEventArgs) instead.")]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.iOS.SKCanvasView

Obsoleted methods:

```diff
 [Obsolete ("Use OnPaintSurface(SKPaintSurfaceEventArgs) instead.")]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.iOS.SKGLLayer

Obsoleted properties:

```diff
 [Obsolete ("Use PaintSurface instead.")]
 public ISKGLLayerDelegate SKDelegate { get; set; }
```

Obsoleted methods:

```diff
 [Obsolete ("Use OnPaintSurface(SKPaintGLSurfaceEventArgs) instead.")]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.iOS.SKGLView

Obsoleted methods:

```diff
 [Obsolete ("Use OnPaintSurface(SKPaintGLSurfaceEventArgs) instead.")]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.iOS.SKPaintGLSurfaceEventArgs

Obsoleted constructors:

```diff
 [Obsolete ("Use SKPaintGLSurfaceEventArgs(SKSurface, GRBackendRenderTarget, SKColorType, GRSurfaceOrigin) instead.")]
 public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```

Added constructors:

```csharp
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget);
public SKPaintGLSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKColorType colorType);
```

Obsoleted properties:

```diff
 [Obsolete ("Use BackendRenderTarget instead.")]
 public SkiaSharp.GRBackendRenderTargetDesc RenderTarget { get; }
```

Added properties:

```csharp
public SkiaSharp.GRBackendRenderTarget BackendRenderTarget { get; }
public SkiaSharp.SKColorType ColorType { get; }
public SkiaSharp.GRSurfaceOrigin Origin { get; }
```



