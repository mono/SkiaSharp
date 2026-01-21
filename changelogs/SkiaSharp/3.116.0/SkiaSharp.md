# API diff: SkiaSharp.dll

## SkiaSharp.dll

> Assembly Version Changed: 3.116.0.0 vs 2.88.0.0

### Namespace SkiaSharp

#### Type Changed: SkiaSharp.GRBackendRenderTarget

Removed constructor:

```csharp
[Obsolete ("Use GRBackendRenderTarget(int, int, int, int, GRGlFramebufferInfo) instead.")]
public GRBackendRenderTarget (GRBackend backend, GRBackendRenderTargetDesc desc);
```


#### Type Changed: SkiaSharp.GRBackendTexture

Removed constructors:

```csharp
[Obsolete ("Use GRBackendTexture(int, int, bool, GRGlTextureInfo) instead.")]
public GRBackendTexture (GRBackendTextureDesc desc);

[Obsolete ("Use GRBackendTexture(int, int, bool, GRGlTextureInfo) instead.")]
public GRBackendTexture (GRGlBackendTextureDesc desc);
```

Added constructor:

```csharp
public GRBackendTexture (int width, int height, bool mipmapped, GRMtlTextureInfo mtlInfo);
```


#### Type Changed: SkiaSharp.GRContext

Modified properties:

```diff
 public ---override--- GRBackend Backend { get; }
 public ---override--- bool IsAbandoned { get; }
```

Removed methods:

```csharp
[Obsolete ("Use CreateGl() instead.")]
public static GRContext Create (GRBackend backend);

[Obsolete ("Use CreateGl(GRGlInterface) instead.")]
public static GRContext Create (GRBackend backend, GRGlInterface backendContext);

[Obsolete ("Use CreateGl(GRGlInterface) instead.")]
public static GRContext Create (GRBackend backend, IntPtr backendContext);

[Obsolete]
public int GetRecommendedSampleCount (GRPixelConfig config, float dpi);

[Obsolete ("Use GetResourceCacheLimit() instead.")]
public void GetResourceCacheLimits (out int maxResources, out long maxResourceBytes);

[Obsolete ("Use SetResourceCacheLimit(long) instead.")]
public void SetResourceCacheLimits (int maxResources, long maxResourceBytes);
```

Added methods:

```csharp
public static GRContext CreateMetal (GRMtlBackendContext backendContext);
public static GRContext CreateMetal (GRMtlBackendContext backendContext, GRContextOptions options);
```


#### Type Changed: SkiaSharp.GRGlFramebufferInfo

Added property:

```csharp
public bool Protected { get; set; }
```


#### Type Changed: SkiaSharp.GRGlInterface

Removed methods:

```csharp
[Obsolete ("Use CreateAngle(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleAngleInterface (GRGlGetProcDelegate get);

[Obsolete ("Use CreateAngle(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleAngleInterface (object context, GRGlGetProcDelegate get);

[Obsolete ("Use CreateOpenGl(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleGlInterface (GRGlGetProcDelegate get);

[Obsolete ("Use CreateOpenGl(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleGlInterface (object context, GRGlGetProcDelegate get);

[Obsolete ("Use CreateGles(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleGlesInterface (GRGlGetProcDelegate get);

[Obsolete ("Use CreateGles(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleGlesInterface (object context, GRGlGetProcDelegate get);

[Obsolete ("Use Create(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleInterface (GRGlGetProcDelegate get);

[Obsolete ("Use Create(GRGlGetProcedureAddressDelegate) instead.")]
public static GRGlInterface AssembleInterface (object context, GRGlGetProcDelegate get);

[Obsolete ("Use Create() instead.")]
public static GRGlInterface CreateDefaultInterface ();

[Obsolete ("Use Create() instead.")]
public static GRGlInterface CreateNativeAngleInterface ();

[Obsolete ("Use CreateEvas(IntPtr) instead.")]
public static GRGlInterface CreateNativeEvasInterface (IntPtr evas);

[Obsolete ("Use Create() instead.")]
public static GRGlInterface CreateNativeGlInterface ();
```


#### Type Changed: SkiaSharp.GRGlTextureInfo

Added property:

```csharp
public bool Protected { get; set; }
```


#### Type Changed: SkiaSharp.GRRecordingContext

Modified properties:

```diff
 public ---virtual--- GRBackend Backend { get; }
```

Added properties:

```csharp
public virtual bool IsAbandoned { get; }
public int MaxRenderTargetSize { get; }
public int MaxTextureSize { get; }
```


#### Type Changed: SkiaSharp.SKAbstractManagedStream

Modified methods:

```diff
-protected abstract IntPtr OnCreateNew ()
+protected abstract IntPtr OnCreateNew ()
-protected virtual IntPtr OnDuplicate ()
+protected virtual IntPtr OnDuplicate ()
-protected virtual IntPtr OnFork ()
+protected virtual IntPtr OnFork ()
-protected abstract IntPtr OnGetLength ()
+protected abstract IntPtr OnGetLength ()
-protected abstract IntPtr OnGetPosition ()
+protected abstract IntPtr OnGetPosition ()
-protected abstract bool OnHasLength ()
+protected abstract bool OnHasLength ()
-protected abstract bool OnHasPosition ()
+protected abstract bool OnHasPosition ()
-protected abstract bool OnIsAtEnd ()
+protected abstract bool OnIsAtEnd ()
-protected abstract bool OnMove (int offset)
+protected abstract bool OnMove (int offset)
-protected abstract IntPtr OnPeek (IntPtr buffer, IntPtr size)
+protected abstract IntPtr OnPeek (IntPtr buffer, IntPtr size)
-protected abstract IntPtr OnRead (IntPtr buffer, IntPtr size)
+protected abstract IntPtr OnRead (IntPtr buffer, IntPtr size)
-protected abstract bool OnRewind ()
+protected abstract bool OnRewind ()
-protected abstract bool OnSeek (IntPtr position)
+protected abstract bool OnSeek (IntPtr position)
```


#### Type Changed: SkiaSharp.SKAbstractManagedWStream

Modified methods:

```diff
-protected abstract IntPtr OnBytesWritten ()
+protected abstract IntPtr OnBytesWritten ()
-protected abstract void OnFlush ()
+protected abstract void OnFlush ()
-protected abstract bool OnWrite (IntPtr buffer, IntPtr size)
+protected abstract bool OnWrite (IntPtr buffer, IntPtr size)
```


#### Type Changed: SkiaSharp.SKBitmap

Removed constructors:

```csharp
[Obsolete ("The Index8 color type and color table is no longer supported. Use SKBitmap(SKImageInfo) instead.")]
public SKBitmap (SKImageInfo info, SKColorTable ctable);

[Obsolete ("The Index8 color type and color table is no longer supported. Use SKBitmap(SKImageInfo, SKBitmapAllocFlags) instead.")]
public SKBitmap (SKImageInfo info, SKColorTable ctable, SKBitmapAllocFlags flags);
```

Removed properties:

```csharp
[Obsolete ("The Index8 color type and color table is no longer supported.")]
public SKColorTable ColorTable { get; }

[Obsolete]
public bool IsVolatile { get; set; }
```

Removed methods:

```csharp
[Obsolete ("Use GetAddress instead.")]
public IntPtr GetAddr (int x, int y);

[Obsolete]
public ushort GetAddr16 (int x, int y);

[Obsolete]
public uint GetAddr32 (int x, int y);

[Obsolete]
public byte GetAddr8 (int x, int y);

[Obsolete ("The Index8 color type and color table is no longer supported. Use GetPixel(int, int) instead.")]
public SKPMColor GetIndex8Color (int x, int y);
public System.ReadOnlySpan<byte> GetPixelSpan ();
public bool InstallMaskPixels (SKMask mask);

[Obsolete ("The Index8 color type and color table is no longer supported. Use InstallPixels(SKImageInfo, IntPtr, int) instead.")]
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);

[Obsolete ("The Index8 color type and color table is no longer supported. Use InstallPixels(SKImageInfo, IntPtr, int, SKBitmapReleaseDelegate, object) instead.")]
public bool InstallPixels (SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable, SKBitmapReleaseDelegate releaseProc, object context);

[Obsolete ("Use ScalePixels(SKBitmap, SKFilterQuality) instead.")]
public bool Resize (SKBitmap dst, SKBitmapResizeMethod method);

[Obsolete ("Use Resize(SKImageInfo, SKFilterQuality) instead.")]
public SKBitmap Resize (SKImageInfo info, SKBitmapResizeMethod method);

[Obsolete ("Use ScalePixels(SKBitmap, SKFilterQuality) instead.")]
public static bool Resize (SKBitmap dst, SKBitmap src, SKBitmapResizeMethod method);

[Obsolete ("The Index8 color type and color table is no longer supported.")]
public void SetColorTable (SKColorTable ct);

[Obsolete ("The Index8 color type and color table is no longer supported. Use SetPixels(IntPtr) instead.")]
public void SetPixels (IntPtr pixels, SKColorTable ct);
```

Obsoleted methods:

```diff
 [Obsolete ("Use Resize(SKImageInfo info, SKSamplingOptions sampling) instead.")]
 public SKBitmap Resize (SKImageInfo info, SKFilterQuality quality);
 [Obsolete ("Use Resize(SKSizeI size, SKSamplingOptions sampling) instead.")]
 public SKBitmap Resize (SKSizeI size, SKFilterQuality quality);
 [Obsolete ("Use ScalePixels(SKBitmap destination, SKSamplingOptions sampling) instead.")]
 public bool ScalePixels (SKBitmap destination, SKFilterQuality quality);
 [Obsolete ("Use ScalePixels(SKPixmap destination, SKSamplingOptions sampling) instead.")]
 public bool ScalePixels (SKPixmap destination, SKFilterQuality quality);
```

Added methods:

```csharp
public System.Span<byte> GetPixelSpan ();
public System.Span<byte> GetPixelSpan (int x, int y);
public SKBitmap Resize (SKImageInfo info, SKSamplingOptions sampling);
public SKBitmap Resize (SKSizeI size, SKSamplingOptions sampling);
public bool ScalePixels (SKBitmap destination, SKSamplingOptions sampling);
public bool ScalePixels (SKPixmap destination, SKSamplingOptions sampling);

[Obsolete ("Use ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) instead.")]
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterQuality quality);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling);

[Obsolete ("Use ToShader(SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling, SKMatrix localMatrix) instead.")]
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterQuality quality, SKMatrix localMatrix);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling, SKMatrix localMatrix);
```


#### Type Changed: SkiaSharp.SKCanvas

Added property:

```csharp
public SKMatrix44 TotalMatrix44 { get; }
```

Removed methods:

