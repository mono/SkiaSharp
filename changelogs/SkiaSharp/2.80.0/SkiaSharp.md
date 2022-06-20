# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 2.80.0.0 vs 1.68.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRBackend

Added value:

```csharp
Dawn = 3,
```


#### Type Changed: SkiaSharp.GRBackendRenderTarget

Added constructor:

```csharp
public GRBackendRenderTarget (int width, int height, int sampleCount, GRVkImageInfo vkImageInfo);
```


#### Type Changed: SkiaSharp.GRBackendTexture

Added constructor:

```csharp
public GRBackendTexture (int width, int height, GRVkImageInfo vkInfo);
```


#### Type Changed: SkiaSharp.GRContext

Obsoleted methods:

```diff
 [Obsolete ()]
 public static GRContext Create (GRBackend backend);
 [Obsolete ()]
 public static GRContext Create (GRBackend backend, GRGlInterface backendContext);
 [Obsolete ()]
 public void GetResourceCacheLimits (out int maxResources, out long maxResourceBytes);
 [Obsolete ()]
 public void SetResourceCacheLimits (int maxResources, long maxResourceBytes);
```

Added methods:

```csharp
public static GRContext CreateVulkan (GRVkBackendContext backendContext);
public long GetResourceCacheLimit ();
public void SetResourceCacheLimit (long maxResourceBytes);
```


#### Type Changed: SkiaSharp.GRGlInterface

Obsoleted methods:

```diff
 [Obsolete ()]
 public static GRGlInterface AssembleAngleInterface (GRGlGetProcDelegate get);
 [Obsolete ()]
 public static GRGlInterface AssembleAngleInterface (object context, GRGlGetProcDelegate get);
 [Obsolete ()]
 public static GRGlInterface AssembleGlInterface (GRGlGetProcDelegate get);
 [Obsolete ()]
 public static GRGlInterface AssembleGlInterface (object context, GRGlGetProcDelegate get);
 [Obsolete ()]
 public static GRGlInterface AssembleGlesInterface (GRGlGetProcDelegate get);
 [Obsolete ()]
 public static GRGlInterface AssembleGlesInterface (object context, GRGlGetProcDelegate get);
 [Obsolete ()]
 public static GRGlInterface AssembleInterface (GRGlGetProcDelegate get);
 [Obsolete ()]
 public static GRGlInterface AssembleInterface (object context, GRGlGetProcDelegate get);
 [Obsolete ()]
 public static GRGlInterface CreateDefaultInterface ();
 [Obsolete ()]
 public static GRGlInterface CreateNativeAngleInterface ();
 [Obsolete ()]
 public static GRGlInterface CreateNativeEvasInterface (IntPtr evas);
 [Obsolete ()]
 public static GRGlInterface CreateNativeGlInterface ();
```

Added methods:

```csharp
public static GRGlInterface Create ();
public static GRGlInterface Create (GRGlGetProcedureAddressDelegate get);
public static GRGlInterface CreateAngle (GRGlGetProcedureAddressDelegate get);
public static GRGlInterface CreateEvas (IntPtr evas);
public static GRGlInterface CreateGles (GRGlGetProcedureAddressDelegate get);
public static GRGlInterface CreateOpenGl (GRGlGetProcedureAddressDelegate get);
public static GRGlInterface CreateWebGl (GRGlGetProcedureAddressDelegate get);
```


#### Type Changed: SkiaSharp.GRPixelConfig

Obsoleted fields:

```diff
 [Obsolete ()]
 RgFloat = 12,
 [Obsolete ()]
 RgbaFloat = 11,
 [Obsolete ()]
 Sbgra8888 = 9,
```

Added values:

```csharp
Alpha16 = 22,
Alpha8AsAlpha = 15,
Alpha8AsRed = 16,
AlphaHalfAsLum = 17,
AlphaHalfAsRed = 18,
Gray8AsLum = 19,
Gray8AsRed = 20,
Rg1616 = 23,
Rg88 = 26,
RgHalf = 25,
Rgb888x = 27,
RgbEtc1 = 28,
Rgba16161616 = 24,
RgbaHalfClamped = 21,
```


#### Type Changed: SkiaSharp.SKBitmap

Added constructor:

```csharp
public SKBitmap (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace);
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public IntPtr GetAddr (int x, int y);
 [Obsolete ()]
 public ushort GetAddr16 (int x, int y);
 [Obsolete ()]
 public uint GetAddr32 (int x, int y);
 [Obsolete ()]
 public byte GetAddr8 (int x, int y);
```

Added method:

```csharp
public IntPtr GetAddress (int x, int y);
```


#### Type Changed: SkiaSharp.SKCanvas

