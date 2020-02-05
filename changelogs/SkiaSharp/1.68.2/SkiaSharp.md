# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRBackendRenderTargetDesc

Added interface:

```csharp
System.IEquatable<GRBackendRenderTargetDesc>
```

Added methods:

```csharp
public virtual bool Equals (GRBackendRenderTargetDesc obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (GRBackendRenderTargetDesc left, GRBackendRenderTargetDesc right);
public static bool op_Inequality (GRBackendRenderTargetDesc left, GRBackendRenderTargetDesc right);
```


#### Type Changed: SkiaSharp.GRBackendTextureDesc

Added interface:

```csharp
System.IEquatable<GRBackendTextureDesc>
```

Added properties:

```csharp
public SKRectI Rect { get; }
public SKSizeI Size { get; }
```

Added methods:

```csharp
public virtual bool Equals (GRBackendTextureDesc obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (GRBackendTextureDesc left, GRBackendTextureDesc right);
public static bool op_Inequality (GRBackendTextureDesc left, GRBackendTextureDesc right);
```


#### Type Changed: SkiaSharp.GRGlBackendTextureDesc

Added interface:

```csharp
System.IEquatable<GRGlBackendTextureDesc>
```

Added properties:

```csharp
public SKRectI Rect { get; }
public SKSizeI Size { get; }
```

Added methods:

```csharp
public virtual bool Equals (GRGlBackendTextureDesc obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (GRGlBackendTextureDesc left, GRGlBackendTextureDesc right);
public static bool op_Inequality (GRGlBackendTextureDesc left, GRGlBackendTextureDesc right);
```


#### Type Changed: SkiaSharp.GRGlFramebufferInfo

Added interface:

```csharp
System.IEquatable<GRGlFramebufferInfo>
```

Added methods:

```csharp
public virtual bool Equals (GRGlFramebufferInfo obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (GRGlFramebufferInfo left, GRGlFramebufferInfo right);
public static bool op_Inequality (GRGlFramebufferInfo left, GRGlFramebufferInfo right);
```


#### Type Changed: SkiaSharp.GRGlTextureInfo

Added constructor:

```csharp
public GRGlTextureInfo (uint target, uint id);
```

Added interface:

```csharp
System.IEquatable<GRGlTextureInfo>
```

Added methods:

```csharp
public virtual bool Equals (GRGlTextureInfo obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (GRGlTextureInfo left, GRGlTextureInfo right);
public static bool op_Inequality (GRGlTextureInfo left, GRGlTextureInfo right);
```


#### Type Changed: SkiaSharp.SKBitmap

Added methods:

```csharp
public SKShader ToShader ();
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix);
```


#### Type Changed: SkiaSharp.SKCanvas

Added properties:

```csharp
public bool IsClipEmpty { get; }
public bool IsClipRect { get; }
```

Added methods:

```csharp
public void DrawArc (SKRect oval, float startAngle, float sweepAngle, bool useCenter, SKPaint paint);
public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKPaint paint);
public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKPaint paint);
public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKRect cullRect, SKPaint paint);
public void DrawPatch (SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKPaint paint);
public void DrawPatch (SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKBlendMode mode, SKPaint paint);
public void DrawRoundRectDifference (SKRoundRect outer, SKRoundRect inner, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKCodecFrameInfo

Added interface:

```csharp
System.IEquatable<SKCodecFrameInfo>
```

Added methods:

```csharp
public virtual bool Equals (SKCodecFrameInfo obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKCodecFrameInfo left, SKCodecFrameInfo right);
public static bool op_Inequality (SKCodecFrameInfo left, SKCodecFrameInfo right);
```


#### Type Changed: SkiaSharp.SKCodecOptions

Added interface:

```csharp
System.IEquatable<SKCodecOptions>
```

Added methods:

```csharp
public virtual bool Equals (SKCodecOptions obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKCodecOptions left, SKCodecOptions right);
public static bool op_Inequality (SKCodecOptions left, SKCodecOptions right);
```


#### Type Changed: SkiaSharp.SKColor

Added interface:

```csharp
System.IEquatable<SKColor>
```

Modified methods:

```diff
-public override bool Equals (object other)
+public override bool Equals (object obj)
```

Added method:

```csharp
public virtual bool Equals (SKColor obj);
```


#### Type Changed: SkiaSharp.SKColorSpacePrimaries

Added interface:

```csharp
System.IEquatable<SKColorSpacePrimaries>
```

Added field:

```csharp
public static SKColorSpacePrimaries Empty;
```

Added methods:

```csharp
public virtual bool Equals (SKColorSpacePrimaries obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKColorSpacePrimaries left, SKColorSpacePrimaries right);
public static bool op_Inequality (SKColorSpacePrimaries left, SKColorSpacePrimaries right);
```


#### Type Changed: SkiaSharp.SKColorSpaceTransferFn

Added interface:

```csharp
System.IEquatable<SKColorSpaceTransferFn>
```

Added field:

```csharp
public static SKColorSpaceTransferFn Empty;
```

Added methods:

```csharp
public virtual bool Equals (SKColorSpaceTransferFn obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKColorSpaceTransferFn left, SKColorSpaceTransferFn right);
public static bool op_Inequality (SKColorSpaceTransferFn left, SKColorSpaceTransferFn right);
```


#### Type Changed: SkiaSharp.SKDocumentPdfMetadata

Added interface:

```csharp
System.IEquatable<SKDocumentPdfMetadata>
```

Added methods:

```csharp
public virtual bool Equals (SKDocumentPdfMetadata obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKDocumentPdfMetadata left, SKDocumentPdfMetadata right);
public static bool op_Inequality (SKDocumentPdfMetadata left, SKDocumentPdfMetadata right);
```


#### Type Changed: SkiaSharp.SKFontMetrics

Added interface:

```csharp
System.IEquatable<SKFontMetrics>
```

Added methods:

```csharp
public virtual bool Equals (SKFontMetrics obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKFontMetrics left, SKFontMetrics right);
public static bool op_Inequality (SKFontMetrics left, SKFontMetrics right);
```


#### Type Changed: SkiaSharp.SKHighContrastConfig

Added interface:

```csharp
System.IEquatable<SKHighContrastConfig>
```

Added methods:

```csharp
public virtual bool Equals (SKHighContrastConfig obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKHighContrastConfig left, SKHighContrastConfig right);
public static bool op_Inequality (SKHighContrastConfig left, SKHighContrastConfig right);
```


#### Type Changed: SkiaSharp.SKImage

Added method:

```csharp
public SKShader ToShader ();
```


#### Type Changed: SkiaSharp.SKImageInfo

Added interface:

```csharp
System.IEquatable<SKImageInfo>
```

Added methods:

```csharp
public virtual bool Equals (SKImageInfo obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKImageInfo left, SKImageInfo right);
public static bool op_Inequality (SKImageInfo left, SKImageInfo right);
```


#### Type Changed: SkiaSharp.SKJpegEncoderOptions

Added interface:

```csharp
System.IEquatable<SKJpegEncoderOptions>
```

Added methods:

```csharp
public virtual bool Equals (SKJpegEncoderOptions obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKJpegEncoderOptions left, SKJpegEncoderOptions right);
public static bool op_Inequality (SKJpegEncoderOptions left, SKJpegEncoderOptions right);
```


#### Type Changed: SkiaSharp.SKLattice

Added interface:

```csharp
System.IEquatable<SKLattice>
```

Added methods:

```csharp
public virtual bool Equals (SKLattice obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKLattice left, SKLattice right);
public static bool op_Inequality (SKLattice left, SKLattice right);
```


#### Type Changed: SkiaSharp.SKMask

Added interface:

```csharp
System.IEquatable<SKMask>
```

Added methods:

```csharp
public virtual bool Equals (SKMask obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKMask left, SKMask right);
public static bool op_Inequality (SKMask left, SKMask right);
```


#### Type Changed: SkiaSharp.SKMatrix

Added interface:

```csharp
System.IEquatable<SKMatrix>
```

Added fields:

```csharp
public static SKMatrix Empty;
public static SKMatrix Identity;
```

Added property:

```csharp
public bool IsInvertible { get; }
```

Obsoleted methods:

```diff
 [Obsolete ("Use MapRect(SKRect) instead.")]
 public static void MapRect (ref SKMatrix matrix, out SKRect dest, ref SKRect source);
 [Obsolete ("Use PostConcat(SKMatrix) instead.")]
 public static void PostConcat (ref SKMatrix target, SKMatrix matrix);
 [Obsolete ("Use PostConcat(SKMatrix) instead.")]
 public static void PostConcat (ref SKMatrix target, ref SKMatrix matrix);
 [Obsolete ("Use PreConcat(SKMatrix) instead.")]
 public static void PreConcat (ref SKMatrix target, SKMatrix matrix);
 [Obsolete ("Use PreConcat(SKMatrix) instead.")]
 public static void PreConcat (ref SKMatrix target, ref SKMatrix matrix);
 [Obsolete ("Use CreateRotation(float) instead.")]
 public static void Rotate (ref SKMatrix matrix, float radians);
 [Obsolete ("Use CreateRotation(float, float, float) instead.")]
 public static void Rotate (ref SKMatrix matrix, float radians, float pivotx, float pivoty);
 [Obsolete ("Use CreateRotationDegrees(float) instead.")]
 public static void RotateDegrees (ref SKMatrix matrix, float degrees);
 [Obsolete ("Use CreateRotationDegrees(float, float, float) instead.")]
 public static void RotateDegrees (ref SKMatrix matrix, float degrees, float pivotx, float pivoty);
 [Obsolete ()]
 public void SetScaleTranslate (float sx, float sy, float tx, float ty);
