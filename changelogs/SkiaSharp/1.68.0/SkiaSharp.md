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
 [Obsolete ()]
 public static GRContext Create (GRBackend backend, IntPtr backendContext);
 [Obsolete ()]
 public int GetRecommendedSampleCount (GRPixelConfig config, float dpi);
```

Added methods:

```csharp
public static GRContext CreateGl ();
public static GRContext CreateGl (GRGlInterface backendContext);
public int GetMaxSurfaceSampleCount (SKColorType colorType);
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
 [Obsolete ()]
 public SKBitmap (SKImageInfo info, SKColorTable ctable);
 [Obsolete ()]
 public SKBitmap (SKImageInfo info, SKColorTable ctable, SKBitmapAllocFlags flags);
```

Added constructor:

```csharp
public SKBitmap (SKImageInfo info, SKBitmapAllocFlags flags);
```

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKColorTable ColorTable { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public SKPMColor GetIndex8Color (int x, int y);
 [Obsolete ()]
 public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);
 [Obsolete ()]
 public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable, SKBitmapReleaseDelegate releaseProc, object context);
 [Obsolete ()]
 public bool Resize (SKBitmap dst, SKBitmapResizeMethod method);
 [Obsolete ()]
 public SKBitmap Resize (SKImageInfo info, SKBitmapResizeMethod method);
 [Obsolete ()]
 public static bool Resize (SKBitmap dst, SKBitmap src, SKBitmapResizeMethod method);
 [Obsolete ()]
 public void SetColorTable (SKColorTable ct);
 [Obsolete ()]
 public void SetPixels (IntPtr pixels, SKColorTable ct);
```

Added methods:

```csharp
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc);
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc, object context);
public SKBitmap Resize (SKImageInfo info, SKFilterQuality quality);
public bool ScalePixels (SKBitmap destination, SKFilterQuality quality);
public bool ScalePixels (SKPixmap destination, SKFilterQuality quality);
public bool TryAllocPixels (SKImageInfo info);
public bool TryAllocPixels (SKImageInfo info, SKBitmapAllocFlags flags);
public bool TryAllocPixels (SKImageInfo info, int rowBytes);
```


#### Type Changed: SkiaSharp.SKCanvas

Added method:

```csharp
public void DrawText (SKTextBlob text, float x, float y, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKCodec

Removed property:

```csharp
public SKEncodedInfo EncodedInfo { get; }
```

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKCodecOrigin Origin { get; }
```

Added property:

```csharp
public SKEncodedOrigin EncodedOrigin { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKColorTable colorTable, ref int colorTableCount);
 [Obsolete ()]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, IntPtr colorTable, ref int colorTableCount);
 [Obsolete ()]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
 [Obsolete ()]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
 [Obsolete ()]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
 [Obsolete ()]
 public SKCodecResult GetPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
 [Obsolete ()]
 public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
 [Obsolete ()]
 public SKCodecResult StartIncrementalDecode (SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
 [Obsolete ()]
 public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
 [Obsolete ()]
 public SKCodecResult StartScanlineDecode (SKImageInfo info, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
```

Added methods:

```csharp
public static SKCodec Create (System.IO.Stream stream);
public static SKCodec Create (string filename);
public static SKCodec Create (SKStream stream, out SKCodecResult result);
public static SKCodec Create (System.IO.Stream stream, out SKCodecResult result);
public static SKCodec Create (string filename, out SKCodecResult result);
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

Modified methods:

```diff
 public SKDocument CreatePdf (SKWStream stream, float dpi--- = 72---)
 public SKDocument CreatePdf (string path, float dpi--- = 72---)
 public SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata, float dpi--- = 72---)
 public SKDocument CreateXps (SKWStream stream, float dpi--- = 72---)
 public SKDocument CreateXps (string path, float dpi--- = 72---)
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata, float dpi);
```

Added methods:

```csharp
public static SKDocument CreatePdf (SKWStream stream);
public static SKDocument CreatePdf (System.IO.Stream stream);
public static SKDocument CreatePdf (string path);
public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata);
public static SKDocument CreatePdf (System.IO.Stream stream, SKDocumentPdfMetadata metadata);
public static SKDocument CreatePdf (System.IO.Stream stream, float dpi);
public static SKDocument CreatePdf (string path, SKDocumentPdfMetadata metadata);
public static SKDocument CreateXps (SKWStream stream);
public static SKDocument CreateXps (System.IO.Stream stream);
public static SKDocument CreateXps (string path);
public static SKDocument CreateXps (System.IO.Stream stream, float dpi);
```


#### Type Changed: SkiaSharp.SKDocumentPdfMetadata

Added constructors:

```csharp
public SKDocumentPdfMetadata (int encodingQuality);
public SKDocumentPdfMetadata (float rasterDpi);
public SKDocumentPdfMetadata (float rasterDpi, int encodingQuality);
```

Added fields:

```csharp
public static SKDocumentPdfMetadata Default;
public static const int DefaultEncodingQuality;
public static const float DefaultRasterDpi;
```

Added properties:

```csharp
public int EncodingQuality { get; set; }
public bool PdfA { get; set; }
public float RasterDpi { get; set; }
```


#### Type Changed: SkiaSharp.SKFileStream

Added property:

```csharp
public bool IsValid { get; }
```


#### Type Changed: SkiaSharp.SKFileWStream

Added property:

```csharp
public bool IsValid { get; }
```


#### Type Changed: SkiaSharp.SKFontManager

Added property:

```csharp
public System.Collections.Generic.IEnumerable<string> FontFamilies { get; }
```

Added methods:

```csharp
public static SKFontManager CreateDefault ();
public SKTypeface CreateTypeface (SKData data, int index);
public SKTypeface CreateTypeface (SKStreamAsset stream, int index);
public SKTypeface CreateTypeface (System.IO.Stream stream, int index);
public SKTypeface CreateTypeface (string path, int index);
public SKFontStyleSet GetFontStyles (int index);
public SKFontStyleSet GetFontStyles (string familyName);
public SKTypeface MatchCharacter (string familyName, SKFontStyle style, string[] bcp47, int character);
public SKTypeface MatchFamily (string familyName, SKFontStyle style);
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
 [Obsolete ()]
 public SKData Encode (SKPixelSerializer serializer);
 [Obsolete ()]
 public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc);
 [Obsolete ()]
 public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc);
 [Obsolete ()]
 public static SKImage FromAdoptedTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);
 [Obsolete ()]
 public static SKImage FromAdoptedTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);
 [Obsolete ()]
 public static SKImage FromPixelCopy (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);
 [Obsolete ()]
 public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc);
 [Obsolete ()]
 public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc);
 [Obsolete ()]
 public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);
 [Obsolete ()]
 public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);
 [Obsolete ()]
 public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);
 [Obsolete ()]
 public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);
 [Obsolete ()]
 public static SKImage FromTexture (GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);
 [Obsolete ()]
 public static SKImage FromTexture (GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);
```

Added methods:

```csharp
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, SKColorType colorType);
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha);
public static SKImage FromAdoptedTexture (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace);
public static SKImage FromEncodedData (SKStream data);
public static SKImage FromEncodedData (byte[] data);
public static SKImage FromEncodedData (System.IO.Stream data);
public static SKImage FromEncodedData (string filename);
public static SKImage FromPixelCopy (SKImageInfo info, SKStream pixels);
public static SKImage FromPixelCopy (SKImageInfo info, byte[] pixels);
public static SKImage FromPixelCopy (SKImageInfo info, System.IO.Stream pixels);
public static SKImage FromPixelCopy (SKImageInfo info, SKStream pixels, int rowBytes);
public static SKImage FromPixelCopy (SKImageInfo info, byte[] pixels, int rowBytes);
public static SKImage FromPixelCopy (SKImageInfo info, System.IO.Stream pixels, int rowBytes);
public static SKImage FromPixels (SKImageInfo info, SKData data, int rowBytes);
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
 [Obsolete ()]
 public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKBlendMode[] modes, SKImageFilter.CropRect cropRect);
 [Obsolete ()]
 public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKBlendMode mode, SKImageFilter.CropRect cropRect);
```

Modified methods:

```diff
 public SKImageFilter CreateMerge (SKImageFilter[] filters, SKBlendMode[] modes--- = NULL---, SKImageFilter.CropRect cropRect = NULL)
```

Added methods:

```csharp
public static SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold, SKImageFilter input);
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
 [Obsolete ()]
 public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags);
 [Obsolete ()]
 public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags);
