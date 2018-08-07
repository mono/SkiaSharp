# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.49.0.0 vs 0.0.0.0

### New Namespace SkiaSharp

#### New Type: SkiaSharp.SKAlphaType

```csharp
[Serializable]
public enum SKAlphaType {
	Opaque = 0,
	Premul = 1,
	Unpremul = 2,
}
```

#### New Type: SkiaSharp.SKAutoCanvasRestore

```csharp
public class SKAutoCanvasRestore : System.IDisposable {
	// constructors
	public SKAutoCanvasRestore (SKCanvas canvas, bool doSave);
	// methods
	public virtual void Dispose ();
	public void Restore ();
}
```

#### New Type: SkiaSharp.SKBitmap

```csharp
public class SKBitmap : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKBitmap ();
	public SKBitmap (SKImageInfo info);
	public SKBitmap (SKImageInfo info, int rowBytes);
	public SKBitmap (int width, int height, bool isOpaque);
	public SKBitmap (int width, int height, SKColorType colorType, SKAlphaType alphaType);
	// properties
	public SKAlphaType AlphaType { get; }
	public int ByteCount { get; }
	public byte[] Bytes { get; }
	public int BytesPerPixel { get; }
	public SKColorType ColorType { get; }
	public bool DrawsNothing { get; }
	public int Height { get; }
	public SKImageInfo Info { get; }
	public bool IsEmpty { get; }
	public bool IsImmutable { get; }
	public bool IsNull { get; }
	public bool IsVolatile { get; set; }
	public SKColor[] Pixels { get; set; }
	public int RowBytes { get; }
	public int Width { get; }
	// methods
	public bool CanCopyTo (SKColorType colorType);
	public SKBitmap Copy ();
	public SKBitmap Copy (SKColorType colorType);
	public bool CopyTo (SKBitmap destination);
	public bool CopyTo (SKBitmap destination, SKColorType colorType);
	public static SKBitmap Decode (SKStreamRewindable stream, SKColorType pref);
	public static SKBitmap Decode (byte[] buffer, SKColorType pref);
	public static SKBitmap Decode (string filename, SKColorType pref);
	public static SKImageInfo DecodeBounds (SKStreamRewindable stream, SKColorType pref);
	public static SKImageInfo DecodeBounds (byte[] buffer, SKColorType pref);
	public static SKImageInfo DecodeBounds (string filename, SKColorType pref);
	protected override void Dispose (bool disposing);
	public void Erase (SKColor color);
	public void Erase (SKColor color, SKRectI rect);
	public SKColor GetPixel (int x, int y);
	public IntPtr GetPixels (out IntPtr length);
	public void LockPixels ();
	public void Reset ();
	public void SetImmutable ();
	public void SetPixel (int x, int y, SKColor color);
	public void UnlockPixels ();
}
```

#### New Type: SkiaSharp.SKBlurStyle

```csharp
[Serializable]
public enum SKBlurStyle {
	Inner = 3,
	Normal = 0,
	Outer = 2,
	Solid = 1,
}
```

#### New Type: SkiaSharp.SKCanvas

```csharp
public class SKCanvas : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public int SaveCount { get; }
	public SKMatrix TotalMatrix { get; }
	// methods
	public void Clear ();
	public void Clear (SKColor color);
	public void ClipPath (SKPath path);
	public void ClipRect (SKRect rect);
	public void Concat (ref SKMatrix m);
	public void DrawBitmap (SKBitmap bitmap, SKRect dest, SKPaint paint);
	public void DrawBitmap (SKBitmap bitmap, SKRect source, SKRect dest, SKPaint paint);
	public void DrawBitmap (SKBitmap bitmap, float x, float y, SKPaint paint);
	public void DrawColor (SKColor color, SKXferMode mode);
	public void DrawImage (SKImage image, SKRect dest, SKPaint paint);
	public void DrawImage (SKImage image, SKRect source, SKRect dest, SKPaint paint);
	public void DrawImage (SKImage image, float x, float y, SKPaint paint);
	public void DrawLine (float x0, float y0, float x1, float y1, SKPaint paint);
	public void DrawOval (SKRect rect, SKPaint paint);
	public void DrawPaint (SKPaint paint);
	public void DrawPath (SKPath path, SKPaint paint);
	public void DrawPicture (SKPicture picture, SKPaint paint);
	public void DrawPicture (SKPicture picture, ref SKMatrix matrix, SKPaint paint);
	public void DrawPoint (float x, float y, SKColor color);
	public void DrawPoint (float x, float y, SKPaint paint);
	public void DrawPoints (SKPointMode mode, SKPoint[] points, SKPaint paint);
	public void DrawRect (SKRect rect, SKPaint paint);
	public void DrawText (string text, SKPoint[] points, SKPaint paint);
	public void DrawText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint);
	public void DrawText (string text, float x, float y, SKPaint paint);
	public void DrawText (IntPtr buffer, int length, float x, float y, SKPaint paint);
	public void DrawText (string text, SKPath path, float hOffset, float vOffset, SKPaint paint);
	public void DrawText (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint);
	public void ResetMatrix ();
	public void Restore ();
	public void RestoreToCount (int count);
	public void RotateDegrees (float degrees);
	public void RotateRadians (float radians);
	public void Save ();
	public void SaveLayer (SKPaint paint);
	public void SaveLayer (SKRect limit, SKPaint paint);
	public void Scale (SKPoint size);
	public void Scale (float sx, float sy);
	public void SetMatrix (SKMatrix matrix);
	public void Skew (SKPoint skew);
	public void Skew (float sx, float sy);
	public void Translate (SKPoint point);
	public void Translate (float dx, float dy);
}
```

