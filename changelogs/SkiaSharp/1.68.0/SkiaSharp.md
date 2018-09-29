# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 1.68.0.0 vs 1.60.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRBackend

Modified fields:

```diff
-OpenGL = 0
+OpenGL = 1
-Vulkan = 1
+Vulkan = 2
```

Added value:

```csharp
Metal = 0,
```


#### Type Changed: SkiaSharp.GRContext

Added property:

```csharp
public GRBackend Backend { get; }
```

Removed methods:

```csharp
public static GRContext Create (GRBackend backend, GRGlInterface backendContext, GRContextOptions options);
public static GRContext Create (GRBackend backend, IntPtr backendContext, GRContextOptions options);
```

Obsoleted methods:

```diff
 [Obsolete ("Use Create(GRBackend, GRGlInterface) instead.")]
 public static GRContext Create (GRBackend backend, IntPtr backendContext);
 [Obsolete ("Use GetMaxSurfaceSampleCountForColorType(SKColorType) instead.")]
 public int GetRecommendedSampleCount (GRPixelConfig config, float dpi);
```

Added methods:

```csharp
public static GRContext CreateGl ();
public static GRContext CreateGl (GRGlInterface backendContext);
public int GetMaxSurfaceSampleCountForColorType (SKColorType colorType);
```


#### Type Changed: SkiaSharp.GRGlInterface

Removed method:

```csharp
public GRGlInterface Clone ();
```


#### Type Changed: SkiaSharp.GRGlTextureInfo

Added constructor:

```csharp
public GRGlTextureInfo (uint target, uint id, uint format);
```

Added property:

```csharp
public uint Format { get; set; }
```


#### Type Changed: SkiaSharp.GRPixelConfig

Removed value:

```csharp
Rgba8888SInt = 9,
```

Modified fields:

```diff
-AlphaHalf = 12
+AlphaHalf = 13
-Bgra8888 = 6
+Bgra8888 = 7
-RgFloat = 11
+RgFloat = 12
-RgbaFloat = 10
+RgbaFloat = 11
-RgbaHalf = 13
+RgbaHalf = 14
-Sbgra8888 = 8
+Sbgra8888 = 9
-Srgba8888 = 7
+Srgba8888 = 8
```

Added values:

```csharp
Rgb888 = 6,
Rgba1010102 = 10,
```


#### Type Changed: SkiaSharp.GRSurfaceOrigin

Modified fields:

```diff
-BottomLeft = 2
+BottomLeft = 1
-TopLeft = 1
+TopLeft = 0
```


#### Type Changed: SkiaSharp.SKBitmap

Obsoleted constructors:

```diff
 [Obsolete ("The Index8 color type and color table is no longer supported. Use SKBitmap(SKImageInfo) instead.")]
 public SKBitmap (SKImageInfo info, SKColorTable ctable);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use SKBitmap(SKImageInfo, SKBitmapAllocFlags) instead.")]
 public SKBitmap (SKImageInfo info, SKColorTable ctable, SKBitmapAllocFlags flags);
```

Added constructor:

```csharp
public SKBitmap (SKImageInfo info, SKBitmapAllocFlags flags);
```

Obsoleted properties:

```diff
 [Obsolete ("The Index8 color type and color table is no longer supported.")]
 public SKColorTable ColorTable { get; }
```

Obsoleted methods:

```diff
 [Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixel(int, int) instead.")]
 public SKPMColor GetIndex8Color (int x, int y);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use InstallPixels(SKImageInfo, IntPtr, int) instead.")]
 public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use InstallPixels(SKImageInfo, IntPtr, int, SKBitmapReleaseDelegate, object) instead.")]
 public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable, SKBitmapReleaseDelegate releaseProc, object context);
 [Obsolete ()]
 public bool Resize (SKBitmap dst, SKBitmapResizeMethod method);
 [Obsolete ()]
 public SKBitmap Resize (SKImageInfo info, SKBitmapResizeMethod method);
 [Obsolete ()]
 public static bool Resize (SKBitmap dst, SKBitmap src, SKBitmapResizeMethod method);
 [Obsolete ("The Index8 color type and color table is no longer supported.")]
 public void SetColorTable (SKColorTable ct);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use SetPixels(IntPtr) instead.")]
 public void SetPixels (IntPtr pixels, SKColorTable ct);
```