```

Added method:

```csharp
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM);
```


#### Type Changed: SkiaSharp.SKPaint

Modified methods:

```diff
 public bool GetFillPath (SKPath src, SKPath dst, float resScale--- = 1---)
 public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, float resScale--- = 1---)
```

Added methods:

```csharp
public bool ContainsGlyphs (byte[] text);
public bool ContainsGlyphs (string text);
public bool ContainsGlyphs (IntPtr text, int length);
public int CountGlyphs (byte[] text);
public int CountGlyphs (string text);
public int CountGlyphs (IntPtr text, int length);
public SKPath GetFillPath (SKPath src);
public bool GetFillPath (SKPath src, SKPath dst);
public SKPath GetFillPath (SKPath src, SKRect cullRect);
public SKPath GetFillPath (SKPath src, float resScale);
public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect);
public SKPath GetFillPath (SKPath src, SKRect cullRect, float resScale);
public float[] GetGlyphWidths (byte[] text);
public float[] GetGlyphWidths (string text);
public float[] GetGlyphWidths (byte[] text, out SKRect[] bounds);
public float[] GetGlyphWidths (IntPtr text, int length);
public float[] GetGlyphWidths (string text, out SKRect[] bounds);
public float[] GetGlyphWidths (IntPtr text, int length, out SKRect[] bounds);
public ushort[] GetGlyphs (byte[] text);
public ushort[] GetGlyphs (string text);
public ushort[] GetGlyphs (IntPtr text, int length);
public float[] GetHorizontalTextIntercepts (byte[] text, float[] xpositions, float y, float upperBounds, float lowerBounds);
public float[] GetHorizontalTextIntercepts (string text, float[] xpositions, float y, float upperBounds, float lowerBounds);
public float[] GetHorizontalTextIntercepts (IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds);
public float[] GetPositionedTextIntercepts (byte[] text, SKPoint[] positions, float upperBounds, float lowerBounds);
public float[] GetPositionedTextIntercepts (string text, SKPoint[] positions, float upperBounds, float lowerBounds);
public float[] GetPositionedTextIntercepts (IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds);
public float[] GetTextIntercepts (SKTextBlob text, float upperBounds, float lowerBounds);
public float[] GetTextIntercepts (byte[] text, float x, float y, float upperBounds, float lowerBounds);
public float[] GetTextIntercepts (string text, float x, float y, float upperBounds, float lowerBounds);
public float[] GetTextIntercepts (IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds);
```


#### Type Changed: SkiaSharp.SKPath

Added properties:

```csharp
public bool IsLine { get; }
public bool IsOval { get; }
public bool IsRect { get; }
public bool IsRoundRect { get; }
```

Added methods:

```csharp
public SKPoint[] GetLine ();
public SKRect GetOvalBounds ();
public SKRect GetRect ();
public SKRect GetRect (out bool isClosed, out SKPathDirection direction);
public SKRoundRect GetRoundRect ();
```


#### Type Changed: SkiaSharp.SKPathEffect

Removed method:

```csharp
public static SKPathEffect CreateArcTo (float radius);
```

Added methods:

```csharp
public static SKPathEffect CreateTrim (float start, float stop);
public static SKPathEffect CreateTrim (float start, float stop, SKTrimPathEffectMode mode);
```


#### Type Changed: SkiaSharp.SKPixelSerializer

Added constructor:

```csharp
protected SKPixelSerializer ();
```

Removed method:

```csharp
protected override void Dispose (bool disposing);
```

Added methods:

```csharp
protected virtual SKData OnEncode (SKPixmap pixmap);
protected virtual bool OnUseEncodedData (IntPtr data, IntPtr length);
```


#### Type Changed: SkiaSharp.SKPixmap

Obsoleted constructors:

```diff
 [Obsolete ()]
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
 [Obsolete ()]
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
 [Obsolete ()]
 public void Reset (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable);
 [Obsolete ()]
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


#### Type Changed: SkiaSharp.SKPoint

Added properties:

```csharp
public float Length { get; }
public float LengthSquared { get; }
```

Added methods:

```csharp
public static float Distance (SKPoint point, SKPoint other);
public static float DistanceSquared (SKPoint point, SKPoint other);
public static SKPoint Normalize (SKPoint point);
public static SKPoint Reflect (SKPoint point, SKPoint normal);
```


#### Type Changed: SkiaSharp.SKPointI

Added properties:

```csharp
public int Length { get; }
public int LengthSquared { get; }
```

