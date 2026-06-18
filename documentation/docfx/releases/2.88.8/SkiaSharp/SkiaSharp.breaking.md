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