#### New Type: SkiaSharp.SKClipType

```csharp
[Serializable]
public enum SKClipType {
	Difference = 1,
	Intersect = 0,
}
```

#### New Type: SkiaSharp.SKColor

```csharp
public struct SKColor {
	// constructors
	public SKColor (byte red, byte green, byte blue);
	public SKColor (byte red, byte green, byte blue, byte alpha);
	// properties
	public byte Alpha { get; }
	public byte Blue { get; }
	public byte Green { get; }
	public byte Red { get; }
	// methods
	public override bool Equals (object other);
	public override int GetHashCode ();
	public override string ToString ();
	public SKColor WithAlpha (byte alpha);
}
```

#### New Type: SkiaSharp.SKColorFilter

```csharp
public class SKColorFilter : SkiaSharp.SKObject, System.IDisposable {
	// fields
	public static const int MaxCubeSize;
	public static const int MinCubeSize;
	// methods
	public static SKColorFilter CreateColorCube (SKData cubeData, int cubeDimension);
	public static SKColorFilter CreateColorCube (byte[] cubeData, int cubeDimension);
	public static SKColorFilter CreateColorMatrix (float[] matrix);
	public static SKColorFilter CreateCompose (SKColorFilter outer, SKColorFilter inner);
	public static SKColorFilter CreateLighting (SKColor mul, SKColor add);
	public static SKColorFilter CreateLumaColor ();
	public static SKColorFilter CreateTable (byte[] table);
	public static SKColorFilter CreateTable (byte[] tableA, byte[] tableR, byte[] tableG, byte[] tableB);
	public static SKColorFilter CreateXferMode (SKColor c, SKXferMode mode);
	protected override void Dispose (bool disposing);
	public static bool IsValid3DColorCube (SKData cubeData, int cubeDimension);
}
```

#### New Type: SkiaSharp.SKColorProfileType

```csharp
[Serializable]
public enum SKColorProfileType {
	Linear = 0,
	SRGB = 1,
}
```

#### New Type: SkiaSharp.SKColorType

```csharp
[Serializable]
public enum SKColorType {
	Alpha_8 = 3,
	Bgra_8888 = 2,
	N_32 = 5,
	Rgb_565 = 4,
	Rgba_8888 = 1,
	Unknown = 0,
}
```

#### New Type: SkiaSharp.SKColors

