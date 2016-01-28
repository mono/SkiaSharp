//
// Skia Definitions, enumerations and interop structures
//
// Author:
//    Miguel de Icaza (miguel@xamarin.com)
//
// Copyright 2015 Xamarin Inc
//
// TODO: 
//   Add more ToString, operators, convenience methods to various structures here (point, rect, etc)
//   Sadly, the Rectangles are not binary compatible with the System.Drawing ones.
//
// SkMatrix could benefit from bringing some of the operators defined in C++
//
using System;
using System.Runtime.InteropServices;
using System.Globalization;
 	
namespace SkiaSharp
{
	public enum SKImageDecoderResult {
		Failure        = 0,
		PartialSuccess = 1,
		Success        = 2 
	}

	public enum SKImageDecoderMode {
		DecodeBounds,
		DecodePixels
	}

	public enum SKImageDecoderFormat {
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
			color = (uint)(0xff000000 | (red << 16) | (green << 8) | blue);
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

	public enum SKTypefaceStyle {
		Normal,
		Bold,
		Italic,
		BoldItalic
	}

	public enum SKPointMode {
		Points, Lines, Polygon
	}

	public enum SKPathDirection {
		Clockwise,
		CounterClockwise
	}

	public enum SKColorType {
		Unknown,
		Rgba_8888,
		Bgra_8888,
		Alpha_8,
		Rgb_565,
		N_32
	}

	public enum SKColorProfileType {
		Linear,
		SRGB
	}

	public enum SKAlphaType {
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
		DawShadowOnly,
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

	[StructLayout(LayoutKind.Sequential)]
	public struct SKImageInfo {
		public static SKImageInfo Empty;

		public int Width;
		public int Height;
		public SKColorType ColorType;
		public SKAlphaType AlphaType;

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
				case SKColorType.Alpha_8:
					return 1;
				case SKColorType.Rgb_565:
					return 2;
				case SKColorType.Bgra_8888:
				case SKColorType.Rgba_8888:
				case SKColorType.N_32:
					return 4;
				}
				throw new ArgumentOutOfRangeException ("ColorType");
			}
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
		public SKPixelGeometry PixelGeometry { get; set; }
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKPoint {
		public float X, Y;
		public SKPoint(float x, float y)
		{
			X = x;
			Y = y;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKPointI {
		public int X, Y;
		public SKPointI(int x, int y)
		{
			X = x;
			Y = y;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct SKPoint3
	{
		public float X, Y, Z;
		public SKPoint3(float x, float y, float z)
		{
			X = x;
			Y = y;
			Z = z;
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

		static float sdot (float a, float b, float c, float d) => a * b + c * d;
		static float scross(float a, float b, float c, float d) => a * b - c * d;

		public static SKMatrix MakeIdentity ()
		{
			return new SKMatrix () { ScaleX = 1, ScaleY = 1, Persp2 = 1 };
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
		}

		public static SKMatrix MakeScale (float sx, float sy)
		{
			if (sx == 1 && sy == 1)
				return MakeIdentity ();
			return new SKMatrix () { ScaleX = sx, ScaleY = sy, Persp2 = 1 };
				
		}

		/// <summary>
		/// Set the matrix to scale by sx and sy, with a pivot point at (px, py).
		/// The pivot point is the coordinate that should remain unchanged by the
		/// specified transformation.
		public static SKMatrix MakeScale (float sx, float sy, float pivotX, float pivotY)
		{
			if (sx == 1 && sy == 1)
				return MakeIdentity ();
			//this->setScaleTranslate(sx, sy, px - sx * px, py - sy * py);

			return new SKMatrix () { 
				ScaleX = sx, ScaleY = sy, 
				TransX = pivotX - sx * pivotX,
				TransY = pivotY - sy * pivotY,
				Persp2 = 1 
			};
		}

		public static SKMatrix MakeTranslation (float dx, float dy)
		{
			return new SKMatrix () { 
				ScaleX = 1, ScaleY = 1,
				TransX = dx, TransY = dy,
				Persp2 = 1
			};
		}

		public static SKMatrix MakeRotation (float radians)
		{
			var sin = (float) Math.Sin (radians);
			var cos = (float)Math.Cos (radians);

			return new SKMatrix () {
				ScaleX = cos,
				SkewX = -sin,
				TransX = 0,
				SkewY = sin,
				ScaleY = cos,
				TransY = 0,
				Persp0 = 0,
				Persp1 = 0,
				Persp2 = 1
			};
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
				Persp2 = 1
			};
		}
	}
}