Obsoleted methods:

```diff
 [Obsolete ()]
 public void DrawPositionedText (byte[] text, SKPoint[] points, SKPaint paint);
 [Obsolete ()]
 public void DrawPositionedText (string text, SKPoint[] points, SKPaint paint);
 [Obsolete ()]
 public void DrawPositionedText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint);
 [Obsolete ()]
 public void DrawText (byte[] text, SKPoint p, SKPaint paint);
 [Obsolete ()]
 public void DrawText (byte[] text, float x, float y, SKPaint paint);
 [Obsolete ()]
 public void DrawText (IntPtr buffer, int length, SKPoint p, SKPaint paint);
 [Obsolete ()]
 public void DrawText (IntPtr buffer, int length, float x, float y, SKPaint paint);
 [Obsolete ()]
 public void DrawTextOnPath (byte[] text, SKPath path, SKPoint offset, SKPaint paint);
 [Obsolete ()]
 public void DrawTextOnPath (byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint);
 [Obsolete ()]
 public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, SKPoint offset, SKPaint paint);
 [Obsolete ()]
 public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint);
```

Added methods:

```csharp
public void DrawText (string text, float x, float y, SKFont font, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, SKPoint offset, bool warpGlyphs, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, SKPoint offset, bool warpGlyphs, SKFont font, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKCodecOptions

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKTransferFunctionBehavior PremulBehavior { get; set; }
```


#### Type Changed: SkiaSharp.SKColorSpace

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKNamedGamma NamedGamma { get; }
 [Obsolete ()]
 public SKColorSpaceType Type { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut);
 [Obsolete ()]
 public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50);
 [Obsolete ()]
 public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut);
 [Obsolete ()]
 public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50);
 [Obsolete ()]
 public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKColorSpaceGamut gamut);
 [Obsolete ()]
 public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKMatrix44 toXyzD50);
 [Obsolete ()]
 public SKMatrix44 FromXyzD50 ();
 [Obsolete ()]
 public SKMatrix44 ToXyzD50 ();
 [Obsolete ()]
 public bool ToXyzD50 (SKMatrix44 toXyzD50);
```

Added methods:

```csharp
public static SKColorSpace CreateIcc (SKColorSpaceIccProfile profile);
public static SKColorSpace CreateIcc (SKData input);
public static SKColorSpace CreateIcc (System.ReadOnlySpan<byte> input);
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn transferFn, SKColorSpaceXyz toXyzD50);
public SKColorSpaceTransferFn GetNumericalTransferFunction ();
public SKColorSpaceXyz ToColorSpaceXyz ();
public bool ToColorSpaceXyz (out SKColorSpaceXyz toXyzD50);
public SKColorSpace ToLinearGamma ();
public SKColorSpaceIccProfile ToProfile ();
public SKColorSpace ToSrgbGamma ();
```


#### Type Changed: SkiaSharp.SKColorSpacePrimaries

Obsoleted methods:

```diff
 [Obsolete ()]
 public SKMatrix44 ToXyzD50 ();
 [Obsolete ()]
 public bool ToXyzD50 (SKMatrix44 toXyzD50);
```

Added methods:

```csharp
public SKColorSpaceXyz ToColorSpaceXyz ();
public bool ToColorSpaceXyz (out SKColorSpaceXyz toXyzD50);
```


#### Type Changed: SkiaSharp.SKColorSpaceTransferFn

Added properties:

```csharp
public static SKColorSpaceTransferFn Hlg { get; }
public static SKColorSpaceTransferFn Linear { get; }
public static SKColorSpaceTransferFn Pq { get; }
public static SKColorSpaceTransferFn Rec2020 { get; }
public static SKColorSpaceTransferFn Srgb { get; }
public static SKColorSpaceTransferFn TwoDotTwo { get; }
```


#### Type Changed: SkiaSharp.SKColorType

Added values:

```csharp
Alpha16 = 16,
AlphaF16 = 14,
Rg1616 = 17,
Rg88 = 13,
RgF16 = 15,
Rgba16161616 = 18,
RgbaF16Clamped = 11,
RgbaF32 = 12,
```


#### Type Changed: SkiaSharp.SKImage

Obsoleted methods:

```diff
 [Obsolete ()]
 public SKImage ToTextureImage (GRContext context, SKColorSpace colorspace);
```

Added method:

```csharp
public SKImage ToTextureImage (GRContext context, bool mipmapped);
```


#### Type Changed: SkiaSharp.SKImageFilter

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKImageFilter CreateDisplacementMapEffect (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);
 [Obsolete ()]
 public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input, SKImageFilter.CropRect cropRect);
 [Obsolete ()]
 public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);
```