```csharp
public void Concat (ref SKMatrix m);
public void DrawDrawable (SKDrawable drawable, ref SKMatrix matrix);
public void DrawPicture (SKPicture picture, ref SKMatrix matrix, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawPositionedText (byte[] text, SKPoint[] points, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawPositionedText (string text, SKPoint[] points, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawPositionedText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawText (byte[] text, SKPoint p, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawText (byte[] text, float x, float y, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawText (IntPtr buffer, int length, SKPoint p, SKPaint paint);

[Obsolete ("Use DrawText(SKTextBlob, float, float, SKPaint) instead.")]
public void DrawText (IntPtr buffer, int length, float x, float y, SKPaint paint);

[Obsolete ("Use DrawTextOnPath(string, SKPath, SKPoint, SKPaint) instead.")]
public void DrawTextOnPath (byte[] text, SKPath path, SKPoint offset, SKPaint paint);

[Obsolete ("Use DrawTextOnPath(string, SKPath, float, float, SKPaint) instead.")]
public void DrawTextOnPath (byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint);

[Obsolete ("Use DrawTextOnPath(string, SKPath, SKPoint, SKPaint) instead.")]
public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, SKPoint offset, SKPaint paint);

[Obsolete ("Use DrawTextOnPath(string, SKPath, float, float, SKPaint) instead.")]
public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint);
```

Modified methods:

```diff
-public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKPaint paint = NULL)
+public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKPaint paint)
-public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKPaint paint = NULL)
+public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKPaint paint)
-public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKRect cullRect, SKPaint paint = NULL)
+public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKRect cullRect, SKPaint paint)
```

Obsoleted methods:

```diff
 [Obsolete ("Use DrawText(string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
 public void DrawText (string text, SKPoint p, SKPaint paint);
 [Obsolete ("Use DrawText(string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
 public void DrawText (string text, float x, float y, SKPaint paint);
 [Obsolete ("Use DrawTextOnPath(string text, SKPath path, float hOffset, float vOffset, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
 public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKPaint paint);
 [Obsolete ("Use DrawTextOnPath(string text, SKPath path, SKPoint offset, bool warpGlyphs, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
 public void DrawTextOnPath (string text, SKPath path, SKPoint offset, bool warpGlyphs, SKPaint paint);
 [Obsolete ("Use DrawTextOnPath(string text, SKPath path, float hOffset, float vOffset, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
 public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKPaint paint);
 [Obsolete ("Use SetMatrix(in SKMatrix) instead.")]
 public void SetMatrix (SKMatrix matrix);
```

Added methods:

```csharp
public void Concat (ref SKMatrix m);
public void Concat (ref SKMatrix44 m);
public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKSamplingOptions sampling, SKPaint paint);
public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKSamplingOptions sampling, SKPaint paint);
public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKSamplingOptions sampling, SKRect cullRect, SKPaint paint);
public void DrawBitmapLattice (SKBitmap bitmap, SKLattice lattice, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawBitmapLattice (SKBitmap bitmap, int[] xDivs, int[] yDivs, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawBitmapNinePatch (SKBitmap bitmap, SKRectI center, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawDrawable (SKDrawable drawable, ref SKMatrix matrix);
public void DrawImage (SKImage image, SKPoint p, SKSamplingOptions sampling, SKPaint paint);
public void DrawImage (SKImage image, SKRect dest, SKSamplingOptions sampling, SKPaint paint);
public void DrawImage (SKImage image, SKRect source, SKRect dest, SKSamplingOptions sampling, SKPaint paint);
public void DrawImage (SKImage image, float x, float y, SKSamplingOptions sampling, SKPaint paint);
public void DrawImageLattice (SKImage image, SKLattice lattice, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawImageLattice (SKImage image, int[] xDivs, int[] yDivs, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawImageNinePatch (SKImage image, SKRectI center, SKRect dst, SKFilterMode filterMode, SKPaint paint);
public void DrawPicture (SKPicture picture, ref SKMatrix matrix, SKPaint paint);
public void DrawText (string text, SKPoint p, SKFont font, SKPaint paint);
public void DrawText (string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint);
public void DrawText (string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKFont font, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKTextAlign textAlign, SKFont font, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKFont font, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, SKPoint offset, bool warpGlyphs, SKTextAlign textAlign, SKFont font, SKPaint paint);
public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKTextAlign textAlign, SKFont font, SKPaint paint);
public void SetMatrix (ref SKMatrix matrix);
public void SetMatrix (ref SKMatrix44 matrix);
```


#### Type Changed: SkiaSharp.SKCodec

Removed property:

```csharp
[Obsolete ("Use EncodedOrigin instead.")]
public SKCodecOrigin Origin { get; }
```

Removed methods:

```csharp
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


#### Type Changed: SkiaSharp.SKCodecFrameInfo

Added properties:

```csharp
public SKCodecAnimationBlend Blend { get; set; }
public SKRectI FrameRect { get; set; }
public bool HasAlphaWithinBounds { get; set; }
```


#### Type Changed: SkiaSharp.SKCodecOptions

Removed property:

```csharp
[Obsolete]
public SKTransferFunctionBehavior PremulBehavior { get; set; }
```


#### Type Changed: SkiaSharp.SKColorFilter

Added methods:

```csharp
public static SKColorFilter CreateColorMatrix (System.ReadOnlySpan<float> matrix);
public static SKColorFilter CreateTable (System.ReadOnlySpan<byte> table);
public static SKColorFilter CreateTable (System.ReadOnlySpan<byte> tableA, System.ReadOnlySpan<byte> tableR, System.ReadOnlySpan<byte> tableG, System.ReadOnlySpan<byte> tableB);
```


#### Type Changed: SkiaSharp.SKColorSpace

Removed properties:

```csharp
[Obsolete ("Use GetNumericalTransferFunction() instead.")]
public SKNamedGamma NamedGamma { get; }

[Obsolete]
public SKColorSpaceType Type { get; }
```

Removed methods:

```csharp
[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKColorSpaceGamut gamut);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKNamedGamma gamma, SKMatrix44 toXyzD50);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);

[Obsolete ("Use CreateRgb(SKColorSpaceTransferFn, SKColorSpaceXyz) instead.")]
public static SKColorSpace CreateRgb (SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);

[Obsolete]
public SKMatrix44 FromXyzD50 ();

[Obsolete ("Use ToColorSpaceXyz() instead.")]
public SKMatrix44 ToXyzD50 ();

[Obsolete ("Use ToColorSpaceXyz(out SKColorSpaceXyz) instead.")]
public bool ToXyzD50 (SKMatrix44 toXyzD50);
```


#### Type Changed: SkiaSharp.SKColorSpacePrimaries

Removed methods:

```csharp
[Obsolete ("Use ToColorSpaceXyz() instead.")]
public SKMatrix44 ToXyzD50 ();

