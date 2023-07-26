# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.60.0.0 vs 1.59.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Removed method:

```csharp
[Obsolete]
public void Flush (GRContextFlushBits flagsBitfield);
```


#### Type Changed: SkiaSharp.GRContextOptions

Removed properties:

```csharp
public int MaxOpCombineLookahead { get; set; }
public int MaxOpCombineLookback { get; set; }
```

Added properties:

```csharp
public float GlyphCacheTextureMaximumBytes { get; set; }
public bool WireframeMode { get; set; }
```


#### Type Changed: SkiaSharp.GRContextOptionsGpuPathRenderers

Removed values:

```csharp
DistanceField = 128,
Pls = 64,
```

Modified fields:

```diff
-All = 1023
+All = 511
-Default = 512
+Default = 256
-Tessellating = 256
+Tessellating = 128
```

Added value:

```csharp
Small = 64,
```


#### Type Changed: SkiaSharp.GRGlInterface

Removed method:

```csharp
[Obsolete]
public static GRGlInterface CreateNativeInterface ();
```


#### Type Changed: SkiaSharp.GRPixelConfig

Removed value:

```csharp
Etc1 = 9,
```

Added value:

```csharp
Rgba8888SInt = 9,
```


#### Type Changed: SkiaSharp.GRSurfaceOrigin

Modified fields:

```diff
-BottomLeft = 1
+BottomLeft = 2
-TopLeft = 0
+TopLeft = 1
```


#### Type Changed: SkiaSharp.SKBitmap

Removed methods:

```csharp
[Obsolete]
public bool CopyPixelsTo (IntPtr dst, int dstSize, int dstRowBytes, bool preserveDstPad);
public void LockPixels ();
public void UnlockPixels ();
```


#### Type Changed: SkiaSharp.SKCanvas

Removed properties:

```csharp
[Obsolete]
public SKRect ClipBounds { get; }

[Obsolete]
public SKRectI ClipDeviceBounds { get; }
```

Removed methods:

```csharp
[Obsolete]
public void ClipPath (SKPath path, SKRegionOperation operation, bool antialias);

[Obsolete]
public void ClipRect (SKRect rect, SKRegionOperation operation, bool antialias);

[Obsolete]
public void DrawColor (SKColor color, SKXferMode mode);

[Obsolete]
public void DrawText (string text, SKPoint[] points, SKPaint paint);

[Obsolete]
public void DrawText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint);

[Obsolete]
public void DrawText (byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete]
public void DrawText (string text, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete]
public void DrawText (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete]
public bool GetClipBounds (ref SKRect bounds);

[Obsolete]
public bool GetClipDeviceBounds (ref SKRectI bounds);
```

Added methods:

```csharp
public void DrawBitmap (SKBitmap bitmap, SKPoint p, SKPaint paint);
public void DrawCircle (SKPoint c, float radius, SKPaint paint);
public void DrawImage (SKImage image, SKPoint p, SKPaint paint);
public void DrawLine (SKPoint p0, SKPoint p1, SKPaint paint);
public void DrawOval (SKPoint c, SKSize r, SKPaint paint);
public void DrawPicture (SKPicture picture, SKPoint p, SKPaint paint);
public void DrawPicture (SKPicture picture, float x, float y, SKPaint paint);
public void DrawPoint (SKPoint p, SKColor color);
public void DrawPoint (SKPoint p, SKPaint paint);
public void DrawRect (float x, float y, float w, float h, SKPaint paint);
public void DrawRoundRect (SKRect rect, SKSize r, SKPaint paint);
public void DrawRoundRect (float x, float y, float w, float h, float rx, float ry, SKPaint paint);
public void DrawSurface (SKSurface surface, SKPoint p, SKPaint paint);
public void DrawText (byte[] text, SKPoint p, SKPaint paint);
public void DrawText (string text, SKPoint p, SKPaint paint);
public void DrawText (IntPtr buffer, int length, SKPoint p, SKPaint paint);
public void DrawTextOnPath (byte[] text, SKPath path, SKPoint offset, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKPaint paint);
public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, SKPoint offset, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKColorFilter

Removed methods:

```csharp
[Obsolete]
public static SKColorFilter CreateBlendMode (SKColor c, SKXferMode mode);

[Obsolete]
public static SKColorFilter CreateXferMode (SKColor c, SKXferMode mode);
```


#### Type Changed: SkiaSharp.SKColorSpace

Removed methods:

```csharp
[Obsolete]
public static SKMatrix44 ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries);

[Obsolete]
public static bool ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries, SKMatrix44 toXyzD50);
```


#### Type Changed: SkiaSharp.SKData

Removed constructors:

```csharp
[Obsolete]
public SKData ();

[Obsolete]
public SKData (byte[] bytes);

[Obsolete]
public SKData (byte[] bytes, ulong length);

[Obsolete]
public SKData (IntPtr bytes, ulong length);
```

Removed method:

```csharp
[Obsolete]
public static SKData FromMallocMemory (IntPtr bytes, ulong length);
```


#### Type Changed: SkiaSharp.SKDocument

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKDocument CreatePdf (string path, float dpi);
 [Obsolete ()]
 public static SKDocument CreateXps (string path, float dpi);
```