Added methods:

```csharp
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);
```


#### Type Changed: SkiaSharp.SKJpegEncoderOptions

Obsoleted constructors:

```diff
 [Obsolete ()]
 public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption, SKTransferFunctionBehavior blendBehavior);
```

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKTransferFunctionBehavior BlendBehavior { get; set; }
```


#### Type Changed: SkiaSharp.SKMaskFilter

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder);
 [Obsolete ()]
 public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM);
```

Added method:

```csharp
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, bool respectCTM);
```


#### Type Changed: SkiaSharp.SKMaskFormat

Added value:

```csharp
Sdf = 5,
```


#### Type Changed: SkiaSharp.SKMatrix

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKMatrix MakeIdentity ();
 [Obsolete ()]
 public static SKMatrix MakeRotation (float radians);
 [Obsolete ()]
 public static SKMatrix MakeRotation (float radians, float pivotx, float pivoty);
 [Obsolete ()]
 public static SKMatrix MakeRotationDegrees (float degrees);
 [Obsolete ()]
 public static SKMatrix MakeRotationDegrees (float degrees, float pivotx, float pivoty);
 [Obsolete ()]
 public static SKMatrix MakeScale (float sx, float sy);
 [Obsolete ()]
 public static SKMatrix MakeScale (float sx, float sy, float pivotX, float pivotY);
 [Obsolete ()]
 public static SKMatrix MakeSkew (float sx, float sy);
 [Obsolete ()]
 public static SKMatrix MakeTranslation (float dx, float dy);
```


#### Type Changed: SkiaSharp.SKNativeObject

Added method:

```csharp
protected virtual void DisposeUnownedManaged ();
```


#### Type Changed: SkiaSharp.SKObject

Added method:

```csharp
protected override void DisposeUnownedManaged ();
```


#### Type Changed: SkiaSharp.SKPaint

Added constructor:

```csharp
public SKPaint (SKFont font);
```

Obsoleted properties:

```diff
 [Obsolete ()]
 public bool DeviceKerningEnabled { get; set; }
 [Obsolete ()]
 public bool IsVerticalText { get; set; }
```

Added property:

```csharp
public SKColorF ColorF { get; set; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public float GetFontMetrics (out SKFontMetrics metrics, float scale);
```

Modified methods:

```diff
 public float GetFontMetrics (out SKFontMetrics metrics, float scale--- = 0---)
```

Added methods:

```csharp
public long BreakText (System.ReadOnlySpan<byte> text, float maxWidth);
public long BreakText (System.ReadOnlySpan<char> text, float maxWidth);
public long BreakText (System.ReadOnlySpan<byte> text, float maxWidth, out float measuredWidth);
public long BreakText (System.ReadOnlySpan<char> text, float maxWidth, out float measuredWidth);
public bool ContainsGlyphs (System.ReadOnlySpan<byte> text);
public bool ContainsGlyphs (System.ReadOnlySpan<char> text);
public int CountGlyphs (System.ReadOnlySpan<byte> text);
public int CountGlyphs (System.ReadOnlySpan<char> text);
public float GetFontMetrics (out SKFontMetrics metrics);
public float[] GetGlyphOffsets (System.ReadOnlySpan<byte> text, float origin);
public float[] GetGlyphOffsets (System.ReadOnlySpan<char> text, float origin);
public float[] GetGlyphOffsets (string text, float origin);
public float[] GetGlyphOffsets (IntPtr text, int length, float origin);
public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<byte> text, SKPoint origin);
public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<char> text, SKPoint origin);
public SKPoint[] GetGlyphPositions (string text, SKPoint origin);
public SKPoint[] GetGlyphPositions (IntPtr text, int length, SKPoint origin);
public float[] GetGlyphWidths (System.ReadOnlySpan<byte> text);
public float[] GetGlyphWidths (System.ReadOnlySpan<char> text);
public float[] GetGlyphWidths (System.ReadOnlySpan<byte> text, out SKRect[] bounds);
public float[] GetGlyphWidths (System.ReadOnlySpan<char> text, out SKRect[] bounds);
public ushort[] GetGlyphs (System.ReadOnlySpan<byte> text);
public ushort[] GetGlyphs (System.ReadOnlySpan<char> text);
public float[] GetHorizontalTextIntercepts (System.ReadOnlySpan<byte> text, System.ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds);
public float[] GetHorizontalTextIntercepts (System.ReadOnlySpan<char> text, System.ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds);
public float[] GetPositionedTextIntercepts (System.ReadOnlySpan<byte> text, System.ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds);
public float[] GetPositionedTextIntercepts (System.ReadOnlySpan<char> text, System.ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds);
public float[] GetTextIntercepts (System.ReadOnlySpan<byte> text, float x, float y, float upperBounds, float lowerBounds);
public float[] GetTextIntercepts (System.ReadOnlySpan<char> text, float x, float y, float upperBounds, float lowerBounds);
public SKPath GetTextPath (System.ReadOnlySpan<byte> text, System.ReadOnlySpan<SKPoint> points);
public SKPath GetTextPath (System.ReadOnlySpan<char> text, System.ReadOnlySpan<SKPoint> points);
public SKPath GetTextPath (IntPtr buffer, int length, System.ReadOnlySpan<SKPoint> points);
public SKPath GetTextPath (System.ReadOnlySpan<byte> text, float x, float y);
public SKPath GetTextPath (System.ReadOnlySpan<char> text, float x, float y);
public float MeasureText (System.ReadOnlySpan<byte> text);
public float MeasureText (System.ReadOnlySpan<char> text);
public float MeasureText (System.ReadOnlySpan<byte> text, ref SKRect bounds);
public float MeasureText (System.ReadOnlySpan<char> text, ref SKRect bounds);
public void SetColor (SKColorF color, SKColorSpace colorspace);
public SKFont ToFont ();
```


#### Type Changed: SkiaSharp.SKPath

Added methods:

```csharp
public SKPath ToWinding ();
public bool ToWinding (SKPath result);
```

#### Type Changed: SkiaSharp.SKPath.Iterator

Obsoleted methods:

```diff
 [Obsolete ()]
 public SKPathVerb Next (SKPoint[] points, bool doConsumeDegenerates, bool exact);
