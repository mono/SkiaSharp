# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 3.0.0.0 vs 2.88.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Removed methods:

```csharp
[Obsolete]
public static GRContext Create (GRBackend backend);

[Obsolete]
public static GRContext Create (GRBackend backend, GRGlInterface backendContext);

[Obsolete]
public static GRContext Create (GRBackend backend, IntPtr backendContext);

[Obsolete]
public int GetRecommendedSampleCount (GRPixelConfig config, float dpi);

[Obsolete]
public void GetResourceCacheLimits (out int maxResources, out long maxResourceBytes);

[Obsolete]
public void SetResourceCacheLimits (int maxResources, long maxResourceBytes);
```


#### Type Changed: SkiaSharp.GRGlInterface

Removed methods:

```csharp
[Obsolete]
public static GRGlInterface AssembleAngleInterface (GRGlGetProcDelegate get);

[Obsolete]
public static GRGlInterface AssembleAngleInterface (object context, GRGlGetProcDelegate get);

[Obsolete]
public static GRGlInterface AssembleGlInterface (GRGlGetProcDelegate get);

[Obsolete]
public static GRGlInterface AssembleGlInterface (object context, GRGlGetProcDelegate get);

[Obsolete]
public static GRGlInterface AssembleGlesInterface (GRGlGetProcDelegate get);

[Obsolete]
public static GRGlInterface AssembleGlesInterface (object context, GRGlGetProcDelegate get);

[Obsolete]
public static GRGlInterface AssembleInterface (GRGlGetProcDelegate get);

[Obsolete]
public static GRGlInterface AssembleInterface (object context, GRGlGetProcDelegate get);

[Obsolete]
public static GRGlInterface CreateDefaultInterface ();

[Obsolete]
public static GRGlInterface CreateNativeAngleInterface ();

[Obsolete]
public static GRGlInterface CreateNativeEvasInterface (IntPtr evas);

[Obsolete]
public static GRGlInterface CreateNativeGlInterface ();
```


#### Type Changed: SkiaSharp.SKBitmap

Removed methods:

```csharp
[Obsolete]
public IntPtr GetAddr (int x, int y);

[Obsolete]
public ushort GetAddr16 (int x, int y);

[Obsolete]
public uint GetAddr32 (int x, int y);

[Obsolete]
public byte GetAddr8 (int x, int y);

[Obsolete]
public SKPMColor GetIndex8Color (int x, int y);
public System.ReadOnlySpan<byte> GetPixelSpan ();
public bool InstallMaskPixels (SKMask mask);

[Obsolete]
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);

[Obsolete]
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable, SKBitmapReleaseDelegate releaseProc, object context);

[Obsolete]
public bool Resize (SKBitmap dst, SKBitmapResizeMethod method);

[Obsolete]
public SKBitmap Resize (SKImageInfo info, SKBitmapResizeMethod method);

[Obsolete]
public static bool Resize (SKBitmap dst, SKBitmap src, SKBitmapResizeMethod method);

[Obsolete]
public void SetColorTable (SKColorTable ct);

[Obsolete]
public void SetPixels (IntPtr pixels, SKColorTable ct);
```


#### Type Changed: SkiaSharp.SKCanvas

Removed methods:

```csharp
public void Concat (ref SKMatrix m);
public void DrawDrawable (SKDrawable drawable, ref SKMatrix matrix);
public void DrawPicture (SKPicture picture, ref SKMatrix matrix, SKPaint paint);

[Obsolete]
public void DrawPositionedText (byte[] text, SKPoint[] points, SKPaint paint);

[Obsolete]
public void DrawPositionedText (string text, SKPoint[] points, SKPaint paint);

[Obsolete]
public void DrawPositionedText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint);

[Obsolete]
public void DrawText (byte[] text, SKPoint p, SKPaint paint);

[Obsolete]
public void DrawText (byte[] text, float x, float y, SKPaint paint);

[Obsolete]
public void DrawText (IntPtr buffer, int length, SKPoint p, SKPaint paint);

[Obsolete]
public void DrawText (IntPtr buffer, int length, float x, float y, SKPaint paint);

[Obsolete]
public void DrawTextOnPath (byte[] text, SKPath path, SKPoint offset, SKPaint paint);

[Obsolete]
public void DrawTextOnPath (byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete]
public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, SKPoint offset, SKPaint paint);

[Obsolete]
public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint);
public void SetMatrix (SKMatrix matrix);
```


