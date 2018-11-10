using System;
using System.Runtime.InteropServices;
using System.Globalization;

using sk_string_t = System.IntPtr;
 	
namespace SkiaSharp
{
	public enum SKCodecResult {
		Success,
		IncompleteInput,
		ErrorInInput,
		InvalidConversion,
		InvalidScale,
		InvalidParameters,
		InvalidInput,
		CouldNotRewind,
		InternalError,
		Unimplemented,
	}

	[Obsolete ("Use SKEncodedOrigin instead.")]
	public enum SKCodecOrigin {
		TopLeft = 1,
		TopRight = 2,
		BottomRight = 3,
		BottomLeft = 4,
		LeftTop = 5,
		RightTop = 6,
		RightBottom = 7,
		LeftBottom = 8,
	}

	public enum SKEncodedOrigin {
		TopLeft = 1,
		TopRight = 2,
		BottomRight = 3,
		BottomLeft = 4,
		LeftTop = 5,
		RightTop = 6,
		RightBottom = 7,
		LeftBottom = 8,
		Default = TopLeft,
	}

	public enum SKEncodedImageFormat {
		Bmp,
		Gif,
		Ico,
		Jpeg,
		Png,
		Wbmp,
		Webp,
		Pkm,
		Ktx,
		Astc,
		Dng,
		// Heif // appears to be development still
	}

	public enum SKFontStyleWeight {
		Invisible   =   0,
		Thin        = 100,
		ExtraLight  = 200,
		Light       = 300,
		Normal      = 400,
		Medium      = 500,
		SemiBold    = 600,
		Bold        = 700,
		ExtraBold   = 800,
		Black       = 900,
		ExtraBlack  =1000,
	};

	public enum SKFontStyleWidth {
		UltraCondensed   = 1,
		ExtraCondensed   = 2,
		Condensed        = 3,
		SemiCondensed    = 4,
		Normal           = 5,
		SemiExpanded     = 6,
		Expanded         = 7,
		ExtraExpanded    = 8,
		UltraExpanded    = 9,
	};

	public enum SKFontStyleSlant {
		Upright = 0,
		Italic  = 1,
		Oblique = 2,
	};

	public enum SKPointMode {
		Points, Lines, Polygon
	}

	public enum SKPathDirection {
		Clockwise,
		CounterClockwise
	}

	public enum SKPathArcSize {
		Small,
		Large
	}

	public enum SKPathFillType
	{
		Winding,
		EvenOdd,
		InverseWinding,
		InverseEvenOdd
	}

	[Flags]
	public enum SKPathSegmentMask {
		Line  = 1 << 0,
		Quad  = 1 << 1,
		Conic = 1 << 2,
		Cubic = 1 << 3,
	}

	public enum SKColorType {
		Unknown,
		Alpha8,
		Rgb565,
		Argb4444,
		Rgba8888,
		Rgb888x,
		Bgra8888,
		Rgba1010102,
		Rgb101010x,
		Gray8,
		RgbaF16
	}

	public enum SKAlphaType {
		Unknown,
		Opaque,
		Premul,
		Unpremul
	}

	public enum SKShaderTileMode {
		Clamp, Repeat, Mirror
	}

	public enum SKBlurStyle {
		Normal, Solid, Outer, Inner
	}

	public enum SKBlendMode {
		Clear,
		Src,
		Dst,
		SrcOver,
		DstOver,
		SrcIn,
		DstIn,
		SrcOut,
		DstOut,
		SrcATop,
		DstATop,
		Xor,
		Plus,
		Modulate,
		Screen,
		Overlay,
		Darken,
		Lighten,
		ColorDodge,
		ColorBurn,
		HardLight,
		SoftLight,
		Difference,
		Exclusion,
		Multiply,
		Hue,
		Saturation,
		Color,
		Luminosity,
	}

	public enum SKPixelGeometry {
		Unknown,
		RgbHorizontal,
		BgrHorizontal,
		RgbVertical,
		BgrVertical
	}

	[Flags]
	public enum SKSurfacePropsFlags {
		None = 0,
		UseDeviceIndependentFonts = 1 << 0,
	}

	public enum SKEncoding {
		Utf8, Utf16, Utf32
	}

	public static partial class SkiaExtensions {
		public static bool IsBgr (this SKPixelGeometry pg)
		{
			return pg == SKPixelGeometry.BgrHorizontal || pg == SKPixelGeometry.BgrVertical;
		}

		public static bool IsRgb (this SKPixelGeometry pg)
		{
			return pg == SKPixelGeometry.RgbHorizontal || pg == SKPixelGeometry.RgbVertical;
		}

		public static bool IsVertical (this SKPixelGeometry pg)
		{
			return pg == SKPixelGeometry.BgrVertical || pg == SKPixelGeometry.RgbVertical;
		}

		public static bool IsHorizontal (this SKPixelGeometry pg)
		{
			return pg == SKPixelGeometry.BgrHorizontal || pg == SKPixelGeometry.RgbHorizontal;
		}
	}

	public enum SKStrokeCap {
		Butt, Round, Square
	}

	public enum SKStrokeJoin {
		Miter, Round, Bevel,
	}

	public enum SKTextAlign {
		Left, Center, Right
	}

	public enum SKTextEncoding {
		Utf8, Utf16, Utf32, GlyphId
	}

	public enum SKFilterQuality
	{
		None,
		Low,
		Medium,
		High
	}

	[Flags]
	public enum SKCropRectFlags
	{
		HasNone = 0,
		HasLeft = 0x01,
		HasTop = 0x02,
		HasWidth = 0x04,
		HasHeight = 0x08,
		HasAll = 0x0F,
	}

	public enum SKDropShadowImageFilterShadowMode
	{
		DrawShadowAndForeground,
		DrawShadowOnly,
	}

	public enum SKDisplacementMapEffectChannelSelectorType
	{
		Unknown,
		R,
		G,
		B,
		A,
	}

	public enum SKMatrixConvolutionTileMode
	{
		Clamp,
		Repeat,
		ClampToBlack,
	}

	public enum SKPaintStyle
	{
		Fill,
		Stroke,
		StrokeAndFill,
	}

	public enum SKPaintHinting
	{
		NoHinting = 0,
		Slight = 1,
		Normal = 2,
		Full = 3
	}

