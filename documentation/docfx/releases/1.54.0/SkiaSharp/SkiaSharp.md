# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.54.0.0 vs 1.53.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKCanvas

Added methods:

```csharp
public void DrawBitmapLattice (SKBitmap bitmap, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint);
public void DrawBitmapNinePatch (SKBitmap bitmap, SKRectI center, SKRect dst, SKPaint paint);
public void DrawImageLattice (SKImage image, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint);
public void DrawImageNinePatch (SKImage image, SKRectI center, SKRect dst, SKPaint paint);
public void Flush ();
```


#### Type Changed: SkiaSharp.SKCodecOptions

Removed fields:

```csharp
public bool HasSubset;
public SKRectI Subset;
public SKZeroInitialized ZeroInitialized;
```

Added properties:

```csharp
public bool HasSubset { get; set; }
public SKRectI Subset { get; set; }
public SKZeroInitialized ZeroInitialized { get; set; }
```


#### Type Changed: SkiaSharp.SKColor

Added field:

```csharp
public static SKColor Empty;
```

Added property:

```csharp
public float Hue { get; }
```

Added methods:

```csharp
public static SKColor FromHsl (float h, float s, float l, byte a);
public static SKColor FromHsv (float h, float s, float v, byte a);
public void ToHsl (out float h, out float s, out float l);
public void ToHsv (out float h, out float s, out float v);
public SKColor WithBlue (byte blue);
public SKColor WithGreen (byte green);
public SKColor WithRed (byte red);
public static bool op_Equality (SKColor left, SKColor right);
public static bool op_Inequality (SKColor left, SKColor right);
```


#### Type Changed: SkiaSharp.SKColorFilter

Added method:

```csharp
public static SKColorFilter CreateGamma (float gamma);
```


#### Type Changed: SkiaSharp.SKFontStyleWeight

Added values:

```csharp
ExtraBlack = 1000,
Invisible = 0,
```


#### Type Changed: SkiaSharp.SKImageInfo

Removed fields:

```csharp
public SKAlphaType AlphaType;
public SKColorType ColorType;
public int Height;
public int Width;
```

Modified properties:

```diff
-public SKPointI Size { get; }
+public SKSizeI Size { get; }
```

Added properties:

```csharp
public SKAlphaType AlphaType { get; set; }
public SKColorType ColorType { get; set; }
public int Height { get; set; }
public int Width { get; set; }
```


#### Type Changed: SkiaSharp.SKPath

Added property:

```csharp
public SKPoint LastPoint { get; }
```

Added methods:

```csharp
public bool Contains (float x, float y);
public void Offset (float dx, float dy);
```


#### Type Changed: SkiaSharp.SKPoint

Added methods:

```csharp
public static SKPoint Add (SKPoint pt, SKPoint sz);
public static SKPoint Add (SKPoint pt, SKPointI sz);
public static SKPoint Subtract (SKPoint pt, SKPoint sz);
public static SKPoint Subtract (SKPoint pt, SKPointI sz);
public static SKPoint op_Addition (SKPoint pt, SKPoint sz);
public static SKPoint op_Addition (SKPoint pt, SKPointI sz);
public static SKPoint op_Subtraction (SKPoint pt, SKPoint sz);
public static SKPoint op_Subtraction (SKPoint pt, SKPointI sz);
```


#### Type Changed: SkiaSharp.SKPoint3

Added methods:

```csharp
public static SKPoint3 Add (SKPoint3 pt, SKPoint3 sz);
public static SKPoint3 Subtract (SKPoint3 pt, SKPoint3 sz);
public static SKPoint3 op_Addition (SKPoint3 pt, SKPoint3 sz);
public static SKPoint3 op_Subtraction (SKPoint3 pt, SKPoint3 sz);
```


#### Type Changed: SkiaSharp.SKPointI

Added methods:

```csharp
public static SKPointI Add (SKPointI pt, SKPointI sz);
public static SKPointI Subtract (SKPointI pt, SKPointI sz);
public static SKPointI op_Addition (SKPointI pt, SKPointI sz);
public static SKPointI op_Subtraction (SKPointI pt, SKPointI sz);
```


#### Type Changed: SkiaSharp.SKRect

Removed fields:

```csharp
public float Bottom;
public float Left;
public float Right;
public float Top;
```

Added field:

```csharp
public static SKRect Empty;
```

Added properties:

```csharp
public float Bottom { get; set; }
public float Height { get; }
public bool IsEmpty { get; }
public float Left { get; set; }
public SKPoint Location { get; set; }
public float Right { get; set; }
public SKSize Size { get; set; }
public float Top { get; set; }
public float Width { get; }
```

Added methods:

```csharp
public bool Contains (SKPoint pt);
public bool Contains (SKRect rect);
public bool Contains (float x, float y);
public static SKRect Create (SKSize size);
public static SKRect Create (SKPoint location, SKSize size);
public override bool Equals (object obj);
public override int GetHashCode ();
public void Inflate (SKSize size);
public void Inflate (float x, float y);
public static SKRect Inflate (SKRect rect, float x, float y);
public void Intersect (SKRect rect);
public static SKRect Intersect (SKRect a, SKRect b);
public bool IntersectsWith (SKRect rect);
public bool IntersectsWithInclusive (SKRect rect);
public void Offset (SKPoint pos);
public void Offset (float x, float y);
public override string ToString ();
public void Union (SKRect rect);
public static SKRect Union (SKRect a, SKRect b);
public static bool op_Equality (SKRect left, SKRect right);
public static SKRect op_Implicit (SKRectI r);
public static bool op_Inequality (SKRect left, SKRect right);
```


#### Type Changed: SkiaSharp.SKRectI

Removed fields:

```csharp
public int Bottom;
public int Left;
public int Right;
public int Top;
```

Added properties:

```csharp
public int Bottom { get; set; }
public int Height { get; }
public bool IsEmpty { get; }
public int Left { get; set; }
public SKPointI Location { get; set; }
public int Right { get; set; }
public SKSizeI Size { get; set; }
public int Top { get; set; }
public int Width { get; }
```

Added methods:

```csharp
public static SKRectI Ceiling (SKRect value);
public bool Contains (SKPointI pt);
public bool Contains (SKRectI rect);
public bool Contains (int x, int y);
public static SKRectI Create (SKSizeI size);
public static SKRectI Create (SKPointI location, SKSizeI size);
public override bool Equals (object obj);
public override int GetHashCode ();
public void Inflate (SKSizeI size);
public void Inflate (int width, int height);
public static SKRectI Inflate (SKRectI rect, int x, int y);
public void Intersect (SKRectI rect);
public static SKRectI Intersect (SKRectI a, SKRectI b);
public bool IntersectsWith (SKRectI rect);
public void Offset (SKPointI pos);
public void Offset (int x, int y);
public static SKRectI Round (SKRect value);
public override string ToString ();
public static SKRectI Truncate (SKRect value);
public void Union (SKRectI rect);
public static SKRectI Union (SKRectI a, SKRectI b);
public static bool op_Equality (SKRectI left, SKRectI right);
public static bool op_Inequality (SKRectI left, SKRectI right);
```


#### Type Changed: SkiaSharp.SKSize

Added constructor:

```csharp
public SKSize (SKPoint pt);
```

Removed fields:

```csharp
public float Height;
public float Width;
```

Added field:

```csharp
public static SKSize Empty;
```

Added properties:

```csharp
public float Height { get; set; }
public bool IsEmpty { get; }
public float Width { get; set; }
```

Added methods:

```csharp
public static SKSize Add (SKSize sz1, SKSize sz2);
public override bool Equals (object obj);
public override int GetHashCode ();
public static SKSize Subtract (SKSize sz1, SKSize sz2);
public SKPoint ToPoint ();
public SKSizeI ToSizeI ();
public override string ToString ();
public static SKSize op_Addition (SKSize sz1, SKSize sz2);
public static bool op_Equality (SKSize sz1, SKSize sz2);
public static SKPoint op_Explicit (SKSize size);
public static SKSize op_Implicit (SKSizeI size);
public static bool op_Inequality (SKSize sz1, SKSize sz2);
public static SKSize op_Subtraction (SKSize sz1, SKSize sz2);
```


#### Type Changed: SkiaSharp.SKSizeI

Added constructor:

```csharp
public SKSizeI (SKPointI pt);
```

Removed fields:

```csharp
public int Height;
public int Width;
```

Added field:

```csharp
public static SKSizeI Empty;
```

Added properties:

```csharp
public int Height { get; set; }
public bool IsEmpty { get; }
public int Width { get; set; }
```

Added methods:

```csharp
public static SKSizeI Add (SKSizeI sz1, SKSizeI sz2);
public override bool Equals (object obj);
public override int GetHashCode ();
public static SKSizeI Subtract (SKSizeI sz1, SKSizeI sz2);
public SKPointI ToPointI ();
public override string ToString ();
public static SKSizeI op_Addition (SKSizeI sz1, SKSizeI sz2);
public static bool op_Equality (SKSizeI sz1, SKSizeI sz2);
public static SKPointI op_Explicit (SKSizeI size);
public static bool op_Inequality (SKSizeI sz1, SKSizeI sz2);
public static SKSizeI op_Subtraction (SKSizeI sz1, SKSizeI sz2);
```


#### Type Changed: SkiaSharp.SKSurface

Added methods:

```csharp
public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc);
public static SKSurface Create (GRContext context, GRBackendTextureDesc desc);
public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props);
public static SKSurface Create (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info);
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount);
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProps props);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);
```


#### Type Changed: SkiaSharp.SKSurfaceProps

Removed field:

```csharp
public SKPixelGeometry PixelGeometry;
```

Added properties:

```csharp
public SKSurfacePropsFlags Flags { get; set; }
public SKPixelGeometry PixelGeometry { get; set; }
```


#### New Type: SkiaSharp.GRBackend

```csharp
[Serializable]
public enum GRBackend {
	OpenGL = 0,
	Vulkan = 1,
}
```

#### New Type: SkiaSharp.GRBackendRenderTargetDesc