Added methods:

```csharp
public static float Distance (SKPointI point, SKPointI other);
public static float DistanceSquared (SKPointI point, SKPointI other);
public static SKPointI Normalize (SKPointI point);
public static SKPointI Reflect (SKPointI point, SKPointI normal);
```


#### Type Changed: SkiaSharp.SKRectI

Added method:

```csharp
public bool IntersectsWithInclusive (SKRectI rect);
```


#### Type Changed: SkiaSharp.SKRoundRect

Added property:

```csharp
public SKPoint[] Radii { get; }
```


#### Type Changed: SkiaSharp.SKShader

Added methods:

```csharp
public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle);
public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix localMatrix);
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

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKSurfaceProps SurfaceProps { get; }
```

Added property:

```csharp
public SKSurfaceProperties SurfaceProperties { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc);
 [Obsolete ()]
 public static SKSurface Create (GRContext context, GRBackendTextureDesc desc);
 [Obsolete ()]
 public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc);
 [Obsolete ()]
 public static SKSurface Create (SKImageInfo info, SKSurfaceProps props);
 [Obsolete ()]
 public static SKSurface Create (SKPixmap pixmap, SKSurfaceProps props);
 [Obsolete ()]
 public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props);
 [Obsolete ()]
 public static SKSurface Create (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);
 [Obsolete ()]
 public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);
 [Obsolete ()]
 public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props);
 [Obsolete ()]
 public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType);
 [Obsolete ()]
 public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProps props);
 [Obsolete ()]
 public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props);
 [Obsolete ()]
 public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes);
 [Obsolete ()]
 public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props);
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc);
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc);
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);
 [Obsolete ()]
 public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);
```

Added methods:

```csharp
public static SKSurface Create (SKImageInfo info, SKSurfaceProperties props);
public static SKSurface Create (SKImageInfo info, int rowBytes);
public static SKSurface Create (SKImageInfo info, IntPtr pixels);
public static SKSurface Create (SKPixmap pixmap, SKSurfaceProperties props);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType);
public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType);
public static SKSurface Create (SKImageInfo info, int rowBytes, SKSurfaceProperties props);
public static SKSurface Create (SKImageInfo info, IntPtr pixels, SKSurfaceProperties props);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
public static SKSurface Create (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props);
public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProperties props);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType);
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin);
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props);
public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context);
public static SKSurface Create (GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context, SKSurfaceProperties props);
public static SKSurface Create (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props);
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
public static SKSurface CreateNull (int width, int height);
```


#### Type Changed: SkiaSharp.SKTypeface

Obsoleted properties:

```diff
 [Obsolete ()]
 public SKTypefaceStyle Style { get; }
```

Added properties:

```csharp
public static SKTypeface Default { get; }
public SKFontStyle FontStyle { get; }
```

Obsoleted methods:

```diff
 [Obsolete ()]
 public int CharsToGlyphs (string chars, out ushort[] glyphs);
 [Obsolete ()]
 public int CharsToGlyphs (IntPtr str, int strlen, SKEncoding encoding, out ushort[] glyphs);
 [Obsolete ()]
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
public int CountGlyphs (byte[] str, SKEncoding encoding);
public int CountGlyphs (string str, SKEncoding encoding);
public static SKTypeface CreateDefault ();
public static SKTypeface FromFamilyName (string familyName);
public static SKTypeface FromFamilyName (string familyName, SKFontStyle style);
public ushort[] GetGlyphs (string text);
public ushort[] GetGlyphs (byte[] text, SKEncoding encoding);
public ushort[] GetGlyphs (string text, SKEncoding encoding);
public int GetGlyphs (string text, out ushort[] glyphs);
public int GetGlyphs (byte[] text, SKEncoding encoding, out ushort[] glyphs);
public ushort[] GetGlyphs (IntPtr text, int length, SKEncoding encoding);
public int GetGlyphs (string text, SKEncoding encoding, out ushort[] glyphs);
public int GetGlyphs (IntPtr text, int length, SKEncoding encoding, out ushort[] glyphs);
```


#### Type Changed: SkiaSharp.SkiaExtensions

Added methods:

```csharp
public static SKColorType ToColorType (this GRPixelConfig config);

[Obsolete]
public static SKFilterQuality ToFilterQuality (this SKBitmapResizeMethod method);
public static uint ToGlSizedFormat (this GRPixelConfig config);
public static uint ToGlSizedFormat (this SKColorType colorType);
public static GRPixelConfig ToPixelConfig (this SKColorType colorType);
```