```csharp
public struct SKColors {
	// fields
	public static SKColor AliceBlue;
	public static SKColor AntiqueWhite;
	public static SKColor Aqua;
	public static SKColor Aquamarine;
	public static SKColor Azure;
	public static SKColor Beige;
	public static SKColor Bisque;
	public static SKColor Black;
	public static SKColor BlanchedAlmond;
	public static SKColor Blue;
	public static SKColor BlueViolet;
	public static SKColor Brown;
	public static SKColor BurlyWood;
	public static SKColor CadetBlue;
	public static SKColor Chartreuse;
	public static SKColor Chocolate;
	public static SKColor Coral;
	public static SKColor CornflowerBlue;
	public static SKColor Cornsilk;
	public static SKColor Crimson;
	public static SKColor Cyan;
	public static SKColor DarkBlue;
	public static SKColor DarkCyan;
	public static SKColor DarkGoldenrod;
	public static SKColor DarkGray;
	public static SKColor DarkGreen;
	public static SKColor DarkKhaki;
	public static SKColor DarkMagenta;
	public static SKColor DarkOliveGreen;
	public static SKColor DarkOrange;
	public static SKColor DarkOrchid;
	public static SKColor DarkRed;
	public static SKColor DarkSalmon;
	public static SKColor DarkSeaGreen;
	public static SKColor DarkSlateBlue;
	public static SKColor DarkSlateGray;
	public static SKColor DarkTurquoise;
	public static SKColor DarkViolet;
	public static SKColor DeepPink;
	public static SKColor DeepSkyBlue;
	public static SKColor DimGray;
	public static SKColor DodgerBlue;
	public static SKColor Firebrick;
	public static SKColor FloralWhite;
	public static SKColor ForestGreen;
	public static SKColor Fuchsia;
	public static SKColor Gainsboro;
	public static SKColor GhostWhite;
	public static SKColor Gold;
	public static SKColor Goldenrod;
	public static SKColor Gray;
	public static SKColor Green;
	public static SKColor GreenYellow;
	public static SKColor Honeydew;
	public static SKColor HotPink;
	public static SKColor IndianRed;
	public static SKColor Indigo;
	public static SKColor Ivory;
	public static SKColor Khaki;
	public static SKColor Lavender;
	public static SKColor LavenderBlush;
	public static SKColor LawnGreen;
	public static SKColor LemonChiffon;
	public static SKColor LightBlue;
	public static SKColor LightCoral;
	public static SKColor LightCyan;
	public static SKColor LightGoldenrodYellow;
	public static SKColor LightGray;
	public static SKColor LightGreen;
	public static SKColor LightPink;
	public static SKColor LightSalmon;
	public static SKColor LightSeaGreen;
	public static SKColor LightSkyBlue;
	public static SKColor LightSlateGray;
	public static SKColor LightSteelBlue;
	public static SKColor LightYellow;
	public static SKColor Lime;
	public static SKColor LimeGreen;
	public static SKColor Linen;
	public static SKColor Magenta;
	public static SKColor Maroon;
	public static SKColor MediumAquamarine;
	public static SKColor MediumBlue;
	public static SKColor MediumOrchid;
	public static SKColor MediumPurple;
	public static SKColor MediumSeaGreen;
	public static SKColor MediumSlateBlue;
	public static SKColor MediumSpringGreen;
	public static SKColor MediumTurquoise;
	public static SKColor MediumVioletRed;
	public static SKColor MidnightBlue;
	public static SKColor MintCream;
	public static SKColor MistyRose;
	public static SKColor Moccasin;
	public static SKColor NavajoWhite;
	public static SKColor Navy;
	public static SKColor OldLace;
	public static SKColor Olive;
	public static SKColor OliveDrab;
	public static SKColor Orange;
	public static SKColor OrangeRed;
	public static SKColor Orchid;
	public static SKColor PaleGoldenrod;
	public static SKColor PaleGreen;
	public static SKColor PaleTurquoise;
	public static SKColor PaleVioletRed;
	public static SKColor PapayaWhip;
	public static SKColor PeachPuff;
	public static SKColor Peru;
	public static SKColor Pink;
	public static SKColor Plum;
	public static SKColor PowderBlue;
	public static SKColor Purple;
	public static SKColor Red;
	public static SKColor RosyBrown;
	public static SKColor RoyalBlue;
	public static SKColor SaddleBrown;
	public static SKColor Salmon;
	public static SKColor SandyBrown;
	public static SKColor SeaGreen;
	public static SKColor SeaShell;
	public static SKColor Sienna;
	public static SKColor Silver;
	public static SKColor SkyBlue;
	public static SKColor SlateBlue;
	public static SKColor SlateGray;
	public static SKColor Snow;
	public static SKColor SpringGreen;
	public static SKColor SteelBlue;
	public static SKColor Tan;
	public static SKColor Teal;
	public static SKColor Thistle;
	public static SKColor Tomato;
	public static SKColor Transparent;
	public static SKColor Turquoise;
	public static SKColor Violet;
	public static SKColor Wheat;
	public static SKColor White;
	public static SKColor WhiteSmoke;
	public static SKColor Yellow;
	public static SKColor YellowGreen;
	// properties
	public static SKColor Empty { get; }
}
```

#### New Type: SkiaSharp.SKCropRectFlags

```csharp
[Serializable]
[Flags]
public enum SKCropRectFlags {
	HasAll = 15,
	HasHeight = 8,
	HasLeft = 1,
	HasTop = 2,
	HasWidth = 4,
}
```

#### New Type: SkiaSharp.SKData

```csharp
public class SKData : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKData ();
	public SKData (byte[] bytes);
	public SKData (IntPtr bytes, ulong length);
	// properties
	public IntPtr Data { get; }
	public long Size { get; }
	// methods
	public System.IO.Stream AsStream ();
	protected override void Dispose (bool disposing);
	public static SKData FromMallocMemory (IntPtr bytes, ulong length);
	public void SaveTo (System.IO.Stream target);
	public SKData Subset (ulong offset, ulong length);
}
```