	public enum SKRegionOperation
	{
		Difference,
		Intersect,
		Union,
		XOR,
		ReverseDifference,
		Replace,
	}

	public enum SKClipOperation
	{
		Difference,
		Intersect,
	}

	[Obsolete("Use SKSurfaceProperties instead.")]
	public struct SKSurfaceProps {
		public SKPixelGeometry PixelGeometry { get; set; }
		public SKSurfacePropsFlags Flags { get; set; }
	}

	public enum SKZeroInitialized {
		Yes,
		No,
	}

	public enum SKCodecScanlineOrder {
		TopDown,
		BottomUp
	}


	public enum SKTransferFunctionBehavior {
		Respect,
		Ignore,
	}


	[StructLayout (LayoutKind.Sequential)]
	internal unsafe struct SKCodecOptionsInternal {
		public SKZeroInitialized fZeroInitialized;
		public SKRectI* fSubset;
		public int fFrameIndex;
		public int fPriorFrame;
		public SKTransferFunctionBehavior fPremulBehavior;
	}

	public struct SKCodecOptions {
		public static readonly SKCodecOptions Default;

		static SKCodecOptions ()
		{
			Default = new SKCodecOptions (SKZeroInitialized.No);
		}
		public SKCodecOptions (SKZeroInitialized zeroInitialized) {
			ZeroInitialized = zeroInitialized;
			Subset = null;
			FrameIndex = 0;
			PriorFrame = -1;
			PremulBehavior = SKTransferFunctionBehavior.Respect;
		}
		public SKCodecOptions (SKZeroInitialized zeroInitialized, SKRectI subset) {
			ZeroInitialized = zeroInitialized;
			Subset = subset;
			FrameIndex = 0;
			PriorFrame = -1;
			PremulBehavior = SKTransferFunctionBehavior.Respect;
		}
		public SKCodecOptions (SKRectI subset) {
			ZeroInitialized = SKZeroInitialized.No;
			Subset = subset;
			FrameIndex = 0;
			PriorFrame = -1;
			PremulBehavior = SKTransferFunctionBehavior.Respect;
		}
		public SKCodecOptions (int frameIndex) {
			ZeroInitialized = SKZeroInitialized.No;
			Subset = null;
			FrameIndex = frameIndex;
			PriorFrame = -1;
			PremulBehavior = SKTransferFunctionBehavior.Respect;
		}
		public SKCodecOptions (int frameIndex, int priorFrame) {
			ZeroInitialized = SKZeroInitialized.No;
			Subset = null;
			FrameIndex = frameIndex;
			PriorFrame = priorFrame;
			PremulBehavior = SKTransferFunctionBehavior.Respect;
		}
		public SKZeroInitialized ZeroInitialized { get; set; }
		public SKRectI? Subset { get; set; }
		public bool HasSubset => Subset != null;
		public int FrameIndex { get; set; }
		public int PriorFrame { get; set; }
		public SKTransferFunctionBehavior PremulBehavior { get; set; }
	}