#### Type Changed: SkiaSharp.StringUtilities

Added method:

```csharp
public static byte[] GetEncodedText (string text, SKEncoding encoding);
```


#### Removed Type SkiaSharp.GRContextOptions
#### Removed Type SkiaSharp.GRContextOptionsGpuPathRenderers
#### Removed Type SkiaSharp.SKEncodedInfo
#### Removed Type SkiaSharp.SKEncodedInfoAlpha
#### Removed Type SkiaSharp.SKEncodedInfoColor
#### Removed Type SkiaSharp.SKLatticeFlags
#### New Type: SkiaSharp.GRBackendRenderTarget

```csharp
public class GRBackendRenderTarget : SkiaSharp.SKObject, System.IDisposable {
	// constructors

	[Obsolete]
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

	[Obsolete]
public GRBackendTexture (GRBackendTextureDesc desc);

	[Obsolete]
public GRBackendTexture (GRGlBackendTextureDesc desc);
	public GRBackendTexture (int width, int height, bool mipmapped, GRGlTextureInfo glInfo);
	// properties
	public GRBackend Backend { get; }
	public bool HasMipMaps { get; }
	public int Height { get; }
	public bool IsValid { get; }
	public SKRectI Rect { get; }
	public SKSizeI Size { get; }
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
public class SKFontStyleSet : SkiaSharp.SKObject, System.Collections.Generic.IEnumerable<SKFontStyle>, System.Collections.Generic.IReadOnlyCollection<SKFontStyle>, System.Collections.Generic.IReadOnlyList<SKFontStyle>, System.Collections.IEnumerable, System.IDisposable {
	// constructors
	public SKFontStyleSet ();
	// properties
	public virtual int Count { get; }
	public virtual SKFontStyle Item { get; }
	// methods
	public SKTypeface CreateTypeface (SKFontStyle style);
	public SKTypeface CreateTypeface (int index);
	protected override void Dispose (bool disposing);
	public virtual System.Collections.Generic.IEnumerator<SKFontStyle> GetEnumerator ();
	public string GetStyleName (int index);
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

#### New Type: SkiaSharp.SKSurfaceProperties

```csharp
public class SKSurfaceProperties : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKSurfaceProperties (SKPixelGeometry pixelGeometry);

	[Obsolete]
public SKSurfaceProperties (SKSurfaceProps props);
	public SKSurfaceProperties (SKSurfacePropsFlags flags, SKPixelGeometry pixelGeometry);
	public SKSurfaceProperties (uint flags, SKPixelGeometry pixelGeometry);
	// properties
	public SKSurfacePropsFlags Flags { get; }
	public bool IsUseDeviceIndependentFonts { get; }
	public SKPixelGeometry PixelGeometry { get; }
	// methods
	protected override void Dispose (bool disposing);
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

#### New Type: SkiaSharp.SKTextBlob

```csharp
public class SKTextBlob : SkiaSharp.SKObject, System.IDisposable {
	// properties
	public SKRect Bounds { get; }
	public uint UniqueId { get; }
	// methods
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKTextBlobBuilder

```csharp
public class SKTextBlobBuilder : SkiaSharp.SKObject, System.IDisposable {
	// constructors
	public SKTextBlobBuilder ();
	// methods
	public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions);
	public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, SKRect bounds);
	public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] utf8Text, uint[] clusters);
	public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters);
	public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] utf8Text, uint[] clusters, SKRect bounds);
	public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters, SKRect bounds);
	public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions);
	public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, SKRect bounds);
	public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] utf8Text, uint[] clusters);
	public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters);
	public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] utf8Text, uint[] clusters, SKRect bounds);
	public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters, SKRect bounds);
	public void AddRun (SKPaint font, float x, float y, ushort[] glyphs);
	public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, SKRect bounds);
	public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] utf8Text, uint[] clusters);
	public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters);
	public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] utf8Text, uint[] clusters, SKRect bounds);
	public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters, SKRect bounds);
	public SKTextBlob Build ();
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKTrimPathEffectMode

```csharp
[Serializable]
public enum SKTrimPathEffectMode {
	Inverted = 1,
	Normal = 0,
}
```