#### New Type: SkiaSharp.SKDisplacementMapEffectChannelSelectorType

```csharp
[Serializable]
public enum SKDisplacementMapEffectChannelSelectorType {
	A = 4,
	B = 3,
	G = 2,
	R = 1,
	Unknown = 0,
}
```

#### New Type: SkiaSharp.SKDropShadowImageFilterShadowMode

```csharp
[Serializable]
public enum SKDropShadowImageFilterShadowMode {
	DrawShadowAndForeground = 0,
	DrawShadowOnly = 1,
}
```

#### New Type: SkiaSharp.SKEncoding

```csharp
[Serializable]
public enum SKEncoding {
	Utf16 = 1,
	Utf32 = 2,
	Utf8 = 0,
}
```

#### New Type: SkiaSharp.SKFileStream

```csharp
public class SKFileStream : SkiaSharp.SKStreamAsset, System.IDisposable {
	// constructors
	public SKFileStream (string path);
}
```

#### New Type: SkiaSharp.SKFilterQuality

```csharp
[Serializable]
public enum SKFilterQuality {
	High = 3,
	Low = 1,
	Medium = 2,
	None = 0,
}
```

#### New Type: SkiaSharp.SKImage

```csharp
public class SKImage : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public int Height { get; }
	public uint UniqueId { get; }
	public int Width { get; }
	// methods
	protected override void Dispose (bool disposing);
	public SKData Encode ();
	public SKData Encode (SKImageEncodeFormat format, int quality);
	public static SKImage FromData (SKData data);
	public static SKImage FromData (SKData data, SKRectI subset);
	public static SKImage FromPixels (SKImageInfo info, IntPtr pixels, int rowBytes);
}
```

#### New Type: SkiaSharp.SKImageDecoder

```csharp
public class SKImageDecoder : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKImageDecoder (SKStreamRewindable stream);
	// properties
	public bool DitherImage { get; set; }
	public SKImageDecoderFormat Format { get; }
	public string FormatName { get; }
	public bool PreferQualityOverSpeed { get; set; }
	public bool RequireUnpremultipliedColors { get; set; }
	public int SampleSize { get; set; }
	public bool ShouldCancelDecode { get; }
	public bool SkipWritingZeros { get; set; }
	// methods
	public void CancelDecode ();
	public SKImageDecoderResult Decode (SKStream stream, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode);
	public static bool DecodeFile (string filename, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode);
	public static bool DecodeFile (string filename, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode, ref SKImageDecoderFormat format);
	public static bool DecodeFileBounds (string filename, out SKImageInfo info, SKColorType pref);
	public static bool DecodeFileBounds (string filename, out SKImageInfo info, SKColorType pref, ref SKImageDecoderFormat format);
	public static bool DecodeMemory (byte[] buffer, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode);
	public static bool DecodeMemory (byte[] buffer, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode, ref SKImageDecoderFormat format);
	public static bool DecodeMemoryBounds (byte[] buffer, out SKImageInfo info, SKColorType pref);
	public static bool DecodeMemoryBounds (byte[] buffer, out SKImageInfo info, SKColorType pref, ref SKImageDecoderFormat format);
	public static bool DecodeStream (SKStreamRewindable stream, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode);
	public static bool DecodeStream (SKStreamRewindable stream, SKBitmap bitmap, SKColorType pref, SKImageDecoderMode mode, ref SKImageDecoderFormat format);
	public static bool DecodeStreamBounds (SKStreamRewindable stream, out SKImageInfo info, SKColorType pref);
	public static bool DecodeStreamBounds (SKStreamRewindable stream, out SKImageInfo info, SKColorType pref, ref SKImageDecoderFormat format);
	protected override void Dispose (bool disposing);
	public static SKImageDecoderFormat GetFormat (SKStreamRewindable stream);
	public static string GetFormatName (SKImageDecoderFormat format);
}
```

#### New Type: SkiaSharp.SKImageDecoderFormat

```csharp
[Serializable]
public enum SKImageDecoderFormat {
	Astc = 10,
	Bmp = 1,
	Gif = 2,
	Ico = 3,
	Jpeg = 4,
	Ktx = 9,
	Pkm = 8,
	Png = 5,
	Unknown = 0,
	Wbmp = 6,
	Webp = 7,
}
```

#### New Type: SkiaSharp.SKImageDecoderMode

```csharp
[Serializable]
public enum SKImageDecoderMode {
	DecodeBounds = 0,
	DecodePixels = 1,
}
```

