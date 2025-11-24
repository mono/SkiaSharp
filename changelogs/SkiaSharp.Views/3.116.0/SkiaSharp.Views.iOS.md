# API diff: SkiaSharp.Views.iOS.dll

## SkiaSharp.Views.iOS.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.iOS

#### Type Changed: SkiaSharp.Views.iOS.SKCanvasLayer

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


#### Type Changed: SkiaSharp.Views.iOS.SKCanvasView

Removed method:

```csharp
[Obsolete]
public virtual void DrawInSurface (SkiaSharp.SKSurface surface, SkiaSharp.SKImageInfo info);
```


#### Type Changed: SkiaSharp.Views.iOS.SKPaintMetalSurfaceEventArgs

Added constructors:

```csharp
public SKPaintMetalSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info);
public SKPaintMetalSurfaceEventArgs (SkiaSharp.SKSurface surface, SkiaSharp.GRBackendRenderTarget renderTarget, SkiaSharp.GRSurfaceOrigin origin, SkiaSharp.SKImageInfo info, SkiaSharp.SKImageInfo rawInfo);
```

Added properties:

```csharp
public SkiaSharp.SKImageInfo Info { get; }
public SkiaSharp.SKImageInfo RawInfo { get; }
```


#### Removed Type SkiaSharp.Views.iOS.Extensions
#### Removed Type SkiaSharp.Views.iOS.SKPaintGLSurfaceEventArgs

