# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.58.0.0 vs 1.57.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRContextOptions

Removed properties:

```csharp
public bool ClipBatchToBounds { get; set; }
public bool DrawBatchBounds { get; set; }
public bool ForceSWPathMasks { get; set; }
public int MaxBatchLookahead { get; set; }
public int MaxBatchLookback { get; set; }
```

Added properties:

```csharp
public bool ClipDrawOpsToBounds { get; set; }
public bool ForceSoftwarePathMasks { get; set; }
public int MaxOpCombineLookahead { get; set; }
public int MaxOpCombineLookback { get; set; }
public bool RequireDecodeDisableForSrgb { get; set; }
```


#### Type Changed: SkiaSharp.GRPixelConfig

Removed values:

```csharp
Astc12x12 = 12,
Latc = 10,
R11Eac = 11,
```

Modified fields:

```diff
-AlphaHalf = 14
+AlphaHalf = 12
-RgbaFloat = 13
+RgbaFloat = 10
-RgbaHalf = 15
+RgbaHalf = 13
```

Added value:

```csharp
RgFloat = 11,
```


#### Type Changed: SkiaSharp.SKBitmap

Added property:

```csharp
public SKColorSpace ColorSpace { get; }
```


#### Type Changed: SkiaSharp.SKCanvas

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKRect ClipBounds { get; }
 [Obsolete ()]
 public SKRectI ClipDeviceBounds { get; }