```

Added methods:

```csharp
public static SKMatrix Concat (SKMatrix first, SKMatrix second);
public static SKMatrix CreateIdentity ();
public static SKMatrix CreateRotation (float radians);
public static SKMatrix CreateRotation (float radians, float pivotX, float pivotY);
public static SKMatrix CreateRotationDegrees (float degrees);
public static SKMatrix CreateRotationDegrees (float degrees, float pivotX, float pivotY);
public static SKMatrix CreateScale (float x, float y);
public static SKMatrix CreateScale (float x, float y, float pivotX, float pivotY);
public static SKMatrix CreateSkew (float x, float y);
public static SKMatrix CreateTranslation (float x, float y);
public virtual bool Equals (SKMatrix obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public SKMatrix Invert ();
public SKMatrix PostConcat (SKMatrix matrix);
public SKMatrix PreConcat (SKMatrix matrix);
public static bool op_Equality (SKMatrix left, SKMatrix right);
public static bool op_Inequality (SKMatrix left, SKMatrix right);
```


#### Type Changed: SkiaSharp.SKMatrix44

Added property:

```csharp
public bool IsInvertible { get; }
```

Added methods:

```csharp
public static SKMatrix44 CreateTranslation (float x, float y, float z);
public static SKMatrix44 op_Implicit (SKMatrix matrix);
```


#### Type Changed: SkiaSharp.SKPMColor

Added interface:

```csharp
System.IEquatable<SKPMColor>
```

Modified methods:

```diff
-public override bool Equals (object other)
+public override bool Equals (object obj)
```

Added method:

```csharp
public virtual bool Equals (SKPMColor obj);
```


#### Type Changed: SkiaSharp.SKPicture

Added methods:

```csharp
public SKShader ToShader ();
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKRect tile);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix, SKRect tile);
```


#### Type Changed: SkiaSharp.SKPixmap

Added methods:

```csharp
public bool Erase (SKColorF color);
public bool Erase (SKColorF color, SKRectI subset);
```


#### Type Changed: SkiaSharp.SKPngEncoderOptions

Added interface:

```csharp
System.IEquatable<SKPngEncoderOptions>
```

Added methods:

```csharp
public virtual bool Equals (SKPngEncoderOptions obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKPngEncoderOptions left, SKPngEncoderOptions right);
public static bool op_Inequality (SKPngEncoderOptions left, SKPngEncoderOptions right);
```


#### Type Changed: SkiaSharp.SKPoint

Added interface:

```csharp
System.IEquatable<SKPoint>
```

Added method:

```csharp
public virtual bool Equals (SKPoint obj);
```


#### Type Changed: SkiaSharp.SKPoint3

Added interface:

```csharp
System.IEquatable<SKPoint3>
```

Added method:

```csharp
public virtual bool Equals (SKPoint3 obj);
```


#### Type Changed: SkiaSharp.SKPointI

Added interface:

```csharp
System.IEquatable<SKPointI>
```

Added method:

```csharp
public virtual bool Equals (SKPointI obj);
```


#### Type Changed: SkiaSharp.SKRect

Added interface:

```csharp
System.IEquatable<SKRect>
```

Added method:

```csharp
public virtual bool Equals (SKRect obj);
```


#### Type Changed: SkiaSharp.SKRectI

Added interface:

```csharp
System.IEquatable<SKRectI>
```

Added method:

```csharp
public virtual bool Equals (SKRectI obj);
```


#### Type Changed: SkiaSharp.SKRegion

Added properties:

```csharp
public bool IsComplex { get; }
public bool IsEmpty { get; }
public bool IsRect { get; }
```

Added methods:

```csharp
public bool Contains (SKPath path);
public bool Contains (SKRectI rect);
public SKRegion.ClipIterator CreateClipIterator (SKRectI clip);
public SKRegion.RectIterator CreateRectIterator ();
public SKRegion.SpanIterator CreateSpanIterator (int y, int left, int right);
public SKPath GetBoundaryPath ();
public bool QuickContains (SKRectI rect);
public bool QuickReject (SKPath path);
public bool QuickReject (SKRectI rect);
public bool QuickReject (SKRegion region);
public void SetEmpty ();
public bool SetRects (SKRectI[] rects);
public void Translate (int x, int y);
```


#### Type Changed: SkiaSharp.SKRoundRect

Added constructor:

```csharp
public SKRoundRect (SKRect rect, float radius);
```


#### Type Changed: SkiaSharp.SKShader

Added methods:

```csharp
public static SKShader CreateBitmap (SKBitmap src);
public static SKShader CreateColor (SKColorF color, SKColorSpace colorspace);
public static SKShader CreateImage (SKImage src);
public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy);
public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix);
public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode);
public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode);
public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix);
public static SKShader CreatePerlinNoiseFractalNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI tileSize);
public static SKShader CreatePerlinNoiseImprovedNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float z);
public static SKShader CreatePerlinNoiseTurbulence (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI tileSize);
public static SKShader CreatePicture (SKPicture src);
public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode);
public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode);
public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix);
public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace);
public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos);
public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKMatrix localMatrix);
public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode tileMode, float startAngle, float endAngle);
public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle);
public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix localMatrix);
public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode);
public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode);
public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix);
public SKShader WithColorFilter (SKColorFilter filter);
public SKShader WithLocalMatrix (SKMatrix localMatrix);
```


#### Type Changed: SkiaSharp.SKSize

Added interface:

```csharp
System.IEquatable<SKSize>
```

Modified methods:

```diff
-public bool op_Equality (SKSize sz1, SKSize sz2---right---)
+public bool op_Equality (SKSize left, SKSize +++sz2+++right)
-public bool op_Inequality (SKSize sz1, SKSize sz2---right---)
+public bool op_Inequality (SKSize left, SKSize +++sz2+++right)
```

Added method:

```csharp
public virtual bool Equals (SKSize obj);
```


#### Type Changed: SkiaSharp.SKSizeI

Added interface:

```csharp
System.IEquatable<SKSizeI>
```

Modified methods:

```diff
-public bool op_Equality (SKSizeI sz1, SKSizeI sz2---right---)
+public bool op_Equality (SKSizeI left, SKSizeI +++sz2+++right)
-public bool op_Inequality (SKSizeI sz1, SKSizeI sz2---right---)
+public bool op_Inequality (SKSizeI left, SKSizeI +++sz2+++right)
```

Added method:

```csharp
public virtual bool Equals (SKSizeI obj);
```


#### Type Changed: SkiaSharp.SKSurfaceProps

Added interface:

```csharp
System.IEquatable<SKSurfaceProps>
```

Added methods:

```csharp
public virtual bool Equals (SKSurfaceProps obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKSurfaceProps left, SKSurfaceProps right);
public static bool op_Inequality (SKSurfaceProps left, SKSurfaceProps right);
```


#### Type Changed: SkiaSharp.SKWebpEncoderOptions

Added interface:

```csharp
System.IEquatable<SKWebpEncoderOptions>
```

Added methods:

```csharp
public virtual bool Equals (SKWebpEncoderOptions obj);
public override bool Equals (object obj);
public override int GetHashCode ();
public static bool op_Equality (SKWebpEncoderOptions left, SKWebpEncoderOptions right);
public static bool op_Inequality (SKWebpEncoderOptions left, SKWebpEncoderOptions right);
```


#### New Type: SkiaSharp.SKColorF

```csharp
public struct SKColorF, System.IEquatable<SKColorF> {
	// constructors
	public SKColorF (float red, float green, float blue);
	public SKColorF (float red, float green, float blue, float alpha);
	// fields
	public static SKColorF Empty;
	// properties
	public float Alpha { get; }
	public float Blue { get; }
	public float Green { get; }
	public float Hue { get; }
	public float Red { get; }
	// methods
	public SKColorF Clamp ();
	public virtual bool Equals (SKColorF obj);
	public override bool Equals (object obj);
	public static SKColorF FromHsl (float h, float s, float l, float a);
	public static SKColorF FromHsv (float h, float s, float v, float a);
	public override int GetHashCode ();
	public void ToHsl (out float h, out float s, out float l);
	public void ToHsv (out float h, out float s, out float v);
	public override string ToString ();
	public SKColorF WithAlpha (float alpha);
	public SKColorF WithBlue (float blue);
	public SKColorF WithGreen (float green);
	public SKColorF WithRed (float red);
	public static bool op_Equality (SKColorF left, SKColorF right);
	public static SKColor op_Explicit (SKColorF color);
	public static SKColorF op_Implicit (SKColor color);
	public static bool op_Inequality (SKColorF left, SKColorF right);
}
```

#### New Type: SkiaSharp.SKRotationScaleMatrix

```csharp
public struct SKRotationScaleMatrix, System.IEquatable<SKRotationScaleMatrix> {
	// constructors
	public SKRotationScaleMatrix (float scos, float ssin, float tx, float ty);
	// fields
	public static SKRotationScaleMatrix Empty;
	public static SKRotationScaleMatrix Identity;
	// properties
	public float SCos { get; set; }
	public float SSin { get; set; }
	public float TX { get; set; }
	public float TY { get; set; }
	// methods
	public static SKRotationScaleMatrix CreateIdentity ();
	public static SKRotationScaleMatrix CreateRotation (float radians, float anchorX, float anchorY);
	public static SKRotationScaleMatrix CreateRotationDegrees (float degrees, float anchorX, float anchorY);
	public static SKRotationScaleMatrix CreateScale (float s);
	public static SKRotationScaleMatrix CreateTranslation (float x, float y);
	public virtual bool Equals (SKRotationScaleMatrix obj);
	public override bool Equals (object obj);
	public static SKRotationScaleMatrix FromDegrees (float scale, float degrees, float tx, float ty, float anchorX, float anchorY);
	public static SKRotationScaleMatrix FromRadians (float scale, float radians, float tx, float ty, float anchorX, float anchorY);
	public override int GetHashCode ();
	public SKMatrix ToMatrix ();
	public static bool op_Equality (SKRotationScaleMatrix left, SKRotationScaleMatrix right);
	public static bool op_Inequality (SKRotationScaleMatrix left, SKRotationScaleMatrix right);
}
```