#### Type Changed: SkiaSharp.SKCodec

Removed methods:

```csharp
[Obsolete]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKColorTable colorTable, ref int colorTableCount);

[Obsolete]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, IntPtr colorTable, ref int colorTableCount);

[Obsolete]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);

[Obsolete]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);

[Obsolete]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);

[Obsolete]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);

[Obsolete]
public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);

[Obsolete]
public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);

[Obsolete]
public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);

[Obsolete]
public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
```


#### Type Changed: SkiaSharp.SKColorSpace

Removed methods:

```csharp
[Obsolete]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut);

[Obsolete]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50);

[Obsolete]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut);

[Obsolete]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50);

[Obsolete]
public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKColorSpaceGamut gamut);

[Obsolete]
public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKMatrix44 toXyzD50);

[Obsolete]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);

[Obsolete]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);

[Obsolete]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);

[Obsolete]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);

[Obsolete]
public SKMatrix44 FromXyzD50 ();

[Obsolete]
public SKMatrix44 ToXyzD50 ();

[Obsolete]
public bool ToXyzD50 (SKMatrix44 toXyzD50);
```


#### Type Changed: SkiaSharp.SKColorSpacePrimaries

Removed methods:

```csharp
[Obsolete]
public SKMatrix44 ToXyzD50 ();

[Obsolete]
public bool ToXyzD50 (SKMatrix44 toXyzD50);
```


#### Type Changed: SkiaSharp.SKDocument

Removed method:

```csharp
[Obsolete]
public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata, float dpi);
```


#### Type Changed: SkiaSharp.SKDrawable

Removed method:

```csharp
public void Draw (SKCanvas canvas, ref SKMatrix matrix);
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

Removed methods:

```csharp
[Obsolete]
public SKData Encode (SKPixelSerializer serializer);

[Obsolete]
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc);

[Obsolete]
public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc);

[Obsolete]
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);

[Obsolete]
public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);

[Obsolete]
public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);

[Obsolete]
public static SKImage FromPixelData (SKImageInfo info, SKData data, int rowBytes);

[Obsolete]
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc);

[Obsolete]
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc);

[Obsolete]
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);

[Obsolete]
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);

[Obsolete]
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);

[Obsolete]
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);

[Obsolete]
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);

[Obsolete]
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);

[Obsolete]
public SKImage ToTextureImage (GRContext context, SKColorSpace colorspace);
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

[Obsolete]
public static SKImageFilter CreateDisplacementMapEffect (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKImageFilter.CropRect cropRect);

[Obsolete]
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateErode (int radiusX, int radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateImage (SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality);
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMatrix (SKMatrix matrix, SKFilterQuality quality, SKImageFilter input);

[Obsolete]
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKImageFilter.CropRect cropRect);

[Obsolete]
public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKBlendMode[] modes, SKImageFilter.CropRect cropRect);

[Obsolete]
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKBlendMode mode, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateOffset (float dx, float dy, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePaint (SKPaint paint, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
```

Modified methods:

```diff
 public SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold, SKImageFilter input--- = NULL---)
```

#### Removed Type SkiaSharp.SKImageFilter.CropRect

#### Type Changed: SkiaSharp.SKJpegEncoderOptions

Modified properties:

```diff
 public SKJpegEncoderAlphaOption AlphaOption { get; ---set;--- }
 public SKJpegEncoderDownsample Downsample { get; ---set;--- }
 public int Quality { get; ---set;--- }
```


#### Type Changed: SkiaSharp.SKMaskFilter

Removed methods:

```csharp
[Obsolete]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags);