```csharp
public struct GRBackendRenderTargetDesc {
	// properties
	public GRPixelConfig Config { get; set; }
	public int Height { get; set; }
	public GRSurfaceOrigin Origin { get; set; }
	public IntPtr RenderTargetHandle { get; set; }
	public int SampleCount { get; set; }
	public int StencilBits { get; set; }
	public int Width { get; set; }
}
```

#### New Type: SkiaSharp.GRBackendTextureDesc

```csharp
public struct GRBackendTextureDesc {
	// properties
	public GRPixelConfig Config { get; set; }
	public GRBackendTextureDescFlags Flags { get; set; }
	public int Height { get; set; }
	public GRSurfaceOrigin Origin { get; set; }
	public int SampleCount { get; set; }
	public IntPtr TextureHandle { get; set; }
	public int Width { get; set; }
}
```

#### New Type: SkiaSharp.GRBackendTextureDescFlags

```csharp
[Serializable]
[Flags]
public enum GRBackendTextureDescFlags {
	None = 0,
	RenderTarget = 1,
}
```

#### New Type: SkiaSharp.GRContext

```csharp
public class GRContext : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public void AbandonContext (bool releaseResources);
	public static GRContext Create (GRBackend backend);
	public static GRContext Create (GRBackend backend, GRGlInterface backendContext);
	public static GRContext Create (GRBackend backend, IntPtr backendContext);
	public static GRContext Create (GRBackend backend, GRGlInterface backendContext, GRContextOptions options);
	public static GRContext Create (GRBackend backend, IntPtr backendContext, GRContextOptions options);
	protected override void Dispose (bool disposing);
	public int GetRecommendedSampleCount (GRPixelConfig config, float dpi);
	public void GetResourceCacheLimits (out int maxResources, out long maxResourceBytes);
	public void GetResourceCacheUsage (out int maxResources, out long maxResourceBytes);
	public void SetResourceCacheLimits (int maxResources, long maxResourceBytes);
}
```

#### New Type: SkiaSharp.GRContextOptions

```csharp
public struct GRContextOptions {
	// properties
	public int BufferMapThreshold { get; set; }
	public bool ClipBatchToBounds { get; set; }
	public bool DoManualMipmapping { get; set; }
	public bool DrawBatchBounds { get; set; }
	public bool ImmediateMode { get; set; }
	public int MaxBatchLookahead { get; set; }
	public int MaxBatchLookback { get; set; }
	public int MaxTextureSizeOverride { get; set; }
	public int MaxTileSizeOverride { get; set; }
	public bool SuppressDualSourceBlending { get; set; }
	public bool SuppressPrints { get; set; }
	public bool UseDrawInsteadOfPartialRenderTargetWrite { get; set; }
	public bool UseShaderSwizzling { get; set; }
}
```

#### New Type: SkiaSharp.GRGlGetProcDelegate

```csharp
public sealed delegate GRGlGetProcDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GRGlGetProcDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (object context, string name, System.AsyncCallback callback, object object);
	public virtual IntPtr EndInvoke (System.IAsyncResult result);
	public virtual IntPtr Invoke (object context, string name);
}
```

#### New Type: SkiaSharp.GRGlInterface

```csharp
public class GRGlInterface : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public static GRGlInterface AssembleGlInterface (GRGlGetProcDelegate get);
	public static GRGlInterface AssembleGlInterface (object context, GRGlGetProcDelegate get);
	public static GRGlInterface AssembleGlesInterface (GRGlGetProcDelegate get);
	public static GRGlInterface AssembleGlesInterface (object context, GRGlGetProcDelegate get);
	public static GRGlInterface AssembleInterface (GRGlGetProcDelegate get);
	public static GRGlInterface AssembleInterface (object context, GRGlGetProcDelegate get);
	public GRGlInterface Clone ();
	public static GRGlInterface CreateDefaultInterface ();
	public static GRGlInterface CreateNativeInterface ();
	protected override void Dispose (bool disposing);
	public bool HasExtension (string extension);
	public bool Validate ();
}
```

#### New Type: SkiaSharp.GRPixelConfig

```csharp
[Serializable]
public enum GRPixelConfig {
	Alpha8 = 1,
	AlphaHalf = 14,
	Astc12x12 = 12,
	Bgra8888 = 6,
	Etc1 = 9,
	Index8 = 2,
	Latc = 10,
	R11Eac = 11,
	Rgb565 = 3,
	Rgba4444 = 4,
	Rgba8888 = 5,
	RgbaFloat = 13,
	RgbaHalf = 15,
	Sbgra8888 = 8,
	Srgba8888 = 7,
	Unknown = 0,
}
```

#### New Type: SkiaSharp.GRSurfaceOrigin

```csharp
[Serializable]
public enum GRSurfaceOrigin {
	BottomLeft = 1,
	TopLeft = 0,
}
```

#### New Type: SkiaSharp.SKSurfacePropsFlags

```csharp
[Serializable]
[Flags]
public enum SKSurfacePropsFlags {
	UseDeviceIndependentFonts = 1,
}
```