#### New Type: SkiaSharp.SKImageDecoderResult

```csharp
[Serializable]
public enum SKImageDecoderResult {
	Failure = 0,
	PartialSuccess = 1,
	Success = 2,
}
```

#### New Type: SkiaSharp.SKImageEncodeFormat

```csharp
[Serializable]
public enum SKImageEncodeFormat {
	Bmp = 1,
	Gif = 2,
	Ico = 3,
	Jpeg = 4,
	Ktx = 8,
	Png = 5,
	Unknown = 0,
	Wbmp = 6,
	Webp = 7,
}
```

#### New Type: SkiaSharp.SKImageFilter

```csharp
public class SKImageFilter : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public static SKImageFilter CreateAlphaThreshold (SKRectI region, float innerThreshold, float outerThreshold, SKImageFilter input);
	public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateCompose (SKImageFilter outer, SKImageFilter inner);
	public static SKImageFilter CreateCompose (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateDilate (int radiusX, int radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateDownSample (float scale, SKImageFilter input);
	public static SKImageFilter CreateDownSample (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateErode (int radiusX, int radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input);
	public static SKImageFilter CreateMatrix (SKMatrix matrix, SKFilterQuality quality, SKImageFilter input);
	public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKXferMode[] modes, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateOffset (float dx, float dy, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreatePicture (SKPicture picture);
	public static SKImageFilter CreatePicture (SKPicture picture, SKRect cropRect);
	public static SKImageFilter CreatePictureForLocalspace (SKPicture picture, SKRect cropRect, SKFilterQuality filterQuality);
	public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
	public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
	protected override void Dispose (bool disposing);

	// inner types
	public class CropRect : SkiaSharp.SKObject, System.IDisposable {
		// constructors
		public SKImageFilter.CropRect ();
		public SKImageFilter.CropRect (SKRect rect, SKCropRectFlags flags);
		// properties
		public SKCropRectFlags Flags { get; }
		public SKRect Rect { get; }
		// methods
		protected override void Dispose (bool disposing);
	}
}
```

#### New Type: SkiaSharp.SKImageInfo

```csharp
public struct SKImageInfo {
	// constructors
	public SKImageInfo (int width, int height, SKColorType colorType, SKAlphaType alphaType);
	// fields
	public SKAlphaType AlphaType;
	public SKColorType ColorType;
	public static SKImageInfo Empty;
	public int Height;
	public int Width;
	// properties
	public int BytesPerPixel { get; }
	public bool IsEmpty { get; }
	public bool IsOpaque { get; }
	public SKRectI Rect { get; }
	public int RowBytes { get; }
	public SKPointI Size { get; }
}
```

#### New Type: SkiaSharp.SKManagedStream

```csharp
public class SKManagedStream : SkiaSharp.SKStreamAsset, System.IDisposable {
	// constructors
	public SKManagedStream (System.IO.Stream managedStream);
	public SKManagedStream (System.IO.Stream managedStream, bool disposeManagedStream);
	// methods
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKMaskFilter

```csharp
public class SKMaskFilter : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public static float ConvertRadiusToSigma (float radius);
	public static float ConvertSigmaToRadius (float sigma);
	public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma);
	public static SKMaskFilter CreateClip (byte min, byte max);
	public static SKMaskFilter CreateEmboss (float blurSigma, SKPoint3 direction, float ambient, float specular);
	public static SKMaskFilter CreateEmboss (float blurSigma, float directionX, float directionY, float directionZ, float ambient, float specular);
	public static SKMaskFilter CreateGamma (float gamma);
	public static SKMaskFilter CreateTable (byte[] table);
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKMatrix

```csharp
public struct SKMatrix {
	// fields
	public float Persp0;
	public float Persp1;
	public float Persp2;
	public float ScaleX;
	public float ScaleY;
	public float SkewX;
	public float SkewY;
	public float TransX;
	public float TransY;
	// methods
	public static SKMatrix MakeIdentity ();
	public static SKMatrix MakeRotation (float radians);
	public static SKMatrix MakeScale (float sx, float sy);
	public static SKMatrix MakeScale (float sx, float sy, float pivotX, float pivotY);
	public static SKMatrix MakeSkew (float sx, float sy);
	public static SKMatrix MakeTranslation (float dx, float dy);
	public void SetScaleTranslate (float sx, float sy, float tx, float ty);
}
```

#### New Type: SkiaSharp.SKMatrixConvolutionTileMode

```csharp
[Serializable]
public enum SKMatrixConvolutionTileMode {
	Clamp = 0,
	ClampToBlack = 2,
	Repeat = 1,
}
```