[Obsolete ("Use ToColorSpaceXyz(out SKColorSpaceXyz) instead.")]
public bool ToXyzD50 (SKMatrix44 toXyzD50);
```


#### Type Changed: SkiaSharp.SKColorSpaceXyz

Added field:

```csharp
public static SKColorSpaceXyz Identity;
```

Removed property:

```csharp
[Obsolete ("Use DisplayP3 instead.")]
public static SKColorSpaceXyz Dcip3 { get; }
```


#### Type Changed: SkiaSharp.SKColorType

Added values:

```csharp
Bgr101010xXR = 21,
R8Unorm = 23,
Srgba8888 = 22,
```


#### Type Changed: SkiaSharp.SKDocument

Removed method:

```csharp
[Obsolete ("Use CreatePdf(SKWStream, SKDocumentPdfMetadata) instead.")]
public static SKDocument CreatePdf (SKWStream stream, SKDocumentPdfMetadata metadata, float dpi);
```


#### Type Changed: SkiaSharp.SKDrawable

Added property:

```csharp
public int ApproximateBytesUsed { get; }
```

Removed method:

```csharp
public void Draw (SKCanvas canvas, ref SKMatrix matrix);
```

Modified methods:

```diff
-protected virtual void OnDraw (SKCanvas canvas)
+protected virtual void OnDraw (SKCanvas canvas)
-protected virtual SKRect OnGetBounds ()
+protected virtual SKRect OnGetBounds ()
-protected virtual SKPicture OnSnapshot ()
+protected virtual SKPicture OnSnapshot ()
```

Added methods:

```csharp
public void Draw (SKCanvas canvas, ref SKMatrix matrix);
protected virtual int OnGetApproximateBytesUsed ();
```


#### Type Changed: SkiaSharp.SKEncodedImageFormat

Added value:

```csharp
Jpegxl = 13,
```


#### Type Changed: SkiaSharp.SKFont

Added methods:

```csharp
public int BreakText (System.ReadOnlySpan<char> text, float maxWidth, SKPaint paint);
public int BreakText (string text, float maxWidth, SKPaint paint);
public int BreakText (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, float maxWidth, SKPaint paint);
public int BreakText (System.ReadOnlySpan<char> text, float maxWidth, out float measuredWidth, SKPaint paint);
public int BreakText (string text, float maxWidth, out float measuredWidth, SKPaint paint);
public int BreakText (IntPtr text, int length, SKTextEncoding encoding, float maxWidth, SKPaint paint);
public int BreakText (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, float maxWidth, out float measuredWidth, SKPaint paint);
public int BreakText (IntPtr text, int length, SKTextEncoding encoding, float maxWidth, out float measuredWidth, SKPaint paint);
public float[] GetGlyphOffsets (System.ReadOnlySpan<char> text, float origin);
public float[] GetGlyphOffsets (System.ReadOnlySpan<ushort> glyphs, float origin);
public float[] GetGlyphOffsets (string text, float origin);
public float[] GetGlyphOffsets (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, float origin);
public void GetGlyphOffsets (System.ReadOnlySpan<char> text, System.Span<float> offsets, float origin);
public void GetGlyphOffsets (string text, System.Span<float> offsets, float origin);
public float[] GetGlyphOffsets (IntPtr text, int length, SKTextEncoding encoding, float origin);
public void GetGlyphOffsets (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, System.Span<float> offsets, float origin);
public void GetGlyphOffsets (IntPtr text, int length, SKTextEncoding encoding, System.Span<float> offsets, float origin);
public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<char> text, SKPoint origin);
public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<ushort> glyphs, SKPoint origin);
public SKPoint[] GetGlyphPositions (string text, SKPoint origin);
public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPoint origin);
public void GetGlyphPositions (System.ReadOnlySpan<char> text, System.Span<SKPoint> offsets, SKPoint origin);
public void GetGlyphPositions (string text, System.Span<SKPoint> offsets, SKPoint origin);
public SKPoint[] GetGlyphPositions (IntPtr text, int length, SKTextEncoding encoding, SKPoint origin);
public void GetGlyphPositions (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, System.Span<SKPoint> offsets, SKPoint origin);
public void GetGlyphPositions (IntPtr text, int length, SKTextEncoding encoding, System.Span<SKPoint> offsets, SKPoint origin);
public float[] GetGlyphWidths (System.ReadOnlySpan<char> text, SKPaint paint);
public float[] GetGlyphWidths (System.ReadOnlySpan<ushort> glyphs, SKPaint paint);
public float[] GetGlyphWidths (string text, SKPaint paint);
public float[] GetGlyphWidths (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPaint paint);
public float[] GetGlyphWidths (System.ReadOnlySpan<char> text, out SKRect[] bounds, SKPaint paint);
public float[] GetGlyphWidths (System.ReadOnlySpan<ushort> glyphs, out SKRect[] bounds, SKPaint paint);
public float[] GetGlyphWidths (string text, out SKRect[] bounds, SKPaint paint);
public float[] GetGlyphWidths (IntPtr text, int length, SKTextEncoding encoding, SKPaint paint);
public float[] GetGlyphWidths (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, out SKRect[] bounds, SKPaint paint);
public void GetGlyphWidths (System.ReadOnlySpan<char> text, System.Span<float> widths, System.Span<SKRect> bounds, SKPaint paint);
public void GetGlyphWidths (string text, System.Span<float> widths, System.Span<SKRect> bounds, SKPaint paint);
public float[] GetGlyphWidths (IntPtr text, int length, SKTextEncoding encoding, out SKRect[] bounds, SKPaint paint);
public void GetGlyphWidths (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, System.Span<float> widths, System.Span<SKRect> bounds, SKPaint paint);
public void GetGlyphWidths (IntPtr text, int length, SKTextEncoding encoding, System.Span<float> widths, System.Span<SKRect> bounds, SKPaint paint);
public ushort[] GetGlyphs (System.ReadOnlySpan<char> text);
public ushort[] GetGlyphs (System.ReadOnlySpan<int> codepoints);
public ushort[] GetGlyphs (string text);
public ushort[] GetGlyphs (System.ReadOnlySpan<byte> text, SKTextEncoding encoding);
public ushort[] GetGlyphs (IntPtr text, int length, SKTextEncoding encoding);
public SKPath GetTextPath (System.ReadOnlySpan<char> text, SKPoint origin);
public SKPath GetTextPath (System.ReadOnlySpan<char> text, System.ReadOnlySpan<SKPoint> positions);
public SKPath GetTextPath (string text, SKPoint origin);
public SKPath GetTextPath (string text, System.ReadOnlySpan<SKPoint> positions);
public SKPath GetTextPath (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPoint origin);
public SKPath GetTextPath (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, System.ReadOnlySpan<SKPoint> positions);
public SKPath GetTextPath (IntPtr text, int length, SKTextEncoding encoding, SKPoint origin);
public SKPath GetTextPath (IntPtr text, int length, SKTextEncoding encoding, System.ReadOnlySpan<SKPoint> positions);
public SKPath GetTextPathOnPath (System.ReadOnlySpan<char> text, SKPath path, SKTextAlign textAlign, SKPoint origin);
public SKPath GetTextPathOnPath (System.ReadOnlySpan<ushort> glyphs, SKPath path, SKTextAlign textAlign, SKPoint origin);
public SKPath GetTextPathOnPath (string text, SKPath path, SKTextAlign textAlign, SKPoint origin);
public SKPath GetTextPathOnPath (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPath path, SKTextAlign textAlign, SKPoint origin);
public SKPath GetTextPathOnPath (System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> glyphWidths, System.ReadOnlySpan<SKPoint> glyphPositions, SKPath path, SKTextAlign textAlign);
public SKPath GetTextPathOnPath (IntPtr text, int length, SKTextEncoding encoding, SKPath path, SKTextAlign textAlign, SKPoint origin);
public float MeasureText (System.ReadOnlySpan<char> text, SKPaint paint);
public float MeasureText (string text, SKPaint paint);
public float MeasureText (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, SKPaint paint);
public float MeasureText (System.ReadOnlySpan<char> text, out SKRect bounds, SKPaint paint);
public float MeasureText (string text, out SKRect bounds, SKPaint paint);
public float MeasureText (IntPtr text, int length, SKTextEncoding encoding, SKPaint paint);
public float MeasureText (System.ReadOnlySpan<byte> text, SKTextEncoding encoding, out SKRect bounds, SKPaint paint);
public float MeasureText (IntPtr text, int length, SKTextEncoding encoding, out SKRect bounds, SKPaint paint);
```


#### Type Changed: SkiaSharp.SKFontManager

Removed method:

```csharp
public SKTypeface MatchTypeface (SKTypeface face, SKFontStyle style);
```


#### Type Changed: SkiaSharp.SKFrontBufferedManagedStream

Modified methods:

```diff
-protected override IntPtr OnCreateNew ()
+protected override IntPtr OnCreateNew ()
-protected override IntPtr OnGetLength ()
+protected override IntPtr OnGetLength ()
-protected override IntPtr OnGetPosition ()
+protected override IntPtr OnGetPosition ()
-protected override bool OnHasLength ()
+protected override bool OnHasLength ()
-protected override bool OnHasPosition ()
+protected override bool OnHasPosition ()
-protected override bool OnIsAtEnd ()
+protected override bool OnIsAtEnd ()
-protected override bool OnMove (int offset)
+protected override bool OnMove (int offset)
-protected override IntPtr OnPeek (IntPtr buffer, IntPtr size)
+protected override IntPtr OnPeek (IntPtr buffer, IntPtr size)
-protected override IntPtr OnRead (IntPtr buffer, IntPtr size)
+protected override IntPtr OnRead (IntPtr buffer, IntPtr size)
-protected override bool OnRewind ()
+protected override bool OnRewind ()
-protected override bool OnSeek (IntPtr position)
+protected override bool OnSeek (IntPtr position)
```


#### Type Changed: SkiaSharp.SKGraphics

Removed methods:

```csharp
public static int GetFontCachePointSizeLimit ();
public static int SetFontCachePointSizeLimit (int count);
```


#### Type Changed: SkiaSharp.SKHorizontalRunBuffer

Added property:

```csharp
public System.Span<float> Positions { get; }
```

Obsoleted methods:

```diff
 [Obsolete ("Use Positions instead.")]
 public System.Span<float> GetPositionSpan ();
```


#### Type Changed: SkiaSharp.SKImage

Removed methods:

```csharp
[Obsolete]
public SKData Encode (SKPixelSerializer serializer);

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

[Obsolete ("Use FromPixels (SKImageInfo, SKData, int) instead.")]
public static SKImage FromPixelData (SKImageInfo info, SKData data, int rowBytes);

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

[Obsolete ("Use ToTextureImage(GRContext) instead.")]
public SKImage ToTextureImage (GRContext context, SKColorSpace colorspace);
```

Obsoleted methods:

```diff
 [Obsolete ("Use ScalePixels(SKPixmap dst, SKSamplingOptions sampling) instead.")]
 public bool ScalePixels (SKPixmap dst, SKFilterQuality quality);
 [Obsolete ("Use ScalePixels(SKPixmap dst, SKSamplingOptions sampling, SKImageCachingHint cachingHint) instead.")]
 public bool ScalePixels (SKPixmap dst, SKFilterQuality quality, SKImageCachingHint cachingHint);
```

Added methods:

```csharp
public bool ScalePixels (SKPixmap dst, SKSamplingOptions sampling);
public bool ScalePixels (SKPixmap dst, SKSamplingOptions sampling, SKImageCachingHint cachingHint);
public SKImage Subset (GRRecordingContext context, SKRectI subset);
public SKShader ToRawShader ();
public SKShader ToRawShader (SKShaderTileMode tileX, SKShaderTileMode tileY);
public SKShader ToRawShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix localMatrix);
public SKShader ToRawShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling);
public SKShader ToRawShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix localMatrix);

[Obsolete ("Use ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling) instead.")]
public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKFilterQuality quality);
public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling);

[Obsolete ("Use ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix localMatrix) instead.")]
public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKFilterQuality quality, SKMatrix localMatrix);
public SKShader ToShader (SKShaderTileMode tileX, SKShaderTileMode tileY, SKSamplingOptions sampling, SKMatrix localMatrix);
public SKImage ToTextureImage (GRContext context, bool mipmapped, bool budgeted);
```


#### Type Changed: SkiaSharp.SKImageFilter

Removed methods:

```csharp
public static SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold);
public static SKImageFilter CreateAlphaThreshold (SKRectI region, float innerThreshold, float outerThreshold, SKImageFilter input);
public static SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold, SKImageFilter input);
public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter background, SKImageFilter foreground, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDilate (int radiusX, int radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);

[Obsolete ("Use CreateDisplacementMapEffect(SKColorChannel, SKColorChannel, float, SKImageFilter, SKImageFilter, SKImageFilter.CropRect) instead.")]
public static SKImageFilter CreateDisplacementMapEffect (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKImageFilter.CropRect cropRect);

[Obsolete ("Use CreateDropShadow or CreateDropShadowOnly instead.")]
public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateErode (int radiusX, int radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMagnifier (SKRect src, float inset);
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input);
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, System.ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);

[Obsolete ("Use CreateMatrixConvolution(SKSizeI, ReadOnlySpan<float>, float, float, SKPointI, SKShaderTileMode, bool, SKImageFilter, SKImageFilter.CropRect) instead.")]
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (System.ReadOnlySpan<SKImageFilter> filters, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKImageFilter.CropRect cropRect);

[Obsolete ("Use CreateMerge(ReadOnlySpan<SKImageFilter>, SKImageFilter.CropRect) instead.")]
public static SKImageFilter CreateMerge (SKImageFilter[] filters, SKBlendMode[] modes, SKImageFilter.CropRect cropRect);