#### Type Changed: SkiaSharp.SKDynamicMemoryWStream

Removed method:

```csharp
public void CopyTo (SKWStream dst);
```

Added method:

```csharp
public bool CopyTo (SKWStream dst);
```


#### Type Changed: SkiaSharp.SKFileStream

Added methods:

```csharp
public static bool IsPathSupported (string path);
public static SKStreamAsset OpenStream (string path);
```


#### Type Changed: SkiaSharp.SKFileWStream

Added methods:

```csharp
public static bool IsPathSupported (string path);
public static SKWStream OpenStream (string path);
```


#### Type Changed: SkiaSharp.SKFontStyleWidth

Removed value:

```csharp
[Obsolete]
UltaExpanded = 9,
```


#### Type Changed: SkiaSharp.SKImage

Added property:

```csharp
public bool IsLazyGenerated { get; }
```

Removed methods:

```csharp
[Obsolete]
public SKData Encode (SKImageEncodeFormat format, int quality);

[Obsolete]
public static SKImage FromData (SKData data);

[Obsolete]
public static SKImage FromData (SKData data, SKRectI subset);
```

Added methods:

```csharp
public SKData Encode (SKPixelSerializer serializer);
public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc);
public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc);
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
[Obsolete]
public static SKImageFilter CreateCompose (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);
```


#### Type Changed: SkiaSharp.SKImageInfo

Added methods:

```csharp
public SKImageInfo WithAlphaType (SKAlphaType newAlphaType);
public SKImageInfo WithColorSpace (SKColorSpace newColorSpace);
public SKImageInfo WithColorType (SKColorType newColorType);
```


#### Type Changed: SkiaSharp.SKManagedStream

Modified base type:

```diff
-SkiaSharp.SKStreamAsset
+SkiaSharp.SKAbstractManagedStream
```

Added methods:

```csharp
protected override IntPtr OnCreateNew ();
protected override IntPtr OnGetLength ();
protected override IntPtr OnGetPosition ();
protected override bool OnHasLength ();
protected override bool OnHasPosition ();
protected override bool OnIsAtEnd ();
protected override bool OnMove (int offset);
protected override IntPtr OnPeek (IntPtr buffer, IntPtr size);
protected override IntPtr OnRead (IntPtr buffer, IntPtr size);
protected override bool OnRewind ();
protected override bool OnSeek (IntPtr position);
```


#### Type Changed: SkiaSharp.SKManagedWStream

Modified base type:

```diff
-SkiaSharp.SKWStream
+SkiaSharp.SKAbstractManagedWStream
```

Added methods:

```csharp
protected override IntPtr OnBytesWritten ();
protected override void OnFlush ();
protected override bool OnWrite (IntPtr buffer, IntPtr size);
```


#### Type Changed: SkiaSharp.SKMatrix

Removed method:

```csharp
[Obsolete]
public SKPoint MapXY (float x, float y);
```


#### Type Changed: SkiaSharp.SKPaint

Removed property:

```csharp
[Obsolete]
public SKXferMode XferMode { get; set; }
```

Added methods:

```csharp
public long BreakText (byte[] text, float maxWidth);
public long BreakText (IntPtr buffer, IntPtr length, float maxWidth);
public long BreakText (string text, float maxWidth, out float measuredWidth, out string measuredText);
```


#### Type Changed: SkiaSharp.SKPath

Removed methods:

```csharp
[Obsolete]
public void AddPath (SKPath other, SKPath.AddMode mode);

[Obsolete]
public void AddPath (SKPath other, ref SKMatrix matrix, SKPath.AddMode mode);

[Obsolete]
public void AddPath (SKPath other, float dx, float dy, SKPath.AddMode mode);
```


#### Type Changed: SkiaSharp.SKPathEffect

Removed method:

```csharp
[Obsolete]
public static SKPathEffect Create1DPath (SKPath path, float advance, float phase, SkPath1DPathEffectStyle style);
```


#### Type Changed: SkiaSharp.SKPathMeasure

Removed method:

```csharp
[Obsolete]
public bool GetMatrix (float distance, out SKMatrix matrix, SKPathMeasure.MatrixFlags flags);
```


#### Type Changed: SkiaSharp.SKPicture

Removed property:

```csharp
[Obsolete]
public SKRect Bounds { get; }
```


#### Type Changed: SkiaSharp.SKPixmap

Added methods:

```csharp
public SKPixmap WithAlphaType (SKAlphaType newAlphaType);
public SKPixmap WithColorSpace (SKColorSpace newColorSpace);
public SKPixmap WithColorType (SKColorType newColorType);
```


#### Type Changed: SkiaSharp.SKShader

Removed method:

```csharp
[Obsolete]
public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB, SKXferMode mode);
```


#### Type Changed: SkiaSharp.SKStream

Added method:

```csharp
public int Peek (IntPtr buffer, int size);
```


#### Type Changed: SkiaSharp.SKStrokeJoin

Removed value:

```csharp
[Obsolete]
Mitter = 0,
```