[Obsolete]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder);

[Obsolete]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags);

[Obsolete]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM);
```


#### Type Changed: SkiaSharp.SKMatrix

Removed methods:

```csharp
public static void Concat (ref SKMatrix target, ref SKMatrix first, ref SKMatrix second);

[Obsolete]
public static SKMatrix MakeIdentity ();

[Obsolete]
public static SKMatrix MakeRotation (float radians);

[Obsolete]
public static SKMatrix MakeRotation (float radians, float pivotx, float pivoty);

[Obsolete]
public static SKMatrix MakeRotationDegrees (float degrees);

[Obsolete]
public static SKMatrix MakeRotationDegrees (float degrees, float pivotx, float pivoty);

[Obsolete]
public static SKMatrix MakeScale (float sx, float sy);

[Obsolete]
public static SKMatrix MakeScale (float sx, float sy, float pivotX, float pivotY);

[Obsolete]
public static SKMatrix MakeSkew (float sx, float sy);

[Obsolete]
public static SKMatrix MakeTranslation (float dx, float dy);

[Obsolete]
public static void MapRect (ref SKMatrix matrix, out SKRect dest, ref SKRect source);

[Obsolete]
public static void PostConcat (ref SKMatrix target, SKMatrix matrix);

[Obsolete]
public static void PostConcat (ref SKMatrix target, ref SKMatrix matrix);

[Obsolete]
public static void PreConcat (ref SKMatrix target, SKMatrix matrix);

[Obsolete]
public static void PreConcat (ref SKMatrix target, ref SKMatrix matrix);

[Obsolete]
public static void Rotate (ref SKMatrix matrix, float radians);

[Obsolete]
public static void Rotate (ref SKMatrix matrix, float radians, float pivotx, float pivoty);

[Obsolete]
public static void RotateDegrees (ref SKMatrix matrix, float degrees);

[Obsolete]
public static void RotateDegrees (ref SKMatrix matrix, float degrees, float pivotx, float pivoty);

[Obsolete]
public void SetScaleTranslate (float sx, float sy, float tx, float ty);
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

Removed interface:

```csharp
System.IDisposable
```

Removed property:

```csharp
public SKMatrix44TypeMask Type { get; }
```

Removed methods:

```csharp
public static SKMatrix44 CreateTranslate (float x, float y, float z);
public double Determinant ();
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

Modified methods:

```diff
-public SKPoint MapPoint (SKPoint src)
+public SKPoint MapPoint (SKPoint point)
```


#### Type Changed: SkiaSharp.SKPaint

Removed method:

```csharp
[Obsolete]
public float GetFontMetrics (out SKFontMetrics metrics, float scale);
```


#### Type Changed: SkiaSharp.SKPath

Modified properties:

```diff
 public SKPathConvexity Convexity { get; ---set;--- }
```

Removed methods:

```csharp
public void AddPath (SKPath other, ref SKMatrix matrix, SKPathAddMode mode);

[Obsolete]
public void AddRoundedRect (SKRect rect, float rx, float ry, SKPathDirection dir);
public void Transform (SKMatrix matrix);
public void Transform (SKMatrix matrix, SKPath destination);
```

#### Type Changed: SkiaSharp.SKPath.Iterator

Removed method:

```csharp
[Obsolete]
public SKPathVerb Next (SKPoint[] points, bool doConsumeDegenerates, bool exact);
```



#### Type Changed: SkiaSharp.SKPixmap

Removed methods:

```csharp
[Obsolete]
public static bool Encode (SKWStream dst, SKPixmap src, SKJpegEncoderOptions options);

[Obsolete]
public static bool Encode (SKWStream dst, SKPixmap src, SKPngEncoderOptions options);

[Obsolete]
public static bool Encode (SKWStream dst, SKPixmap src, SKWebpEncoderOptions options);

