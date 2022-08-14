# API diff: SkiaSharp.Views.Mac.dll

## SkiaSharp.Views.Mac.dll

> Assembly Version Changed: 1.68.0.0 vs 1.60.0.0

### Namespace SkiaSharp.Views.Mac

#### Type Changed: SkiaSharp.Views.Mac.SKCanvasLayer

Obsoleted properties:

```diff
 [Obsolete ()]
 public ISKCanvasLayerDelegate SKDelegate { get; set; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.Mac.SKCanvasView

Obsoleted methods:

```diff
 [Obsolete ()]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.Mac.SKGLLayer

Obsoleted properties:

```diff
 [Obsolete ()]
 public ISKGLLayerDelegate SKDelegate { get; set; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.Mac.SKGLView

Obsoleted methods:

```diff
 [Obsolete ()]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.Mac.SKPaintGLSurfaceEventArgs

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