#### New Type: SkiaSharp.SKMemoryStream

```csharp
public class SKMemoryStream : SkiaSharp.SKStreamMemory, System.IDisposable {
	// constructors
	public SKMemoryStream ();
	public SKMemoryStream (SKData data);
	public SKMemoryStream (byte[] data);
	public SKMemoryStream (ulong length);
	// methods
	public void SetMemory (byte[] data);
}
```

#### New Type: SkiaSharp.SKObject

```csharp
public abstract class SKObject : System.IDisposable {
	// properties
	protected IntPtr Handle { get; set; }
	// methods
	public virtual void Dispose ();
	protected virtual void Dispose (bool disposing);
	protected override void ~SKObject ();
}
```

#### New Type: SkiaSharp.SKPaint

```csharp
public class SKPaint : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKPaint ();
	// properties
	public SKColor Color { get; set; }
	public SKColorFilter ColorFilter { get; set; }
	public SKImageFilter ImageFilter { get; set; }
	public bool IsAntialias { get; set; }
	public bool IsStroke { get; set; }
	public SKMaskFilter MaskFilter { get; set; }
	public SKShader Shader { get; set; }
	public SKStrokeCap StrokeCap { get; set; }
	public SKStrokeJoin StrokeJoin { get; set; }
	public float StrokeMiter { get; set; }
	public float StrokeWidth { get; set; }
	public SKTextAlign TextAlign { get; set; }
	public SKTextEncoding TextEncoding { get; set; }
	public float TextScaleX { get; set; }
	public float TextSize { get; set; }
	public float TextSkewX { get; set; }
	public SKTypeface Typeface { get; set; }
	public SKXferMode XferMode { get; set; }
	// methods
	public long BreakText (string text, float maxWidth);
	public long BreakText (string text, float maxWidth, out float measuredWidth);
	public long BreakText (IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth);
	protected override void Dispose (bool disposing);
	public float MeasureText (string text);
	public float MeasureText (IntPtr buffer, IntPtr length);
	public float MeasureText (string text, ref SKRect bounds);
	public float MeasureText (IntPtr buffer, IntPtr length, ref SKRect bounds);
}
```

#### New Type: SkiaSharp.SKPath

```csharp
public class SKPath : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKPath ();
	// methods
	public void AddOval (SKRect rect, SKPathDirection direction);
	public void AddRect (SKRect rect, SKPathDirection direction);
	public void Close ();
	public void ConicTo (float x0, float y0, float x1, float y1, float w);
	public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2);
	protected override void Dispose (bool disposing);
	public bool GetBounds (out SKRect rect);
	public void LineTo (float x, float y);
	public void MoveTo (float x, float y);
	public void QuadTo (float x0, float y0, float x1, float y1);
}
```

#### New Type: SkiaSharp.SKPathDirection

```csharp
[Serializable]
public enum SKPathDirection {
	Clockwise = 0,
	CounterClockwise = 1,
}
```

#### New Type: SkiaSharp.SKPicture