[Obsolete]
public static bool Encode (SKWStream dst, SKBitmap src, SKEncodedImageFormat format, int quality);

[Obsolete]
public static bool Encode (SKWStream dst, SKPixmap src, SKEncodedImageFormat encoder, int quality);
public bool Erase (SKColorF color, SKColorSpace colorspace, SKRectI subset);
public System.ReadOnlySpan<byte> GetPixelSpan ();

[Obsolete]
public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKTransferFunctionBehavior behavior);

[Obsolete]
public void Reset (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable);

[Obsolete]
public static bool Resize (SKPixmap dst, SKPixmap src, SKBitmapResizeMethod method);
```


#### Type Changed: SkiaSharp.SKPngEncoderOptions

Modified properties:

```diff
 public SKPngEncoderFilterFlags FilterFlags { get; ---set;--- }
 public int ZLibLevel { get; ---set;--- }
```


#### Type Changed: SkiaSharp.SKRegion

Removed method:

```csharp
public bool SetRects (SKRectI[] rects);
```


#### Type Changed: SkiaSharp.SKRunBuffer

Removed methods:

```csharp
[Obsolete]
public System.Span<uint> GetClusterSpan ();

[Obsolete]
public System.Span<byte> GetTextSpan ();

[Obsolete]
public void SetClusters (System.ReadOnlySpan<uint> clusters);

[Obsolete]
public void SetText (System.ReadOnlySpan<byte> text);
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


#### Type Changed: SkiaSharp.SKRuntimeEffectChildren

Removed property:

```csharp
public SKShader Item { set; }
```

Removed methods:

```csharp
public void Add (string name, SKShader value);
public SKShader[] ToArray ();
```


#### Type Changed: SkiaSharp.SKShader

Removed methods:

```csharp
public static SKShader CreateLerp (float weight, SKShader dst, SKShader src);
public static SKShader CreatePerlinNoiseImprovedNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float z);
```


#### Type Changed: SkiaSharp.SKSurface

Removed methods:

```csharp
[Obsolete]
public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc);

[Obsolete]
public static SKSurface Create (GRContext context, GRBackendTextureDesc desc);

[Obsolete]
public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc);

[Obsolete]
public static SKSurface Create (SKImageInfo info, SKSurfaceProps props);

[Obsolete]
public static SKSurface Create (SKPixmap pixmap, SKSurfaceProps props);

[Obsolete]
public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props);

[Obsolete]
public static SKSurface Create (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete]
public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete]
public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props);

[Obsolete]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType);

[Obsolete]
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProps props);

[Obsolete]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props);

[Obsolete]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes);

[Obsolete]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props);

[Obsolete]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
```


#### Type Changed: SkiaSharp.SKSvgCanvas

Removed method:

```csharp
[Obsolete]
public static SKCanvas Create (SKRect bounds, SKXmlWriter writer);
```


#### Type Changed: SkiaSharp.SKTextBlobBuilder

Removed methods:

```csharp
[Obsolete]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions);

[Obsolete]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions);

[Obsolete]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, SKRect? bounds);

[Obsolete]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, SKRect bounds);

[Obsolete]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);

[Obsolete]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters);

[Obsolete]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters);

[Obsolete]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);

[Obsolete]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters, SKRect bounds);

[Obsolete]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters, SKRect bounds);

[Obsolete]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions);

[Obsolete]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions);

[Obsolete]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, SKRect? bounds);

[Obsolete]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, SKRect bounds);

[Obsolete]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);

[Obsolete]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters);

[Obsolete]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters);

[Obsolete]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);

[Obsolete]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters, SKRect bounds);

[Obsolete]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters, SKRect bounds);

[Obsolete]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs);

[Obsolete]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs);

[Obsolete]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, SKRect? bounds);

[Obsolete]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, SKRect bounds);

[Obsolete]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);

[Obsolete]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters);

[Obsolete]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters);

[Obsolete]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);

[Obsolete]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters, SKRect bounds);

[Obsolete]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters, SKRect bounds);

[Obsolete]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y);

[Obsolete]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount);

[Obsolete]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, SKRect? bounds);

[Obsolete]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount, SKRect? bounds);

[Obsolete]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count);

[Obsolete]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount);

[Obsolete]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, SKRect? bounds);

[Obsolete]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount, SKRect? bounds);
public SKRotationScaleRunBuffer AllocateRotationScaleRun (SKFont font, int count);

[Obsolete]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y);

[Obsolete]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount);

[Obsolete]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, SKRect? bounds);

[Obsolete]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount, SKRect? bounds);
```


