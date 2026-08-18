# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

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
```


#### Type Changed: SkiaSharp.GRGlInterface

Added method:

```csharp
public static GRGlInterface CreateAngle ();
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


