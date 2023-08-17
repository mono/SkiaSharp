# API diff: SkiaSharp.Views.tvOS.dll

## SkiaSharp.Views.tvOS.dll

> Assembly Version Changed: 1.68.0.0 vs 1.60.0.0

### Namespace SkiaSharp.Views.tvOS

#### Type Changed: SkiaSharp.Views.tvOS.SKCanvasLayer

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


#### Type Changed: SkiaSharp.Views.tvOS.SKCanvasView

Obsoleted methods:

```diff
 [Obsolete ()]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.tvOS.SKGLLayer

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


#### Type Changed: SkiaSharp.Views.tvOS.SKGLView

Obsoleted methods:

```diff
 [Obsolete ()]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.tvOS.SKPaintGLSurfaceEventArgs

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



