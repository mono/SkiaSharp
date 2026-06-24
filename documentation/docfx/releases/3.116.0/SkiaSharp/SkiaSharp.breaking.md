# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContext

Removed methods:

```csharp
[Obsolete ("Use CreateGl() instead.")]
public static GRContext Create (GRBackend backend);

[Obsolete ("Use CreateGl(GRGlInterface) instead.")]
public static GRContext Create (GRBackend backend, GRGlInterface backendContext);

[Obsolete ("Use CreateGl(GRGlInterface) instead.")]
public static GRContext Create (GRBackend backend, IntPtr backendContext);

[Obsolete]
public int GetRecommendedSampleCount (GRPixelConfig config, float dpi);

[Obsolete ("Use GetResourceCacheLimit() instead.")]
public void GetResourceCacheLimits (out int maxResources, out long maxResourceBytes);

[Obsolete ("Use SetResourceCacheLimit(long) instead.")]
public void SetResourceCacheLimits (int maxResources, long maxResourceBytes);
```


#### Type Changed: SkiaSharp.GRGlInterface

Removed methods:

```csharp
[Obsolete ("Use CreateAngle(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleAngleInterface (GRGlGetProcDelegate get);

[Obsolete ("Use CreateAngle(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleAngleInterface (object context, GRGlGetProcDelegate get);

[Obsolete ("Use CreateOpenGl(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleGlInterface (GRGlGetProcDelegate get);

[Obsolete ("Use CreateOpenGl(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleGlInterface (object context, GRGlGetProcDelegate get);

[Obsolete ("Use CreateGles(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleGlesInterface (GRGlGetProcDelegate get);

[Obsolete ("Use CreateGles(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleGlesInterface (object context, GRGlGetProcDelegate get);

[Obsolete ("Use Create(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleInterface (GRGlGetProcDelegate get);

[Obsolete ("Use Create(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleInterface (object context, GRGlGetProcDelegate get);

[Obsolete ("Use Create() instead.")]
public static GRGlInterface CreateDefaultInterface ();

[Obsolete ("Use Create() instead.")]
public static GRGlInterface CreateNativeAngleInterface ();

[Obsolete ("Use CreateEvas(IntPtr) instead.")]
public static GRGlInterface CreateNativeEvasInterface (IntPtr evas);

[Obsolete ("Use Create() instead.")]
public static GRGlInterface CreateNativeGlInterface ();
```


#### Type Changed: SkiaSharp.SKBitmap

Removed methods:

```csharp
[Obsolete ("Use GetAddress instead.")]
public IntPtr GetAddr (int x, int y);

[Obsolete]
public ushort GetAddr16 (int x, int y);

[Obsolete]
public uint GetAddr32 (int x, int y);

[Obsolete]
public byte GetAddr8 (int x, int y);

[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixel(int, int) instead.")]
public SKPMColor GetIndex8Color (int x, int y);
public System.ReadOnlySpan<byte> GetPixelSpan ();
public bool InstallMaskPixels (SKMask mask);

[Obsolete ("The Index8 color type and color table is no longer supported. Use InstallPixels(SKImageInfo, IntPtr, int) instead.")]
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);

[Obsolete ("The Index8 color type and color table is no longer supported. Use InstallPixels(SKImageInfo, IntPtr, int, SKBitmapReleaseDelegate, object) instead.")]
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable, SKBitmapReleaseDelegate releaseProc, object context);

[Obsolete ("Use ScalePixels(SKBitmap, SKFilterQuality) instead.")]
public bool Resize (SKBitmap dst, SKBitmapResizeMethod method);

[Obsolete ("Use Resize(SKImageInfo, SKFilterQuality) instead.")]
public SKBitmap Resize (SKImageInfo info, SKBitmapResizeMethod method);

[Obsolete ("Use ScalePixels(SKBitmap, SKFilterQuality) instead.")]
public static bool Resize (SKBitmap dst, SKBitmap src, SKBitmapResizeMethod method);

[Obsolete ("The Index8 color type and color table is no longer supported.")]
public void SetColorTable (SKColorTable ct);

[Obsolete ("The Index8 color type and color table is no longer supported. Use SetPixels(IntPtr) instead.")]
public void SetPixels (IntPtr pixels, SKColorTable ct);
```