[Obsolete ("Use CreateMerge(SKImageFilter, SKImageFilter, SKImageFilter.CropRect) instead.")]
public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, SKBlendMode mode, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateOffset (float dx, float dy, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePaint (SKPaint paint, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateShader (SKShader shader, bool dither, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input, SKImageFilter.CropRect cropRect);
public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input, SKImageFilter.CropRect cropRect);
```

Obsoleted methods:

```diff
 [Obsolete ("Use CreateImage(SKImage, SKRect, SKRect, SKSamplingOptions) instead.")]
 public static SKImageFilter CreateImage (SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality);
 [Obsolete ("Use SetMatrix(in SKMatrix) instead.")]
 public static SKImageFilter CreateMatrix (SKMatrix matrix);
 [Obsolete ("Use SetMatrix(in SKMatrix, SKSamplingOptions, SKImageFilter) instead.")]
 public static SKImageFilter CreateMatrix (SKMatrix matrix, SKFilterQuality quality, SKImageFilter input);
 [Obsolete ("Use CreateShader(SKShader) instead.")]
 public static SKImageFilter CreatePaint (SKPaint paint);
 [Obsolete ("Use CreateShader(SKShader, bool, SKRect) instead.")]
 public static SKImageFilter CreatePaint (SKPaint paint, SKRect cropRect);
```

Modified methods:

```diff
 public SKImageFilter CreateMatrix (SKMatrix matrix, SKFilterQuality quality, SKImageFilter input--- = NULL---)
```

Added methods:

```csharp
public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background);
public static SKImageFilter CreateBlendMode (SKBlender blender, SKImageFilter background);
public static SKImageFilter CreateBlendMode (SKBlender blender, SKImageFilter background, SKImageFilter foreground);
public static SKImageFilter CreateBlendMode (SKBlender blender, SKImageFilter background, SKImageFilter foreground, SKRect cropRect);
public static SKImageFilter CreateImage (SKImage image, SKSamplingOptions sampling);
public static SKImageFilter CreateImage (SKImage image, SKRect src, SKRect dst, SKSamplingOptions sampling);
public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling);
public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter input);
public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter input, SKRect cropRect);
public static SKImageFilter CreateMatrix (ref SKMatrix matrix);
public static SKImageFilter CreateMatrix (ref SKMatrix matrix, SKSamplingOptions sampling);
public static SKImageFilter CreateMatrix (ref SKMatrix matrix, SKSamplingOptions sampling, SKImageFilter input);
public static SKImageFilter CreateMerge (System.ReadOnlySpan<SKImageFilter> filters, SKRect* cropRect);
```

#### Removed Type SkiaSharp.SKImageFilter.CropRect

#### Type Changed: SkiaSharp.SKImageInfo

Added property:

```csharp
public int BitShiftPerPixel { get; }
```


#### Type Changed: SkiaSharp.SKJpegEncoderOptions

Removed constructor:

```csharp
[Obsolete ("Use SKJpegEncoderOptions(int, SKJpegEncoderDownsample, SKJpegEncoderAlphaOption) instead.")]
public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption, SKTransferFunctionBehavior blendBehavior);
```

Added constructor:

```csharp
public SKJpegEncoderOptions (int quality);
```

Removed property:

```csharp
[Obsolete]
public SKTransferFunctionBehavior BlendBehavior { get; set; }
```

Modified properties:

```diff
 public SKJpegEncoderAlphaOption AlphaOption { get; ---set;--- }
 public SKJpegEncoderDownsample Downsample { get; ---set;--- }
 public int Quality { get; ---set;--- }
```


#### Type Changed: SkiaSharp.SKManagedStream

Modified methods:

```diff
-protected override IntPtr OnCreateNew ()
+protected override IntPtr OnCreateNew ()
-protected override IntPtr OnDuplicate ()
+protected override IntPtr OnDuplicate ()
-protected override IntPtr OnFork ()
+protected override IntPtr OnFork ()
-protected override IntPtr OnGetLength ()
+protected override IntPtr OnGetLength ()
-protected override IntPtr OnGetPosition ()
+protected override IntPtr OnGetPosition ()
-protected override bool OnHasLength ()
+protected override bool OnHasLength ()
-protected override bool OnHasPosition ()
+protected override bool OnHasPosition ()
-protected override bool OnIsAtEnd ()
+protected override bool OnIsAtEnd ()
-protected override bool OnMove (int offset)
+protected override bool OnMove (int offset)
-protected override IntPtr OnPeek (IntPtr buffer, IntPtr size)
+protected override IntPtr OnPeek (IntPtr buffer, IntPtr size)
-protected override IntPtr OnRead (IntPtr buffer, IntPtr size)
+protected override IntPtr OnRead (IntPtr buffer, IntPtr size)
-protected override bool OnRewind ()
+protected override bool OnRewind ()
-protected override bool OnSeek (IntPtr position)
+protected override bool OnSeek (IntPtr position)
```


#### Type Changed: SkiaSharp.SKManagedWStream

Modified methods:

```diff
-protected override IntPtr OnBytesWritten ()
+protected override IntPtr OnBytesWritten ()
-protected override void OnFlush ()
+protected override void OnFlush ()
-protected override bool OnWrite (IntPtr buffer, IntPtr size)
+protected override bool OnWrite (IntPtr buffer, IntPtr size)
```


#### Type Changed: SkiaSharp.SKMaskFilter

Removed methods:

```csharp
[Obsolete ("Use CreateBlur(SKBlurStyle, float) instead.")]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags);

[Obsolete ("Use CreateBlur(SKBlurStyle, float) instead.")]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder);

[Obsolete ("Use CreateBlur(SKBlurStyle, float) instead.")]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags);

[Obsolete ("Use CreateBlur(SKBlurStyle, float, bool) instead.")]
public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM);
```


#### Type Changed: SkiaSharp.SKMatrix

Removed methods:

```csharp
public static void Concat (ref SKMatrix target, ref SKMatrix first, ref SKMatrix second);

[Obsolete ("Use CreateIdentity() instead.")]
public static SKMatrix MakeIdentity ();

[Obsolete ("Use CreateRotation(float) instead.")]
public static SKMatrix MakeRotation (float radians);

[Obsolete ("Use CreateRotation(float, float, float) instead.")]
public static SKMatrix MakeRotation (float radians, float pivotx, float pivoty);

[Obsolete ("Use CreateRotationDegrees(float) instead.")]
public static SKMatrix MakeRotationDegrees (float degrees);

[Obsolete ("Use CreateRotationDegrees(float, float, float) instead.")]
public static SKMatrix MakeRotationDegrees (float degrees, float pivotx, float pivoty);

[Obsolete ("Use CreateScale(float, float) instead.")]
public static SKMatrix MakeScale (float sx, float sy);

[Obsolete ("Use CreateScale(float, float, float, float) instead.")]
public static SKMatrix MakeScale (float sx, float sy, float pivotX, float pivotY);

[Obsolete ("Use CreateSkew(float, float) instead.")]
public static SKMatrix MakeSkew (float sx, float sy);

[Obsolete ("Use CreateTranslation(float, float) instead.")]
public static SKMatrix MakeTranslation (float dx, float dy);

[Obsolete ("Use MapRect(SKRect) instead.")]
public static void MapRect (ref SKMatrix matrix, out SKRect dest, ref SKRect source);

[Obsolete ("Use PostConcat(SKMatrix) instead.")]
public static void PostConcat (ref SKMatrix target, SKMatrix matrix);

[Obsolete ("Use PostConcat(SKMatrix) instead.")]
public static void PostConcat (ref SKMatrix target, ref SKMatrix matrix);

[Obsolete ("Use PreConcat(SKMatrix) instead.")]
public static void PreConcat (ref SKMatrix target, SKMatrix matrix);

[Obsolete ("Use PreConcat(SKMatrix) instead.")]
public static void PreConcat (ref SKMatrix target, ref SKMatrix matrix);

[Obsolete ("Use CreateRotation(float) instead.")]
public static void Rotate (ref SKMatrix matrix, float radians);

[Obsolete ("Use CreateRotation(float, float, float) instead.")]
public static void Rotate (ref SKMatrix matrix, float radians, float pivotx, float pivoty);

[Obsolete ("Use CreateRotationDegrees(float) instead.")]
public static void RotateDegrees (ref SKMatrix matrix, float degrees);

[Obsolete ("Use CreateRotationDegrees(float, float, float) instead.")]
public static void RotateDegrees (ref SKMatrix matrix, float degrees, float pivotx, float pivoty);

[Obsolete ("Use CreateScaleTranslation(float, float, float, float) instead.")]
public void SetScaleTranslate (float sx, float sy, float tx, float ty);
```

Added methods:

```csharp
public void MapPoints (System.Span<SKPoint> result, System.ReadOnlySpan<SKPoint> points);
public void MapVectors (System.Span<SKPoint> result, System.ReadOnlySpan<SKPoint> vectors);
```


#### Type Changed: SkiaSharp.SKMatrix44

Modified base type:

```diff
-SkiaSharp.SKObject
+System.ValueType
```

Removed constructor:

```csharp
public SKMatrix44 (SKMatrix44 a, SKMatrix44 b);
```

Added constructor:

```csharp
public SKMatrix44 (float m00, float m01, float m02, float m03, float m10, float m11, float m12, float m13, float m20, float m21, float m22, float m23, float m30, float m31, float m32, float m33);
```

Removed interface:

```csharp
System.IDisposable
```

Added interface:

```csharp
System.IEquatable<SKMatrix44>
```

Added fields:

```csharp
public static SKMatrix44 Empty;
public static SKMatrix44 Identity;
```

Removed property:

```csharp
public SKMatrix44TypeMask Type { get; }
```

Added properties:

```csharp
public float M00 { get; set; }
public float M01 { get; set; }
public float M02 { get; set; }
public float M03 { get; set; }
public float M10 { get; set; }
public float M11 { get; set; }
public float M12 { get; set; }
public float M13 { get; set; }
public float M20 { get; set; }
public float M21 { get; set; }
public float M22 { get; set; }
public float M23 { get; set; }
public float M30 { get; set; }
public float M31 { get; set; }
public float M32 { get; set; }
public float M33 { get; set; }
```

Removed methods:

```csharp
public static SKMatrix44 CreateTranslate (float x, float y, float z);
public double Determinant ();
protected override void Dispose (bool disposing);
protected override void DisposeNative ();
public static bool Equal (SKMatrix44 left, SKMatrix44 right);
public static SKMatrix44 FromColumnMajor (float[] src);
public static SKMatrix44 FromRowMajor (float[] src);
public bool Invert (SKMatrix44 inverse);
public SKPoint[] MapPoints (SKPoint[] src);
public float[] MapScalars (float[] srcVector4);
public void MapScalars (float[] srcVector4, float[] dstVector4);
public float[] MapScalars (float x, float y, float z, float w);
public float[] MapVector2 (float[] src2);
public void MapVector2 (float[] src2, float[] dst4);
public void PostConcat (SKMatrix44 m);
public void PostScale (float sx, float sy, float sz);
public void PostTranslate (float dx, float dy, float dz);
public void PreConcat (SKMatrix44 m);
public void PreScale (float sx, float sy, float sz);
public void PreTranslate (float dx, float dy, float dz);
public bool Preserves2DAxisAlignment (float epsilon);
public void Set3x3ColumnMajor (float[] src);
public void Set3x3RowMajor (float[] src);
public void SetColumnMajor (float[] src);
public void SetConcat (SKMatrix44 a, SKMatrix44 b);
public void SetIdentity ();
public void SetRotationAbout (float x, float y, float z, float radians);
public void SetRotationAboutDegrees (float x, float y, float z, float degrees);
public void SetRotationAboutUnit (float x, float y, float z, float radians);
public void SetRowMajor (float[] src);
public void SetScale (float sx, float sy, float sz);
public void SetTranslate (float dx, float dy, float dz);
public void ToColumnMajor (float[] dst);
public void ToRowMajor (float[] dst);
public void Transpose ();
```

Modified methods:

```diff
-public SKPoint MapPoint (SKPoint src)
+public SKPoint MapPoint (SKPoint point)
```

Added methods:

```csharp
public static SKMatrix44 Add (SKMatrix44 value1, SKMatrix44 value2);
public static SKMatrix44 Concat (SKMatrix44 first, SKMatrix44 second);
public static void Concat (ref SKMatrix44 target, SKMatrix44 first, SKMatrix44 second);
public static SKMatrix44 CreateScale (float x, float y, float z, float pivotX, float pivotY, float pivotZ);
public float Determinant ();
public virtual bool Equals (SKMatrix44 obj);
public override bool Equals (object obj);
public static SKMatrix44 FromColumnMajor (System.ReadOnlySpan<float> src);
public static SKMatrix44 FromRowMajor (System.ReadOnlySpan<float> src);
public override int GetHashCode ();
public SKPoint3 MapPoint (SKPoint3 point);
public SKPoint MapPoint (float x, float y);
public SKPoint3 MapPoint (float x, float y, float z);
public static SKMatrix44 Multiply (SKMatrix44 value1, SKMatrix44 value2);
public static SKMatrix44 Multiply (SKMatrix44 value1, float value2);
public static SKMatrix44 Negate (SKMatrix44 value);
public SKMatrix44 PostConcat (SKMatrix44 matrix);
public SKMatrix44 PreConcat (SKMatrix44 matrix);
public static SKMatrix44 Subtract (SKMatrix44 value1, SKMatrix44 value2);
public void ToColumnMajor (System.Span<float> dst);
public void ToRowMajor (System.Span<float> dst);
public SKMatrix44 Transpose ();
public bool TryInvert (out SKMatrix44 inverse);
public static SKMatrix44 op_Addition (SKMatrix44 value1, SKMatrix44 value2);
public static bool op_Equality (SKMatrix44 left, SKMatrix44 right);
public static System.Numerics.Matrix4x4 op_Implicit (SKMatrix44 matrix);
public static SKMatrix44 op_Implicit (System.Numerics.Matrix4x4 matrix);
public static bool op_Inequality (SKMatrix44 left, SKMatrix44 right);
public static SKMatrix44 op_Multiply (SKMatrix44 value1, SKMatrix44 value2);
public static SKMatrix44 op_Multiply (SKMatrix44 value1, float value2);
public static SKMatrix44 op_Subtraction (SKMatrix44 value1, SKMatrix44 value2);
public static SKMatrix44 op_UnaryNegation (SKMatrix44 value);
```


#### Type Changed: SkiaSharp.SKPaint

Obsoleted constructors:

```diff
 [Obsolete ("Use SKFont instead.")]
 public SKPaint (SKFont font);
