namespace SkiaSharp {
	public enum SKAlphaType {
		Opaque = 0,
		Premul = 1,
		Unpremul = 2,
	}
	public enum SKBlurStyle {
		Inner = 3,
		Normal = 0,
		Outer = 2,
		Solid = 1,
	}
	public partial class SKCanvas {
		internal SKCanvas() { }
		public void Clear() { }
		public void Clear (SKColor color) {} 
		public void ClipPath(SkiaSharp.SKPath path) { }
		public void ClipRect(SkiaSharp.SKRect rect) { }
		public void Concat(ref SkiaSharp.SKMatrix m) { }
		public void DrawColor(SkiaSharp.SKColor color, SkiaSharp.SKXferMode mode=(SkiaSharp.SKXferMode)(1)) { }
		public void DrawImage(SkiaSharp.SKPath path, SkiaSharp.SKImage image, float x, float y, SkiaSharp.SKPaint paint) { }
		public void DrawImageScaled(SkiaSharp.SKPath path, SkiaSharp.SKImage image, SkiaSharp.SKRect dest, SkiaSharp.SKPaint paint) { }
		public void DrawImageScaled(SkiaSharp.SKPath path, SkiaSharp.SKImage image, SkiaSharp.SKRect source, SkiaSharp.SKRect dest, SkiaSharp.SKPaint paint) { }
		public void DrawLine(float x0, float y0, float x1, float y1, SkiaSharp.SKPaint paint) { }
		public void DrawOval(SkiaSharp.SKRect rect, SkiaSharp.SKPaint paint) { }
		public void DrawPaint(SkiaSharp.SKPaint paint) { }
		public void DrawPath(SkiaSharp.SKPath path, SkiaSharp.SKPaint paint) { }
		public void DrawPicture(SkiaSharp.SKPicture picture, ref SkiaSharp.SKMatrix matrix, SkiaSharp.SKPaint paint) { }
		public void DrawPicture(SkiaSharp.SKPicture picture, SkiaSharp.SKPaint paint) { }
		public void DrawPoint(float x, float y, SkiaSharp.SKColor color) { }
		public void DrawPoint(float x, float y, SkiaSharp.SKPaint paint) { }
		public void DrawPoints(SkiaSharp.SKPointMode mode, SkiaSharp.SKPoint[] points, SkiaSharp.SKPaint paint) { }
		public void DrawRect(SkiaSharp.SKRect rect, SkiaSharp.SKPaint paint) { }
		public void DrawText(string text, SkiaSharp.SKPath path, float hOffset, float vOffset, SkiaSharp.SKPaint paint) { }
		public void DrawText(string text, SkiaSharp.SKPoint[] points, SkiaSharp.SKPaint paint) { }
		public void DrawText(string text, float x, float y, SkiaSharp.SKPaint paint) { }
		public void Restore() { }
		public void RotateDegrees(float degrees) { }
		public void RotateRadians(float radians) { }
		public void Save() { }
		public void SaveLayer(SkiaSharp.SKPaint paint) { }
		public void SaveLayer(SkiaSharp.SKRect limit, SkiaSharp.SKPaint paint) { }
		public void Scale(float sx, float sy) { }
		public void Scale(SKPoint size) { }
		public void Skew(float sx, float sy) { }
		public void Skew(SKPoint skew) { }
		public void Translate(float dx, float dy) { }
		public void Translate(SKPoint point) { }
	}
	public class SKAutoCanvasRestore : System.IDisposable
	{
		public SKAutoCanvasRestore (SKCanvas canvas, bool doSave) { }
		public void Dispose () { }
		public void Restore () { }
	}
	public enum SKClipType {
		Difference = 1,
		Intersect = 0,
	}
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public partial struct SKColor {
		public SKColor(byte red, byte green, byte blue) { throw new System.NotImplementedException(); }
		public SKColor(byte red, byte green, byte blue, byte alpha) { throw new System.NotImplementedException(); }
		public SKColor WithAlpha (byte alpha) { return default(SKColor); }
		public byte Alpha { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(byte); } }
		public byte Blue { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(byte); } }
		public byte Green { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(byte); } }
		public byte Red { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(byte); } }
		public override bool Equals(object other) { return default(bool); }
		public override int GetHashCode() { return default(int); }
		public override string ToString() { return default(string); }
	}
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, Size=1)]
	public partial struct SKColors {
		public static SkiaSharp.SKColor AliceBlue;
		public static SkiaSharp.SKColor AntiqueWhite;
		public static SkiaSharp.SKColor Aqua;
		public static SkiaSharp.SKColor Aquamarine;
		public static SkiaSharp.SKColor Azure;
		public static SkiaSharp.SKColor Beige;
		public static SkiaSharp.SKColor Bisque;
		public static SkiaSharp.SKColor Black;
		public static SkiaSharp.SKColor BlanchedAlmond;
		public static SkiaSharp.SKColor Blue;
		public static SkiaSharp.SKColor BlueViolet;
		public static SkiaSharp.SKColor Brown;
		public static SkiaSharp.SKColor BurlyWood;
		public static SkiaSharp.SKColor CadetBlue;
		public static SkiaSharp.SKColor Chartreuse;
		public static SkiaSharp.SKColor Chocolate;
		public static SkiaSharp.SKColor Coral;
		public static SkiaSharp.SKColor CornflowerBlue;
		public static SkiaSharp.SKColor Cornsilk;
		public static SkiaSharp.SKColor Crimson;
		public static SkiaSharp.SKColor Cyan;
		public static SkiaSharp.SKColor DarkBlue;
		public static SkiaSharp.SKColor DarkCyan;
		public static SkiaSharp.SKColor DarkGoldenrod;
		public static SkiaSharp.SKColor DarkGray;
		public static SkiaSharp.SKColor DarkGreen;
		public static SkiaSharp.SKColor DarkKhaki;
		public static SkiaSharp.SKColor DarkMagenta;
		public static SkiaSharp.SKColor DarkOliveGreen;
		public static SkiaSharp.SKColor DarkOrange;
		public static SkiaSharp.SKColor DarkOrchid;
		public static SkiaSharp.SKColor DarkRed;
		public static SkiaSharp.SKColor DarkSalmon;
		public static SkiaSharp.SKColor DarkSeaGreen;
		public static SkiaSharp.SKColor DarkSlateBlue;
		public static SkiaSharp.SKColor DarkSlateGray;
		public static SkiaSharp.SKColor DarkTurquoise;
		public static SkiaSharp.SKColor DarkViolet;
		public static SkiaSharp.SKColor DeepPink;
		public static SkiaSharp.SKColor DeepSkyBlue;
		public static SkiaSharp.SKColor DimGray;
		public static SkiaSharp.SKColor DodgerBlue;
		public static SkiaSharp.SKColor Firebrick;
		public static SkiaSharp.SKColor FloralWhite;
		public static SkiaSharp.SKColor ForestGreen;
		public static SkiaSharp.SKColor Fuchsia;
		public static SkiaSharp.SKColor Gainsboro;
		public static SkiaSharp.SKColor GhostWhite;
		public static SkiaSharp.SKColor Gold;
		public static SkiaSharp.SKColor Goldenrod;
		public static SkiaSharp.SKColor Gray;
		public static SkiaSharp.SKColor Green;
		public static SkiaSharp.SKColor GreenYellow;
		public static SkiaSharp.SKColor Honeydew;
		public static SkiaSharp.SKColor HotPink;
		public static SkiaSharp.SKColor IndianRed;
		public static SkiaSharp.SKColor Indigo;
		public static SkiaSharp.SKColor Ivory;
		public static SkiaSharp.SKColor Khaki;
		public static SkiaSharp.SKColor Lavender;
		public static SkiaSharp.SKColor LavenderBlush;
		public static SkiaSharp.SKColor LawnGreen;
		public static SkiaSharp.SKColor LemonChiffon;
		public static SkiaSharp.SKColor LightBlue;
		public static SkiaSharp.SKColor LightCoral;
		public static SkiaSharp.SKColor LightCyan;
		public static SkiaSharp.SKColor LightGoldenrodYellow;
		public static SkiaSharp.SKColor LightGray;
		public static SkiaSharp.SKColor LightGreen;
		public static SkiaSharp.SKColor LightPink;
		public static SkiaSharp.SKColor LightSalmon;
		public static SkiaSharp.SKColor LightSeaGreen;
		public static SkiaSharp.SKColor LightSkyBlue;
		public static SkiaSharp.SKColor LightSlateGray;
		public static SkiaSharp.SKColor LightSteelBlue;
		public static SkiaSharp.SKColor LightYellow;
		public static SkiaSharp.SKColor Lime;
		public static SkiaSharp.SKColor LimeGreen;
		public static SkiaSharp.SKColor Linen;
		public static SkiaSharp.SKColor Magenta;
		public static SkiaSharp.SKColor Maroon;
		public static SkiaSharp.SKColor MediumAquamarine;
		public static SkiaSharp.SKColor MediumBlue;
		public static SkiaSharp.SKColor MediumOrchid;
		public static SkiaSharp.SKColor MediumPurple;
		public static SkiaSharp.SKColor MediumSeaGreen;
		public static SkiaSharp.SKColor MediumSlateBlue;
		public static SkiaSharp.SKColor MediumSpringGreen;
		public static SkiaSharp.SKColor MediumTurquoise;
		public static SkiaSharp.SKColor MediumVioletRed;
		public static SkiaSharp.SKColor MidnightBlue;
		public static SkiaSharp.SKColor MintCream;
		public static SkiaSharp.SKColor MistyRose;
		public static SkiaSharp.SKColor Moccasin;
		public static SkiaSharp.SKColor NavajoWhite;
		public static SkiaSharp.SKColor Navy;
		public static SkiaSharp.SKColor OldLace;
		public static SkiaSharp.SKColor Olive;
		public static SkiaSharp.SKColor OliveDrab;
		public static SkiaSharp.SKColor Orange;
		public static SkiaSharp.SKColor OrangeRed;
		public static SkiaSharp.SKColor Orchid;
		public static SkiaSharp.SKColor PaleGoldenrod;
		public static SkiaSharp.SKColor PaleGreen;
		public static SkiaSharp.SKColor PaleTurquoise;
		public static SkiaSharp.SKColor PaleVioletRed;
		public static SkiaSharp.SKColor PapayaWhip;
		public static SkiaSharp.SKColor PeachPuff;
		public static SkiaSharp.SKColor Peru;
		public static SkiaSharp.SKColor Pink;
		public static SkiaSharp.SKColor Plum;
		public static SkiaSharp.SKColor PowderBlue;
		public static SkiaSharp.SKColor Purple;
		public static SkiaSharp.SKColor Red;
		public static SkiaSharp.SKColor RosyBrown;
		public static SkiaSharp.SKColor RoyalBlue;
		public static SkiaSharp.SKColor SaddleBrown;
		public static SkiaSharp.SKColor Salmon;
		public static SkiaSharp.SKColor SandyBrown;
		public static SkiaSharp.SKColor SeaGreen;
		public static SkiaSharp.SKColor SeaShell;
		public static SkiaSharp.SKColor Sienna;
		public static SkiaSharp.SKColor Silver;
		public static SkiaSharp.SKColor SkyBlue;
		public static SkiaSharp.SKColor SlateBlue;
		public static SkiaSharp.SKColor SlateGray;
		public static SkiaSharp.SKColor Snow;
		public static SkiaSharp.SKColor SpringGreen;
		public static SkiaSharp.SKColor SteelBlue;
		public static SkiaSharp.SKColor Tan;
		public static SkiaSharp.SKColor Teal;
		public static SkiaSharp.SKColor Thistle;
		public static SkiaSharp.SKColor Tomato;
		public static SkiaSharp.SKColor Transparent;
		public static SkiaSharp.SKColor Turquoise;
		public static SkiaSharp.SKColor Violet;
		public static SkiaSharp.SKColor Wheat;
		public static SkiaSharp.SKColor White;
		public static SkiaSharp.SKColor WhiteSmoke;
		public static SkiaSharp.SKColor Yellow;
		public static SkiaSharp.SKColor YellowGreen;
		public static SkiaSharp.SKColor Empty { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(SkiaSharp.SKColor); } }
	}
	public enum SKColorType {
		Alpha_8 = 3,
		Bgra_8888 = 2,
		Rgba_8888 = 1,
		Unknown = 0,
	}
	public partial class SKData : System.IDisposable {
		public SKData() { }
		public SKData(System.IntPtr bytes, ulong length) { }
		public System.IntPtr Data { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(System.IntPtr); } }
		public long Size { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(long); } }
		public void Dispose() { }
		protected virtual void Dispose(bool disposing) { }
		~SKData() { }
		public static SkiaSharp.SKData FromMallocMemory(System.IntPtr bytes, ulong length) { return default(SkiaSharp.SKData); }
		public void SaveTo(System.IO.Stream target) { }
		public SkiaSharp.SKData Subset(ulong offset, ulong length) { return default(SkiaSharp.SKData); }
	}
	public enum SKEncoding {
		Utf16 = 1,
		Utf32 = 2,
		Utf8 = 0,
	}
	public enum SKTextEncoding {
		GlyphId = 3,
		Utf16 = 1,
		Utf32 = 2,
		Utf8 = 0,
	}
	public static partial class SkiaExtensions {
		public static bool IsBgr(this SkiaSharp.SKPixelGeometry pg) { return default(bool); }
		public static bool IsHorizontal(this SkiaSharp.SKPixelGeometry pg) { return default(bool); }
		public static bool IsRgb(this SkiaSharp.SKPixelGeometry pg) { return default(bool); }
		public static bool IsVertical(this SkiaSharp.SKPixelGeometry pg) { return default(bool); }
	}
	public partial class SKImage : System.IDisposable {
		internal SKImage() { }
		public int Height { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } }
		public uint UniqueId { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(uint); } }
		public int Width { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(int); } }
		public void Dispose() { }
		protected virtual void Dispose(bool disposing) { }
		public SkiaSharp.SKData Encode() { return default(SkiaSharp.SKData); }
		~SKImage() { }
		public static SkiaSharp.SKImage FromData(SkiaSharp.SKData data, SkiaSharp.SKRectI subset) { return default(SkiaSharp.SKImage); }
		public static SkiaSharp.SKImage FromPixels(SkiaSharp.SKImageInfo info, System.IntPtr pixels, int rowBytes) { return default(SkiaSharp.SKImage); }
	}
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public partial struct SKImageInfo {
		public SkiaSharp.SKAlphaType AlphaType;
		public SkiaSharp.SKColorType ColorType;
		public int Height;
		public int Width;
		public SKImageInfo(int width, int height, SkiaSharp.SKColorType colorType, SkiaSharp.SKAlphaType alphaType) { throw new System.NotImplementedException(); }
	}
	public partial class SKMaskFilter : System.IDisposable {
		public SKMaskFilter(SkiaSharp.SKBlurStyle blurStyle, float sigma) { }
		public void Dispose() { }
		protected virtual void Dispose(bool disposing) { }
		~SKMaskFilter() { }
	}
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public partial struct SKMatrix {
		public float Persp0;
		public float Persp1;
		public float Persp2;
		public float ScaleX;
		public float ScaleY;
		public float SkewX;
		public float SkewY;
		public float TransX;
		public float TransY;
		public static SkiaSharp.SKMatrix MakeIdentity() { return default(SkiaSharp.SKMatrix); }
		public static SkiaSharp.SKMatrix MakeRotation(float radians) { return default(SkiaSharp.SKMatrix); }
		public static SkiaSharp.SKMatrix MakeScale(float sx, float sy) { return default(SkiaSharp.SKMatrix); }
		public static SkiaSharp.SKMatrix MakeScale(float sx, float sy, float pivotX, float pivotY) { return default(SkiaSharp.SKMatrix); }
		public static SkiaSharp.SKMatrix MakeSkew(float sx, float sy) { return default(SkiaSharp.SKMatrix); }
		public static SkiaSharp.SKMatrix MakeTranslation(float dx, float dy) { return default(SkiaSharp.SKMatrix); }
		public void SetScaleTranslate(float sx, float sy, float tx, float ty) { }
	}
	public partial class SKPaint : System.IDisposable {
		public SKPaint() { }
		public SkiaSharp.SKColor Color { get { return default(SkiaSharp.SKColor); } set { } }
		public bool IsAntialias { get { return default(bool); } set { } }
		public bool IsStroke { get { return default(bool); } set { } }
		public SkiaSharp.SKMaskFilter MaskFilter { get { return default(SkiaSharp.SKMaskFilter); } set { } }
		public SkiaSharp.SKShader Shader { get { return default(SkiaSharp.SKShader); } set { } }
		public SkiaSharp.SKStrokeCap StrokeCap { get { return default(SkiaSharp.SKStrokeCap); } set { } }
		public SkiaSharp.SKStrokeJoin StrokeJoin { get { return default(SkiaSharp.SKStrokeJoin); } set { } }
		public float StrokeMiter { get { return default(float); } set { } }
		public float StrokeWidth { get { return default(float); } set { } }
		public float TextSize { get { return default(float); } set { } }
		public SkiaSharp.SKTypeface Typeface { get { return default(SkiaSharp.SKTypeface); } set { } }
		public SkiaSharp.SKXferMode XferMode { get { return default(SkiaSharp.SKXferMode); } set { } }
		public SkiaSharp.SKTextAlign TextAlign { get { return default(SkiaSharp.SKTextAlign); } set { } }
		public SkiaSharp.SKTextEncoding TextEncoding { get { return default(SkiaSharp.SKTextEncoding); } set { } }
		public float TextScaleX { get { return default(float); } set { } }
		public float TextSkewX { get { return default(float); } set { } }
		public void Dispose() { }
		protected virtual void Dispose(bool disposing) { }
		~SKPaint() { }
	}
	public partial class SKPath : System.IDisposable {
		public SKPath() { }
		public void AddOval(SkiaSharp.SKRect rect, SkiaSharp.SKPathDirection direction) { }
		public void AddRect(SkiaSharp.SKRect rect, SkiaSharp.SKPathDirection direction) { }
		public void Close() { }
		public void ConicTo(float x0, float y0, float x1, float y1, float w) { }
		public void CubicTo(float x0, float y0, float x1, float y1, float x2, float y2) { }
		public void Dispose() { }
		protected virtual void Dispose(bool disposing) { }
		~SKPath() { }
		public bool GetBounds(out SkiaSharp.SKRect rect) { rect = default(SkiaSharp.SKRect); return default(bool); }
		public void LineTo(float x, float y) { }
		public void MoveTo(float x, float y) { }
		public void QuadTo(float x0, float y0, float x1, float y1) { }
	}
	public enum SKPathDirection {
		Clockwise = 0,
		CounterClockwise = 1,
	}
	public partial class SKPicture : System.IDisposable {
		internal SKPicture() { }
		public SkiaSharp.SKRect Bounds { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(SkiaSharp.SKRect); } }
		public uint UniqueId { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(uint); } }
		public void Dispose() { }
		protected virtual void Dispose(bool disposing) { }
		~SKPicture() { }
	}
	public partial class SKPictureRecorder : System.IDisposable {
		public SKPictureRecorder() { }
		public SkiaSharp.SKCanvas BeginRecording(SkiaSharp.SKRect rect) { return default(SkiaSharp.SKCanvas); }
		public void Dispose() { }
		protected virtual void Dispose(bool disposing) { }
		public SkiaSharp.SKPicture EndRecording() { return default(SkiaSharp.SKPicture); }
		~SKPictureRecorder() { }
	}
	public enum SKPixelGeometry {
		BgrHorizontal = 2,
		BgrVertical = 4,
		RgbHorizontal = 1,
		RgbVertical = 3,
		Unknown = 0,
	}
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public partial struct SKPoint {
		public float X;
		public float Y;
		public SKPoint(float x, float y) { throw new System.NotImplementedException(); }
	}
	public enum SKPointMode {
		Lines = 1,
		Points = 0,
		Polygon = 2,
	}
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public partial struct SKRect {
		public float Bottom;
		public float Left;
		public float Right;
		public float Top;
		public SKRect(float left, float top, float right, float bottom) { throw new System.NotImplementedException(); }
		public static SKRect Create (float width, float height) { throw new System.NotImplementedException(); }
		public static SKRect Create (float x, float y, float width, float height) { throw new System.NotImplementedException(); }
	}
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public partial struct SKRectI {
		public int Bottom;
		public int Left;
		public int Right;
		public int Top;
		public SKRectI(int left, int top, int right, int bottom) { throw new System.NotImplementedException(); }
		public static SKRectI Create (int width, int height) { throw new System.NotImplementedException(); }
		public static SKRectI Create (int x, int y, int width, int height) { throw new System.NotImplementedException(); }
	}
	public partial class SKShader : System.IDisposable {
		internal SKShader() { }
		public static SkiaSharp.SKShader CreateLinearGradient(SkiaSharp.SKPoint start, SkiaSharp.SKPoint end, SkiaSharp.SKColor[] colors, System.Single[] colorPos, SkiaSharp.SKShaderTileMode mode) { return default(SkiaSharp.SKShader); }
		public static SkiaSharp.SKShader CreateLinearGradient(SkiaSharp.SKPoint start, SkiaSharp.SKPoint end, SkiaSharp.SKColor[] colors, System.Single[] colorPos, SkiaSharp.SKShaderTileMode mode, SkiaSharp.SKMatrix localMatrix) { return default(SkiaSharp.SKShader); }
		public static SkiaSharp.SKShader CreateRadialGradient(SkiaSharp.SKPoint center, float radius, SkiaSharp.SKColor[] colors, System.Single[] colorPos, SkiaSharp.SKShaderTileMode mode) { return default(SkiaSharp.SKShader); }
		public static SkiaSharp.SKShader CreateRadialGradient(SkiaSharp.SKPoint center, float radius, SkiaSharp.SKColor[] colors, System.Single[] colorPos, SkiaSharp.SKShaderTileMode mode, SkiaSharp.SKMatrix localMatrix) { return default(SkiaSharp.SKShader); }
		public static SkiaSharp.SKShader CreateSweepGradient(SkiaSharp.SKPoint center, SkiaSharp.SKColor[] colors, System.Single[] colorPos) { return default(SkiaSharp.SKShader); }
		public static SkiaSharp.SKShader CreateSweepGradient(SkiaSharp.SKPoint center, SkiaSharp.SKColor[] colors, System.Single[] colorPos, SkiaSharp.SKMatrix localMatrix) { return default(SkiaSharp.SKShader); }
		public static SkiaSharp.SKShader CreateTwoPointConicalGradient(SkiaSharp.SKPoint start, float startRadius, SkiaSharp.SKPoint end, float endRadius, SkiaSharp.SKColor[] colors, System.Single[] colorPos, SkiaSharp.SKShaderTileMode mode) { return default(SkiaSharp.SKShader); }
		public static SkiaSharp.SKShader CreateTwoPointConicalGradient(SkiaSharp.SKPoint start, float startRadius, SkiaSharp.SKPoint end, float endRadius, SkiaSharp.SKColor[] colors, System.Single[] colorPos, SkiaSharp.SKShaderTileMode mode, SkiaSharp.SKMatrix localMatrix) { return default(SkiaSharp.SKShader); }
		public void Dispose() { }
		protected virtual void Dispose(bool disposing) { }
		~SKShader() { }
	}
	public enum SKShaderTileMode {
		Clamp = 0,
		Mirror = 2,
		Repeat = 1,
	}
	public enum SKStrokeCap {
		Butt = 0,
		Round = 1,
		Square = 2,
	}
	public enum SKStrokeJoin {
		Bevel = 2,
		Mitter = 0,
		Round = 1,
	}
	public enum SKTextAlign {
		Left = 0,
		Center = 1,
		Right = 2,
	}
	public partial class SKSurface : System.IDisposable {
		internal SKSurface() { }
		public SkiaSharp.SKCanvas Canvas { get { return default(SkiaSharp.SKCanvas); } }
		public static SkiaSharp.SKSurface Create(SkiaSharp.SKImageInfo info) { return default(SkiaSharp.SKSurface); }
		public static SkiaSharp.SKSurface Create(SkiaSharp.SKImageInfo info, SkiaSharp.SKSurfaceProps props) { return default(SkiaSharp.SKSurface); }
		public static SkiaSharp.SKSurface Create(SkiaSharp.SKImageInfo info, System.IntPtr pixels, int rowBytes) { return default(SkiaSharp.SKSurface); }
		public static SkiaSharp.SKSurface Create(SkiaSharp.SKImageInfo info, System.IntPtr pixels, int rowBytes, SkiaSharp.SKSurfaceProps props) { return default(SkiaSharp.SKSurface); }
		public static SkiaSharp.SKSurface Create(int width, int height, SkiaSharp.SKColorType colorType, SkiaSharp.SKAlphaType alphaType) { return default(SkiaSharp.SKSurface); }
		public static SkiaSharp.SKSurface Create(int width, int height, SkiaSharp.SKColorType colorType, SkiaSharp.SKAlphaType alphaType, SkiaSharp.SKSurfaceProps props) { return default(SkiaSharp.SKSurface); }
		public static SkiaSharp.SKSurface Create(int width, int height, SkiaSharp.SKColorType colorType, SkiaSharp.SKAlphaType alphaType, System.IntPtr pixels, int rowBytes) { return default(SkiaSharp.SKSurface); }
		public static SkiaSharp.SKSurface Create(int width, int height, SkiaSharp.SKColorType colorType, SkiaSharp.SKAlphaType alphaType, System.IntPtr pixels, int rowBytes, SkiaSharp.SKSurfaceProps props) { return default(SkiaSharp.SKSurface); }
		public void Dispose() { }
		protected virtual void Dispose(bool disposing) { }
		~SKSurface() { }
		public SkiaSharp.SKImage Snapshot() { return default(SkiaSharp.SKImage); }
	}
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
	public partial struct SKSurfaceProps {
		public SkiaSharp.SKPixelGeometry PixelGeometry { [System.Runtime.CompilerServices.CompilerGeneratedAttribute]get { return default(SkiaSharp.SKPixelGeometry); } [System.Runtime.CompilerServices.CompilerGeneratedAttribute]set { } }
	}
	public partial class SKTypeface : System.IDisposable {
		internal SKTypeface() { }
		public int CharsToGlyphs(System.IntPtr str, int strlen, SkiaSharp.SKEncoding encoding, out System.UInt16[] glyphs) { glyphs = default(System.UInt16[]); return default(int); }
		public int CharsToGlyphs(string chars, out System.UInt16[] glyphs) { glyphs = default(System.UInt16[]); return default(int); }
		public int CountGlyphs(System.IntPtr str, int strLen, SkiaSharp.SKEncoding encoding) { return default(int); }
		public int CountGlyphs(string str) { return default(int); }
		public void Dispose() { }
		protected virtual void Dispose(bool disposing) { }
		~SKTypeface() { }
		public static SkiaSharp.SKTypeface FromFamilyName(string familyName, SkiaSharp.SKTypefaceStyle style=(SkiaSharp.SKTypefaceStyle)(0)) { return default(SkiaSharp.SKTypeface); }
		public static SkiaSharp.SKTypeface FromTypeface(SkiaSharp.SKTypeface typeface, SkiaSharp.SKTypefaceStyle style=(SkiaSharp.SKTypefaceStyle)(0)) { return default(SkiaSharp.SKTypeface); }
		public static SkiaSharp.SKTypeface FromFile(string path, int index = 0) { return default(SkiaSharp.SKTypeface); }
		public static SkiaSharp.SKTypeface FromStream(SkiaSharp.SKStreamAsset stream, int index = 0) { return default(SkiaSharp.SKTypeface); }
	}
	public abstract class SKStream : System.IDisposable
	{
		internal SKStream (System.IntPtr handle, bool owns) { }
		public void Dispose () { }
		protected virtual void Dispose (bool disposing) { }
		~SKStream () { }
		public bool IsAtEnd { get { return default(bool); } }
		public System.SByte ReadSByte () { return 0; }
		public System.Int16 ReadInt16 () { return 0; }
		public System.Int32 ReadInt32 () { return 0; }
		public System.Byte ReadByte () { return 0; }
		public System.UInt16 ReadUInt16 () { return 0; }
		public System.UInt32 ReadUInt32 () { return 0; }
	}
	public abstract class SKStreamRewindable : SKStream
	{
		internal SKStreamRewindable (System.IntPtr handle, bool owns) : base (handle, owns) { }
	}
	public abstract class SKStreamSeekable : SKStreamRewindable
	{
		internal SKStreamSeekable (System.IntPtr handle, bool owns) : base (handle, owns) { }
	}
	public abstract class SKStreamAsset : SKStreamSeekable
	{
		internal SKStreamAsset (System.IntPtr handle, bool owns) : base (handle, owns) { }
	}
	public abstract class SKStreamMemory : SKStreamAsset
	{
		internal SKStreamMemory (System.IntPtr handle, bool owns) : base (handle, owns) { }
	}
	public class SKFileStream : SKStreamAsset
	{
		public SKFileStream (string path) : base (default(System.IntPtr), default(bool)) { }
	}
	public class SKMemoryStream : SKStreamMemory
	{
		public SKMemoryStream () : base (default(System.IntPtr), default(bool)) { }
		public SKMemoryStream (long length) : base (default(System.IntPtr), default(bool)) { }
		public SKMemoryStream (SKData data) : base (default(System.IntPtr), default(bool)) { }
		public SKMemoryStream (byte[] data) : this () { }
		public void SetMemory (byte[] data) { }
	}
	public class SKManagedStream : SKStreamAsset
	{
		public SKManagedStream (System.IO.Stream managedStream) : base (default(System.IntPtr), default(bool)) { }
	}
	public enum SKTypefaceStyle {
		Bold = 1,
		BoldItalic = 3,
		Italic = 2,
		Normal = 0,
	}
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
}


