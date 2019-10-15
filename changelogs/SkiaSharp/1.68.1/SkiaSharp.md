# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRBackendRenderTarget

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.GRBackendTexture

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SK3dView

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKAbstractManagedStream

Added methods:

```csharp
protected override void DisposeNative ();
protected virtual IntPtr OnDuplicate ();
protected virtual IntPtr OnFork ();
```


#### Type Changed: SkiaSharp.SKAbstractManagedWStream

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKBitmap

Added methods:

```csharp
protected override void DisposeNative ();
public System.ReadOnlySpan<byte> GetPixelSpan ();
```


#### Type Changed: SkiaSharp.SKCanvas

Added methods:

```csharp
protected override void DisposeNative ();
public void DrawDrawable (SKDrawable drawable, ref SKMatrix matrix);
public void DrawDrawable (SKDrawable drawable, SKPoint p);
public void DrawDrawable (SKDrawable drawable, float x, float y);
```


#### Type Changed: SkiaSharp.SKCodec

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKColorFilter

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKColorSpace

Added properties:

```csharp
public bool IsNumericalTransferFunction { get; }
public SKNamedGamma NamedGamma { get; }
public SKColorSpaceType Type { get; }
```

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Obsoleted methods:

```diff
 [Obsolete ("Use CreateRgb (SKColorSpaceRenderTargetGamma, SKColorSpaceGamut) instead.")]
 public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);
 [Obsolete ("Use CreateRgb (SKColorSpaceRenderTargetGamma, SKMatrix44) instead.")]
 public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);
 [Obsolete ("Use CreateRgb (SKColorSpaceTransferFn, SKColorSpaceGamut) instead.")]
 public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);
 [Obsolete ("Use CreateRgb (SKColorSpaceTransferFn, SKMatrix44) instead.")]
 public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);
```

Modified methods:

```diff
 public SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags--- = 0---)
 public SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags--- = 0---)
 public SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags--- = 0---)
 public SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags--- = 0---)
```

Added methods:

```csharp
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut);
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50);
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut);
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50);
public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKColorSpaceGamut gamut);
public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKMatrix44 toXyzD50);
public SKMatrix44 FromXyzD50 ();
public bool GetNumericalTransferFunction (out SKColorSpaceTransferFn fn);
```


#### Type Changed: SkiaSharp.SKColorSpacePrimaries

Added constructor:

```csharp
public SKColorSpacePrimaries (float[] values);
```

Added property:

```csharp
public float[] Values { get; }
```


#### Type Changed: SkiaSharp.SKColorSpaceTransferFn

Added constructor:

```csharp
public SKColorSpaceTransferFn (float[] values);
```

Added property:

```csharp
public float[] Values { get; }
```

Added method:

```csharp
public float Transform (float x);
```


#### Type Changed: SkiaSharp.SKColorTable

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKData

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added methods:

```csharp
public System.ReadOnlySpan<byte> AsSpan ();
public static SKData CreateCopy (System.ReadOnlySpan<byte> bytes);
```


#### Type Changed: SkiaSharp.SKDocument

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKDynamicMemoryWStream

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKFileStream

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKFileWStream

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKFontManager

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKFontStyle

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKFontStyleSet

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKFrontBufferedManagedStream

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeManaged ();
```


#### Type Changed: SkiaSharp.SKImage

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added methods:

```csharp
public static SKImage FromEncodedData (System.ReadOnlySpan<byte> data);
public static SKImage FromPixelCopy (SKImageInfo info, System.ReadOnlySpan<byte> pixels);
public static SKImage FromPixelCopy (SKImageInfo info, System.ReadOnlySpan<byte> pixels, int rowBytes);
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

#### Type Changed: SkiaSharp.SKImageFilter.CropRect

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```



#### Type Changed: SkiaSharp.SKManagedStream

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added methods:

```csharp
public int CopyTo (SKWStream destination);
protected override void DisposeManaged ();
protected override IntPtr OnDuplicate ();
protected override IntPtr OnFork ();
public SKStreamAsset ToMemoryStream ();
```


#### Type Changed: SkiaSharp.SKManagedWStream

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeManaged ();
```


