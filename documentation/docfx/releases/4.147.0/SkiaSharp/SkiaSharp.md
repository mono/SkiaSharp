# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 4.147.0.0 vs 3.119.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Added methods:

```csharp
public void Flush (SKImage image);
public void Flush (SKSurface surface);
```


#### Type Changed: SkiaSharp.GRVkImageInfo

Modified properties:

```diff
-public GrVkYcbcrConversionInfo YcbcrConversionInfo { get; set; }
+public GRVkYcbcrConversionInfo YcbcrConversionInfo { get; set; }
```


#### Type Changed: SkiaSharp.GrVkYcbcrConversionInfo

Removed interface:

```csharp
System.IEquatable<GrVkYcbcrConversionInfo>
```

Obsoleted properties:

```diff
 [Obsolete ("FormatFeatures is no longer supported in the native API.")]
 public uint FormatFeatures { get; set; }
```

Added properties:

```csharp
public GRVkYcbcrComponents Components { get; set; }
public bool SamplerFilterMustMatchChromaFilter { get; set; }
public bool SupportsLinearFilter { get; set; }
```

Removed methods:

```csharp
public virtual bool Equals (GrVkYcbcrConversionInfo obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (GrVkYcbcrConversionInfo left, GrVkYcbcrConversionInfo right);
public static bool op_Inequality (GrVkYcbcrConversionInfo left, GrVkYcbcrConversionInfo right);
```

Added methods:

```csharp
public static GrVkYcbcrConversionInfo op_Implicit (GRVkYcbcrConversionInfo value);
public static GRVkYcbcrConversionInfo op_Implicit (GrVkYcbcrConversionInfo value);
```


#### Type Changed: SkiaSharp.SKCanvas

Added methods:

```csharp
public void DrawSurface (SKSurface surface, SKPoint p, SKSamplingOptions sampling, SKPaint paint);
public void DrawSurface (SKSurface surface, float x, float y, SKSamplingOptions sampling, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKColorSpace

Added method:

```csharp
public static SKColorSpace CreateCicp (SKColorspacePrimariesCicp colorPrimaries, SKColorspaceTransferFnCicp transferCharacteristics);
```


#### Type Changed: SkiaSharp.SKColorType

Added values:

```csharp
Bgra10101010XR = 25,
R16Unorm = 27,
RgbF16F16F16x = 26,
```


#### Type Changed: SkiaSharp.SKDocument

Added methods:

```csharp
public static SKDocument CreateXps (SKWStream stream, SKDocumentXpsOptions options);
public static SKDocument CreateXps (System.IO.Stream stream, SKDocumentXpsOptions options);
public static SKDocument CreateXps (string path, SKDocumentXpsOptions options);
```


#### Type Changed: SkiaSharp.SKMaskFilter

Added method:

```csharp
public static SKMaskFilter CreateShader (SKShader shader);
```


#### Type Changed: SkiaSharp.SKPaint

Obsoleted methods:

```diff
 [Obsolete ("Use the SKPathBuilder overload instead.")]
 public bool GetFillPath (SKPath src, SKPath dst);
 [Obsolete ("Use the SKPathBuilder overload instead.")]
 public bool GetFillPath (SKPath src, SKPath dst, SKMatrix matrix);
 [Obsolete ("Use the SKPathBuilder overload instead.")]
 public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect);
 [Obsolete ("Use the SKPathBuilder overload instead.")]
 public bool GetFillPath (SKPath src, SKPath dst, float resScale);
 [Obsolete ("Use the SKPathBuilder overload instead.")]
 public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, SKMatrix matrix);
 [Obsolete ("Use the SKPathBuilder overload instead.")]
 public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, float resScale);
