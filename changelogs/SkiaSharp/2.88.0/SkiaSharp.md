# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 2.88.0.0 vs 2.80.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRBackend

Added value:

```csharp
Direct3D = 4,
```


#### Type Changed: SkiaSharp.GRContext

Modified base type:

```diff
-SkiaSharp.SKObject
+SkiaSharp.GRRecordingContext
```

Added methods:

```csharp
protected override void DisposeNative ();
public void Flush (bool submit, bool synchronous);
public void Submit (bool synchronous);
```


#### Type Changed: SkiaSharp.GRVkImageInfo

Added properties:

```csharp
public uint ImageUsageFlags { get; set; }
public uint SampleCount { get; set; }
public uint SharingMode { get; set; }
```


#### Type Changed: SkiaSharp.SKBitmap

Obsoleted properties:

```diff
 [Obsolete ()]
 public bool IsVolatile { get; set; }
```


#### Type Changed: SkiaSharp.SKCanvas

Added methods:

```csharp
public void Clear (SKColorF color);
public void DrawColor (SKColorF color, SKBlendMode mode);
```


#### Type Changed: SkiaSharp.SKColorSpaceXyz

Obsoleted properties:

```diff
 [Obsolete ()]
 public static SKColorSpaceXyz Dcip3 { get; }
```

Added property:

```csharp
public static SKColorSpaceXyz DisplayP3 { get; }
```


#### Type Changed: SkiaSharp.SKColorType

Added values:

```csharp
Bgr101010x = 20,
Bgra1010102 = 19,
```


#### Type Changed: SkiaSharp.SKImage

Added methods:

```csharp
public SKImage ApplyImageFilter (GRContext context, SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset);
public SKImage ApplyImageFilter (GRRecordingContext context, SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset);
public static SKImage FromAdoptedTexture (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType);
public static SKImage FromAdoptedTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
public static SKImage FromAdoptedTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha);
public static SKImage FromAdoptedTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace);
public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType);
public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha);
public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace);
public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc);
public static SKImage FromTexture (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc, object releaseContext);
public bool IsValid (GRRecordingContext context);
```


#### Type Changed: SkiaSharp.SKImageFilter

Added methods:

```csharp
public static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
```


#### Type Changed: SkiaSharp.SKPixmap

Added method:

```csharp
public bool Erase (SKColorF color, SKColorSpace colorspace, SKRectI subset);
```


#### Type Changed: SkiaSharp.SKSurface

Added property:

```csharp
public GRRecordingContext Context { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType);
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props);
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType);
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace);
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props);
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
```

Added methods:

```csharp
public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, SKColorType colorType);
public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType);
public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info);
public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType);
public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props);
public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount);
public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace);
public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType);
public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin);
public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props);
public static SKSurface Create (GRRecordingContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace);
public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface Create (GRRecordingContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
public static SKSurface Create (GRRecordingContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips);
public void Flush (bool submit, bool synchronous);
```


#### New Type: SkiaSharp.GRRecordingContext

```csharp
public class GRRecordingContext : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public GRBackend Backend { get; }
	// methods
	public int GetMaxSurfaceSampleCount (SKColorType colorType);
}
```

#### New Type: SkiaSharp.SKRuntimeEffect

```csharp
public class SKRuntimeEffect : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public System.Collections.Generic.IReadOnlyList<string> Children { get; }
	public int UniformSize { get; }
	public System.Collections.Generic.IReadOnlyList<string> Uniforms { get; }
	// methods
	public static SKRuntimeEffect Create (string sksl, out string errors);
	public SKColorFilter ToColorFilter ();
	public SKColorFilter ToColorFilter (SKRuntimeEffectUniforms uniforms);
	public SKColorFilter ToColorFilter (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children);
	public SKShader ToShader (bool isOpaque);
	public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms);
	public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children);
	public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children, SKMatrix localMatrix);
}
```

#### New Type: SkiaSharp.SKRuntimeEffectChildren

```csharp
public class SKRuntimeEffectChildren : System.Collections.Generic.IEnumerable<string>, System.Collections.IEnumerable {
	// constructors
	public SKRuntimeEffectChildren (SKRuntimeEffect effect);
	// properties
	public int Count { get; }
	public SKShader Item { set; }
	public System.Collections.Generic.IReadOnlyList<string> Names { get; }
	// methods
	public void Add (string name, SKShader value);
	public bool Contains (string name);
	public virtual System.Collections.Generic.IEnumerator<string> GetEnumerator ();
	public void Reset ();
	public SKShader[] ToArray ();
}
```

#### New Type: SkiaSharp.SKRuntimeEffectUniform

```csharp
public struct SKRuntimeEffectUniform {
	// properties
	public static SKRuntimeEffectUniform Empty { get; }
	public bool IsEmpty { get; }
	public int Size { get; }
	// methods
	public void WriteTo (System.Span<byte> data);
	public static SKRuntimeEffectUniform op_Implicit (SKMatrix value);
	public static SKRuntimeEffectUniform op_Implicit (System.ReadOnlySpan<float> value);
	public static SKRuntimeEffectUniform op_Implicit (float value);
	public static SKRuntimeEffectUniform op_Implicit (float[] value);
	public static SKRuntimeEffectUniform op_Implicit (float[][] value);
	public static SKRuntimeEffectUniform op_Implicit (System.Span<float> value);
}
```

#### New Type: SkiaSharp.SKRuntimeEffectUniforms

```csharp
public class SKRuntimeEffectUniforms : System.Collections.Generic.IEnumerable<string>, System.Collections.IEnumerable {
	// constructors
	public SKRuntimeEffectUniforms (SKRuntimeEffect effect);
	// properties
	public int Count { get; }
	public SKRuntimeEffectUniform Item { set; }
	public System.Collections.Generic.IReadOnlyList<string> Names { get; }
	// methods
	public void Add (string name, SKRuntimeEffectUniform value);
	public bool Contains (string name);
	public virtual System.Collections.Generic.IEnumerator<string> GetEnumerator ();
	public void Reset ();
	public SKData ToData ();
}
```


### New Namespace SkiaSharp.Internals

#### New Type: SkiaSharp.Internals.IPlatformLock

```csharp
public interface IPlatformLock {
	// methods
	public virtual void EnterReadLock ();
	public virtual void EnterUpgradeableReadLock ();
	public virtual void EnterWriteLock ();
	public virtual void ExitReadLock ();
	public virtual void ExitUpgradeableReadLock ();
	public virtual void ExitWriteLock ();
}
```

#### New Type: SkiaSharp.Internals.PlatformLock

```csharp
public static class PlatformLock {
	// properties
	public static System.Func<IPlatformLock> Factory { get; set; }
	// methods
	public static IPlatformLock Create ();
	public static IPlatformLock DefaultFactory ();
}
```

