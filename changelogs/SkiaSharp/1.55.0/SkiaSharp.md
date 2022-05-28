# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.55.0.0 vs 1.54.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Obsoleted methods:

```diff
 [Obsolete ()]
 public void Flush (GRContextFlushBits flagsBitfield);
```

Modified methods:

```diff
 public void Flush (GRContextFlushBits flagsBitfield--- = 0---)
```

Added method:

```csharp
public void Flush ();
```


#### Type Changed: SkiaSharp.GRContextOptions

Added properties:

```csharp
public bool AllowPathMaskCaching { get; set; }
public static GRContextOptions Default { get; }
public bool DisableDistanceFieldPaths { get; set; }
public bool EnableInstancedRendering { get; set; }
public bool ForceSWPathMasks { get; set; }
```


#### Type Changed: SkiaSharp.SKCanvas

Added properties:

```csharp
public SKRect ClipBounds { get; }
public SKRectI ClipDeviceBounds { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public void ClipPath (SKPath path, SKRegionOperation operation, bool antialias);
 [Obsolete ()]
 public void ClipRect (SKRect rect, SKRegionOperation operation, bool antialias);
 [Obsolete ()]
 public void DrawColor (SKColor color, SKXferMode mode);
```

Modified methods:

```diff
 public void ClipPath (SKPath path, SKRegionOperation operation--- = 1---, bool antialias = false)
 public void ClipRect (SKRect rect, SKRegionOperation operation--- = 1---, bool antialias = false)
 public void DrawColor (SKColor color, SKXferMode mode--- = 1---)
```

Added methods:

```csharp
public void ClipPath (SKPath path, SKClipOperation operation, bool antialias);
public void ClipRect (SKRect rect, SKClipOperation operation, bool antialias);
public void ClipRegion (SKRegion region, SKClipOperation operation);
public void DrawBitmapLattice (SKBitmap bitmap, SKLattice lattice, SKRect dst, SKPaint paint);
public void DrawColor (SKColor color, SKBlendMode mode);
public void DrawImageLattice (SKImage image, SKLattice lattice, SKRect dst, SKPaint paint);
public void DrawOval (float cx, float cy, float rx, float ry, SKPaint paint);
public void Scale (float s);
```


#### Type Changed: SkiaSharp.SKCodec

Removed method:

```csharp
public void GetValidSubset (ref SKRectI desiredSubset);
```

Added method:

```csharp
public bool GetValidSubset (ref SKRectI desiredSubset);
```


#### Type Changed: SkiaSharp.SKCodecOptions

Added constructor:

```csharp
public SKCodecOptions (SKRectI subset);
```

Modified properties:

```diff
 public bool HasSubset { get; ---set;--- }
-public SKRectI Subset { get; set; }
+public SKRectI? Subset { get; set; }
```


#### Type Changed: SkiaSharp.SKColor

Added methods:

```csharp
public static SKColor Parse (string hexString);
public static bool TryParse (string hexString, out SKColor color);
```


#### Type Changed: SkiaSharp.SKColorFilter

Obsoleted fields:

```diff
 [Obsolete ()]
 public static const int MaxCubeSize;
 [Obsolete ()]
 public static const int MinCubeSize;
```

Added fields:

```csharp
public static const int ColorMatrixSize;
public static const int MaxColorCubeDimension;
public static const int MinColorCubeDimension;
```


#### Type Changed: SkiaSharp.SKDocument

Removed method:

```csharp
public bool Close ();
```

Added method:

```csharp
public void Close ();
```


#### Type Changed: SkiaSharp.SKFontStyleWidth

Obsoleted fields:

```diff
 [Obsolete ()]
 UltaExpanded = 9,
```

Added value:

```csharp
UltraExpanded = 9,
```