#### Type Changed: SkiaSharp.SKCanvas

Removed methods:

```csharp
public void Concat (ref SKMatrix m);
public void DrawDrawable (SKDrawable drawable, ref SKMatrix matrix);
public void DrawPicture (SKPicture picture, ref SKMatrix matrix, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawPositionedText (byte[] text, SKPoint[] points, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawPositionedText (string text, SKPoint[] points, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawPositionedText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawText (byte[] text, SKPoint p, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawText (byte[] text, float x, float y, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawText (IntPtr buffer, int length, SKPoint p, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawText (IntPtr buffer, int length, float x, float y, SKPaint paint);

[Obsolete ("Use DrawTextOnPath(string, SKPath, SKPoint, SKPaint) instead.")]
public void DrawTextOnPath (byte[] text, SKPath path, SKPoint offset, SKPaint paint);

[Obsolete ("Use DrawTextOnPath(string, SKPath, float, float, SKPaint) instead.")]
public void DrawTextOnPath (byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete ("Use DrawTextOnPath(string, SKPath, SKPoint, SKPaint) instead.")]
public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, SKPoint offset, SKPaint paint);

[Obsolete ("Use DrawTextOnPath(string, SKPath, float, float, SKPaint) instead.")]
public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKCodec

Removed methods:

```csharp
[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr) instead.")]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKColorTable colorTable, ref int colorTableCount);

[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr) instead.")]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, IntPtr colorTable, ref int colorTableCount);

[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, SKCodecOptions) instead.")]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);

[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, SKCodecOptions) instead.")]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);

[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);

[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);

[Obsolete ("The Index8 color type and color table is no longer supported. Use StartIncrementalDecode(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);

[Obsolete ("The Index8 color type and color table is no longer supported. Use StartIncrementalDecode(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);

[Obsolete ("The Index8 color type and color table is no longer supported. Use StartScanlineDecode(SKImageInfo, SKCodecOptions) instead.")]
public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);

[Obsolete ("The Index8 color type and color table is no longer supported. Use StartScanlineDecode(SKImageInfo, SKCodecOptions) instead.")]
public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
```


#### Type Changed: SkiaSharp.SKColorSpace

Removed methods:

```csharp
[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKColorSpaceGamut gamut);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKMatrix44 toXyzD50);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);

[Obsolete]
public SKMatrix44 FromXyzD50 ();

[Obsolete ("Use ToColorSpaceXyz() instead.")]
public SKMatrix44 ToXyzD50 ();

[Obsolete ("Use ToColorSpaceXyz(out SKColorSpaceXyz) instead.")]
public bool ToXyzD50 (SKMatrix44 toXyzD50);
```


#### Type Changed: SkiaSharp.SKColorSpacePrimaries

Removed methods:

```csharp
[Obsolete ("Use ToColorSpaceXyz() instead.")]
public SKMatrix44 ToXyzD50 ();

[Obsolete ("Use ToColorSpaceXyz(out SKColorSpaceXyz) instead.")]
public bool ToXyzD50 (SKMatrix44 toXyzD50);
```


#### Type Changed: SkiaSharp.SKDocument

Removed method:

```csharp
[Obsolete ("Use CreatePdf(SKWStream, SKDocumentPdfMetadata) instead.")]
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

[Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc);

[Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc);

[Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);

[Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);

[Obsolete ("The Index8 color type and color table is no longer supported. Use FromPixelCopy(SKImageInfo, IntPtr, int) instead.")]
public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);

[Obsolete ("Use FromPixels (SKImageInfo, SKData, int) instead.")]
public static SKImage FromPixelData (SKImageInfo info, SKData data, int rowBytes);

[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc);

[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc);

[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);

[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);

[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate) instead.")]
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);

[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate) instead.")]
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);

[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate, object) instead.")]
public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);

[Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate, object) instead.")]
public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);

[Obsolete ("Use ToTextureImage(GRContext) instead.")]
public SKImage ToTextureImage (GRContext context, SKColorSpace colorspace);
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed methods:

```csharp
public static SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold);
public static SKImageFilter CreateAlphaThreshold (SKRectI region, float innerThreshold, float outerThreshold, SKImageFilter input);
public static SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold, SKImageFilter input);
public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter background, SKImageFilter foreground, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDilate (int radiusX, int radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);

[Obsolete ("Use CreateDisplacementMapEffect(SKColorChannel, SKColorChannel, float, SKImageFilter, SKImageFilter, SKImageFilter.CropRect) instead.")]
public static SKImageFilter CreateDisplacementMapEffect (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKImageFilter.CropRect cropRect);

[Obsolete ("Use CreateDropShadow or CreateDropShadowOnly instead.")]
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateErode (int radiusX, int radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMagnifier (SKRect src, float inset);
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input);
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, System.ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);

[Obsolete ("Use CreateMatrixConvolution(SKSizeI, ReadOnlySpan<float>, float, float, SKPointI, SKShaderTileMode, bool, SKImageFilter, SKImageFilter.CropRect) instead.")]
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (System.ReadOnlySpan<SKImageFilter> filters, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKImageFilter.CropRect cropRect);

[Obsolete ("Use CreateMerge(ReadOnlySpan<SKImageFilter>, SKImageFilter.CropRect) instead.")]
public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKBlendMode[] modes, SKImageFilter.CropRect cropRect);

[Obsolete ("Use CreateMerge(SKImageFilter, SKImageFilter, SKImageFilter.CropRect) instead.")]
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKBlendMode mode, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateOffset (float dx, float dy, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePaint (SKPaint paint, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateShader (SKShader shader, bool dither, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
```

Modified methods:

```diff
 public SKImageFilter CreateMatrix (SKMatrix matrix, SKFilterQuality quality, SKImageFilter input--- = NULL---)
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
[Obsolete ("Use CreateBlur(SKBlurStyle, float) instead.")]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags);

[Obsolete ("Use CreateBlur(SKBlurStyle, float) instead.")]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder);

[Obsolete ("Use CreateBlur(SKBlurStyle, float) instead.")]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags);

[Obsolete ("Use CreateBlur(SKBlurStyle, float, bool) instead.")]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM);
```


#### Type Changed: SkiaSharp.SKMatrix

Removed methods:

```csharp
public static void Concat (ref SKMatrix target, ref SKMatrix first, ref SKMatrix second);

[Obsolete ("Use CreateIdentity() instead.")]
public static SKMatrix MakeIdentity ();

[Obsolete ("Use CreateRotation(float) instead.")]
public static SKMatrix MakeRotation (float radians);

[Obsolete ("Use CreateRotation(float, float, float) instead.")]
public static SKMatrix MakeRotation (float radians, float pivotx, float pivoty);

[Obsolete ("Use CreateRotationDegrees(float) instead.")]
public static SKMatrix MakeRotationDegrees (float degrees);

[Obsolete ("Use CreateRotationDegrees(float, float, float) instead.")]
public static SKMatrix MakeRotationDegrees (float degrees, float pivotx, float pivoty);

[Obsolete ("Use CreateScale(float, float) instead.")]
public static SKMatrix MakeScale (float sx, float sy);

[Obsolete ("Use CreateScale(float, float, float, float) instead.")]
public static SKMatrix MakeScale (float sx, float sy, float pivotX, float pivotY);

[Obsolete ("Use CreateSkew(float, float) instead.")]
public static SKMatrix MakeSkew (float sx, float sy);

[Obsolete ("Use CreateTranslation(float, float) instead.")]
public static SKMatrix MakeTranslation (float dx, float dy);

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

[Obsolete ("Use CreateScaleTranslation(float, float, float, float) instead.")]
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
[Obsolete ("Use GetFontMetrics (out SKFontMetrics) instead.")]
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

[Obsolete ("Use AddRoundRect instead.")]
public void AddRoundedRect (SKRect rect, float rx, float ry, SKPathDirection dir);
```

#### Type Changed: SkiaSharp.SKPath.Iterator

Removed method:

```csharp
[Obsolete ("Use Next(SKPoint[]) instead.")]
public SKPathVerb Next (SKPoint[] points, bool doConsumeDegenerates, bool exact);
```



#### Type Changed: SkiaSharp.SKPixmap

Removed methods:

```csharp
[Obsolete ("Use Encode(SKWStream, SKJpegEncoderOptions) instead.")]
public static bool Encode (SKWStream dst, SKPixmap src, SKJpegEncoderOptions options);

[Obsolete ("Use Encode(SKWStream, SKPngEncoderOptions) instead.")]
public static bool Encode (SKWStream dst, SKPixmap src, SKPngEncoderOptions options);

[Obsolete ("Use Encode(SKWStream, SKWebpEncoderOptions) instead.")]
public static bool Encode (SKWStream dst, SKPixmap src, SKWebpEncoderOptions options);

[Obsolete ("Use Encode(SKWStream, SKEncodedImageFormat, int) instead.")]
public static bool Encode (SKWStream dst, SKBitmap src, SKEncodedImageFormat format, int quality);

[Obsolete ("Use Encode(SKWStream, SKEncodedImageFormat, int) instead.")]
public static bool Encode (SKWStream dst, SKPixmap src, SKEncodedImageFormat encoder, int quality);
public bool Erase (SKColorF color, SKColorSpace colorspace, SKRectI subset);
public System.ReadOnlySpan<byte> GetPixelSpan ();

[Obsolete ("Use ReadPixels(SKImageInfo, IntPtr, int, int, int) instead.")]
public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKTransferFunctionBehavior behavior);

[Obsolete ("The Index8 color type and color table is no longer supported. Use Reset(SKImageInfo, IntPtr, int) instead.")]
public void Reset (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable);

[Obsolete ("Use ScalePixels(SKPixmap, SKFilterQuality) instead.")]
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
[Obsolete ("Use Create(GRContext, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType) instead.")]
public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
public static SKSurface Create (GRContext context, GRBackendTextureDesc desc);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc);

[Obsolete ("Use Create(SKImageInfo, SKSurfaceProperties) instead.")]
public static SKSurface Create (SKImageInfo info, SKSurfaceProps props);

[Obsolete ("Use Create(SKPixmap, SKSurfaceProperties) instead.")]
public static SKSurface Create (SKPixmap pixmap, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface Create (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete ("Use Create(SKImageInfo, IntPtr, rowBytes, SKSurfaceProperties) instead.")]
public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props);

[Obsolete ("Use Create(SKImageInfo) instead.")]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType);

[Obsolete ("Use Create(GRContext, bool, SKImageInfo, int, SKSurfaceProperties) instead.")]
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProps props);

[Obsolete ("Use Create(SKImageInfo, SKSurfaceProperties) instead.")]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props);

[Obsolete ("Use Create(SKImageInfo, IntPtr, int) instead.")]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes);

[Obsolete ("Use Create(SKImageInfo, IntPtr, int, SKSurfaceProperties) instead.")]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc);

[Obsolete ("Use Create(GRContext, GRBackendTexture, SKColorType) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);

[Obsolete ("Use Create(GRContext, GRBackendTexture, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKColorSpace) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKColorSpace, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
```


#### Type Changed: SkiaSharp.SKSvgCanvas

Removed method:

```csharp
[Obsolete ("Use Create(SKRect, Stream) instead.")]
public static SKCanvas Create (SKRect bounds, SKXmlWriter writer);
```


#### Type Changed: SkiaSharp.SKTextBlobBuilder

Removed methods:

```csharp
[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, SKRect? bounds);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, SKRect bounds);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, SKRect? bounds);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, SKRect bounds);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, SKRect? bounds);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, SKRect bounds);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y);

[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount);

[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, SKRect? bounds);

[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount, SKRect? bounds);

[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count);

[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount);

[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, SKRect? bounds);

[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount, SKRect? bounds);
public SKRotationScaleRunBuffer AllocateRotationScaleRun (SKFont font, int count);

[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y);

[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount);

[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, SKRect? bounds);

[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount, SKRect? bounds);
```


