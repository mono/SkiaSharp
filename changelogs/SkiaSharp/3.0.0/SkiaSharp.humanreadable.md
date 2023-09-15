# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 3.0.0.0 vs 2.88.0.0

### Major Changes

The diff below contains all the changes that are in addition to the removal of obsolete types and members. The 3.x release is a major upgrade and many of the obsolete types and members needed to go away.

#### Platform Reduction

SkiaSharp supports many platforms, however in 3.x we reduce the platforms to just the more modern ones:

* .NET Standard 2.0+
* .NET Framework 4.6.2+
* .NET 7+ (All the platforms: Android, iOS, Mac Catalyst, macOS, Tizen, tvOS, Windows)

#### Improvements

There are some small improvements in the initial release of 3.x, and many more will be added with later builds.

* Improved interopability with `System.Numerics`:
  * `SKMatrix44` is now implicitly compatible with `System.Numerics.Matrix4x4` in both directions.
  * `SKPoint` is now implicitly compatible with `System.Numerics.Vector2` in both directions.
  * `SKPoint3` is now implicitly compatible with `System.Numerics.Vector3` in both directions.
  * `SKPointI` is now implicitly cast to `System.Numerics.Vector3`.
* `SKRuntimeEffect` now works on both CPU and GPU:
  * GPU is accelerated and support more targets: `SKColorFilter` and `SKShader` (there is also a new `SKBlender` that is not yet exposed in SkiaSharp).
  * CPU is NOT accelerated and may be very slow.
* `SKMatrix44` is now a high-performance struct that can be used on any `SKCanvas`.

#### Breaking Changes

With the major update from 2x to 3x, some APIs were broken to make maintainance easier as well as to simplify things for consumers.

Below is a list of notable breaking changes.

* `SKImageFilter.CropRect` was replaced with `SKRect`  
  Many of the `SKImageFilter.Create*` members accepted a `SKImageFilter.CropRect` which was a type of `SKRect` wrapper. This is now replaced with the simpler and easier-to-use `SKRect`.
* `SKMatrix44` was entirely rewritten and is no longer a `class`  
  Previously, `SKMatrix44` was a `class` that was used for several some cases, however, it is now the main underlying matrix type for most transforms. Because it has to pass across the native interop layer a lot as well as the usage increasing, it is now a `struct` to reduce the load on the GC. The new API is similar to the old one, but is really a totally different type with a different purpose. It is complatible and interchangeable with the `System.Numerics.Matrix4x4` type - and even have implicit cast operators to aid the usability.
* `SKRuntimeEffect` was rewritten and now supports more destination types (instead of just `SKShader`)  
  Along with this new feature set, the creation of effects requires a different create methods. The is also a new builder pattern that can be used to simplify construction.
* `SK3dView` was removed because it was expensive to use  
  The new `SKMatrix44` can do all the same things as well as just using `System.Numerics.Matrix4x4` and related `System.Numerics` types.

##### Removed [Obsolete] Types and Members

Many types and members were obsoleted at trhe start of the 2.x version (and some before). 
The 3.x release will be removing all the members that were previously marked `[Obsolete]`.

Some of the notable removals are:

* Some `GRGlInterface` builders - All overloads that accepted `GRGlGetProcDelegate` and the other long-winded names.  
  These methods were long and have much shorted and more accurately named `Create<backend>` variants. Some that were removed are: `GRGlInterface.Assemble*`, `GRGlInterface.CreateDefault*`, and `GRGlInterface.CreateNative*`.
* `SKBitmap.GetAddr*` - All the `GetAddr*` members on `SKBitmap`.  
  The same values can be obtained from `GetPixelSpan`.
* `SKBitmapResizeMethod` - `SKBitmapResizeMethod` and all usages are removed.
  This type has been obsolete for some time. There is now a brand new and far more flexible `SKSamplingOptions` which can produce the same result.
* `SKColorTable` - All types and members relating to `SKColorTable`.  
  Support was removed in 1.68, but the types and members remained and just mapped to the non-`SKColorTable` types and members.
* `SKMask` - All types and members relating to `SKMask` have been removed.
* `SKXmlWriter` - All types and members relating to `SKXmlWriter` and `SKXmlStreamWriter` have been removed.

#### Obsoleted Types and Members

With the major update from 2x to 3x, several APIs are no longer the recommeneded way to do something. There might be a better or cleaner way of doing something. For all of these types and members, they will be marked `[Obsolete]` and removed in the next major release.

Some of the notable obsolete items are:

* `SKFilterQuality` - All usages of the `SKFilterQuality` enum are now obsolete.  
  There is now a brand new and far more flexible `SKSamplingOptions` which can produce the same result.
* `SKFont` & `SKPaint` - All the "font-related" members on `SKPaint` have been marked obsolete and now exist on `SKFont`.  
  In previous skia versions, the `SKPaint` functionality was split into 2 objects: `SKPaint` and `SKFont`. SkiaSharp tried to maintain 100% backwards compatibility by re-merginf the types. However, this is getting hard to maintain. As a result, `SKFont` is now the correct replacement to work with typefaces and character styles. All APIs tha accepted just a `SKPaint` now also have an overload that accepts `SKFont` and `SKTextAlign`.

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRGlFramebufferInfo

Added property:

```csharp
public bool Protected { get; set; }
```

#### Type Changed: SkiaSharp.GRGlTextureInfo

Added property:

```csharp
public bool Protected { get; set; }
```


#### Type Changed: SkiaSharp.GRRecordingContext

Added properties:

```csharp
public virtual bool IsAbandoned { get; }
public int MaxRenderTargetSize { get; }
public int MaxTextureSize { get; }
```


#### Type Changed: SkiaSharp.SKBitmap

Removed methods:

```csharp
public System.ReadOnlySpan<byte> GetPixelSpan ();
public bool InstallMaskPixels (SKMask mask);
```

Obsoleted methods:

```cs
[Obsolete]
public SKBitmap Resize (SKImageInfo info, SKFilterQuality quality);
[Obsolete]
public SKBitmap Resize (SKSizeI size, SKFilterQuality quality);
[Obsolete]
public bool ScalePixels (SKBitmap destination, SKFilterQuality quality);
[Obsolete]
public bool ScalePixels (SKPixmap destination, SKFilterQuality quality);
```

