# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.53.0.0 vs 1.49.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.SKAlphaType

Modified fields:

```diff
-Opaque = 0
+Opaque = 1
-Premul = 1
+Premul = 2
-Unpremul = 2
+Unpremul = 3
```


#### Type Changed: SkiaSharp.SKBitmap

Removed methods:

```csharp
public static SKBitmap Decode (SKStreamRewindable stream, SKColorType pref);
public static SKBitmap Decode (byte[] buffer, SKColorType pref);
public static SKBitmap Decode (string filename, SKColorType pref);
public static SKImageInfo DecodeBounds (SKStreamRewindable stream, SKColorType pref);
public static SKImageInfo DecodeBounds (byte[] buffer, SKColorType pref);
public static SKImageInfo DecodeBounds (string filename, SKColorType pref);
```


#### Type Changed: SkiaSharp.SKCanvas

Removed methods:

```csharp
public void ClipPath (SKPath path);
public void ClipRect (SKRect rect);
public void Save ();
public void SaveLayer (SKPaint paint);
public void SaveLayer (SKRect limit, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKColorType

Removed values:

```csharp
Alpha_8 = 3,
Bgra_8888 = 2,
N_32 = 5,
Rgb_565 = 4,
Rgba_8888 = 1,
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
public static SKImageFilter CreateDownSample (float scale, SKImageFilter input);
```


#### Type Changed: SkiaSharp.SKPictureRecorder

Removed constructor:

```csharp
public SKPictureRecorder (IntPtr handle);
```


#### Type Changed: SkiaSharp.SKSurfaceProps

Removed property:

```csharp
public SKPixelGeometry PixelGeometry { get; set; }
```


#### Removed Type SkiaSharp.SKImageDecoder
#### Removed Type SkiaSharp.SKImageDecoderFormat
#### Removed Type SkiaSharp.SKImageDecoderMode
#### Removed Type SkiaSharp.SKImageDecoderResult

