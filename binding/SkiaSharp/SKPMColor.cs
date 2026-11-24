#nullable disable

using System;
using System.Runtime.CompilerServices;

namespace SkiaSharp
{
	/// <summary>32-bit ARGB premultiplied color value.</summary>
	/// <remarks>The byte order for this value is configuration dependent. This is different from <see cref="T:SkiaSharp.SKColor" />, which is unpremultiplied, and is always in the same byte order.</remarks>
	public readonly unsafe struct SKPMColor : IEquatable<SKPMColor>
	{
		private readonly uint color;

		/// <summary>Creates a color from the specified integer.</summary>
		/// <param name="value">The integer value of the premultiplied color.</param>
		/// <remarks />
		public SKPMColor (uint value)
		{
			color = value;
		}

		/// <summary>Gets the alpha component of the color.</summary>
		/// <value>The alpha component value.</value>
		/// <remarks />
		public readonly byte Alpha => (byte)((color >> SKImageInfo.PlatformColorAlphaShift) & 0xff);
		/// <summary>Gets the red component of the color.</summary>
		/// <value>The red component value.</value>
		/// <remarks />
		public readonly byte Red => (byte)((color >> SKImageInfo.PlatformColorRedShift) & 0xff);
		/// <summary>Gets the green component of the color.</summary>
		/// <value>The green component value.</value>
		/// <remarks />
		public readonly byte Green => (byte)((color >> SKImageInfo.PlatformColorGreenShift) & 0xff);
		/// <summary>Gets the blue component of the color.</summary>
		/// <value>The blue component value.</value>
		/// <remarks />
		public readonly byte Blue => (byte)((color >> SKImageInfo.PlatformColorBlueShift) & 0xff);

		// PreMultiply

		/// <summary>Converts an unpremultiplied <see cref="T:SkiaSharp.SKColor" /> to a premultiplied <see cref="T:SkiaSharp.SKPMColor" />.</summary>
		/// <param name="color">The unpremultiplied color to convert.</param>
		/// <returns>Returns the new premultiplied <see cref="T:SkiaSharp.SKPMColor" />.</returns>
		/// <remarks />
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

		/// <summary>Converts an array of unpremultiplied <see cref="T:SkiaSharp.SKColor" />s to an array of premultiplied <see cref="T:SkiaSharp.SKPMColor" />s.</summary>
		/// <param name="colors">The unpremultiplied colors to convert.</param>
		/// <returns>Returns the new array of premultiplied <see cref="T:SkiaSharp.SKPMColor" />s.</returns>
		/// <remarks />
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

		/// <summary>Converts a premultiplied <see cref="T:SkiaSharp.SKPMColor" /> to the unpremultiplied <see cref="T:SkiaSharp.SKColor" />.</summary>
		/// <param name="pmcolor">The premultiplied color to convert.</param>
		/// <returns>Returns the new unpremultiplied <see cref="T:SkiaSharp.SKColor" />.</returns>
		/// <remarks />
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

		/// <summary>Converts an array of premultiplied <see cref="T:SkiaSharp.SKPMColor" />s to an array of unpremultiplied <see cref="T:SkiaSharp.SKColor" />s.</summary>
		/// <param name="pmcolors">The premultiplied colors to convert.</param>
		/// <returns>Returns the new array of unpremultiplied <see cref="T:SkiaSharp.SKColor" />s.</returns>
		/// <remarks />
		public static SKColor[] UnPreMultiply (SKPMColor[] pmcolors)
		{
			var colors = new SKColor[pmcolors.Length];
			fixed (SKColor* c = colors)
			fixed (SKPMColor* pm = pmcolors) {
				SkiaApi.sk_color_unpremultiply_array ((uint*)pm, pmcolors.Length, (uint*)c);
			}
			return colors;
		}

		/// <summary>Converts an unpremultiplied <see cref="T:SkiaSharp.SKColor" /> to the premultiplied <see cref="T:SkiaSharp.SKPMColor" />.</summary>
		/// <param name="color">The unpremultiplied color to convert.</param>
		/// <returns>Returns the new premultiplied <see cref="T:SkiaSharp.SKPMColor" />.</returns>
		/// <remarks />
		public static explicit operator SKPMColor (SKColor color) =>
			SKPMColor.PreMultiply (color);

		/// <summary>Converts a premultiplied <see cref="T:SkiaSharp.SKPMColor" /> to the unpremultiplied <see cref="T:SkiaSharp.SKColor" />.</summary>
		/// <param name="color">The premultiplied color to convert.</param>
		/// <returns>Returns the new unpremultiplied <see cref="T:SkiaSharp.SKColor" />.</returns>
		/// <remarks />
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

		/// <summary>Returns the color as a string in the format: #AARRGGBB.</summary>
		/// <returns>The string representation of the color.</returns>
		/// <remarks />
		public readonly override string ToString () =>
			$"#{Alpha:x2}{Red:x2}{Green:x2}{Blue:x2}";

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKPMColor" /> to compare with the current color.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKPMColor obj) =>
			obj.color == color;

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object other) =>
			other is SKPMColor f && Equals (f);

		/// <summary>Indicates whether two <see cref="T:SkiaSharp.SKPMColor" /> objects are equal.</summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		/// <returns>Returns <see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKPMColor left, SKPMColor right) =>
			left.Equals (right);

		/// <summary>Indicates whether two <see cref="T:SkiaSharp.SKPMColor" /> objects are different.</summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		/// <returns>Returns <see langword="true" /> if <paramref name="left" /> is not equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKPMColor left, SKPMColor right) =>
			!left.Equals (right);

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>Returns a hash code for the current object.</returns>
		/// <remarks />
		public readonly override int GetHashCode () =>
			color.GetHashCode ();

		/// <summary>Converts a UInt32 to a <see cref="T:SkiaSharp.SKPMColor" />.</summary>
		/// <param name="color">The UInt32 representation of a color.</param>
		/// <returns>The new <see cref="T:SkiaSharp.SKPMColor" /> instance.</returns>
		/// <remarks />
		public static implicit operator SKPMColor (uint color) =>
			new SKPMColor (color);

		/// <summary>Converts a <see cref="T:SkiaSharp.SKPMColor" /> to a UInt32.</summary>
		/// <param name="color">The color to convert.</param>
		/// <returns>The UInt32 value for the color.</returns>
		/// <remarks />
		public static explicit operator uint (SKPMColor color) =>
			color.color;
	}
}