```

Removed properties:

```csharp
[Obsolete]
public bool DeviceKerningEnabled { get; set; }

[Obsolete]
public bool IsVerticalText { get; set; }
```

Obsoleted properties:

```diff
 [Obsolete ("Use SKFont.Embolden instead.")]
 public bool FakeBoldText { get; set; }
 [Obsolete ("Use SKSamplingOptions instead.")]
 public SKFilterQuality FilterQuality { get; set; }
 [Obsolete ("Use SKFont.Metrics instead.")]
 public SKFontMetrics FontMetrics { get; }
 [Obsolete ("Use SKFont.Spacing instead.")]
 public float FontSpacing { get; }
 [Obsolete ("Use SKFont.Hinting instead.")]
 public SKPaintHinting HintingLevel { get; set; }
 [Obsolete ("Use SKFont.ForceAutoHinting instead.")]
 public bool IsAutohinted { get; set; }
 [Obsolete ("Use SKFont.EmbeddedBitmaps instead.")]
 public bool IsEmbeddedBitmapText { get; set; }
 [Obsolete ("Use SKFont.LinearMetrics instead.")]
 public bool IsLinearText { get; set; }
 [Obsolete ("Use SKFont.Edging instead.")]
 public bool LcdRenderText { get; set; }
 [Obsolete ("Use SKFont.Subpixel instead.")]
 public bool SubpixelText { get; set; }
 [Obsolete ("Use SKTextAlign method overloads instead.")]
 public SKTextAlign TextAlign { get; set; }
 [Obsolete ("Use SKTextEncoding method overloads instead.")]
 public SKTextEncoding TextEncoding { get; set; }
 [Obsolete ("Use SKFont.ScaleX instead.")]
 public float TextScaleX { get; set; }
 [Obsolete ("Use SKFont.Size instead.")]
 public float TextSize { get; set; }
 [Obsolete ("Use SKFont.SkewX instead.")]
 public float TextSkewX { get; set; }
 [Obsolete ("Use SKFont.Typeface instead.")]
 public SKTypeface Typeface { get; set; }