```

Added methods:

```csharp
public bool GetFillPath (SKPath src, SKPathBuilder dst);
public bool GetFillPath (SKPath src, SKPathBuilder dst, SKMatrix matrix);
public bool GetFillPath (SKPath src, SKPathBuilder dst, SKRect cullRect);
public bool GetFillPath (SKPath src, SKPathBuilder dst, float resScale);
public bool GetFillPath (SKPath src, SKPathBuilder dst, SKRect cullRect, SKMatrix matrix);
public bool GetFillPath (SKPath src, SKPathBuilder dst, SKRect cullRect, float resScale);
```


#### Type Changed: SkiaSharp.SKPath

Added property:

```csharp
protected override IntPtr Handle { get; set; }
```

Obsoleted methods:

```diff
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddArc (SKRect oval, float startAngle, float sweepAngle);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddCircle (float x, float y, float radius, SKPathDirection dir);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddOval (SKRect rect, SKPathDirection direction);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddPath (SKPath other, SKPathAddMode mode);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddPath (SKPath other, ref SKMatrix matrix, SKPathAddMode mode);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddPath (SKPath other, float dx, float dy, SKPathAddMode mode);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddPathReverse (SKPath other);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddPoly (SKPoint[] points, bool close);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddPoly (System.ReadOnlySpan<SKPoint> points, bool close);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddRect (SKRect rect, SKPathDirection direction);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddRect (SKRect rect, SKPathDirection direction, uint startIndex);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddRoundRect (SKRoundRect rect, SKPathDirection direction);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddRoundRect (SKRoundRect rect, SKPathDirection direction, uint startIndex);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void AddRoundRect (SKRect rect, float rx, float ry, SKPathDirection dir);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void ArcTo (SKPoint point1, SKPoint point2, float radius);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void ArcTo (SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void ArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void ArcTo (float x1, float y1, float x2, float y2, float radius);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void ArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void Close ();
 [Obsolete ("Use SKPathBuilder instead.")]
 public void ConicTo (SKPoint point0, SKPoint point1, float w);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void ConicTo (float x0, float y0, float x1, float y1, float w);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void CubicTo (SKPoint point0, SKPoint point1, SKPoint point2);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void LineTo (SKPoint point);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void LineTo (float x, float y);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void MoveTo (SKPoint point);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void MoveTo (float x, float y);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void QuadTo (SKPoint point0, SKPoint point1);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void QuadTo (float x0, float y0, float x1, float y1);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RConicTo (SKPoint point0, SKPoint point1, float w);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RConicTo (float dx0, float dy0, float dx1, float dy1, float w);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RCubicTo (SKPoint point0, SKPoint point1, SKPoint point2);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RCubicTo (float dx0, float dy0, float dx1, float dy1, float dx2, float dy2);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RLineTo (SKPoint point);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RLineTo (float dx, float dy);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RMoveTo (SKPoint point);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RMoveTo (float dx, float dy);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RQuadTo (SKPoint point0, SKPoint point1);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void RQuadTo (float dx0, float dy0, float dx1, float dy1);
 [Obsolete ("Use SKPathBuilder instead.")]
 public void Rewind ();
```


#### Type Changed: SkiaSharp.SKPathMeasure

Obsoleted methods:

```diff
 [Obsolete ("Use the SKPathBuilder overload instead.")]
 public bool GetSegment (float start, float stop, SKPath dst, bool startWithMoveTo);
```

Added method:

```csharp
public bool GetSegment (float start, float stop, SKPathBuilder dst, bool startWithMoveTo);
```


#### Type Changed: SkiaSharp.SKStream

Obsoleted methods:

```diff
 [Obsolete ("The native stream move offset is capped at a 32-bit int. Use Move(int) instead.")]
 public bool Move (long offset);
```

Added method:

```csharp
public SKData GetData ();
```


#### Type Changed: SkiaSharp.SKSurface

Added methods:

```csharp
public void Draw (SKCanvas canvas, SKPoint p, SKSamplingOptions sampling, SKPaint paint);
public void Draw (SKCanvas canvas, float x, float y, SKSamplingOptions sampling, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKTypeface

Added properties:

```csharp
public static SKTypeface Empty { get; }
public bool IsEmpty { get; }
public int VariationDesignParameterCount { get; }
public SKFontVariationAxis[] VariationDesignParameters { get; }
public SKFontVariationPositionCoordinate[] VariationDesignPosition { get; }
public int VariationDesignPositionCount { get; }
```

Added methods:

```csharp
public SKTypeface Clone (SKFontArguments args);
public SKTypeface Clone (int paletteIndex);
public SKTypeface Clone (System.ReadOnlySpan<SKFontVariationPositionCoordinate> position);
public int GetVariationDesignParameters (System.Span<SKFontVariationAxis> axes);
public int GetVariationDesignPosition (System.Span<SKFontVariationPositionCoordinate> coordinates);
```


#### New Type: SkiaSharp.GRVkYcbcrComponents

```csharp
public struct GRVkYcbcrComponents, System.IEquatable<GRVkYcbcrComponents> {
	// properties
	public uint A { get; set; }
	public uint B { get; set; }
	public uint G { get; set; }
	public uint R { get; set; }
	// methods
	public virtual bool Equals (GRVkYcbcrComponents obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (GRVkYcbcrComponents left, GRVkYcbcrComponents right);
	public static bool op_Inequality (GRVkYcbcrComponents left, GRVkYcbcrComponents right);
}
```

#### New Type: SkiaSharp.GRVkYcbcrConversionInfo

```csharp
public struct GRVkYcbcrConversionInfo, System.IEquatable<GRVkYcbcrConversionInfo> {
	// properties
	public uint ChromaFilter { get; set; }
	public GRVkYcbcrComponents Components { get; set; }
	public ulong ExternalFormat { get; set; }
	public uint ForceExplicitReconstruction { get; set; }
	public uint Format { get; set; }
	public bool SamplerFilterMustMatchChromaFilter { get; set; }
	public bool SupportsLinearFilter { get; set; }
	public uint XChromaOffset { get; set; }
	public uint YChromaOffset { get; set; }
	public uint YcbcrModel { get; set; }
	public uint YcbcrRange { get; set; }
	// methods
	public virtual bool Equals (GRVkYcbcrConversionInfo obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (GRVkYcbcrConversionInfo left, GRVkYcbcrConversionInfo right);
	public static bool op_Inequality (GRVkYcbcrConversionInfo left, GRVkYcbcrConversionInfo right);
}
```

#### New Type: SkiaSharp.SKColorspacePrimariesCicp

```csharp
[Serializable]
public enum SKColorspacePrimariesCicp {
	GenericFilm = 8,
	ItuTH273Value22 = 22,
	Rec2020 = 9,
	Rec470SystemBg = 5,
	Rec470SystemM = 4,
	Rec601 = 6,
	Rec709 = 1,
	SmpteEg4321 = 12,
	SmpteRp4312 = 11,
	SmpteSt240 = 7,
	SmpteSt4281 = 10,
	Unknown = 0,
}
```

#### New Type: SkiaSharp.SKColorspaceTransferFnCicp

```csharp
[Serializable]
public enum SKColorspaceTransferFnCicp {
	Hlg = 18,
	Iec6196621 = 13,
	Iec6196624 = 11,
	Linear = 8,
	Pq = 16,
	Rec202010bit = 14,
	Rec202012bit = 15,
	Rec470SystemBg = 5,
	Rec470SystemM = 4,
	Rec601 = 6,
	Rec709 = 1,
	SmpteSt240 = 7,
	SmpteSt4281 = 17,
	Unknown = 0,
}
```

#### New Type: SkiaSharp.SKDocumentXpsOptions

```csharp
public struct SKDocumentXpsOptions, System.IEquatable<SKDocumentXpsOptions> {
	// properties
	public bool AllowNoPngs { get; set; }
	public float Dpi { get; set; }
	// methods
	public virtual bool Equals (SKDocumentXpsOptions obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKDocumentXpsOptions left, SKDocumentXpsOptions right);
	public static bool op_Inequality (SKDocumentXpsOptions left, SKDocumentXpsOptions right);
}
```

#### New Type: SkiaSharp.SKFontArguments

```csharp
public struct SKFontArguments {
	// properties
	public int CollectionIndex { get; set; }
	public int PaletteIndex { get; set; }
	public System.ReadOnlySpan<SKFontPaletteOverride> PaletteOverrides { get; set; }
	public System.ReadOnlySpan<SKFontVariationPositionCoordinate> VariationDesignPosition { get; set; }
}
```

#### New Type: SkiaSharp.SKFontPaletteOverride

```csharp
public struct SKFontPaletteOverride, System.IEquatable<SKFontPaletteOverride> {
	// properties
	public uint Color { get; set; }
	public ushort Index { get; set; }
	// methods
	public virtual bool Equals (SKFontPaletteOverride obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKFontPaletteOverride left, SKFontPaletteOverride right);
	public static bool op_Inequality (SKFontPaletteOverride left, SKFontPaletteOverride right);
}
```

#### New Type: SkiaSharp.SKFontVariationAxis

```csharp
public struct SKFontVariationAxis, System.IEquatable<SKFontVariationAxis> {
	// properties
	public float Default { get; set; }
	public bool IsHidden { get; set; }
	public float Max { get; set; }
	public float Min { get; set; }
	public SKFourByteTag Tag { get; set; }
	// methods
	public virtual bool Equals (SKFontVariationAxis obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKFontVariationAxis left, SKFontVariationAxis right);
	public static bool op_Inequality (SKFontVariationAxis left, SKFontVariationAxis right);
}
```

#### New Type: SkiaSharp.SKFontVariationPositionCoordinate

```csharp
public struct SKFontVariationPositionCoordinate, System.IEquatable<SKFontVariationPositionCoordinate> {
	// properties
	public SKFourByteTag Axis { get; set; }
	public float Value { get; set; }
	// methods
	public virtual bool Equals (SKFontVariationPositionCoordinate obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKFontVariationPositionCoordinate left, SKFontVariationPositionCoordinate right);
	public static bool op_Inequality (SKFontVariationPositionCoordinate left, SKFontVariationPositionCoordinate right);
}
```

#### New Type: SkiaSharp.SKFourByteTag

```csharp
public struct SKFourByteTag, System.IEquatable<SKFourByteTag> {
	// constructors
	public SKFourByteTag (uint value);
	public SKFourByteTag (char c1, char c2, char c3, char c4);
	// methods
	public virtual bool Equals (SKFourByteTag other);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static SKFourByteTag Parse (string tag);
	public override string ToString ();
	public static bool op_Equality (SKFourByteTag left, SKFourByteTag right);
	public static uint op_Implicit (SKFourByteTag tag);
	public static SKFourByteTag op_Implicit (uint tag);
	public static bool op_Inequality (SKFourByteTag left, SKFourByteTag right);
}
```

#### New Type: SkiaSharp.SKPathBuilder

```csharp
public class SKPathBuilder : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKPathBuilder ();
	public SKPathBuilder (SKPath path);
	// properties
	public SKPathFillType FillType { get; set; }
	// methods
	public void AddArc (SKRect oval, float startAngle, float sweepAngle);
	public void AddCircle (float x, float y, float radius, SKPathDirection dir);
	public void AddOval (SKRect rect, SKPathDirection direction);
	public void AddPath (SKPath other, SKPathAddMode mode);
	public void AddPath (SKPath other, ref SKMatrix matrix, SKPathAddMode mode);
	public void AddPath (SKPath other, float dx, float dy, SKPathAddMode mode);
	public void AddPoly (SKPoint[] points, bool close);
	public void AddPoly (System.ReadOnlySpan<SKPoint> points, bool close);
	public void AddRect (SKRect rect, SKPathDirection direction);
	public void AddRect (SKRect rect, SKPathDirection direction, uint startIndex);
	public void AddRoundRect (SKRoundRect rect, SKPathDirection direction);
	public void AddRoundRect (SKRoundRect rect, SKPathDirection direction, uint startIndex);
	public void AddRoundRect (SKRect rect, float rx, float ry, SKPathDirection dir);
	public void ArcTo (SKPoint point1, SKPoint point2, float radius);
	public void ArcTo (SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo);
	public void ArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy);
	public void ArcTo (float x1, float y1, float x2, float y2, float radius);
	public void ArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);
	public void Close ();
	public void ConicTo (SKPoint point0, SKPoint point1, float w);
	public void ConicTo (float x0, float y0, float x1, float y1, float w);
	public void CubicTo (SKPoint point0, SKPoint point1, SKPoint point2);
	public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2);
	public SKPath Detach ();
	protected override void Dispose (bool disposing);
	protected override void DisposeNative ();
	public void LineTo (SKPoint point);
	public void LineTo (float x, float y);
	public void MoveTo (SKPoint point);
	public void MoveTo (float x, float y);
	public void QuadTo (SKPoint point0, SKPoint point1);
	public void QuadTo (float x0, float y0, float x1, float y1);
	public void RArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy);
	public void RArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);
	public void RConicTo (SKPoint point0, SKPoint point1, float w);
	public void RConicTo (float dx0, float dy0, float dx1, float dy1, float w);
	public void RCubicTo (SKPoint point0, SKPoint point1, SKPoint point2);
	public void RCubicTo (float dx0, float dy0, float dx1, float dy1, float dx2, float dy2);
	public void RLineTo (SKPoint point);
	public void RLineTo (float dx, float dy);
	public void RMoveTo (SKPoint point);
	public void RMoveTo (float dx, float dy);
	public void RQuadTo (SKPoint point0, SKPoint point1);
	public void RQuadTo (float dx0, float dy0, float dx1, float dy1);
	public void Reset ();
	public void ReverseAddPath (SKPath other);
	public SKPath Snapshot ();
}
```

#### New Type: SkiaSharp.SKWebpEncoder

```csharp
public static class SKWebpEncoder {
	// methods
	public static SKData Encode (SKPixmap src, SKWebpEncoderOptions options);
	public static bool Encode (SKWStream dst, SKPixmap src, SKWebpEncoderOptions options);
	public static bool Encode (System.IO.Stream dst, SKPixmap src, SKWebpEncoderOptions options);
	public static SKData EncodeAnimated (System.ReadOnlySpan<SKWebpEncoderFrame> frames, SKWebpEncoderOptions options);
	public static bool EncodeAnimated (SKWStream dst, System.ReadOnlySpan<SKWebpEncoderFrame> frames, SKWebpEncoderOptions options);
	public static bool EncodeAnimated (System.IO.Stream dst, System.ReadOnlySpan<SKWebpEncoderFrame> frames, SKWebpEncoderOptions options);
}
```

#### New Type: SkiaSharp.SKWebpEncoderFrame

```csharp
public struct SKWebpEncoderFrame {
	// constructors
	public SKWebpEncoderFrame (SKBitmap bitmap, System.TimeSpan duration);
	public SKWebpEncoderFrame (SKImage image, System.TimeSpan duration);
	public SKWebpEncoderFrame (SKPixmap pixmap, System.TimeSpan duration);
	// properties
	public System.TimeSpan Duration { get; set; }
	public SKPixmap Pixmap { get; set; }
}
```


