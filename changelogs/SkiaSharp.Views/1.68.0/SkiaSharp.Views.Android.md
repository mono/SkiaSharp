# API diff: SkiaSharp.Views.Android.dll

## SkiaSharp.Views.Android.dll

> Assembly Version Changed: 1.68.0.0 vs 1.60.0.0

### Namespace SkiaSharp.Views.Android

#### Type Changed: SkiaSharp.Views.Android.SKCanvasView

Obsoleted methods:

```diff
 [Obsolete ()]
 protected virtual void OnDraw (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLSurfaceView

Added event:

```csharp
public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public virtual void SetRenderer (SKGLSurfaceView.ISKRenderer renderer);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLSurfaceViewRenderer

Obsoleted methods:

```diff
 [Obsolete ()]
 protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```

Modified methods:

```diff
-protected abstract void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget)
+protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget)
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLTextureView

Added event:

```csharp
public event System.EventHandler<SKPaintGLSurfaceEventArgs> PaintSurface;
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public virtual void SetRenderer (SKGLTextureView.ISKRenderer renderer);
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLTextureViewRenderer

Obsoleted methods:

```diff
 [Obsolete ()]
 protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```

Modified methods:

```diff
-protected abstract void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget)
+protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget)
```

Added method:

```csharp
protected virtual void OnPaintSurface (SKPaintGLSurfaceEventArgs e);
```


#### Type Changed: SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs

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