#### Type Changed: SkiaSharp.SKTypeface

Removed methods:

```csharp
[Obsolete ("Use GetGlyphs(string, out ushort[]) instead.")]
public int CharsToGlyphs (string chars, out ushort[] glyphs);

[Obsolete ("Use GetGlyphs(IntPtr, int, SKTextEncoding, out ushort[]) instead.")]
public int CharsToGlyphs (IntPtr str, int strlen, SKEncoding encoding, out ushort[] glyphs);

[Obsolete ("Use CountGlyphs(byte[], SKTextEncoding) instead.")]
public int CountGlyphs (byte[] str, SKEncoding encoding);

[Obsolete ("Use CountGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
public int CountGlyphs (System.ReadOnlySpan<byte> str, SKEncoding encoding);

[Obsolete ("Use CountGlyphs(string) instead.")]
public int CountGlyphs (string str, SKEncoding encoding);

[Obsolete ("Use CountGlyphs(IntPtr, int, SKTextEncoding) instead.")]
public int CountGlyphs (IntPtr str, int strLen, SKEncoding encoding);

[Obsolete ("Use FromFamilyName(string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant) instead.")]
public static SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style);

[Obsolete]
public static SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style);

[Obsolete ("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
public ushort[] GetGlyphs (byte[] text, SKEncoding encoding);

[Obsolete ("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
public ushort[] GetGlyphs (System.ReadOnlySpan<byte> text, SKEncoding encoding);

[Obsolete ("Use GetGlyphs(string) instead.")]
public ushort[] GetGlyphs (string text, SKEncoding encoding);

[Obsolete ("Use GetGlyphs(string) instead.")]
public int GetGlyphs (string text, out ushort[] glyphs);

[Obsolete ("Use GetGlyphs(byte[], SKTextEncoding) instead.")]
public int GetGlyphs (byte[] text, SKEncoding encoding, out ushort[] glyphs);

[Obsolete ("Use GetGlyphs(IntPtr, int, SKTextEncoding) instead.")]
public ushort[] GetGlyphs (IntPtr text, int length, SKEncoding encoding);

[Obsolete ("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
public int GetGlyphs (System.ReadOnlySpan<byte> text, SKEncoding encoding, out ushort[] glyphs);

[Obsolete ("Use GetGlyphs(string) instead.")]
public int GetGlyphs (string text, SKEncoding encoding, out ushort[] glyphs);

[Obsolete ("Use GetGlyphs(IntPtr, int, SKTextEncoding) instead.")]
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
[Obsolete ("Use SKColorChannel instead.")]
public static SKColorChannel ToColorChannel (this SKDisplacementMapEffectChannelSelectorType channelSelectorType);

[Obsolete]
public static SKColorSpaceTransferFn ToColorSpaceTransferFn (this SKColorSpaceRenderTargetGamma gamma);

[Obsolete]
public static SKColorSpaceTransferFn ToColorSpaceTransferFn (this SKNamedGamma gamma);

[Obsolete]
public static SKColorSpaceXyz ToColorSpaceXyz (this SKColorSpaceGamut gamut);

[Obsolete]
public static SKColorSpaceXyz ToColorSpaceXyz (this SKMatrix44 matrix);

[Obsolete ("Use SKColorType instead.")]
public static SKColorType ToColorType (this GRPixelConfig config);

[Obsolete]
public static SKFilterQuality ToFilterQuality (this SKBitmapResizeMethod method);

[Obsolete ("Use SKColorType instead.")]
public static uint ToGlSizedFormat (this GRPixelConfig config);

[Obsolete ("Use SKColorType instead.")]
public static GRPixelConfig ToPixelConfig (this SKColorType colorType);

[Obsolete ("Use SKShaderTileMode instead.")]
public static SKShaderTileMode ToShaderTileMode (this SKMatrixConvolutionTileMode tileMode);

[Obsolete]
public static SKTextEncoding ToTextEncoding (this SKEncoding encoding);
```


#### Type Changed: SkiaSharp.StringUtilities

Removed method:

```csharp
[Obsolete ("Use GetEncodedText(string, SKTextEncoding) instead.")]
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

