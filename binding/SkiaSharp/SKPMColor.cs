#nullable disable

using System;
using System.Runtime.CompilerServices;

namespace SkiaSharp
{
	public readonly unsafe struct SKPMColor : IEquatable<SKPMColor>
	{
		private readonly uint color;

		public SKPMColor (uint value)
		{
			color = value;
		}

		public readonly byte Alpha => (byte)((color >> SKImageInfo.PlatformColorAlphaShift) & 0xff);
		public readonly byte Red => (byte)((color >> SKImageInfo.PlatformColorRedShift) & 0xff);
		public readonly byte Green => (byte)((color >> SKImageInfo.PlatformColorGreenShift) & 0xff);
		public readonly byte Blue => (byte)((color >> SKImageInfo.PlatformColorBlueShift) & 0xff);

		// PreMultiply

		public static SKPMColor PreMultiply (SKColor color)
		{
			uint a = color.Alpha;
			uint r = color.Red;
			uint g = color.Green;
			uint b = color.Blue;

			// Opaque colors are unchanged; MulDiv255Round (c, 255) == c for every c.
			if (a != 255) {
				r = MulDiv255Round (r, a);
				g = MulDiv255Round (g, a);
				b = MulDiv255Round (b, a);
			}

			var pmcolor =
				(a << SKImageInfo.PlatformColorAlphaShift) |
				(r << SKImageInfo.PlatformColorRedShift) |
				(g << SKImageInfo.PlatformColorGreenShift) |
				(b << SKImageInfo.PlatformColorBlueShift);
			return new SKPMColor (pmcolor);
		}

		public static SKPMColor[] PreMultiply (SKColor[] colors)
		{
			var pmcolors = new SKPMColor[colors.Length];
			fixed (SKColor* c = colors)
			fixed (SKPMColor* pm = pmcolors) {
				SkiaApi.sk_color_premultiply_array ((uint*)c, colors.Length, (uint*)pm);
			}
			return pmcolors;
		}

		// UnPreMultiply

		public static SKColor UnPreMultiply (SKPMColor pmcolor)
		{
			uint a = pmcolor.Alpha;
			var scale = UnPreMultiplyScale (a);
			return new SKColor (
				(byte)ApplyUnPreMultiplyScale (scale, pmcolor.Red),
				(byte)ApplyUnPreMultiplyScale (scale, pmcolor.Green),
				(byte)ApplyUnPreMultiplyScale (scale, pmcolor.Blue),
				(byte)a);
		}

		public static SKColor[] UnPreMultiply (SKPMColor[] pmcolors)
		{
			var colors = new SKColor[pmcolors.Length];
			fixed (SKColor* c = colors)
			fixed (SKPMColor* pm = pmcolors) {
				SkiaApi.sk_color_unpremultiply_array ((uint*)pm, pmcolors.Length, (uint*)c);
			}
			return colors;
		}

		public static explicit operator SKPMColor (SKColor color) =>
			SKPMColor.PreMultiply (color);

		public static explicit operator SKColor (SKPMColor color) =>
			SKPMColor.UnPreMultiply (color);

		// Managed replicas of Skia's SkMulDiv255Round / SkUnPreMultiply::ApplyScale
		// (skia/src/core/SkColorData.h and skia/src/core/SkUnPreMultiply.cpp),
		// kept bit-exact with the native sk_color_(un)premultiply for every input.

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static uint MulDiv255Round (uint value, uint alpha)
		{
			var prod = value * alpha + 128;
			return (prod + (prod >> 8)) >> 8;
		}

		private static readonly uint[] unpremultiplyScale = CreateUnPreMultiplyScaleTable ();

		private static uint[] CreateUnPreMultiplyScaleTable ()
		{
			// Mirrors Skia's SkUnPreMultiply::gTable: round((255 << 24) / alpha), computed once.
			// Replaces a per-call 32-bit divide with a table lookup, matching the native path.
			var table = new uint[256];
			for (uint a = 1; a < 256; a++)
				table[a] = ((255u << 24) + (a >> 1)) / a;
			return table;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static uint UnPreMultiplyScale (uint alpha) =>
			unpremultiplyScale[alpha & 0xff];

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static uint ApplyUnPreMultiplyScale (uint scale, uint component) =>
			unchecked ((scale * component + (1u << 23)) >> 24);

		public readonly override string ToString () =>
			$"#{Alpha:x2}{Red:x2}{Green:x2}{Blue:x2}";

		public readonly bool Equals (SKPMColor obj) =>
			obj.color == color;

		public readonly override bool Equals (object other) =>
			other is SKPMColor f && Equals (f);

		public static bool operator == (SKPMColor left, SKPMColor right) =>
			left.Equals (right);

		public static bool operator != (SKPMColor left, SKPMColor right) =>
			!left.Equals (right);

		public readonly override int GetHashCode () =>
			color.GetHashCode ();

		public static implicit operator SKPMColor (uint color) =>
			new SKPMColor (color);

		public static explicit operator uint (SKPMColor color) =>
			color.color;
	}
}