```

Added property:

```csharp
public SKBlender Blender { get; set; }
```

Removed method:

```csharp
[Obsolete ("Use GetFontMetrics (out SKFontMetrics) instead.")]
public float GetFontMetrics (out SKFontMetrics metrics, float scale);
```

Obsoleted methods:

```diff
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (byte[] text, float maxWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (System.ReadOnlySpan<byte> text, float maxWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (System.ReadOnlySpan<char> text, float maxWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (string text, float maxWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (byte[] text, float maxWidth, out float measuredWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (IntPtr buffer, int length, float maxWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (IntPtr buffer, IntPtr length, float maxWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (System.ReadOnlySpan<byte> text, float maxWidth, out float measuredWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (System.ReadOnlySpan<char> text, float maxWidth, out float measuredWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (string text, float maxWidth, out float measuredWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (IntPtr buffer, int length, float maxWidth, out float measuredWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth);
 [Obsolete ("Use SKFont.BreakText() instead.")]
 public long BreakText (string text, float maxWidth, out float measuredWidth, out string measuredText);
 [Obsolete ("Use SKFont.ContainsGlyphs() instead.")]
 public bool ContainsGlyphs (byte[] text);
 [Obsolete ("Use SKFont.ContainsGlyphs() instead.")]
 public bool ContainsGlyphs (System.ReadOnlySpan<byte> text);
 [Obsolete ("Use SKFont.ContainsGlyphs() instead.")]
 public bool ContainsGlyphs (System.ReadOnlySpan<char> text);
 [Obsolete ("Use SKFont.ContainsGlyphs() instead.")]
 public bool ContainsGlyphs (string text);
 [Obsolete ("Use SKFont.ContainsGlyphs() instead.")]
 public bool ContainsGlyphs (IntPtr text, int length);
 [Obsolete ("Use SKFont.ContainsGlyphs() instead.")]
 public bool ContainsGlyphs (IntPtr text, IntPtr length);
 [Obsolete ("Use SKFont.CountGlyphs() instead.")]
 public int CountGlyphs (byte[] text);
 [Obsolete ("Use SKFont.CountGlyphs() instead.")]
 public int CountGlyphs (System.ReadOnlySpan<byte> text);
 [Obsolete ("Use SKFont.CountGlyphs() instead.")]
 public int CountGlyphs (System.ReadOnlySpan<char> text);
 [Obsolete ("Use SKFont.CountGlyphs() instead.")]
 public int CountGlyphs (string text);
 [Obsolete ("Use SKFont.CountGlyphs() instead.")]
 public int CountGlyphs (IntPtr text, int length);
 [Obsolete ("Use SKFont.CountGlyphs() instead.")]
 public int CountGlyphs (IntPtr text, IntPtr length);
 [Obsolete ("Use SKFont.GetFontMetrics() instead.")]
 public float GetFontMetrics (out SKFontMetrics metrics);
 [Obsolete ("Use SKFont.GetGlyphOffsets() instead.")]
 public float[] GetGlyphOffsets (System.ReadOnlySpan<byte> text, float origin);
 [Obsolete ("Use SKFont.GetGlyphOffsets() instead.")]
 public float[] GetGlyphOffsets (System.ReadOnlySpan<char> text, float origin);
 [Obsolete ("Use SKFont.GetGlyphOffsets() instead.")]
 public float[] GetGlyphOffsets (string text, float origin);
 [Obsolete ("Use SKFont.GetGlyphOffsets() instead.")]
 public float[] GetGlyphOffsets (IntPtr text, int length, float origin);
 [Obsolete ("Use SKFont.GetGlyphPositions() instead.")]
 public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<byte> text, SKPoint origin);
 [Obsolete ("Use SKFont.GetGlyphPositions() instead.")]
 public SKPoint[] GetGlyphPositions (System.ReadOnlySpan<char> text, SKPoint origin);
 [Obsolete ("Use SKFont.GetGlyphPositions() instead.")]
 public SKPoint[] GetGlyphPositions (string text, SKPoint origin);
 [Obsolete ("Use SKFont.GetGlyphPositions() instead.")]
 public SKPoint[] GetGlyphPositions (IntPtr text, int length, SKPoint origin);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (byte[] text);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (System.ReadOnlySpan<byte> text);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (System.ReadOnlySpan<char> text);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (string text);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (byte[] text, out SKRect[] bounds);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (IntPtr text, int length);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (IntPtr text, IntPtr length);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (System.ReadOnlySpan<byte> text, out SKRect[] bounds);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (System.ReadOnlySpan<char> text, out SKRect[] bounds);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (string text, out SKRect[] bounds);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (IntPtr text, int length, out SKRect[] bounds);
 [Obsolete ("Use SKFont.GetGlyphWidths() instead.")]
 public float[] GetGlyphWidths (IntPtr text, IntPtr length, out SKRect[] bounds);
 [Obsolete ("Use SKFont.GetGlyphs() instead.")]
 public ushort[] GetGlyphs (byte[] text);
 [Obsolete ("Use SKFont.GetGlyphs() instead.")]
 public ushort[] GetGlyphs (System.ReadOnlySpan<byte> text);
 [Obsolete ("Use SKFont.GetGlyphs() instead.")]
 public ushort[] GetGlyphs (System.ReadOnlySpan<char> text);
 [Obsolete ("Use SKFont.GetGlyphs() instead.")]
 public ushort[] GetGlyphs (string text);
 [Obsolete ("Use SKFont.GetGlyphs() instead.")]
 public ushort[] GetGlyphs (IntPtr text, int length);
 [Obsolete ("Use SKFont.GetGlyphs() instead.")]
 public ushort[] GetGlyphs (IntPtr text, IntPtr length);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetHorizontalTextIntercepts (byte[] text, float[] xpositions, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetHorizontalTextIntercepts (System.ReadOnlySpan<byte> text, System.ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetHorizontalTextIntercepts (System.ReadOnlySpan<char> text, System.ReadOnlySpan<float> xpositions, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetHorizontalTextIntercepts (string text, float[] xpositions, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetHorizontalTextIntercepts (IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetHorizontalTextIntercepts (IntPtr text, IntPtr length, float[] xpositions, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetPositionedTextIntercepts (byte[] text, SKPoint[] positions, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetPositionedTextIntercepts (System.ReadOnlySpan<byte> text, System.ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetPositionedTextIntercepts (System.ReadOnlySpan<char> text, System.ReadOnlySpan<SKPoint> positions, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetPositionedTextIntercepts (string text, SKPoint[] positions, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetPositionedTextIntercepts (IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetPositionedTextIntercepts (IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetTextIntercepts (SKTextBlob text, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetTextIntercepts (byte[] text, float x, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetTextIntercepts (System.ReadOnlySpan<byte> text, float x, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetTextIntercepts (System.ReadOnlySpan<char> text, float x, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetTextIntercepts (string text, float x, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetTextIntercepts (IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKTextBlob.GetIntercepts() instead.")]
 public float[] GetTextIntercepts (IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (byte[] text, SKPoint[] points);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (System.ReadOnlySpan<byte> text, System.ReadOnlySpan<SKPoint> points);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (System.ReadOnlySpan<char> text, System.ReadOnlySpan<SKPoint> points);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (string text, SKPoint[] points);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (byte[] text, float x, float y);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (IntPtr buffer, int length, SKPoint[] points);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (IntPtr buffer, int length, System.ReadOnlySpan<SKPoint> points);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (IntPtr buffer, IntPtr length, SKPoint[] points);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (System.ReadOnlySpan<byte> text, float x, float y);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (System.ReadOnlySpan<char> text, float x, float y);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (string text, float x, float y);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (IntPtr buffer, int length, float x, float y);
 [Obsolete ("Use SKFont.GetTextPath() instead.")]
 public SKPath GetTextPath (IntPtr buffer, IntPtr length, float x, float y);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (byte[] text);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (System.ReadOnlySpan<byte> text);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (System.ReadOnlySpan<char> text);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (string text);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (byte[] text, ref SKRect bounds);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (IntPtr buffer, int length);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (IntPtr buffer, IntPtr length);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (System.ReadOnlySpan<byte> text, ref SKRect bounds);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (System.ReadOnlySpan<char> text, ref SKRect bounds);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (string text, ref SKRect bounds);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (IntPtr buffer, int length, ref SKRect bounds);
 [Obsolete ("Use SKFont.MeasureText() instead.")]
 public float MeasureText (IntPtr buffer, IntPtr length, ref SKRect bounds);
 [Obsolete ("Use SKFont instead.")]
 public SKFont ToFont ();
```

Added methods:

```csharp
public SKPath GetFillPath (SKPath src, SKMatrix matrix);
public bool GetFillPath (SKPath src, SKPath dst, SKMatrix matrix);
public SKPath GetFillPath (SKPath src, SKRect cullRect, SKMatrix matrix);
public bool GetFillPath (SKPath src, SKPath dst, SKRect cullRect, SKMatrix matrix);
```


#### Type Changed: SkiaSharp.SKPath

Modified properties:

```diff
 public SKPathConvexity Convexity { get; ---set;--- }
```

Removed methods:

```csharp
public void AddPath (SKPath other, ref SKMatrix matrix, SKPathAddMode mode);

[Obsolete ("Use AddRoundRect instead.")]
public void AddRoundedRect (SKRect rect, float rx, float ry, SKPathDirection dir);
```

Obsoleted methods:

```diff
 [Obsolete ("Use Transform(in SKMatrix) instead.")]
 public void Transform (SKMatrix matrix);
 [Obsolete ("Use Transform(in SKMatrix matrix, SKPath destination) instead.")]
 public void Transform (SKMatrix matrix, SKPath destination);
```

Added methods:

```csharp
public void AddPath (SKPath other, ref SKMatrix matrix, SKPathAddMode mode);
public void AddPoly (System.ReadOnlySpan<SKPoint> points, bool close);
public void Transform (ref SKMatrix matrix);
public void Transform (ref SKMatrix matrix, SKPath destination);
```

#### Type Changed: SkiaSharp.SKPath.Iterator

Removed method:

```csharp
[Obsolete ("Use Next(SKPoint[]) instead.")]
public SKPathVerb Next (SKPoint[] points, bool doConsumeDegenerates, bool exact);
```



#### Type Changed: SkiaSharp.SKPicture

Added properties:

```csharp
public int ApproximateBytesUsed { get; }
public int ApproximateOperationCount { get; }
```

Added methods:

```csharp
public int GetApproximateOperationCount (bool includeNested);
public void Playback (SKCanvas canvas);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKRect tile);
public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKMatrix localMatrix, SKRect tile);
```


#### Type Changed: SkiaSharp.SKPictureRecorder

Added method:

```csharp
public SKCanvas BeginRecording (SKRect cullRect, bool useRTree);
```


#### Type Changed: SkiaSharp.SKPixmap

Removed constructor:

```csharp
[Obsolete ("The Index8 color type and color table is no longer supported. Use SKPixmap(SKImageInfo, IntPtr, int) instead.")]
public SKPixmap (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable);
```

Removed property:

```csharp
[Obsolete ("The Index8 color type and color table is no longer supported.")]
public SKColorTable ColorTable { get; }
```

Added properties:

```csharp
public int BitShiftPerPixel { get; }
public long BytesSize64 { get; }
```

Removed methods:

```csharp
[Obsolete ("Use Encode(SKWStream, SKJpegEncoderOptions) instead.")]
public static bool Encode (SKWStream dst, SKPixmap src, SKJpegEncoderOptions options);

[Obsolete ("Use Encode(SKWStream, SKPngEncoderOptions) instead.")]
public static bool Encode (SKWStream dst, SKPixmap src, SKPngEncoderOptions options);

[Obsolete ("Use Encode(SKWStream, SKWebpEncoderOptions) instead.")]
public static bool Encode (SKWStream dst, SKPixmap src, SKWebpEncoderOptions options);

[Obsolete ("Use Encode(SKWStream, SKEncodedImageFormat, int) instead.")]
public static bool Encode (SKWStream dst, SKBitmap src, SKEncodedImageFormat format, int quality);

[Obsolete ("Use Encode(SKWStream, SKEncodedImageFormat, int) instead.")]
public static bool Encode (SKWStream dst, SKPixmap src, SKEncodedImageFormat encoder, int quality);
public bool Erase (SKColorF color, SKColorSpace colorspace, SKRectI subset);
public System.ReadOnlySpan<byte> GetPixelSpan ();

[Obsolete ("Use ReadPixels(SKImageInfo, IntPtr, int, int, int) instead.")]
public bool ReadPixels (SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKTransferFunctionBehavior behavior);

[Obsolete ("The Index8 color type and color table is no longer supported. Use Reset(SKImageInfo, IntPtr, int) instead.")]
public void Reset (SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable);

[Obsolete ("Use ScalePixels(SKPixmap, SKFilterQuality) instead.")]
public static bool Resize (SKPixmap dst, SKPixmap src, SKBitmapResizeMethod method);
```

Obsoleted methods:

```diff
 [Obsolete ("Use ScalePixels(SKPixmap destination, SKSamplingOptions sampling) instead.")]
 public bool ScalePixels (SKPixmap destination, SKFilterQuality quality);
```

Added methods:

```csharp
public bool ComputeIsOpaque ();
public float GetPixelAlpha (int x, int y);
public SKColorF GetPixelColorF (int x, int y);
public System.Span<byte> GetPixelSpan ();
public System.Span<T> GetPixelSpan<T> (int x, int y);
public System.Span<byte> GetPixelSpan (int x, int y);
public bool ScalePixels (SKPixmap destination);
public bool ScalePixels (SKPixmap destination, SKSamplingOptions sampling);
```


#### Type Changed: SkiaSharp.SKPngEncoderOptions

Removed constructor:

```csharp
[Obsolete ("Using SKPngEncoderOptions(SKPngEncoderFilterFlags, int) instead.")]
public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel, SKTransferFunctionBehavior unpremulBehavior);
```

Removed property:

```csharp
[Obsolete]
public SKTransferFunctionBehavior UnpremulBehavior { get; set; }
```

Modified properties:

```diff
 public SKPngEncoderFilterFlags FilterFlags { get; ---set;--- }
 public int ZLibLevel { get; ---set;--- }
```


#### Type Changed: SkiaSharp.SKPoint

Added methods:

```csharp
public static System.Numerics.Vector2 op_Implicit (SKPoint point);
public static SKPoint op_Implicit (System.Numerics.Vector2 vector);
```


#### Type Changed: SkiaSharp.SKPoint3

Added methods:

```csharp
public static System.Numerics.Vector3 op_Implicit (SKPoint3 point);
public static SKPoint3 op_Implicit (System.Numerics.Vector3 vector);
```


#### Type Changed: SkiaSharp.SKPointI

Added method:

```csharp
public static System.Numerics.Vector2 op_Implicit (SKPointI point);
```


#### Type Changed: SkiaSharp.SKPositionedRunBuffer

Added property:

```csharp
public System.Span<SKPoint> Positions { get; }
```

Obsoleted methods:

```diff
 [Obsolete ("Use Positions instead.")]
 public System.Span<SKPoint> GetPositionSpan ();
```


#### Type Changed: SkiaSharp.SKRegion

Removed method:

```csharp
public bool SetRects (SKRectI[] rects);
```

Added method:

```csharp
public bool SetRects (System.ReadOnlySpan<SKRectI> rects);
```


#### Type Changed: SkiaSharp.SKRotationScaleRunBuffer

Added property:

```csharp
public System.Span<SKRotationScaleMatrix> Positions { get; }
```

Obsoleted methods:

```diff
 [Obsolete ("Use Positions instead.")]
 public System.Span<SKRotationScaleMatrix> GetRotationScaleSpan ();
 [Obsolete ("Use SetPositions instead.")]
 public void SetRotationScale (System.ReadOnlySpan<SKRotationScaleMatrix> positions);
```

Added method:

```csharp
public void SetPositions (System.ReadOnlySpan<SKRotationScaleMatrix> positions);
```


#### Type Changed: SkiaSharp.SKRoundRect

Added method:

```csharp
public void SetRectRadii (SKRect rect, System.ReadOnlySpan<SKPoint> radii);
```


#### Type Changed: SkiaSharp.SKRunBuffer

Removed property:

```csharp
[Obsolete]
public int TextSize { get; }
```

Added property:

```csharp
public System.Span<ushort> Glyphs { get; }
```

Removed methods:

```csharp
[Obsolete]
public System.Span<uint> GetClusterSpan ();

[Obsolete]
public System.Span<byte> GetTextSpan ();

[Obsolete]
public void SetClusters (System.ReadOnlySpan<uint> clusters);

[Obsolete]
public void SetText (System.ReadOnlySpan<byte> text);
```

Obsoleted methods:

```diff
 [Obsolete ("Use Glyphs instead.")]
 public System.Span<ushort> GetGlyphSpan ();
```


#### Type Changed: SkiaSharp.SKRuntimeEffect

Removed methods:

```csharp
public static SKRuntimeEffect Create (string sksl, out string errors);
public SKShader ToShader (bool isOpaque);
public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms);
public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children);
public SKShader ToShader (bool isOpaque, SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children, SKMatrix localMatrix);
```

Added methods:

```csharp
public static SKRuntimeBlenderBuilder BuildBlender (string sksl);
public static SKRuntimeColorFilterBuilder BuildColorFilter (string sksl);
public static SKRuntimeShaderBuilder BuildShader (string sksl);
public static SKRuntimeEffect CreateBlender (string sksl, out string errors);
public static SKRuntimeEffect CreateColorFilter (string sksl, out string errors);
public static SKRuntimeEffect CreateShader (string sksl, out string errors);
public SKBlender ToBlender ();
public SKBlender ToBlender (SKRuntimeEffectUniforms uniforms);
public SKBlender ToBlender (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children);
public SKShader ToShader ();
public SKShader ToShader (SKRuntimeEffectUniforms uniforms);
public SKShader ToShader (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children);
public SKShader ToShader (SKRuntimeEffectUniforms uniforms, SKRuntimeEffectChildren children, SKMatrix localMatrix);
```


#### Type Changed: SkiaSharp.SKRuntimeEffectChildren

Added interface:

```csharp
System.IDisposable
```

Removed property:

```csharp
public SKShader Item { set; }
```

Added property:

```csharp
public SKRuntimeEffectChild? Item { set; }
```

Removed methods:

```csharp
public void Add (string name, SKShader value);
public SKShader[] ToArray ();
```

Added methods:

```csharp
public void Add (string name, SKRuntimeEffectChild? value);
public virtual void Dispose ();
public SKObject[] ToArray ();
```


#### Type Changed: SkiaSharp.SKRuntimeEffectUniform

Added methods:

```csharp
public static SKRuntimeEffectUniform op_Implicit (SKColor value);
public static SKRuntimeEffectUniform op_Implicit (SKColorF value);
public static SKRuntimeEffectUniform op_Implicit (SKPoint value);
public static SKRuntimeEffectUniform op_Implicit (SKPoint3 value);
public static SKRuntimeEffectUniform op_Implicit (SKPointI value);
public static SKRuntimeEffectUniform op_Implicit (SKSize value);
public static SKRuntimeEffectUniform op_Implicit (SKSizeI value);
public static SKRuntimeEffectUniform op_Implicit (int value);
public static SKRuntimeEffectUniform op_Implicit (int[] value);
public static SKRuntimeEffectUniform op_Implicit (System.ReadOnlySpan<int> value);
public static SKRuntimeEffectUniform op_Implicit (System.Span<int> value);
```


#### Type Changed: SkiaSharp.SKRuntimeEffectUniforms

Added interface:

```csharp
System.IDisposable
```

Added property:

```csharp
public int Size { get; }
```

Added method:

```csharp
public virtual void Dispose ();
```


#### Type Changed: SkiaSharp.SKShader

Removed methods:

```csharp
public static SKShader CreateLerp (float weight, SKShader dst, SKShader src);
public static SKShader CreatePerlinNoiseImprovedNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float z);
```

Added methods:

```csharp
public static SKShader CreateBlend (SKBlendMode mode, SKShader shaderA, SKShader shaderB);
public static SKShader CreateBlend (SKBlender blender, SKShader shaderA, SKShader shaderB);

[Obsolete ("Use CreateImage(SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) instead.")]
public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterQuality quality);
public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling);

[Obsolete ("Use CreateImage(SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling, SKMatrix localMatrix) instead.")]
public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterQuality quality, SKMatrix localMatrix);
public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling, SKMatrix localMatrix);
public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode);
public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKRect tile);
public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKMatrix localMatrix, SKRect tile);
```


#### Type Changed: SkiaSharp.SKSurface

Removed property:

```csharp
[Obsolete ("Use SurfaceProperties instead.")]
public SKSurfaceProps SurfaceProps { get; }
```

Removed methods:

```csharp
[Obsolete ("Use Create(GRContext, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType) instead.")]
public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
public static SKSurface Create (GRContext context, GRBackendTextureDesc desc);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc);

[Obsolete ("Use Create(SKImageInfo, SKSurfaceProperties) instead.")]
public static SKSurface Create (SKImageInfo info, SKSurfaceProps props);

[Obsolete ("Use Create(SKPixmap, SKSurfaceProperties) instead.")]
public static SKSurface Create (SKPixmap pixmap, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendRenderTarget, GRSurfaceOrigin, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface Create (GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface Create (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface Create (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete ("Use Create(SKImageInfo, IntPtr, rowBytes, SKSurfaceProperties) instead.")]
public static SKSurface Create (SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props);

[Obsolete ("Use Create(SKImageInfo) instead.")]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType);

[Obsolete ("Use Create(GRContext, bool, SKImageInfo, int, SKSurfaceProperties) instead.")]
public static SKSurface Create (GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProps props);

[Obsolete ("Use Create(SKImageInfo, SKSurfaceProperties) instead.")]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props);

[Obsolete ("Use Create(SKImageInfo, IntPtr, int) instead.")]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes);

[Obsolete ("Use Create(SKImageInfo, IntPtr, int, SKSurfaceProperties) instead.")]
public static SKSurface Create (int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc);

[Obsolete ("Use Create(GRContext, GRBackendTexture, SKColorType) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);

[Obsolete ("Use Create(GRContext, GRBackendTexture, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKColorSpace) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props);

[Obsolete ("Use Create(GRContext, GRBackendTexture, GRSurfaceOrigin, int, SKColorType, SKColorSpace, SKSurfaceProperties) instead.")]
public static SKSurface CreateAsRenderTarget (GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
```


#### Type Changed: SkiaSharp.SKSurfaceProperties

Removed constructor:

```csharp
[Obsolete]
public SKSurfaceProperties (SKSurfaceProps props);
```


#### Type Changed: SkiaSharp.SKSvgCanvas

Removed method:

```csharp
[Obsolete ("Use Create(SKRect, Stream) instead.")]
public static SKCanvas Create (SKRect bounds, SKXmlWriter writer);
```


#### Type Changed: SkiaSharp.SKTextBlobBuilder

Removed methods:

```csharp
[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, SKRect? bounds);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, SKRect bounds);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<float> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, SKRect? bounds);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, SKRect bounds);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<SKPoint> positions, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, SKRect? bounds);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, SKRect bounds);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, System.ReadOnlySpan<ushort> glyphs, System.ReadOnlySpan<byte> text, System.ReadOnlySpan<uint> clusters, SKRect? bounds);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters, SKRect bounds);

[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y);

[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount);

[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, SKRect? bounds);

[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount, SKRect? bounds);

[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count);

[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount);

[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, SKRect? bounds);

[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount, SKRect? bounds);
public SKRotationScaleRunBuffer AllocateRotationScaleRun (SKFont font, int count);

[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y);

[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount);

[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, SKRect? bounds);

[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount, SKRect? bounds);
```

Added methods:

```csharp
public SKHorizontalTextRunBuffer AllocateHorizontalTextRun (SKFont font, int count, float y, int textByteCount, SKRect? bounds);
public SKPositionedTextRunBuffer AllocatePositionedTextRun (SKFont font, int count, int textByteCount, SKRect? bounds);
public SkiaSharp.SKRawRunBuffer<float> AllocateRawHorizontalRun (SKFont font, int count, float y, SKRect? bounds);
public SkiaSharp.SKRawRunBuffer<float> AllocateRawHorizontalTextRun (SKFont font, int count, float y, int textByteCount, SKRect? bounds);
public SkiaSharp.SKRawRunBuffer<SKPoint> AllocateRawPositionedRun (SKFont font, int count, SKRect? bounds);
public SkiaSharp.SKRawRunBuffer<SKPoint> AllocateRawPositionedTextRun (SKFont font, int count, int textByteCount, SKRect? bounds);
public SkiaSharp.SKRawRunBuffer<SKRotationScaleMatrix> AllocateRawRotationScaleRun (SKFont font, int count, SKRect? bounds);
public SkiaSharp.SKRawRunBuffer<SKRotationScaleMatrix> AllocateRawRotationScaleTextRun (SKFont font, int count, int textByteCount, SKRect? bounds);
public SkiaSharp.SKRawRunBuffer<float> AllocateRawRun (SKFont font, int count, float x, float y, SKRect? bounds);
public SkiaSharp.SKRawRunBuffer<float> AllocateRawTextRun (SKFont font, int count, float x, float y, int textByteCount, SKRect? bounds);
public SKRotationScaleRunBuffer AllocateRotationScaleRun (SKFont font, int count, SKRect? bounds);
public SKRotationScaleTextRunBuffer AllocateRotationScaleTextRun (SKFont font, int count, int textByteCount, SKRect? bounds);
public SKTextRunBuffer AllocateTextRun (SKFont font, int count, float x, float y, int textByteCount, SKRect? bounds);
```


#### Type Changed: SkiaSharp.SKTraceMemoryDump

Modified methods:

```diff
-protected virtual void OnDumpNumericValue (string dumpName, string valueName, string units, ulong value)
+protected virtual void OnDumpNumericValue (string dumpName, string valueName, string units, ulong value)
-protected virtual void OnDumpStringValue (string dumpName, string valueName, string value)
+protected virtual void OnDumpStringValue (string dumpName, string valueName, string value)
```


#### Type Changed: SkiaSharp.SKTypeface

Removed property:

```csharp
[Obsolete ("Use FontWeight and FontSlant instead.")]
public SKTypefaceStyle Style { get; }
```

Removed methods:

```csharp
[Obsolete ("Use GetGlyphs(string, out ushort[]) instead.")]
public int CharsToGlyphs (string chars, out ushort[] glyphs);

[Obsolete ("Use GetGlyphs(IntPtr, int, SKTextEncoding, out ushort[]) instead.")]
public int CharsToGlyphs (IntPtr str, int strlen, SKEncoding encoding, out ushort[] glyphs);

[Obsolete ("Use CountGlyphs(byte[], SKTextEncoding) instead.")]
public int CountGlyphs (byte[] str, SKEncoding encoding);

[Obsolete ("Use CountGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
public int CountGlyphs (System.ReadOnlySpan<byte> str, SKEncoding encoding);

[Obsolete ("Use CountGlyphs(string) instead.")]
public int CountGlyphs (string str, SKEncoding encoding);

[Obsolete ("Use CountGlyphs(IntPtr, int, SKTextEncoding) instead.")]
public int CountGlyphs (IntPtr str, int strLen, SKEncoding encoding);

[Obsolete ("Use FromFamilyName(string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant) instead.")]
public static SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style);

[Obsolete]
public static SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style);

[Obsolete ("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
public ushort[] GetGlyphs (byte[] text, SKEncoding encoding);

[Obsolete ("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
public ushort[] GetGlyphs (System.ReadOnlySpan<byte> text, SKEncoding encoding);

[Obsolete ("Use GetGlyphs(string) instead.")]
public ushort[] GetGlyphs (string text, SKEncoding encoding);

[Obsolete ("Use GetGlyphs(string) instead.")]
public int GetGlyphs (string text, out ushort[] glyphs);

[Obsolete ("Use GetGlyphs(byte[], SKTextEncoding) instead.")]
public int GetGlyphs (byte[] text, SKEncoding encoding, out ushort[] glyphs);

[Obsolete ("Use GetGlyphs(IntPtr, int, SKTextEncoding) instead.")]
public ushort[] GetGlyphs (IntPtr text, int length, SKEncoding encoding);

[Obsolete ("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
public int GetGlyphs (System.ReadOnlySpan<byte> text, SKEncoding encoding, out ushort[] glyphs);

[Obsolete ("Use GetGlyphs(string) instead.")]
public int GetGlyphs (string text, SKEncoding encoding, out ushort[] glyphs);

[Obsolete ("Use GetGlyphs(IntPtr, int, SKTextEncoding) instead.")]
public int GetGlyphs (IntPtr text, int length, SKEncoding encoding, out ushort[] glyphs);
```


#### Type Changed: SkiaSharp.SKWebpEncoderOptions

Removed constructor:

```csharp
[Obsolete ("Use SKWebpEncoderOptions(SKWebpEncoderCompression, float) instead.")]
public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality, SKTransferFunctionBehavior unpremulBehavior);
```

Removed property:

```csharp
[Obsolete]
public SKTransferFunctionBehavior UnpremulBehavior { get; set; }
```

Modified properties:

```diff
 public SKWebpEncoderCompression Compression { get; ---set;--- }
 public float Quality { get; ---set;--- }
```


#### Type Changed: SkiaSharp.SkiaExtensions

Removed methods:

```csharp
[Obsolete ("Use SKColorChannel instead.")]
public static SKColorChannel ToColorChannel (this SKDisplacementMapEffectChannelSelectorType channelSelectorType);

[Obsolete]
public static SKColorSpaceTransferFn ToColorSpaceTransferFn (this SKColorSpaceRenderTargetGamma gamma);

[Obsolete]
public static SKColorSpaceTransferFn ToColorSpaceTransferFn (this SKNamedGamma gamma);

[Obsolete]
public static SKColorSpaceXyz ToColorSpaceXyz (this SKColorSpaceGamut gamut);

[Obsolete]
public static SKColorSpaceXyz ToColorSpaceXyz (this SKMatrix44 matrix);

[Obsolete ("Use SKColorType instead.")]
public static SKColorType ToColorType (this GRPixelConfig config);

[Obsolete]
public static SKFilterQuality ToFilterQuality (this SKBitmapResizeMethod method);

[Obsolete ("Use SKColorType instead.")]
public static uint ToGlSizedFormat (this GRPixelConfig config);

[Obsolete ("Use SKColorType instead.")]
public static GRPixelConfig ToPixelConfig (this SKColorType colorType);

[Obsolete ("Use SKShaderTileMode instead.")]
public static SKShaderTileMode ToShaderTileMode (this SKMatrixConvolutionTileMode tileMode);

[Obsolete]
public static SKTextEncoding ToTextEncoding (this SKEncoding encoding);
```

Added methods:

```csharp
public static int GetBitShiftPerPixel (this SKColorType colorType);

[Obsolete ("Use SKSamplingOptions instead.")]
public static SKSamplingOptions ToSamplingOptions (this SKFilterQuality quality);
```


#### Type Changed: SkiaSharp.StringUtilities

Removed method:

```csharp
[Obsolete ("Use GetEncodedText(string, SKTextEncoding) instead.")]
public static byte[] GetEncodedText (string text, SKEncoding encoding);
```


#### Removed Type SkiaSharp.SK3dView
#### Removed Type SkiaSharp.SKAutoMaskFreeImage
#### Removed Type SkiaSharp.SKCropRectFlags
#### Removed Type SkiaSharp.SKMask
#### Removed Type SkiaSharp.SKMaskFormat
#### Removed Type SkiaSharp.SKMatrix44TypeMask
#### Removed Type SkiaSharp.SKXmlStreamWriter
#### Removed Type SkiaSharp.SKXmlWriter
#### New Type: SkiaSharp.GRMtlBackendContext

```csharp
public class GRMtlBackendContext : System.IDisposable {
	// constructors
	public GRMtlBackendContext ();
	// properties
	public IntPtr DeviceHandle { get; set; }
	public IntPtr QueueHandle { get; set; }
	// methods
	public virtual void Dispose ();
	protected virtual void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.GRMtlTextureInfo

```csharp
public struct GRMtlTextureInfo {
	// constructors
	public GRMtlTextureInfo (IntPtr textureHandle);
	// properties
	public IntPtr TextureHandle { get; set; }
	// methods
	public bool Equals (GRMtlTextureInfo obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (GRMtlTextureInfo left, GRMtlTextureInfo right);
	public static bool op_Inequality (GRMtlTextureInfo left, GRMtlTextureInfo right);
}
```

#### New Type: SkiaSharp.SKBlender

```csharp
public class SKBlender : SkiaSharp.SKObject, System.IDisposable {
	// methods
	public static SKBlender CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor);
	public static SKBlender CreateBlendMode (SKBlendMode mode);
	protected override void Dispose (bool disposing);
}
```

#### New Type: SkiaSharp.SKCodecAnimationBlend

```csharp
[Serializable]
public enum SKCodecAnimationBlend {
	Src = 1,
	SrcOver = 0,
}
```

#### New Type: SkiaSharp.SKCubicResampler

```csharp
public struct SKCubicResampler, System.IEquatable<SKCubicResampler> {
	// constructors
	public SKCubicResampler (float b, float c);
	// fields
	public static SKCubicResampler CatmullRom;
	public static SKCubicResampler Mitchell;
	// properties
	public float B { get; }
	public float C { get; }
	// methods
	public virtual bool Equals (SKCubicResampler obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKCubicResampler left, SKCubicResampler right);
	public static bool op_Inequality (SKCubicResampler left, SKCubicResampler right);
}
```

#### New Type: SkiaSharp.SKFilterMode

```csharp
[Serializable]
public enum SKFilterMode {
	Linear = 1,
	Nearest = 0,
}
```

#### New Type: SkiaSharp.SKHorizontalTextRunBuffer

```csharp
public sealed class SKHorizontalTextRunBuffer : SkiaSharp.SKTextRunBuffer {
	// properties
	public System.Span<float> Positions { get; }
	// methods
	public void SetPositions (System.ReadOnlySpan<float> positions);
}
```

#### New Type: SkiaSharp.SKMipmapMode

```csharp
[Serializable]
public enum SKMipmapMode {
	Linear = 2,
	Nearest = 1,
	None = 0,
}
```

#### New Type: SkiaSharp.SKPositionedTextRunBuffer

```csharp
public sealed class SKPositionedTextRunBuffer : SkiaSharp.SKTextRunBuffer {
	// properties
	public System.Span<SKPoint> Positions { get; }
	// methods
	public void SetPositions (System.ReadOnlySpan<SKPoint> positions);
}
```

#### New Type: SkiaSharp.SKRawRunBuffer`1

```csharp
public struct SKRawRunBuffer`1 {
	// properties
	public System.Span<uint> Clusters { get; }
	public System.Span<ushort> Glyphs { get; }
	public System.Span<T> Positions { get; }
	public System.Span<byte> Text { get; }
}
```

#### New Type: SkiaSharp.SKRotationScaleTextRunBuffer

```csharp
public sealed class SKRotationScaleTextRunBuffer : SkiaSharp.SKTextRunBuffer {
	// properties
	public System.Span<SKRotationScaleMatrix> Positions { get; }
	// methods
	public void SetPositions (System.ReadOnlySpan<SKRotationScaleMatrix> positions);
}
```

#### New Type: SkiaSharp.SKRuntimeBlenderBuilder

```csharp
public class SKRuntimeBlenderBuilder : SkiaSharp.SKRuntimeEffectBuilder, System.IDisposable {
	// constructors
	public SKRuntimeBlenderBuilder (SKRuntimeEffect effect);
	// methods
	public SKBlender Build ();
}
```

#### New Type: SkiaSharp.SKRuntimeColorFilterBuilder

```csharp
public class SKRuntimeColorFilterBuilder : SkiaSharp.SKRuntimeEffectBuilder, System.IDisposable {
	// constructors
	public SKRuntimeColorFilterBuilder (SKRuntimeEffect effect);
	// methods
	public SKColorFilter Build ();
}
```

#### New Type: SkiaSharp.SKRuntimeEffectBuilder

```csharp
public class SKRuntimeEffectBuilder : System.IDisposable {
	// constructors
	public SKRuntimeEffectBuilder (SKRuntimeEffect effect);
	// properties
	public SKRuntimeEffectChildren Children { get; }
	public SKRuntimeEffect Effect { get; }
	public SKRuntimeEffectUniforms Uniforms { get; }
	// methods
	public virtual void Dispose ();
}
```

#### New Type: SkiaSharp.SKRuntimeEffectBuilderException

```csharp
public class SKRuntimeEffectBuilderException : System.ApplicationException, System.Runtime.Serialization.ISerializable {
	// constructors
	public SKRuntimeEffectBuilderException (string message);
}
```

#### New Type: SkiaSharp.SKRuntimeEffectChild

```csharp
public struct SKRuntimeEffectChild {
	// constructors
	public SKRuntimeEffectChild (SKBlender blender);
	public SKRuntimeEffectChild (SKColorFilter colorFilter);
	public SKRuntimeEffectChild (SKShader shader);
	// properties
	public SKBlender Blender { get; }
	public SKColorFilter ColorFilter { get; }
	public SKShader Shader { get; }
	public SKObject Value { get; }
	// methods
	public static SKRuntimeEffectChild op_Implicit (SKBlender blender);
	public static SKRuntimeEffectChild op_Implicit (SKColorFilter colorFilter);
	public static SKRuntimeEffectChild op_Implicit (SKShader shader);
}
```

#### New Type: SkiaSharp.SKRuntimeShaderBuilder

```csharp
public class SKRuntimeShaderBuilder : SkiaSharp.SKRuntimeEffectBuilder, System.IDisposable {
	// constructors
	public SKRuntimeShaderBuilder (SKRuntimeEffect effect);
	// methods
	public SKShader Build ();
	public SKShader Build (SKMatrix localMatrix);
}
```

#### New Type: SkiaSharp.SKSamplingOptions

```csharp
public struct SKSamplingOptions, System.IEquatable<SKSamplingOptions> {
	// constructors
	public SKSamplingOptions (SKCubicResampler resampler);
	public SKSamplingOptions (SKFilterMode filter);
	public SKSamplingOptions (int maxAniso);
	public SKSamplingOptions (SKFilterMode filter, SKMipmapMode mipmap);
	// fields
	public static SKSamplingOptions Default;
	// properties
	public SKCubicResampler Cubic { get; }
	public SKFilterMode Filter { get; }
	public bool IsAniso { get; }
	public int MaxAniso { get; }
	public SKMipmapMode Mipmap { get; }
	public bool UseCubic { get; }
	// methods
	public virtual bool Equals (SKSamplingOptions obj);
	public override bool Equals (object obj);
	public override int GetHashCode ();
	public static bool op_Equality (SKSamplingOptions left, SKSamplingOptions right);
	public static bool op_Inequality (SKSamplingOptions left, SKSamplingOptions right);
}
```

#### New Type: SkiaSharp.SKTextRunBuffer

```csharp
public class SKTextRunBuffer : SkiaSharp.SKRunBuffer {
	// properties
	public System.Span<uint> Clusters { get; }
	public System.Span<byte> Text { get; }
	public int TextSize { get; }
	// methods
	public void SetClusters (System.ReadOnlySpan<uint> clusters);
	public void SetText (System.ReadOnlySpan<byte> text);
}
```


