# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.53.0.0 vs 1.49.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKAlphaType

Modified fields:

```diff
-Opaque = 0
+Opaque = 1
-Premul = 1
+Premul = 2
-Unpremul = 2
+Unpremul = 3
```

Added value:

```csharp
Unknown = 0,
```


#### Type Changed: SkiaSharp.SKBitmap

Removed methods:

```csharp
public static SKBitmap Decode (SKStreamRewindable stream, SKColorType pref);
public static SKBitmap Decode (byte[] buffer, SKColorType pref);
public static SKBitmap Decode (string filename, SKColorType pref);
public static SKImageInfo DecodeBounds (SKStreamRewindable stream, SKColorType pref);
public static SKImageInfo DecodeBounds (byte[] buffer, SKColorType pref);
public static SKImageInfo DecodeBounds (string filename, SKColorType pref);
```

Added methods:

```csharp
public static SKBitmap Decode (SKCodec codec);
public static SKBitmap Decode (SKData data);
public static SKBitmap Decode (SKStream stream);
public static SKBitmap Decode (byte[] buffer);
public static SKBitmap Decode (string filename);
public static SKImageInfo DecodeBounds (SKData data);
public static SKImageInfo DecodeBounds (SKStream stream);
public static SKImageInfo DecodeBounds (byte[] buffer);
public static SKImageInfo DecodeBounds (string filename);
```


#### Type Changed: SkiaSharp.SKCanvas

Removed methods:

```csharp
public void ClipPath (SKPath path);
public void ClipRect (SKRect rect);
public void Save ();
public void SaveLayer (SKPaint paint);
public void SaveLayer (SKRect limit, SKPaint paint);
```

Added methods:

```csharp
public void ClipPath (SKPath path, SKRegionOperation operation, bool antialias);
public void ClipRect (SKRect rect, SKRegionOperation operation, bool antialias);
public void DrawCircle (float cx, float cy, float radius, SKPaint paint);
public void DrawRoundRect (SKRect rect, float rx, float ry, SKPaint paint);
public bool GetClipBounds (ref SKRect bounds);
public bool GetClipDeviceBounds (ref SKRectI bounds);
public void RotateDegrees (float degrees, float px, float py);
public void RotateRadians (float radians, float px, float py);
public int Save ();
public int SaveLayer (SKPaint paint);
public int SaveLayer (SKRect limit, SKPaint paint);
public void Scale (float sx, float sy, float px, float py);
```


#### Type Changed: SkiaSharp.SKColorType

Removed values:

```csharp
Alpha_8 = 3,
Bgra_8888 = 2,
N_32 = 5,
Rgb_565 = 4,
Rgba_8888 = 1,
```

Added values:

```csharp
Alpha8 = 1,
Argb4444 = 3,
Bgra8888 = 5,
Gray8 = 7,
Index8 = 6,
Rgb565 = 2,
Rgba8888 = 4,
RgbaF16 = 8,
```


#### Type Changed: SkiaSharp.SKFileStream

Added method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKImage

Added method:

```csharp
public static SKImage FromBitmap (SKBitmap bitmap);
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
public static SKImageFilter CreateDownSample (float scale, SKImageFilter input);
```


#### Type Changed: SkiaSharp.SKImageInfo

Added constructors:

```csharp
public SKImageInfo (int width, int height);
public SKImageInfo (int width, int height, SKColorType colorType);
```

Added field:

```csharp
public static SKColorType PlatformColorType;
```

Added property:

```csharp
public int BytesSize { get; }
```


#### Type Changed: SkiaSharp.SKMatrix

Added methods:

```csharp
public static void Concat (ref SKMatrix target, ref SKMatrix first, ref SKMatrix second);
public static SKMatrix MakeRotation (float radians, float pivotx, float pivoty);
public static SKMatrix MakeRotationDegrees (float degrees);
public static SKMatrix MakeRotationDegrees (float degrees, float pivotx, float pivoty);
public SKPoint[] MapPoints (SKPoint[] points);
public void MapPoints (SKPoint[] result, SKPoint[] points);
public float MapRadius (float radius);
public SKRect MapRect (SKRect source);
public void MapRect (ref SKMatrix matrix, out SKRect dest, ref SKRect source);
public SKPoint MapVector (float x, float y);
public SKPoint[] MapVectors (SKPoint[] vectors);
public void MapVectors (SKPoint[] result, SKPoint[] vectors);
public SKPoint MapXY (float x, float y);
public static void PostConcat (ref SKMatrix target, ref SKMatrix matrix);
public static void PreConcat (ref SKMatrix target, ref SKMatrix matrix);
public static void Rotate (ref SKMatrix matrix, float radians);
public static void Rotate (ref SKMatrix matrix, float radians, float pivotx, float pivoty);
public static void RotateDegrees (ref SKMatrix matrix, float degrees);
public static void RotateDegrees (ref SKMatrix matrix, float degrees, float pivotx, float pivoty);
public bool TryInvert (out SKMatrix inverse);
```


#### Type Changed: SkiaSharp.SKMemoryStream

Added method:

```csharp
protected override void Dispose (bool disposing);
```


#### Type Changed: SkiaSharp.SKObject

Added property:

```csharp
protected bool OwnsHandle { get; }
```


#### Type Changed: SkiaSharp.SKPaint

Added properties:

```csharp
public SKFilterQuality FilterQuality { get; set; }
public SKFontMetrics FontMetrics { get; }
public bool IsDither { get; set; }
public bool IsVerticalText { get; set; }
public SKPathEffect PathEffect { get; set; }
```

Added methods:

```csharp
public SKPath GetTextPath (string text, SKPoint[] points);
public SKPath GetTextPath (IntPtr buffer, IntPtr length, SKPoint[] points);
public SKPath GetTextPath (string text, float x, float y);
public SKPath GetTextPath (IntPtr buffer, IntPtr length, float x, float y);
```


#### Type Changed: SkiaSharp.SKPath

Added constructor:

```csharp
public SKPath (SKPath path);
```

Added property:

```csharp
public SKPathFillType FillType { get; set; }
```

Added methods:

```csharp
public void AddArc (SKRect oval, float startAngle, float sweepAngle);
public void AddCircle (float x, float y, float radius, SKPathDirection dir);
public void AddPath (SKPath other, SKPath.AddMode mode);
public void AddPath (SKPath other, ref SKMatrix matrix, SKPath.AddMode mode);
public void AddPath (SKPath other, float dx, float dy, SKPath.AddMode mode);
public void AddPathReverse (SKPath other);
public void AddRect (SKRect rect, SKPathDirection direction, uint startIndex);
public void AddRoundedRect (SKRect rect, float rx, float ry, SKPathDirection dir);
public void ArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);
public SKPath.Iterator CreateIterator (bool forceClose);
public SKPath.RawIterator CreateRawIterator ();
public void RArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);
public void RConicTo (float dx0, float dy0, float dx1, float dy1, float w);
public void RCubicTo (float dx0, float dy0, float dx1, float dy1, float dx2, float dy2);
public void RLineTo (float dx, float dy);
public void RMoveTo (float dx, float dy);
public void RQuadTo (float dx0, float dy0, float dx1, float dy1);
public void Reset ();
public void Rewind ();
public void Transform (SKMatrix matrix);
```


#### Type Changed: SkiaSharp.SKPictureRecorder

Removed constructor:

```csharp
public SKPictureRecorder (IntPtr handle);
```

Added constructor:

```csharp
public SKPictureRecorder (IntPtr handle, bool owns);
```


#### Type Changed: SkiaSharp.SKSurfaceProps

Added field:

```csharp
public SKPixelGeometry PixelGeometry;
```

Removed property:

```csharp
public SKPixelGeometry PixelGeometry { get; set; }
```


#### Type Changed: SkiaSharp.SKTypeface

Added property:

```csharp
public string FamilyName { get; }
```

Added methods:

```csharp
public static SKTypeface FromFamilyName (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant);
public static SKTypeface FromFamilyName (string familyName, int weight, int width, SKFontStyleSlant slant);
public byte[] GetTableData (uint tag);
public uint[] GetTableTags ();
public bool TryGetTableData (uint tag, out byte[] tableData);
```


#### Removed Type SkiaSharp.SKImageDecoder
#### Removed Type SkiaSharp.SKImageDecoderFormat
#### Removed Type SkiaSharp.SKImageDecoderMode
#### Removed Type SkiaSharp.SKImageDecoderResult
#### New Type: SkiaSharp.SKCodec

```csharp
public class SKCodec : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public SKEncodedFormat EncodedFormat { get; }
	public SKImageInfo Info { get; }
	public static int MinBufferedBytesNeeded { get; }
	public SKCodecOrigin Origin { get; }
	public byte[] Pixels { get; }
	// methods
	public static SKCodec Create (SKData data);
	public static SKCodec Create (SKStream stream);
	protected override void Dispose (bool disposing);
	public SKCodecResult GetPixels (out byte[] pixels);
	public SKCodecResult GetPixels (SKImageInfo info, byte[] pixels);
	public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels);
	public SKCodecResult GetPixels (SKImageInfo info, out byte[] pixels);
	public SKSizeI GetScaledDimensions (float desiredScale);
	public void GetValidSubset (ref SKRectI desiredSubset);
}
```

#### New Type: SkiaSharp.SKCodecOptions

```csharp
public struct SKCodecOptions {
	// fields
	public bool HasSubset;
	public SKRectI Subset;
	public SKZeroInitialized ZeroInitialized;
}
```

#### New Type: SkiaSharp.SKCodecOrigin

```csharp
[Serializable]
public enum SKCodecOrigin {
	BottomLeft = 4,
	BottomRight = 3,
	LeftBottom = 8,
	LeftTop = 5,
	RightBottom = 7,
	RightTop = 6,
	TopLeft = 1,
	TopRight = 2,
}
```

#### New Type: SkiaSharp.SKCodecResult

```csharp
[Serializable]
public enum SKCodecResult {
	CouldNotRewind = 6,
	IncompleteInput = 1,
	InvalidConversion = 2,
	InvalidInput = 5,
	InvalidParameters = 4,
	InvalidScale = 3,
	Success = 0,
	Unimplemented = 7,
}
```

#### New Type: SkiaSharp.SKDocument

```csharp
public class SKDocument : SkiaSharp.SKObject, System.IDisposable {
	// fields
	public static const float DefaultRasterDpi;
	// methods
	public void Abort ();
	public SKCanvas BeginPage (float width, float height);
	public SKCanvas BeginPage (float width, float height, SKRect content);
	public bool Close ();
	public static SKDocument CreatePdf (SKWStream stream, float dpi);
	public static SKDocument CreatePdf (string path, float dpi);
	protected override void Dispose (bool disposing);
	public void EndPage ();
}
```

#### New Type: SkiaSharp.SKDynamicMemoryWStream