Added methods:

```csharp
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc);
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc, object context);
public bool TryAllocPixels (SKImageInfo info);
public bool TryAllocPixels (SKImageInfo info, SKBitmapAllocFlags flags);
public bool TryAllocPixels (SKImageInfo info, int rowBytes);
```


#### Type Changed: SkiaSharp.SKCodec

Removed property:

```csharp
public SKEncodedInfo EncodedInfo { get; }
```

Modified properties:

```diff
-public SKCodecOrigin Origin { get; }
+public SKEncodedOrigin Origin { get; }
```

Obsoleted methods:

```diff
 [Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr) instead.")]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKColorTable colorTable, ref int colorTableCount);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr) instead.")]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, IntPtr colorTable, ref int colorTableCount);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, SKCodecOptions) instead.")]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, SKCodecOptions) instead.")]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixels(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use StartIncrementalDecode(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
 public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use StartIncrementalDecode(SKImageInfo, IntPtr, int, SKCodecOptions) instead.")]
 public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use StartScanlineDecode(SKImageInfo, SKCodecOptions) instead.")]
 public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use StartScanlineDecode(SKImageInfo, SKCodecOptions) instead.")]
 public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
```

Added methods:

```csharp
public static SKCodec Create (SKStream stream, out SKCodecResult result);
public bool GetFrameInfo (int index, out SKCodecFrameInfo frameInfo);
```


#### Type Changed: SkiaSharp.SKCodecFrameInfo

Added property:

```csharp
public SKCodecAnimationDisposalMethod DisposalMethod { get; set; }
```


#### Type Changed: SkiaSharp.SKCodecOptions

Removed constructor:

```csharp
public SKCodecOptions (int frameIndex, bool hasPriorFrame);
```

Added constructors:

```csharp
public SKCodecOptions (int frameIndex);
public SKCodecOptions (int frameIndex, int priorFrame);
```

Removed property:

```csharp
public bool HasPriorFrame { get; set; }
```

Added property:

```csharp
public int PriorFrame { get; set; }
```


#### Type Changed: SkiaSharp.SKCodecResult

Modified fields:

```diff
-CouldNotRewind = 6
+CouldNotRewind = 7
-InvalidConversion = 2
+InvalidConversion = 3
-InvalidInput = 5
+InvalidInput = 6
-InvalidParameters = 4
+InvalidParameters = 5
-InvalidScale = 3
+InvalidScale = 4
-Unimplemented = 7
+Unimplemented = 9
```

Added values:

```csharp
ErrorInInput = 2,
InternalError = 8,
```


#### Type Changed: SkiaSharp.SKColorFilter

Added field:

```csharp
public static const int TableMaxLength;
```


#### Type Changed: SkiaSharp.SKColorType

Removed value:

```csharp
Index8 = 6,
```

Modified fields:

```diff
-Bgra8888 = 5
+Bgra8888 = 6
-Gray8 = 7
+Gray8 = 9
-RgbaF16 = 8
+RgbaF16 = 10
```

Added values:

```csharp
Rgb101010x = 8,
Rgb888x = 5,
Rgba1010102 = 7,
```


#### Type Changed: SkiaSharp.SKDocument

Obsoleted methods:

```diff
 [Obsolete ("Use CreatePdf(SKWStream) instead.")]
 public static SKDocument CreatePdf (SKWStream stream, float dpi);
 [Obsolete ("Use CreatePdf(SKWStream, SKDocumentPdfMetadata) instead.")]
 public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata, float dpi);
```

Modified methods:

```diff
 public SKDocument CreatePdf (SKWStream stream, float dpi--- = 72---)
 public SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata, float dpi--- = 72---)
```

Added methods:

```csharp
public static SKDocument CreatePdf (SKWStream stream);
public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata);
```