#### Type Changed: SkiaSharp.SKMaskFilter

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKMatrix

Added constructor:

```csharp
public SKMatrix (float scaleX, float skewX, float transX, float skewY, float scaleY, float transY, float persp0, float persp1, float persp2);
```


#### Type Changed: SkiaSharp.SKMatrix44

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKMemoryStream

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKNativeObject

Added properties:

```csharp
protected bool IgnorePublicDispose { get; set; }
protected bool IsDisposed { get; }
protected virtual bool OwnsHandle { get; set; }
```

Added methods:

```csharp
protected void DisposeInternal ();
protected virtual void DisposeManaged ();
protected virtual void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKObject

Removed property:

```csharp
protected bool OwnsHandle { get; }
```

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added methods:

```csharp
protected override void DisposeManaged ();
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKPaint

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added methods:

```csharp
public long BreakText (IntPtr buffer, int length, float maxWidth);
public long BreakText (IntPtr buffer, int length, float maxWidth, out float measuredWidth);
public bool ContainsGlyphs (IntPtr text, IntPtr length);
public int CountGlyphs (IntPtr text, IntPtr length);
protected override void DisposeNative ();
public float[] GetGlyphWidths (IntPtr text, IntPtr length);
public float[] GetGlyphWidths (IntPtr text, IntPtr length, out SKRect[] bounds);
public ushort[] GetGlyphs (IntPtr text, IntPtr length);
public float[] GetHorizontalTextIntercepts (IntPtr text, IntPtr length, float[] xpositions, float y, float upperBounds, float lowerBounds);
public float[] GetPositionedTextIntercepts (IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds);
public float[] GetTextIntercepts (IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds);
public SKPath GetTextPath (IntPtr buffer, int length, SKPoint[] points);
public SKPath GetTextPath (IntPtr buffer, int length, float x, float y);
public float MeasureText (IntPtr buffer, int length);
public float MeasureText (IntPtr buffer, int length, ref SKRect bounds);
public void Reset ();
```


#### Type Changed: SkiaSharp.SKPath

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```

#### Type Changed: SkiaSharp.SKPath.Iterator

Modified base type:

```diff
-SkiaSharp.SKNativeObject
+SkiaSharp.SKObject
```

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKPath.OpBuilder

Modified base type:

```diff
-SkiaSharp.SKNativeObject
+SkiaSharp.SKObject
```

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKPath.RawIterator

Modified base type:

```diff
-SkiaSharp.SKNativeObject
+SkiaSharp.SKObject
```

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```



#### Type Changed: SkiaSharp.SKPathEffect

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKPathMeasure

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKPicture

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKPictureRecorder

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added methods:

```csharp
protected override void DisposeNative ();
public SKDrawable EndRecordingAsDrawable ();
```


#### Type Changed: SkiaSharp.SKPixmap

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added methods:

```csharp
protected override void DisposeNative ();
public System.ReadOnlySpan<byte> GetPixelSpan ();
```


#### Type Changed: SkiaSharp.SKRegion

Added constructors:

```csharp
public SKRegion (SKPath path);
public SKRegion (SKRectI rect);
```

Added methods:

```csharp
protected override void DisposeNative ();
public bool Intersects (SKPath path);
public bool Op (SKPath path, SKRegionOperation op);
```


#### Type Changed: SkiaSharp.SKRoundRect

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKShader

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKSurface

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKSurfaceProperties

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKSwizzle

Added methods:

```csharp
public static void SwapRedBlue (System.ReadOnlySpan<byte> pixels, int count);
public static void SwapRedBlue (System.ReadOnlySpan<byte> dest, System.ReadOnlySpan<byte> src, int count);
```


#### Type Changed: SkiaSharp.SKTextBlob

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKTextBlobBuilder

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Modified methods:

```diff
-public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] utf8Text, uint[] clusters)
+public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters)
-public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] utf8Text, uint[] clusters, SKRect bounds)
+public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters, SKRect bounds)
-public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] utf8Text, uint[] clusters)
+public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters)
-public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] utf8Text, uint[] clusters, SKRect bounds)
+public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters, SKRect bounds)
-public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] utf8Text, uint[] clusters)
+public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters)
-public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] utf8Text, uint[] clusters, SKRect bounds)
+public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters, SKRect bounds)
```

Added methods:

```csharp
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions);
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, SKRect? bounds);
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions);
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, SKRect? bounds);
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs);
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, SKRect? bounds);
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y);
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount);
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, SKRect? bounds);
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount, SKRect? bounds);
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count);
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount);
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, SKRect? bounds);
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount, SKRect? bounds);
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y);
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount);
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, SKRect? bounds);
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount, SKRect? bounds);
protected override void DisposeNative ();
```


#### Type Changed: SkiaSharp.SKTypeface

Added property:

```csharp
public int TableCount { get; }
```

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added methods:

```csharp
public int CountGlyphs (System.ReadOnlySpan<byte> str, SKEncoding encoding);
public ushort[] GetGlyphs (System.ReadOnlySpan<byte> text, SKEncoding encoding);
public int GetGlyphs (System.ReadOnlySpan<byte> text, SKEncoding encoding, out ushort[] glyphs);
public int GetTableSize (uint tag);
public bool TryGetTableData (uint tag, int offset, int length, IntPtr tableData);
public bool TryGetTableTags (out uint[] tags);
```


#### Type Changed: SkiaSharp.SKVertices

Removed method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKXmlStreamWriter

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added method:

```csharp
protected override void DisposeNative ();
```


#### New Type: SkiaSharp.SKColorSpaceType

```csharp
[Serializable]
public enum SKColorSpaceType {
	Cmyk = 1,
	Gray = 2,
	Rgb = 0,
}
```

#### New Type: SkiaSharp.SKDrawable

```csharp
public class SKDrawable : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	protected SKDrawable ();
	protected SKDrawable (bool owns);
	// properties
	public SKRect Bounds { get; }
	public uint GenerationId { get; }
	// methods
	protected override void DisposeNative ();
	public void Draw (SKCanvas canvas, ref SKMatrix matrix);
	public void Draw (SKCanvas canvas, float x, float y);
	public void NotifyDrawingChanged ();
	protected virtual void OnDraw (SKCanvas canvas);
	protected virtual SKRect OnGetBounds ();
	protected virtual SKPicture OnSnapshot ();
	public SKPicture Snapshot ();
}
```

#### New Type: SkiaSharp.SKHorizontalRunBuffer

```csharp
public sealed class SKHorizontalRunBuffer : SkiaSharp.SKRunBuffer {
	// methods
	public System.Span<float> GetPositionSpan ();
	public void SetPositions (System.ReadOnlySpan<float> positions);
}
```

#### New Type: SkiaSharp.SKNamedGamma

```csharp
[Serializable]
public enum SKNamedGamma {
	Linear = 0,
	NonStandard = 3,
	Srgb = 1,
	TwoDotTwoCurve = 2,
}
```

#### New Type: SkiaSharp.SKPositionedRunBuffer

```csharp
public sealed class SKPositionedRunBuffer : SkiaSharp.SKRunBuffer {
	// methods
	public System.Span<SKPoint> GetPositionSpan ();
	public void SetPositions (System.ReadOnlySpan<SKPoint> positions);
}
```

#### New Type: SkiaSharp.SKRunBuffer

```csharp
public class SKRunBuffer {
	// properties
	public int Size { get; }
	public int TextSize { get; }
	// methods
	public System.Span<uint> GetClusterSpan ();
	public System.Span<ushort> GetGlyphSpan ();
	public System.Span<byte> GetTextSpan ();
	public void SetClusters (System.ReadOnlySpan<uint> clusters);
	public void SetGlyphs (System.ReadOnlySpan<ushort> glyphs);
	public void SetText (System.ReadOnlySpan<byte> text);
}
```