```csharp
public class SKDynamicMemoryWStream : SkiaSharp.SKWStream, System.IDisposable {
	// constructors
	public SKDynamicMemoryWStream ();
	// methods
	public SKData CopyToData ();
	public SKStreamAsset DetachAsStream ();
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKEncodedFormat

```csharp
[Serializable]
public enum SKEncodedFormat {
	Astc = 10,
	Bmp = 1,
	Dng = 11,
	Gif = 2,
	Ico = 3,
	Jpeg = 4,
	Ktx = 9,
	Pkm = 8,
	Png = 5,
	Unknown = 0,
	Wbmp = 6,
	Webp = 7,
}
```

#### New Type: SkiaSharp.SKFileWStream

```csharp
public class SKFileWStream : SkiaSharp.SKWStream, System.IDisposable {
	// constructors
	public SKFileWStream (string path);
	// methods
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKFontMetrics

```csharp
public struct SKFontMetrics {
	// properties
	public float Ascent { get; }
	public float AverageCharacterWidth { get; }
	public float Bottom { get; }
	public float CapHeight { get; }
	public float Descent { get; }
	public float Leading { get; }
	public float MaxCharacterWidth { get; }
	public float Top { get; }
	public float? UnderlinePosition { get; }
	public float? UnderlineThickness { get; }
	public float XHeight { get; }
	public float XMax { get; }
	public float XMin { get; }
}
```

#### New Type: SkiaSharp.SKFontStyleSlant

```csharp
[Serializable]
public enum SKFontStyleSlant {
	Italic = 1,
	Oblique = 2,
	Upright = 0,
}
```

#### New Type: SkiaSharp.SKFontStyleWeight

```csharp
[Serializable]
public enum SKFontStyleWeight {
	Black = 900,
	Bold = 700,
	ExtraBold = 800,
	ExtraLight = 200,
	Light = 300,
	Medium = 500,
	Normal = 400,
	SemiBold = 600,
	Thin = 100,
}
```

#### New Type: SkiaSharp.SKFontStyleWidth

```csharp
[Serializable]
public enum SKFontStyleWidth {
	Condensed = 3,
	Expanded = 7,
	ExtraCondensed = 2,
	ExtraExpanded = 8,
	Normal = 5,
	SemiCondensed = 4,
	SemiExpanded = 6,
	UltaExpanded = 9,
	UltraCondensed = 1,
}
```

#### New Type: SkiaSharp.SKPathArcSize

```csharp
[Serializable]
public enum SKPathArcSize {
	Large = 1,
	Small = 0,
}
```

#### New Type: SkiaSharp.SKPathEffect

```csharp
public class SKPathEffect : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public static SKPathEffect Create1DPath (SKPath path, float advance, float phase, SkPath1DPathEffectStyle style);
	public static SKPathEffect Create2DLine (float width, SKMatrix matrix);
	public static SKPathEffect Create2DPath (SKMatrix matrix, SKPath path);
	public static SKPathEffect CreateCompose (SKPathEffect outer, SKPathEffect inner);
	public static SKPathEffect CreateCorner (float radius);
	public static SKPathEffect CreateDash (float[] intervals, float phase);
	public static SKPathEffect CreateDiscrete (float segLength, float deviation, uint seedAssist);
	public static SKPathEffect CreateSum (SKPathEffect first, SKPathEffect second);
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKPathFillType

```csharp
[Serializable]
public enum SKPathFillType {
	EvenOdd = 1,
	InverseEvenOdd = 3,
	InverseWinding = 2,
	Winding = 0,
}
```

#### New Type: SkiaSharp.SKRegionOperation

```csharp
[Serializable]
public enum SKRegionOperation {
	Difference = 0,
	Intersect = 1,
	Replace = 5,
	ReverseDifference = 4,
	Union = 2,
	XOR = 3,
}
```

#### New Type: SkiaSharp.SKWStream

```csharp
public abstract class SKWStream : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public int BytesWritten { get; }
	// methods
	public void Flush ();
	public static int GetSizeOfPackedUInt32 (uint value);
	public void NewLine ();
	public bool Write (byte[] buffer, int size);
	public bool Write16 (ushort value);
	public bool Write32 (uint value);
	public bool Write8 (byte value);
	public bool WriteBigDecimalAsText (long value, int digits);
	public bool WriteBool (bool value);
	public bool WriteDecimalAsTest (int value);
	public bool WriteHexAsText (uint value, int digits);
	public bool WritePackedUInt32 (uint value);
	public bool WriteScalar (float value);
	public bool WriteScalarAsText (float value);
	public bool WriteStream (SKStream input, int length);
	public bool WriteText (string value);
}
```

#### New Type: SkiaSharp.SKZeroInitialized

```csharp
[Serializable]
public enum SKZeroInitialized {
	No = 1,
	Yes = 0,
}
```

#### New Type: SkiaSharp.SkPath1DPathEffectStyle

```csharp
[Serializable]
public enum SkPath1DPathEffectStyle {
	Morph = 2,
	Rotate = 1,
	Translate = 0,
}
```