#### Type Changed: SkiaSharp.SKImageFilter

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKImageFilter CreateCompose (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);
```

Added methods:

```csharp
public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter background, SKImageFilter foreground, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDisplacementMapEffect (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateTile (SKRect src, SKRect dst, SKImageFilter input);
```


#### Type Changed: SkiaSharp.SKMatrix

Modified methods:

```diff
-static public void MapRect (ref SKMatrix matrix, out SKRect dest, ref SKRect source)
+ public void MapRect (ref SKMatrix matrix, out SKRect dest, ref SKRect source)
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public SKPoint MapXY (float x, float y);
```

Added methods:

```csharp
public static void Concat (ref SKMatrix target, SKMatrix first, SKMatrix second);
public SKPoint MapPoint (SKPoint point);
public SKPoint MapPoint (float x, float y);
public static void PostConcat (ref SKMatrix target, SKMatrix matrix);
public static void PreConcat (ref SKMatrix target, SKMatrix matrix);
```


#### Type Changed: SkiaSharp.SKPaint

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKXferMode XferMode { get; set; }
```

Added properties:

```csharp
public SKBlendMode BlendMode { get; set; }
public float FontSpacing { get; }
```

Added methods:

```csharp
public SKPaint Clone ();
public bool GetFillPath (SKPath src, SKPath dst, float resScale);
public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, float resScale);
public float GetFontMetrics (out SKFontMetrics metrics, float scale);
```


#### Type Changed: SkiaSharp.SKPath

Added methods:

```csharp
public void ArcTo (SKPoint point1, SKPoint point2, float radius);
public void ArcTo (SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo);
public void ArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy);
public void ArcTo (float x1, float y1, float x2, float y2, float radius);
public void ConicTo (SKPoint point0, SKPoint point1, float w);
public static SKPoint[] ConvertConicToQuads (SKPoint p0, SKPoint p1, SKPoint p2, float w, int pow2);
public static int ConvertConicToQuads (SKPoint p0, SKPoint p1, SKPoint p2, float w, SKPoint[] pts, int pow2);
public static int ConvertConicToQuads (SKPoint p0, SKPoint p1, SKPoint p2, float w, out SKPoint[] pts, int pow2);
public void CubicTo (SKPoint point0, SKPoint point1, SKPoint point2);
public void LineTo (SKPoint point);
public void MoveTo (SKPoint point);
public void Offset (SKPoint offset);
public SKPath Op (SKPath other, SKPathOp op);
public void QuadTo (SKPoint point0, SKPoint point1);
public void RArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy);
public void RConicTo (SKPoint point0, SKPoint point1, float w);
public void RCubicTo (SKPoint point0, SKPoint point1, SKPoint point2);
public void RLineTo (SKPoint point);
public void RMoveTo (SKPoint point);
public void RQuadTo (SKPoint point0, SKPoint point1);
public SKPath Simplify ();
```


#### Type Changed: SkiaSharp.SKPathEffect

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKPathEffect Create1DPath (SKPath path, float advance, float phase, SkPath1DPathEffectStyle style);
```

Added method:

```csharp
public static SKPathEffect Create1DPath (SKPath path, float advance, float phase, SKPath1DPathEffectStyle style);
```


#### Type Changed: SkiaSharp.SKPictureRecorder

Removed constructor:

```csharp
public SKPictureRecorder (IntPtr handle, bool owns);
```

Added property:

```csharp
public SKCanvas RecordingCanvas { get; }
```


#### Type Changed: SkiaSharp.SKPoint

Added methods:

```csharp
public void Offset (SKPoint p);
public void Offset (float dx, float dy);
```


#### Type Changed: SkiaSharp.SKRect

Added properties:

```csharp
public float MidX { get; }
public float MidY { get; }
public SKRect Standardized { get; }
```

Added methods:

```csharp
public SKRect AspectFill (SKSize size);
public SKRect AspectFit (SKSize size);
```


#### Type Changed: SkiaSharp.SKRectI

Added properties:

```csharp
public int MidX { get; }
public int MidY { get; }
public SKRectI Standardized { get; }
```

Added methods:

```csharp
public SKRectI AspectFill (SKSizeI size);
public SKRectI AspectFit (SKSizeI size);
```


#### Type Changed: SkiaSharp.SKTypeface

Added properties:

```csharp
public SKFontStyleSlant FontSlant { get; }
public int FontWeight { get; }
public int FontWidth { get; }
public SKTypefaceStyle Style { get; }
```


#### New Type: SkiaSharp.SKBlendMode

```csharp
[Serializable]
public enum SKBlendMode {
	Clear = 0,
	Color = 27,
	ColorBurn = 19,
	ColorDodge = 18,
	Darken = 16,
	Difference = 22,
	Dst = 2,
	DstATop = 10,
	DstIn = 6,
	DstOut = 8,
	DstOver = 4,
	Exclusion = 23,
	HardLight = 20,
	Hue = 25,
	Lighten = 17,
	Luminosity = 28,
	Modulate = 13,
	Multiply = 24,
	Overlay = 15,
	Plus = 12,
	Saturation = 26,
	Screen = 14,
	SoftLight = 21,
	Src = 1,
	SrcATop = 9,
	SrcIn = 5,
	SrcOut = 7,
	SrcOver = 3,
	Xor = 11,
}
```

#### New Type: SkiaSharp.SKClipOperation

```csharp
[Serializable]
public enum SKClipOperation {
	Difference = 0,
	Intersect = 1,
}
```

#### New Type: SkiaSharp.SKLattice

```csharp
public struct SKLattice {
	// properties
	public SKRectI? Bounds { get; set; }
	public SKLatticeFlags[] Flags { get; set; }
	public int[] XDivs { get; set; }
	public int[] YDivs { get; set; }
}
```

#### New Type: SkiaSharp.SKLatticeFlags

```csharp
[Serializable]
public enum SKLatticeFlags {
	Default = 0,
	Transparent = 1,
}
```

#### New Type: SkiaSharp.SKPath1DPathEffectStyle

```csharp
[Serializable]
public enum SKPath1DPathEffectStyle {
	Morph = 2,
	Rotate = 1,
	Translate = 0,
}
```

#### New Type: SkiaSharp.SKPathMeasure

```csharp
public class SKPathMeasure : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKPathMeasure ();
	public SKPathMeasure (SKPath path, bool forceClosed, float resScale);
	// properties
	public bool IsClosed { get; }
	public float Length { get; }
	// methods
	protected override void Dispose (bool disposing);
	public bool GetMatrix (float distance, out SKMatrix matrix, SKPathMeasure.MatrixFlags flags);
	public bool GetPosition (float distance, out SKPoint position);
	public bool GetPositionAndTangent (float distance, out SKPoint position, out SKPoint tangent);
	public bool GetSegment (float start, float stop, SKPath dst, bool startWithMoveTo);
	public bool GetTangent (float distance, out SKPoint tangent);
	public bool NextContour ();
	public void SetPath (SKPath path, bool forceClosed);

	// inner types
	[Serializable]
	[Flags]
	public enum MatrixFlags {
		GetPosition = 1,
		GetPositionAndTangent = 3,
		GetTangent = 2,
	}
}
```

#### New Type: SkiaSharp.SKRegion

```csharp
public class SKRegion : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKRegion ();
	public SKRegion (SKRegion region);
	// properties
	public SKRectI Bounds { get; }
	// methods
	public bool Contains (SKPointI xy);
	public bool Contains (SKRegion src);
	public bool Contains (int x, int y);
	public bool Intersects (SKRectI rect);
	public bool Intersects (SKRegion region);
	public bool Op (SKRectI rect, SKRegionOperation op);
	public bool Op (SKRegion region, SKRegionOperation op);
	public bool Op (int left, int top, int right, int bottom, SKRegionOperation op);
	public bool SetPath (SKPath path);
	public bool SetPath (SKPath path, SKRegion clip);
	public bool SetRect (SKRectI rect);
	public bool SetRegion (SKRegion region);
}
```


