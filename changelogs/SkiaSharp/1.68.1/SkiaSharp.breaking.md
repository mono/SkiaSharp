# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKColorSpace

Modified methods:

```diff
 public SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags--- = 0---)
 public SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags--- = 0---)
 public SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags--- = 0---)
 public SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags--- = 0---)
```