#### Type Changed: SkiaSharp.SKFontManager

Added methods:

```csharp
public static SKFontManager CreateDefault ();
public SKFontStyleSet CreateStyleSet (int index);
public SKTypeface FromData (SKData data, int index);
public SKTypeface FromFile (string path, int index);
public SKTypeface FromStream (SKStreamAsset stream, int index);
public SKTypeface FromStream (System.IO.Stream stream, int index);
public SKTypeface MatchCharacter (string familyName, SKFontStyle style, string[] bcp47, int character);
public SKFontStyleSet MatchFamily (string familyName);
public SKTypeface MatchFamily (string familyName, SKFontStyle style);

[Obsolete]
public SKTypeface MatchTypeface (SKTypeface face, SKFontStyle style);
```


#### Type Changed: SkiaSharp.SKFontMetrics

Added properties:

```csharp
public float? StrikeoutPosition { get; }
public float? StrikeoutThickness { get; }
```


#### Type Changed: SkiaSharp.SKImage

Added properties:

```csharp
public SKColorSpace ColorSpace { get; }
public SKColorType ColorType { get; }
public SKData EncodedData { get; }
```

Obsoleted methods:

```diff
 [Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
 public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc);
 [Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
 public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc);
 [Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
 public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);
 [Obsolete ("Use FromAdoptedTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
 public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);
 [Obsolete ("The Index8 color type and color table is no longer supported. Use FromPixelCopy(SKImageInfo, IntPtr, int) instead.")]
 public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);
 [Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
 public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc);
 [Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
 public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc);
 [Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
 public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);
 [Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType) instead.")]
 public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);
 [Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate) instead.")]
 public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);
 [Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate) instead.")]
 public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);
 [Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate, object) instead.")]
 public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);
 [Obsolete ("Use FromTexture(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKAlphaType, SKColorSpace, SKImageTextureReleaseDelegate, object) instead.")]
 public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);
```

Added methods:

```csharp
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, SKColorType colorType);
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha);
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace);
public static SKImage FromTexture (GRContext context, GRBackendTexture texture, SKColorType colorType);
public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha);
public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace);
public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc);
public static SKImage FromTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc, object releaseContext);
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed method:

```csharp
public static SKImageFilter CreatePictureForLocalspace (SKPicture picture, SKRect cropRect, SKFilterQuality filterQuality);
```

Obsoleted methods:

```diff
 [Obsolete ("Use CreateMerge(SKImageFilter[], SKImageFilter.CropRect) instead.")]
 public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKBlendMode[] modes, SKImageFilter.CropRect cropRect);
 [Obsolete ("Use CreateMerge(SKImageFilter, SKImageFilter, SKImageFilter.CropRect) instead.")]
 public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKBlendMode mode, SKImageFilter.CropRect cropRect);
```

Modified methods:

```diff
 public SKImageFilter CreateMerge (SKImageFilter[] filters, SKBlendMode[] modes--- = NULL---, SKImageFilter.CropRect cropRect = NULL)
```

Added methods:

```csharp
public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKImageFilter.CropRect cropRect);
```


#### Type Changed: SkiaSharp.SKImageInfo

Added method:

```csharp
public SKImageInfo WithSize (int width, int height);
```


#### Type Changed: SkiaSharp.SKLattice

Removed property:

```csharp
public SKLatticeFlags[] Flags { get; set; }
```

Added properties:

```csharp
public SKColor[] Colors { get; set; }
public SKLatticeRectType[] RectTypes { get; set; }
```


#### Type Changed: SkiaSharp.SKManagedPixelSerializer

Removed methods:

```csharp
protected virtual SKData OnEncode (SKPixmap pixmap);
protected virtual bool OnUseEncodedData (IntPtr data, IntPtr length);
```


#### Type Changed: SkiaSharp.SKMask

Modified properties:

```diff
-public SKRectI Bounds { get; set; }
+public SKRectI Bounds { get; }
-public SKMaskFormat Format { get; set; }
+public SKMaskFormat Format { get; }
-public IntPtr Image { get; set; }
+public IntPtr Image { get; }
-public uint RowBytes { get; set; }
+public uint RowBytes { get; }
```


#### Type Changed: SkiaSharp.SKMaskFilter

Added field:

```csharp
public static const int TableMaxLength;
```

Obsoleted methods:

```diff
 [Obsolete ("Use CreateBlur(SKBlurStyle, float) instead.")]
 public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags);
 [Obsolete ("Use CreateBlur(SKBlurStyle, float, SKRect) instead.")]
 public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags);
