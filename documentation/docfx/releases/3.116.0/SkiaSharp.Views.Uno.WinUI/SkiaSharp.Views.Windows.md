# API diff: SkiaSharp.Views.Windows.dll

## SkiaSharp.Views.Windows.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp.Views.Windows

#### Type Changed: SkiaSharp.Views.Windows.GlobalStaticResources

Removed method:

```csharp
[Obsolete ("This method is provided for binary backward compatibility. It will always return null.")]
public static object FindResource (string name);
```


#### Type Changed: SkiaSharp.Views.Windows.SKPaintGLSurfaceEventArgs

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


#### Removed Type SkiaSharp.Views.Windows.Extensions

