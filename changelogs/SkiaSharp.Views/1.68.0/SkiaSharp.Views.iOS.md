# API diff: SkiaSharp.Views.iOS.dll

## SkiaSharp.Views.iOS.dll

> Assembly Version Changed: 1.68.0.0 vs 1.60.0.0

### Namespace SkiaSharp.Views.iOS

#### Type Changed: SkiaSharp.Views.iOS.SKCanvasLayer

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


#### Type Changed: SkiaSharp.Views.iOS.SKCanvasView

Obsoleted methods:

```diff
 [Obsolete ()]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.iOS.SKGLLayer

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


#### Type Changed: SkiaSharp.Views.iOS.SKGLView

Obsoleted methods:

```diff
 [Obsolete ()]
 public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.iOS.SKPaintGLSurfaceEventArgs

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