```

Modified methods:

```diff
 public SKPathVerb Next (SKPoint[] points, bool doConsumeDegenerates--- = true---, bool exact--- = false---)
```

Added methods:

```csharp
public SKPathVerb Next (SKPoint[] points);
public SKPathVerb Next (System.Span<SKPoint> points);
```


#### Type Changed: SkiaSharp.SKPath.RawIterator

Added method:

```csharp
public SKPathVerb Next (System.Span<SKPoint> points);
```



#### Type Changed: SkiaSharp.SKPixmap

Obsoleted methods:

```diff
 [Obsolete ()]
 public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKTransferFunctionBehavior behavior);
```


#### Type Changed: SkiaSharp.SKPngEncoderOptions

Obsoleted constructors:

```diff
 [Obsolete ()]
 public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel, SKTransferFunctionBehavior unpremulBehavior);
```

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKTransferFunctionBehavior UnpremulBehavior { get; set; }
```


#### Type Changed: SkiaSharp.SKRunBuffer

Obsoleted properties:

```diff
 [Obsolete ()]
 public int TextSize { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public System.Span<uint> GetClusterSpan ();
 [Obsolete ()]
 public System.Span<byte> GetTextSpan ();
 [Obsolete ()]
 public void SetClusters (System.ReadOnlySpan<uint> clusters);
 [Obsolete ()]
 public void SetText (System.ReadOnlySpan<byte> text);
```


#### Type Changed: SkiaSharp.SKShader

Added method:

```csharp
public static SKShader CreateLerp (float weight, SKShader dst, SKShader src);
```


#### Type Changed: SkiaSharp.SKShaderTileMode

Added value:

```csharp
Decal = 3,
```


#### Type Changed: SkiaSharp.SKSurface

Added method:

```csharp
public SKImage Snapshot (SKRectI bounds);
```


#### Type Changed: SkiaSharp.SKTextBlob

Added methods:

```csharp
public int CountIntercepts (float upperBounds, float lowerBounds, SKPaint paint);
public static SKTextBlob Create (System.ReadOnlySpan<char> text, SKFont font, SKPoint origin);
public static SKTextBlob Create (string text, SKFont font, SKPoint origin);
public static SKTextBlob Create (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, SKPoint origin);
public static SKTextBlob Create (IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPoint origin);
public static SKTextBlob CreateHorizontal (System.ReadOnlySpan<char> text, SKFont font, System.ReadOnlySpan<float> positions, float y);
public static SKTextBlob CreateHorizontal (string text, SKFont font, System.ReadOnlySpan<float> positions, float y);
public static SKTextBlob CreateHorizontal (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, System.ReadOnlySpan<float> positions, float y);
public static SKTextBlob CreateHorizontal (IntPtr text, int length, SKTextEncoding encoding, SKFont font, System.ReadOnlySpan<float> positions, float y);
public static SKTextBlob CreatePathPositioned (System.ReadOnlySpan<char> text, SKFont font, SKPath path, SKTextAlign textAlign, SKPoint origin);
public static SKTextBlob CreatePathPositioned (string text, SKFont font, SKPath path, SKTextAlign textAlign, SKPoint origin);
public static SKTextBlob CreatePathPositioned (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign, SKPoint origin);
public static SKTextBlob CreatePathPositioned (IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign, SKPoint origin);
public static SKTextBlob CreatePositioned (System.ReadOnlySpan<char> text, SKFont font, System.ReadOnlySpan<SKPoint> positions);
public static SKTextBlob CreatePositioned (string text, SKFont font, System.ReadOnlySpan<SKPoint> positions);
public static SKTextBlob CreatePositioned (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, System.ReadOnlySpan<SKPoint> positions);
public static SKTextBlob CreatePositioned (IntPtr text, int length, SKTextEncoding encoding, SKFont font, System.ReadOnlySpan<SKPoint> positions);
public static SKTextBlob CreateRotationScale (System.ReadOnlySpan<char> text, SKFont font, System.ReadOnlySpan<SKRotationScaleMatrix> positions);
public static SKTextBlob CreateRotationScale (string text, SKFont font, System.ReadOnlySpan<SKRotationScaleMatrix> positions);
public static SKTextBlob CreateRotationScale (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, System.ReadOnlySpan<SKRotationScaleMatrix> positions);
public static SKTextBlob CreateRotationScale (IntPtr text, int length, SKTextEncoding encoding, SKFont font, System.ReadOnlySpan<SKRotationScaleMatrix> positions);
public float[] GetIntercepts (float upperBounds, float lowerBounds, SKPaint paint);
public void GetIntercepts (float upperBounds, float lowerBounds, System.Span<float> intervals, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKTextBlobBuilder

Obsoleted methods:

```diff
 [Obsolete ()]
 public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions);
 [Obsolete ()]
 public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions);
 [Obsolete ()]
 public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, SKRect? bounds);
 [Obsolete ()]
 public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, SKRect bounds);
 [Obsolete ()]
 public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);
 [Obsolete ()]
 public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters);
 [Obsolete ()]
 public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters);
 [Obsolete ()]
 public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);
 [Obsolete ()]
 public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters, SKRect bounds);
 [Obsolete ()]
 public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters, SKRect bounds);
 [Obsolete ()]
 public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions);
 [Obsolete ()]
 public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions);
 [Obsolete ()]
 public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, SKRect? bounds);
 [Obsolete ()]
 public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, SKRect bounds);
 [Obsolete ()]
 public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);
 [Obsolete ()]
 public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters);
 [Obsolete ()]
 public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters);
 [Obsolete ()]
 public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);
 [Obsolete ()]
 public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters, SKRect bounds);
 [Obsolete ()]
 public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters, SKRect bounds);
 [Obsolete ()]
 public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs);
 [Obsolete ()]
 public void AddRun (SKPaint font, float x, float y, ushort[] glyphs);
 [Obsolete ()]
 public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, SKRect? bounds);
 [Obsolete ()]
 public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, SKRect bounds);
 [Obsolete ()]
 public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);
 [Obsolete ()]
 public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters);
 [Obsolete ()]
 public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters);
 [Obsolete ()]
 public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);
 [Obsolete ()]
 public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters, SKRect bounds);
 [Obsolete ()]
 public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters, SKRect bounds);
 [Obsolete ()]
 public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y);
 [Obsolete ()]
 public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount);
 [Obsolete ()]
 public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, SKRect? bounds);
 [Obsolete ()]
 public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount, SKRect? bounds);
 [Obsolete ()]
 public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count);
 [Obsolete ()]
 public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount);
 [Obsolete ()]
 public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, SKRect? bounds);
 [Obsolete ()]
 public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount, SKRect? bounds);
 [Obsolete ()]
 public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y);
 [Obsolete ()]
 public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount);
 [Obsolete ()]
 public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, SKRect? bounds);
 [Obsolete ()]
 public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount, SKRect? bounds);