#### Type Changed: SkiaSharp.SKTypeface

Removed methods:

```csharp
[Obsolete]
public int CharsToGlyphs (string chars, out ushort[] glyphs);

[Obsolete]
public int CharsToGlyphs (IntPtr str, int strlen, SKEncoding encoding, out ushort[] glyphs);

[Obsolete]
public int CountGlyphs (byte[] str, SKEncoding encoding);

[Obsolete]
public int CountGlyphs (System.ReadOnlySpan<byte> str, SKEncoding encoding);

[Obsolete]
public int CountGlyphs (string str, SKEncoding encoding);

[Obsolete]
public int CountGlyphs (IntPtr str, int strLen, SKEncoding encoding);

[Obsolete]
public static SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style);

[Obsolete]
public static SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style);

[Obsolete]
public ushort[] GetGlyphs (byte[] text, SKEncoding encoding);

[Obsolete]
public ushort[] GetGlyphs (System.ReadOnlySpan<byte> text, SKEncoding encoding);

[Obsolete]
public ushort[] GetGlyphs (string text, SKEncoding encoding);

[Obsolete]
public int GetGlyphs (string text, out ushort[] glyphs);

[Obsolete]
public int GetGlyphs (byte[] text, SKEncoding encoding, out ushort[] glyphs);

[Obsolete]
public ushort[] GetGlyphs (IntPtr text, int length, SKEncoding encoding);

[Obsolete]
public int GetGlyphs (System.ReadOnlySpan<byte> text, SKEncoding encoding, out ushort[] glyphs);

[Obsolete]
public int GetGlyphs (string text, SKEncoding encoding, out ushort[] glyphs);

[Obsolete]
public int GetGlyphs (IntPtr text, int length, SKEncoding encoding, out ushort[] glyphs);
```


#### Type Changed: SkiaSharp.SKWebpEncoderOptions

Modified properties:

```diff
 public SKWebpEncoderCompression Compression { get; ---set;--- }
 public float Quality { get; ---set;--- }
```


#### Type Changed: SkiaSharp.SkiaExtensions

Removed methods:

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
public static SKColorType ToColorType (this GRPixelConfig config);

[Obsolete]
public static SKFilterQuality ToFilterQuality (this SKBitmapResizeMethod method);

[Obsolete]
public static uint ToGlSizedFormat (this GRPixelConfig config);

[Obsolete]
public static GRPixelConfig ToPixelConfig (this SKColorType colorType);

[Obsolete]
public static SKShaderTileMode ToShaderTileMode (this SKMatrixConvolutionTileMode tileMode);

[Obsolete]
public static SKTextEncoding ToTextEncoding (this SKEncoding encoding);
```


#### Type Changed: SkiaSharp.StringUtilities

Removed method:

```csharp
[Obsolete]
public static byte[] GetEncodedText (string text, SKEncoding encoding);
```


#### Removed Type SkiaSharp.SK3dView
#### Removed Type SkiaSharp.SKAutoMaskFreeImage
#### Removed Type SkiaSharp.SKCropRectFlags
#### Removed Type SkiaSharp.SKMask
#### Removed Type SkiaSharp.SKMaskFormat
#### Removed Type SkiaSharp.SKMatrix44TypeMask
#### Removed Type SkiaSharp.SKXmlStreamWriter
#### Removed Type SkiaSharp.SKXmlWriter