Added methods:

```csharp
public System.Span<byte> GetPixelSpan ();
public System.Span<byte> GetPixelSpan (int x, int y);
public SKBitmap Resize (SKImageInfo info, SKSamplingOptions sampling);
public SKBitmap Resize (SKSizeI size, SKSamplingOptions sampling);
public bool ScalePixels (SKBitmap destination, SKSamplingOptions sampling);
public bool ScalePixels (SKPixmap destination, SKSamplingOptions sampling);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling, SKMatrix localMatrix);
```


#### Type Changed: SkiaSharp.SKCanvas

Added property:

```csharp
public SKMatrix44 TotalMatrix44 { get; }
```

Obsoleted methods:

```cs
[Obsolete]
public void DrawText (string text, SKPoint p, SKPaint paint);
[Obsolete]
public void DrawText (string text, float x, float y, SKPaint paint);
[Obsolete]
public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKPaint paint);
[Obsolete]
public void DrawTextOnPath (string text, SKPath path, SKPoint offset, bool warpGlyphs, SKPaint paint);
[Obsolete]
public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKPaint paint);
```

Added methods:

```csharp
public void Concat (ref SKMatrix m);
public void Concat (ref SKMatrix44 m);
public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKSamplingOptions sampling, SKPaint paint);
public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKSamplingOptions sampling, SKPaint paint);
public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKSamplingOptions sampling, SKRect cullRect, SKPaint paint);
public void DrawBitmapLattice (SKBitmap bitmap, SKLattice lattice, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawBitmapLattice (SKBitmap bitmap, int[] xDivs, int[] yDivs, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawBitmapNinePatch (SKBitmap bitmap, SKRectI center, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawDrawable (SKDrawable drawable, ref SKMatrix matrix);
public void DrawImage (SKImage image, SKPoint p, SKSamplingOptions sampling, SKPaint paint);
public void DrawImage (SKImage image, SKRect dest, SKSamplingOptions sampling, SKPaint paint);
public void DrawImage (SKImage image, SKRect source, SKRect dest, SKSamplingOptions sampling, SKPaint paint);
public void DrawImage (SKImage image, float x, float y, SKSamplingOptions sampling, SKPaint paint);
public void DrawImageLattice (SKImage image, SKLattice lattice, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawImageLattice (SKImage image, int[] xDivs, int[] yDivs, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawImageNinePatch (SKImage image, SKRectI center, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawPicture (SKPicture picture, ref SKMatrix matrix, SKPaint paint);
public void DrawText (string text, SKPoint p, SKFont font, SKPaint paint);
public void DrawText (string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint);
public void DrawText (string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKFont font, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKTextAlign textAlign, SKFont font, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKFont font, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, SKPoint offset, bool warpGlyphs, SKTextAlign textAlign, SKFont font, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKTextAlign textAlign, SKFont font, SKPaint paint);
public void SetMatrix (ref SKMatrix matrix);
public void SetMatrix (ref SKMatrix44 matrix);
```


#### Type Changed: SkiaSharp.SKCodecFrameInfo

Added properties:

```csharp
public SKCodecAnimationBlend Blend { get; set; }
public SKRectI FrameRect { get; set; }
public bool HasAlphaWithinBounds { get; set; }
```


#### Type Changed: SkiaSharp.SKColorSpaceXyz

Added field:

```csharp
public static SKColorSpaceXyz Identity;
```


#### Type Changed: SkiaSharp.SKColorType

Added values:

```csharp
Bgr101010xXR = 21,
R8Unorm = 23,
Srgba8888 = 22,
```


#### Type Changed: SkiaSharp.SKDrawable

Added methods:

```csharp
protected virtual int OnGetApproximateBytesUsed ();
```


#### Type Changed: SkiaSharp.SKEncodedImageFormat

Added value:

```csharp
Jpegxl = 13,
```


#### Type Changed: SkiaSharp.SKFont

Added methods:

```csharp
public int BreakText (System.ReadOnlySpan<char> text, float maxWidth, SKPaint paint);
public int BreakText (string text, float maxWidth, SKPaint paint);
public int BreakText (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, float maxWidth, SKPaint paint);
public int BreakText (System.ReadOnlySpan<char> text, float maxWidth, out float measuredWidth, SKPaint paint);
public int BreakText (string text, float maxWidth, out float measuredWidth, SKPaint paint);
public int BreakText (IntPtr text, int length, SKTextEncoding encoding, float maxWidth, SKPaint paint);
public int BreakText (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, float maxWidth, out float measuredWidth, SKPaint paint);
public int BreakText (IntPtr text, int length, SKTextEncoding encoding, float maxWidth, out float measuredWidth, SKPaint paint);
public float[] GetGlyphOffsets (System.ReadOnlySpan<char> text, float origin);
public float[] GetGlyphOffsets (System.ReadOnlySpan<ushort> glyphs, float origin);
public float[] GetGlyphOffsets (string text, float origin);
public float[] GetGlyphOffsets (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, float origin);
public void GetGlyphOffsets (System.ReadOnlySpan<char> text, System.Span<float> offsets, float origin);
public void GetGlyphOffsets (string text, System.Span<float> offsets, float origin);
public float[] GetGlyphOffsets (IntPtr text, int length, SKTextEncoding encoding, float origin);
public void GetGlyphOffsets (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, System.Span<float> offsets, float origin);
public void GetGlyphOffsets (IntPtr text, int length, SKTextEncoding encoding, System.Span<float> offsets, float origin);
public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<char> text, SKPoint origin);
public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<ushort> glyphs, SKPoint origin);
public SKPoint[] GetGlyphPositions (string text, SKPoint origin);
public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPoint origin);
public void GetGlyphPositions (System.ReadOnlySpan<char> text, System.Span<SKPoint> offsets, SKPoint origin);
public void GetGlyphPositions (string text, System.Span<SKPoint> offsets, SKPoint origin);
public SKPoint[] GetGlyphPositions (IntPtr text, int length, SKTextEncoding encoding, SKPoint origin);
public void GetGlyphPositions (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, System.Span<SKPoint> offsets, SKPoint origin);
public void GetGlyphPositions (IntPtr text, int length, SKTextEncoding encoding, System.Span<SKPoint> offsets, SKPoint origin);
public float[] GetGlyphWidths (System.ReadOnlySpan<char> text, SKPaint paint);
public float[] GetGlyphWidths (System.ReadOnlySpan<ushort> glyphs, SKPaint paint);
public float[] GetGlyphWidths (string text, SKPaint paint);
public float[] GetGlyphWidths (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPaint paint);
public float[] GetGlyphWidths (System.ReadOnlySpan<char> text, out SKRect[] bounds, SKPaint paint);
public float[] GetGlyphWidths (System.ReadOnlySpan<ushort> glyphs, out SKRect[] bounds, SKPaint paint);
public float[] GetGlyphWidths (string text, out SKRect[] bounds, SKPaint paint);
public float[] GetGlyphWidths (IntPtr text, int length, SKTextEncoding encoding, SKPaint paint);
public float[] GetGlyphWidths (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, out SKRect[] bounds, SKPaint paint);
public void GetGlyphWidths (System.ReadOnlySpan<char> text, System.Span<float> widths, System.Span<SKRect> bounds, SKPaint paint);
public void GetGlyphWidths (string text, System.Span<float> widths, System.Span<SKRect> bounds, SKPaint paint);
public float[] GetGlyphWidths (IntPtr text, int length, SKTextEncoding encoding, out SKRect[] bounds, SKPaint paint);
public void GetGlyphWidths (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, System.Span<float> widths, System.Span<SKRect> bounds, SKPaint paint);
public void GetGlyphWidths (IntPtr text, int length, SKTextEncoding encoding, System.Span<float> widths, System.Span<SKRect> bounds, SKPaint paint);
public ushort[] GetGlyphs (System.ReadOnlySpan<char> text);
public ushort[] GetGlyphs (System.ReadOnlySpan<int> codepoints);
public ushort[] GetGlyphs (string text);
public ushort[] GetGlyphs (System.ReadOnlySpan<byte> text, SKTextEncoding encoding);
public ushort[] GetGlyphs (IntPtr text, int length, SKTextEncoding encoding);
public SKPath GetTextPath (System.ReadOnlySpan<char> text, SKPoint origin);
public SKPath GetTextPath (System.ReadOnlySpan<char> text, System.ReadOnlySpan<SKPoint> positions);
public SKPath GetTextPath (string text, SKPoint origin);
public SKPath GetTextPath (string text, System.ReadOnlySpan<SKPoint> positions);
public SKPath GetTextPath (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPoint origin);
public SKPath GetTextPath (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, System.ReadOnlySpan<SKPoint> positions);
public SKPath GetTextPath (IntPtr text, int length, SKTextEncoding encoding, SKPoint origin);
public SKPath GetTextPath (IntPtr text, int length, SKTextEncoding encoding, System.ReadOnlySpan<SKPoint> positions);
public SKPath GetTextPathOnPath (System.ReadOnlySpan<char> text, SKPath path, SKTextAlign textAlign, SKPoint origin);
public SKPath GetTextPathOnPath (System.ReadOnlySpan<ushort> glyphs, SKPath path, SKTextAlign textAlign, SKPoint origin);
public SKPath GetTextPathOnPath (string text, SKPath path, SKTextAlign textAlign, SKPoint origin);
public SKPath GetTextPathOnPath (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPath path, SKTextAlign textAlign, SKPoint origin);
public SKPath GetTextPathOnPath (System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> glyphWidths, System.ReadOnlySpan<SKPoint> glyphPositions, SKPath path, SKTextAlign textAlign);
public SKPath GetTextPathOnPath (IntPtr text, int length, SKTextEncoding encoding, SKPath path, SKTextAlign textAlign, SKPoint origin);
public float MeasureText (System.ReadOnlySpan<char> text, SKPaint paint);
public float MeasureText (string text, SKPaint paint);
public float MeasureText (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPaint paint);
public float MeasureText (System.ReadOnlySpan<char> text, out SKRect bounds, SKPaint paint);
public float MeasureText (string text, out SKRect bounds, SKPaint paint);
public float MeasureText (IntPtr text, int length, SKTextEncoding encoding, SKPaint paint);
public float MeasureText (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, out SKRect bounds, SKPaint paint);
public float MeasureText (IntPtr text, int length, SKTextEncoding encoding, out SKRect bounds, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKFontManager

Removed method:

```csharp
public SKTypeface MatchTypeface (SKTypeface face, SKFontStyle style);
```


#### Type Changed: SkiaSharp.SKGraphics

Removed methods:

```csharp
public static int GetFontCachePointSizeLimit ();
public static int SetFontCachePointSizeLimit (int count);
```


#### Type Changed: SkiaSharp.SKImage

Obsoleted methods:

```cs
[Obsolete]
public bool ScalePixels (SKPixmap dst, SKFilterQuality quality);
[Obsolete]
public bool ScalePixels (SKPixmap dst, SKFilterQuality quality, SKImageCachingHint cachingHint);
```

Added methods:

```csharp
public bool ScalePixels (SKPixmap dst, SKSamplingOptions sampling);
public bool ScalePixels (SKPixmap dst, SKSamplingOptions sampling, SKImageCachingHint cachingHint);
public SKImage Subset (GRRecordingContext context, SKRectI subset);
public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling);
public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix localMatrix);
public SKImage ToTextureImage (GRContext context, bool mipmapped, bool budgeted);
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed methods:

```csharp
public static SKImageFilter CreateAlphaThreshold (SKRectI region, float innerThreshold, float outerThreshold, SKImageFilter input);
public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter background, SKImageFilter foreground, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDilate (int radiusX, int radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateErode (int radiusX, int radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateImage (SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality);
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMatrix (SKMatrix matrix, SKFilterQuality quality, SKImageFilter input);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateOffset (float dx, float dy, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePaint (SKPaint paint, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
```

Added methods:

```csharp
public static SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold);
public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background);
public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground);
public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground, SKRect cropRect);
public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter background);
public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter background, SKImageFilter foreground);
public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter background, SKImageFilter foreground, SKRect cropRect);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter input);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter input);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateColorFilter (SKColorFilter cf);
public static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter input);
public static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateDilate (float radiusX, float radiusY);
public static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter input);
public static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement);
public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input);
public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd);
public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input);
public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess);
public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input);
public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color);
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input);
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color);
public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input);
public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateErode (float radiusX, float radiusY);
public static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter input);
public static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateImage (SKImage image, SKSamplingOptions sampling);
public static SKImageFilter CreateImage (SKImage image, SKRect src, SKRect dst, SKSamplingOptions sampling);
public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling);
public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter input);
public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateMatrix (ref SKMatrix matrix);
public static SKImageFilter CreateMatrix (ref SKMatrix matrix, SKSamplingOptions sampling);
public static SKImageFilter CreateMatrix (ref SKMatrix matrix, SKSamplingOptions sampling, SKImageFilter input);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, System.ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, System.ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, System.ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateMerge (System.ReadOnlySpan<SKImageFilter> filters);
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second);
public static SKImageFilter CreateMerge (System.ReadOnlySpan<SKImageFilter> filters, SKRect cropRect);
public static SKImageFilter CreateMerge (System.ReadOnlySpan<SKImageFilter> filters, SKRect* cropRect);
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKRect cropRect);
public static SKImageFilter CreateOffset (float radiusX, float radiusY);
public static SKImageFilter CreateOffset (float radiusX, float radiusY, SKImageFilter input);
public static SKImageFilter CreateOffset (float radiusX, float radiusY, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd);
public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input);
public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess);
public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input);
public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateShader (SKShader shader);
public static SKImageFilter CreateShader (SKShader shader, bool dither);
public static SKImageFilter CreateShader (SKShader shader, bool dither, SKRect cropRect);
public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd);
public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input);
public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess);
public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input);
public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateTile (SKRect src, SKRect dst);
```

#### Type Changed: SkiaSharp.SKImageInfo

Added property:

```csharp
public int BitShiftPerPixel { get; }
```


#### Type Changed: SkiaSharp.SKJpegEncoderOptions

Added constructor:

```csharp
public SKJpegEncoderOptions (int quality);
```

Modified properties:

```diff
-public SKJpegEncoderAlphaOption AlphaOption { get; set; }
+public SKJpegEncoderAlphaOption AlphaOption { get;  }
-public SKJpegEncoderDownsample Downsample { get; set; }
+public SKJpegEncoderDownsample Downsample { get; }
-public int Quality { get; set; }
+public int Quality { get; }
```


#### Type Changed: SkiaSharp.SKMatrix

Removed methods:

```csharp
public static void Concat (ref SKMatrix target, ref SKMatrix first, ref SKMatrix second);
```


#### Type Changed: SkiaSharp.SKMatrix44

Modified base type:

```diff
-SkiaSharp.SKObject
+System.ValueType
```

Removed constructor:

```csharp
public SKMatrix44 (SKMatrix44 a, SKMatrix44 b);
```

Added constructor:

```csharp
public SKMatrix44 (float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33);
```

Removed interface:

```csharp
System.IDisposable
```

Added interface:

```csharp
System.IEquatable<SKMatrix44>
```

Added fields:

```csharp
public static SKMatrix44 Empty;
public static SKMatrix44 Identity;
```

Removed property:

```csharp
public SKMatrix44TypeMask Type { get; }
```

Added properties:

```csharp
public float M00 { get; set; }
public float M01 { get; set; }
public float M02 { get; set; }
public float M03 { get; set; }
public float M10 { get; set; }
public float M11 { get; set; }
public float M12 { get; set; }
public float M13 { get; set; }
public float M20 { get; set; }
public float M21 { get; set; }
public float M22 { get; set; }
public float M23 { get; set; }
public float M30 { get; set; }
public float M31 { get; set; }
public float M32 { get; set; }
public float M33 { get; set; }
```

Removed methods:

```csharp
public static SKMatrix44 CreateTranslate (float x, float y, float z);
public double Determinant ();
protected override void Dispose (bool disposing);
protected override void DisposeNative ();
public static bool Equal (SKMatrix44 left, SKMatrix44 right);
public static SKMatrix44 FromColumnMajor (float[] src);
public static SKMatrix44 FromRowMajor (float[] src);
public bool Invert (SKMatrix44 inverse);
public SKPoint[] MapPoints (SKPoint[] src);
public float[] MapScalars (float[] srcVector4);
public void MapScalars (float[] srcVector4, float[] dstVector4);
public float[] MapScalars (float x, float y, float z, float w);
public float[] MapVector2 (float[] src2);
public void MapVector2 (float[] src2, float[] dst4);
public void PostConcat (SKMatrix44 m);
public void PostScale (float sx, float sy, float sz);
public void PostTranslate (float dx, float dy, float dz);
public void PreConcat (SKMatrix44 m);
public void PreScale (float sx, float sy, float sz);
public void PreTranslate (float dx, float dy, float dz);
public bool Preserves2DAxisAlignment (float epsilon);
public void Set3x3ColumnMajor (float[] src);
public void Set3x3RowMajor (float[] src);
public void SetColumnMajor (float[] src);
public void SetConcat (SKMatrix44 a, SKMatrix44 b);
public void SetIdentity ();
public void SetRotationAbout (float x, float y, float z, float radians);
public void SetRotationAboutDegrees (float x, float y, float z, float degrees);
public void SetRotationAboutUnit (float x, float y, float z, float radians);
public void SetRowMajor (float[] src);
public void SetScale (float sx, float sy, float sz);
public void SetTranslate (float dx, float dy, float dz);
public void ToColumnMajor (float[] dst);
public void ToRowMajor (float[] dst);
public void Transpose ();
```

Added methods:

```csharp
public static SKMatrix44 Add (SKMatrix44 value1, SKMatrix44 value2);
public static SKMatrix44 Concat (SKMatrix44 first, SKMatrix44 second);
public static void Concat (ref SKMatrix44 target, SKMatrix44 first, SKMatrix44 second);
public static SKMatrix44 CreateScale (float x, float y, float z, float pivotX, float pivotY, float pivotZ);
public float Determinant ();
public virtual bool Equals (SKMatrix44 obj);
public override bool Equals (object obj);
public static SKMatrix44 FromColumnMajor (System.ReadOnlySpan<float> src);
public static SKMatrix44 FromRowMajor (System.ReadOnlySpan<float> src);
public override int GetHashCode ();
public SKPoint3 MapPoint (SKPoint3 point);
public SKPoint MapPoint (float x, float y);
public SKPoint3 MapPoint (float x, float y, float z);
public static SKMatrix44 Multiply (SKMatrix44 value1, SKMatrix44 value2);
public static SKMatrix44 Multiply (SKMatrix44 value1, float value2);
public static SKMatrix44 Negate (SKMatrix44 value);
public SKMatrix44 PostConcat (SKMatrix44 matrix);
public SKMatrix44 PreConcat (SKMatrix44 matrix);
public static SKMatrix44 Subtract (SKMatrix44 value1, SKMatrix44 value2);
public void ToColumnMajor (System.Span<float> dst);
public void ToRowMajor (System.Span<float> dst);
public SKMatrix44 Transpose ();
public bool TryInvert (out SKMatrix44 inverse);
public static SKMatrix44 op_Addition (SKMatrix44 value1, SKMatrix44 value2);
public static bool op_Equality (SKMatrix44 left, SKMatrix44 right);
public static System.Numerics.Matrix4x4 op_Implicit (SKMatrix44 matrix);
public static SKMatrix44 op_Implicit (System.Numerics.Matrix4x4 matrix);
public static bool op_Inequality (SKMatrix44 left, SKMatrix44 right);
public static SKMatrix44 op_Multiply (SKMatrix44 value1, SKMatrix44 value2);
public static SKMatrix44 op_Multiply (SKMatrix44 value1, float value2);
public static SKMatrix44 op_Subtraction (SKMatrix44 value1, SKMatrix44 value2);
public static SKMatrix44 op_UnaryNegation (SKMatrix44 value);
```


#### Type Changed: SkiaSharp.SKPaint

Obsoleted properties:

```cs
[Obsolete]
public bool FakeBoldText { get; set; }
[Obsolete]
public SKFilterQuality FilterQuality { get; set; }
[Obsolete]
public SKFontMetrics FontMetrics { get; }
[Obsolete]
public float FontSpacing { get; }
[Obsolete]
public SKPaintHinting HintingLevel { get; set; }
[Obsolete]
public bool IsAutohinted { get; set; }
[Obsolete]
public bool IsEmbeddedBitmapText { get; set; }
[Obsolete]
public bool IsLinearText { get; set; }
[Obsolete]
public bool LcdRenderText { get; set; }
[Obsolete]
public bool SubpixelText { get; set; }
[Obsolete]
public SKTextAlign TextAlign { get; set; }
[Obsolete]
public SKTextEncoding TextEncoding { get; set; }
[Obsolete]
public float TextScaleX { get; set; }
[Obsolete]
public float TextSize { get; set; }
[Obsolete]
public float TextSkewX { get; set; }
[Obsolete]
public SKTypeface Typeface { get; set; }
```

Removed method:

```csharp
[Obsolete]
public float GetFontMetrics (out SKFontMetrics metrics, float scale);
```

Obsoleted methods:

```cs
[Obsolete]
public long BreakText (byte[] text, float maxWidth);
[Obsolete]
public long BreakText (System.ReadOnlySpan<byte> text, float maxWidth);
[Obsolete]
public long BreakText (System.ReadOnlySpan<char> text, float maxWidth);
[Obsolete]
public long BreakText (string text, float maxWidth);
[Obsolete]
public long BreakText (byte[] text, float maxWidth, out float measuredWidth);
[Obsolete]
public long BreakText (IntPtr buffer, int length, float maxWidth);
[Obsolete]
public long BreakText (IntPtr buffer, IntPtr length, float maxWidth);
[Obsolete]
public long BreakText (System.ReadOnlySpan<byte> text, float maxWidth, out float measuredWidth);
[Obsolete]
public long BreakText (System.ReadOnlySpan<char> text, float maxWidth, out float measuredWidth);
[Obsolete]
public long BreakText (string text, float maxWidth, out float measuredWidth);
[Obsolete]
public long BreakText (IntPtr buffer, int length, float maxWidth, out float measuredWidth);
[Obsolete]
public long BreakText (IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth);
[Obsolete]
public long BreakText (string text, float maxWidth, out float measuredWidth, out string measuredText);
[Obsolete]
public bool ContainsGlyphs (byte[] text);
[Obsolete]
public bool ContainsGlyphs (System.ReadOnlySpan<byte> text);
[Obsolete]
public bool ContainsGlyphs (System.ReadOnlySpan<char> text);
[Obsolete]
public bool ContainsGlyphs (string text);
[Obsolete]
public bool ContainsGlyphs (IntPtr text, int length);
[Obsolete]
public bool ContainsGlyphs (IntPtr text, IntPtr length);
[Obsolete]
public int CountGlyphs (byte[] text);
[Obsolete]
public int CountGlyphs (System.ReadOnlySpan<byte> text);
[Obsolete]
public int CountGlyphs (System.ReadOnlySpan<char> text);
[Obsolete]
public int CountGlyphs (string text);
[Obsolete]
public int CountGlyphs (IntPtr text, int length);
[Obsolete]
public int CountGlyphs (IntPtr text, IntPtr length);
[Obsolete]
public float GetFontMetrics (out SKFontMetrics metrics);
[Obsolete]
public float[] GetGlyphOffsets (System.ReadOnlySpan<byte> text, float origin);
[Obsolete]
public float[] GetGlyphOffsets (System.ReadOnlySpan<char> text, float origin);
[Obsolete]
public float[] GetGlyphOffsets (string text, float origin);
[Obsolete]
public float[] GetGlyphOffsets (IntPtr text, int length, float origin);
[Obsolete]
public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<byte> text, SKPoint origin);
[Obsolete]
public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<char> text, SKPoint origin);
[Obsolete]
public SKPoint[] GetGlyphPositions (string text, SKPoint origin);
[Obsolete]
public SKPoint[] GetGlyphPositions (IntPtr text, int length, SKPoint origin);
[Obsolete]
public float[] GetGlyphWidths (byte[] text);
[Obsolete]
public float[] GetGlyphWidths (System.ReadOnlySpan<byte> text);
[Obsolete]
public float[] GetGlyphWidths (System.ReadOnlySpan<char> text);
[Obsolete]
public float[] GetGlyphWidths (string text);
[Obsolete]
public float[] GetGlyphWidths (byte[] text, out SKRect[] bounds);
[Obsolete]
public float[] GetGlyphWidths (IntPtr text, int length);
[Obsolete]
public float[] GetGlyphWidths (IntPtr text, IntPtr length);
[Obsolete]
public float[] GetGlyphWidths (System.ReadOnlySpan<byte> text, out SKRect[] bounds);
[Obsolete]
public float[] GetGlyphWidths (System.ReadOnlySpan<char> text, out SKRect[] bounds);
[Obsolete]
public float[] GetGlyphWidths (string text, out SKRect[] bounds);
[Obsolete]
public float[] GetGlyphWidths (IntPtr text, int length, out SKRect[] bounds);
[Obsolete]
public float[] GetGlyphWidths (IntPtr text, IntPtr length, out SKRect[] bounds);
[Obsolete]
public ushort[] GetGlyphs (byte[] text);
[Obsolete]
public ushort[] GetGlyphs (System.ReadOnlySpan<byte> text);
[Obsolete]
public ushort[] GetGlyphs (System.ReadOnlySpan<char> text);
[Obsolete]
public ushort[] GetGlyphs (string text);
[Obsolete]
public ushort[] GetGlyphs (IntPtr text, int length);
[Obsolete]
public ushort[] GetGlyphs (IntPtr text, IntPtr length);
[Obsolete]
public float[] GetHorizontalTextIntercepts (byte[] text, float[] xpositions, float y, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetHorizontalTextIntercepts (System.ReadOnlySpan<byte> text, System.ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetHorizontalTextIntercepts (System.ReadOnlySpan<char> text, System.ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetHorizontalTextIntercepts (string text, float[] xpositions, float y, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetHorizontalTextIntercepts (IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetHorizontalTextIntercepts (IntPtr text, IntPtr length, float[] xpositions, float y, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetPositionedTextIntercepts (byte[] text, SKPoint[] positions, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetPositionedTextIntercepts (System.ReadOnlySpan<byte> text, System.ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetPositionedTextIntercepts (System.ReadOnlySpan<char> text, System.ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetPositionedTextIntercepts (string text, SKPoint[] positions, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetPositionedTextIntercepts (IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetPositionedTextIntercepts (IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetTextIntercepts (SKTextBlob text, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetTextIntercepts (byte[] text, float x, float y, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetTextIntercepts (System.ReadOnlySpan<byte> text, float x, float y, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetTextIntercepts (System.ReadOnlySpan<char> text, float x, float y, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetTextIntercepts (string text, float x, float y, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetTextIntercepts (IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds);
[Obsolete]
public float[] GetTextIntercepts (IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds);
[Obsolete]
public SKPath GetTextPath (byte[] text, SKPoint[] points);
[Obsolete]
public SKPath GetTextPath (System.ReadOnlySpan<byte> text, System.ReadOnlySpan<SKPoint> points);
[Obsolete]
public SKPath GetTextPath (System.ReadOnlySpan<char> text, System.ReadOnlySpan<SKPoint> points);
[Obsolete]
public SKPath GetTextPath (string text, SKPoint[] points);
[Obsolete]
public SKPath GetTextPath (byte[] text, float x, float y);
[Obsolete]
public SKPath GetTextPath (IntPtr buffer, int length, SKPoint[] points);
[Obsolete]
public SKPath GetTextPath (IntPtr buffer, int length, System.ReadOnlySpan<SKPoint> points);
[Obsolete]
public SKPath GetTextPath (IntPtr buffer, IntPtr length, SKPoint[] points);
[Obsolete]
public SKPath GetTextPath (System.ReadOnlySpan<byte> text, float x, float y);
[Obsolete]
public SKPath GetTextPath (System.ReadOnlySpan<char> text, float x, float y);
[Obsolete]
public SKPath GetTextPath (string text, float x, float y);
[Obsolete]
public SKPath GetTextPath (IntPtr buffer, int length, float x, float y);
[Obsolete]
public SKPath GetTextPath (IntPtr buffer, IntPtr length, float x, float y);
[Obsolete]
public float MeasureText (byte[] text);
[Obsolete]
public float MeasureText (System.ReadOnlySpan<byte> text);
[Obsolete]
public float MeasureText (System.ReadOnlySpan<char> text);
[Obsolete]
public float MeasureText (string text);
[Obsolete]
public float MeasureText (byte[] text, ref SKRect bounds);
[Obsolete]
public float MeasureText (IntPtr buffer, int length);
[Obsolete]
public float MeasureText (IntPtr buffer, IntPtr length);
[Obsolete]
public float MeasureText (System.ReadOnlySpan<byte> text, ref SKRect bounds);
[Obsolete]
public float MeasureText (System.ReadOnlySpan<char> text, ref SKRect bounds);
[Obsolete]
public float MeasureText (string text, ref SKRect bounds);
[Obsolete]
public float MeasureText (IntPtr buffer, int length, ref SKRect bounds);
[Obsolete]
public float MeasureText (IntPtr buffer, IntPtr length, ref SKRect bounds);
[Obsolete]
public SKFont ToFont ();
```

Added methods:

```csharp
public SKPath GetFillPath (SKPath src, SKMatrix matrix);
public bool GetFillPath (SKPath src, SKPath dst, SKMatrix matrix);
public SKPath GetFillPath (SKPath src, SKRect cullRect, SKMatrix matrix);
public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, SKMatrix matrix);
```