```csharp
public class SKPicture : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public SKRect Bounds { get; }
	public uint UniqueId { get; }
	// methods
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKPictureRecorder

```csharp
public class SKPictureRecorder : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKPictureRecorder ();
	public SKPictureRecorder (IntPtr handle);
	// methods
	public SKCanvas BeginRecording (SKRect rect);
	protected override void Dispose (bool disposing);
	public SKPicture EndRecording ();
}
```

#### New Type: SkiaSharp.SKPixelGeometry

```csharp
[Serializable]
public enum SKPixelGeometry {
	BgrHorizontal = 2,
	BgrVertical = 4,
	RgbHorizontal = 1,
	RgbVertical = 3,
	Unknown = 0,
}
```

#### New Type: SkiaSharp.SKPoint

```csharp
public struct SKPoint {
	// constructors
	public SKPoint (float x, float y);
	// fields
	public static SKPoint Empty;
	// properties
	public bool IsEmpty { get; }
	public float X { get; set; }
	public float Y { get; set; }
	// methods
	public static SKPoint Add (SKPoint pt, SKSize sz);
	public static SKPoint Add (SKPoint pt, SKSizeI sz);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static SKPoint Subtract (SKPoint pt, SKSize sz);
	public static SKPoint Subtract (SKPoint pt, SKSizeI sz);
	public override string ToString ();
	public static SKPoint op_Addition (SKPoint pt, SKSize sz);
	public static SKPoint op_Addition (SKPoint pt, SKSizeI sz);
	public static bool op_Equality (SKPoint left, SKPoint right);
	public static bool op_Inequality (SKPoint left, SKPoint right);
	public static SKPoint op_Subtraction (SKPoint pt, SKSize sz);
	public static SKPoint op_Subtraction (SKPoint pt, SKSizeI sz);
}
```

#### New Type: SkiaSharp.SKPoint3

```csharp
public struct SKPoint3 {
	// constructors
	public SKPoint3 (float x, float y, float z);
	// fields
	public static SKPoint3 Empty;
	// properties
	public bool IsEmpty { get; }
	public float X { get; set; }
	public float Y { get; set; }
	public float Z { get; set; }
	// methods
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public override string ToString ();
	public static bool op_Equality (SKPoint3 left, SKPoint3 right);
	public static bool op_Inequality (SKPoint3 left, SKPoint3 right);
}
```

#### New Type: SkiaSharp.SKPointI

```csharp
public struct SKPointI {
	// constructors
	public SKPointI (SKSizeI sz);
	public SKPointI (int x, int y);
	// fields
	public static SKPointI Empty;
	// properties
	public bool IsEmpty { get; }
	public int X { get; set; }
	public int Y { get; set; }
	// methods
	public static SKPointI Add (SKPointI pt, SKSizeI sz);
	public static SKPointI Ceiling (SKPoint value);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public void Offset (SKPointI p);
	public void Offset (int dx, int dy);
	public static SKPointI Round (SKPoint value);
	public static SKPointI Subtract (SKPointI pt, SKSizeI sz);
	public override string ToString ();
	public static SKPointI Truncate (SKPoint value);
	public static SKPointI op_Addition (SKPointI pt, SKSizeI sz);
	public static bool op_Equality (SKPointI left, SKPointI right);
	public static SKSizeI op_Explicit (SKPointI p);
	public static SKPoint op_Implicit (SKPointI p);
	public static bool op_Inequality (SKPointI left, SKPointI right);
	public static SKPointI op_Subtraction (SKPointI pt, SKSizeI sz);
}
```

#### New Type: SkiaSharp.SKPointMode

```csharp
[Serializable]
public enum SKPointMode {
	Lines = 1,
	Points = 0,
	Polygon = 2,
}
```

#### New Type: SkiaSharp.SKRect

```csharp
public struct SKRect {
	// constructors
	public SKRect (float left, float top, float right, float bottom);
	// fields
	public float Bottom;
	public float Left;
	public float Right;
	public float Top;
	// methods
	public static SKRect Create (float width, float height);
	public static SKRect Create (float x, float y, float width, float height);
}
```

#### New Type: SkiaSharp.SKRectI

```csharp
public struct SKRectI {
	// constructors
	public SKRectI (int left, int top, int right, int bottom);
	// fields
	public int Bottom;
	public int Left;
	public int Right;
	public int Top;
	// methods
	public static SKRectI Create (int width, int height);
	public static SKRectI Create (int x, int y, int width, int height);
}
```

#### New Type: SkiaSharp.SKShader

```csharp
public class SKShader : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy);
	public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix);
	public static SKShader CreateColor (SKColor color);
	public static SKShader CreateColorFilter (SKShader shader, SKColorFilter filter);
	public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB);
	public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB, SKXferMode mode);
	public static SKShader CreateEmpty ();
	public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor[] colors, float[] colorPos, SKShaderTileMode mode);
	public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix);
	public static SKShader CreateLocalMatrix (SKShader shader, SKMatrix localMatrix);
	public static SKShader CreatePerlinNoiseFractalNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed);
	public static SKShader CreatePerlinNoiseFractalNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize);
	public static SKShader CreatePerlinNoiseTurbulence (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed);
	public static SKShader CreatePerlinNoiseTurbulence (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize);
	public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode);
	public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix);
	public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, float[] colorPos);
	public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, float[] colorPos, SKMatrix localMatrix);
	public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode);
	public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix);
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKShaderTileMode

```csharp
[Serializable]
public enum SKShaderTileMode {
	Clamp = 0,
	Mirror = 2,
	Repeat = 1,
}
```

#### New Type: SkiaSharp.SKSize

```csharp
public struct SKSize {
	// constructors
	public SKSize (float width, float height);
	// fields
	public float Height;
	public float Width;
}
```

#### New Type: SkiaSharp.SKSizeI

```csharp
public struct SKSizeI {
	// constructors
	public SKSizeI (int width, int height);
	// fields
	public int Height;
	public int Width;
}
```

#### New Type: SkiaSharp.SKStream

