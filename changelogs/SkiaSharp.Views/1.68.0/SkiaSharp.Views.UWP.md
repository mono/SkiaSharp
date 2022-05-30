# API diff: SkiaSharp.Views.UWP.dll

## SkiaSharp.Views.UWP.dll

> Assembly Version Changed: 1.68.0.0 vs 1.60.0.0

### Namespace SkiaSharp.Views.UWP

#### Type Changed: SkiaSharp.Views.UWP.AngleSwapChainPanel

Removed properties:

```csharp
public GlesBackingOption BackingOption { get; set; }
public GlesContext Context { get; set; }
public GlesDepthFormat DepthFormat { get; set; }
public GlesMultisampling Multisampling { get; set; }
public GlesStencilFormat StencilFormat { get; set; }
```

Modified properties:

```diff
 public double ContentsScale { get; ---set;--- }
```


#### Type Changed: SkiaSharp.Views.UWP.SKPaintGLSurfaceEventArgs

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


#### Type Changed: SkiaSharp.Views.UWP.SKSwapChainPanel

Removed method:

```csharp
protected override Windows.Foundation.Size ArrangeOverride (Windows.Foundation.Size finalSize);
```


#### Removed Type SkiaSharp.Views.UWP.GlesBackingOption
#### Removed Type SkiaSharp.Views.UWP.GlesContext
#### Removed Type SkiaSharp.Views.UWP.GlesDepthFormat
#### Removed Type SkiaSharp.Views.UWP.GlesMultisampling
#### Removed Type SkiaSharp.Views.UWP.GlesRenderTarget
#### Removed Type SkiaSharp.Views.UWP.GlesStencilFormat

