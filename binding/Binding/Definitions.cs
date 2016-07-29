//
// Skia Definitions, enumerations and interop structures
//
// Author:
//    Miguel de Icaza (miguel@xamarin.com)
//
// Copyright 2016 Xamarin Inc
//
// TODO: 
//   Add more ToString, operators, convenience methods to various structures here (point, rect, etc)
//   Sadly, the Rectangles are not binary compatible with the System.Drawing ones.
//
// SkMatrix could benefit from bringing some of the operators defined in C++
//
// Augmented primitives come from Mono:
// Author:
//   Mike Kestner (mkestner@speakeasy.net)
//
// Copyright (C) 2001 Mike Kestner
// Copyright (C) 2004,2006 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
using System;
using System.Runtime.InteropServices;
using System.Globalization;
 	
namespace SkiaSharp
{
	public enum SKCodecResult {
		Success,
		IncompleteInput,
		InvalidConversion,
		InvalidScale,
		InvalidParameters,
		InvalidInput,
		CouldNotRewind,
		Unimplemented,
	}

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

	public enum SKEncodedFormat {
		Unknown,
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
	}

	public partial struct SKColor {

		internal SKColor (uint value)
		{
			color = value;
		}

		uint color;
		public SKColor (byte red, byte green, byte blue, byte alpha)
		{
			color = (uint)((alpha << 24) | (red << 16) | (green << 8) | blue);
		}

		public SKColor (byte red, byte green, byte blue)
		{
			color = (uint)(0xff000000u | (red << 16) | (green << 8) | blue);
		}

		public SKColor WithAlpha (byte alpha)
		{
			return new SKColor (Red, Green, Blue, alpha);
		}

