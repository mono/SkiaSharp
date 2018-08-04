# API diff: SkiaSharp.dll

## SkiaSharp.dll

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKBitmap

Added constructor:

```csharp
public SKBitmap (SKImageInfo info, SKColorTable ctable);
```

Added property:

```csharp
public SKColorTable ColorTable { get; }
```

Added method:

```csharp
public SKColor GetIndex8Color (int x, int y);
```


#### Type Changed: SkiaSharp.SKCodec

Added methods:

```csharp
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKColorTable colorTable, ref int colorTableCount);
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, IntPtr colorTable, ref int colorTableCount);
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
```


#### Type Changed: SkiaSharp.SKCodecOptions

Added constructors:

```csharp
public SKCodecOptions (SKZeroInitialized zeroInitialized);
public SKCodecOptions (SKZeroInitialized zeroInitialized, SKRectI subset);
```

Added field:

```csharp
public static SKCodecOptions Default;
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
public static SKImageFilter CreateDownSample (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input, SKImageFilter.CropRect cropRect);
```

Added method:

```csharp
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input, SKImageFilter.CropRect cropRect);
```


#### Type Changed: SkiaSharp.SKRectI

Added field:

```csharp
public static SKRectI Empty;
```


#### New Type: SkiaSharp.SKColorTable

```csharp
public class SKColorTable : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKColorTable ();
	public SKColorTable (SKColor[] colors);
	public SKColorTable (int count);
	public SKColorTable (SKColor[] colors, int count);
	// fields
	public static const int MaxLength;
	// properties
	public SKColor[] Colors { get; }
	public int Count { get; }
	// methods
	protected override void Dispose (bool disposing);
	public IntPtr ReadColors ();
}
```


