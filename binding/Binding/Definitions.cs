//
// Skia Definitions, enumerations and interop structures
//
// Author:
//    Miguel de Icaza (miguel@xamarin.com)
//
// Copyright 2016 Xamarin Inc
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

using GRBackendObject = System.IntPtr;
using GRBackendContext = System.IntPtr;
using sk_string_t = System.IntPtr;
 	
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
	}

	[Flags]
	public enum SKTypefaceStyle {
		Normal     = 0,
		Bold       = 0x01,
		Italic     = 0x02,
		BoldItalic = 0x03
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
		[Obsolete("Use UltraExpanded instead.", true)]
		UltaExpanded     = UltraExpanded
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
		Bgra8888,
		Index8,
		Gray8,
		RgbaF16
	}

	[Obsolete ("May be removed in the next version.", true)]
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

	[Flags]
	public enum SKBlurMaskFilterFlags {
		None = 0x00,
		IgnoreTransform = 0x01,
		HighQuality = 0x02,
		All = IgnoreTransform | HighQuality,
	}

	[Obsolete ("Use SKBlendMode instead.", true)]
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

	[Obsolete ("Use SKClipOperation instead.", true)]
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

	public enum SKBitmapResizeMethod {
		Box,
		Triangle,
		Lanczos3,
		Hamming,
		Mitchell
	}

	[Flags]
	public enum SKSurfacePropsFlags {
		UseDeviceIndependentFonts = 1 << 0,
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
		Miter, Round, Bevel,
		[Obsolete ("Use SKStrokeJoin.Miter instead.", true)]
		Mitter = Miter,
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

	[StructLayout(LayoutKind.Sequential)]
	public struct SKSurfaceProps {
		private SKPixelGeometry pixelGeometry;
		private SKSurfacePropsFlags flags;

		public SKPixelGeometry PixelGeometry {
			get { return pixelGeometry; }
			set { pixelGeometry = value; }
		}

		public SKSurfacePropsFlags Flags {
			get { return flags; }
			set { flags = value; }
		}
	}

	public enum SKZeroInitialized {
		Yes,
		No,
	}

	public enum SKCodecScanlineOrder {
		TopDown,
		BottomUp
	}

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct SKCodecOptionsInternal {
		public SKZeroInitialized fZeroInitialized;
		public SKRectI* fSubset;
		public IntPtr fFrameIndex;
		[MarshalAs(UnmanagedType.I1)]
		public bool fHasPriorFrame;
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
			HasPriorFrame = false;
		}
		public SKCodecOptions (SKZeroInitialized zeroInitialized, SKRectI subset) {
			ZeroInitialized = zeroInitialized;
			Subset = subset;
			FrameIndex = 0;
			HasPriorFrame = false;
		}
		public SKCodecOptions (SKRectI subset) {
			ZeroInitialized = SKZeroInitialized.No;
			Subset = subset;
			FrameIndex = 0;
			HasPriorFrame = false;
		}
		public SKCodecOptions (int frameIndex, bool hasPriorFrame) {
			ZeroInitialized = SKZeroInitialized.No;
			Subset = null;
			FrameIndex = frameIndex;
			HasPriorFrame = hasPriorFrame;
		}
		public SKZeroInitialized ZeroInitialized { get; set; }
		public SKRectI? Subset { get; set; }
		public bool HasSubset => Subset != null;
		public int FrameIndex { get; set; }
		public bool HasPriorFrame { get; set; }
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKCodecFrameInfo {
		private IntPtr requiredFrame;
		private IntPtr duration;
		[MarshalAs (UnmanagedType.I1)]
		private bool fullyRecieved;

		public int RequiredFrame {
			get { return (int)requiredFrame; }
			set { requiredFrame = (IntPtr)value; }
		}

		public int Duration {
			get { return (int)duration; }
			set { duration = (IntPtr)value; }
		}

		public bool FullyRecieved {
			get { return fullyRecieved; }
			set { fullyRecieved = value; }
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

		public static SKRectI Ceiling (SKRect value)
		{
			int x, y, r, b;
			checked {
				x = (int) Math.Ceiling (value.Left);
				y = (int) Math.Ceiling (value.Top);
				r = (int) Math.Ceiling (value.Right);
				b = (int) Math.Ceiling (value.Bottom);
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

	public enum GRSurfaceOrigin {
		TopLeft,
		BottomLeft,
	}

	public enum GRPixelConfig {
		Unknown,
		Alpha8,
		Gray8,
		Rgb565,
		Rgba4444,
		Rgba8888,
		Bgra8888,
		Srgba8888,
		Sbgra8888,
		Etc1,
		Latc,
		R11Eac,
		Astc12x12,
		RgbaFloat,
		AlphaHalf,
		RgbaHalf,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct GRBackendRenderTargetDesc {
		private int width;
		private int height;
		private GRPixelConfig config;
		private GRSurfaceOrigin origin;
		private int sampleCount;
		private int stencilBits;
		private GRBackendObject renderTargetHandle;

		public int Width {
			get { return width; }
			set { width = value; }
		}
		public int Height {
			get { return height; }
			set { height = value; }
		}
		public GRPixelConfig Config {
			get { return config; }
			set { config = value; }
		}
		public GRSurfaceOrigin Origin {
			get { return origin; }
			set { origin = value; }
		}
		public int SampleCount {
			get { return sampleCount; }
			set { sampleCount = value; }
		}
		public int StencilBits {
			get { return stencilBits; }
			set { stencilBits = value; }
		}
		public GRBackendObject RenderTargetHandle {
			get { return renderTargetHandle; }
			set { renderTargetHandle = value; }
		}
		public SKSizeI Size => new SKSizeI (width, height);
		public SKRectI Rect => new SKRectI (0, 0, width, height);
	}
	
	public enum GRBackend {
		OpenGL,
		Vulkan,
	}

	[Flags]
	public enum GRGlBackendState : UInt32 {
		RenderTarget     = 1 << 0,
		TextureBinding   = 1 << 1,
		View             = 1 << 2, // scissor and viewport
		Blend            = 1 << 3,
		MSAAEnable       = 1 << 4,
		Vertex           = 1 << 5,
		Stencil          = 1 << 6,
		PixelStore       = 1 << 7,
		Program          = 1 << 8,
		FixedFunction    = 1 << 9,
		Misc             = 1 << 10,
		PathRendering    = 1 << 11,
		All              = 0xffff
	}

	[Flags]
	public enum GRBackendState : UInt32 {
		All = 0xffffffff,
	}

	[Flags]
	public enum GRBackendTextureDescFlags {
		None = 0,
		RenderTarget = 1,
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct GRBackendTextureDesc {
		private GRBackendTextureDescFlags flags;
		private GRSurfaceOrigin origin;
		private int width;
		private int height;
		private GRPixelConfig config;
		private int sampleCount;
		private GRBackendObject textureHandle;
		
		public GRBackendTextureDescFlags Flags {
			get { return flags; }
			set { flags = value; }
		}
		public GRSurfaceOrigin Origin {
			get { return origin; }
			set { origin = value; }
		}
		public int Width {
			get { return width; }
			set { width = value; }
		}
		public int Height {
			get { return height; }
			set { height = value; }
		}
		public GRPixelConfig Config {
			get { return config; }
			set { config = value; }
		}
		public int SampleCount {
			get { return sampleCount; }
			set { sampleCount = value; }
		}
		public GRBackendObject TextureHandle {
			get { return textureHandle; }
			set { textureHandle = value; }
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct GRContextOptions {
		[MarshalAs(UnmanagedType.I1)]
		private bool suppressPrints;
		private int maxTextureSizeOverride;
		private int maxTileSizeOverride;
		[MarshalAs(UnmanagedType.I1)]
		private bool suppressDualSourceBlending;
		private int bufferMapThreshold;
		[MarshalAs(UnmanagedType.I1)]
		private bool useDrawInsteadOfPartialRenderTargetWrite;
		[MarshalAs(UnmanagedType.I1)]
		private bool immediateMode;
		[MarshalAs(UnmanagedType.I1)]
		private bool clipBatchToBounds;
		[MarshalAs(UnmanagedType.I1)]
		private bool drawBatchBounds;
		private int maxBatchLookback;
		private int maxBatchLookahead;
		[MarshalAs(UnmanagedType.I1)]
		private bool useShaderSwizzling;
		[MarshalAs(UnmanagedType.I1)]
		private bool doManualMipmapping;
		[MarshalAs(UnmanagedType.I1)]
		private bool enableInstancedRendering;
		[MarshalAs(UnmanagedType.I1)]
		private bool disableDistanceFieldPaths;
		[MarshalAs(UnmanagedType.I1)]
		private bool allowPathMaskCaching;
		[MarshalAs(UnmanagedType.I1)]
		private bool forceSWPathMasks;

		public bool SuppressPrints {
			get { return suppressPrints; }
			set { suppressPrints = value; }
		}
		public int MaxTextureSizeOverride {
			get { return maxTextureSizeOverride; }
			set { maxTextureSizeOverride = value; }
		}
		public int MaxTileSizeOverride {
			get { return maxTileSizeOverride; }
			set { maxTileSizeOverride = value; }
		}
		public bool SuppressDualSourceBlending {
			get { return suppressDualSourceBlending; }
			set { suppressDualSourceBlending = value; }
		}
		public int BufferMapThreshold {
			get { return bufferMapThreshold; }
			set { bufferMapThreshold = value; }
		}
		public bool UseDrawInsteadOfPartialRenderTargetWrite {
			get { return useDrawInsteadOfPartialRenderTargetWrite; }
			set { useDrawInsteadOfPartialRenderTargetWrite = value; }
		}
		public bool ImmediateMode {
			get { return immediateMode; }
			set { immediateMode = value; }
		}
		public bool ClipBatchToBounds {
			get { return clipBatchToBounds; }
			set { clipBatchToBounds = value; }
		}
		public bool DrawBatchBounds {
			get { return drawBatchBounds; }
			set { drawBatchBounds = value; }
		}
		public int MaxBatchLookback {
			get { return maxBatchLookback; }
			set { maxBatchLookback = value; }
		}
		public int MaxBatchLookahead {
			get { return maxBatchLookahead; }
			set { maxBatchLookahead = value; }
		}
		public bool UseShaderSwizzling {
			get { return useShaderSwizzling; }
			set { useShaderSwizzling = value; }
		}
		public bool DoManualMipmapping {
			get { return doManualMipmapping; }
			set { doManualMipmapping = value; }
		}
		public bool EnableInstancedRendering {
			get { return enableInstancedRendering; }
			set { enableInstancedRendering = value; }
		}
		public bool DisableDistanceFieldPaths {
			get { return disableDistanceFieldPaths; }
			set { disableDistanceFieldPaths = value; }
		}
		public bool AllowPathMaskCaching {
			get { return allowPathMaskCaching; }
			set { allowPathMaskCaching = value; }
		}
		public bool ForceSWPathMasks {
			get { return forceSWPathMasks; }
			set { forceSWPathMasks = value; }
		}

		public static GRContextOptions Default {
			get {
				return new GRContextOptions {
					suppressPrints = false,
					maxTextureSizeOverride = int.MaxValue,
					maxTileSizeOverride = 0,
					suppressDualSourceBlending = false,
					bufferMapThreshold = -1,
					useDrawInsteadOfPartialRenderTargetWrite = false,
					immediateMode = false,
					clipBatchToBounds = false,
					drawBatchBounds = false,
					maxBatchLookback = -1,
					maxBatchLookahead = -1,
					useShaderSwizzling = false,
					doManualMipmapping = false,
					enableInstancedRendering = false,
					disableDistanceFieldPaths = false,
					allowPathMaskCaching = false,
					forceSWPathMasks = false
				};
			}
		}
	}
	
	[Obsolete ("Use GRContext.Flush() instead.", true)]
	public enum GRContextFlushBits {
		None = 0,
		Discard = 0x2,
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

	public enum SKLatticeFlags {
		Default,
		Transparent = 1 << 0,
	};

	[StructLayout(LayoutKind.Sequential)]
	internal unsafe struct SKLatticeInternal {
		public int* fXDivs;
		public int* fYDivs;
		public SKLatticeFlags* fFlags;
		public int fXCount;
		public int fYCount;
		public SKRectI* fBounds;
	}

	public struct SKLattice {
		public int[] XDivs { get; set; }
		public int[] YDivs { get; set; }
		public SKLatticeFlags[] Flags { get; set; }
		public SKRectI? Bounds { get; set; }
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

	public struct SKEncodedInfo {
		private SKEncodedInfoColor color;
		private SKEncodedInfoAlpha alpha;
		private byte bitsPerComponent;

		public SKEncodedInfo (SKEncodedInfoColor color) {
			this.color = color;
			this.bitsPerComponent = 8;

			switch (color) {
				case SKEncodedInfoColor.Gray:
				case SKEncodedInfoColor.Rgb:
				case SKEncodedInfoColor.Bgr:
				case SKEncodedInfoColor.Bgrx:
				case SKEncodedInfoColor.Yuv:
				case SKEncodedInfoColor.InvertedCmyk:
				case SKEncodedInfoColor.Ycck:
					this.alpha = SKEncodedInfoAlpha.Opaque;
					break;
				case SKEncodedInfoColor.GrayAlpha:
				case SKEncodedInfoColor.Palette:
				case SKEncodedInfoColor.Rgba:
				case SKEncodedInfoColor.Bgra:
				case SKEncodedInfoColor.Yuva:
					this.alpha = SKEncodedInfoAlpha.Unpremul;
					break;
				default:
					throw new ArgumentOutOfRangeException (nameof (color));
			}
		}

		public SKEncodedInfo (SKEncodedInfoColor color, SKEncodedInfoAlpha alpha, byte bitsPerComponent) {
			if (bitsPerComponent != 1 && bitsPerComponent != 2 && bitsPerComponent != 4 && bitsPerComponent != 8 && bitsPerComponent != 16) {
				throw new ArgumentException ("The bits per component must be 1, 2, 4, 8 or 16.", nameof (bitsPerComponent));
			}

			switch (color) {
				case SKEncodedInfoColor.Gray:
					if (alpha != SKEncodedInfoAlpha.Opaque)
						throw new ArgumentException ("The alpha must be opaque.", nameof (alpha));
					break;
				case SKEncodedInfoColor.GrayAlpha:
					if (alpha == SKEncodedInfoAlpha.Opaque)
						throw new ArgumentException ("The alpha must not be opaque.", nameof (alpha));
					break;
				case SKEncodedInfoColor.Palette:
					if (bitsPerComponent == 16)
						throw new ArgumentException ("The bits per component must be 1, 2, 4 or 8.", nameof (bitsPerComponent));
					break;
				case SKEncodedInfoColor.Rgb:
				case SKEncodedInfoColor.Bgr:
				case SKEncodedInfoColor.Bgrx:
					if (alpha != SKEncodedInfoAlpha.Opaque)
						throw new ArgumentException ("The alpha must be opaque.", nameof (alpha));
					if (bitsPerComponent < 8)
						throw new ArgumentException ("The bits per component must be 8 or 16.", nameof (bitsPerComponent));
					break;
				case SKEncodedInfoColor.Yuv:
				case SKEncodedInfoColor.InvertedCmyk:
				case SKEncodedInfoColor.Ycck:
					if (alpha != SKEncodedInfoAlpha.Opaque)
						throw new ArgumentException ("The alpha must be opaque.", nameof (alpha));
					if (bitsPerComponent != 8)
						throw new ArgumentException ("The bits per component must be 8.", nameof (bitsPerComponent));
					break;
				case SKEncodedInfoColor.Rgba:
					if (alpha == SKEncodedInfoAlpha.Opaque)
						throw new ArgumentException ("The alpha must not be opaque.", nameof (alpha));
					if (bitsPerComponent < 8)
						throw new ArgumentException ("The bits per component must be 8 or 16.", nameof (bitsPerComponent));
					break;
				case SKEncodedInfoColor.Bgra:
				case SKEncodedInfoColor.Yuva:
					if (alpha == SKEncodedInfoAlpha.Opaque)
						throw new ArgumentException ("The alpha must not be opaque.", nameof (alpha));
					if (bitsPerComponent != 8)
						throw new ArgumentException ("The bits per component must be 8.", nameof (bitsPerComponent));
					break;
				default:
					throw new ArgumentOutOfRangeException (nameof (color));
			}

			this.color = color;
			this.alpha = alpha;
			this.bitsPerComponent = bitsPerComponent;
		}

		public SKEncodedInfoColor Color => color;
		public SKEncodedInfoAlpha Alpha => alpha;
		public byte BitsPerComponent => bitsPerComponent;
		public byte BitsPerPixel {
			get {
				switch (color) {
					case SKEncodedInfoColor.Gray:
						return bitsPerComponent;
					case SKEncodedInfoColor.GrayAlpha:
						return (byte)(2 * bitsPerComponent);
					case SKEncodedInfoColor.Palette:
						return bitsPerComponent;
					case SKEncodedInfoColor.Rgb:
					case SKEncodedInfoColor.Bgr:
					case SKEncodedInfoColor.Yuv:
						return (byte)(3 * bitsPerComponent);
					case SKEncodedInfoColor.Rgba:
					case SKEncodedInfoColor.Bgra:
					case SKEncodedInfoColor.Bgrx:
					case SKEncodedInfoColor.Yuva:
					case SKEncodedInfoColor.InvertedCmyk:
					case SKEncodedInfoColor.Ycck:
						return (byte)(4 * bitsPerComponent);
					default:
						return (byte)0;
				}
			}
		}
	}

	public enum SKEncodedInfoAlpha {
		Opaque,
		Unpremul,
		Binary,
	}

	public enum SKEncodedInfoColor {
		Gray,
		GrayAlpha,
		Palette,
		Rgb,
		Rgba,
		Bgr,
		Bgrx,
		Bgra,
		Yuv,
		Yuva,
		InvertedCmyk,
		Ycck,
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
}