```

Added properties:

```csharp
public SKRectI DeviceClipBounds { get; }
public SKRect LocalClipBounds { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public bool GetClipBounds (ref SKRect bounds);
 [Obsolete ()]
 public bool GetClipDeviceBounds (ref SKRectI bounds);
```

Added methods:

```csharp
public bool GetDeviceClipBounds (out SKRectI bounds);
public bool GetLocalClipBounds (out SKRect bounds);
```


#### Type Changed: SkiaSharp.SKColorFilter

Removed fields:

```csharp
public static const int MaxColorCubeDimension;

[Obsolete]
public static const int MaxCubeSize;
public static const int MinColorCubeDimension;

[Obsolete]
public static const int MinCubeSize;
```

Removed methods:

```csharp
public static SKColorFilter CreateColorCube (SKData cubeData, int cubeDimension);
public static SKColorFilter CreateColorCube (byte[] cubeData, int cubeDimension);
public static SKColorFilter CreateGamma (float gamma);
public static bool IsValid3DColorCube (SKData cubeData, int cubeDimension);
```

Added methods:

```csharp
public static SKColorFilter CreateHighContrast (SKHighContrastConfig config);
public static SKColorFilter CreateHighContrast (bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast);
```


#### Type Changed: SkiaSharp.SKData

Added methods:

```csharp
public static SKData Create (SKStream stream);
public static SKData Create (System.IO.Stream stream);
public static SKData Create (string filename);
public static SKData Create (SKStream stream, int length);
public static SKData Create (SKStream stream, long length);
public static SKData Create (SKStream stream, ulong length);
public static SKData Create (System.IO.Stream stream, int length);
public static SKData Create (System.IO.Stream stream, long length);
public static SKData Create (System.IO.Stream stream, ulong length);
```


#### Type Changed: SkiaSharp.SKImageEncodeFormat

Modified fields:

```diff
-Bmp = 0
+Bmp = 1
-Gif = 1
+Gif = 2
-Ico = 2
+Ico = 3
-Jpeg = 3
+Jpeg = 4
-Png = 4
+Png = 5
-Wbmp = 5
+Wbmp = 6
-Webp = 6
+Webp = 7
```

Added value:

```csharp
Unknown = 0,
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input);
```

Added methods:

```csharp
public static SKImageFilter CreateImage (SKImage image);
public static SKImageFilter CreateImage (SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality);
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePaint (SKPaint paint, SKImageFilter.CropRect cropRect);
```


#### Type Changed: SkiaSharp.SKImageInfo

Added constructor:

```csharp
public SKImageInfo (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace);
```

Added property:

```csharp
public SKColorSpace ColorSpace { get; set; }
```


#### Type Changed: SkiaSharp.SKPaint

Removed properties:

```csharp
public bool StrikeThruText { get; set; }
public bool UnderlineText { get; set; }
```


#### Type Changed: SkiaSharp.SKPath

Added method:

```csharp
public SKRect ComputeTightBounds ();
```


#### Type Changed: SkiaSharp.SKPathEffect

Added method:

```csharp
public static SKPathEffect CreateArcTo (float radius);
```


#### Type Changed: SkiaSharp.SKPixmap

Added property:

```csharp
public SKColorSpace ColorSpace { get; }
```


#### Type Changed: SkiaSharp.SKSurface

Added methods:

```csharp
public static SKSurface Create (SKPixmap pixmap);
public static SKSurface Create (SKPixmap pixmap, SKSurfaceProps props);
```


#### New Type: SkiaSharp.SKColorSpace

```csharp
public class SKColorSpace : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public bool GammaIsCloseToSrgb { get; }
	public bool GammaIsLinear { get; }
	// methods
	public static SKMatrix44 ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries);
	public static bool ConvertPrimariesToXyzD50 (SKColorSpacePrimaries primaries, SKMatrix44 toXyzD50);
	public static SKColorSpace CreateIcc (byte[] input);
	public static SKColorSpace CreateIcc (byte[] input, long length);
	public static SKColorSpace CreateIcc (IntPtr input, long length);
	public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);
	public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);
	public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);
	public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);
	public static SKColorSpace CreateSrgb ();
	public static SKColorSpace CreateSrgbLinear ();
	protected override void Dispose (bool disposing);
	public static bool Equal (SKColorSpace left, SKColorSpace right);
	public SKMatrix44 ToXyzD50 ();
	public bool ToXyzD50 (SKMatrix44 toXyzD50);
}
```

#### New Type: SkiaSharp.SKColorSpaceFlags

```csharp
[Serializable]
public enum SKColorSpaceFlags {
	NonLinearBlending = 1,
}
```

#### New Type: SkiaSharp.SKColorSpaceGamut

```csharp
[Serializable]
public enum SKColorSpaceGamut {
	AdobeRgb = 1,
	Dcip3D65 = 2,
	Rec2020 = 3,
	Srgb = 0,
}
```

#### New Type: SkiaSharp.SKColorSpacePrimaries

```csharp
public struct SKColorSpacePrimaries {
	// constructors
	public SKColorSpacePrimaries (float rx, float ry, float gx, float gy, float bx, float by, float wx, float wy);
	// properties
	public float BX { get; set; }
	public float BY { get; set; }
	public float GX { get; set; }
	public float GY { get; set; }
	public float RX { get; set; }
	public float RY { get; set; }
	public float WX { get; set; }
	public float WY { get; set; }
}
```

#### New Type: SkiaSharp.SKColorSpaceRenderTargetGamma

```csharp
[Serializable]
public enum SKColorSpaceRenderTargetGamma {
	Linear = 0,
	Srgb = 1,
}
```

#### New Type: SkiaSharp.SKColorSpaceTransferFn

```csharp
public struct SKColorSpaceTransferFn {
	// constructors
	public SKColorSpaceTransferFn (float g, float a, float b, float c, float d, float e, float f);
	// properties
	public float A { get; set; }
	public float B { get; set; }
	public float C { get; set; }
	public float D { get; set; }
	public float E { get; set; }
	public float F { get; set; }
	public float G { get; set; }
}
```

#### New Type: SkiaSharp.SKHighContrastConfig

```csharp
public struct SKHighContrastConfig {
	// constructors
	public SKHighContrastConfig (bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast);
	// fields
	public static SKHighContrastConfig Default;
	// properties
	public float Contrast { get; set; }
	public bool Grayscale { get; set; }
	public SKHighContrastConfigInvertStyle InvertStyle { get; set; }
	public bool IsValid { get; }
}
```

#### New Type: SkiaSharp.SKHighContrastConfigInvertStyle

```csharp
[Serializable]
public enum SKHighContrastConfigInvertStyle {
	InvertBrightness = 1,
	InvertLightness = 2,
	NoInvert = 0,
}
```