```

Added methods:

```csharp
public void AddHorizontalRun (System.ReadOnlySpan<ushort> glyphs, SKFont font, System.ReadOnlySpan<float> positions, float y);
public void AddPathPositionedRun (System.ReadOnlySpan<ushort> glyphs, SKFont font, System.ReadOnlySpan<float> glyphWidths, System.ReadOnlySpan<SKPoint> glyphOffsets, SKPath path, SKTextAlign textAlign);
public void AddPositionedRun (System.ReadOnlySpan<ushort> glyphs, SKFont font, System.ReadOnlySpan<SKPoint> positions);
public void AddRotationScaleRun (System.ReadOnlySpan<ushort> glyphs, SKFont font, System.ReadOnlySpan<SKRotationScaleMatrix> positions);
public void AddRun (System.ReadOnlySpan<ushort> glyphs, SKFont font, SKPoint origin);
public SKHorizontalRunBuffer AllocateHorizontalRun (SKFont font, int count, float y, SKRect? bounds);
public SKPositionedRunBuffer AllocatePositionedRun (SKFont font, int count, SKRect? bounds);
public SKRotationScaleRunBuffer AllocateRotationScaleRun (SKFont font, int count);
public SKRunBuffer AllocateRun (SKFont font, int count, float x, float y, SKRect? bounds);
```


#### Type Changed: SkiaSharp.SKTypeface

Added methods:

```csharp
public bool ContainsGlyph (int codepoint);
public bool ContainsGlyphs (System.ReadOnlySpan<int> codepoints);
public ushort GetGlyph (int codepoint);
public ushort[] GetGlyphs (System.ReadOnlySpan<int> codepoints);
public SKFont ToFont ();
public SKFont ToFont (float size, float scaleX, float skewX);
```


#### Type Changed: SkiaSharp.SKWebpEncoderOptions

Obsoleted constructors:

```diff
 [Obsolete ()]
 public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality, SKTransferFunctionBehavior unpremulBehavior);
```

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKTransferFunctionBehavior UnpremulBehavior { get; set; }
```


#### Type Changed: SkiaSharp.SkiaExtensions

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKColorType ToColorType (this GRPixelConfig config);
 [Obsolete ()]
 public static uint ToGlSizedFormat (this GRPixelConfig config);
 [Obsolete ()]
 public static GRPixelConfig ToPixelConfig (this SKColorType colorType);
 [Obsolete ()]
 public static SKTextEncoding ToTextEncoding (this SKEncoding encoding);
```

Added methods:

```csharp

[Obsolete]
public static SKColorChannel ToColorChannel (this SKDisplacementMapEffectChannelSelectorType channelSelectorType);

[Obsolete]
public static SKColorSpaceTransferFn ToColorSpaceTransferFn (this SKColorSpaceRenderTargetGamma gamma);

[Obsolete]
public static SKColorSpaceTransferFn ToColorSpaceTransferFn (this SKNamedGamma gamma);

[Obsolete]
public static SKColorSpaceXyz ToColorSpaceXyz (this SKColorSpaceGamut gamut);

[Obsolete]
public static SKColorSpaceXyz ToColorSpaceXyz (this SKMatrix44 matrix);

