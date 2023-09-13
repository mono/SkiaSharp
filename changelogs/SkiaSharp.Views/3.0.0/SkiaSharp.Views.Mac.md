# API diff: SkiaSharp.Views.Mac.dll

## SkiaSharp.Views.Mac.dll

> Assembly Version Changed: 3.0.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Mac

#### Type Changed: SkiaSharp.Views.Mac.SKCanvasLayer

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


#### Type Changed: SkiaSharp.Views.Mac.SKCanvasView

Removed method:

```csharp
[Obsolete]
public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```


#### Type Changed: SkiaSharp.Views.Mac.SKGLLayer

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


#### Type Changed: SkiaSharp.Views.Mac.SKGLView

Removed method:

```csharp
[Obsolete]
public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTargetDesc renderTarget);
```


#### Type Changed: SkiaSharp.Views.Mac.SKPaintGLSurfaceEventArgs

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


#### Removed Type SkiaSharp.Views.Mac.Extensions

