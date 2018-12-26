# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKColorSpace

Added properties:

```csharp
public bool IsNumericalTransferFunction { get; }
public SKNamedGamma NamedGamma { get; }
public SKColorSpaceType Type { get; }
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


#### Type Changed: SkiaSharp.SKPaint

Added methods:

```csharp
public long BreakText (IntPtr buffer, int length, float maxWidth);
public long BreakText (IntPtr buffer, int length, float maxWidth, out float measuredWidth);
public bool ContainsGlyphs (IntPtr text, IntPtr length);
public int CountGlyphs (IntPtr text, IntPtr length);
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