[Obsolete]
public static SKShaderTileMode ToShaderTileMode (this SKMatrixConvolutionTileMode tileMode);
```


#### New Type: SkiaSharp.GRGlGetProcedureAddressDelegate

```csharp
public sealed delegate GRGlGetProcedureAddressDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GRGlGetProcedureAddressDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (string name, System.AsyncCallback callback, object object);
	public virtual IntPtr EndInvoke (System.IAsyncResult result);
	public virtual IntPtr Invoke (string name);
}
```

#### New Type: SkiaSharp.GRVkAlloc

```csharp
public struct GRVkAlloc, System.IEquatable<GRVkAlloc> {
	// properties
	public IntPtr BackendMemory { get; set; }
	public uint Flags { get; set; }
	public ulong Memory { get; set; }
	public ulong Offset { get; set; }
	public ulong Size { get; set; }
	// methods
	public virtual bool Equals (GRVkAlloc obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (GRVkAlloc left, GRVkAlloc right);
	public static bool op_Inequality (GRVkAlloc left, GRVkAlloc right);
}
```

#### New Type: SkiaSharp.GRVkBackendContext

```csharp
public class GRVkBackendContext : System.IDisposable {
	// constructors
	public GRVkBackendContext ();
	// properties
	public GRVkExtensions Extensions { get; set; }
	public GRVkGetProcedureAddressDelegate GetProcedureAddress { get; set; }
	public uint GraphicsQueueIndex { get; set; }
	public uint MaxAPIVersion { get; set; }
	public bool ProtectedContext { get; set; }
	public IntPtr VkDevice { get; set; }
	public IntPtr VkInstance { get; set; }
	public IntPtr VkPhysicalDevice { get; set; }
	public IntPtr VkPhysicalDeviceFeatures { get; set; }
	public IntPtr VkPhysicalDeviceFeatures2 { get; set; }
	public IntPtr VkQueue { get; set; }
	// methods
	public virtual void Dispose ();
	protected virtual void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.GRVkExtensions

```csharp
public class GRVkExtensions : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public static GRVkExtensions Create (GRVkGetProcedureAddressDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice, string[] instanceExtensions, string[] deviceExtensions);
	protected override void DisposeNative ();
	public void HasExtension (string extension, int minVersion);
	public void Initialize (GRVkGetProcedureAddressDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice);
	public void Initialize (GRVkGetProcedureAddressDelegate getProc, IntPtr vkInstance, IntPtr vkPhysicalDevice, string[] instanceExtensions, string[] deviceExtensions);
}
```

#### New Type: SkiaSharp.GRVkGetProcedureAddressDelegate

```csharp
public sealed delegate GRVkGetProcedureAddressDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public GRVkGetProcedureAddressDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (string name, IntPtr instance, IntPtr device, System.AsyncCallback callback, object object);
	public virtual IntPtr EndInvoke (System.IAsyncResult result);
	public virtual IntPtr Invoke (string name, IntPtr instance, IntPtr device);
}
```

#### New Type: SkiaSharp.GRVkImageInfo

```csharp
public struct GRVkImageInfo, System.IEquatable<GRVkImageInfo> {
	// properties
	public GRVkAlloc Alloc { get; set; }
	public uint CurrentQueueFamily { get; set; }
	public uint Format { get; set; }
	public ulong Image { get; set; }
	public uint ImageLayout { get; set; }
	public uint ImageTiling { get; set; }
	public uint LevelCount { get; set; }
	public bool Protected { get; set; }
	public GrVkYcbcrConversionInfo YcbcrConversionInfo { get; set; }
	// methods
	public virtual bool Equals (GRVkImageInfo obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (GRVkImageInfo left, GRVkImageInfo right);
	public static bool op_Inequality (GRVkImageInfo left, GRVkImageInfo right);
}
```

#### New Type: SkiaSharp.GrVkYcbcrConversionInfo

```csharp
public struct GrVkYcbcrConversionInfo, System.IEquatable<GrVkYcbcrConversionInfo> {
	// properties
	public uint ChromaFilter { get; set; }
	public ulong ExternalFormat { get; set; }
	public uint ForceExplicitReconstruction { get; set; }
	public uint Format { get; set; }
	public uint FormatFeatures { get; set; }
	public uint XChromaOffset { get; set; }
	public uint YChromaOffset { get; set; }
	public uint YcbcrModel { get; set; }
	public uint YcbcrRange { get; set; }
	// methods
	public virtual bool Equals (GrVkYcbcrConversionInfo obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (GrVkYcbcrConversionInfo left, GrVkYcbcrConversionInfo right);
	public static bool op_Inequality (GrVkYcbcrConversionInfo left, GrVkYcbcrConversionInfo right);
}
```

#### New Type: SkiaSharp.SKColorChannel

```csharp
[Serializable]
public enum SKColorChannel {
	A = 3,
	B = 2,
	G = 1,
	R = 0,
}
```

#### New Type: SkiaSharp.SKColorSpaceIccProfile

```csharp
public class SKColorSpaceIccProfile : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKColorSpaceIccProfile ();
	// properties
	public IntPtr Buffer { get; }
	public long Size { get; }
	// methods
	public static SKColorSpaceIccProfile Create (SKData data);
	public static SKColorSpaceIccProfile Create (byte[] data);
	public static SKColorSpaceIccProfile Create (System.ReadOnlySpan<byte> data);
	public static SKColorSpaceIccProfile Create (IntPtr data, long length);
	protected override void DisposeNative ();
	public SKColorSpaceXyz ToColorSpaceXyz ();
	public bool ToColorSpaceXyz (out SKColorSpaceXyz toXyzD50);
}
```

#### New Type: SkiaSharp.SKColorSpaceXyz

```csharp
public struct SKColorSpaceXyz, System.IEquatable<SKColorSpaceXyz> {
	// constructors
	public SKColorSpaceXyz (float value);
	public SKColorSpaceXyz (float[] values);
	public SKColorSpaceXyz (float m00, float m01, float m02, float m10, float m11, float m12, float m20, float m21, float m22);
	// fields
	public static SKColorSpaceXyz Empty;
	// properties
	public static SKColorSpaceXyz AdobeRgb { get; }
	public static SKColorSpaceXyz Dcip3 { get; }
	public float Item { get; }
	public static SKColorSpaceXyz Rec2020 { get; }
	public static SKColorSpaceXyz Srgb { get; }
	public float[] Values { get; set; }
	public static SKColorSpaceXyz Xyz { get; }
	// methods
	public static SKColorSpaceXyz Concat (SKColorSpaceXyz a, SKColorSpaceXyz b);
	public virtual bool Equals (SKColorSpaceXyz obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public SKColorSpaceXyz Invert ();
	public static bool op_Equality (SKColorSpaceXyz left, SKColorSpaceXyz right);
	public static bool op_Inequality (SKColorSpaceXyz left, SKColorSpaceXyz right);
}
```

#### New Type: SkiaSharp.SKFont

```csharp
public class SKFont : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKFont ();
	public SKFont (SKTypeface typeface, float size, float scaleX, float skewX);
	// properties
	public bool BaselineSnap { get; set; }
	public SKFontEdging Edging { get; set; }
	public bool EmbeddedBitmaps { get; set; }
	public bool Embolden { get; set; }
	public bool ForceAutoHinting { get; set; }
	public SKFontHinting Hinting { get; set; }
	public bool LinearMetrics { get; set; }
	public SKFontMetrics Metrics { get; }
	public float ScaleX { get; set; }
	public float Size { get; set; }
	public float SkewX { get; set; }
	public float Spacing { get; }
	public bool Subpixel { get; set; }
	public SKTypeface Typeface { get; set; }
	// methods
	public bool ContainsGlyph (int codepoint);
	public bool ContainsGlyphs (System.ReadOnlySpan<char> text);
	public bool ContainsGlyphs (System.ReadOnlySpan<int> codepoints);
	public bool ContainsGlyphs (string text);
	public bool ContainsGlyphs (System.ReadOnlySpan<byte> text, SKTextEncoding encoding);
	public bool ContainsGlyphs (IntPtr text, int length, SKTextEncoding encoding);
	public int CountGlyphs (System.ReadOnlySpan<char> text);
	public int CountGlyphs (string text);
	public int CountGlyphs (System.ReadOnlySpan<byte> text, SKTextEncoding encoding);
	public int CountGlyphs (IntPtr text, int length, SKTextEncoding encoding);
	protected override void DisposeNative ();
	public float GetFontMetrics (out SKFontMetrics metrics);
	public ushort GetGlyph (int codepoint);
	public void GetGlyphOffsets (System.ReadOnlySpan<ushort> glyphs, System.Span<float> offsets, float origin);
	public SKPath GetGlyphPath (ushort glyph);
	public void GetGlyphPaths (System.ReadOnlySpan<ushort> glyphs, SKGlyphPathDelegate glyphPathDelegate);
	public void GetGlyphPositions (System.ReadOnlySpan<ushort> glyphs, System.Span<SKPoint> positions, SKPoint origin);
	public void GetGlyphWidths (System.ReadOnlySpan<ushort> glyphs, System.Span<float> widths, System.Span<SKRect> bounds, SKPaint paint);
	public void GetGlyphs (System.ReadOnlySpan<char> text, System.Span<ushort> glyphs);
	public void GetGlyphs (System.ReadOnlySpan<int> codepoints, System.Span<ushort> glyphs);
	public void GetGlyphs (string text, System.Span<ushort> glyphs);
	public void GetGlyphs (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, System.Span<ushort> glyphs);
	public void GetGlyphs (IntPtr text, int length, SKTextEncoding encoding, System.Span<ushort> glyphs);
	public float MeasureText (System.ReadOnlySpan<ushort> glyphs, SKPaint paint);
	public float MeasureText (System.ReadOnlySpan<ushort> glyphs, out SKRect bounds, SKPaint paint);
}
```

#### New Type: SkiaSharp.SKFontEdging

```csharp
[Serializable]
public enum SKFontEdging {
	Alias = 0,
	Antialias = 1,
	SubpixelAntialias = 2,
}
```

#### New Type: SkiaSharp.SKFontHinting

```csharp
[Serializable]
public enum SKFontHinting {
	Full = 3,
	None = 0,
	Normal = 2,
	Slight = 1,
}
```

#### New Type: SkiaSharp.SKGlyphPathDelegate

```csharp
public sealed delegate SKGlyphPathDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public SKGlyphPathDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (SKPath path, SKMatrix matrix, System.AsyncCallback callback, object object);
	public virtual void EndInvoke (System.IAsyncResult result);
	public virtual void Invoke (SKPath path, SKMatrix matrix);
}
```

#### New Type: SkiaSharp.SKRotationScaleRunBuffer

```csharp
public sealed class SKRotationScaleRunBuffer : SkiaSharp.SKRunBuffer {
	// methods
	public System.Span<SKRotationScaleMatrix> GetRotationScaleSpan ();
	public void SetRotationScale (System.ReadOnlySpan<SKRotationScaleMatrix> positions);
}
```

#### New Type: SkiaSharp.SkiaSharpVersion

```csharp
public static class SkiaSharpVersion {
	// properties
	public static System.Version Native { get; }
	public static System.Version NativeMinimum { get; }
	// methods
	public static bool CheckNativeLibraryCompatible (bool throwIfIncompatible);
}
```