#### Type Changed: SkiaSharp.SKPath

Modified properties:

```diff
-public SKPathConvexity Convexity { get; set; }
+public SKPathConvexity Convexity { get; }
```


#### Type Changed: SkiaSharp.SKPicture

Added methods:

```csharp
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKRect tile);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKMatrix localMatrix, SKRect tile);
```


#### Type Changed: SkiaSharp.SKPixmap

Added properties:

```csharp
public int BitShiftPerPixel { get; }
public long BytesSize64 { get; }
```

Removed methods:

```csharp
public bool Erase (SKColorF color, SKColorSpace colorspace, SKRectI subset);
public System.ReadOnlySpan<byte> GetPixelSpan ();
```

Obsoleted methods:

```cs
[Obsolete]
public bool ScalePixels (SKPixmap destination, SKFilterQuality quality);
```

Added methods:

```csharp
public bool ComputeIsOpaque ();
public float GetPixelAlpha (int x, int y);
public SKColorF GetPixelColorF (int x, int y);
public System.Span<byte> GetPixelSpan ();
public System.Span<T> GetPixelSpan<T> (int x, int y);
public System.Span<byte> GetPixelSpan (int x, int y);
public bool ScalePixels (SKPixmap destination);
public bool ScalePixels (SKPixmap destination, SKSamplingOptions sampling);
```


#### Type Changed: SkiaSharp.SKPngEncoderOptions

Modified properties:

```diff
-public SKPngEncoderFilterFlags FilterFlags { get; set; }
+public SKPngEncoderFilterFlags FilterFlags { get; }
-public int ZLibLevel { get; set; }
+public int ZLibLevel { get; }
```


#### Type Changed: SkiaSharp.SKPoint

Added methods:

```csharp
public static System.Numerics.Vector2 op_Implicit (SKPoint point);
public static SKPoint op_Implicit (System.Numerics.Vector2 vector);
```


#### Type Changed: SkiaSharp.SKPoint3

Added methods:

```csharp
public static System.Numerics.Vector3 op_Implicit (SKPoint3 point);
public static SKPoint3 op_Implicit (System.Numerics.Vector3 vector);
```


#### Type Changed: SkiaSharp.SKPointI

Added method:

```csharp
public static System.Numerics.Vector2 op_Implicit (SKPointI point);
```


#### Type Changed: SkiaSharp.SKRegion

Removed method:

```csharp
public bool SetRects (SKRectI[] rects);
```

Added method:

```csharp
public bool SetRects (System.ReadOnlySpan<SKRectI> rects);
```


#### Type Changed: SkiaSharp.SKRuntimeEffect

Removed methods:

```csharp
public static SKRuntimeEffect Create (string sksl, out string errors);
public SKShader ToShader (bool isOpaque);
public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms);
public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children);
public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children, SKMatrix localMatrix);
```

Added methods:

```csharp
public static SKRuntimeColorFilterBuilder BuildColorFilter (string sksl);
public static SKRuntimeShaderBuilder BuildShader (string sksl);
public static SKRuntimeEffect CreateColorFilter (string sksl, out string errors);
public static SKRuntimeEffect CreateShader (string sksl, out string errors);
public SKShader ToShader ();
public SKShader ToShader (SKRuntimeEffectUniforms uniforms);
public SKShader ToShader (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children);
public SKShader ToShader (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children, SKMatrix localMatrix);
```


#### Type Changed: SkiaSharp.SKRuntimeEffectChildren

Added interface:

```csharp
System.IDisposable
```

Removed property:

```csharp
public SKShader Item { set; }
```

Added property:

```csharp
public SKRuntimeEffectChild? Item { set; }
```

Removed methods:

```csharp
public void Add (string name, SKShader value);
public SKShader[] ToArray ();
```

Added methods:

```csharp
public void Add (string name, SKRuntimeEffectChild? value);
public virtual void Dispose ();
public SKObject[] ToArray ();
```


#### Type Changed: SkiaSharp.SKRuntimeEffectUniform

Added methods:

```csharp
public static SKRuntimeEffectUniform op_Implicit (SKColor value);
public static SKRuntimeEffectUniform op_Implicit (SKColorF value);
public static SKRuntimeEffectUniform op_Implicit (SKPoint value);
public static SKRuntimeEffectUniform op_Implicit (SKPoint3 value);
public static SKRuntimeEffectUniform op_Implicit (SKPointI value);
public static SKRuntimeEffectUniform op_Implicit (SKSize value);
public static SKRuntimeEffectUniform op_Implicit (SKSizeI value);
public static SKRuntimeEffectUniform op_Implicit (int value);
public static SKRuntimeEffectUniform op_Implicit (int[] value);
public static SKRuntimeEffectUniform op_Implicit (System.ReadOnlySpan<int> value);
public static SKRuntimeEffectUniform op_Implicit (System.Span<int> value);
```


#### Type Changed: SkiaSharp.SKRuntimeEffectUniforms

Added interface:

```csharp
System.IDisposable
```

Added property:

```csharp
public int Size { get; }
```

Added method:

```csharp
public virtual void Dispose ();
```


#### Type Changed: SkiaSharp.SKShader

Removed methods:

```csharp
public static SKShader CreateLerp (float weight, SKShader dst, SKShader src);
public static SKShader CreatePerlinNoiseImprovedNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float z);
```

Added methods:

```csharp
public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling);
public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling, SKMatrix localMatrix);
public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode);
public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKRect tile);
public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKMatrix localMatrix, SKRect tile);
```



#### Type Changed: SkiaSharp.SKTextBlobBuilder

Removed methods:

```csharp
public SKRotationScaleRunBuffer AllocateRotationScaleRun (SKFont font, int count);
```

Added methods:

```csharp
public SKHorizontalTextRunBuffer AllocateHorizontalTextRun (SKFont font, int count, float y, int textByteCount, SKRect? bounds);
public SKPositionedTextRunBuffer AllocatePositionedTextRun (SKFont font, int count, int textByteCount, SKRect? bounds);
public SKRotationScaleRunBuffer AllocateRotationScaleRun (SKFont font, int count, SKRect? bounds);
public SKRotationScaleRunBuffer AllocateRotationScaleTextRun (SKFont font, int count, int textByteCount, SKRect? bounds);
public SKTextRunBuffer AllocateTextRun (SKFont font, int count, float x, float y, int textByteCount, SKRect? bounds);
```


