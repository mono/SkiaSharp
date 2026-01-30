# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 3.119.0.0 vs 3.116.0.0

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
public GRBackendRenderTarget (int width, int height, GRD3DTextureResourceInfo d3dTextureInfo);
public GRBackendRenderTarget (int width, int height, GRMtlTextureInfo mtlInfo);
public GRBackendRenderTarget (int width, int height, GRVkImageInfo vkImageInfo);
```


#### Type Changed: SkiaSharp.GRBackendTexture

Added constructor:

```csharp
public GRBackendTexture (int width, int height, GRD3DTextureResourceInfo d3dTextureInfo);
```


#### Type Changed: SkiaSharp.GRContext

Added methods:

```csharp
public static GRContext CreateDirect3D (GRD3DBackendContext backendContext);
public static GRContext CreateDirect3D (GRD3DBackendContext backendContext, GRContextOptions options);
```


#### Type Changed: SkiaSharp.SKCanvas

Added properties:

```csharp
public GRRecordingContext Context { get; }
public SKSurface Surface { get; }
```

Added method:

```csharp
public int SaveLayer (ref SKCanvasSaveLayerRec rec);
```


#### Type Changed: SkiaSharp.SKColorFilter

Added methods:

```csharp
public static SKColorFilter CreateHslaColorMatrix (System.ReadOnlySpan<float> matrix);
public static SKColorFilter CreateLerp (float weight, SKColorFilter filter0, SKColorFilter filter1);
public static SKColorFilter CreateLinearToSrgbGamma ();
public static SKColorFilter CreateSrgbToLinearGamma ();
```


#### Type Changed: SkiaSharp.SKColorType

Added value:

```csharp
Rgba10x6 = 24,
```


#### New Type: SkiaSharp.GRD3DBackendContext

```csharp
public class GRD3DBackendContext : System.IDisposable {
	// constructors
	public GRD3DBackendContext ();
	// properties
	public IntPtr Adapter { get; set; }
	public IntPtr Device { get; set; }
	public bool ProtectedContext { get; set; }
	public IntPtr Queue { get; set; }
	// methods
	public virtual void Dispose ();
	protected virtual void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.GRD3DTextureResourceInfo

```csharp
public class GRD3DTextureResourceInfo : System.IDisposable {
	// constructors
	public GRD3DTextureResourceInfo ();
	// properties
	public uint Format { get; set; }
	public uint LevelCount { get; set; }
	public bool Protected { get; set; }
	public IntPtr Resource { get; set; }
	public uint ResourceState { get; set; }
	public uint SampleCount { get; set; }
	public uint SampleQualityPattern { get; set; }
	// methods
	public virtual void Dispose ();
	protected virtual void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKCanvasSaveLayerRec

```csharp
public struct SKCanvasSaveLayerRec {
	// properties
	public SKImageFilter Backdrop { get; set; }
	public SKRect? Bounds { get; set; }
	public SKCanvasSaveLayerRecFlags Flags { get; set; }
	public SKPaint Paint { get; set; }
}
```

#### New Type: SkiaSharp.SKCanvasSaveLayerRecFlags

```csharp
[Serializable]
public enum SKCanvasSaveLayerRecFlags {
	F16ColorType = 16,
	InitializeWithPrevious = 4,
	None = 0,
	PreserveLcdText = 2,
}
```


