# API diff: SkiaSharp.Views.Android.dll

## SkiaSharp.Views.Android.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Android

#### Type Changed: SkiaSharp.Views.Android.Resource

Modified base type:

```diff
-System.Object
+_Microsoft.Android.Resource.Designer.Resource
```

#### Removed Type SkiaSharp.Views.Android.Resource.Attribute
#### Removed Type SkiaSharp.Views.Android.Resource.Styleable

#### Type Changed: SkiaSharp.Views.Android.SKCanvasView

Removed method:

```csharp
[Obsolete ("Use OnPaintSurface(SKPaintSurfaceEventArgs) instead.")]
protected virtual void OnDraw (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLSurfaceView

Removed method:

```csharp
[Obsolete ("Use PaintSurface instead.")]
public virtual void SetRenderer (SKGLSurfaceView.ISKRenderer renderer);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLSurfaceViewRenderer

Removed method:

```csharp
[Obsolete ("Use OnPaintSurface(SKPaintGLSurfaceEventArgs) instead.")]
protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLTextureView

Removed method:

```csharp
[Obsolete ("Use PaintSurface instead.")]
public virtual void SetRenderer (SKGLTextureView.ISKRenderer renderer);
```


#### Type Changed: SkiaSharp.Views.Android.SKGLTextureViewRenderer

Removed method:

```csharp
[Obsolete ("Use OnPaintSurface(SKPaintGLSurfaceEventArgs) instead.")]
protected virtual void OnDrawFrame (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```


#### Type Changed: SkiaSharp.Views.Android.SKPaintGLSurfaceEventArgs

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


#### Removed Type SkiaSharp.Views.Android.Extensions

