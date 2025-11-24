# API diff: SkiaSharp.Views.tvOS.dll

## SkiaSharp.Views.tvOS.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.tvOS

#### Type Changed: SkiaSharp.Views.tvOS.SKCanvasLayer

Removed property:

```csharp
[Obsolete]
public ISKCanvasLayerDelegate SKDelegate { get; set; }
```

Removed method:

```csharp
[Obsolete]
public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```


#### Type Changed: SkiaSharp.Views.tvOS.SKCanvasView

Removed method:

```csharp
[Obsolete]
public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```


#### Type Changed: SkiaSharp.Views.tvOS.SKGLLayer

Removed property:

```csharp
[Obsolete]
public ISKGLLayerDelegate SKDelegate { get; set; }
```

Removed method:

```csharp
[Obsolete]
public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```


#### Type Changed: SkiaSharp.Views.tvOS.SKGLView

Removed method:

```csharp
[Obsolete]
public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```


#### Type Changed: SkiaSharp.Views.tvOS.SKPaintGLSurfaceEventArgs

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


#### Removed Type SkiaSharp.Views.tvOS.Extensions

