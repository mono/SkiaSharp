# API diff: SkiaSharp.Views.Android.dll

## SkiaSharp.Views.Android.dll

> Assembly Version Changed: 3.0.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Android

#### Type Changed: SkiaSharp.Views.Android.SKCanvasView

Removed method:

```csharp
[Obsolete]
protected virtual void OnDraw (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLSurfaceView

Removed method:

```csharp
[Obsolete]
public virtual void SetRenderer (SKGLSurfaceView.ISKRenderer renderer);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLSurfaceViewRenderer

Removed method:

```csharp
[Obsolete]
protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLTextureView

Removed method:

```csharp
[Obsolete]
public virtual void SetRenderer (SKGLTextureView.ISKRenderer renderer);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLTextureViewRenderer

Removed method:

```csharp
[Obsolete]
protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```


#### Type Changed: SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs

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


#### Removed Type SkiaSharp.Views.Android.Extensions