```

Added method:

```csharp
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM);
```


#### Type Changed: SkiaSharp.SKPathEffect

Removed method:

```csharp
public static SKPathEffect CreateArcTo (float radius);
```


#### Type Changed: SkiaSharp.SKPixelSerializer

Modified base type:

```diff
-SkiaSharp.SKObject
+System.Object
```

Added constructor:

```csharp
protected SKPixelSerializer ();
```

Modified methods:

```diff
-protected override void Dispose (bool disposing)
+protected virtual void Dispose (bool disposing)
```

Added methods:

```csharp
public virtual void Dispose ();
protected virtual SKData OnEncode (SKPixmap pixmap);
protected virtual bool OnUseEncodedData (IntPtr data, IntPtr length);
```


#### Type Changed: SkiaSharp.SKPixmap

Obsoleted constructors:

```diff
 [Obsolete ("The Index8 color type and color table is no longer supported. Use SKPixmap(SKImageInfo, IntPtr, int) instead.")]
 public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable);
```

Modified constructors:

```diff
 public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable--- = NULL---)
```

Added constructor:

```csharp
public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes);
```

Obsoleted properties:

```diff
 [Obsolete ("The Index8 color type and color table is no longer supported.")]
 public SKColorTable ColorTable { get; }
```

Added properties:

```csharp
public int BytesSize { get; }
public SKRectI Rect { get; }
public SKSizeI Size { get; }
```

Obsoleted methods:

```diff
 [Obsolete ("The Index8 color type and color table is no longer supported. Use Reset(SKImageInfo, IntPtr, int) instead.")]
 public void Reset (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable);
 [Obsolete ("Use ScalePixels(SKPixmap, SKFilterQuality) instead.")]
 public static bool Resize (SKPixmap dst, SKPixmap src, SKBitmapResizeMethod method);
```

Modified methods:

```diff
 public void Reset (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable--- = NULL---)
```

Added methods:

```csharp
public bool Erase (SKColor color);
public bool Erase (SKColor color, SKRectI subset);
public SKPixmap ExtractSubset (SKRectI subset);
public bool ExtractSubset (SKPixmap result, SKRectI subset);
public SKColor GetPixelColor (int x, int y);
public IntPtr GetPixels (int x, int y);
public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKTransferFunctionBehavior behavior);
public void Reset (SKImageInfo info, IntPtr addr, int rowBytes);
public bool ScalePixels (SKPixmap destination, SKFilterQuality quality);
```


#### Type Changed: SkiaSharp.SKStream

Added methods:

```csharp
public bool ReadBool ();
public bool ReadBool (out bool buffer);
public bool ReadByte (out byte buffer);
public bool ReadInt16 (out short buffer);
public bool ReadInt32 (out int buffer);
public bool ReadSByte (out sbyte buffer);
public bool ReadUInt16 (out ushort buffer);
public bool ReadUInt32 (out uint buffer);
```


#### Type Changed: SkiaSharp.SKSurface

Obsoleted methods:

```diff
 [Obsolete ("Use Create(GRContext, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType) instead.")]
 public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc);
 [Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
 public static SKSurface Create (GRContext context, GRBackendTextureDesc desc);
 [Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
 public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc);
 [Obsolete ("Use Create(GRContext, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType, SKSurfaceProps) instead.")]
 public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props);
 [Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProps) instead.")]
 public static SKSurface Create (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);
 [Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProps) instead.")]
 public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);
 [Obsolete ("Use Create(SKImageInfo) instead.")]
 public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType);
 [Obsolete ("Use Create(SKImageInfo, SKSurfaceProps) instead.")]
 public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props);
 [Obsolete ("Use Create(SKImageInfo, IntPtr, int) instead.")]
 public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes);
 [Obsolete ("Use Create(SKImageInfo, IntPtr, int, SKSurfaceProps) instead.")]
 public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props);
 [Obsolete ("Use CreateAsRenderTarget(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc);
 [Obsolete ("Use CreateAsRenderTarget(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc);
 [Obsolete ("Use CreateAsRenderTarget(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProps) instead.")]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);
 [Obsolete ("Use CreateAsRenderTarget(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProps) instead.")]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);
```

Added methods:

```csharp
public static SKSurface Create (SKImageInfo info, int rowBytes);
public static SKSurface Create (SKImageInfo info, IntPtr pixels);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType);
public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType);
public static SKSurface Create (SKImageInfo info, int rowBytes, SKSurfaceProps props);
public static SKSurface Create (SKImageInfo info, IntPtr pixels, SKSurfaceProps props);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProps props);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProps props);
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, SKSurfaceProps props);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProps props);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProps props);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType);
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin);
public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProps props);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProps props);
public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context, SKSurfaceProps props);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProps props);
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProps props, bool shouldCreateWithMips);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProps props);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProps props);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProps props);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProps props);
public static SKSurface CreateNull (int width, int height);
```


#### Type Changed: SkiaSharp.SKTypeface

Obsoleted properties:

```diff
 [Obsolete ("Use FontWeight and FontSlant instead.")]
 public SKTypefaceStyle Style { get; }
