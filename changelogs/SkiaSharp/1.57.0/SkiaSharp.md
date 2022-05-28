# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.57.0.0 vs 1.56.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRPixelConfig

Removed value:

```csharp
Index8 = 2,
```

Added value:

```csharp
Gray8 = 2,
```


#### Type Changed: SkiaSharp.SKBitmap

Added methods:

```csharp
public bool Encode (SKWStream dst, SKEncodedImageFormat format, int quality);
public void NotifyPixelsChanged ();
public bool PeekPixels (SKPixmap pixmap);
```


#### Type Changed: SkiaSharp.SKCanvas

Added method:

```csharp
public void DrawSurface (SKSurface surface, float x, float y, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKCodec

Modified properties:

```diff
-public SKEncodedFormat EncodedFormat { get; }
+public SKEncodedImageFormat EncodedFormat { get; }
```


#### Type Changed: SkiaSharp.SKCodecFrameInfo

Added property:

```csharp
public bool FullyRecieved { get; set; }
```


#### Type Changed: SkiaSharp.SKData

Added methods:

```csharp
public static SKData Create (int size);
public static SKData Create (ulong size);
```


#### Type Changed: SkiaSharp.SKDocument

Added methods:

```csharp
public static SKDocument CreateXps (SKWStream stream, float dpi);
public static SKDocument CreateXps (string path, float dpi);
```


#### Type Changed: SkiaSharp.SKDynamicMemoryWStream

Added methods:

```csharp
public void CopyTo (SKWStream dst);
public void CopyTo (IntPtr data);
public SKData DetachAsData ();
```


#### Type Changed: SkiaSharp.SKImage

Removed method:

```csharp
public SKImage ToTextureImage (GRContext context);
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public SKData Encode (SKImageEncodeFormat format, int quality);
```

Added method:

```csharp
public SKData Encode (SKEncodedImageFormat format, int quality);
```


#### Type Changed: SkiaSharp.SKImageEncodeFormat

Removed value:

```csharp
Unknown = 0,
```

Modified fields:

```diff
-Bmp = 1
+Bmp = 0
-Gif = 2
+Gif = 1
-Ico = 3
+Ico = 2
-Jpeg = 4
+Jpeg = 3
-Png = 5
+Png = 4
-Wbmp = 6
+Wbmp = 5
-Webp = 7
+Webp = 6
```


#### Type Changed: SkiaSharp.SKMaskFilter

Removed methods:

```csharp
public static SKMaskFilter CreateEmboss (float blurSigma, SKPoint3 direction, float ambient, float specular);
public static SKMaskFilter CreateEmboss (float blurSigma, float directionX, float directionY, float directionZ, float ambient, float specular);
public static SKMaskFilter CreateShadow (float occluderHeight, SKPoint3 lightPos, float lightRadius, float ambientAlpha, float spotAlpha, SKShadowMaskFilterShadowFlags flags);
```


#### Type Changed: SkiaSharp.SKPixmap

Added methods:

```csharp
public SKData Encode (SKEncodedImageFormat encoder, int quality);
public bool Encode (SKWStream dst, SKEncodedImageFormat encoder, int quality);
public static bool Encode (SKWStream dst, SKPixmap src, SKEncodedImageFormat encoder, int quality);
```


#### Type Changed: SkiaSharp.SKSurface

Added property:

```csharp
public SKSurfaceProps SurfaceProps { get; }
```

Added methods:

```csharp
public void Draw (SKCanvas canvas, float x, float y, SKPaint paint);
public SKPixmap PeekPixels ();
public bool PeekPixels (SKPixmap pixmap);
public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY);
```


#### Type Changed: SkiaSharp.SKWStream

Removed method:

```csharp
public void NewLine ();
```

Added method:

```csharp
public bool NewLine ();
```


#### Removed Type SkiaSharp.SKEncodedFormat
#### Removed Type SkiaSharp.SKShadowMaskFilterShadowFlags
#### New Type: SkiaSharp.SKEncodedImageFormat

```csharp
[Serializable]
public enum SKEncodedImageFormat {
	Astc = 9,
	Bmp = 0,
	Dng = 10,
	Gif = 1,
	Ico = 2,
	Jpeg = 3,
	Ktx = 8,
	Pkm = 7,
	Png = 4,
	Wbmp = 5,
	Webp = 6,
}
```


