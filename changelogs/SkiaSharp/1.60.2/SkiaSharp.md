# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKBitmap

Obsoleted methods:

```diff
 [Obsolete ()]
 public bool Encode (SKWStream dst, SKEncodedImageFormat format, int quality);
```


#### Type Changed: SkiaSharp.SKCanvas

Added methods:

```csharp
public void ClipRoundRect (SKRoundRect rect, SKClipOperation operation, bool antialias);
public void DrawRoundRect (SKRoundRect rect, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKPath

Obsoleted methods:

```diff
 [Obsolete ()]
 public void AddRoundedRect (SKRect rect, float rx, float ry, SKPathDirection dir);
```

Added methods:

```csharp
public void AddRoundRect (SKRoundRect rect, SKPathDirection direction);
public void AddRoundRect (SKRoundRect rect, SKPathDirection direction, uint startIndex);
public void AddRoundRect (SKRect rect, float rx, float ry, SKPathDirection dir);
```


#### Type Changed: SkiaSharp.SKPixmap

Added methods:

```csharp
public SKData Encode (SKJpegEncoderOptions options);
public SKData Encode (SKPngEncoderOptions options);
public SKData Encode (SKWebpEncoderOptions options);
public bool Encode (SKWStream dst, SKJpegEncoderOptions options);
public bool Encode (SKWStream dst, SKPngEncoderOptions options);
public bool Encode (SKWStream dst, SKWebpEncoderOptions options);
public static bool Encode (SKWStream dst, SKPixmap src, SKJpegEncoderOptions options);
public static bool Encode (SKWStream dst, SKPixmap src, SKPngEncoderOptions options);
public static bool Encode (SKWStream dst, SKPixmap src, SKWebpEncoderOptions options);
public static bool Encode (SKWStream dst, SKBitmap src, SKEncodedImageFormat format, int quality);
```


#### New Type: SkiaSharp.SKJpegEncoderAlphaOption

```csharp
[Serializable]
public enum SKJpegEncoderAlphaOption {
	BlendOnBlack = 1,
	Ignore = 0,
}
```

#### New Type: SkiaSharp.SKJpegEncoderDownsample

```csharp
[Serializable]
public enum SKJpegEncoderDownsample {
	Downsample420 = 0,
	Downsample422 = 1,
	Downsample444 = 2,
}
```

#### New Type: SkiaSharp.SKJpegEncoderOptions

```csharp
public struct SKJpegEncoderOptions {
	// constructors
	public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption);
	public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption, SKTransferFunctionBehavior blendBehavior);
	// fields
	public static SKJpegEncoderOptions Default;
	// properties
	public SKJpegEncoderAlphaOption AlphaOption { get; set; }
	public SKTransferFunctionBehavior BlendBehavior { get; set; }
	public SKJpegEncoderDownsample Downsample { get; set; }
	public int Quality { get; set; }
}
```

#### New Type: SkiaSharp.SKNWayCanvas

```csharp
public class SKNWayCanvas : SkiaSharp.SKNoDrawCanvas, System.IDisposable {
	// constructors
	public SKNWayCanvas (int width, int height);
	// methods
	public void AddCanvas (SKCanvas canvas);
	public void RemoveAll ();
	public void RemoveCanvas (SKCanvas canvas);
}
```

#### New Type: SkiaSharp.SKNoDrawCanvas

```csharp
public class SKNoDrawCanvas : SkiaSharp.SKCanvas, System.IDisposable {
	// constructors
	public SKNoDrawCanvas (int width, int height);
}
```

#### New Type: SkiaSharp.SKPngEncoderFilterFlags

```csharp
[Serializable]
[Flags]
public enum SKPngEncoderFilterFlags {
	AllFilters = 248,
	Avg = 64,
	NoFilters = 0,
	None = 8,
	Paeth = 128,
	Sub = 16,
	Up = 32,
}
```

#### New Type: SkiaSharp.SKPngEncoderOptions

```csharp
public struct SKPngEncoderOptions {
	// constructors
	public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel);
	public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel, SKTransferFunctionBehavior unpremulBehavior);
	// fields
	public static SKPngEncoderOptions Default;
	// properties
	public SKPngEncoderFilterFlags FilterFlags { get; set; }
	public SKTransferFunctionBehavior UnpremulBehavior { get; set; }
	public int ZLibLevel { get; set; }
}
```

#### New Type: SkiaSharp.SKRoundRect

```csharp
public class SKRoundRect : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKRoundRect ();
	public SKRoundRect (SKRect rect);
	public SKRoundRect (SKRoundRect rrect);
	public SKRoundRect (SKRect rect, float xRadius, float yRadius);
	// properties
	public bool AllCornersCircular { get; }
	public float Height { get; }
	public bool IsValid { get; }
	public SKRect Rect { get; }
	public SKRoundRectType Type { get; }
	public float Width { get; }
	// methods
	public bool CheckAllCornersCircular (float tolerance);
	public bool Contains (SKRect rect);
	public void Deflate (SKSize size);
	public void Deflate (float dx, float dy);
	protected override void Dispose (bool disposing);
	public SKPoint GetRadii (SKRoundRectCorner corner);
	public void Inflate (SKSize size);
	public void Inflate (float dx, float dy);
	public void Offset (SKPoint pos);
	public void Offset (float dx, float dy);
	public void SetEmpty ();
	public void SetNinePatch (SKRect rect, float leftRadius, float topRadius, float rightRadius, float bottomRadius);
	public void SetOval (SKRect rect);
	public void SetRect (SKRect rect);
	public void SetRect (SKRect rect, float xRadius, float yRadius);
	public void SetRectRadii (SKRect rect, SKPoint[] radii);
	public SKRoundRect Transform (SKMatrix matrix);
}
```

#### New Type: SkiaSharp.SKRoundRectCorner

```csharp
[Serializable]
public enum SKRoundRectCorner {
	LowerLeft = 3,
	LowerRight = 2,
	UpperLeft = 0,
	UpperRight = 1,
}
```

#### New Type: SkiaSharp.SKRoundRectType

```csharp
[Serializable]
public enum SKRoundRectType {
	Complex = 5,
	Empty = 0,
	NinePatch = 4,
	Oval = 2,
	Rect = 1,
	Simple = 3,
}
```

#### New Type: SkiaSharp.SKWebpEncoderCompression

```csharp
[Serializable]
public enum SKWebpEncoderCompression {
	Lossless = 1,
	Lossy = 0,
}
```

#### New Type: SkiaSharp.SKWebpEncoderOptions

```csharp
public struct SKWebpEncoderOptions {
	// constructors
	public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality);
	public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality, SKTransferFunctionBehavior unpremulBehavior);
	// fields
	public static SKWebpEncoderOptions Default;
	// properties
	public SKWebpEncoderCompression Compression { get; set; }
	public float Quality { get; set; }
	public SKTransferFunctionBehavior UnpremulBehavior { get; set; }
}
```


