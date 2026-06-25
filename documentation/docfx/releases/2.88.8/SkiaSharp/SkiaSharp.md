# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKImageFilter

Modified methods:

```diff
 public SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold, SKImageFilter input--- = NULL---)
 public SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter background, SKImageFilter foreground--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateDilate (int radiusX, int radiusY, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateErode (int radiusX, int radiusY, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateMerge (SKImageFilter[] filters, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateOffset (float dx, float dy, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreatePaint (SKPaint paint, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
 public SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input--- = NULL---, SKImageFilter.CropRect cropRect--- = NULL---)
```

Added methods:

```csharp
public static SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold);
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
public static SKImageFilter CreateMagnifier (SKRect src, float inset);
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input);
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateMatrix (SKMatrix matrix);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, System.ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, System.ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, System.ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, System.ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateMerge (System.ReadOnlySpan<SKImageFilter> filters);
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second);
public static SKImageFilter CreateMerge (System.ReadOnlySpan<SKImageFilter> filters, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (System.ReadOnlySpan<SKImageFilter> filters, SKRect cropRect);
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKRect cropRect);
public static SKImageFilter CreateOffset (float radiusX, float radiusY);
public static SKImageFilter CreateOffset (float radiusX, float radiusY, SKImageFilter input);
public static SKImageFilter CreateOffset (float radiusX, float radiusY, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreatePaint (SKPaint paint);
public static SKImageFilter CreatePaint (SKPaint paint, SKRect cropRect);
public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd);
public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input);
public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess);
public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input);
public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateShader (SKShader shader);
public static SKImageFilter CreateShader (SKShader shader, bool dither);
public static SKImageFilter CreateShader (SKShader shader, bool dither, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateShader (SKShader shader, bool dither, SKRect cropRect);
public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd);
public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input);
public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess);
public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input);
public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateTile (SKRect src, SKRect dst);
```



