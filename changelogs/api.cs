namespace SkiaSharp
{
    public enum GRBackend
    {
        Metal = 0,
        OpenGL = 1,
        Vulkan = 2,
    }
    public class GRBackendRenderTarget : SKObject
    {
        public GRBackendRenderTarget(GRBackend backend, GRBackendRenderTargetDesc desc);
        public GRBackendRenderTarget(int width, int height, int sampleCount, int stencilBits, GRGlFramebufferInfo glInfo);
        public GRBackend Backend { get; }
        public int Height { get; }
        public bool IsValid { get; }
        public SKRectI Rect { get; }
        public int SampleCount { get; }
        public SKSizeI Size { get; }
        public int StencilBits { get; }
        public int Width { get; }
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public GRGlFramebufferInfo GetGlFramebufferInfo();
        public bool GetGlFramebufferInfo(out GRGlFramebufferInfo glInfo);
    }
    public struct GRBackendRenderTargetDesc
    {
        public GRPixelConfig Config { get; set; }
        public int Height { get; set; }
        public GRSurfaceOrigin Origin { get; set; }
        public SKRectI Rect { get; }
        public IntPtr RenderTargetHandle { get; set; }
        public int SampleCount { get; set; }
        public SKSizeI Size { get; }
        public int StencilBits { get; set; }
        public int Width { get; set; }
    }
    public enum GRBackendState : uint
    {
        All = (uint)4294967295,
        None = (uint)0,
    }
    public class GRBackendTexture : SKObject
    {
        public GRBackendTexture(GRBackendTextureDesc desc);
        public GRBackendTexture(GRGlBackendTextureDesc desc);
        public GRBackendTexture(int width, int height, bool mipmapped, GRGlTextureInfo glInfo);
        public GRBackend Backend { get; }
        public bool HasMipMaps { get; }
        public int Height { get; }
        public bool IsValid { get; }
        public SKRectI Rect { get; }
        public SKSizeI Size { get; }
        public int Width { get; }
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public GRGlTextureInfo GetGlTextureInfo();
        public bool GetGlTextureInfo(out GRGlTextureInfo glInfo);
    }
    public struct GRBackendTextureDesc
    {
        public GRPixelConfig Config { get; set; }
        public GRBackendTextureDescFlags Flags { get; set; }
        public int Height { get; set; }
        public GRSurfaceOrigin Origin { get; set; }
        public int SampleCount { get; set; }
        public IntPtr TextureHandle { get; set; }
        public int Width { get; set; }
    }
    public enum GRBackendTextureDescFlags
    {
        None = 0,
        RenderTarget = 1,
    }
    public class GRContext : SKObject
    {
        public GRBackend Backend { get; }
        public void AbandonContext(bool releaseResources = false);
        public static GRContext Create(GRBackend backend);
        public static GRContext Create(GRBackend backend, GRGlInterface backendContext);
        public static GRContext Create(GRBackend backend, IntPtr backendContext);
        public static GRContext CreateGl();
        public static GRContext CreateGl(GRGlInterface backendContext);
        protected override void Dispose(bool disposing);
        public void Flush();
        public int GetMaxSurfaceSampleCount(SKColorType colorType);
        public int GetRecommendedSampleCount(GRPixelConfig config, float dpi);
        public void GetResourceCacheLimits(out int maxResources, out long maxResourceBytes);
        public void GetResourceCacheUsage(out int maxResources, out long maxResourceBytes);
        public void ResetContext(GRBackendState state = GRBackendState.All);
        public void ResetContext(GRGlBackendState state);
        public void ResetContext(uint state);
        public void SetResourceCacheLimits(int maxResources, long maxResourceBytes);
    }
    public enum GRGlBackendState : uint
    {
        All = (uint)65535,
        Blend = (uint)8,
        FixedFunction = (uint)512,
        Misc = (uint)1024,
        MSAAEnable = (uint)16,
        None = (uint)0,
        PathRendering = (uint)2048,
        PixelStore = (uint)128,
        Program = (uint)256,
        RenderTarget = (uint)1,
        Stencil = (uint)64,
        TextureBinding = (uint)2,
        Vertex = (uint)32,
        View = (uint)4,
    }
    public struct GRGlBackendTextureDesc
    {
        public GRPixelConfig Config { get; set; }
        public GRBackendTextureDescFlags Flags { get; set; }
        public int Height { get; set; }
        public GRSurfaceOrigin Origin { get; set; }
        public int SampleCount { get; set; }
        public GRGlTextureInfo TextureHandle { get; set; }
        public int Width { get; set; }
    }
    public struct GRGlFramebufferInfo
    {
        public GRGlFramebufferInfo(uint fboId);
        public GRGlFramebufferInfo(uint fboId, uint format);
        public uint Format { get; set; }
        public uint FramebufferObjectId { get; set; }
    }
    public delegate IntPtr GRGlGetProcDelegate(object context, string name);
    public class GRGlInterface : SKObject
    {
        public static GRGlInterface AssembleAngleInterface(GRGlGetProcDelegate @get);
        public static GRGlInterface AssembleAngleInterface(object context, GRGlGetProcDelegate @get);
        public static GRGlInterface AssembleGlesInterface(GRGlGetProcDelegate @get);
        public static GRGlInterface AssembleGlesInterface(object context, GRGlGetProcDelegate @get);
        public static GRGlInterface AssembleGlInterface(GRGlGetProcDelegate @get);
        public static GRGlInterface AssembleGlInterface(object context, GRGlGetProcDelegate @get);
        public static GRGlInterface AssembleInterface(GRGlGetProcDelegate @get);
        public static GRGlInterface AssembleInterface(object context, GRGlGetProcDelegate @get);
        public static GRGlInterface CreateDefaultInterface();
        public static GRGlInterface CreateNativeAngleInterface();
        public static GRGlInterface CreateNativeEvasInterface(IntPtr evas);
        public static GRGlInterface CreateNativeGlInterface();
        protected override void Dispose(bool disposing);
        public bool HasExtension(string extension);
        public bool Validate();
    }
    public struct GRGlTextureInfo
    {
        public GRGlTextureInfo(uint target, uint id, uint format);
        public uint Format { get; set; }
        public uint Id { get; set; }
        public uint Target { get; set; }
    }
    public enum GRPixelConfig
    {
        Alpha8 = 1,
        AlphaHalf = 13,
        Bgra8888 = 7,
        Gray8 = 2,
        Rgb565 = 3,
        Rgb888 = 6,
        Rgba1010102 = 10,
        Rgba4444 = 4,
        Rgba8888 = 5,
        RgbaFloat = 11,
        RgbaHalf = 14,
        RgFloat = 12,
        Sbgra8888 = 9,
        Srgba8888 = 8,
        Unknown = 0,
    }
    public enum GRSurfaceOrigin
    {
        BottomLeft = 1,
        TopLeft = 0,
    }
    public class SK3dView : SKObject
    {
        public SK3dView();
        public SKMatrix Matrix { get; }
        public void ApplyToCanvas(SKCanvas canvas);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public float DotWithNormal(float dx, float dy, float dz);
        public void GetMatrix(ref SKMatrix matrix);
        public void Restore();
        public void RotateXDegrees(float degrees);
        public void RotateXRadians(float radians);
        public void RotateYDegrees(float degrees);
        public void RotateYRadians(float radians);
        public void RotateZDegrees(float degrees);
        public void RotateZRadians(float radians);
        public void Save();
        public void Translate(float x, float y, float z);
        public void TranslateX(float x);
        public void TranslateY(float y);
        public void TranslateZ(float z);
    }
    public abstract class SKAbstractManagedStream : SKStreamAsset
    {
        protected SKAbstractManagedStream();
        protected SKAbstractManagedStream(bool owns);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        protected abstract IntPtr OnCreateNew();
        protected virtual IntPtr OnDuplicate();
        protected virtual IntPtr OnFork();
        protected abstract IntPtr OnGetLength();
        protected abstract IntPtr OnGetPosition();
        protected abstract bool OnHasLength();
        protected abstract bool OnHasPosition();
        protected abstract bool OnIsAtEnd();
        protected abstract bool OnMove(int offset);
        protected abstract IntPtr OnPeek(IntPtr buffer, IntPtr size);
        protected abstract IntPtr OnRead(IntPtr buffer, IntPtr size);
        protected abstract bool OnRewind();
        protected abstract bool OnSeek(IntPtr position);
    }
    public abstract class SKAbstractManagedWStream : SKWStream
    {
        protected SKAbstractManagedWStream();
        protected SKAbstractManagedWStream(bool owns);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        protected abstract IntPtr OnBytesWritten();
        protected abstract void OnFlush();
        protected abstract bool OnWrite(IntPtr buffer, IntPtr size);
    }
    public enum SKAlphaType
    {
        Opaque = 1,
        Premul = 2,
        Unknown = 0,
        Unpremul = 3,
    }
    public class SKAutoCanvasRestore : IDisposable
    {
        public SKAutoCanvasRestore(SKCanvas canvas);
        public SKAutoCanvasRestore(SKCanvas canvas, bool doSave);
        public void Dispose();
        public void Restore();
    }
    public class SKAutoCoInitialize : IDisposable
    {
        public SKAutoCoInitialize();
        public bool Initialized { get; }
        public void Dispose();
        public void Uninitialize();
    }
    public class SKAutoMaskFreeImage : IDisposable
    {
        public SKAutoMaskFreeImage(IntPtr maskImage);
        public void Dispose();
    }
    public class SKBitmap : SKObject
    {
        public SKBitmap();
        public SKBitmap(SKImageInfo info);
        public SKBitmap(SKImageInfo info, SKBitmapAllocFlags flags);
        public SKBitmap(SKImageInfo info, SKColorTable ctable);
        public SKBitmap(SKImageInfo info, SKColorTable ctable, SKBitmapAllocFlags flags);
        public SKBitmap(SKImageInfo info, int rowBytes);
        public SKBitmap(int width, int height, SKColorType colorType, SKAlphaType alphaType);
        public SKBitmap(int width, int height, bool isOpaque = false);
        public SKAlphaType AlphaType { get; }
        public int ByteCount { get; }
        public byte[] Bytes { get; }
        public int BytesPerPixel { get; }
        public SKColorSpace ColorSpace { get; }
        public SKColorTable ColorTable { get; }
        public SKColorType ColorType { get; }
        public bool DrawsNothing { get; }
        public int Height { get; }
        public SKImageInfo Info { get; }
        public bool IsEmpty { get; }
        public bool IsImmutable { get; }
        public bool IsNull { get; }
        public bool IsVolatile { get; set; }
        public SKColor[] Pixels { get; set; }
        public bool ReadyToDraw { get; }
        public int RowBytes { get; }
        public int Width { get; }
        public bool CanCopyTo(SKColorType colorType);
        public SKBitmap Copy();
        public SKBitmap Copy(SKColorType colorType);
        public bool CopyTo(SKBitmap destination);
        public bool CopyTo(SKBitmap destination, SKColorType colorType);
        public static SKBitmap Decode(SKCodec codec);
        public static SKBitmap Decode(SKCodec codec, SKImageInfo bitmapInfo);
        public static SKBitmap Decode(SKData data);
        public static SKBitmap Decode(SKData data, SKImageInfo bitmapInfo);
        public static SKBitmap Decode(SKStream stream);
        public static SKBitmap Decode(SKStream stream, SKImageInfo bitmapInfo);
        public static SKBitmap Decode(byte[] buffer);
        public static SKBitmap Decode(byte[] buffer, SKImageInfo bitmapInfo);
        public static SKBitmap Decode(Stream stream);
        public static SKBitmap Decode(Stream stream, SKImageInfo bitmapInfo);
        public static SKBitmap Decode(string filename);
        public static SKBitmap Decode(string filename, SKImageInfo bitmapInfo);
        public static SKImageInfo DecodeBounds(SKData data);
        public static SKImageInfo DecodeBounds(SKStream stream);
        public static SKImageInfo DecodeBounds(byte[] buffer);
        public static SKImageInfo DecodeBounds(Stream stream);
        public static SKImageInfo DecodeBounds(string filename);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public bool Encode(SKWStream dst, SKEncodedImageFormat format, int quality);
        public void Erase(SKColor color);
        public void Erase(SKColor color, SKRectI rect);
        public bool ExtractAlpha(SKBitmap destination);
        public bool ExtractAlpha(SKBitmap destination, SKPaint paint);
        public bool ExtractAlpha(SKBitmap destination, SKPaint paint, out SKPointI offset);
        public bool ExtractAlpha(SKBitmap destination, out SKPointI offset);
        public bool ExtractSubset(SKBitmap destination, SKRectI subset);
        public static SKBitmap FromImage(SKImage image);
        public IntPtr GetAddr(int x, int y);
        public ushort GetAddr16(int x, int y);
        public uint GetAddr32(int x, int y);
        public byte GetAddr8(int x, int y);
        public SKPMColor GetIndex8Color(int x, int y);
        public SKColor GetPixel(int x, int y);
        public IntPtr GetPixels();
        public IntPtr GetPixels(out IntPtr length);
        public ReadOnlySpan<byte> GetPixelSpan();
        public bool InstallMaskPixels(SKMask mask);
        public bool InstallPixels(SKImageInfo info, IntPtr pixels);
        public bool InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes);
        public bool InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc);
        public bool InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKBitmapReleaseDelegate releaseProc, object context);
        public bool InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);
        public bool InstallPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable, SKBitmapReleaseDelegate releaseProc, object context);
        public bool InstallPixels(SKPixmap pixmap);
        public void NotifyPixelsChanged();
        public SKPixmap PeekPixels();
        public bool PeekPixels(SKPixmap pixmap);
        public void Reset();
        public static bool Resize(SKBitmap dst, SKBitmap src, SKBitmapResizeMethod method);
        public bool Resize(SKBitmap dst, SKBitmapResizeMethod method);
        public SKBitmap Resize(SKImageInfo info, SKBitmapResizeMethod method);
        public SKBitmap Resize(SKImageInfo info, SKFilterQuality quality);
        public bool ScalePixels(SKBitmap destination, SKFilterQuality quality);
        public bool ScalePixels(SKPixmap destination, SKFilterQuality quality);
        public void SetColorTable(SKColorTable ct);
        public void SetImmutable();
        public void SetPixel(int x, int y, SKColor color);
        public void SetPixels(IntPtr pixels);
        public void SetPixels(IntPtr pixels, SKColorTable ct);
        public bool TryAllocPixels(SKImageInfo info);
        public bool TryAllocPixels(SKImageInfo info, SKBitmapAllocFlags flags);
        public bool TryAllocPixels(SKImageInfo info, int rowBytes);
    }
    public enum SKBitmapAllocFlags
    {
        None = 0,
        ZeroPixels = 1,
    }
    public delegate void SKBitmapReleaseDelegate(IntPtr address, object context);
    public enum SKBitmapResizeMethod
    {
        Box = 0,
        Hamming = 3,
        Lanczos3 = 2,
        Mitchell = 4,
        Triangle = 1,
    }
    public enum SKBlendMode
    {
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
    public enum SKBlurMaskFilterFlags
    {
        All = 3,
        HighQuality = 2,
        IgnoreTransform = 1,
        None = 0,
    }
    public enum SKBlurStyle
    {
        Inner = 3,
        Normal = 0,
        Outer = 2,
        Solid = 1,
    }
    public class SKCanvas : SKObject
    {
        public SKCanvas(SKBitmap bitmap);
        public SKRectI DeviceClipBounds { get; }
        public bool IsClipEmpty { get; }
        public bool IsClipRect { get; }
        public SKRect LocalClipBounds { get; }
        public int SaveCount { get; }
        public SKMatrix TotalMatrix { get; }
        public void Clear();
        public void Clear(SKColor color);
        public void ClipPath(SKPath path, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false);
        public void ClipRect(SKRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false);
        public void ClipRegion(SKRegion region, SKClipOperation operation = SKClipOperation.Intersect);
        public void ClipRoundRect(SKRoundRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false);
        public void Concat(ref SKMatrix m);
        public void Discard();
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public void DrawAnnotation(SKRect rect, string key, SKData value);
        public void DrawArc(SKRect oval, float startAngle, float sweepAngle, bool useCenter, SKPaint paint);
        public void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKBlendMode mode, SKPaint paint);
        public void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKBlendMode mode, SKRect cullRect, SKPaint paint);
        public void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKPaint paint);
        public void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKRect cullRect, SKPaint paint);
        public void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKPaint paint);
        public void DrawAtlas(SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKRect cullRect, SKPaint paint);
        public void DrawBitmap(SKBitmap bitmap, SKPoint p, SKPaint paint = null);
        public void DrawBitmap(SKBitmap bitmap, SKRect dest, SKPaint paint = null);
        public void DrawBitmap(SKBitmap bitmap, SKRect source, SKRect dest, SKPaint paint = null);
        public void DrawBitmap(SKBitmap bitmap, float x, float y, SKPaint paint = null);
        public void DrawBitmapLattice(SKBitmap bitmap, SKLattice lattice, SKRect dst, SKPaint paint = null);
        public void DrawBitmapLattice(SKBitmap bitmap, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint = null);
        public void DrawBitmapNinePatch(SKBitmap bitmap, SKRectI center, SKRect dst, SKPaint paint = null);
        public void DrawCircle(SKPoint c, float radius, SKPaint paint);
        public void DrawCircle(float cx, float cy, float radius, SKPaint paint);
        public void DrawColor(SKColor color, SKBlendMode mode = SKBlendMode.Src);
        public void DrawDrawable(SKDrawable drawable, ref SKMatrix matrix);
        public void DrawDrawable(SKDrawable drawable, SKPoint p);
        public void DrawDrawable(SKDrawable drawable, float x, float y);
        public void DrawImage(SKImage image, SKPoint p, SKPaint paint = null);
        public void DrawImage(SKImage image, SKRect dest, SKPaint paint = null);
        public void DrawImage(SKImage image, SKRect source, SKRect dest, SKPaint paint = null);
        public void DrawImage(SKImage image, float x, float y, SKPaint paint = null);
        public void DrawImageLattice(SKImage image, SKLattice lattice, SKRect dst, SKPaint paint = null);
        public void DrawImageLattice(SKImage image, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint = null);
        public void DrawImageNinePatch(SKImage image, SKRectI center, SKRect dst, SKPaint paint = null);
        public void DrawLine(SKPoint p0, SKPoint p1, SKPaint paint);
        public void DrawLine(float x0, float y0, float x1, float y1, SKPaint paint);
        public void DrawLinkDestinationAnnotation(SKRect rect, SKData value);
        public SKData DrawLinkDestinationAnnotation(SKRect rect, string value);
        public void DrawNamedDestinationAnnotation(SKPoint point, SKData value);
        public SKData DrawNamedDestinationAnnotation(SKPoint point, string value);
        public void DrawOval(SKPoint c, SKSize r, SKPaint paint);
        public void DrawOval(SKRect rect, SKPaint paint);
        public void DrawOval(float cx, float cy, float rx, float ry, SKPaint paint);
        public void DrawPaint(SKPaint paint);
        public void DrawPatch(SKPoint[] cubics, SKColor[] colors, SKBlendMode mode, SKPaint paint);
        public void DrawPatch(SKPoint[] cubics, SKColor[] colors, SKPaint paint);
        public void DrawPatch(SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKBlendMode mode, SKPaint paint);
        public void DrawPatch(SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKPaint paint);
        public void DrawPatch(SKPoint[] cubics, SKPoint[] texCoords, SKBlendMode mode, SKPaint paint);
        public void DrawPatch(SKPoint[] cubics, SKPoint[] texCoords, SKPaint paint);
        public void DrawPath(SKPath path, SKPaint paint);
        public void DrawPicture(SKPicture picture, ref SKMatrix matrix, SKPaint paint = null);
        public void DrawPicture(SKPicture picture, SKPaint paint = null);
        public void DrawPicture(SKPicture picture, SKPoint p, SKPaint paint = null);
        public void DrawPicture(SKPicture picture, float x, float y, SKPaint paint = null);
        public void DrawPoint(SKPoint p, SKColor color);
        public void DrawPoint(SKPoint p, SKPaint paint);
        public void DrawPoint(float x, float y, SKColor color);
        public void DrawPoint(float x, float y, SKPaint paint);
        public void DrawPoints(SKPointMode mode, SKPoint[] points, SKPaint paint);
        public void DrawPositionedText(byte[] text, SKPoint[] points, SKPaint paint);
        public void DrawPositionedText(IntPtr buffer, int length, SKPoint[] points, SKPaint paint);
        public void DrawPositionedText(string text, SKPoint[] points, SKPaint paint);
        public void DrawRect(SKRect rect, SKPaint paint);
        public void DrawRect(float x, float y, float w, float h, SKPaint paint);
        public void DrawRegion(SKRegion region, SKPaint paint);
        public void DrawRoundRect(SKRect rect, SKSize r, SKPaint paint);
        public void DrawRoundRect(SKRect rect, float rx, float ry, SKPaint paint);
        public void DrawRoundRect(SKRoundRect rect, SKPaint paint);
        public void DrawRoundRect(float x, float y, float w, float h, float rx, float ry, SKPaint paint);
        public void DrawRoundRectDifference(SKRoundRect outer, SKRoundRect inner, SKPaint paint);
        public void DrawSurface(SKSurface surface, SKPoint p, SKPaint paint = null);
        public void DrawSurface(SKSurface surface, float x, float y, SKPaint paint = null);
        public void DrawText(SKTextBlob text, float x, float y, SKPaint paint);
        public void DrawText(byte[] text, SKPoint p, SKPaint paint);
        public void DrawText(byte[] text, float x, float y, SKPaint paint);
        public void DrawText(IntPtr buffer, int length, SKPoint p, SKPaint paint);
        public void DrawText(IntPtr buffer, int length, float x, float y, SKPaint paint);
        public void DrawText(string text, SKPoint p, SKPaint paint);
        public void DrawText(string text, float x, float y, SKPaint paint);
        public void DrawTextOnPath(byte[] text, SKPath path, SKPoint offset, SKPaint paint);
        public void DrawTextOnPath(byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint);
        public void DrawTextOnPath(IntPtr buffer, int length, SKPath path, SKPoint offset, SKPaint paint);
        public void DrawTextOnPath(IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint);
        public void DrawTextOnPath(string text, SKPath path, SKPoint offset, SKPaint paint);
        public void DrawTextOnPath(string text, SKPath path, float hOffset, float vOffset, SKPaint paint);
        public void DrawUrlAnnotation(SKRect rect, SKData value);
        public SKData DrawUrlAnnotation(SKRect rect, string value);
        public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKColor[] colors, SKPaint paint);
        public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKBlendMode mode, ushort[] indices, SKPaint paint);
        public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKPaint paint);
        public void DrawVertices(SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, ushort[] indices, SKPaint paint);
        public void DrawVertices(SKVertices vertices, SKBlendMode mode, SKPaint paint);
        public void Flush();
        public bool GetDeviceClipBounds(out SKRectI bounds);
        public bool GetLocalClipBounds(out SKRect bounds);
        public bool QuickReject(SKPath path);
        public bool QuickReject(SKRect rect);
        public void ResetMatrix();
        public void Restore();
        public void RestoreToCount(int count);
        public void RotateDegrees(float degrees);
        public void RotateDegrees(float degrees, float px, float py);
        public void RotateRadians(float radians);
        public void RotateRadians(float radians, float px, float py);
        public int Save();
        public int SaveLayer(SKPaint paint);
        public int SaveLayer(SKRect limit, SKPaint paint);
        public void Scale(SKPoint size);
        public void Scale(float s);
        public void Scale(float sx, float sy);
        public void Scale(float sx, float sy, float px, float py);
        public void SetMatrix(SKMatrix matrix);
        public void Skew(SKPoint skew);
        public void Skew(float sx, float sy);
        public void Translate(SKPoint point);
        public void Translate(float dx, float dy);
    }
    public enum SKClipOperation
    {
        Difference = 0,
        Intersect = 1,
    }
    public class SKCodec : SKObject
    {
        public SKEncodedImageFormat EncodedFormat { get; }
        public SKEncodedOrigin EncodedOrigin { get; }
        public int FrameCount { get; }
        public SKCodecFrameInfo[] FrameInfo { get; }
        public SKImageInfo Info { get; }
        public static int MinBufferedBytesNeeded { get; }
        public int NextScanline { get; }
        public SKCodecOrigin Origin { get; }
        public byte[] Pixels { get; }
        public int RepetitionCount { get; }
        public SKCodecScanlineOrder ScanlineOrder { get; }
        public static SKCodec Create(SKData data);
        public static SKCodec Create(SKStream stream);
        public static SKCodec Create(SKStream stream, out SKCodecResult result);
        public static SKCodec Create(Stream stream);
        public static SKCodec Create(Stream stream, out SKCodecResult result);
        public static SKCodec Create(string filename);
        public static SKCodec Create(string filename, out SKCodecResult result);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public bool GetFrameInfo(int index, out SKCodecFrameInfo frameInfo);
        public int GetOutputScanline(int inputScanline);
        public SKCodecResult GetPixels(SKImageInfo info, byte[] pixels);
        public SKCodecResult GetPixels(SKImageInfo info, out byte[] pixels);
        public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels);
        public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, SKCodecOptions options);
        public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
        public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
        public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, SKColorTable colorTable, ref int colorTableCount);
        public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options);
        public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
        public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
        public SKCodecResult GetPixels(SKImageInfo info, IntPtr pixels, IntPtr colorTable, ref int colorTableCount);
        public SKCodecResult GetPixels(out byte[] pixels);
        public SKSizeI GetScaledDimensions(float desiredScale);
        public int GetScanlines(IntPtr dst, int countLines, int rowBytes);
        public bool GetValidSubset(ref SKRectI desiredSubset);
        public SKCodecResult IncrementalDecode();
        public SKCodecResult IncrementalDecode(out int rowsDecoded);
        public bool SkipScanlines(int countLines);
        public SKCodecResult StartIncrementalDecode(SKImageInfo info, IntPtr pixels, int rowBytes);
        public SKCodecResult StartIncrementalDecode(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options);
        public SKCodecResult StartIncrementalDecode(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
        public SKCodecResult StartIncrementalDecode(SKImageInfo info, IntPtr pixels, int rowBytes, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
        public SKCodecResult StartScanlineDecode(SKImageInfo info);
        public SKCodecResult StartScanlineDecode(SKImageInfo info, SKCodecOptions options);
        public SKCodecResult StartScanlineDecode(SKImageInfo info, SKCodecOptions options, SKColorTable colorTable, ref int colorTableCount);
        public SKCodecResult StartScanlineDecode(SKImageInfo info, SKCodecOptions options, IntPtr colorTable, ref int colorTableCount);
    }
    public enum SKCodecAnimationDisposalMethod
    {
        Keep = 1,
        RestoreBackgroundColor = 2,
        RestorePrevious = 3,
    }
    public struct SKCodecFrameInfo
    {
        public SKAlphaType AlphaType { get; set; }
        public SKCodecAnimationDisposalMethod DisposalMethod { get; set; }
        public int Duration { get; set; }
        public bool FullyRecieved { get; set; }
        public int RequiredFrame { get; set; }
    }
    public struct SKCodecOptions
    {
        public static readonly SKCodecOptions Default;
        public SKCodecOptions(SKRectI subset);
        public SKCodecOptions(SKZeroInitialized zeroInitialized);
        public SKCodecOptions(SKZeroInitialized zeroInitialized, SKRectI subset);
        public SKCodecOptions(int frameIndex);
        public SKCodecOptions(int frameIndex, int priorFrame);
        public int FrameIndex { get; set; }
        public bool HasSubset { get; }
        public SKTransferFunctionBehavior PremulBehavior { get; set; }
        public int PriorFrame { get; set; }
        public Nullable<SKRectI> Subset { get; set; }
        public SKZeroInitialized ZeroInitialized { get; set; }
    }
    public enum SKCodecOrigin
    {
        BottomLeft = 4,
        BottomRight = 3,
        LeftBottom = 8,
        LeftTop = 5,
        RightBottom = 7,
        RightTop = 6,
        TopLeft = 1,
        TopRight = 2,
    }
    public enum SKCodecResult
    {
        CouldNotRewind = 7,
        ErrorInInput = 2,
        IncompleteInput = 1,
        InternalError = 8,
        InvalidConversion = 3,
        InvalidInput = 6,
        InvalidParameters = 5,
        InvalidScale = 4,
        Success = 0,
        Unimplemented = 9,
    }
    public enum SKCodecScanlineOrder
    {
        BottomUp = 1,
        TopDown = 0,
    }
    public struct SKColor
    {
        public static readonly SKColor Empty;
        public SKColor(byte red, byte green, byte blue);
        public SKColor(byte red, byte green, byte blue, byte alpha);
        public SKColor(uint value);
        public byte Alpha { get; }
        public byte Blue { get; }
        public byte Green { get; }
        public float Hue { get; }
        public byte Red { get; }
        public override bool Equals(object other);
        public static SKColor FromHsl(float h, float s, float l, byte a = (byte)255);
        public static SKColor FromHsv(float h, float s, float v, byte a = (byte)255);
        public override int GetHashCode();
        public static bool operator ==(SKColor left, SKColor right);
        public static explicit operator uint (SKColor color);
        public static implicit operator SKColor (uint color);
        public static bool operator !=(SKColor left, SKColor right);
        public static SKColor Parse(string hexString);
        public void ToHsl(out float h, out float s, out float l);
        public void ToHsv(out float h, out float s, out float v);
        public override string ToString();
        public static bool TryParse(string hexString, out SKColor color);
        public SKColor WithAlpha(byte alpha);
        public SKColor WithBlue(byte blue);
        public SKColor WithGreen(byte green);
        public SKColor WithRed(byte red);
    }
    public class SKColorFilter : SKObject
    {
        public const int ColorMatrixSize = 20;
        public const int TableMaxLength = 256;
        public static SKColorFilter CreateBlendMode(SKColor c, SKBlendMode mode);
        public static SKColorFilter CreateColorMatrix(float[] matrix);
        public static SKColorFilter CreateCompose(SKColorFilter outer, SKColorFilter inner);
        public static SKColorFilter CreateHighContrast(SKHighContrastConfig config);
        public static SKColorFilter CreateHighContrast(bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast);
        public static SKColorFilter CreateLighting(SKColor mul, SKColor add);
        public static SKColorFilter CreateLumaColor();
        public static SKColorFilter CreateTable(byte[] table);
        public static SKColorFilter CreateTable(byte[] tableA, byte[] tableR, byte[] tableG, byte[] tableB);
        protected override void Dispose(bool disposing);
    }
    public struct SKColors
    {
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
        public static SKColor Empty { get; }
    }
    public class SKColorSpace : SKObject
    {
        public bool GammaIsCloseToSrgb { get; }
        public bool GammaIsLinear { get; }
        public bool IsNumericalTransferFunction { get; }
        public bool IsSrgb { get; }
        public SKNamedGamma NamedGamma { get; }
        public SKColorSpaceType Type { get; }
        public static SKColorSpace CreateIcc(byte[] input);
        public static SKColorSpace CreateIcc(byte[] input, long length);
        public static SKColorSpace CreateIcc(IntPtr input, long length);
        public static SKColorSpace CreateRgb(SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut);
        public static SKColorSpace CreateRgb(SKColorSpaceRenderTargetGamma gamma, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);
        public static SKColorSpace CreateRgb(SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50);
        public static SKColorSpace CreateRgb(SKColorSpaceRenderTargetGamma gamma, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);
        public static SKColorSpace CreateRgb(SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut);
        public static SKColorSpace CreateRgb(SKColorSpaceTransferFn coeffs, SKColorSpaceGamut gamut, SKColorSpaceFlags flags);
        public static SKColorSpace CreateRgb(SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50);
        public static SKColorSpace CreateRgb(SKColorSpaceTransferFn coeffs, SKMatrix44 toXyzD50, SKColorSpaceFlags flags);
        public static SKColorSpace CreateRgb(SKNamedGamma gamma, SKColorSpaceGamut gamut);
        public static SKColorSpace CreateRgb(SKNamedGamma gamma, SKMatrix44 toXyzD50);
        public static SKColorSpace CreateSrgb();
        public static SKColorSpace CreateSrgbLinear();
        protected override void Dispose(bool disposing);
        public static bool Equal(SKColorSpace left, SKColorSpace right);
        public SKMatrix44 FromXyzD50();
        public bool GetNumericalTransferFunction(out SKColorSpaceTransferFn fn);
        public SKMatrix44 ToXyzD50();
        public bool ToXyzD50(SKMatrix44 toXyzD50);
    }
    public enum SKColorSpaceFlags
    {
        None = 0,
        NonLinearBlending = 1,
    }
    public enum SKColorSpaceGamut
    {
        AdobeRgb = 1,
        Dcip3D65 = 2,
        Rec2020 = 3,
        Srgb = 0,
    }
    public struct SKColorSpacePrimaries
    {
        public SKColorSpacePrimaries(float rx, float ry, float gx, float gy, float bx, float by, float wx, float wy);
        public SKColorSpacePrimaries(float[] values);
        public float BX { get; set; }
        public float BY { get; set; }
        public float GX { get; set; }
        public float GY { get; set; }
        public float RX { get; set; }
        public float RY { get; set; }
        public float[] Values { get; }
        public float WX { get; set; }
        public float WY { get; set; }
        public SKMatrix44 ToXyzD50();
        public bool ToXyzD50(SKMatrix44 toXyzD50);
    }
    public enum SKColorSpaceRenderTargetGamma
    {
        Linear = 0,
        Srgb = 1,
    }
    public struct SKColorSpaceTransferFn
    {
        public SKColorSpaceTransferFn(float g, float a, float b, float c, float d, float e, float f);
        public SKColorSpaceTransferFn(float[] values);
        public float A { get; set; }
        public float B { get; set; }
        public float C { get; set; }
        public float D { get; set; }
        public float E { get; set; }
        public float F { get; set; }
        public float G { get; set; }
        public float[] Values { get; }
        public SKColorSpaceTransferFn Invert();
        public float Transform(float x);
    }
    public enum SKColorSpaceType
    {
        Cmyk = 1,
        Gray = 2,
        Rgb = 0,
    }
    public class SKColorTable : SKObject
    {
        public const int MaxLength = 256;
        public SKColorTable();
        public SKColorTable(SKColor[] colors);
        public SKColorTable(SKColor[] colors, int count);
        public SKColorTable(SKPMColor[] colors);
        public SKColorTable(SKPMColor[] colors, int count);
        public SKColorTable(int count);
        public SKPMColor[] Colors { get; }
        public int Count { get; }
        public SKPMColor this[int index] { get; }
        public SKColor[] UnPreMultipledColors { get; }
        protected override void Dispose(bool disposing);
        public SKColor GetUnPreMultipliedColor(int index);
        public IntPtr ReadColors();
    }
    public enum SKColorType
    {
        Alpha8 = 1,
        Argb4444 = 3,
        Bgra8888 = 6,
        Gray8 = 9,
        Rgb101010x = 8,
        Rgb565 = 2,
        Rgb888x = 5,
        Rgba1010102 = 7,
        Rgba8888 = 4,
        RgbaF16 = 10,
        Unknown = 0,
    }
    public enum SKCropRectFlags
    {
        HasAll = 15,
        HasHeight = 8,
        HasLeft = 1,
        HasNone = 0,
        HasTop = 2,
        HasWidth = 4,
    }
    public class SKData : SKObject
    {
        public IntPtr Data { get; }
        public static SKData Empty { get; }
        public bool IsEmpty { get; }
        public long Size { get; }
        public ReadOnlySpan<byte> AsSpan();
        public Stream AsStream();
        public Stream AsStream(bool streamDisposesData);
        public static SKData Create(SKStream stream);
        public static SKData Create(SKStream stream, int length);
        public static SKData Create(SKStream stream, long length);
        public static SKData Create(SKStream stream, ulong length);
        public static SKData Create(int size);
        public static SKData Create(IntPtr address, int length);
        public static SKData Create(IntPtr address, int length, SKDataReleaseDelegate releaseProc);
        public static SKData Create(IntPtr address, int length, SKDataReleaseDelegate releaseProc, object context);
        public static SKData Create(Stream stream);
        public static SKData Create(Stream stream, int length);
        public static SKData Create(Stream stream, long length);
        public static SKData Create(Stream stream, ulong length);
        public static SKData Create(string filename);
        public static SKData Create(ulong size);
        public static SKData CreateCopy(byte[] bytes);
        public static SKData CreateCopy(byte[] bytes, ulong length);
        public static SKData CreateCopy(IntPtr bytes, ulong length);
        public static SKData CreateCopy(ReadOnlySpan<byte> bytes);
        protected override void Dispose(bool disposing);
        public void SaveTo(Stream target);
        public SKData Subset(ulong offset, ulong length);
        public byte[] ToArray();
    }
    public delegate void SKDataReleaseDelegate(IntPtr address, object context);
    public enum SKDisplacementMapEffectChannelSelectorType
    {
        A = 4,
        B = 3,
        G = 2,
        R = 1,
        Unknown = 0,
    }
    public class SKDocument : SKObject
    {
        public const float DefaultRasterDpi = 72f;
        public void Abort();
        public SKCanvas BeginPage(float width, float height);
        public SKCanvas BeginPage(float width, float height, SKRect content);
        public void Close();
        public static SKDocument CreatePdf(SKWStream stream);
        public static SKDocument CreatePdf(SKWStream stream, SKDocumentPdfMetadata metadata);
        public static SKDocument CreatePdf(SKWStream stream, SKDocumentPdfMetadata metadata, float dpi);
        public static SKDocument CreatePdf(SKWStream stream, float dpi);
        public static SKDocument CreatePdf(Stream stream);
        public static SKDocument CreatePdf(Stream stream, SKDocumentPdfMetadata metadata);
        public static SKDocument CreatePdf(Stream stream, float dpi);
        public static SKDocument CreatePdf(string path);
        public static SKDocument CreatePdf(string path, SKDocumentPdfMetadata metadata);
        public static SKDocument CreatePdf(string path, float dpi);
        public static SKDocument CreateXps(SKWStream stream);
        public static SKDocument CreateXps(SKWStream stream, float dpi);
        public static SKDocument CreateXps(Stream stream);
        public static SKDocument CreateXps(Stream stream, float dpi);
        public static SKDocument CreateXps(string path);
        public static SKDocument CreateXps(string path, float dpi);
        protected override void Dispose(bool disposing);
        public void EndPage();
    }
    public struct SKDocumentPdfMetadata
    {
        public static readonly SKDocumentPdfMetadata Default;
        public const int DefaultEncodingQuality = 101;
        public const float DefaultRasterDpi = 72f;
        public SKDocumentPdfMetadata(int encodingQuality);
        public SKDocumentPdfMetadata(float rasterDpi);
        public SKDocumentPdfMetadata(float rasterDpi, int encodingQuality);
        public string Author { get; set; }
        public Nullable<DateTime> Creation { get; set; }
        public string Creator { get; set; }
        public int EncodingQuality { get; set; }
        public string Keywords { get; set; }
        public Nullable<DateTime> Modified { get; set; }
        public bool PdfA { get; set; }
        public string Producer { get; set; }
        public float RasterDpi { get; set; }
        public string Subject { get; set; }
        public string Title { get; set; }
    }
    public class SKDrawable : SKObject
    {
        protected SKDrawable();
        protected SKDrawable(bool owns);
        public SKRect Bounds { get; }
        public uint GenerationId { get; }
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public void Draw(SKCanvas canvas, ref SKMatrix matrix);
        public void Draw(SKCanvas canvas, float x, float y);
        public void NotifyDrawingChanged();
        protected virtual void OnDraw(SKCanvas canvas);
        protected virtual SKRect OnGetBounds();
        protected virtual SKPicture OnSnapshot();
        public SKPicture Snapshot();
    }
    public enum SKDropShadowImageFilterShadowMode
    {
        DrawShadowAndForeground = 0,
        DrawShadowOnly = 1,
    }
    public class SKDynamicMemoryWStream : SKWStream
    {
        public SKDynamicMemoryWStream();
        public bool CopyTo(SKWStream dst);
        public void CopyTo(IntPtr data);
        public SKData CopyToData();
        public SKData DetachAsData();
        public SKStreamAsset DetachAsStream();
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
    }
    public enum SKEncodedImageFormat
    {
        Astc = 9,
        Bmp = 0,
        Dng = 10,
        Gif = 1,
        Heif = 11,
        Ico = 2,
        Jpeg = 3,
        Ktx = 8,
        Pkm = 7,
        Png = 4,
        Wbmp = 5,
        Webp = 6,
    }
    public enum SKEncodedOrigin
    {
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
    public enum SKEncoding
    {
        Utf16 = 1,
        Utf32 = 2,
        Utf8 = 0,
    }
    public class SKFileStream : SKStreamAsset
    {
        public SKFileStream(string path);
        public bool IsValid { get; }
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public static bool IsPathSupported(string path);
        public static SKStreamAsset OpenStream(string path);
    }
    public class SKFileWStream : SKWStream
    {
        public SKFileWStream(string path);
        public bool IsValid { get; }
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public static bool IsPathSupported(string path);
        public static SKWStream OpenStream(string path);
    }
    public enum SKFilterQuality
    {
        High = 3,
        Low = 1,
        Medium = 2,
        None = 0,
    }
    public class SKFontManager : SKObject
    {
        public static SKFontManager Default { get; }
        public IEnumerable<string> FontFamilies { get; }
        public int FontFamilyCount { get; }
        public static SKFontManager CreateDefault();
        public SKTypeface CreateTypeface(SKData data, int index = 0);
        public SKTypeface CreateTypeface(SKStreamAsset stream, int index = 0);
        public SKTypeface CreateTypeface(Stream stream, int index = 0);
        public SKTypeface CreateTypeface(string path, int index = 0);
        protected override void Dispose(bool disposing);
        public string GetFamilyName(int index);
        public string[] GetFontFamilies();
        public SKFontStyleSet GetFontStyles(int index);
        public SKFontStyleSet GetFontStyles(string familyName);
        public SKTypeface MatchCharacter(char character);
        public SKTypeface MatchCharacter(int character);
        public SKTypeface MatchCharacter(string familyName, SKFontStyle style, string[] bcp47, int character);
        public SKTypeface MatchCharacter(string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, char character);
        public SKTypeface MatchCharacter(string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, string[] bcp47, int character);
        public SKTypeface MatchCharacter(string familyName, char character);
        public SKTypeface MatchCharacter(string familyName, int character);
        public SKTypeface MatchCharacter(string familyName, int weight, int width, SKFontStyleSlant slant, string[] bcp47, int character);
        public SKTypeface MatchCharacter(string familyName, string[] bcp47, char character);
        public SKTypeface MatchCharacter(string familyName, string[] bcp47, int character);
        public SKTypeface MatchFamily(string familyName, SKFontStyle style);
        public SKTypeface MatchTypeface(SKTypeface face, SKFontStyle style);
    }
    public struct SKFontMetrics
    {
        public float Ascent { get; }
        public float AverageCharacterWidth { get; }
        public float Bottom { get; }
        public float CapHeight { get; }
        public float Descent { get; }
        public float Leading { get; }
        public float MaxCharacterWidth { get; }
        public Nullable<float> StrikeoutPosition { get; }
        public Nullable<float> StrikeoutThickness { get; }
        public float Top { get; }
        public Nullable<float> UnderlinePosition { get; }
        public Nullable<float> UnderlineThickness { get; }
        public float XHeight { get; }
        public float XMax { get; }
        public float XMin { get; }
    }
    public class SKFontStyle : SKObject
    {
        public SKFontStyle();
        public SKFontStyle(SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant);
        public SKFontStyle(int weight, int width, SKFontStyleSlant slant);
        public static SKFontStyle Bold { get; }
        public static SKFontStyle BoldItalic { get; }
        public static SKFontStyle Italic { get; }
        public static SKFontStyle Normal { get; }
        public SKFontStyleSlant Slant { get; }
        public int Weight { get; }
        public int Width { get; }
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
    }
    public class SKFontStyleSet : SKObject, IEnumerable, IEnumerable<SKFontStyle>, IReadOnlyCollection<SKFontStyle>, IReadOnlyList<SKFontStyle>
    {
        public SKFontStyleSet();
        public int Count { get; }
        public SKFontStyle this[int index] { get; }
        public SKTypeface CreateTypeface(SKFontStyle style);
        public SKTypeface CreateTypeface(int index);
        protected override void Dispose(bool disposing);
        public IEnumerator<SKFontStyle> GetEnumerator();
        public string GetStyleName(int index);
        IEnumerator System.Collections.IEnumerable.GetEnumerator();
    }
    public enum SKFontStyleSlant
    {
        Italic = 1,
        Oblique = 2,
        Upright = 0,
    }
    public enum SKFontStyleWeight
    {
        Black = 900,
        Bold = 700,
        ExtraBlack = 1000,
        ExtraBold = 800,
        ExtraLight = 200,
        Invisible = 0,
        Light = 300,
        Medium = 500,
        Normal = 400,
        SemiBold = 600,
        Thin = 100,
    }
    public enum SKFontStyleWidth
    {
        Condensed = 3,
        Expanded = 7,
        ExtraCondensed = 2,
        ExtraExpanded = 8,
        Normal = 5,
        SemiCondensed = 4,
        SemiExpanded = 6,
        UltraCondensed = 1,
        UltraExpanded = 9,
    }
    public class SKFrontBufferedManagedStream : SKAbstractManagedStream
    {
        public SKFrontBufferedManagedStream(SKStream nativeStream, int bufferSize);
        public SKFrontBufferedManagedStream(SKStream nativeStream, int bufferSize, bool disposeUnderlyingStream);
        public SKFrontBufferedManagedStream(Stream managedStream, int bufferSize);
        public SKFrontBufferedManagedStream(Stream managedStream, int bufferSize, bool disposeUnderlyingStream);
        protected override void Dispose(bool disposing);
        protected override void DisposeManaged();
        protected override IntPtr OnCreateNew();
        protected override IntPtr OnGetLength();
        protected override IntPtr OnGetPosition();
        protected override bool OnHasLength();
        protected override bool OnHasPosition();
        protected override bool OnIsAtEnd();
        protected override bool OnMove(int offset);
        protected override IntPtr OnPeek(IntPtr buffer, IntPtr size);
        protected override IntPtr OnRead(IntPtr buffer, IntPtr size);
        protected override bool OnRewind();
        protected override bool OnSeek(IntPtr position);
    }
    public class SKFrontBufferedStream : Stream
    {
        public const int DefaultBufferSize = 4096;
        public SKFrontBufferedStream(Stream stream);
        public SKFrontBufferedStream(Stream stream, bool disposeUnderlyingStream);
        public SKFrontBufferedStream(Stream stream, long bufferSize);
        public SKFrontBufferedStream(Stream stream, long bufferSize, bool disposeUnderlyingStream);
        public override bool CanRead { get; }
        public override bool CanSeek { get; }
        public override bool CanWrite { get; }
        public override long Length { get; }
        public override long Position { get; set; }
        protected override void Dispose(bool disposing);
        public override void Flush();
        public override int Read(byte[] buffer, int offset, int count);
        public override long Seek(long offset, SeekOrigin origin);
        public override void SetLength(long value);
        public override void Write(byte[] buffer, int offset, int count);
    }
    public struct SKHighContrastConfig
    {
        public static readonly SKHighContrastConfig Default;
        public SKHighContrastConfig(bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast);
        public float Contrast { get; set; }
        public bool Grayscale { get; set; }
        public SKHighContrastConfigInvertStyle InvertStyle { get; set; }
        public bool IsValid { get; }
    }
    public enum SKHighContrastConfigInvertStyle
    {
        InvertBrightness = 1,
        InvertLightness = 2,
        NoInvert = 0,
    }
    public sealed class SKHorizontalRunBuffer : SKRunBuffer
    {
        public Span<float> GetPositionSpan();
        public void SetPositions(ReadOnlySpan<float> positions);
    }
    public static class SkiaExtensions
    {
        public static bool IsBgr(this SKPixelGeometry pg);
        public static bool IsHorizontal(this SKPixelGeometry pg);
        public static bool IsRgb(this SKPixelGeometry pg);
        public static bool IsVertical(this SKPixelGeometry pg);
        public static SKColorType ToColorType(this GRPixelConfig config);
        public static SKFilterQuality ToFilterQuality(this SKBitmapResizeMethod method);
        public static uint ToGlSizedFormat(this GRPixelConfig config);
        public static uint ToGlSizedFormat(this SKColorType colorType);
        public static GRPixelConfig ToPixelConfig(this SKColorType colorType);
    }
    public class SKImage : SKObject
    {
        public SKAlphaType AlphaType { get; }
        public SKColorSpace ColorSpace { get; }
        public SKColorType ColorType { get; }
        public SKData EncodedData { get; }
        public int Height { get; }
        public bool IsAlphaOnly { get; }
        public bool IsLazyGenerated { get; }
        public bool IsTextureBacked { get; }
        public uint UniqueId { get; }
        public int Width { get; }
        public SKImage ApplyImageFilter(SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPoint outOffset);
        public SKImage ApplyImageFilter(SKImageFilter filter, SKRectI subset, SKRectI clipBounds, out SKRectI outSubset, out SKPointI outOffset);
        public static SKImage Create(SKImageInfo info);
        protected override void Dispose(bool disposing);
        public SKData Encode();
        public SKData Encode(SKEncodedImageFormat format, int quality);
        public SKData Encode(SKPixelSerializer serializer);
        public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
        public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha);
        public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace);
        public static SKImage FromAdoptedTexture(GRContext context, GRBackendTexture texture, SKColorType colorType);
        public static SKImage FromAdoptedTexture(GRContext context, GRBackendTextureDesc desc);
        public static SKImage FromAdoptedTexture(GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);
        public static SKImage FromAdoptedTexture(GRContext context, GRGlBackendTextureDesc desc);
        public static SKImage FromAdoptedTexture(GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);
        public static SKImage FromBitmap(SKBitmap bitmap);
        public static SKImage FromEncodedData(SKData data);
        public static SKImage FromEncodedData(SKData data, SKRectI subset);
        public static SKImage FromEncodedData(SKStream data);
        public static SKImage FromEncodedData(byte[] data);
        public static SKImage FromEncodedData(Stream data);
        public static SKImage FromEncodedData(ReadOnlySpan<byte> data);
        public static SKImage FromEncodedData(string filename);
        public static SKImage FromPicture(SKPicture picture, SKSizeI dimensions);
        public static SKImage FromPicture(SKPicture picture, SKSizeI dimensions, SKMatrix matrix);
        public static SKImage FromPicture(SKPicture picture, SKSizeI dimensions, SKMatrix matrix, SKPaint paint);
        public static SKImage FromPicture(SKPicture picture, SKSizeI dimensions, SKPaint paint);
        public static SKImage FromPixelCopy(SKImageInfo info, SKStream pixels);
        public static SKImage FromPixelCopy(SKImageInfo info, SKStream pixels, int rowBytes);
        public static SKImage FromPixelCopy(SKImageInfo info, byte[] pixels);
        public static SKImage FromPixelCopy(SKImageInfo info, byte[] pixels, int rowBytes);
        public static SKImage FromPixelCopy(SKImageInfo info, IntPtr pixels);
        public static SKImage FromPixelCopy(SKImageInfo info, IntPtr pixels, int rowBytes);
        public static SKImage FromPixelCopy(SKImageInfo info, IntPtr pixels, int rowBytes, SKColorTable ctable);
        public static SKImage FromPixelCopy(SKImageInfo info, Stream pixels);
        public static SKImage FromPixelCopy(SKImageInfo info, Stream pixels, int rowBytes);
        public static SKImage FromPixelCopy(SKImageInfo info, ReadOnlySpan<byte> pixels);
        public static SKImage FromPixelCopy(SKImageInfo info, ReadOnlySpan<byte> pixels, int rowBytes);
        public static SKImage FromPixelCopy(SKPixmap pixmap);
        public static SKImage FromPixelData(SKImageInfo info, SKData data, int rowBytes);
        public static SKImage FromPixels(SKImageInfo info, SKData data, int rowBytes);
        public static SKImage FromPixels(SKImageInfo info, IntPtr pixels);
        public static SKImage FromPixels(SKImageInfo info, IntPtr pixels, int rowBytes);
        public static SKImage FromPixels(SKPixmap pixmap);
        public static SKImage FromPixels(SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc);
        public static SKImage FromPixels(SKPixmap pixmap, SKImageRasterReleaseDelegate releaseProc, object releaseContext);
        public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
        public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha);
        public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace);
        public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc);
        public static SKImage FromTexture(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKAlphaType alpha, SKColorSpace colorspace, SKImageTextureReleaseDelegate releaseProc, object releaseContext);
        public static SKImage FromTexture(GRContext context, GRBackendTexture texture, SKColorType colorType);
        public static SKImage FromTexture(GRContext context, GRBackendTextureDesc desc);
        public static SKImage FromTexture(GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha);
        public static SKImage FromTexture(GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);
        public static SKImage FromTexture(GRContext context, GRBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);
        public static SKImage FromTexture(GRContext context, GRGlBackendTextureDesc desc);
        public static SKImage FromTexture(GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha);
        public static SKImage FromTexture(GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc);
        public static SKImage FromTexture(GRContext context, GRGlBackendTextureDesc desc, SKAlphaType alpha, SKImageTextureReleaseDelegate releaseProc, object releaseContext);
        public SKPixmap PeekPixels();
        public bool PeekPixels(SKPixmap pixmap);
        public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY);
        public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKImageCachingHint cachingHint);
        public bool ReadPixels(SKPixmap pixmap, int srcX, int srcY);
        public bool ReadPixels(SKPixmap pixmap, int srcX, int srcY, SKImageCachingHint cachingHint);
        public bool ScalePixels(SKPixmap dst, SKFilterQuality quality);
        public bool ScalePixels(SKPixmap dst, SKFilterQuality quality, SKImageCachingHint cachingHint);
        public SKImage Subset(SKRectI subset);
        public SKImage ToRasterImage();
        public SKShader ToShader();
        public SKShader ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY);
        public SKShader ToShader(SKShaderTileMode tileX, SKShaderTileMode tileY, SKMatrix localMatrix);
    }
    public enum SKImageCachingHint
    {
        Allow = 0,
        Disallow = 1,
    }
    public class SKImageFilter : SKObject
    {
        public static SKImageFilter CreateAlphaThreshold(SKRectI region, float innerThreshold, float outerThreshold, SKImageFilter input = null);
        public static SKImageFilter CreateAlphaThreshold(SKRegion region, float innerThreshold, float outerThreshold, SKImageFilter input = null);
        public static SKImageFilter CreateArithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateBlendMode(SKBlendMode mode, SKImageFilter background, SKImageFilter foreground = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateBlur(float sigmaX, float sigmaY, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateColorFilter(SKColorFilter cf, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateCompose(SKImageFilter outer, SKImageFilter inner);
        public static SKImageFilter CreateDilate(int radiusX, int radiusY, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateDisplacementMapEffect(SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateDistantLitDiffuse(SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateDistantLitSpecular(SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateErode(int radiusX, int radiusY, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateImage(SKImage image);
        public static SKImageFilter CreateImage(SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality);
        public static SKImageFilter CreateMagnifier(SKRect src, float inset, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateMatrix(SKMatrix matrix, SKFilterQuality quality, SKImageFilter input = null);
        public static SKImageFilter CreateMatrixConvolution(SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateMerge(SKImageFilter first, SKImageFilter second, SKBlendMode mode, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateMerge(SKImageFilter first, SKImageFilter second, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateMerge(SKImageFilter[] filters, SKBlendMode[] modes, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateMerge(SKImageFilter[] filters, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateOffset(float dx, float dy, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreatePaint(SKPaint paint, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreatePicture(SKPicture picture);
        public static SKImageFilter CreatePicture(SKPicture picture, SKRect cropRect);
        public static SKImageFilter CreatePointLitDiffuse(SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreatePointLitSpecular(SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateSpotLitDiffuse(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateSpotLitSpecular(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null);
        public static SKImageFilter CreateTile(SKRect src, SKRect dst, SKImageFilter input);
        protected override void Dispose(bool disposing);
        public class CropRect : SKObject
        {
            public CropRect();
            public CropRect(SKRect rect, SKCropRectFlags flags = SKCropRectFlags.HasAll);
            public SKCropRectFlags Flags { get; }
            public SKRect Rect { get; }
            protected override void Dispose(bool disposing);
            protected override void DisposeNative();
        }
    }
    public struct SKImageInfo
    {
        public static readonly SKImageInfo Empty;
        public static readonly int PlatformColorAlphaShift;
        public static readonly int PlatformColorBlueShift;
        public static readonly int PlatformColorGreenShift;
        public static readonly int PlatformColorRedShift;
        public static readonly SKColorType PlatformColorType;
        public SKImageInfo(int width, int height);
        public SKImageInfo(int width, int height, SKColorType colorType);
        public SKImageInfo(int width, int height, SKColorType colorType, SKAlphaType alphaType);
        public SKImageInfo(int width, int height, SKColorType colorType, SKAlphaType alphaType, SKColorSpace colorspace);
        public SKAlphaType AlphaType { get; set; }
        public int BitsPerPixel { get; }
        public int BytesPerPixel { get; }
        public int BytesSize { get; }
        public long BytesSize64 { get; }
        public SKColorSpace ColorSpace { get; set; }
        public SKColorType ColorType { get; set; }
        public int Height { get; set; }
        public bool IsEmpty { get; }
        public bool IsOpaque { get; }
        public SKRectI Rect { get; }
        public int RowBytes { get; }
        public long RowBytes64 { get; }
        public SKSizeI Size { get; }
        public int Width { get; set; }
        public SKImageInfo WithAlphaType(SKAlphaType newAlphaType);
        public SKImageInfo WithColorSpace(SKColorSpace newColorSpace);
        public SKImageInfo WithColorType(SKColorType newColorType);
        public SKImageInfo WithSize(int width, int height);
    }
    public delegate void SKImageRasterReleaseDelegate(IntPtr pixels, object context);
    public delegate void SKImageTextureReleaseDelegate(object context);
    public enum SKJpegEncoderAlphaOption
    {
        BlendOnBlack = 1,
        Ignore = 0,
    }
    public enum SKJpegEncoderDownsample
    {
        Downsample420 = 0,
        Downsample422 = 1,
        Downsample444 = 2,
    }
    public struct SKJpegEncoderOptions
    {
        public static readonly SKJpegEncoderOptions Default;
        public SKJpegEncoderOptions(int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption);
        public SKJpegEncoderOptions(int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption, SKTransferFunctionBehavior blendBehavior);
        public SKJpegEncoderAlphaOption AlphaOption { get; set; }
        public SKTransferFunctionBehavior BlendBehavior { get; set; }
        public SKJpegEncoderDownsample Downsample { get; set; }
        public int Quality { get; set; }
    }
    public struct SKLattice
    {
        public Nullable<SKRectI> Bounds { get; set; }
        public SKColor[] Colors { get; set; }
        public SKLatticeRectType[] RectTypes { get; set; }
        public int[] XDivs { get; set; }
        public int[] YDivs { get; set; }
    }
    public enum SKLatticeRectType
    {
        Default = 0,
        FixedColor = 2,
        Transparent = 1,
    }
    public abstract class SKManagedPixelSerializer : SKPixelSerializer
    {
        public SKManagedPixelSerializer();
    }
    public class SKManagedStream : SKAbstractManagedStream
    {
        public SKManagedStream(Stream managedStream);
        public SKManagedStream(Stream managedStream, bool disposeManagedStream);
        public int CopyTo(SKWStream destination);
        protected override void Dispose(bool disposing);
        protected override void DisposeManaged();
        protected override IntPtr OnCreateNew();
        protected override IntPtr OnDuplicate();
        protected override IntPtr OnFork();
        protected override IntPtr OnGetLength();
        protected override IntPtr OnGetPosition();
        protected override bool OnHasLength();
        protected override bool OnHasPosition();
        protected override bool OnIsAtEnd();
        protected override bool OnMove(int offset);
        protected override IntPtr OnPeek(IntPtr buffer, IntPtr size);
        protected override IntPtr OnRead(IntPtr buffer, IntPtr size);
        protected override bool OnRewind();
        protected override bool OnSeek(IntPtr position);
        public SKStreamAsset ToMemoryStream();
    }
    public class SKManagedWStream : SKAbstractManagedWStream
    {
        public SKManagedWStream(Stream managedStream);
        public SKManagedWStream(Stream managedStream, bool disposeManagedStream);
        protected override void Dispose(bool disposing);
        protected override void DisposeManaged();
        protected override IntPtr OnBytesWritten();
        protected override void OnFlush();
        protected override bool OnWrite(IntPtr buffer, IntPtr size);
    }
    public struct SKMask
    {
        public SKMask(SKRectI bounds, uint rowBytes, SKMaskFormat format);
        public SKMask(IntPtr image, SKRectI bounds, uint rowBytes, SKMaskFormat format);
        public SKRectI Bounds { get; set; }
        public SKMaskFormat Format { get; set; }
        public IntPtr Image { get; set; }
        public bool IsEmpty { get; }
        public uint RowBytes { get; set; }
        public long AllocateImage();
        public static IntPtr AllocateImage(long size);
        public long ComputeImageSize();
        public long ComputeTotalImageSize();
        public static SKMask Create(byte[] image, SKRectI bounds, uint rowBytes, SKMaskFormat format);
        public void FreeImage();
        public static void FreeImage(IntPtr image);
        public IntPtr GetAddr(int x, int y);
        public byte GetAddr1(int x, int y);
        public ushort GetAddr16(int x, int y);
        public uint GetAddr32(int x, int y);
        public byte GetAddr8(int x, int y);
    }
    public class SKMaskFilter : SKObject
    {
        public const int TableMaxLength = 256;
        public static float ConvertRadiusToSigma(float radius);
        public static float ConvertSigmaToRadius(float sigma);
        public static SKMaskFilter CreateBlur(SKBlurStyle blurStyle, float sigma);
        public static SKMaskFilter CreateBlur(SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags);
        public static SKMaskFilter CreateBlur(SKBlurStyle blurStyle, float sigma, SKRect occluder);
        public static SKMaskFilter CreateBlur(SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags);
        public static SKMaskFilter CreateBlur(SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM);
        public static SKMaskFilter CreateClip(byte min, byte max);
        public static SKMaskFilter CreateGamma(float gamma);
        public static SKMaskFilter CreateTable(byte[] table);
        protected override void Dispose(bool disposing);
    }
    public enum SKMaskFormat
    {
        A8 = 1,
        Argb32 = 3,
        BW = 0,
        Lcd16 = 4,
        ThreeD = 2,
    }
    public struct SKMatrix
    {
        public SKMatrix(float scaleX, float skewX, float transX, float skewY, float scaleY, float transY, float persp0, float persp1, float persp2);
        public float Persp0 { get; set; }
        public float Persp1 { get; set; }
        public float Persp2 { get; set; }
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float SkewX { get; set; }
        public float SkewY { get; set; }
        public float TransX { get; set; }
        public float TransY { get; set; }
        public float[] Values { get; set; }
        public static void Concat(ref SKMatrix target, SKMatrix first, SKMatrix second);
        public static void Concat(ref SKMatrix target, ref SKMatrix first, ref SKMatrix second);
        public void GetValues(float[] values);
        public static SKMatrix MakeIdentity();
        public static SKMatrix MakeRotation(float radians);
        public static SKMatrix MakeRotation(float radians, float pivotx, float pivoty);
        public static SKMatrix MakeRotationDegrees(float degrees);
        public static SKMatrix MakeRotationDegrees(float degrees, float pivotx, float pivoty);
        public static SKMatrix MakeScale(float sx, float sy);
        public static SKMatrix MakeScale(float sx, float sy, float pivotX, float pivotY);
        public static SKMatrix MakeSkew(float sx, float sy);
        public static SKMatrix MakeTranslation(float dx, float dy);
        public SKPoint MapPoint(SKPoint point);
        public SKPoint MapPoint(float x, float y);
        public SKPoint[] MapPoints(SKPoint[] points);
        public void MapPoints(SKPoint[] result, SKPoint[] points);
        public float MapRadius(float radius);
        public static void MapRect(ref SKMatrix matrix, out SKRect dest, ref SKRect source);
        public SKRect MapRect(SKRect source);
        public SKPoint MapVector(float x, float y);
        public SKPoint[] MapVectors(SKPoint[] vectors);
        public void MapVectors(SKPoint[] result, SKPoint[] vectors);
        public static void PostConcat(ref SKMatrix target, SKMatrix matrix);
        public static void PostConcat(ref SKMatrix target, ref SKMatrix matrix);
        public static void PreConcat(ref SKMatrix target, SKMatrix matrix);
        public static void PreConcat(ref SKMatrix target, ref SKMatrix matrix);
        public static void Rotate(ref SKMatrix matrix, float radians);
        public static void Rotate(ref SKMatrix matrix, float radians, float pivotx, float pivoty);
        public static void RotateDegrees(ref SKMatrix matrix, float degrees);
        public static void RotateDegrees(ref SKMatrix matrix, float degrees, float pivotx, float pivoty);
        public void SetScaleTranslate(float sx, float sy, float tx, float ty);
        public bool TryInvert(out SKMatrix inverse);
    }
    public class SKMatrix44 : SKObject
    {
        public SKMatrix44();
        public SKMatrix44(SKMatrix src);
        public SKMatrix44(SKMatrix44 src);
        public SKMatrix44(SKMatrix44 a, SKMatrix44 b);
        public float this[int row, int column] { get; set; }
        public SKMatrix Matrix { get; }
        public SKMatrix44TypeMask Type { get; }
        public static SKMatrix44 CreateIdentity();
        public static SKMatrix44 CreateRotation(float x, float y, float z, float radians);
        public static SKMatrix44 CreateRotationDegrees(float x, float y, float z, float degrees);
        public static SKMatrix44 CreateScale(float x, float y, float z);
        public static SKMatrix44 CreateTranslate(float x, float y, float z);
        public double Determinant();
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public static bool Equal(SKMatrix44 left, SKMatrix44 right);
        public static SKMatrix44 FromColumnMajor(float[] src);
        public static SKMatrix44 FromRowMajor(float[] src);
        public SKMatrix44 Invert();
        public bool Invert(SKMatrix44 inverse);
        public SKPoint MapPoint(SKPoint src);
        public SKPoint[] MapPoints(SKPoint[] src);
        public float[] MapScalars(float x, float y, float z, float w);
        public float[] MapScalars(float[] srcVector4);
        public void MapScalars(float[] srcVector4, float[] dstVector4);
        public float[] MapVector2(float[] src2);
        public void MapVector2(float[] src2, float[] dst4);
        public void PostConcat(SKMatrix44 m);
        public void PostScale(float sx, float sy, float sz);
        public void PostTranslate(float dx, float dy, float dz);
        public void PreConcat(SKMatrix44 m);
        public void PreScale(float sx, float sy, float sz);
        public bool Preserves2DAxisAlignment(float epsilon);
        public void PreTranslate(float dx, float dy, float dz);
        public void SetColumnMajor(float[] src);
        public void SetConcat(SKMatrix44 a, SKMatrix44 b);
        public void SetIdentity();
        public void SetRotationAbout(float x, float y, float z, float radians);
        public void SetRotationAboutDegrees(float x, float y, float z, float degrees);
        public void SetRotationAboutUnit(float x, float y, float z, float radians);
        public void SetRowMajor(float[] src);
        public void SetScale(float sx, float sy, float sz);
        public void SetTranslate(float dx, float dy, float dz);
        public float[] ToColumnMajor();
        public void ToColumnMajor(float[] dst);
        public float[] ToRowMajor();
        public void ToRowMajor(float[] dst);
        public void Transpose();
    }
    public enum SKMatrix44TypeMask
    {
        Affine = 4,
        Identity = 0,
        Perspective = 8,
        Scale = 2,
        Translate = 1,
    }
    public enum SKMatrixConvolutionTileMode
    {
        Clamp = 0,
        ClampToBlack = 2,
        Repeat = 1,
    }
    public class SKMemoryStream : SKStreamMemory
    {
        public SKMemoryStream();
        public SKMemoryStream(SKData data);
        public SKMemoryStream(byte[] data);
        public SKMemoryStream(ulong length);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public void SetMemory(byte[] data);
    }
    public enum SKNamedGamma
    {
        Linear = 0,
        NonStandard = 3,
        Srgb = 1,
        TwoDotTwoCurve = 2,
    }
    public abstract class SKNativeObject : IDisposable
    {
        public virtual IntPtr Handle { get; protected set; }
        protected internal bool IgnorePublicDispose { get; protected set; }
        protected internal bool IsDisposed { get; }
        protected internal virtual bool OwnsHandle { get; protected set; }
        public void Dispose();
        protected virtual void Dispose(bool disposing);
        protected void DisposeInternal();
        protected virtual void DisposeManaged();
        protected virtual void DisposeNative();
        ~SKNativeObject();
    }
    public class SKNoDrawCanvas : SKCanvas
    {
        public SKNoDrawCanvas(int width, int height);
    }
    public class SKNWayCanvas : SKNoDrawCanvas
    {
        public SKNWayCanvas(int width, int height);
        public void AddCanvas(SKCanvas canvas);
        public void RemoveAll();
        public void RemoveCanvas(SKCanvas canvas);
    }
    public abstract class SKObject : SKNativeObject
    {
        public override IntPtr Handle { get; protected set; }
        protected override void Dispose(bool disposing);
        protected override void DisposeManaged();
        protected override void DisposeNative();
    }
    public class SKOverdrawCanvas : SKNWayCanvas
    {
        public SKOverdrawCanvas(SKCanvas canvas);
    }
    public class SKPaint : SKObject
    {
        public SKPaint();
        public SKBlendMode BlendMode { get; set; }
        public SKColor Color { get; set; }
        public SKColorFilter ColorFilter { get; set; }
        public bool DeviceKerningEnabled { get; set; }
        public bool FakeBoldText { get; set; }
        public SKFilterQuality FilterQuality { get; set; }
        public SKFontMetrics FontMetrics { get; }
        public float FontSpacing { get; }
        public SKPaintHinting HintingLevel { get; set; }
        public SKImageFilter ImageFilter { get; set; }
        public bool IsAntialias { get; set; }
        public bool IsAutohinted { get; set; }
        public bool IsDither { get; set; }
        public bool IsEmbeddedBitmapText { get; set; }
        public bool IsLinearText { get; set; }
        public bool IsStroke { get; set; }
        public bool IsVerticalText { get; set; }
        public bool LcdRenderText { get; set; }
        public SKMaskFilter MaskFilter { get; set; }
        public SKPathEffect PathEffect { get; set; }
        public SKShader Shader { get; set; }
        public SKStrokeCap StrokeCap { get; set; }
        public SKStrokeJoin StrokeJoin { get; set; }
        public float StrokeMiter { get; set; }
        public float StrokeWidth { get; set; }
        public SKPaintStyle Style { get; set; }
        public bool SubpixelText { get; set; }
        public SKTextAlign TextAlign { get; set; }
        public SKTextEncoding TextEncoding { get; set; }
        public float TextScaleX { get; set; }
        public float TextSize { get; set; }
        public float TextSkewX { get; set; }
        public SKTypeface Typeface { get; set; }
        public long BreakText(byte[] text, float maxWidth);
        public long BreakText(byte[] text, float maxWidth, out float measuredWidth);
        public long BreakText(IntPtr buffer, int length, float maxWidth);
        public long BreakText(IntPtr buffer, int length, float maxWidth, out float measuredWidth);
        public long BreakText(IntPtr buffer, IntPtr length, float maxWidth);
        public long BreakText(IntPtr buffer, IntPtr length, float maxWidth, out float measuredWidth);
        public long BreakText(string text, float maxWidth);
        public long BreakText(string text, float maxWidth, out float measuredWidth);
        public long BreakText(string text, float maxWidth, out float measuredWidth, out string measuredText);
        public SKPaint Clone();
        public bool ContainsGlyphs(byte[] text);
        public bool ContainsGlyphs(IntPtr text, int length);
        public bool ContainsGlyphs(IntPtr text, IntPtr length);
        public bool ContainsGlyphs(string text);
        public int CountGlyphs(byte[] text);
        public int CountGlyphs(IntPtr text, int length);
        public int CountGlyphs(IntPtr text, IntPtr length);
        public int CountGlyphs(string text);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public SKPath GetFillPath(SKPath src);
        public bool GetFillPath(SKPath src, SKPath dst);
        public bool GetFillPath(SKPath src, SKPath dst, SKRect cullRect);
        public bool GetFillPath(SKPath src, SKPath dst, SKRect cullRect, float resScale);
        public bool GetFillPath(SKPath src, SKPath dst, float resScale);
        public SKPath GetFillPath(SKPath src, SKRect cullRect);
        public SKPath GetFillPath(SKPath src, SKRect cullRect, float resScale);
        public SKPath GetFillPath(SKPath src, float resScale);
        public float GetFontMetrics(out SKFontMetrics metrics, float scale = 0f);
        public ushort[] GetGlyphs(byte[] text);
        public ushort[] GetGlyphs(IntPtr text, int length);
        public ushort[] GetGlyphs(IntPtr text, IntPtr length);
        public ushort[] GetGlyphs(string text);
        public float[] GetGlyphWidths(byte[] text);
        public float[] GetGlyphWidths(byte[] text, out SKRect[] bounds);
        public float[] GetGlyphWidths(IntPtr text, int length);
        public float[] GetGlyphWidths(IntPtr text, int length, out SKRect[] bounds);
        public float[] GetGlyphWidths(IntPtr text, IntPtr length);
        public float[] GetGlyphWidths(IntPtr text, IntPtr length, out SKRect[] bounds);
        public float[] GetGlyphWidths(string text);
        public float[] GetGlyphWidths(string text, out SKRect[] bounds);
        public float[] GetHorizontalTextIntercepts(byte[] text, float[] xpositions, float y, float upperBounds, float lowerBounds);
        public float[] GetHorizontalTextIntercepts(IntPtr text, int length, float[] xpositions, float y, float upperBounds, float lowerBounds);
        public float[] GetHorizontalTextIntercepts(IntPtr text, IntPtr length, float[] xpositions, float y, float upperBounds, float lowerBounds);
        public float[] GetHorizontalTextIntercepts(string text, float[] xpositions, float y, float upperBounds, float lowerBounds);
        public float[] GetPositionedTextIntercepts(byte[] text, SKPoint[] positions, float upperBounds, float lowerBounds);
        public float[] GetPositionedTextIntercepts(IntPtr text, int length, SKPoint[] positions, float upperBounds, float lowerBounds);
        public float[] GetPositionedTextIntercepts(IntPtr text, IntPtr length, SKPoint[] positions, float upperBounds, float lowerBounds);
        public float[] GetPositionedTextIntercepts(string text, SKPoint[] positions, float upperBounds, float lowerBounds);
        public float[] GetTextIntercepts(SKTextBlob text, float upperBounds, float lowerBounds);
        public float[] GetTextIntercepts(byte[] text, float x, float y, float upperBounds, float lowerBounds);
        public float[] GetTextIntercepts(IntPtr text, int length, float x, float y, float upperBounds, float lowerBounds);
        public float[] GetTextIntercepts(IntPtr text, IntPtr length, float x, float y, float upperBounds, float lowerBounds);
        public float[] GetTextIntercepts(string text, float x, float y, float upperBounds, float lowerBounds);
        public SKPath GetTextPath(byte[] text, SKPoint[] points);
        public SKPath GetTextPath(byte[] text, float x, float y);
        public SKPath GetTextPath(IntPtr buffer, int length, SKPoint[] points);
        public SKPath GetTextPath(IntPtr buffer, int length, float x, float y);
        public SKPath GetTextPath(IntPtr buffer, IntPtr length, SKPoint[] points);
        public SKPath GetTextPath(IntPtr buffer, IntPtr length, float x, float y);
        public SKPath GetTextPath(string text, SKPoint[] points);
        public SKPath GetTextPath(string text, float x, float y);
        public float MeasureText(byte[] text);
        public float MeasureText(byte[] text, ref SKRect bounds);
        public float MeasureText(IntPtr buffer, int length);
        public float MeasureText(IntPtr buffer, int length, ref SKRect bounds);
        public float MeasureText(IntPtr buffer, IntPtr length);
        public float MeasureText(IntPtr buffer, IntPtr length, ref SKRect bounds);
        public float MeasureText(string text);
        public float MeasureText(string text, ref SKRect bounds);
        public void Reset();
    }
    public enum SKPaintHinting
    {
        Full = 3,
        NoHinting = 0,
        Normal = 2,
        Slight = 1,
    }
    public enum SKPaintStyle
    {
        Fill = 0,
        Stroke = 1,
        StrokeAndFill = 2,
    }
    public class SKPath : SKObject
    {
        public SKPath();
        public SKPath(SKPath path);
        public SKRect Bounds { get; }
        public SKPathConvexity Convexity { get; set; }
        public SKPathFillType FillType { get; set; }
        public bool IsConcave { get; }
        public bool IsConvex { get; }
        public bool IsEmpty { get; }
        public bool IsLine { get; }
        public bool IsOval { get; }
        public bool IsRect { get; }
        public bool IsRoundRect { get; }
        public SKPoint this[int index] { get; }
        public SKPoint LastPoint { get; }
        public int PointCount { get; }
        public SKPoint[] Points { get; }
        public SKPathSegmentMask SegmentMasks { get; }
        public SKRect TightBounds { get; }
        public int VerbCount { get; }
        public void AddArc(SKRect oval, float startAngle, float sweepAngle);
        public void AddCircle(float x, float y, float radius, SKPathDirection dir = SKPathDirection.Clockwise);
        public void AddOval(SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise);
        public void AddPath(SKPath other, ref SKMatrix matrix, SKPathAddMode mode = SKPathAddMode.Append);
        public void AddPath(SKPath other, SKPathAddMode mode = SKPathAddMode.Append);
        public void AddPath(SKPath other, float dx, float dy, SKPathAddMode mode = SKPathAddMode.Append);
        public void AddPathReverse(SKPath other);
        public void AddPoly(SKPoint[] points, bool close = true);
        public void AddRect(SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise);
        public void AddRect(SKRect rect, SKPathDirection direction, uint startIndex);
        public void AddRoundedRect(SKRect rect, float rx, float ry, SKPathDirection dir = SKPathDirection.Clockwise);
        public void AddRoundRect(SKRect rect, float rx, float ry, SKPathDirection dir = SKPathDirection.Clockwise);
        public void AddRoundRect(SKRoundRect rect, SKPathDirection direction = SKPathDirection.Clockwise);
        public void AddRoundRect(SKRoundRect rect, SKPathDirection direction, uint startIndex);
        public void ArcTo(SKPoint point1, SKPoint point2, float radius);
        public void ArcTo(SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy);
        public void ArcTo(SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo);
        public void ArcTo(float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);
        public void ArcTo(float x1, float y1, float x2, float y2, float radius);
        public void Close();
        public SKRect ComputeTightBounds();
        public void ConicTo(SKPoint point0, SKPoint point1, float w);
        public void ConicTo(float x0, float y0, float x1, float y1, float w);
        public bool Contains(float x, float y);
        public static int ConvertConicToQuads(SKPoint p0, SKPoint p1, SKPoint p2, float w, SKPoint[] pts, int pow2);
        public static int ConvertConicToQuads(SKPoint p0, SKPoint p1, SKPoint p2, float w, out SKPoint[] pts, int pow2);
        public static SKPoint[] ConvertConicToQuads(SKPoint p0, SKPoint p1, SKPoint p2, float w, int pow2);
        public SKPath.Iterator CreateIterator(bool forceClose);
        public SKPath.RawIterator CreateRawIterator();
        public void CubicTo(SKPoint point0, SKPoint point1, SKPoint point2);
        public void CubicTo(float x0, float y0, float x1, float y1, float x2, float y2);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public bool GetBounds(out SKRect rect);
        public SKPoint[] GetLine();
        public SKRect GetOvalBounds();
        public SKPoint GetPoint(int index);
        public int GetPoints(SKPoint[] points, int max);
        public SKPoint[] GetPoints(int max);
        public SKRect GetRect();
        public SKRect GetRect(out bool isClosed, out SKPathDirection direction);
        public SKRoundRect GetRoundRect();
        public bool GetTightBounds(out SKRect result);
        public void LineTo(SKPoint point);
        public void LineTo(float x, float y);
        public void MoveTo(SKPoint point);
        public void MoveTo(float x, float y);
        public void Offset(SKPoint offset);
        public void Offset(float dx, float dy);
        public SKPath Op(SKPath other, SKPathOp op);
        public bool Op(SKPath other, SKPathOp op, SKPath result);
        public static SKPath ParseSvgPathData(string svgPath);
        public void QuadTo(SKPoint point0, SKPoint point1);
        public void QuadTo(float x0, float y0, float x1, float y1);
        public void RArcTo(SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy);
        public void RArcTo(float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);
        public void RConicTo(SKPoint point0, SKPoint point1, float w);
        public void RConicTo(float dx0, float dy0, float dx1, float dy1, float w);
        public void RCubicTo(SKPoint point0, SKPoint point1, SKPoint point2);
        public void RCubicTo(float dx0, float dy0, float dx1, float dy1, float dx2, float dy2);
        public void Reset();
        public void Rewind();
        public void RLineTo(SKPoint point);
        public void RLineTo(float dx, float dy);
        public void RMoveTo(SKPoint point);
        public void RMoveTo(float dx, float dy);
        public void RQuadTo(SKPoint point0, SKPoint point1);
        public void RQuadTo(float dx0, float dy0, float dx1, float dy1);
        public SKPath Simplify();
        public bool Simplify(SKPath result);
        public string ToSvgPathData();
        public void Transform(SKMatrix matrix);
        public class Iterator : SKObject
        {
            public float ConicWeight();
            protected override void Dispose(bool disposing);
            protected override void DisposeNative();
            public bool IsCloseContour();
            public bool IsCloseLine();
            public SKPathVerb Next(SKPoint[] points, bool doConsumeDegenerates = true, bool exact = false);
        }
        public class OpBuilder : SKObject
        {
            public OpBuilder();
            public void Add(SKPath path, SKPathOp op);
            protected override void Dispose(bool disposing);
            protected override void DisposeNative();
            public bool Resolve(SKPath result);
        }
        public class RawIterator : SKObject
        {
            public float ConicWeight();
            protected override void Dispose(bool disposing);
            protected override void DisposeNative();
            public SKPathVerb Next(SKPoint[] points);
            public SKPathVerb Peek();
        }
    }
    public enum SKPath1DPathEffectStyle
    {
        Morph = 2,
        Rotate = 1,
        Translate = 0,
    }
    public enum SKPathAddMode
    {
        Append = 0,
        Extend = 1,
    }
    public enum SKPathArcSize
    {
        Large = 1,
        Small = 0,
    }
    public enum SKPathConvexity
    {
        Concave = 2,
        Convex = 1,
        Unknown = 0,
    }
    public enum SKPathDirection
    {
        Clockwise = 0,
        CounterClockwise = 1,
    }
    public class SKPathEffect : SKObject
    {
        public static SKPathEffect Create1DPath(SKPath path, float advance, float phase, SKPath1DPathEffectStyle style);
        public static SKPathEffect Create2DLine(float width, SKMatrix matrix);
        public static SKPathEffect Create2DPath(SKMatrix matrix, SKPath path);
        public static SKPathEffect CreateCompose(SKPathEffect outer, SKPathEffect inner);
        public static SKPathEffect CreateCorner(float radius);
        public static SKPathEffect CreateDash(float[] intervals, float phase);
        public static SKPathEffect CreateDiscrete(float segLength, float deviation, uint seedAssist = (uint)0);
        public static SKPathEffect CreateSum(SKPathEffect first, SKPathEffect second);
        public static SKPathEffect CreateTrim(float start, float stop);
        public static SKPathEffect CreateTrim(float start, float stop, SKTrimPathEffectMode mode);
        protected override void Dispose(bool disposing);
    }
    public enum SKPathFillType
    {
        EvenOdd = 1,
        InverseEvenOdd = 3,
        InverseWinding = 2,
        Winding = 0,
    }
    public class SKPathMeasure : SKObject
    {
        public SKPathMeasure();
        public SKPathMeasure(SKPath path, bool forceClosed = false, float resScale = 1f);
        public bool IsClosed { get; }
        public float Length { get; }
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public bool GetMatrix(float distance, out SKMatrix matrix, SKPathMeasureMatrixFlags flags);
        public bool GetPosition(float distance, out SKPoint position);
        public bool GetPositionAndTangent(float distance, out SKPoint position, out SKPoint tangent);
        public bool GetSegment(float start, float stop, SKPath dst, bool startWithMoveTo);
        public bool GetTangent(float distance, out SKPoint tangent);
        public bool NextContour();
        public void SetPath(SKPath path, bool forceClosed);
    }
    public enum SKPathMeasureMatrixFlags
    {
        GetPosition = 1,
        GetPositionAndTangent = 3,
        GetTangent = 2,
    }
    public enum SKPathOp
    {
        Difference = 0,
        Intersect = 1,
        ReverseDifference = 4,
        Union = 2,
        Xor = 3,
    }
    public enum SKPathSegmentMask
    {
        Conic = 4,
        Cubic = 8,
        Line = 1,
        Quad = 2,
    }
    public enum SKPathVerb
    {
        Close = 5,
        Conic = 3,
        Cubic = 4,
        Done = 6,
        Line = 1,
        Move = 0,
        Quad = 2,
    }
    public class SKPicture : SKObject
    {
        public SKRect CullRect { get; }
        public uint UniqueId { get; }
        protected override void Dispose(bool disposing);
    }
    public class SKPictureRecorder : SKObject
    {
        public SKPictureRecorder();
        public SKCanvas RecordingCanvas { get; }
        public SKCanvas BeginRecording(SKRect cullRect);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public SKPicture EndRecording();
        public SKDrawable EndRecordingAsDrawable();
    }
    public enum SKPixelGeometry
    {
        BgrHorizontal = 2,
        BgrVertical = 4,
        RgbHorizontal = 1,
        RgbVertical = 3,
        Unknown = 0,
    }
    public abstract class SKPixelSerializer : SKObject
    {
        protected SKPixelSerializer();
        public static SKPixelSerializer Create(Func<SKPixmap, SKData> onEncode);
        public static SKPixelSerializer Create(Func<IntPtr, IntPtr, bool> onUseEncodedData, Func<SKPixmap, SKData> onEncode);
        public SKData Encode(SKPixmap pixmap);
        protected abstract SKData OnEncode(SKPixmap pixmap);
        protected abstract bool OnUseEncodedData(IntPtr data, IntPtr length);
        public bool UseEncodedData(IntPtr data, ulong length);
    }
    public class SKPixmap : SKObject
    {
        public SKPixmap();
        public SKPixmap(SKImageInfo info, IntPtr addr);
        public SKPixmap(SKImageInfo info, IntPtr addr, int rowBytes);
        public SKPixmap(SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable);
        public SKAlphaType AlphaType { get; }
        public int BytesPerPixel { get; }
        public int BytesSize { get; }
        public SKColorSpace ColorSpace { get; }
        public SKColorTable ColorTable { get; }
        public SKColorType ColorType { get; }
        public int Height { get; }
        public SKImageInfo Info { get; }
        public SKRectI Rect { get; }
        public int RowBytes { get; }
        public SKSizeI Size { get; }
        public int Width { get; }
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public SKData Encode(SKEncodedImageFormat encoder, int quality);
        public SKData Encode(SKJpegEncoderOptions options);
        public SKData Encode(SKPngEncoderOptions options);
        public SKData Encode(SKWebpEncoderOptions options);
        public static bool Encode(SKWStream dst, SKBitmap src, SKEncodedImageFormat format, int quality);
        public bool Encode(SKWStream dst, SKEncodedImageFormat encoder, int quality);
        public bool Encode(SKWStream dst, SKJpegEncoderOptions options);
        public static bool Encode(SKWStream dst, SKPixmap src, SKEncodedImageFormat encoder, int quality);
        public static bool Encode(SKWStream dst, SKPixmap src, SKJpegEncoderOptions options);
        public static bool Encode(SKWStream dst, SKPixmap src, SKPngEncoderOptions options);
        public static bool Encode(SKWStream dst, SKPixmap src, SKWebpEncoderOptions options);
        public bool Encode(SKWStream dst, SKPngEncoderOptions options);
        public bool Encode(SKWStream dst, SKWebpEncoderOptions options);
        public bool Erase(SKColor color);
        public bool Erase(SKColor color, SKRectI subset);
        public bool ExtractSubset(SKPixmap result, SKRectI subset);
        public SKPixmap ExtractSubset(SKRectI subset);
        public SKColor GetPixelColor(int x, int y);
        public IntPtr GetPixels();
        public IntPtr GetPixels(int x, int y);
        public ReadOnlySpan<byte> GetPixelSpan();
        public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes);
        public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY);
        public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY, SKTransferFunctionBehavior behavior);
        public bool ReadPixels(SKPixmap pixmap);
        public bool ReadPixels(SKPixmap pixmap, int srcX, int srcY);
        public void Reset();
        public void Reset(SKImageInfo info, IntPtr addr, int rowBytes);
        public void Reset(SKImageInfo info, IntPtr addr, int rowBytes, SKColorTable ctable);
        public static bool Resize(SKPixmap dst, SKPixmap src, SKBitmapResizeMethod method);
        public bool ScalePixels(SKPixmap destination, SKFilterQuality quality);
        public SKPixmap WithAlphaType(SKAlphaType newAlphaType);
        public SKPixmap WithColorSpace(SKColorSpace newColorSpace);
        public SKPixmap WithColorType(SKColorType newColorType);
    }
    public struct SKPMColor
    {
        public SKPMColor(uint value);
        public byte Alpha { get; }
        public byte Blue { get; }
        public byte Green { get; }
        public byte Red { get; }
        public override bool Equals(object other);
        public override int GetHashCode();
        public static bool operator ==(SKPMColor left, SKPMColor right);
        public static explicit operator SKPMColor (SKColor color);
        public static explicit operator SKColor (SKPMColor color);
        public static explicit operator uint (SKPMColor color);
        public static implicit operator SKPMColor (uint color);
        public static bool operator !=(SKPMColor left, SKPMColor right);
        public static SKPMColor PreMultiply(SKColor color);
        public static SKPMColor[] PreMultiply(SKColor[] colors);
        public override string ToString();
        public static SKColor UnPreMultiply(SKPMColor pmcolor);
        public static SKColor[] UnPreMultiply(SKPMColor[] pmcolors);
    }
    public enum SKPngEncoderFilterFlags
    {
        AllFilters = 248,
        Avg = 64,
        NoFilters = 0,
        None = 8,
        Paeth = 128,
        Sub = 16,
        Up = 32,
    }
    public struct SKPngEncoderOptions
    {
        public static readonly SKPngEncoderOptions Default;
        public SKPngEncoderOptions(SKPngEncoderFilterFlags filterFlags, int zLibLevel);
        public SKPngEncoderOptions(SKPngEncoderFilterFlags filterFlags, int zLibLevel, SKTransferFunctionBehavior unpremulBehavior);
        public SKPngEncoderFilterFlags FilterFlags { get; set; }
        public SKTransferFunctionBehavior UnpremulBehavior { get; set; }
        public int ZLibLevel { get; set; }
    }
    public struct SKPoint
    {
        public static readonly SKPoint Empty;
        public SKPoint(float x, float y);
        public bool IsEmpty { get; }
        public float Length { get; }
        public float LengthSquared { get; }
        public float X { get; set; }
        public float Y { get; set; }
        public static SKPoint Add(SKPoint pt, SKPoint sz);
        public static SKPoint Add(SKPoint pt, SKPointI sz);
        public static SKPoint Add(SKPoint pt, SKSize sz);
        public static SKPoint Add(SKPoint pt, SKSizeI sz);
        public static float Distance(SKPoint point, SKPoint other);
        public static float DistanceSquared(SKPoint point, SKPoint other);
        public override bool Equals(object obj);
        public override int GetHashCode();
        public static SKPoint Normalize(SKPoint point);
        public void Offset(SKPoint p);
        public void Offset(float dx, float dy);
        public static SKPoint operator +(SKPoint pt, SKPoint sz);
        public static SKPoint operator +(SKPoint pt, SKPointI sz);
        public static SKPoint operator +(SKPoint pt, SKSize sz);
        public static SKPoint operator +(SKPoint pt, SKSizeI sz);
        public static bool operator ==(SKPoint left, SKPoint right);
        public static bool operator !=(SKPoint left, SKPoint right);
        public static SKPoint operator -(SKPoint pt, SKPoint sz);
        public static SKPoint operator -(SKPoint pt, SKPointI sz);
        public static SKPoint operator -(SKPoint pt, SKSize sz);
        public static SKPoint operator -(SKPoint pt, SKSizeI sz);
        public static SKPoint Reflect(SKPoint point, SKPoint normal);
        public static SKPoint Subtract(SKPoint pt, SKPoint sz);
        public static SKPoint Subtract(SKPoint pt, SKPointI sz);
        public static SKPoint Subtract(SKPoint pt, SKSize sz);
        public static SKPoint Subtract(SKPoint pt, SKSizeI sz);
        public override string ToString();
    }
    public struct SKPoint3
    {
        public static readonly SKPoint3 Empty;
        public SKPoint3(float x, float y, float z);
        public bool IsEmpty { get; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public static SKPoint3 Add(SKPoint3 pt, SKPoint3 sz);
        public override bool Equals(object obj);
        public override int GetHashCode();
        public static SKPoint3 operator +(SKPoint3 pt, SKPoint3 sz);
        public static bool operator ==(SKPoint3 left, SKPoint3 right);
        public static bool operator !=(SKPoint3 left, SKPoint3 right);
        public static SKPoint3 operator -(SKPoint3 pt, SKPoint3 sz);
        public static SKPoint3 Subtract(SKPoint3 pt, SKPoint3 sz);
        public override string ToString();
    }
    public struct SKPointI
    {
        public static readonly SKPointI Empty;
        public SKPointI(SKSizeI sz);
        public SKPointI(int x, int y);
        public bool IsEmpty { get; }
        public int Length { get; }
        public int LengthSquared { get; }
        public int X { get; set; }
        public int Y { get; set; }
        public static SKPointI Add(SKPointI pt, SKPointI sz);
        public static SKPointI Add(SKPointI pt, SKSizeI sz);
        public static SKPointI Ceiling(SKPoint value);
        public static float Distance(SKPointI point, SKPointI other);
        public static float DistanceSquared(SKPointI point, SKPointI other);
        public override bool Equals(object obj);
        public override int GetHashCode();
        public static SKPointI Normalize(SKPointI point);
        public void Offset(SKPointI p);
        public void Offset(int dx, int dy);
        public static SKPointI operator +(SKPointI pt, SKPointI sz);
        public static SKPointI operator +(SKPointI pt, SKSizeI sz);
        public static bool operator ==(SKPointI left, SKPointI right);
        public static explicit operator SKSizeI (SKPointI p);
        public static implicit operator SKPoint (SKPointI p);
        public static bool operator !=(SKPointI left, SKPointI right);
        public static SKPointI operator -(SKPointI pt, SKPointI sz);
        public static SKPointI operator -(SKPointI pt, SKSizeI sz);
        public static SKPointI Reflect(SKPointI point, SKPointI normal);
        public static SKPointI Round(SKPoint value);
        public static SKPointI Subtract(SKPointI pt, SKPointI sz);
        public static SKPointI Subtract(SKPointI pt, SKSizeI sz);
        public override string ToString();
        public static SKPointI Truncate(SKPoint value);
    }
    public enum SKPointMode
    {
        Lines = 1,
        Points = 0,
        Polygon = 2,
    }
    public sealed class SKPositionedRunBuffer : SKRunBuffer
    {
        public Span<SKPoint> GetPositionSpan();
        public void SetPositions(ReadOnlySpan<SKPoint> positions);
    }
    public struct SKRect
    {
        public static readonly SKRect Empty;
        public SKRect(float left, float top, float right, float bottom);
        public float Bottom { get; set; }
        public float Height { get; }
        public bool IsEmpty { get; }
        public float Left { get; set; }
        public SKPoint Location { get; set; }
        public float MidX { get; }
        public float MidY { get; }
        public float Right { get; set; }
        public SKSize Size { get; set; }
        public SKRect Standardized { get; }
        public float Top { get; set; }
        public float Width { get; }
        public SKRect AspectFill(SKSize size);
        public SKRect AspectFit(SKSize size);
        public bool Contains(SKPoint pt);
        public bool Contains(SKRect rect);
        public bool Contains(float x, float y);
        public static SKRect Create(SKPoint location, SKSize size);
        public static SKRect Create(SKSize size);
        public static SKRect Create(float width, float height);
        public static SKRect Create(float x, float y, float width, float height);
        public override bool Equals(object obj);
        public override int GetHashCode();
        public static SKRect Inflate(SKRect rect, float x, float y);
        public void Inflate(SKSize size);
        public void Inflate(float x, float y);
        public void Intersect(SKRect rect);
        public static SKRect Intersect(SKRect a, SKRect b);
        public bool IntersectsWith(SKRect rect);
        public bool IntersectsWithInclusive(SKRect rect);
        public void Offset(SKPoint pos);
        public void Offset(float x, float y);
        public static bool operator ==(SKRect left, SKRect right);
        public static implicit operator SKRect (SKRectI r);
        public static bool operator !=(SKRect left, SKRect right);
        public override string ToString();
        public void Union(SKRect rect);
        public static SKRect Union(SKRect a, SKRect b);
    }
    public struct SKRectI
    {
        public static readonly SKRectI Empty;
        public SKRectI(int left, int top, int right, int bottom);
        public int Bottom { get; set; }
        public int Height { get; }
        public bool IsEmpty { get; }
        public int Left { get; set; }
        public SKPointI Location { get; set; }
        public int MidX { get; }
        public int MidY { get; }
        public int Right { get; set; }
        public SKSizeI Size { get; set; }
        public SKRectI Standardized { get; }
        public int Top { get; set; }
        public int Width { get; }
        public SKRectI AspectFill(SKSizeI size);
        public SKRectI AspectFit(SKSizeI size);
        public static SKRectI Ceiling(SKRect value);
        public static SKRectI Ceiling(SKRect value, bool outwards);
        public bool Contains(SKPointI pt);
        public bool Contains(SKRectI rect);
        public bool Contains(int x, int y);
        public static SKRectI Create(SKPointI location, SKSizeI size);
        public static SKRectI Create(SKSizeI size);
        public static SKRectI Create(int width, int height);
        public static SKRectI Create(int x, int y, int width, int height);
        public override bool Equals(object obj);
        public static SKRectI Floor(SKRect value);
        public static SKRectI Floor(SKRect value, bool inwards);
        public override int GetHashCode();
        public static SKRectI Inflate(SKRectI rect, int x, int y);
        public void Inflate(SKSizeI size);
        public void Inflate(int width, int height);
        public void Intersect(SKRectI rect);
        public static SKRectI Intersect(SKRectI a, SKRectI b);
        public bool IntersectsWith(SKRectI rect);
        public bool IntersectsWithInclusive(SKRectI rect);
        public void Offset(SKPointI pos);
        public void Offset(int x, int y);
        public static bool operator ==(SKRectI left, SKRectI right);
        public static bool operator !=(SKRectI left, SKRectI right);
        public static SKRectI Round(SKRect value);
        public override string ToString();
        public static SKRectI Truncate(SKRect value);
        public void Union(SKRectI rect);
        public static SKRectI Union(SKRectI a, SKRectI b);
    }
    public class SKRegion : SKObject
    {
        public SKRegion();
        public SKRegion(SKPath path);
        public SKRegion(SKRectI rect);
        public SKRegion(SKRegion region);
        public SKRectI Bounds { get; }
        public bool Contains(SKPointI xy);
        public bool Contains(SKRegion src);
        public bool Contains(int x, int y);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public bool Intersects(SKPath path);
        public bool Intersects(SKRectI rect);
        public bool Intersects(SKRegion region);
        public bool Op(SKPath path, SKRegionOperation op);
        public bool Op(SKRectI rect, SKRegionOperation op);
        public bool Op(SKRegion region, SKRegionOperation op);
        public bool Op(int left, int top, int right, int bottom, SKRegionOperation op);
        public bool SetPath(SKPath path);
        public bool SetPath(SKPath path, SKRegion clip);
        public bool SetRect(SKRectI rect);
        public bool SetRegion(SKRegion region);
    }
    public enum SKRegionOperation
    {
        Difference = 0,
        Intersect = 1,
        Replace = 5,
        ReverseDifference = 4,
        Union = 2,
        XOR = 3,
    }
    public struct SKRotationScaleMatrix
    {
        public static readonly SKRotationScaleMatrix Empty;
        public SKRotationScaleMatrix(float scos, float ssin, float tx, float ty);
        public float SCos { get; set; }
        public float SSin { get; set; }
        public float TX { get; set; }
        public float TY { get; set; }
        public static SKRotationScaleMatrix CreateIdentity();
        public static SKRotationScaleMatrix CreateRotation(float radians, float anchorX, float anchorY);
        public static SKRotationScaleMatrix CreateRotationDegrees(float degrees, float anchorX, float anchorY);
        public static SKRotationScaleMatrix CreateScale(float s);
        public static SKRotationScaleMatrix CreateTranslate(float x, float y);
        public static SKRotationScaleMatrix FromDegrees(float scale, float degrees, float tx, float ty, float anchorX, float anchorY);
        public static SKRotationScaleMatrix FromRadians(float scale, float radians, float tx, float ty, float anchorX, float anchorY);
        public SKMatrix ToMatrix();
    }
    public class SKRoundRect : SKObject
    {
        public SKRoundRect();
        public SKRoundRect(SKRect rect);
        public SKRoundRect(SKRect rect, float radius);
        public SKRoundRect(SKRect rect, float xRadius, float yRadius);
        public SKRoundRect(SKRoundRect rrect);
        public bool AllCornersCircular { get; }
        public float Height { get; }
        public bool IsValid { get; }
        public SKPoint[] Radii { get; }
        public SKRect Rect { get; }
        public SKRoundRectType Type { get; }
        public float Width { get; }
        public bool CheckAllCornersCircular(float tolerance);
        public bool Contains(SKRect rect);
        public void Deflate(SKSize size);
        public void Deflate(float dx, float dy);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
        public SKPoint GetRadii(SKRoundRectCorner corner);
        public void Inflate(SKSize size);
        public void Inflate(float dx, float dy);
        public void Offset(SKPoint pos);
        public void Offset(float dx, float dy);
        public void SetEmpty();
        public void SetNinePatch(SKRect rect, float leftRadius, float topRadius, float rightRadius, float bottomRadius);
        public void SetOval(SKRect rect);
        public void SetRect(SKRect rect);
        public void SetRect(SKRect rect, float xRadius, float yRadius);
        public void SetRectRadii(SKRect rect, SKPoint[] radii);
        public SKRoundRect Transform(SKMatrix matrix);
        public bool TryTransform(SKMatrix matrix, out SKRoundRect transformed);
    }
    public enum SKRoundRectCorner
    {
        LowerLeft = 3,
        LowerRight = 2,
        UpperLeft = 0,
        UpperRight = 1,
    }
    public enum SKRoundRectType
    {
        Complex = 5,
        Empty = 0,
        NinePatch = 4,
        Oval = 2,
        Rect = 1,
        Simple = 3,
    }
    public class SKRunBuffer
    {
        public int Size { get; }
        public int TextSize { get; }
        public Span<uint> GetClusterSpan();
        public Span<ushort> GetGlyphSpan();
        public Span<byte> GetTextSpan();
        public void SetClusters(ReadOnlySpan<uint> clusters);
        public void SetGlyphs(ReadOnlySpan<ushort> glyphs);
        public void SetText(ReadOnlySpan<byte> text);
    }
    public class SKShader : SKObject
    {
        public static SKShader CreateBitmap(SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy);
        public static SKShader CreateBitmap(SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix);
        public static SKShader CreateColor(SKColor color);
        public static SKShader CreateColorFilter(SKShader shader, SKColorFilter filter);
        public static SKShader CreateCompose(SKShader shaderA, SKShader shaderB);
        public static SKShader CreateCompose(SKShader shaderA, SKShader shaderB, SKBlendMode mode);
        public static SKShader CreateEmpty();
        public static SKShader CreateLinearGradient(SKPoint start, SKPoint end, SKColor[] colors, SKShaderTileMode mode);
        public static SKShader CreateLinearGradient(SKPoint start, SKPoint end, SKColor[] colors, float[] colorPos, SKShaderTileMode mode);
        public static SKShader CreateLinearGradient(SKPoint start, SKPoint end, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix);
        public static SKShader CreateLocalMatrix(SKShader shader, SKMatrix localMatrix);
        public static SKShader CreatePerlinNoiseFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed);
        public static SKShader CreatePerlinNoiseFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize);
        public static SKShader CreatePerlinNoiseImprovedNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float z);
        public static SKShader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed);
        public static SKShader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize);
        public static SKShader CreatePicture(SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy);
        public static SKShader CreatePicture(SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix, SKRect tile);
        public static SKShader CreatePicture(SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKRect tile);
        public static SKShader CreateRadialGradient(SKPoint center, float radius, SKColor[] colors, SKShaderTileMode mode);
        public static SKShader CreateRadialGradient(SKPoint center, float radius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode);
        public static SKShader CreateRadialGradient(SKPoint center, float radius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix);
        public static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors);
        public static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors, SKShaderTileMode tileMode, float startAngle, float endAngle);
        public static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors, float[] colorPos);
        public static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors, float[] colorPos, SKMatrix localMatrix);
        public static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle);
        public static SKShader CreateSweepGradient(SKPoint center, SKColor[] colors, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix localMatrix);
        public static SKShader CreateTwoPointConicalGradient(SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, SKShaderTileMode mode);
        public static SKShader CreateTwoPointConicalGradient(SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode);
        public static SKShader CreateTwoPointConicalGradient(SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix);
        protected override void Dispose(bool disposing);
    }
    public enum SKShaderTileMode
    {
        Clamp = 0,
        Mirror = 2,
        Repeat = 1,
    }
    public struct SKSize
    {
        public static readonly SKSize Empty;
        public SKSize(SKPoint pt);
        public SKSize(float width, float height);
        public float Height { get; set; }
        public bool IsEmpty { get; }
        public float Width { get; set; }
        public static SKSize Add(SKSize sz1, SKSize sz2);
        public override bool Equals(object obj);
        public override int GetHashCode();
        public static SKSize operator +(SKSize sz1, SKSize sz2);
        public static bool operator ==(SKSize sz1, SKSize sz2);
        public static explicit operator SKPoint (SKSize size);
        public static implicit operator SKSize (SKSizeI size);
        public static bool operator !=(SKSize sz1, SKSize sz2);
        public static SKSize operator -(SKSize sz1, SKSize sz2);
        public static SKSize Subtract(SKSize sz1, SKSize sz2);
        public SKPoint ToPoint();
        public SKSizeI ToSizeI();
        public override string ToString();
    }
    public struct SKSizeI
    {
        public static readonly SKSizeI Empty;
        public SKSizeI(SKPointI pt);
        public SKSizeI(int width, int height);
        public int Height { get; set; }
        public bool IsEmpty { get; }
        public int Width { get; set; }
        public static SKSizeI Add(SKSizeI sz1, SKSizeI sz2);
        public override bool Equals(object obj);
        public override int GetHashCode();
        public static SKSizeI operator +(SKSizeI sz1, SKSizeI sz2);
        public static bool operator ==(SKSizeI sz1, SKSizeI sz2);
        public static explicit operator SKPointI (SKSizeI size);
        public static bool operator !=(SKSizeI sz1, SKSizeI sz2);
        public static SKSizeI operator -(SKSizeI sz1, SKSizeI sz2);
        public static SKSizeI Subtract(SKSizeI sz1, SKSizeI sz2);
        public SKPointI ToPointI();
        public override string ToString();
    }
    public abstract class SKStream : SKObject
    {
        public bool HasLength { get; }
        public bool HasPosition { get; }
        public bool IsAtEnd { get; }
        public int Length { get; }
        public int Position { get; set; }
        public IntPtr GetMemoryBase();
        public bool Move(int offset);
        public bool Move(long offset);
        public int Peek(IntPtr buffer, int size);
        public int Read(byte[] buffer, int size);
        public int Read(IntPtr buffer, int size);
        public bool ReadBool();
        public bool ReadBool(out bool buffer);
        public byte ReadByte();
        public bool ReadByte(out byte buffer);
        public short ReadInt16();
        public bool ReadInt16(out short buffer);
        public int ReadInt32();
        public bool ReadInt32(out int buffer);
        public sbyte ReadSByte();
        public bool ReadSByte(out sbyte buffer);
        public ushort ReadUInt16();
        public bool ReadUInt16(out ushort buffer);
        public uint ReadUInt32();
        public bool ReadUInt32(out uint buffer);
        public bool Rewind();
        public bool Seek(int position);
        public int Skip(int size);
    }
    public abstract class SKStreamAsset : SKStreamSeekable
    {
    }
    public abstract class SKStreamMemory : SKStreamAsset
    {
    }
    public abstract class SKStreamRewindable : SKStream
    {
    }
    public abstract class SKStreamSeekable : SKStreamRewindable
    {
    }
    public enum SKStrokeCap
    {
        Butt = 0,
        Round = 1,
        Square = 2,
    }
    public enum SKStrokeJoin
    {
        Bevel = 2,
        Miter = 0,
        Round = 1,
    }
    public class SKSurface : SKObject
    {
        public SKCanvas Canvas { get; }
        public SKSurfaceProperties SurfaceProperties { get; }
        public SKSurfaceProps SurfaceProps { get; }
        public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType);
        public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace);
        public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
        public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);
        public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType);
        public static SKSurface Create(GRContext context, GRBackendRenderTarget renderTarget, SKColorType colorType, SKSurfaceProperties props);
        public static SKSurface Create(GRContext context, GRBackendRenderTargetDesc desc);
        public static SKSurface Create(GRContext context, GRBackendRenderTargetDesc desc, SKSurfaceProps props);
        public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
        public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);
        public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType);
        public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace);
        public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
        public static SKSurface Create(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props);
        public static SKSurface Create(GRContext context, GRBackendTexture texture, SKColorType colorType);
        public static SKSurface Create(GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props);
        public static SKSurface Create(GRContext context, GRBackendTextureDesc desc);
        public static SKSurface Create(GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);
        public static SKSurface Create(GRContext context, GRGlBackendTextureDesc desc);
        public static SKSurface Create(GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);
        public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info);
        public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, SKSurfaceProperties props);
        public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, int sampleCount);
        public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin);
        public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, int sampleCount, GRSurfaceOrigin origin, SKSurfaceProperties props, bool shouldCreateWithMips);
        public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProperties props);
        public static SKSurface Create(GRContext context, bool budgeted, SKImageInfo info, int sampleCount, SKSurfaceProps props);
        public static SKSurface Create(SKImageInfo info);
        public static SKSurface Create(SKImageInfo info, SKSurfaceProperties props);
        public static SKSurface Create(SKImageInfo info, SKSurfaceProps props);
        public static SKSurface Create(SKImageInfo info, int rowBytes);
        public static SKSurface Create(SKImageInfo info, int rowBytes, SKSurfaceProperties props);
        public static SKSurface Create(SKImageInfo info, IntPtr pixels);
        public static SKSurface Create(SKImageInfo info, IntPtr pixels, SKSurfaceProperties props);
        public static SKSurface Create(SKImageInfo info, IntPtr pixels, int rowBytes);
        public static SKSurface Create(SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProperties props);
        public static SKSurface Create(SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceProps props);
        public static SKSurface Create(SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context);
        public static SKSurface Create(SKImageInfo info, IntPtr pixels, int rowBytes, SKSurfaceReleaseDelegate releaseProc, object context, SKSurfaceProperties props);
        public static SKSurface Create(SKPixmap pixmap);
        public static SKSurface Create(SKPixmap pixmap, SKSurfaceProperties props);
        public static SKSurface Create(SKPixmap pixmap, SKSurfaceProps props);
        public static SKSurface Create(int width, int height, SKColorType colorType, SKAlphaType alphaType);
        public static SKSurface Create(int width, int height, SKColorType colorType, SKAlphaType alphaType, SKSurfaceProps props);
        public static SKSurface Create(int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes);
        public static SKSurface Create(int width, int height, SKColorType colorType, SKAlphaType alphaType, IntPtr pixels, int rowBytes, SKSurfaceProps props);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, SKColorType colorType, SKSurfaceProperties props);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKColorSpace colorspace, SKSurfaceProperties props);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, GRSurfaceOrigin origin, int sampleCount, SKColorType colorType, SKSurfaceProperties props);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, SKColorType colorType);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTexture texture, SKColorType colorType, SKSurfaceProperties props);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTextureDesc desc);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRBackendTextureDesc desc, SKSurfaceProps props);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRGlBackendTextureDesc desc);
        public static SKSurface CreateAsRenderTarget(GRContext context, GRGlBackendTextureDesc desc, SKSurfaceProps props);
        public static SKSurface CreateNull(int width, int height);
        protected override void Dispose(bool disposing);
        public void Draw(SKCanvas canvas, float x, float y, SKPaint paint);
        public SKPixmap PeekPixels();
        public bool PeekPixels(SKPixmap pixmap);
        public bool ReadPixels(SKImageInfo dstInfo, IntPtr dstPixels, int dstRowBytes, int srcX, int srcY);
        public SKImage Snapshot();
    }
    public class SKSurfaceProperties : SKObject
    {
        public SKSurfaceProperties(SKPixelGeometry pixelGeometry);
        public SKSurfaceProperties(SKSurfaceProps props);
        public SKSurfaceProperties(SKSurfacePropsFlags flags, SKPixelGeometry pixelGeometry);
        public SKSurfaceProperties(uint flags, SKPixelGeometry pixelGeometry);
        public SKSurfacePropsFlags Flags { get; }
        public bool IsUseDeviceIndependentFonts { get; }
        public SKPixelGeometry PixelGeometry { get; }
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
    }
    public struct SKSurfaceProps
    {
        public SKSurfacePropsFlags Flags { get; set; }
        public SKPixelGeometry PixelGeometry { get; set; }
    }
    public enum SKSurfacePropsFlags
    {
        None = 0,
        UseDeviceIndependentFonts = 1,
    }
    public delegate void SKSurfaceReleaseDelegate(IntPtr address, object context);
    public class SKSvgCanvas
    {
        public static SKCanvas Create(SKRect bounds, SKXmlWriter writer);
    }
    public static class SKSwizzle
    {
        public static void SwapRedBlue(IntPtr pixels, int count);
        public static void SwapRedBlue(IntPtr dest, IntPtr src, int count);
        public static void SwapRedBlue(ReadOnlySpan<byte> pixels, int count);
        public static void SwapRedBlue(ReadOnlySpan<byte> dest, ReadOnlySpan<byte> src, int count);
    }
    public enum SKTextAlign
    {
        Center = 1,
        Left = 0,
        Right = 2,
    }
    public class SKTextBlob : SKObject
    {
        public SKRect Bounds { get; }
        public uint UniqueId { get; }
        protected override void Dispose(bool disposing);
    }
    public class SKTextBlobBuilder : SKObject
    {
        public SKTextBlobBuilder();
        public void AddHorizontalRun(SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions);
        public void AddHorizontalRun(SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions, Nullable<SKRect> bounds);
        public void AddHorizontalRun(SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters);
        public void AddHorizontalRun(SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters, Nullable<SKRect> bounds);
        public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions);
        public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions, SKRect bounds);
        public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters);
        public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters, SKRect bounds);
        public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters);
        public void AddHorizontalRun(SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters, SKRect bounds);
        public void AddPositionedRun(SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions);
        public void AddPositionedRun(SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions, Nullable<SKRect> bounds);
        public void AddPositionedRun(SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters);
        public void AddPositionedRun(SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters, Nullable<SKRect> bounds);
        public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions);
        public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions, SKRect bounds);
        public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters);
        public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters, SKRect bounds);
        public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters);
        public void AddPositionedRun(SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters, SKRect bounds);
        public void AddRun(SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs);
        public void AddRun(SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs, Nullable<SKRect> bounds);
        public void AddRun(SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters);
        public void AddRun(SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters, Nullable<SKRect> bounds);
        public void AddRun(SKPaint font, float x, float y, ushort[] glyphs);
        public void AddRun(SKPaint font, float x, float y, ushort[] glyphs, SKRect bounds);
        public void AddRun(SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters);
        public void AddRun(SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters, SKRect bounds);
        public void AddRun(SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters);
        public void AddRun(SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters, SKRect bounds);
        public SKHorizontalRunBuffer AllocateHorizontalRun(SKPaint font, int count, float y);
        public SKHorizontalRunBuffer AllocateHorizontalRun(SKPaint font, int count, float y, int textByteCount);
        public SKHorizontalRunBuffer AllocateHorizontalRun(SKPaint font, int count, float y, int textByteCount, Nullable<SKRect> bounds);
        public SKHorizontalRunBuffer AllocateHorizontalRun(SKPaint font, int count, float y, Nullable<SKRect> bounds);
        public SKPositionedRunBuffer AllocatePositionedRun(SKPaint font, int count);
        public SKPositionedRunBuffer AllocatePositionedRun(SKPaint font, int count, int textByteCount);
        public SKPositionedRunBuffer AllocatePositionedRun(SKPaint font, int count, int textByteCount, Nullable<SKRect> bounds);
        public SKPositionedRunBuffer AllocatePositionedRun(SKPaint font, int count, Nullable<SKRect> bounds);
        public SKRunBuffer AllocateRun(SKPaint font, int count, float x, float y);
        public SKRunBuffer AllocateRun(SKPaint font, int count, float x, float y, int textByteCount);
        public SKRunBuffer AllocateRun(SKPaint font, int count, float x, float y, int textByteCount, Nullable<SKRect> bounds);
        public SKRunBuffer AllocateRun(SKPaint font, int count, float x, float y, Nullable<SKRect> bounds);
        public SKTextBlob Build();
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
    }
    public enum SKTextEncoding
    {
        GlyphId = 3,
        Utf16 = 1,
        Utf32 = 2,
        Utf8 = 0,
    }
    public enum SKTransferFunctionBehavior
    {
        Ignore = 1,
        Respect = 0,
    }
    public enum SKTrimPathEffectMode
    {
        Inverted = 1,
        Normal = 0,
    }
    public class SKTypeface : SKObject
    {
        public static SKTypeface Default { get; }
        public string FamilyName { get; }
        public SKFontStyleSlant FontSlant { get; }
        public SKFontStyle FontStyle { get; }
        public int FontWeight { get; }
        public int FontWidth { get; }
        public bool IsBold { get; }
        public bool IsFixedPitch { get; }
        public bool IsItalic { get; }
        public SKTypefaceStyle Style { get; }
        public int TableCount { get; }
        public int UnitsPerEm { get; }
        public int CharsToGlyphs(IntPtr str, int strlen, SKEncoding encoding, out ushort[] glyphs);
        public int CharsToGlyphs(string chars, out ushort[] glyphs);
        public int CountGlyphs(byte[] str, SKEncoding encoding);
        public int CountGlyphs(IntPtr str, int strLen, SKEncoding encoding);
        public int CountGlyphs(ReadOnlySpan<byte> str, SKEncoding encoding);
        public int CountGlyphs(string str);
        public int CountGlyphs(string str, SKEncoding encoding);
        public static SKTypeface CreateDefault();
        protected override void Dispose(bool disposing);
        public static SKTypeface FromData(SKData data, int index = 0);
        public static SKTypeface FromFamilyName(string familyName);
        public static SKTypeface FromFamilyName(string familyName, SKFontStyle style);
        public static SKTypeface FromFamilyName(string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant);
        public static SKTypeface FromFamilyName(string familyName, SKTypefaceStyle style);
        public static SKTypeface FromFamilyName(string familyName, int weight, int width, SKFontStyleSlant slant);
        public static SKTypeface FromFile(string path, int index = 0);
        public static SKTypeface FromStream(SKStreamAsset stream, int index = 0);
        public static SKTypeface FromStream(Stream stream, int index = 0);
        public static SKTypeface FromTypeface(SKTypeface typeface, SKTypefaceStyle style);
        public ushort[] GetGlyphs(byte[] text, SKEncoding encoding);
        public int GetGlyphs(byte[] text, SKEncoding encoding, out ushort[] glyphs);
        public ushort[] GetGlyphs(IntPtr text, int length, SKEncoding encoding);
        public int GetGlyphs(IntPtr text, int length, SKEncoding encoding, out ushort[] glyphs);
        public ushort[] GetGlyphs(ReadOnlySpan<byte> text, SKEncoding encoding);
        public int GetGlyphs(ReadOnlySpan<byte> text, SKEncoding encoding, out ushort[] glyphs);
        public ushort[] GetGlyphs(string text);
        public ushort[] GetGlyphs(string text, SKEncoding encoding);
        public int GetGlyphs(string text, SKEncoding encoding, out ushort[] glyphs);
        public int GetGlyphs(string text, out ushort[] glyphs);
        public byte[] GetTableData(uint tag);
        public int GetTableSize(uint tag);
        public uint[] GetTableTags();
        public SKStreamAsset OpenStream();
        public SKStreamAsset OpenStream(out int ttcIndex);
        public bool TryGetTableData(uint tag, out byte[] tableData);
        public bool TryGetTableData(uint tag, int offset, int length, IntPtr tableData);
        public bool TryGetTableTags(out uint[] tags);
    }
    public enum SKTypefaceStyle
    {
        Bold = 1,
        BoldItalic = 3,
        Italic = 2,
        Normal = 0,
    }
    public enum SKVertexMode
    {
        TriangleFan = 2,
        Triangles = 0,
        TriangleStrip = 1,
    }
    public class SKVertices : SKObject
    {
        public static SKVertices CreateCopy(SKVertexMode vmode, SKPoint[] positions, SKColor[] colors);
        public static SKVertices CreateCopy(SKVertexMode vmode, SKPoint[] positions, SKPoint[] texs, SKColor[] colors);
        public static SKVertices CreateCopy(SKVertexMode vmode, SKPoint[] positions, SKPoint[] texs, SKColor[] colors, ushort[] indices);
        protected override void Dispose(bool disposing);
    }
    public enum SKWebpEncoderCompression
    {
        Lossless = 1,
        Lossy = 0,
    }
    public struct SKWebpEncoderOptions
    {
        public static readonly SKWebpEncoderOptions Default;
        public SKWebpEncoderOptions(SKWebpEncoderCompression compression, float quality);
        public SKWebpEncoderOptions(SKWebpEncoderCompression compression, float quality, SKTransferFunctionBehavior unpremulBehavior);
        public SKWebpEncoderCompression Compression { get; set; }
        public float Quality { get; set; }
        public SKTransferFunctionBehavior UnpremulBehavior { get; set; }
    }
    public abstract class SKWStream : SKObject
    {
        public virtual int BytesWritten { get; }
        public virtual void Flush();
        public static int GetSizeOfPackedUInt32(uint value);
        public bool NewLine();
        public virtual bool Write(byte[] buffer, int size);
        public bool Write16(ushort value);
        public bool Write32(uint value);
        public bool Write8(byte value);
        public bool WriteBigDecimalAsText(long value, int digits);
        public bool WriteBool(bool value);
        public bool WriteDecimalAsTest(int value);
        public bool WriteHexAsText(uint value, int digits);
        public bool WritePackedUInt32(uint value);
        public bool WriteScalar(float value);
        public bool WriteScalarAsText(float value);
        public bool WriteStream(SKStream input, int length);
        public bool WriteText(string value);
    }
    public class SKXmlStreamWriter : SKXmlWriter
    {
        public SKXmlStreamWriter(SKWStream stream);
        protected override void Dispose(bool disposing);
        protected override void DisposeNative();
    }
    public abstract class SKXmlWriter : SKObject
    {
    }
    public enum SKZeroInitialized
    {
        No = 1,
        Yes = 0,
    }
    public static class StringUtilities
    {
        public static byte[] GetEncodedText(string text, SKEncoding encoding);
        public static byte[] GetEncodedText(string text, SKTextEncoding encoding);
        public static string GetString(byte[] data, SKTextEncoding encoding);
        public static string GetString(byte[] data, int index, int count, SKTextEncoding encoding);
        public static string GetString(IntPtr data, int dataLength, SKTextEncoding encoding);
        public static int GetUnicodeCharacterCode(string character, SKTextEncoding encoding);
    }
}
