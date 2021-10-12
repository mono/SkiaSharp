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

Added property:

```csharp
public bool IsAbandoned { get; }
```

Added methods:

```csharp
public static GRContext CreateGl (GRContextOptions options);
public static GRContext CreateGl (GRGlInterface backendContext, GRContextOptions options);
public static GRContext CreateVulkan (GRVkBackendContext backendContext, GRContextOptions options);
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.GRGlInterface

Added method:

```csharp
public static GRGlInterface CreateAngle ();
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


#### Type Changed: SkiaSharp.SKData

Added property:

```csharp
public System.Span<byte> Span { get; }
```

Added methods:

```csharp
public static SKData Create (long size);
public static SKData CreateCopy (IntPtr bytes, int length);
public static SKData CreateCopy (IntPtr bytes, long length);
```


#### Type Changed: SkiaSharp.SKImage

Added property:

```csharp
public SKImageInfo Info { get; }
```

Added method:

```csharp
public SKImage ApplyImageFilter (GRContext context, SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset);
```


#### Type Changed: SkiaSharp.SKPicture

Added methods:

```csharp
public static SKPicture Deserialize (SKData data);
public static SKPicture Deserialize (SKStream stream);
public static SKPicture Deserialize (System.IO.Stream stream);
public static SKPicture Deserialize (System.ReadOnlySpan<byte> data);
public static SKPicture Deserialize (IntPtr data, int length);
public SKData Serialize ();
public void Serialize (SKWStream stream);
public void Serialize (System.IO.Stream stream);
```


#### Type Changed: SkiaSharp.SKSurface

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

Added method:

```csharp
public void Flush ();
```


#### New Type: SkiaSharp.GRContextOptions

```csharp
public class GRContextOptions {
	// constructors
	public GRContextOptions ();
	// properties
	public bool AllowPathMaskCaching { get; set; }
	public bool AvoidStencilBuffers { get; set; }
	public int BufferMapThreshold { get; set; }
	public bool DoManualMipmapping { get; set; }
	public int GlyphCacheTextureMaximumBytes { get; set; }
	public int RuntimeProgramCacheSize { get; set; }
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


