# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.56.0.0 vs 1.55.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKBitmap

Added methods:

```csharp
public bool InstallPixels (SKPixmap pixmap);
public SKPixmap PeekPixels ();
public bool Resize (SKBitmap dst, SKBitmapResizeMethod method);
public SKBitmap Resize (SKImageInfo info, SKBitmapResizeMethod method);
public static bool Resize (SKBitmap dst, SKBitmap src, SKBitmapResizeMethod method);
```


#### Type Changed: SkiaSharp.SKCodec

Added properties:

```csharp
public SKEncodedInfo EncodedInfo { get; }
public int FrameCount { get; }
public SKCodecFrameInfo[] FrameInfo { get; }
public int RepetitionCount { get; }
```

Added methods:

```csharp
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options);
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options);
```


#### Type Changed: SkiaSharp.SKCodecOptions

Added constructor:

```csharp
public SKCodecOptions (int frameIndex, bool hasPriorFrame);
```

Added properties:

```csharp
public int FrameIndex { get; set; }
public bool HasPriorFrame { get; set; }
```


#### Type Changed: SkiaSharp.SKColorFilter

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKColorFilter CreateXferMode (SKColor c, SKXferMode mode);
```

Added method:

```csharp

[Obsolete]
public static SKColorFilter CreateBlendMode (SKColor c, SKXferMode mode);
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKXferMode[] modes, SKImageFilter.CropRect cropRect);
```

Added methods:

```csharp
public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKBlendMode[] modes, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKBlendMode mode, SKImageFilter.CropRect cropRect);
```


#### Type Changed: SkiaSharp.SKMaskFilter

Added method:

```csharp
public static SKMaskFilter CreateShadow (float occluderHeight, SKPoint3 lightPos, float lightRadius, float ambientAlpha, float spotAlpha, SKShadowMaskFilterShadowFlags flags);
```


#### Type Changed: SkiaSharp.SKPicture

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKRect Bounds { get; }
```

Added property:

```csharp
public SKRect CullRect { get; }
```


#### Type Changed: SkiaSharp.SKPictureRecorder

Modified methods:

```diff
-public SKCanvas BeginRecording (SKRect rect)
+public SKCanvas BeginRecording (SKRect cullRect)
```


#### Type Changed: SkiaSharp.SKShader

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB, SKXferMode mode);
```

Added method:

```csharp
public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB, SKBlendMode mode);
```


#### New Type: SkiaSharp.SKBitmapResizeMethod

```csharp
[Serializable]
public enum SKBitmapResizeMethod {
	Box = 0,
	Hamming = 3,
	Lanczos3 = 2,
	Mitchell = 4,
	Triangle = 1,
}
```

#### New Type: SkiaSharp.SKCodecFrameInfo

```csharp
public struct SKCodecFrameInfo {
	// properties
	public int Duration { get; set; }
	public int RequiredFrame { get; set; }
}
```

#### New Type: SkiaSharp.SKEncodedInfo

```csharp
public struct SKEncodedInfo {
	// constructors
	public SKEncodedInfo (SKEncodedInfoColor color);
	public SKEncodedInfo (SKEncodedInfoColor color, SKEncodedInfoAlpha alpha, byte bitsPerComponent);
	// properties
	public SKEncodedInfoAlpha Alpha { get; }
	public byte BitsPerComponent { get; }
	public byte BitsPerPixel { get; }
	public SKEncodedInfoColor Color { get; }
}
```

#### New Type: SkiaSharp.SKEncodedInfoAlpha

```csharp
[Serializable]
public enum SKEncodedInfoAlpha {
	Binary = 2,
	Opaque = 0,
	Unpremul = 1,
}
```

#### New Type: SkiaSharp.SKEncodedInfoColor

```csharp
[Serializable]
public enum SKEncodedInfoColor {
	Bgr = 5,
	Bgra = 7,
	Bgrx = 6,
	Gray = 0,
	GrayAlpha = 1,
	InvertedCmyk = 10,
	Palette = 2,
	Rgb = 3,
	Rgba = 4,
	Ycck = 11,
	Yuv = 8,
	Yuva = 9,
}
```

#### New Type: SkiaSharp.SKPixmap

```csharp
public class SKPixmap : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKPixmap ();
	public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable);
	// properties
	public SKAlphaType AlphaType { get; }
	public int BytesPerPixel { get; }
	public SKColorTable ColorTable { get; }
	public SKColorType ColorType { get; }
	public int Height { get; }
	public SKImageInfo Info { get; }
	public int RowBytes { get; }
	public int Width { get; }
	// methods
	protected override void Dispose (bool disposing);
	public IntPtr GetPixels ();
	public void Reset ();
	public void Reset (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable);
	public static bool Resize (SKPixmap dst, SKPixmap src, SKBitmapResizeMethod method);
}
```

#### New Type: SkiaSharp.SKShadowMaskFilterShadowFlags

```csharp
[Serializable]
[Flags]
public enum SKShadowMaskFilterShadowFlags {
	All = 7,
	GaussianEdge = 4,
	LargerUmbra = 2,
	None = 0,
	TransparentOccluder = 1,
}
```