#### Type Changed: SkiaSharp.SKWebpEncoderOptions

Modified properties:

```diff
-public SKWebpEncoderCompression Compression { get; set; }
+public SKWebpEncoderCompression Compression { get; }
-public float Quality { get; set; }
+public float Quality { get; }
```


#### Type Changed: SkiaSharp.SkiaExtensions

Added methods:

```csharp
public static int GetBitShiftPerPixel (this SKColorType colorType);
```


#### New Type: SkiaSharp.SKCodecAnimationBlend

```csharp
[Serializable]
public enum SKCodecAnimationBlend {
	Src = 1,
	SrcOver = 0,
}
```

#### New Type: SkiaSharp.SKCubicResampler

```csharp
public struct SKCubicResampler, System.IEquatable<SKCubicResampler> {
	// constructors
	public SKCubicResampler (float b, float c);
	// fields
	public static SKCubicResampler CatmullRom;
	public static SKCubicResampler Mitchell;
	// properties
	public float B { get; }
	public float C { get; }
	// methods
	public virtual bool Equals (SKCubicResampler obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKCubicResampler left, SKCubicResampler right);
	public static bool op_Inequality (SKCubicResampler left, SKCubicResampler right);
}
```

#### New Type: SkiaSharp.SKFilterMode

```csharp
[Serializable]
public enum SKFilterMode {
	Linear = 1,
	Nearest = 0,
}
```

#### New Type: SkiaSharp.SKHorizontalTextRunBuffer

```csharp
public sealed class SKHorizontalTextRunBuffer : SkiaSharp.SKTextRunBuffer {
	// methods
	public System.Span<float> GetPositionSpan ();
	public void SetPositions (System.ReadOnlySpan<float> positions);
}
```

#### New Type: SkiaSharp.SKMipmapMode

```csharp
[Serializable]
public enum SKMipmapMode {
	Linear = 2,
	Nearest = 1,
	None = 0,
}
```

#### New Type: SkiaSharp.SKPositionedTextRunBuffer

```csharp
public sealed class SKPositionedTextRunBuffer : SkiaSharp.SKTextRunBuffer {
	// methods
	public System.Span<SKPoint> GetPositionSpan ();
	public void SetPositions (System.ReadOnlySpan<SKPoint> positions);
}
```

#### New Type: SkiaSharp.SKRuntimeColorFilterBuilder

```csharp
public class SKRuntimeColorFilterBuilder : SkiaSharp.SKRuntimeEffectBuilder, System.IDisposable {
	// constructors
	public SKRuntimeColorFilterBuilder (SKRuntimeEffect effect);
	// methods
	public SKColorFilter Build ();
}
```

#### New Type: SkiaSharp.SKRuntimeEffectBuilder

```csharp
public class SKRuntimeEffectBuilder : System.IDisposable {
	// constructors
	public SKRuntimeEffectBuilder (SKRuntimeEffect effect);
	// properties
	public SKRuntimeEffectChildren Children { get; }
	public SKRuntimeEffect Effect { get; }
	public SKRuntimeEffectUniforms Uniforms { get; }
	// methods
	public virtual void Dispose ();
}
```

#### New Type: SkiaSharp.SKRuntimeEffectBuilderException

```csharp
public class SKRuntimeEffectBuilderException : System.ApplicationException, System.Runtime.Serialization.ISerializable {
	// constructors
	public SKRuntimeEffectBuilderException (string message);
}
```

#### New Type: SkiaSharp.SKRuntimeEffectChild

```csharp
public struct SKRuntimeEffectChild {
	// constructors
	public SKRuntimeEffectChild (SKColorFilter colorFilter);
	public SKRuntimeEffectChild (SKShader shader);
	// properties
	public SKColorFilter ColorFilter { get; }
	public SKShader Shader { get; }
	public SKObject Value { get; }
	// methods
	public static SKRuntimeEffectChild op_Implicit (SKColorFilter colorFilter);
	public static SKRuntimeEffectChild op_Implicit (SKShader shader);
}
```

#### New Type: SkiaSharp.SKRuntimeShaderBuilder

```csharp
public class SKRuntimeShaderBuilder : SkiaSharp.SKRuntimeEffectBuilder, System.IDisposable {
	// constructors
	public SKRuntimeShaderBuilder (SKRuntimeEffect effect);
	// methods
	public SKShader Build ();
	public SKShader Build (SKMatrix localMatrix);
}
```

#### New Type: SkiaSharp.SKSamplingOptions

```csharp
public struct SKSamplingOptions, System.IEquatable<SKSamplingOptions> {
	// constructors
	public SKSamplingOptions (SKCubicResampler resampler);
	public SKSamplingOptions (SKFilterMode filter);
	public SKSamplingOptions (int maxAniso);
	public SKSamplingOptions (SKFilterMode filter, SKMipmapMode mipmap);
	// fields
	public static SKSamplingOptions Default;
	// properties
	public SKCubicResampler Cubic { get; }
	public SKFilterMode Filter { get; }
	public bool IsAniso { get; }
	public int MaxAniso { get; }
	public SKMipmapMode Mipmap { get; }
	public bool UseCubic { get; }
	// methods
	public virtual bool Equals (SKSamplingOptions obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKSamplingOptions left, SKSamplingOptions right);
	public static bool op_Inequality (SKSamplingOptions left, SKSamplingOptions right);
}
```

#### New Type: SkiaSharp.SKTextRunBuffer

```csharp
public class SKTextRunBuffer : SkiaSharp.SKRunBuffer {
	// properties
	public int TextSize { get; }
	// methods
	public System.Span<uint> GetClusterSpan ();
	public System.Span<byte> GetTextSpan ();
	public void SetClusters (System.ReadOnlySpan<uint> clusters);
	public void SetText (System.ReadOnlySpan<byte> text);
}
```
