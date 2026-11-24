# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 3.118.0.0 vs 3.116.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRBackend

Added value:

```csharp
Unsupported = 5,
```


#### Type Changed: SkiaSharp.GRBackendRenderTarget

Obsoleted constructors:

```diff
 [Obsolete ("Use GRBackendRenderTarget(int width, int height, GRVkImageInfo vkImageInfo) instead.")]
 public GRBackendRenderTarget (int width, int height, int sampleCount, GRVkImageInfo vkImageInfo);
```

Added constructors:

```csharp
public GRBackendRenderTarget (int width, int height, GRMtlTextureInfo mtlInfo);
public GRBackendRenderTarget (int width, int height, GRVkImageInfo vkImageInfo);
```


#### Type Changed: SkiaSharp.SKCanvas

Added properties:

```csharp
public GRRecordingContext Context { get; }
public SKSurface Surface { get; }
```



