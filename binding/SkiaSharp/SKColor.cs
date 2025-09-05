#nullable disable

using System;
using System.Globalization;

namespace SkiaSharp
{
	/// <summary>
	/// 32-bit ARGB unpremultiplied color value.
	/// </summary>
	/// <remarks>
	/// The color components are always in a known order.
	/// </remarks>
	public readonly struct SKColor : IEquatable<SKColor>
	{
		/// <summary>
		/// Gets an "empty" color, with zero for all the components.
		/// </summary>
		public static readonly SKColor Empty;

		private readonly uint color;

		/// <summary>
		/// Creates a color from the specified integer.
		/// </summary>
		/// <param name="value">The integer value of the unpremultiplied color.</param>
		public SKColor (uint value)
		{
			color = value;
		}

		/// <summary>
		/// Creates a color from the specified red, green, blue and alpha components.
		/// </summary>
		/// <param name="red">The red component.</param>
		/// <param name="green">The green component.</param>
		/// <param name="blue">The blue component.</param>
		/// <param name="alpha">The alpha component.</param>
		public SKColor (byte red, byte green, byte blue, byte alpha)
		{
			color = (uint)((alpha << 24) | (red << 16) | (green << 8) | blue);
		}

		/// <summary>
		/// Creates a color from the specified red, green and blue components.
		/// </summary>
		/// <param name="red">The red component.</param>
		/// <param name="green">The green component.</param>
		/// <param name="blue">The blue component.</param>
		public SKColor (byte red, byte green, byte blue)
		{
			color = (0xff000000u | (uint)(red << 16) | (uint)(green << 8) | blue);
		}

		/// <summary>
		/// Returns a new color based on this current instance, but with the new red channel value.
		/// </summary>
		/// <param name="red">The new red component.</param>
		public readonly SKColor WithRed (byte red) =>
			new SKColor (red, Green, Blue, Alpha);

		/// <summary>
		/// Returns a new color based on this current instance, but with the new green channel value.
		/// </summary>
		/// <param name="green">The new green component.</param>
		public readonly SKColor WithGreen (byte green) =>
			new SKColor (Red, green, Blue, Alpha);

		/// <summary>
		/// Returns a new color based on this current instance, but with the new blue channel value.
		/// </summary>
		/// <param name="blue">The new blue component.</param>
		public readonly SKColor WithBlue (byte blue) =>
			new SKColor (Red, Green, blue, Alpha);

		/// <summary>
		/// Returns a new color based on this current instance, but with the new alpha channel value.
		/// </summary>
		/// <param name="alpha">The new alpha component.</param>
		public readonly SKColor WithAlpha (byte alpha) =>
			new SKColor (Red, Green, Blue, alpha);

		/// <summary>
		/// Gets the alpha component of the color.
		/// </summary>
		public readonly byte Alpha => (byte)((color >> 24) & 0xff);
		/// <summary>
		/// Gets the red component of the color.
		/// </summary>
		public readonly byte Red => (byte)((color >> 16) & 0xff);
		/// <summary>
		/// Gets the green component of the color.
		/// </summary>
		public readonly byte Green => (byte)((color >> 8) & 0xff);
		/// <summary>
		/// Gets the blue component of the color.
		/// </summary>
		public readonly byte Blue => (byte)((color) & 0xff);

		/// <summary>
		/// Gets the hue value.
		/// </summary>
		public readonly float Hue {
			get {
				ToHsv (out var h, out _, out _);
				return h;
			}
		}

		/// <summary>
		/// Creates a color from the specified hue, saturation, lightness/luminosity and alpha values.
		/// </summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="l">The lightness/luminosity value.</param>
		/// <param name="a">The alpha value.</param>
		/// <returns>The new <see cref="SKColor" /> instance.</returns>
		public static SKColor FromHsl (float h, float s, float l, byte a = 255)
		{
			var colorf = SKColorF.FromHsl (h, s, l);

			// RGB results from 0 to 255
			var r = colorf.Red * 255f;
			var g = colorf.Green * 255f;
			var b = colorf.Blue * 255f;

			return new SKColor ((byte)r, (byte)g, (byte)b, a);
		}

		/// <summary>
		/// Creates a color from the specified hue, saturation, value/brightness and alpha values.
		/// </summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="v">The value/brightness value.</param>
		/// <param name="a">The alpha value.</param>
		/// <returns>The new <see cref="SKColor" /> instance.</returns>
		public static SKColor FromHsv (float h, float s, float v, byte a = 255)
		{
			var colorf = SKColorF.FromHsv (h, s, v);

			// RGB results from 0 to 255
			var r = colorf.Red * 255f;
			var g = colorf.Green * 255f;
			var b = colorf.Blue * 255f;

			return new SKColor ((byte)r, (byte)g, (byte)b, a);
		}

		/// <summary>
		/// Converts the current color into it's hue, saturation and lightness/luminosity values.
		/// </summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="l">The lightness/luminosity value.</param>
		/// <remarks>
		/// The alpha value is separate from the HSL calculation and will always be the same as <see cref="SKColor.Alpha" />.
		/// </remarks>
		public readonly void ToHsl (out float h, out float s, out float l)
		{
			// RGB from 0 to 255
			var r = Red / 255f;
			var g = Green / 255f;
			var b = Blue / 255f;

			var colorf = new SKColorF (r, g, b);
			colorf.ToHsl (out h, out s, out l);
		}

		/// <summary>
		/// Converts the current color into it's hue, saturation and value/brightness values.
		/// </summary>
		/// <param name="h">The hue value.</param>
		/// <param name="s">The saturation value.</param>
		/// <param name="v">The value/brightness value.</param>
		/// <remarks>
		/// The alpha value is separate from the HSV/HSB calculation and will always be the same as <see cref="SKColor.Alpha" />.
		/// </remarks>
		public readonly void ToHsv (out float h, out float s, out float v)
		{
			// RGB from 0 to 255
			var r = Red / 255f;
			var g = Green / 255f;
			var b = Blue / 255f;

			var colorf = new SKColorF (r, g, b);
			colorf.ToHsv (out h, out s, out v);
		}

		/// <summary>
		/// Returns the color as a string in the format: #AARRGGBB.
		/// </summary>
		public readonly override string ToString () =>
			$"#{Alpha:x2}{Red:x2}{Green:x2}{Blue:x2}";

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj">The color to compare with the current color.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
		public readonly bool Equals (SKColor obj) =>
			obj.color == color;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
		public readonly override bool Equals (object other) =>
			other is SKColor f && Equals (f);

		/// <summary>
		/// Indicates whether two <see cref="SKColor" /> objects are equal.
		/// </summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		/// <returns>Returns <see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
		public static bool operator == (SKColor left, SKColor right) =>
			left.Equals (right);

		/// <summary>
		/// Indicates whether two <see cref="SKColor" /> objects are different.
		/// </summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		/// <returns>Returns <see langword="true" /> if <paramref name="left" /> is not equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
		public static bool operator != (SKColor left, SKColor right) =>
			!left.Equals (right);

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>Returns a hash code for the current object.</returns>
		public readonly override int GetHashCode () =>
			color.GetHashCode ();

		/// <summary>
		/// Converts a UInt32 to a <see cref="SKColor" />.
		/// </summary>
		/// <param name="color">The UInt32 representation of a color.</param>
		/// <returns>The new <see cref="SKColor" /> instance.</returns>
		public static implicit operator SKColor (uint color) =>
			new SKColor (color);

		/// <summary>
		/// Converts a <see cref="SKColor" /> to a UInt32.
		/// </summary>
		/// <param name="color">The color to convert.</param>
		/// <returns>The UInt32 value for the color.</returns>
		public static explicit operator uint (SKColor color) =>
			color.color;

		/// <summary>
		/// Converts the hexadecimal string representation of a color to its <see cref="SKColor" /> equivalent.
		/// </summary>
		/// <param name="hexString">The hexadecimal string representation of a color.</param>
		/// <returns>The new <see cref="SKColor" /> instance.</returns>
		/// <remarks>
		/// This method can parse a string in the forms with or without a preceding '#' character: AARRGGB, RRGGBB, ARGB, RGB.
		/// </remarks>
		public static SKColor Parse (string hexString)
		{
			if (!TryParse (hexString, out var color))
				throw new ArgumentException ("Invalid hexadecimal color string.", nameof (hexString));
			return color;
		}

		/// <summary>
		/// Converts the hexadecimal string representation of a color to its <see cref="SKColor" /> equivalent.
		/// </summary>
		/// <param name="hexString">The hexadecimal string representation of a color.</param>
		/// <param name="color">The new <see cref="SKColor" /> instance.</param>
		/// <returns>Returns true if the conversion was successful, otherwise false.</returns>
		/// <remarks>
		/// This method can parse a string in the forms with or without a preceding '#' character: AARRGGB, RRGGBB, ARGB, RGB.
		/// </remarks>
		public static bool TryParse (string hexString, out SKColor color)
		{
			if (string.IsNullOrWhiteSpace (hexString)) {
				// error
				color = SKColor.Empty;
				return false;
			}

#if NETCOREAPP3_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
			// clean up string
			var hexSpan = hexString.AsSpan ().Trim ().TrimStart ('#');

			var len = hexSpan.Length;
			if (len == 3 || len == 4) {
				byte a;
				// parse [A]
				if (len == 4) {
					if (!byte.TryParse (hexSpan.Slice (0, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out a)) {
						// error
						color = SKColor.Empty;
						return false;
					}
					a = (byte)(a << 4 | a);
				} else {
					a = 255;
				}

				// parse RGB
				if (!byte.TryParse (hexSpan.Slice (len - 3, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) ||
					!byte.TryParse (hexSpan.Slice (len - 2, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) ||
					!byte.TryParse (hexSpan.Slice (len - 1, 1), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b)) {
					// error
					color = SKColor.Empty;
					return false;
				}

				// success
				color = new SKColor ((byte)(r << 4 | r), (byte)(g << 4 | g), (byte)(b << 4 | b), a);
				return true;
			}

			if (len == 6 || len == 8) {
				// parse [AA]RRGGBB
				if (!uint.TryParse (hexSpan, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var number)) {
					// error
					color = SKColor.Empty;
					return false;
				}

				// success
				color = (SKColor)number;

				// alpha was not provided, so use 255
				if (len == 6) {
					color = color.WithAlpha (255);
				}
				return true;
			}
#else
			// clean up string
			hexString = hexString.Trim ();
			var startIndex = hexString[0] == '#' ? 1 : 0;

			var len = hexString.Length - startIndex;
			if (len == 3 || len == 4) {
				byte a;
				// parse [A]
				if (len == 4) {
					if (!byte.TryParse (string.Concat (new string (hexString[len - 4 + startIndex], 2)), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out a)) {
						// error
						color = SKColor.Empty;
						return false;
					}
				} else {
					a = 255;
				}

				// parse RGB
				if (!byte.TryParse (new string (hexString[len - 3 + startIndex], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var r) ||
					!byte.TryParse (new string (hexString[len - 2 + startIndex], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var g) ||
					!byte.TryParse (new string (hexString[len - 1 + startIndex], 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var b)) {
					// error
					color = SKColor.Empty;
					return false;
				}

				// success
				color = new SKColor (r, g, b, a);
				return true;
			}

			if (len == 6 || len == 8) {
				// parse [AA]RRGGBB
				if (!uint.TryParse (hexString.Substring (startIndex), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var number)) {
					// error
					color = SKColor.Empty;
					return false;
				}

				// success
				color = (SKColor)number;

				// alpha was not provided, so use 255
				if (len == 6) {
					color = color.WithAlpha (255);
				}
				return true;
			}
#endif

			// error
			color = SKColor.Empty;
			return false;
		}
	}
}