```

Added properties:

```csharp
public static SKTypeface Default { get; }
public SKFontStyle FontStyle { get; }
```

Obsoleted methods:

```diff
 [Obsolete ("Use FromFamilyName(string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant) instead.")]
 public static SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style);
 [Obsolete ()]
 public static SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style);
```

Modified methods:

```diff
 public SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style--- = 0---)
 public SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style--- = 0---)
```

Added methods:

```csharp
public static SKTypeface CreateDefault ();
public static SKTypeface FromFamilyName (string familyName);
public static SKTypeface FromFamilyName (string familyName, SKFontStyle style);
```


#### Type Changed: SkiaSharp.SkiaExtensions

Added methods:

```csharp
public static SKColorType ToColorType (this GRPixelConfig config);

[Obsolete]
public static SKFilterQuality ToFilterQuality (this SKBitmapResizeMethod method);
public static GRPixelConfig ToPixelConfig (this SKColorType colorType);
public static uint ToSizedFormat (this GRPixelConfig config);
public static uint ToSizedFormat (this SKColorType colorType);
```


#### Removed Type SkiaSharp.GRContextOptions
#### Removed Type SkiaSharp.GRContextOptionsGpuPathRenderers
#### Removed Type SkiaSharp.SKCodecOrigin
#### Removed Type SkiaSharp.SKEncodedInfo
#### Removed Type SkiaSharp.SKEncodedInfoAlpha
#### Removed Type SkiaSharp.SKEncodedInfoColor
#### Removed Type SkiaSharp.SKLatticeFlags
#### New Type: SkiaSharp.GRBackendRenderTarget

```csharp
public class GRBackendRenderTarget : SkiaSharp.SKObject, System.IDisposable {
	// constructors

	[Obsolete ("Use GRBackendRenderTarget(int, int, int, int, GRGlFramebufferInfo) instead.")]
public GRBackendRenderTarget (GRBackend backend, GRBackendRenderTargetDesc desc);
	public GRBackendRenderTarget (int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo);
	// properties
	public GRBackend Backend { get; }
	public int Height { get; }
	public bool IsValid { get; }
	public SKRectI Rect { get; }
	public int SampleCount { get; }
	public SKSizeI Size { get; }
	public int StencilBits { get; }
	public int Width { get; }
	// methods
	protected override void Dispose (bool disposing);
	public GRGlFramebufferInfo GetGlFramebufferInfo ();
	public bool GetGlFramebufferInfo (out GRGlFramebufferInfo glInfo);
}
```

#### New Type: SkiaSharp.GRBackendTexture

```csharp
public class GRBackendTexture : SkiaSharp.SKObject, System.IDisposable {
	// constructors

