#nullable disable

using System;
using System.Runtime.CompilerServices;

namespace SkiaSharp
{
	/// <summary>32-bit ARGB unpremultiplied color value.</summary>
	/// <remarks>The color components are always in a known order.</remarks>
	public readonly struct SKColor : IEquatable<SKColor>
	{
		/// <summary>Gets an "empty" color, with zero for all the components.</summary>
		/// <remarks />
		public static readonly SKColor Empty;

		private readonly uint color;

		/// <summary>Creates a color from the specified integer.</summary>
		/// <param name="value">The integer value of the unpremultiplied color.</param>
		/// <remarks />
		public SKColor (uint value)
		{
			color = value;
		}

		/// <summary>Creates a color from the specified red, green, blue and alpha components.</summary>
		/// <param name="red">The red component.</param>
		/// <param name="green">The green component.</param>
		/// <param name="blue">The blue component.</param>
		/// <param name="alpha">The alpha component.</param>
		/// <remarks />
		public SKColor (byte red, byte green, byte blue, byte alpha)
		{
			color = (uint)((alpha << 24) | (red << 16) | (green << 8) | blue);
		}

		/// <summary>Creates a color from the specified red, green and blue components.</summary>
		/// <param name="red">The red component.</param>
		/// <param name="green">The green component.</param>
		/// <param name="blue">The blue component.</param>
		/// <remarks />
		public SKColor (byte red, byte green, byte blue)
		{
			color = (0xff000000u | (uint)(red << 16) | (uint)(green << 8) | blue);
		}

		/// <summary>Returns a new color based on this current instance, but with the new red channel value.</summary>
		/// <param name="red">The new red component.</param>
		/// <returns>A new color with the specified red value.</returns>
		/// <remarks />
		public readonly SKColor WithRed (byte red) =>
			new SKColor (red, Green, Blue, Alpha);

		/// <summary>Returns a new color based on this current instance, but with the new green channel value.</summary>
		/// <param name="green">The new green component.</param>
		/// <returns>A new color with the specified green value.</returns>
		/// <remarks />
		public readonly SKColor WithGreen (byte green) =>
			new SKColor (Red, green, Blue, Alpha);

		/// <summary>Returns a new color based on this current instance, but with the new blue channel value.</summary>
		/// <param name="blue">The new blue component.</param>
		/// <returns>A new color with the specified blue value.</returns>
		/// <remarks />
		public readonly SKColor WithBlue (byte blue) =>
			new SKColor (Red, Green, blue, Alpha);

		/// <summary>Returns a new color based on this current instance, but with the new alpha channel value.</summary>
		/// <param name="alpha">The new alpha component.</param>
		/// <returns>A new color with the specified alpha value.</returns>
		/// <remarks />
		public readonly SKColor WithAlpha (byte alpha) =>
			new SKColor (Red, Green, Blue, alpha);

		/// <summary>Gets the alpha component of the color.</summary>
		/// <value>The alpha component of the color.</value>
		/// <remarks />
		public readonly byte Alpha => (byte)((color >> 24) & 0xff);
		/// <summary>Gets the red component of the color.</summary>
		/// <value>The red component of the color.</value>
		/// <remarks />
		public readonly byte Red => (byte)((color >> 16) & 0xff);
		/// <summary>Gets the green component of the color.</summary>
		/// <value>The green component of the color.</value>
		/// <remarks />
		public readonly byte Green => (byte)((color >> 8) & 0xff);
		/// <summary>Gets the blue component of the color.</summary>
		/// <value>The blue component of the color.</value>
		/// <remarks />
		public readonly byte Blue => (byte)((color) & 0xff);

		/// <summary>Gets the hue value.</summary>
		/// <value>The hue value.</value>
		/// <remarks />
		public readonly float Hue {
			get {
				ToHsv (out var h, out _, out _);
				return h;
			}
		}

		/// <summary>Creates a color from the specified hue, saturation, lightness/luminosity and alpha values.</summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="l">The lightness/luminosity value.</param>
		/// <param name="a">The alpha value.</param>
		/// <returns>The new <see cref="T:SkiaSharp.SKColor" /> instance.</returns>
		/// <remarks />
		public static SKColor FromHsl (float h, float s, float l, byte a = 255)
		{
			var colorf = SKColorF.FromHsl (h, s, l);

			// RGB results from 0 to 255
			var r = colorf.Red * 255f;
			var g = colorf.Green * 255f;
			var b = colorf.Blue * 255f;

			return new SKColor ((byte)r, (byte)g, (byte)b, a);
		}

		/// <summary>Creates a color from the specified hue, saturation, value/brightness and alpha values.</summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="v">The value/brightness value.</param>
		/// <param name="a">The alpha value.</param>
		/// <returns>The new <see cref="T:SkiaSharp.SKColor" /> instance.</returns>
		/// <remarks />
		public static SKColor FromHsv (float h, float s, float v, byte a = 255)
		{
			var colorf = SKColorF.FromHsv (h, s, v);

			// RGB results from 0 to 255
			var r = colorf.Red * 255f;
			var g = colorf.Green * 255f;
			var b = colorf.Blue * 255f;

			return new SKColor ((byte)r, (byte)g, (byte)b, a);
		}

		/// <summary>Converts the current color into its hue, saturation and lightness/luminosity values.</summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="l">The lightness/luminosity value.</param>
		/// <remarks>The alpha value is separate from the HSL calculation and will always be the same as <see cref="P:SkiaSharp.SKColor.Alpha" />.</remarks>
		public readonly void ToHsl (out float h, out float s, out float l)
		{
			// RGB from 0 to 255
			var r = Red / 255f;
			var g = Green / 255f;
			var b = Blue / 255f;

			var colorf = new SKColorF (r, g, b);
			colorf.ToHsl (out h, out s, out l);
		}

		/// <summary>Converts the current color into its hue, saturation and value/brightness values.</summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="v">The value/brightness value.</param>
		/// <remarks>The alpha value is separate from the HSV/HSB calculation and will always be the same as <see cref="P:SkiaSharp.SKColor.Alpha" />.</remarks>
		public readonly void ToHsv (out float h, out float s, out float v)
		{
			// RGB from 0 to 255
			var r = Red / 255f;
			var g = Green / 255f;
			var b = Blue / 255f;

			var colorf = new SKColorF (r, g, b);
			colorf.ToHsv (out h, out s, out v);
		}

		/// <summary>Returns the color as a string in the format: #AARRGGBB.</summary>
		/// <returns>The string representation of the color.</returns>
		/// <remarks />
		public readonly override string ToString () =>
			$"#{Alpha:x2}{Red:x2}{Green:x2}{Blue:x2}";

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="obj">The color to compare with the current color.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool Equals (SKColor obj) =>
			obj.color == color;

		/// <summary>Determines whether the specified object is equal to the current object.</summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public readonly override bool Equals (object other) =>
			other is SKColor f && Equals (f);

		/// <summary>Indicates whether two <see cref="T:SkiaSharp.SKColor" /> objects are equal.</summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		/// <returns>Returns <see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (SKColor left, SKColor right) =>
			left.Equals (right);

		/// <summary>Indicates whether two <see cref="T:SkiaSharp.SKColor" /> objects are different.</summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		/// <returns>Returns <see langword="true" /> if <paramref name="left" /> is not equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (SKColor left, SKColor right) =>
			!left.Equals (right);

		/// <summary>Serves as the default hash function.</summary>
		/// <returns>Returns a hash code for the current object.</returns>
		/// <remarks />
		public readonly override int GetHashCode () =>
			color.GetHashCode ();

		/// <summary>Converts a UInt32 to a <see cref="T:SkiaSharp.SKColor" />.</summary>
		/// <param name="color">The UInt32 representation of a color.</param>
		/// <returns>The new <see cref="T:SkiaSharp.SKColor" /> instance.</returns>
		/// <remarks />
		public static implicit operator SKColor (uint color) =>
			new SKColor (color);

		/// <summary>Converts a <see cref="T:SkiaSharp.SKColor" /> to a UInt32.</summary>
		/// <param name="color">The color to convert.</param>
		/// <returns>The UInt32 value for the color.</returns>
		/// <remarks />
		public static explicit operator uint (SKColor color) =>
			color.color;

		/// <summary>Converts the hexadecimal string representation of a color to its <see cref="T:SkiaSharp.SKColor" /> equivalent.</summary>
		/// <param name="hexString">The hexadecimal string representation of a color.</param>
		/// <returns>The new <see cref="T:SkiaSharp.SKColor" /> instance.</returns>
		/// <remarks>This method can parse a string in the forms with or without a preceding '#' character: AARRGGBB, RRGGBB, ARGB, RGB.</remarks>
		public static SKColor Parse (string hexString) =>
			Parse (hexString.AsSpan ());

		/// <summary>Converts the hexadecimal string representation of a color to its <see cref="T:SkiaSharp.SKColor" /> equivalent.</summary>
		/// <param name="hexString">The hexadecimal string representation of a color.</param>
		/// <returns>The new <see cref="T:SkiaSharp.SKColor" /> instance.</returns>
		/// <remarks>This method can parse a string in the forms with or without a preceding '#' character: AARRGGBB, RRGGBB, ARGB, RGB.</remarks>
		public static SKColor Parse (ReadOnlySpan<char> hexString)
		{
			if (!TryParse (hexString, out var color))
				throw new ArgumentException ("Invalid hexadecimal color string.", nameof (hexString));
			return color;
		}

		/// <summary>Converts the hexadecimal string representation of a color to its <see cref="T:SkiaSharp.SKColor" /> equivalent.</summary>
		/// <param name="hexString">The hexadecimal string representation of a color.</param>
		/// <param name="color">The new <see cref="T:SkiaSharp.SKColor" /> instance.</param>
		/// <returns><see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks>This method can parse a string in the forms with or without a preceding '#' character: AARRGGBB, RRGGBB, ARGB, RGB.</remarks>
		public static bool TryParse (string hexString, out SKColor color) =>
			TryParse (hexString.AsSpan (), out color);

		/// <summary>Converts the hexadecimal string representation of a color to its <see cref="T:SkiaSharp.SKColor" /> equivalent.</summary>
		/// <param name="hexString">The hexadecimal string representation of a color.</param>
		/// <param name="color">The new <see cref="T:SkiaSharp.SKColor" /> instance.</param>
		/// <returns><see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks>This method can parse a string in the forms with or without a preceding '#' character: AARRGGBB, RRGGBB, ARGB, RGB.</remarks>
		public static bool TryParse (ReadOnlySpan<char> hexString, out SKColor color)
		{
			color = SKColor.Empty;

			// Trim surrounding whitespace and any leading '#'. Re-basing the span to index 0 also lets
			// the JIT prove the fixed-length indexes below are in range and drop the bounds checks.
			hexString = hexString.Trim ().TrimStart ('#');

			switch (hexString.Length) {
				case 3: {
					// #RGB -> each nibble is duplicated (e.g. "F" -> 0xFF)
					if (!TryParseNibble (hexString[0], out var r) ||
						!TryParseNibble (hexString[1], out var g) ||
						!TryParseNibble (hexString[2], out var b))
						return false;

					color = new SKColor (
						(byte)(r << 4 | r),
						(byte)(g << 4 | g),
						(byte)(b << 4 | b));
					return true;
				}
				case 4: {
					// #ARGB -> each nibble is duplicated (e.g. "F" -> 0xFF)
					if (!TryParseNibble (hexString[0], out var a) ||
						!TryParseNibble (hexString[1], out var r) ||
						!TryParseNibble (hexString[2], out var g) ||
						!TryParseNibble (hexString[3], out var b))
						return false;

					color = new SKColor (
						(byte)(r << 4 | r),
						(byte)(g << 4 | g),
						(byte)(b << 4 | b),
						(byte)(a << 4 | a));
					return true;
				}
				case 6: {
					// #RRGGBB
					if (!TryParseByte (hexString[0], hexString[1], out var r) ||
						!TryParseByte (hexString[2], hexString[3], out var g) ||
						!TryParseByte (hexString[4], hexString[5], out var b))
						return false;

					color = new SKColor (r, g, b);
					return true;
				}
				case 8: {
					// #AARRGGBB
					if (!TryParseByte (hexString[0], hexString[1], out var a) ||
						!TryParseByte (hexString[2], hexString[3], out var r) ||
						!TryParseByte (hexString[4], hexString[5], out var g) ||
						!TryParseByte (hexString[6], hexString[7], out var b))
						return false;

					color = new SKColor (r, g, b, a);
					return true;
				}
				default:
					return false;
			}
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static bool TryParseNibble (char c, out byte value)
		{
			// Convert a single ASCII hex digit to its 0-15 value, case-insensitively
			// ('a'-'f' and 'A'-'F' both map to 10-15).
			if (c >= '0' && c <= '9') {
				value = (byte)(c - '0');
				return true;
			}
			if (c >= 'a' && c <= 'f') {
				value = (byte)(c - 'a' + 10);
				return true;
			}
			if (c >= 'A' && c <= 'F') {
				value = (byte)(c - 'A' + 10);
				return true;
			}

			value = 0;
			return false;
		}

		[MethodImpl (MethodImplOptions.AggressiveInlining)]
		private static bool TryParseByte (char hi, char lo, out byte value)
		{
			if (TryParseNibble (hi, out var h) && TryParseNibble (lo, out var l)) {
				value = (byte)(h << 4 | l);
				return true;
			}

			value = 0;
			return false;
		}
	}
}
