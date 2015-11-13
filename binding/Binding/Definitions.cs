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
	public struct SKColor {

		uint color;
		public SKColor (byte red, byte green, byte blue, byte alpha)
		{
			color = (uint)((alpha << 24) | (red << 16) | (green << 8) | blue);
		}

		public SKColor (byte red, byte green, byte blue)
		{
			color = (uint)((red << 16) | (green << 8) | blue);
		}

		public byte Alpha => (byte)((color >> 24) & 0xff);
		public byte Red => (byte)((color >> 16) & 0xff);
		public byte Green => (byte)((color >> 8) & 0xff);
		public byte Blue => (byte)((color) & 0xff);

		public override string ToString ()
		{
			return string.Format (CultureInfo.InvariantCulture, "#{0:x2}{0:x2}{0:x2}{0:x2}",  Alpha, Red, Green, Blue);
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

	public enum SKPathDirection {
		Clockwise,
		CounterClockwise
	}

	public enum SKColorType {
		Unknown,
		Rgba_8888,
		Bgra_8888,
		Alpha_8
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

	[StructLayout(LayoutKind.Sequential)]
	public struct SKImageInfo {
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
	public struct SKRectI {
		public int Left, Top, Right, Bottom;
		public SKRectI (int left, int top, int right, int bottom)
		{
			Left = left;
			Right = right;
			Top = top;
			Bottom = bottom;
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

		public void SetScaleTransalte (float sx, float sy, float tx, float ty)
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