	[Obsolete ("Use GRBackendTexture(int, int, bool, GRGlTextureInfo) instead.")]
public GRBackendTexture (GRBackendTextureDesc desc);

	[Obsolete ("Use GRBackendTexture(int, int, bool, GRGlTextureInfo) instead.")]
public GRBackendTexture (GRGlBackendTextureDesc desc);
	public GRBackendTexture (int width, int height, bool mipmapped, GRGlTextureInfo glInfo);
	// properties
	public GRBackend Backend { get; }
	public bool HasMipMaps { get; }
	public int Height { get; }
	public bool IsValid { get; }
	public int Width { get; }
	// methods
	protected override void Dispose (bool disposing);
	public GRGlTextureInfo GetGlTextureInfo ();
	public bool GetGlTextureInfo (out GRGlTextureInfo glInfo);
}
```

#### New Type: SkiaSharp.GRGlFramebufferInfo

```csharp
public struct GRGlFramebufferInfo {
	// constructors
	public GRGlFramebufferInfo (uint fboId);
	public GRGlFramebufferInfo (uint fboId, uint format);
	// properties
	public uint Format { get; set; }
	public uint FramebufferObjectId { get; set; }
}
```

#### New Type: SkiaSharp.SKCodecAnimationDisposalMethod

```csharp
[Serializable]
public enum SKCodecAnimationDisposalMethod {
	Keep = 1,
	RestoreBackgroundColor = 2,
	RestorePrevious = 3,
}
```

#### New Type: SkiaSharp.SKEncodedOrigin

```csharp
[Serializable]
public enum SKEncodedOrigin {
	BottomLeft = 4,
	BottomRight = 3,
	Default = 1,
	LeftBottom = 8,
	LeftTop = 5,
	RightBottom = 7,
	RightTop = 6,
	TopLeft = 1,
	TopRight = 2,
}
```

#### New Type: SkiaSharp.SKFontStyle

```csharp
public class SKFontStyle : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKFontStyle ();
	public SKFontStyle (SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant);
	public SKFontStyle (int weight, int width, SKFontStyleSlant slant);
	// properties
	public static SKFontStyle Bold { get; }
	public static SKFontStyle BoldItalic { get; }
	public static SKFontStyle Italic { get; }
	public static SKFontStyle Normal { get; }
	public SKFontStyleSlant Slant { get; }
	public int Weight { get; }
	public int Width { get; }
	// methods
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKFontStyleSet

```csharp
public class SKFontStyleSet : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKFontStyleSet ();
	// properties
	public int Count { get; }
	// methods
	public SKTypeface CreateTypeface (SKFontStyle style);
	public SKTypeface CreateTypeface (int index);
	protected override void Dispose (bool disposing);
	public void GetStyle (int index, out SKFontStyle fontStyle, out string styleName);
}
```

#### New Type: SkiaSharp.SKLatticeRectType

```csharp
[Serializable]
public enum SKLatticeRectType {
	Default = 0,
	FixedColor = 2,
	Transparent = 1,
}
```

#### New Type: SkiaSharp.SKOverdrawCanvas

```csharp
public class SKOverdrawCanvas : SkiaSharp.SKNWayCanvas, System.IDisposable {
	// constructors
	public SKOverdrawCanvas (SKCanvas canvas);
}
```

#### New Type: SkiaSharp.SKSurfaceReleaseDelegate

```csharp
public sealed delegate SKSurfaceReleaseDelegate : System.MulticastDelegate, System.ICloneable, System.Runtime.Serialization.ISerializable {
	// constructors
	public SKSurfaceReleaseDelegate (object object, IntPtr method);
	// methods
	public virtual System.IAsyncResult BeginInvoke (IntPtr address, object context, System.AsyncCallback callback, object object);
	public virtual void EndInvoke (System.IAsyncResult result);
	public virtual void Invoke (IntPtr address, object context);
}
```