		public byte Alpha => (byte)((color >> 24) & 0xff);
		public byte Red => (byte)((color >> 16) & 0xff);
		public byte Green => (byte)((color >> 8) & 0xff);
		public byte Blue => (byte)((color) & 0xff);

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "#{0:x2}{1:x2}{2:x2}{3:x2}",  Alpha, Red, Green, Blue);
		}

		public override bool Equals (object other)
		{
			if (!(other is SKColor))
				return false;

			var c = (SKColor) other;
			return c.color == this.color;
		}

		public override int GetHashCode ()
		{
			return (int) color;
		}
	}

	[Flags]
	public enum SKTypefaceStyle {
		Normal     = 0,
		Bold       = 0x01,
		Italic     = 0x02,
		BoldItalic = 0x03
	}

	public enum SKFontStyleWeight {
		Thin        = 100,
		ExtraLight  = 200,
		Light       = 300,
		Normal      = 400,
		Medium      = 500,
		SemiBold    = 600,
		Bold        = 700,
		ExtraBold   = 800,
		Black       = 900
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
		UltaExpanded     = 9
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

	public enum SKColorType {
		Unknown,
		Alpha8,
		Rgb565,
		Argb4444,
		Rgba8888,
		Bgra8888,
		Index8,
		Gray8,
		RgbaF16
	}

	public enum SKColorProfileType {
		Linear,
		SRGB
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

	public enum SKXferMode {
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

	public enum SKClipType {
		Intersect, Difference 
	}

	public enum SKPixelGeometry {
		Unknown,
		RgbHorizontal,
		BgrHorizontal,
		RgbVertical,
		BgrVertical
	}

	public enum SKEncoding {
		Utf8, Utf16, Utf32
	}

	public static class SkiaExtensions {
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
		Mitter, Round, Bevel
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

	public enum SKRegionOperation
	{
		Difference,
		Intersect,
		Union,
		XOR,
		ReverseDifference,
		Replace,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKImageInfo {
		public static SKImageInfo Empty;
		public static SKColorType PlatformColorType;

		static SKImageInfo ()
		{
#if WINDOWS_UWP
			var isUnix = false;
#else
			var isUnix = Environment.OSVersion.Platform == PlatformID.MacOSX || Environment.OSVersion.Platform == PlatformID.Unix;
#endif
			if (isUnix) {
				// Unix depends on the CPU endianess, but we use RGBA
				PlatformColorType = SKColorType.Rgba8888;
			} else {
				// Windows is always BGRA
				PlatformColorType = SKColorType.Bgra8888;
			}
		}

		public int Width;
		public int Height;
		public SKColorType ColorType;
		public SKAlphaType AlphaType;

		public SKImageInfo (int width, int height)
		{
			this.Width = width;
			this.Height = height;
			this.ColorType = PlatformColorType;
			this.AlphaType = SKAlphaType.Premul;
		}

		public SKImageInfo (int width, int height, SKColorType colorType)
		{
			this.Width = width;
			this.Height = height;
			this.ColorType = colorType;
			this.AlphaType = SKAlphaType.Premul;
		}

		public SKImageInfo (int width, int height, SKColorType colorType, SKAlphaType alphaType)
		{
			this.Width = width;
			this.Height = height;
			this.ColorType = colorType;
			this.AlphaType = alphaType;
		}

		public int BytesPerPixel {
			get {
				switch (ColorType) {
				case SKColorType.Unknown:
					return 0;
				case SKColorType.Alpha8:
				case SKColorType.Index8:
				case SKColorType.Gray8:
					return 1;
				case SKColorType.Rgb565:
				case SKColorType.Argb4444:
					return 2;
				case SKColorType.Bgra8888:
				case SKColorType.Rgba8888:
					return 4;
				case SKColorType.RgbaF16:
					return 8;
				}
				throw new ArgumentOutOfRangeException ("ColorType");
			}
		}

		public int BytesSize {
			get { return Width * Height * BytesPerPixel; }
		}

		public int RowBytes {
			get { return Width * BytesPerPixel; }
		}

		public bool IsEmpty {
			get { return Width <= 0 || Height <= 0; }
		}

		public bool IsOpaque {
			get { return AlphaType == SKAlphaType.Opaque; }
		}

		public SKPointI Size {
			get { return new SKPointI (Width, Height); }
		}

		public SKRectI Rect {
			get { return SKRectI.Create (Width, Height); }
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKSurfaceProps {
		public SKPixelGeometry PixelGeometry;
	}

	public enum SKZeroInitialized {
		Yes,
		No,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKCodecOptions {
		public SKZeroInitialized ZeroInitialized;
		public SKRectI Subset;
		public bool HasSubset;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKPoint {
		private float x, y;
		// -----------------------
		// Public Shared Members
		// -----------------------

		/// <summary>
		///	Empty Shared Field
		/// </summary>
		///
		/// <remarks>
		///	An uninitialized SKPoint Structure.
		/// </remarks>
		
		public static readonly SKPoint Empty;

		/// <summary>
		///	Addition Operator
		/// </summary>
		///
		/// <remarks>
		///	Translates a SKPoint using the Width and Height
		///	properties of the given SKSize.
		/// </remarks>

		public static SKPoint operator + (SKPoint pt, SKSizeI sz)
		{
			return new SKPoint (pt.X + sz.Width, pt.Y + sz.Height);
		}
		public static SKPoint operator + (SKPoint pt, SKSize sz)
		{
			return new SKPoint (pt.X + sz.Width, pt.Y + sz.Height);
		}
		
		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two SKPoint objects. The return value is
		///	based on the equivalence of the X and Y properties 
		///	of the two points.
		/// </remarks>

		public static bool operator == (SKPoint left, SKPoint right)
		{
			return ((left.X == right.X) && (left.Y == right.Y));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two SKPoint objects. The return value is
		///	based on the equivalence of the X and Y properties 
		///	of the two points.
		/// </remarks>

		public static bool operator != (SKPoint left, SKPoint right)
		{
			return ((left.X != right.X) || (left.Y != right.Y));
		}
		
		/// <summary>
		///	Subtraction Operator
		/// </summary>
		///
		/// <remarks>
		///	Translates a SKPoint using the negation of the Width 
		///	and Height properties of the given SKSize.
		/// </remarks>

		public static SKPoint operator - (SKPoint pt, SKSizeI sz)
		{
			return new SKPoint (pt.X - sz.Width, pt.Y - sz.Height);
		}
		public static SKPoint operator - (SKPoint pt, SKSize sz)
		{
			return new SKPoint (pt.X - sz.Width, pt.Y - sz.Height);
		}
		
		// -----------------------
		// Public Constructor
		// -----------------------

		/// <summary>
		///	SKPoint Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a SKPoint from a specified x,y coordinate pair.
		/// </remarks>
		
		public SKPoint (float x, float y)
		{
			this.x = x;
			this.y = y;
		}

		// -----------------------
		// Public Instance Members
		// -----------------------

		/// <summary>
		///	IsEmpty Property
		/// </summary>
		///
		/// <remarks>
		///	Indicates if both X and Y are zero.
		/// </remarks>
		
		public bool IsEmpty {
			get {
				return ((x == 0.0) && (y == 0.0));
			}
		}

		/// <summary>
		///	X Property
		/// </summary>
		///
		/// <remarks>
		///	The X coordinate of the SKPoint.
		/// </remarks>
		
		public float X {
			get {
				return x;
			}
			set {
				x = value;
			}
		}

		/// <summary>
		///	Y Property
		/// </summary>
		///
		/// <remarks>
		///	The Y coordinate of the SKPoint.
		/// </remarks>
		
		public float Y {
			get {
				return y;
			}
			set {
				y = value;
			}
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this SKPoint and another object.
		/// </remarks>
		
		public override bool Equals (object obj)
		{
			if (!(obj is SKPoint))
				return false;

			return (this == (SKPoint) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode ()
		{
			return (int) x ^ (int) y;
		}

		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the SKPoint as a string in coordinate notation.
		/// </remarks>
		
		public override string ToString ()
		{
			return String.Format ("{{X={0}, Y={1}}}", x.ToString (CultureInfo.CurrentCulture),
				y.ToString (CultureInfo.CurrentCulture));
		}

		public static SKPoint Add (SKPoint pt, SKSizeI sz)
		{
			return new SKPoint (pt.X + sz.Width, pt.Y + sz.Height);
		}
		
		public static SKPoint Add (SKPoint pt, SKSize sz)
		{
			return new SKPoint (pt.X + sz.Width, pt.Y + sz.Height);
		}

		public static SKPoint Subtract (SKPoint pt, SKSizeI sz)
		{
			return new SKPoint (pt.X - sz.Width, pt.Y - sz.Height);
		}

		public static SKPoint Subtract (SKPoint pt, SKSize sz)
		{
			return new SKPoint (pt.X - sz.Width, pt.Y - sz.Height);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKPointI {
		// Private x and y coordinate fields.
		private int x, y;

		// -----------------------
		// Public Shared Members
		// -----------------------

		/// <summary>
		///	Empty Shared Field
		/// </summary>
		///
		/// <remarks>
		///	An uninitialized Point Structure.
		/// </remarks>
		
		public static readonly SKPointI Empty;

		/// <summary>
		///	Ceiling Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Point structure from a PointF structure by
		///	taking the ceiling of the X and Y properties.
		/// </remarks>
		
		public static SKPointI Ceiling (SKPoint value)
		{
			int x, y;
			checked {
				x = (int) Math.Ceiling (value.X);
				y = (int) Math.Ceiling (value.Y);
			}

			return new SKPointI (x, y);
		}

		/// <summary>
		///	Round Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Point structure from a PointF structure by
		///	rounding the X and Y properties.
		/// </remarks>
		
		public static SKPointI Round (SKPoint value)
		{
			int x, y;
			checked {
				x = (int) Math.Round (value.X);
				y = (int) Math.Round (value.Y);
			}

			return new SKPointI (x, y);
		}

		/// <summary>
		///	Truncate Shared Method
		/// </summary>
		///
		/// <remarks>
		///	Produces a Point structure from a PointF structure by
		///	truncating the X and Y properties.
		/// </remarks>
		
		// LAMESPEC: Should this be floor, or a pure cast to int?

		public static SKPointI Truncate (SKPoint value)
		{
			int x, y;
			checked {
				x = (int) value.X;
				y = (int) value.Y;
			}

			return new SKPointI (x, y);
		}

		/// <summary>
		///	Addition Operator
		/// </summary>
		///
		/// <remarks>
		///	Translates a Point using the Width and Height
		///	properties of the given <typeref>Size</typeref>.
		/// </remarks>

		public static SKPointI operator + (SKPointI pt, SKSizeI sz)
		{
			return new SKPointI (pt.X + sz.Width, pt.Y + sz.Height);
		}
		
		/// <summary>
		///	Equality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Point objects. The return value is
		///	based on the equivalence of the X and Y properties 
		///	of the two points.
		/// </remarks>

		public static bool operator == (SKPointI left, SKPointI right)
		{
			return ((left.X == right.X) && (left.Y == right.Y));
		}
		
		/// <summary>
		///	Inequality Operator
		/// </summary>
		///
		/// <remarks>
		///	Compares two Point objects. The return value is
		///	based on the equivalence of the X and Y properties 
		///	of the two points.
		/// </remarks>

		public static bool operator != (SKPointI left, SKPointI right)
		{
			return ((left.X != right.X) || (left.Y != right.Y));
		}
		
		/// <summary>
		///	Subtraction Operator
		/// </summary>
		///
		/// <remarks>
		///	Translates a Point using the negation of the Width 
		///	and Height properties of the given Size.
		/// </remarks>

		public static SKPointI operator - (SKPointI pt, SKSizeI sz)
		{
			return new SKPointI (pt.X - sz.Width, pt.Y - sz.Height);
		}
		
		/// <summary>
		///	Point to Size Conversion
		/// </summary>
		///
		/// <remarks>
		///	Returns a Size based on the Coordinates of a given 
		///	Point. Requires explicit cast.
		/// </remarks>

		public static explicit operator SKSizeI (SKPointI p)
		{
			return new SKSizeI (p.X, p.Y);
		}

		/// <summary>
		///	Point to PointF Conversion
		/// </summary>
		///
		/// <remarks>
		///	Creates a PointF based on the coordinates of a given 
		///	Point. No explicit cast is required.
		/// </remarks>

		public static implicit operator SKPoint (SKPointI p)
		{
			return new SKPoint (p.X, p.Y);
		}


		// -----------------------
		// Public Constructors
		// -----------------------

		/// <summary>
		///	Point Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a Point from a Size value.
		/// </remarks>
		
		public SKPointI (SKSizeI sz)
		{
			x = sz.Width;
			y = sz.Height;
		}

		/// <summary>
		///	Point Constructor
		/// </summary>
		///
		/// <remarks>
		///	Creates a Point from a specified x,y coordinate pair.
		/// </remarks>
		
		public SKPointI (int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		// -----------------------
		// Public Instance Members
		// -----------------------

		/// <summary>
		///	IsEmpty Property
		/// </summary>
		///
		/// <remarks>
		///	Indicates if both X and Y are zero.
		/// </remarks>
		
		public bool IsEmpty {
			get {
				return ((x == 0) && (y == 0));
			}
		}

		/// <summary>
		///	X Property
		/// </summary>
		///
		/// <remarks>
		///	The X coordinate of the Point.
		/// </remarks>
		
		public int X {
			get {
				return x;
			}
			set {
				x = value;
			}
		}

		/// <summary>
		///	Y Property
		/// </summary>
		///
		/// <remarks>
		///	The Y coordinate of the Point.
		/// </remarks>
		
		public int Y {
			get {
				return y;
			}
			set {
				y = value;
			}
		}

		/// <summary>
		///	Equals Method
		/// </summary>
		///
		/// <remarks>
		///	Checks equivalence of this Point and another object.
		/// </remarks>
		
		public override bool Equals (object obj)
		{
			if (!(obj is SKPointI))
				return false;

			return (this == (SKPointI) obj);
		}

		/// <summary>
		///	GetHashCode Method
		/// </summary>
		///
		/// <remarks>
		///	Calculates a hashing value.
		/// </remarks>
		
		public override int GetHashCode ()
		{
			return x^y;
		}

		/// <summary>
		///	Offset Method
		/// </summary>
		///
		/// <remarks>
		///	Moves the Point a specified distance.
		/// </remarks>

		public void Offset (int dx, int dy)
		{
			x += dx;
			y += dy;
		}
		
		/// <summary>
		///	ToString Method
		/// </summary>
		///
		/// <remarks>
		///	Formats the Point as a string in coordinate notation.
		/// </remarks>
		
		public override string ToString ()
		{
			return string.Format ("{{X={0},Y={1}}}", x.ToString (CultureInfo.InvariantCulture), 
				y.ToString (CultureInfo.InvariantCulture));
		}
		public static SKPointI Add (SKPointI pt, SKSizeI sz)
		{
			return new SKPointI (pt.X + sz.Width, pt.Y + sz.Height);
		}

		public void Offset (SKPointI p)
		{
			Offset (p.X, p.Y);
		}

		public static SKPointI Subtract (SKPointI pt, SKSizeI sz)
		{
			return new SKPointI (pt.X - sz.Width, pt.Y - sz.Height);
		}
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

		public static bool operator == (SKPoint3 left, SKPoint3 right)
		{
			return ((left.x == right.x) && (left.y == right.y) && (left.z == right.z));
		}
		
		public static bool operator != (SKPoint3 left, SKPoint3 right)
		{
			return ((left.x != right.x) || (left.y != right.y) || (left.z != right.z));
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
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKSize
	{
		public float Width, Height;
		public SKSize(float width, float height)
		{
			Width = width;
			Height = height;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKSizeI
	{
		public int Width, Height;
		public SKSizeI(int width, int height)
		{
			Width = width;
			Height = height;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKRectI {
		public int Left, Top, Right, Bottom;
		public SKRectI (int left, int top, int right, int bottom)
		{
			Left = left;
			Right = right;
			Top = top;
			Bottom = bottom;
		}

		public static SKRectI Create (int width, int height)
		{
			return new SKRectI (0, 0, width, height);
		}

		public static SKRectI Create (int x, int y, int width, int height)
		{
			return new SKRectI (x, y, x + width, y + height);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKRect {
		public float Left, Top, Right, Bottom;
		public SKRect (float left, float top, float right, float bottom)
		{
			Left = left;
			Right = right;
			Top = top;
			Bottom = bottom;
		}

		public static SKRect Create (float width, float height)
		{
			return new SKRect (0, 0, width, height);
		}

		public static SKRect Create (float x, float y, float width, float height)
		{
			return new SKRect (x, y, x + width, y + height);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKMatrix {
		public float ScaleX, SkewX, TransX;
		public float SkewY, ScaleY, TransY;
		public float Persp0, Persp1, Persp2;

#if OPTIMIZED_SKMATRIX

		//
		// If we manage to get an sk_matrix_t that contains the extra
		// the fTypeMask flag, we could accelerate various operations
		// as well, as this caches state of what is needed to be done.
		//
	
		[Flags]
		enum Mask : uint {
			Identity = 0,
			Translate = 1,
			Scale = 2,
			Affine = 4,
			Perspective = 8,
			RectStaysRect = 0x10,
			OnlyPerspectiveValid = 0x40,
			Unknown = 0x80,
			OrableMasks = Translate | Scale | Affine | Perspective,
			AllMasks = OrableMasks | RectStaysRect
		}
		Mask typeMask;

		Mask GetMask ()
		{
			if (typeMask.HasFlag (Mask.Unknown))
				typeMask = (Mask) SkiaApi.sk_matrix_get_type (ref this);

		        // only return the public masks
			return (Mask) ((uint)typeMask & 0xf);
		}
#endif

		static float sdot (float a, float b, float c, float d) => a * b + c * d;
		static float scross(float a, float b, float c, float d) => a * b - c * d;

		public static SKMatrix MakeIdentity ()
		{
			return new SKMatrix () { ScaleX = 1, ScaleY = 1, Persp2 = 1
#if OPTIMIZED_SKMATRIX
					, typeMask = Mask.Identity | Mask.RectStaysRect
#endif
                        };
		}

		public void SetScaleTranslate (float sx, float sy, float tx, float ty)
		{
			ScaleX = sx;
			SkewX = 0;
			TransX = tx;

			SkewY = 0;
			ScaleY = sy;
			TransY = ty;

			Persp0 = 0;
			Persp1 = 0;
			Persp2 = 1;

#if OPTIMIZED_SKMATRIX
			typeMask = Mask.RectStaysRect | 
				((sx != 1 || sy != 1) ? Mask.Scale : 0) |
				((tx != 0 || ty != 0) ? Mask.Translate : 0);
#endif
		}

		public static SKMatrix MakeScale (float sx, float sy)
		{
			if (sx == 1 && sy == 1)
				return MakeIdentity ();
			return new SKMatrix () { ScaleX = sx, ScaleY = sy, Persp2 = 1, 
#if OPTIMIZED_SKMATRIX
typeMask = Mask.Scale | Mask.RectStaysRect
#endif
			};
				
		}

		/// <summary>
		/// Set the matrix to scale by sx and sy, with a pivot point at (px, py).
		/// The pivot point is the coordinate that should remain unchanged by the
		/// specified transformation.
		public static SKMatrix MakeScale (float sx, float sy, float pivotX, float pivotY)
		{
			if (sx == 1 && sy == 1)
				return MakeIdentity ();
			float tx = pivotX - sx * pivotX;
			float ty = pivotY - sy * pivotY;

#if OPTIMIZED_SKMATRIX
			Mask mask = Mask.RectStaysRect | 
				((sx != 1 || sy != 1) ? Mask.Scale : 0) |
				((tx != 0 || ty != 0) ? Mask.Translate : 0);
#endif
			return new SKMatrix () { 
				ScaleX = sx, ScaleY = sy, 
				TransX = tx,
				TransY = ty,
				Persp2 = 1,
#if OPTIMIZED_SKMATRIX
				typeMask = mask
#endif
			};
		}

		public static SKMatrix MakeTranslation (float dx, float dy)
		{
			if (dx == 0 && dy == 0)
				return MakeIdentity ();
			
			return new SKMatrix () { 
				ScaleX = 1, ScaleY = 1,
				TransX = dx, TransY = dy,
				Persp2 = 1,
#if OPTIMIZED_SKMATRIX
				typeMask = Mask.Translate | Mask.RectStaysRect
#endif
			};
		}

		public static SKMatrix MakeRotation (float radians)
		{
			var sin = (float) Math.Sin (radians);
			var cos = (float) Math.Cos (radians);

			var matrix = new SKMatrix ();
			SetSinCos (ref matrix, sin, cos);
			return matrix;
		}

		public static SKMatrix MakeRotation (float radians, float pivotx, float pivoty)
		{
			var sin = (float) Math.Sin (radians);
			var cos = (float) Math.Cos (radians);

			var matrix = new SKMatrix ();
			SetSinCos (ref matrix, sin, cos, pivotx, pivoty);
			return matrix;
		}

		const float degToRad = (float)System.Math.PI / 180.0f;
		
		public static SKMatrix MakeRotationDegrees (float degrees)
		{
			return MakeRotation (degrees * degToRad);
		}

		public static SKMatrix MakeRotationDegrees (float degrees, float pivotx, float pivoty)
		{
			return MakeRotation (degrees * degToRad, pivotx, pivoty);
		}

		static void SetSinCos (ref SKMatrix matrix, float sin, float cos)
		{
			matrix.ScaleX = cos;
			matrix.SkewX = -sin;
			matrix.TransX = 0;
			matrix.SkewY = sin;
			matrix.ScaleY = cos;
			matrix.TransY = 0;
			matrix.Persp0 = 0;
			matrix.Persp1 = 0;
			matrix.Persp2 = 1;
#if OPTIMIZED_SKMATRIX
			matrix.typeMask = Mask.Unknown | Mask.OnlyPerspectiveValid;
#endif
		}

		static void SetSinCos (ref SKMatrix matrix, float sin, float cos, float pivotx, float pivoty)
		{
			float oneMinusCos = 1-cos;
			
			matrix.ScaleX = cos;
			matrix.SkewX = -sin;
			matrix.TransX = sdot(sin, pivoty, oneMinusCos, pivotx);
			matrix.SkewY = sin;
			matrix.ScaleY = cos;
			matrix.TransY = sdot(-sin, pivotx, oneMinusCos, pivoty);
			matrix.Persp0 = 0;
			matrix.Persp1 = 0;
			matrix.Persp2 = 1;
#if OPTIMIZED_SKMATRIX
			matrix.typeMask = Mask.Unknown | Mask.OnlyPerspectiveValid;
#endif
		}
		
		public static void Rotate (ref SKMatrix matrix, float radians, float pivotx, float pivoty)
		{
			var sin = (float) Math.Sin (radians);
			var cos = (float) Math.Cos (radians);
			SetSinCos (ref matrix, sin, cos, pivotx, pivoty);
		}

		public static void RotateDegrees (ref SKMatrix matrix, float degrees, float pivotx, float pivoty)
		{
			var sin = (float) Math.Sin (degrees * degToRad);
			var cos = (float) Math.Cos (degrees * degToRad);
			SetSinCos (ref matrix, sin, cos, pivotx, pivoty);
		}

		public static void Rotate (ref SKMatrix matrix, float radians)
		{
			var sin = (float) Math.Sin (radians);
			var cos = (float) Math.Cos (radians);
			SetSinCos (ref matrix, sin, cos);
		}

		public static void RotateDegrees (ref SKMatrix matrix, float degrees)
		{
			var sin = (float) Math.Sin (degrees * degToRad);
			var cos = (float) Math.Cos (degrees * degToRad);
			SetSinCos (ref matrix, sin, cos);
		}

		public static SKMatrix MakeSkew (float sx, float sy)
		{
			return new SKMatrix () {
				ScaleX = 1,
				SkewX = sx,
				TransX = 0,
				SkewY = sy,
				ScaleY = 1,
				TransY = 0,
				Persp0 = 0,
				Persp1 = 0,
				Persp2 = 1,
#if OPTIMIZED_SKMATRIX
				typeMask = Mask.Unknown | Mask.OnlyPerspectiveValid
#endif
			};
		}

		public bool TryInvert (out SKMatrix inverse)
		{
			return SkiaApi.sk_matrix_try_invert (ref this, out inverse) != 0;
		}

		public static void Concat (ref SKMatrix target, ref SKMatrix first, ref SKMatrix second)
		{
			SkiaApi.sk_matrix_concat (ref target, ref first, ref second);
		}

		public static void PreConcat (ref SKMatrix target, ref SKMatrix matrix)
		{
			SkiaApi.sk_matrix_pre_concat (ref target, ref matrix);
		}

		public static void PostConcat (ref SKMatrix target, ref SKMatrix matrix)
		{
			SkiaApi.sk_matrix_post_concat (ref target, ref matrix);
		}

		public void MapRect (ref SKMatrix matrix, out SKRect dest, ref SKRect source)
		{
			SkiaApi.sk_matrix_map_rect (ref matrix, out dest, ref source);
		}

		public SKRect MapRect (SKRect source)
		{
			SKRect result;
			MapRect (ref this, out result, ref source);
			return result;
		}

		public void MapPoints (SKPoint [] result, SKPoint [] points)
		{
			if (result == null)
				throw new ArgumentNullException ("result");
			if (points == null)
				throw new ArgumentNullException ("points");
			int dl = result.Length;
			if (dl != points.Length)
				throw new ArgumentException ("buffers must be the same size");
			unsafe {
				fixed (SKPoint *rp = &result[0]){
					fixed (SKPoint *pp = &points[0]){
						SkiaApi.sk_matrix_map_points (ref this, (IntPtr) rp, (IntPtr) pp, dl);
					}
				}
			}
		}

		public SKPoint [] MapPoints (SKPoint [] points)
		{
			if (points == null)
				throw new ArgumentNullException ("points");
			var res = new SKPoint [points.Length];
			MapPoints (res, points);
			return res;
		}

		public void MapVectors (SKPoint [] result, SKPoint [] vectors)
		{
			if (result == null)
				throw new ArgumentNullException ("result");
			if (vectors == null)
				throw new ArgumentNullException ("vectors");
			int dl = result.Length;
			if (dl != vectors.Length)
				throw new ArgumentException ("buffers must be the same size");
			unsafe {
				fixed (SKPoint *rp = &result[0]){
					fixed (SKPoint *pp = &vectors[0]){
						SkiaApi.sk_matrix_map_vectors (ref this, (IntPtr) rp, (IntPtr) pp, dl);
					}
				}
			}
		}

		public SKPoint [] MapVectors (SKPoint [] vectors)
		{
			if (vectors == null)
				throw new ArgumentNullException ("vectors");
			var res = new SKPoint [vectors.Length];
			MapVectors (res, vectors);
			return res;
		}

		public SKPoint MapXY (float x, float y)
		{
			SKPoint result;
			SkiaApi.sk_matrix_map_xy (ref this, x, y, out result);
			return result;
		}

		public SKPoint MapVector (float x, float y)
		{
			SKPoint result;
			SkiaApi.sk_matrix_map_vector(ref this, x, y, out result);
			return result;
		}

		public float MapRadius (float radius)
		{
			return SkiaApi.sk_matrix_map_radius (ref this, radius);
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKFontMetrics
	{
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

		const uint flagsUnderlineThicknessIsValid = (1U << 0);
		const uint flagsUnderlinePositionIsValid = (1U << 1);

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

		public float? UnderlineThickness
		{
			get {
				if ((flags & flagsUnderlineThicknessIsValid) != 0)
					return underlineThickness;
				else
					return null;
			}
		}

		public float? UnderlinePosition
		{
			get {
				if ((flags & flagsUnderlinePositionIsValid) != 0)
					return underlinePosition;
				else
					return null;
			}
		}
	}
}