	public enum SKCodecAnimationDisposalMethod {
		Keep                     = 1,
		RestoreBackgroundColor   = 2,
		RestorePrevious          = 3,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKCodecFrameInfo {
		private int requiredFrame;
		private int duration;
		private byte fullyRecieved;
		private SKAlphaType alphaType;
		private SKCodecAnimationDisposalMethod disposalMethod;

		public int RequiredFrame {
			get { return requiredFrame; }
			set { requiredFrame = value; }
		}

		public int Duration {
			get { return duration; }
			set { duration = value; }
		}

		public bool FullyRecieved {
			get { return fullyRecieved != 0; }
			set { fullyRecieved = value ? (byte)1 : (byte)0; }
		}

		public SKAlphaType AlphaType {
			get { return alphaType; }
			set { alphaType = value; }
		}

		public SKCodecAnimationDisposalMethod DisposalMethod {
			get { return disposalMethod; }
			set { disposalMethod = value; }
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKPoint {
		private float x, y;

		public static readonly SKPoint Empty;

		public static SKPoint operator + (SKPoint pt, SKSizeI sz)
		{
			return new SKPoint (pt.X + sz.Width, pt.Y + sz.Height);
		}
		public static SKPoint operator + (SKPoint pt, SKSize sz)
		{
			return new SKPoint (pt.X + sz.Width, pt.Y + sz.Height);
		}
		
		public static SKPoint operator + (SKPoint pt, SKPointI sz)
		{
			return new SKPoint (pt.X + sz.X, pt.Y + sz.Y);
		}
		public static SKPoint operator + (SKPoint pt, SKPoint sz)
		{
			return new SKPoint (pt.X + sz.X, pt.Y + sz.Y);
		}
		
		public static bool operator == (SKPoint left, SKPoint right)
		{
			return ((left.X == right.X) && (left.Y == right.Y));
		}
		
		public static bool operator != (SKPoint left, SKPoint right)
		{
			return !(left == right);
		}
		
		public static SKPoint operator - (SKPoint pt, SKSizeI sz)
		{
			return new SKPoint (pt.X - sz.Width, pt.Y - sz.Height);
		}
		public static SKPoint operator - (SKPoint pt, SKSize sz)
		{
			return new SKPoint (pt.X - sz.Width, pt.Y - sz.Height);
		}
		
		public static SKPoint operator - (SKPoint pt, SKPointI sz)
		{
			return new SKPoint (pt.X - sz.X, pt.Y - sz.Y);
		}
		public static SKPoint operator - (SKPoint pt, SKPoint sz)
		{
			return new SKPoint (pt.X - sz.X, pt.Y - sz.Y);
		}
		
		public SKPoint (float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		public bool IsEmpty {
			get {
				return ((x == 0.0) && (y == 0.0));
			}
		}

		public float X {
			get {
				return x;
			}
			set {
				x = value;
			}
		}

		public float Y {
			get {
				return y;
			}
			set {
				y = value;
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is SKPoint))
				return false;

			return (this == (SKPoint) obj);
		}

		public override int GetHashCode ()
		{
			return (int) x ^ (int) y;
		}

		public override string ToString ()
		{
			return String.Format (
				"{{X={0}, Y={1}}}", 
				x.ToString (CultureInfo.CurrentCulture),
				y.ToString (CultureInfo.CurrentCulture));
		}
		
		public void Offset (SKPoint p)
		{
			Offset (p.X, p.Y);
		}
		
		public void Offset (float dx, float dy)
		{
			x += dx;
			y += dy;
		}

		public static SKPoint Add (SKPoint pt, SKSizeI sz) => pt + sz;
		public static SKPoint Add (SKPoint pt, SKSize sz) => pt + sz;
		public static SKPoint Add (SKPoint pt, SKPointI sz) => pt + sz;
		public static SKPoint Add (SKPoint pt, SKPoint sz) => pt + sz;

		public static SKPoint Subtract (SKPoint pt, SKSizeI sz) => pt - sz;
		public static SKPoint Subtract (SKPoint pt, SKSize sz) => pt - sz;
		public static SKPoint Subtract (SKPoint pt, SKPointI sz) => pt - sz;
		public static SKPoint Subtract (SKPoint pt, SKPoint sz) => pt - sz;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKPointI {
		private int x, y;

		public static readonly SKPointI Empty;

		public static SKPointI Ceiling (SKPoint value)
		{
			int x, y;
			checked {
				x = (int) Math.Ceiling (value.X);
				y = (int) Math.Ceiling (value.Y);
			}

			return new SKPointI (x, y);
		}

		public static SKPointI Round (SKPoint value)
		{
			int x, y;
			checked {
				x = (int) Math.Round (value.X);
				y = (int) Math.Round (value.Y);
			}

			return new SKPointI (x, y);
		}

		public static SKPointI Truncate (SKPoint value)
		{
			int x, y;
			checked {
				x = (int) value.X;
				y = (int) value.Y;
			}

			return new SKPointI (x, y);
		}

		public static SKPointI operator + (SKPointI pt, SKSizeI sz)
		{
			return new SKPointI (pt.X + sz.Width, pt.Y + sz.Height);
		}
		
		public static SKPointI operator + (SKPointI pt, SKPointI sz)
		{
			return new SKPointI (pt.X + sz.X, pt.Y + sz.Y);
		}
		
		public static bool operator == (SKPointI left, SKPointI right)
		{
			return ((left.X == right.X) && (left.Y == right.Y));
		}
		
		public static bool operator != (SKPointI left, SKPointI right)
		{
			return !(left == right);
		}
		
		public static SKPointI operator - (SKPointI pt, SKSizeI sz)
		{
			return new SKPointI (pt.X - sz.Width, pt.Y - sz.Height);
		}
		
		public static SKPointI operator - (SKPointI pt, SKPointI sz)
		{
			return new SKPointI (pt.X - sz.X, pt.Y - sz.Y);
		}
		
		public static explicit operator SKSizeI (SKPointI p)
		{
			return new SKSizeI (p.X, p.Y);
		}

		public static implicit operator SKPoint (SKPointI p)
		{
			return new SKPoint (p.X, p.Y);
		}

		public SKPointI (SKSizeI sz)
		{
			x = sz.Width;
			y = sz.Height;
		}

		public SKPointI (int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public bool IsEmpty {
			get {
				return ((x == 0) && (y == 0));
			}
		}

		public int X {
			get {
				return x;
			}
			set {
				x = value;
			}
		}

		public int Y {
			get {
				return y;
			}
			set {
				y = value;
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is SKPointI))
				return false;

			return (this == (SKPointI) obj);
		}

		public override int GetHashCode ()
		{
			return x^y;
		}

		public void Offset (int dx, int dy)
		{
			x += dx;
			y += dy;
		}

		public override string ToString ()
		{
			return string.Format (
				"{{X={0},Y={1}}}", 
				x.ToString (CultureInfo.InvariantCulture), 
				y.ToString (CultureInfo.InvariantCulture));
		}

		public void Offset (SKPointI p)
		{
			Offset (p.X, p.Y);
		}

		public static SKPointI Add (SKPointI pt, SKSizeI sz) => pt + sz;
		public static SKPointI Add (SKPointI pt, SKPointI sz) => pt + sz;

		public static SKPointI Subtract (SKPointI pt, SKSizeI sz) => pt - sz;
		public static SKPointI Subtract (SKPointI pt, SKPointI sz) => pt - sz;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKPoint3
	{
		public static readonly SKPoint3 Empty;
		private float x, y, z;

		public float X {
			get {
				return x;
			}
			set {
				x = value;
			}
		}

		public float Y {
			get {
				return y;
			}
			set {
				y = value;
			}
		}

		public float Z {
			get {
				return z;
			}
			set {
				z = value;
			}
		}

		public SKPoint3(float x, float y, float z)
		{
			this.x = x;
			this.y = y;
			this.z = z;
		}

		public static SKPoint3 operator + (SKPoint3 pt, SKPoint3 sz)
		{
			return new SKPoint3 (pt.X + sz.X, pt.Y + sz.Y, pt.Z + sz.Z);
		}
		
		public static SKPoint3 operator - (SKPoint3 pt, SKPoint3 sz)
		{
			return new SKPoint3 (pt.X - sz.X, pt.Y - sz.Y, pt.Z - sz.Z);
		}
		
		public static bool operator == (SKPoint3 left, SKPoint3 right)
		{
			return ((left.x == right.x) && (left.y == right.y) && (left.z == right.z));
		}
		
		public static bool operator != (SKPoint3 left, SKPoint3 right)
		{
			return !(left == right);
		}

		public bool IsEmpty {
			get {
				return ((x == 0.0) && (y == 0.0) && (z==0.0));
			}
		}

		public override bool Equals (object obj)
		{
			if (!(obj is SKPoint3))
				return false;

			return (this == (SKPoint3) obj);
		}

		public override int GetHashCode ()
		{
			return (int) x ^ (int) y ^ (int) z;
		}

		public override string ToString ()
		{
			return String.Format ("{{X={0}, Y={1}, Z={2}}}",
					      x.ToString (CultureInfo.CurrentCulture),
					      y.ToString (CultureInfo.CurrentCulture),
					      z.ToString (CultureInfo.CurrentCulture)
				);
		}

		public static SKPoint3 Add (SKPoint3 pt, SKPoint3 sz) => pt + sz;
		
		public static SKPoint3 Subtract (SKPoint3 pt, SKPoint3 sz) => pt - sz;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKSize
	{
		private float width, height;

		public static readonly SKSize Empty;

		public SKSize (float width, float height)
		{
			this.width = width;
			this.height = height;
		}
		
		public SKSize (SKPoint pt)
		{
			this.width = pt.X;
			this.height = pt.Y;
		}

		public bool IsEmpty => (width == 0) && (height == 0);

		public float Width {
			get { return width; }
			set { width = value; }
		}

		public float Height {
			get { return height; }
			set { height = value; }
		}

		public SKPoint ToPoint ()
		{
			return new SKPoint (width, height);
		}

		public SKSizeI ToSizeI ()
		{
			int w, h;
			checked {
				w = (int) width;
				h = (int) height;
			}

			return new SKSizeI (w, h);
		}

		public static SKSize operator + (SKSize sz1, SKSize sz2)
		{
			return new SKSize (sz1.Width + sz2.Width, sz1.Height + sz2.Height);
		}

		public static SKSize operator - (SKSize sz1, SKSize sz2)
		{
			return new SKSize (sz1.Width - sz2.Width, sz1.Height - sz2.Height);
		}

		public static bool operator == (SKSize sz1, SKSize sz2)
		{
			return ((sz1.Width == sz2.Width) && (sz1.Height == sz2.Height));
		}

		public static bool operator != (SKSize sz1, SKSize sz2)
		{
			return !(sz1 == sz2);
		}

		public static explicit operator SKPoint (SKSize size)
		{
			return new SKPoint (size.Width, size.Height);
		}

		public static implicit operator SKSize (SKSizeI size)
		{
			return new SKSize (size.Width, size.Height);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is SKSize))
				return false;

			return (this == (SKSize) obj);
		}

		public override int GetHashCode ()
		{
			return (int) width ^ (int) height;
		}

		public override string ToString ()
		{
			return string.Format (
				"{{Width={0}, Height={1}}}", 
				width.ToString (CultureInfo.CurrentCulture),
				height.ToString (CultureInfo.CurrentCulture));
		}

		public static SKSize Add (SKSize sz1, SKSize sz2) => sz1 + sz2;
		
		public static SKSize Subtract (SKSize sz1, SKSize sz2) => sz1 - sz2;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKSizeI
	{
		private int width, height;

		public static readonly SKSizeI Empty;

		public SKSizeI (int width, int height)
		{
			this.width = width;
			this.height = height;
		}
		
		public SKSizeI (SKPointI pt)
		{
			this.width = pt.X;
			this.height = pt.Y;
		}

		public bool IsEmpty => (width == 0) && (height == 0);

		public int Width {
			get { return width; }
			set { width = value; }
		}

		public int Height {
			get { return height; }
			set { height = value; }
		}

		public SKPointI ToPointI ()
		{
			return new SKPointI (width, height);
		}

		public static SKSizeI operator + (SKSizeI sz1, SKSizeI sz2)
		{
			return new SKSizeI (sz1.Width + sz2.Width, sz1.Height + sz2.Height);
		}

		public static SKSizeI operator - (SKSizeI sz1, SKSizeI sz2)
		{
			return new SKSizeI (sz1.Width - sz2.Width, sz1.Height - sz2.Height);
		}

		public static bool operator == (SKSizeI sz1, SKSizeI sz2)
		{
			return ((sz1.Width == sz2.Width) && (sz1.Height == sz2.Height));
		}

		public static bool operator != (SKSizeI sz1, SKSizeI sz2)
		{
			return !(sz1 == sz2);
		}

		public static explicit operator SKPointI (SKSizeI size)
		{
			return new SKPointI (size.Width, size.Height);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is SKSizeI))
				return false;

			return (this == (SKSizeI) obj);
		}

		public override int GetHashCode ()
		{
			return (int) width ^ (int) height;
		}

		public override string ToString ()
		{
			return string.Format (
				"{{Width={0}, Height={1}}}", 
				width.ToString (CultureInfo.CurrentCulture),
				height.ToString (CultureInfo.CurrentCulture));
		}

		public static SKSizeI Add (SKSizeI sz1, SKSizeI sz2) => sz1 + sz2;
		
		public static SKSizeI Subtract (SKSizeI sz1, SKSizeI sz2) => sz1 - sz2;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKRectI {
		public static readonly SKRectI Empty;
		
		private int left, top, right, bottom;
		
		public SKRectI (int left, int top, int right, int bottom)
		{
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}

		public int Left {
			get { return left; }
			set { left = value; }
		}

		public int Top {
			get { return top; }
			set { top = value; }
		}

		public int Right {
			get { return right; }
			set { right = value; }
		}

		public int Bottom {
			get { return bottom; }
			set { bottom = value; }
		}

		public int MidX => left + (Width / 2);

		public int MidY => top + (Height / 2);

		public int Width => right - left;

		public int Height => bottom - top;

		public bool IsEmpty => this == Empty;

		public SKSizeI Size {
			get { return new SKSizeI (Width, Height); }
			set {
				right = left + value.Width;
				bottom = top + value.Height; 
			}
		}

		public SKPointI Location {
			get { return new SKPointI (left, top); }
			set { this = SKRectI.Create (value, Size); }
		}

		public SKRectI Standardized {
			get {
				if (left > right) {
					if (top > bottom) {
						return new SKRectI (right, bottom, left, top);
					} else {
						return new SKRectI (right, top, left, bottom);
					}
				} else {
					if (top > bottom) {
						return new SKRectI (left, bottom, right, top);
					} else {
						return new SKRectI (left, top, right, bottom);
					}
				}
			}
		}

		public SKRectI AspectFit (SKSizeI size) => Truncate (((SKRect)this).AspectFit (size));

		public SKRectI AspectFill (SKSizeI size) => Truncate (((SKRect)this).AspectFill (size));

		public static SKRectI Ceiling (SKRect value) => Ceiling (value, false);

		public static SKRectI Ceiling (SKRect value, bool outwards)
		{
			int x, y, r, b;
			checked {
				x = (int) (outwards && value.Width > 0 ? Math.Floor (value.Left) : Math.Ceiling (value.Left));
				y = (int) (outwards && value.Height > 0 ? Math.Floor (value.Top) : Math.Ceiling (value.Top));
				r = (int) (outwards && value.Width < 0 ? Math.Floor (value.Right) : Math.Ceiling (value.Right));
				b = (int) (outwards && value.Height < 0 ? Math.Floor (value.Bottom) : Math.Ceiling (value.Bottom));
			}

			return new SKRectI (x, y, r, b);
		}

		public static SKRectI Inflate (SKRectI rect, int x, int y)
		{
			SKRectI r = new SKRectI (rect.left, rect.top, rect.right, rect.bottom);
			r.Inflate (x, y);
			return r;
		}

		public void Inflate (SKSizeI size)
		{
			Inflate (size.Width, size.Height);
		}

		public void Inflate (int width, int height)
		{
			left -= width;
			top -= height;
			right += width;
			bottom += height;
		}

		public static SKRectI Intersect (SKRectI a, SKRectI b)
		{
			if (!a.IntersectsWithInclusive (b))
				return Empty;

			return new SKRectI (
				Math.Max (a.left, b.left),
				Math.Max (a.top, b.top),
				Math.Min (a.right, b.right),
				Math.Min (a.bottom, b.bottom));
		}

		public void Intersect (SKRectI rect)
		{
			this = SKRectI.Intersect (this, rect);
		}

		public static SKRectI Round (SKRect value)
		{
			int x, y, r, b;
			checked {
				x = (int) Math.Round (value.Left);
				y = (int) Math.Round (value.Top);
				r = (int) Math.Round (value.Right);
				b = (int) Math.Round (value.Bottom);
			}

			return new SKRectI (x, y, r, b);
		}

		public static SKRectI Floor (SKRect value) => Floor (value, false);

		public static SKRectI Floor (SKRect value, bool inwards)
		{
			int x, y, r, b;
			checked {
				x = (int) (inwards && value.Width > 0 ? Math.Ceiling (value.Left) : Math.Floor (value.Left));
				y = (int) (inwards && value.Height > 0 ? Math.Ceiling (value.Top) : Math.Floor (value.Top));
				r = (int) (inwards && value.Width < 0 ? Math.Ceiling (value.Right) : Math.Floor (value.Right));
				b = (int) (inwards && value.Height < 0 ? Math.Ceiling (value.Bottom) : Math.Floor (value.Bottom));
			}

			return new SKRectI (x, y, r, b);
		}

		public static SKRectI Truncate (SKRect value)
		{
			int x, y, r, b;
			checked {
				x = (int) value.Left;
				y = (int) value.Top;
				r = (int) value.Right;
				b = (int) value.Bottom;
			}

			return new SKRectI (x, y, r, b);
		}

		public static SKRectI Union (SKRectI a, SKRectI b)
		{
			return new SKRectI (
				Math.Min (a.Left, b.Left),
				Math.Min (a.Top, b.Top),
				Math.Max (a.Right, b.Right),
				Math.Max (a.Bottom, b.Bottom));
		}

		public void Union (SKRectI rect)
		{
			this = SKRectI.Union (this, rect);
		}

		public bool Contains (int x, int y)
		{
			return 
				(x >= left) && (x < right) && 
				(y >= top) && (y < bottom);
		}

		public bool Contains (SKPointI pt)
		{
			return Contains (pt.X, pt.Y);
		}

		public bool Contains (SKRectI rect)
		{
			return
				(left <= rect.left) && (right >= rect.right) && 
				(top <= rect.top) && (bottom >= rect.bottom);
		}

		public override bool Equals (object obj)
		{
			if (!(obj is SKRectI))
				return false;

			return (this == (SKRectI) obj);
		}
		
		public override int GetHashCode ()
		{
			return unchecked((int)(
				(((UInt32)Left)) ^ 
				(((UInt32)Top << 13) | ((UInt32)Top >> 19)) ^
				(((UInt32)Width << 26) | ((UInt32)Width >>  6)) ^
				(((UInt32)Height <<  7) | ((UInt32)Height >> 25))));
		}

		public bool IntersectsWith (SKRectI rect)
		{
			return
				!((left >= rect.right) || (right <= rect.left) ||
				  (top >= rect.bottom) || (bottom <= rect.top));
		}

		private bool IntersectsWithInclusive (SKRectI r)
		{
			return
				!((left > r.right) || (right < r.left) ||
				  (top > r.bottom) || (bottom < r.top));
		}
		
		public void Offset (int x, int y)
		{
			left += x;
			top += y;
			right += x;
			bottom += y;
		}

		public void Offset (SKPointI pos)
		{
			Offset (pos.X, pos.Y);
		}

		public override string ToString ()
		{
			return String.Format (
				"{{Left={0},Top={1},Width={2},Height={3}}}",
				Left.ToString (CultureInfo.CurrentCulture),
				Top.ToString (CultureInfo.CurrentCulture),
				Width.ToString (CultureInfo.CurrentCulture),
				Height.ToString (CultureInfo.CurrentCulture));
		}

		public static bool operator == (SKRectI left, SKRectI right)
		{
			return
				(left.left == right.left) && (left.top == right.top) &&
				(left.right == right.right) && (left.bottom == right.bottom);
		}

		public static bool operator != (SKRectI left, SKRectI right)
		{
			return !(left == right);
		}

		public static SKRectI Create (SKSizeI size)
		{
			return SKRectI.Create (SKPointI.Empty.X, SKPointI.Empty.Y, size.Width, size.Height);
		}

		public static SKRectI Create (SKPointI location, SKSizeI size)
		{
			return SKRectI.Create (location.X, location.Y, size.Width, size.Height);
		}

		public static SKRectI Create (int width, int height)
		{
			return new SKRectI (SKPointI.Empty.X, SKPointI.Empty.X, width, height);
		}

		public static SKRectI Create (int x, int y, int width, int height)
		{
			return new SKRectI (x, y, x + width, y + height);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKRect {
		public static readonly SKRect Empty;

		private float left, top, right, bottom;

		public SKRect (float left, float top, float right, float bottom)
		{
			this.left = left;
			this.right = right;
			this.top = top;
			this.bottom = bottom;
		}

		public float Left {
			get { return left; }
			set { left = value; }
		}

		public float Top {
			get { return top; }
			set { top = value; }
		}

		public float Right {
			get { return right; }
			set { right = value; }
		}

		public float Bottom {
			get { return bottom; }
			set { bottom = value; }
		}

		public float MidX => left + (Width / 2f);

		public float MidY => top + (Height / 2f);

		public float Width => right - left;

		public float Height => bottom - top;

		public bool IsEmpty => this == Empty;

		public SKSize Size {
			get { return new SKSize (Width, Height); }
			set {
				right = left + value.Width;
				bottom = top + value.Height; 
			}
		}

		public SKPoint Location {
			get { return new SKPoint (left, top); }
			set { this = SKRect.Create (value, Size); }
		}

		public SKRect Standardized {
			get {
				if (left > right) {
					if (top > bottom) {
						return new SKRect (right, bottom, left, top);
					} else {
						return new SKRect (right, top, left, bottom);
					}
				} else {
					if (top > bottom) {
						return new SKRect (left, bottom, right, top);
					} else {
						return new SKRect (left, top, right, bottom);
					}
				}
			}
		}

		public SKRect AspectFit (SKSize size) => AspectResize (size, true);

		public SKRect AspectFill (SKSize size) => AspectResize (size, false);

		private SKRect AspectResize (SKSize size, bool fit)
		{
			if (size.Width == 0 || size.Height == 0 || Width == 0 || Height == 0)
				return SKRect.Create (MidX, MidY, 0, 0);

			float aspectWidth = size.Width;
			float aspectHeight = size.Height;
			float imgAspect = aspectWidth / aspectHeight;
			float fullRectAspect = Width / Height;

			bool compare = fit ? (fullRectAspect > imgAspect) : (fullRectAspect < imgAspect);
			if (compare) {
				aspectHeight = Height;
				aspectWidth = aspectHeight * imgAspect;
			} else {
				aspectWidth = Width;
				aspectHeight = aspectWidth / imgAspect;
			}
			float aspectLeft = MidX - (aspectWidth / 2f);
			float aspectTop = MidY - (aspectHeight / 2f);

			return SKRect.Create (aspectLeft, aspectTop, aspectWidth, aspectHeight);
		}

		public static SKRect Inflate (SKRect rect, float x, float y)
		{
			var r = new SKRect (rect.left, rect.top, rect.right, rect.bottom);
			r.Inflate (x, y);
			return r;
		}

		public void Inflate (SKSize size)
		{
			Inflate (size.Width, size.Height);
		}

		public void Inflate (float x, float y)
		{
			left -= x;
			top -= y;
			right += x;
			bottom += y;
		}

		public static SKRect Intersect (SKRect a, SKRect b)
		{
			if (!a.IntersectsWithInclusive (b)) {
				return Empty;
			}
			return new SKRect (
				Math.Max (a.left, b.left),
				Math.Max (a.top, b.top),
				Math.Min (a.right, b.right),
				Math.Min (a.bottom, b.bottom));
		}

		public void Intersect (SKRect rect)
		{
			this = SKRect.Intersect (this, rect);
		}

		public static SKRect Union (SKRect a, SKRect b)
		{
			return new SKRect (
				Math.Min (a.left, b.left),
				Math.Min (a.top, b.top),
				Math.Max (a.right, b.right),
				Math.Max (a.bottom, b.bottom));
		}

		public void Union (SKRect rect)
		{
			this = SKRect.Union (this, rect);
		}

		public static bool operator == (SKRect left, SKRect right)
		{
			return
				(left.left == right.left) && (left.top == right.top) &&
				(left.right == right.right) && (left.bottom == right.bottom);
		}

		public static bool operator != (SKRect left, SKRect right)
		{
			return !(left == right);
		}

		public static implicit operator SKRect (SKRectI r)
		{
			return new SKRect (r.Left, r.Top, r.Right, r.Bottom);
		}

		public bool Contains (float x, float y)
		{
			return (x >= left) && (x < right) && (y >= top) && (y < bottom);
		}

		public bool Contains (SKPoint pt)
		{
			return Contains (pt.X, pt.Y);
		}

		public bool Contains (SKRect rect)
		{
			return 
				(left <= rect.left) && (right >= rect.right) && 
				(top <= rect.top) && (bottom >= rect.bottom);
		}
		
		public override bool Equals (object obj)
		{
			if (!(obj is SKRect))
				return false;

			return this == (SKRect) obj;
		}

		public override int GetHashCode ()
		{
			return unchecked((int)(
				(((UInt32)Left)) ^ 
				(((UInt32)Top << 13) | ((UInt32)Top >> 19)) ^
				(((UInt32)Width << 26) | ((UInt32)Width >>  6)) ^
				(((UInt32)Height <<  7) | ((UInt32)Height >> 25))));
		}

		public bool IntersectsWith (SKRect rect)
		{
			return !((left >= rect.right) || (right <= rect.left) ||
					(top >= rect.bottom) || (bottom <= rect.top));
		}

		public bool IntersectsWithInclusive (SKRect rect)
		{
			return !((left > rect.right) || (right < rect.left) ||
					 (top > rect.bottom) || (bottom < rect.top));
		}
		
		public void Offset (float x, float y)
		{
			left += x;
			top += y;
			right += x;
			bottom += y;
		}

		public void Offset (SKPoint pos)
		{
			Offset (pos.X, pos.Y);
		}

		public override string ToString ()
		{
			return String.Format (
				"{{Left={0},Top={1},Width={2},Height={3}}}",
				Left.ToString (CultureInfo.CurrentCulture),
				Top.ToString (CultureInfo.CurrentCulture),
				Width.ToString (CultureInfo.CurrentCulture),
				Height.ToString (CultureInfo.CurrentCulture));
		}

		public static SKRect Create (SKPoint location, SKSize size)
		{
			return SKRect.Create (location.X, location.Y, size.Width, size.Height);
		}

		public static SKRect Create (SKSize size)
		{
			return SKRect.Create (SKPoint.Empty, size);
		}

		public static SKRect Create (float width, float height)
		{
			return new SKRect (SKPoint.Empty.X, SKPoint.Empty.Y, width, height);
		}

		public static SKRect Create (float x, float y, float width, float height)
		{
			return new SKRect (x, y, x + width, y + height);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKFontMetrics
	{
		private const uint flagsUnderlineThicknessIsValid = (1U << 0);
		private const uint flagsUnderlinePositionIsValid  = (1U << 1);
		private const uint flagsStrikeoutThicknessIsValid = (1U << 2);
		private const uint flagsStrikeoutPositionIsValid  = (1U << 3);

		uint flags;                     // Bit field to identify which values are unknown
		float top;                      // The greatest distance above the baseline for any glyph (will be <= 0)
		float ascent;                   // The recommended distance above the baseline (will be <= 0)
		float descent;                  // The recommended distance below the baseline (will be >= 0)
		float bottom;                   // The greatest distance below the baseline for any glyph (will be >= 0)
		float leading;                  // The recommended distance to add between lines of text (will be >= 0)
		float avgCharWidth;             // the average character width (>= 0)
		float maxCharWidth;             // the max character width (>= 0)
		float xMin;                     // The minimum bounding box x value for all glyphs
		float xMax;                     // The maximum bounding box x value for all glyphs
		float xHeight;                  // The height of an 'x' in px, or 0 if no 'x' in face
		float capHeight;                // The cap height (> 0), or 0 if cannot be determined.
		float underlineThickness;       // underline thickness, or 0 if cannot be determined
		float underlinePosition;        // underline position, or 0 if cannot be determined
		float strikeoutThickness;
		float strikeoutPosition;

		public float Top
		{
			get { return top; }
		}

		public float Ascent
		{
			get { return ascent; }
		}

		public float Descent
		{
			get { return descent; }
		}

		public float Bottom
		{
			get { return bottom; }
		}

		public float Leading
		{
			get { return leading; }
		}

		public float AverageCharacterWidth
		{
			get { return avgCharWidth; }
		}

		public float MaxCharacterWidth
		{
			get { return maxCharWidth; }
		}

		public float XMin
		{
			get { return xMin; }
		}

		public float XMax
		{
			get { return xMax; }
		}

		public float XHeight
		{
			get { return xHeight; }
		}

		public float CapHeight
		{
			get { return capHeight; }
		}

		public float? UnderlineThickness => GetIfValid(underlineThickness, flagsUnderlineThicknessIsValid);
		public float? UnderlinePosition => GetIfValid(underlinePosition, flagsUnderlinePositionIsValid);
		public float? StrikeoutThickness => GetIfValid(strikeoutThickness, flagsStrikeoutThicknessIsValid);
		public float? StrikeoutPosition => GetIfValid(strikeoutPosition, flagsStrikeoutPositionIsValid);

		private float? GetIfValid(float value, uint flag)
		{
			if ((flags & flag) == flag)
				return value;
			else
				return null;
		}
	}
	
	public enum SKPathOp {
		Difference,
		Intersect,
		Union,
		Xor,
		ReverseDifference,
	};

	public enum SKPathConvexity {
		Unknown,
		Convex,
		Concave,
	};

	public enum SKLatticeRectType {
		Default,
		Transparent,
		FixedColor,
	};

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct SKLatticeInternal {
		public int* fXDivs;
		public int* fYDivs;
		public SKLatticeRectType* fRectTypes;
		public int fXCount;
		public int fYCount;
		public SKRectI* fBounds;
		public SKColor* fColors;
	}

	public struct SKLattice {
		public int[] XDivs { get; set; }
		public int[] YDivs { get; set; }
		public SKLatticeRectType[] RectTypes { get; set; }
		public SKRectI? Bounds { get; set; }
		public SKColor[] Colors { get; set; }
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct SKTimeDateTimeInternal {
		public Int16 TimeZoneMinutes;
		public UInt16 Year;
		public Byte Month;
		public Byte DayOfWeek;
		public Byte Day;
		public Byte Hour;
		public Byte Minute;
		public Byte Second;

		public static SKTimeDateTimeInternal Create (DateTime datetime) {
			var zone = datetime.Hour - datetime.ToUniversalTime().Hour;
			return new SKTimeDateTimeInternal {
				TimeZoneMinutes = (Int16)(zone * 60),
				Year = (UInt16)datetime.Year,
				Month = (Byte)datetime.Month,
				DayOfWeek = (Byte)datetime.DayOfWeek,
				Day = (Byte)datetime.Day,
				Hour = (Byte)datetime.Hour,
				Minute = (Byte)datetime.Minute,
				Second = (Byte)datetime.Second
			};
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct SKDocumentPdfMetadataInternal {
		public sk_string_t Title;
		public sk_string_t Author;
		public sk_string_t Subject;
		public sk_string_t Keywords;
		public sk_string_t Creator;
		public sk_string_t Producer;
		public SKTimeDateTimeInternal* Creation;
		public SKTimeDateTimeInternal* Modified;
	}

	public struct SKDocumentPdfMetadata {
		public string Title { get; set; }
		public string Author { get; set; }
		public string Subject { get; set; }
		public string Keywords { get; set; }
		public string Creator { get; set; }
		public string Producer { get; set; }
		public DateTime? Creation { get; set; }
		public DateTime? Modified { get; set; }
	}

	public enum SKColorSpaceGamut {
		Srgb,
		AdobeRgb,
		Dcip3D65,
		Rec2020,
	}

	[Flags]
	public enum SKColorSpaceFlags {
		None = 0,
		NonLinearBlending = 0x1,
	}

	public enum SKColorSpaceRenderTargetGamma {
		Linear,
		Srgb,
	}

	public enum SKMaskFormat {
		BW,
		A8,
		ThreeD,
		Argb32,
		Lcd16,
	}

	[Flags]
	public enum SKMatrix44TypeMask {
		Identity = 0,
		Translate = 0x01,
		Scale = 0x02,
		Affine = 0x04,
		Perspective = 0x08 
	}

	public enum SKVertexMode {
		Triangles,
		TriangleStrip,
		TriangleFan,
	}

	public enum SKImageCachingHint {
		Allow,
		Disallow,
	}

	public enum SKHighContrastConfigInvertStyle {
		NoInvert,
		InvertBrightness,
		InvertLightness,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKHighContrastConfig {
		private byte fGrayscale;
		private SKHighContrastConfigInvertStyle fInvertStyle;
		private float fContrast;
		
		public static readonly SKHighContrastConfig Default;

		static SKHighContrastConfig ()
		{
			Default = new SKHighContrastConfig (false, SKHighContrastConfigInvertStyle.NoInvert, 0.0f);
		}

		public SKHighContrastConfig (bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast)
		{
			fGrayscale = grayscale ? (byte)1 : (byte)0;
			fInvertStyle = invertStyle;
			fContrast = contrast;
		}

		public bool Grayscale { 
			get { return fGrayscale != 0; }
			set { fGrayscale = value ? (byte)1 : (byte)0; }
		}
		public SKHighContrastConfigInvertStyle InvertStyle { 
			get { return fInvertStyle; }
			set { fInvertStyle = value; }
		}
		public float Contrast { 
			get { return fContrast; }
			set { fContrast = value; }
		}

		public bool IsValid =>
			(int)fInvertStyle >= (int)SKHighContrastConfigInvertStyle.NoInvert &&
			(int)fInvertStyle <= (int)SKHighContrastConfigInvertStyle.InvertLightness &&
			fContrast >= -1.0 &&
			fContrast <= 1.0;
	}

	[Flags]
	public enum SKBitmapAllocFlags : uint {
		None = 0,
		ZeroPixels = 1 << 0,
	}

	[Flags]
	public enum SKPngEncoderFilterFlags {
		NoFilters  = 0x00,
		None       = 0x08,
		Sub        = 0x10,
		Up         = 0x20,
		Avg        = 0x40,
		Paeth      = 0x80,
		AllFilters = None | Sub | Up | Avg | Paeth,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKPngEncoderOptions {
		private SKPngEncoderFilterFlags fFilterFlags;
		private int fZLibLevel;
		private SKTransferFunctionBehavior fUnpremulBehavior;
		private IntPtr fComments; // TODO: get and set comments

		public static readonly SKPngEncoderOptions Default;

		static SKPngEncoderOptions ()
		{
			Default = new SKPngEncoderOptions (SKPngEncoderFilterFlags.AllFilters, 6, SKTransferFunctionBehavior.Respect);
		}

		public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel)
		{
			fFilterFlags = filterFlags;
			fZLibLevel = zLibLevel;
			fUnpremulBehavior = SKTransferFunctionBehavior.Respect;
			fComments = IntPtr.Zero;
		}

		public SKPngEncoderOptions (SKPngEncoderFilterFlags filterFlags, int zLibLevel, SKTransferFunctionBehavior unpremulBehavior)
		{
			fFilterFlags = filterFlags;
			fZLibLevel = zLibLevel;
			fUnpremulBehavior = unpremulBehavior;
			fComments = IntPtr.Zero;
		}

		public SKPngEncoderFilterFlags FilterFlags { 
			get { return fFilterFlags; }
			set { fFilterFlags = value; }
		}
		public int ZLibLevel { 
			get { return fZLibLevel; }
			set { fZLibLevel = value; }
		}
		public SKTransferFunctionBehavior UnpremulBehavior { 
			get { return fUnpremulBehavior; }
			set { fUnpremulBehavior = value; }
		}
	}

	public enum SKJpegEncoderDownsample {
		Downsample420,
		Downsample422,
		Downsample444,
	}

	public enum SKJpegEncoderAlphaOption {
		Ignore,
		BlendOnBlack,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKJpegEncoderOptions {
		private int fQuality;
		private SKJpegEncoderDownsample fDownsample;
		private SKJpegEncoderAlphaOption fAlphaOption;
		private SKTransferFunctionBehavior fBlendBehavior;

		public static readonly SKJpegEncoderOptions Default;

		static SKJpegEncoderOptions ()
		{
			Default = new SKJpegEncoderOptions (100, SKJpegEncoderDownsample.Downsample420, SKJpegEncoderAlphaOption.Ignore, SKTransferFunctionBehavior.Respect);
		}

		public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption)
		{
			fQuality = quality;
			fDownsample = downsample;
			fAlphaOption = alphaOption;
			fBlendBehavior = SKTransferFunctionBehavior.Respect;
		}

		public SKJpegEncoderOptions (int quality, SKJpegEncoderDownsample downsample, SKJpegEncoderAlphaOption alphaOption, SKTransferFunctionBehavior blendBehavior)
		{
			fQuality = quality;
			fDownsample = downsample;
			fAlphaOption = alphaOption;
			fBlendBehavior = blendBehavior;
		}

		public int Quality { 
			get { return fQuality; }
			set { fQuality = value; }
		}
		public SKJpegEncoderDownsample Downsample { 
			get { return fDownsample; }
			set { fDownsample = value; }
		}
		public SKJpegEncoderAlphaOption AlphaOption { 
			get { return fAlphaOption; }
			set { fAlphaOption = value; }
		}
		public SKTransferFunctionBehavior BlendBehavior { 
			get { return fBlendBehavior; }
			set { fBlendBehavior = value; }
		}
	}

	public enum SKWebpEncoderCompression {
		Lossy,
		Lossless,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKWebpEncoderOptions {
		private SKWebpEncoderCompression fCompression;
		private float fQuality;
		private SKTransferFunctionBehavior fUnpremulBehavior;

		public static readonly SKWebpEncoderOptions Default;

		static SKWebpEncoderOptions ()
		{
			Default = new SKWebpEncoderOptions (SKWebpEncoderCompression.Lossy, 100, SKTransferFunctionBehavior.Respect);
		}

		public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality)
		{
			fCompression = compression;
			fQuality = quality;
			fUnpremulBehavior = SKTransferFunctionBehavior.Respect;
		}

		public SKWebpEncoderOptions (SKWebpEncoderCompression compression, float quality, SKTransferFunctionBehavior unpremulBehavior)
		{
			fCompression = compression;
			fQuality = quality;
			fUnpremulBehavior = unpremulBehavior;
		}

		public SKWebpEncoderCompression Compression { 
			get { return fCompression; }
			set { fCompression = value; }
		}
		public float Quality { 
			get { return fQuality; }
			set { fQuality = value; }
		}
		public SKTransferFunctionBehavior UnpremulBehavior { 
			get { return fUnpremulBehavior; }
			set { fUnpremulBehavior = value; }
		}
	}

	public enum SKRoundRectType {
		Empty,
		Rect,
		Oval,
		Simple,
		NinePatch,
		Complex,
	}

	public enum SKRoundRectCorner {
		UpperLeft,
		UpperRight,
		LowerRight,
		LowerLeft,
	}
}
