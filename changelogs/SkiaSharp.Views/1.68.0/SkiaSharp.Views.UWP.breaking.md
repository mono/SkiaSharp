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


#### Removed Type SkiaSharp.Views.UWP.GlesBackingOption
#### Removed Type SkiaSharp.Views.UWP.GlesContext
#### Removed Type SkiaSharp.Views.UWP.GlesDepthFormat
#### Removed Type SkiaSharp.Views.UWP.GlesMultisampling
#### Removed Type SkiaSharp.Views.UWP.GlesRenderTarget
#### Removed Type SkiaSharp.Views.UWP.GlesStencilFormat