#### Type Changed: SkiaSharp.SKSurface

Added methods:

```csharp
public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc);
public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);
public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc);
public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);
```


#### Type Changed: SkiaSharp.StringUtilities

Added method:

```csharp
public static string GetString (byte[] data, int index, int count, SKTextEncoding encoding);
```


#### Removed Type SkiaSharp.SKAutoLockPixels
#### New Type: SkiaSharp.GRGlBackendTextureDesc

```csharp
public struct GRGlBackendTextureDesc {
	// properties
	public GRPixelConfig Config { get; set; }
	public GRBackendTextureDescFlags Flags { get; set; }
	public int Height { get; set; }
	public GRSurfaceOrigin Origin { get; set; }
	public int SampleCount { get; set; }
	public GRGlTextureInfo TextureHandle { get; set; }
	public int Width { get; set; }
}
```

#### New Type: SkiaSharp.GRGlTextureInfo

```csharp
public struct GRGlTextureInfo {
	// properties
	public uint Id { get; set; }
	public uint Target { get; set; }
}
```

#### New Type: SkiaSharp.SKAbstractManagedStream

```csharp
public abstract class SKAbstractManagedStream : SkiaSharp.SKStreamAsset, System.IDisposable {
	// constructors
	protected SKAbstractManagedStream ();
	protected SKAbstractManagedStream (bool owns);
	// methods
	protected override void Dispose (bool disposing);
	protected virtual IntPtr OnCreateNew ();
	protected virtual IntPtr OnGetLength ();
	protected virtual IntPtr OnGetPosition ();
	protected virtual bool OnHasLength ();
	protected virtual bool OnHasPosition ();
	protected virtual bool OnIsAtEnd ();
	protected virtual bool OnMove (int offset);
	protected virtual IntPtr OnPeek (IntPtr buffer, IntPtr size);
	protected virtual IntPtr OnRead (IntPtr buffer, IntPtr size);
	protected virtual bool OnRewind ();
	protected virtual bool OnSeek (IntPtr position);
}
```

#### New Type: SkiaSharp.SKAbstractManagedWStream

```csharp
public abstract class SKAbstractManagedWStream : SkiaSharp.SKWStream, System.IDisposable {
	// constructors
	protected SKAbstractManagedWStream ();
	protected SKAbstractManagedWStream (bool owns);
	// methods
	protected override void Dispose (bool disposing);
	protected virtual IntPtr OnBytesWritten ();
	protected virtual void OnFlush ();
	protected virtual bool OnWrite (IntPtr buffer, IntPtr size);
}
```

#### New Type: SkiaSharp.SKFrontBufferedManagedStream

```csharp
public class SKFrontBufferedManagedStream : SkiaSharp.SKAbstractManagedStream, System.IDisposable {
	// constructors
	public SKFrontBufferedManagedStream (SKStream nativeStream, int bufferSize);
	public SKFrontBufferedManagedStream (System.IO.Stream managedStream, int bufferSize);
	public SKFrontBufferedManagedStream (SKStream nativeStream, int bufferSize, bool disposeUnderlyingStream);
	public SKFrontBufferedManagedStream (System.IO.Stream managedStream, int bufferSize, bool disposeUnderlyingStream);
	// methods
	protected override void Dispose (bool disposing);
	protected override IntPtr OnCreateNew ();
	protected override IntPtr OnGetLength ();
	protected override IntPtr OnGetPosition ();
	protected override bool OnHasLength ();
	protected override bool OnHasPosition ();
	protected override bool OnIsAtEnd ();
	protected override bool OnMove (int offset);
	protected override IntPtr OnPeek (IntPtr buffer, IntPtr size);
	protected override IntPtr OnRead (IntPtr buffer, IntPtr size);
	protected override bool OnRewind ();
	protected override bool OnSeek (IntPtr position);
}
```

#### New Type: SkiaSharp.SKManagedPixelSerializer

```csharp
public abstract class SKManagedPixelSerializer : SkiaSharp.SKPixelSerializer, System.IDisposable {
	// constructors
	public SKManagedPixelSerializer ();
	// methods
	protected virtual SKData OnEncode (SKPixmap pixmap);
	protected virtual bool OnUseEncodedData (IntPtr data, IntPtr length);
}
```

#### New Type: SkiaSharp.SKPixelSerializer

```csharp
public class SKPixelSerializer : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public static SKPixelSerializer Create (System.Func<SKPixmap,SkiaSharp.SKData> onEncode);
	public static SKPixelSerializer Create (System.Func<System.IntPtr,System.IntPtr,System.Boolean> onUseEncodedData, System.Func<SKPixmap,SkiaSharp.SKData> onEncode);
	protected override void Dispose (bool disposing);
	public SKData Encode (SKPixmap pixmap);
	public bool UseEncodedData (IntPtr data, ulong length);
}
```

#### New Type: SkiaSharp.SKSwizzle

```csharp
public static class SKSwizzle {
	// methods
	public static void SwapRedBlue (IntPtr pixels, int count);
	public static void SwapRedBlue (IntPtr dest, IntPtr src, int count);
}
```