```csharp
public abstract class SKStream : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public bool HasLength { get; }
	public bool HasPosition { get; }
	public bool IsAtEnd { get; }
	public int Length { get; }
	public int Position { get; set; }
	// methods
	public bool Move (long offset);
	public int Read (byte[] buffer, int size);
	public byte ReadByte ();
	public short ReadInt16 ();
	public int ReadInt32 ();
	public sbyte ReadSByte ();
	public ushort ReadUInt16 ();
	public uint ReadUInt32 ();
	public bool Rewind ();
	public bool Seek (int position);
	public int Skip (int size);
}
```

#### New Type: SkiaSharp.SKStreamAsset

```csharp
public abstract class SKStreamAsset : SkiaSharp.SKStreamSeekable, System.IDisposable {
}
```

#### New Type: SkiaSharp.SKStreamMemory

```csharp
public abstract class SKStreamMemory : SkiaSharp.SKStreamAsset, System.IDisposable {
}
```

#### New Type: SkiaSharp.SKStreamRewindable

```csharp
public abstract class SKStreamRewindable : SkiaSharp.SKStream, System.IDisposable {
}
```

#### New Type: SkiaSharp.SKStreamSeekable

```csharp
public abstract class SKStreamSeekable : SkiaSharp.SKStreamRewindable, System.IDisposable {
}
```

#### New Type: SkiaSharp.SKStrokeCap

```csharp
[Serializable]
public enum SKStrokeCap {
	Butt = 0,
	Round = 1,
	Square = 2,
}
```

#### New Type: SkiaSharp.SKStrokeJoin

```csharp
[Serializable]
public enum SKStrokeJoin {
	Bevel = 2,
	Mitter = 0,
	Round = 1,
}
```

#### New Type: SkiaSharp.SKSurface

```csharp
public class SKSurface : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public SKCanvas Canvas { get; }
	// methods
	public static SKSurface Create (SKImageInfo info);
	public static SKSurface Create (SKImageInfo info, SKSurfaceProps props);
	public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes);
	public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props);
	public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType);
	public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props);
	public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes);
	public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props);
	protected override void Dispose (bool disposing);
	public SKImage Snapshot ();
}
```

#### New Type: SkiaSharp.SKSurfaceProps

```csharp
public struct SKSurfaceProps {
	// properties
	public SKPixelGeometry PixelGeometry { get; set; }
}
```

#### New Type: SkiaSharp.SKTextAlign

```csharp
[Serializable]
public enum SKTextAlign {
	Center = 1,
	Left = 0,
	Right = 2,
}
```

#### New Type: SkiaSharp.SKTextEncoding

```csharp
[Serializable]
public enum SKTextEncoding {
	GlyphId = 3,
	Utf16 = 1,
	Utf32 = 2,
	Utf8 = 0,
}
```

#### New Type: SkiaSharp.SKTypeface

```csharp
public class SKTypeface : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public int CharsToGlyphs (string chars, out ushort[] glyphs);
	public int CharsToGlyphs (IntPtr str, int strlen, SKEncoding encoding, out ushort[] glyphs);
	public int CountGlyphs (string str);
	public int CountGlyphs (IntPtr str, int strLen, SKEncoding encoding);
	protected override void Dispose (bool disposing);
	public static SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style);
	public static SKTypeface FromFile (string path, int index);
	public static SKTypeface FromStream (SKStreamAsset stream, int index);
	public static SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style);
}
```

#### New Type: SkiaSharp.SKTypefaceStyle

```csharp
[Serializable]
public enum SKTypefaceStyle {
	Bold = 1,
	BoldItalic = 3,
	Italic = 2,
	Normal = 0,
}
```

#### New Type: SkiaSharp.SKXferMode

```csharp
[Serializable]
public enum SKXferMode {
	Clear = 0,
	Color = 27,
	ColorBurn = 19,
	ColorDodge = 18,
	Darken = 16,
	Difference = 22,
	Dst = 2,
	DstATop = 10,
	DstIn = 6,
	DstOut = 8,
	DstOver = 4,
	Exclusion = 23,
	HardLight = 20,
	Hue = 25,
	Lighten = 17,
	Luminosity = 28,
	Modulate = 13,
	Multiply = 24,
	Overlay = 15,
	Plus = 12,
	Saturation = 26,
	Screen = 14,
	SoftLight = 21,
	Src = 1,
	SrcATop = 9,
	SrcIn = 5,
	SrcOut = 7,
	SrcOver = 3,
	Xor = 11,
}
```

#### New Type: SkiaSharp.SkiaExtensions

```csharp
public static class SkiaExtensions {
	// methods
	public static bool IsBgr (this SKPixelGeometry pg);
	public static bool IsHorizontal (this SKPixelGeometry pg);
	public static bool IsRgb (this SKPixelGeometry pg);
	public static bool IsVertical (this SKPixelGeometry pg);
}
```

