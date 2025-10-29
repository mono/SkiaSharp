#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// 32-bit ARGB premultiplied color value.
	/// </summary>
	/// <remarks>
	/// The byte order for this value is configuration dependent. This is different from <see cref="SKColor" />, which is unpremultiplied, and is always in the same byte order.
	/// </remarks>
	public readonly unsafe struct SKPMColor : IEquatable<SKPMColor>
	{
		private readonly uint color;

		/// <summary>
		/// Creates a color from the specified integer.
		/// </summary>
		/// <param name="value">The integer value of the premultiplied color.</param>
		public SKPMColor (uint value)
		{
			color = value;
		}

		/// <summary>
		/// Gets the alpha component of the color.
		/// </summary>
		public readonly byte Alpha => (byte)((color >> SKImageInfo.PlatformColorAlphaShift) & 0xff);
		/// <summary>
		/// Gets the red component of the color.
		/// </summary>
		public readonly byte Red => (byte)((color >> SKImageInfo.PlatformColorRedShift) & 0xff);
		/// <summary>
		/// Gets the green component of the color.
		/// </summary>
		public readonly byte Green => (byte)((color >> SKImageInfo.PlatformColorGreenShift) & 0xff);
		/// <summary>
		/// Gets the blue component of the color.
		/// </summary>
		public readonly byte Blue => (byte)((color >> SKImageInfo.PlatformColorBlueShift) & 0xff);

		// PreMultiply

		/// <summary>
		/// Converts an unpremultiplied <see cref="SKColor" /> to a premultiplied <see cref="SKPMColor" />.
		/// </summary>
		/// <param name="color">The unpremultiplied color to convert.</param>
		/// <returns>Returns the new premultiplied <see cref="SKPMColor" />.</returns>
		public static SKPMColor PreMultiply (SKColor color) =>
			SkiaApi.sk_color_premultiply ((uint)color);

		/// <summary>
		/// Converts an array of unpremultiplied <see cref="SKColor" />s to an array of premultiplied <see cref="SKPMColor" />s.
		/// </summary>
		/// <param name="colors">The unpremultiplied colors to convert.</param>
		/// <returns>Returns the new array of premultiplied <see cref="SKPMColor" />s.</returns>
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

		/// <summary>
		/// Converts a premultiplied <see cref="SKPMColor" /> to the unpremultiplied <see cref="SKColor" />.
		/// </summary>
		/// <param name="pmcolor">The premultiplied color to convert.</param>
		/// <returns>Returns the new unpremultiplied <see cref="SKColor" />.</returns>
		public static SKColor UnPreMultiply (SKPMColor pmcolor) =>
			SkiaApi.sk_color_unpremultiply ((uint)pmcolor);

		/// <summary>
		/// Converts an array of premultiplied <see cref="SKPMColor" />s to an array of unpremultiplied <see cref="SKColor" />s.
		/// </summary>
		/// <param name="pmcolors">The premultiplied colors to convert.</param>
		/// <returns>Returns the new array of unpremultiplied <see cref="SKColor" />s.</returns>
		public static SKColor[] UnPreMultiply (SKPMColor[] pmcolors)
		{
			var colors = new SKColor[pmcolors.Length];
			fixed (SKColor* c = colors)
			fixed (SKPMColor* pm = pmcolors) {
				SkiaApi.sk_color_unpremultiply_array ((uint*)pm, pmcolors.Length, (uint*)c);
			}
			return colors;
		}

		/// <summary>
		/// Converts an unpremultiplied <see cref="SKColor" /> to the premultiplied <see cref="SKPMColor" />.
		/// </summary>
		/// <param name="color">The unpremultiplied color to convert.</param>
		/// <returns>Returns the new premultiplied <see cref="SKPMColor" />.</returns>
		public static explicit operator SKPMColor (SKColor color) =>
			SKPMColor.PreMultiply (color);

		/// <summary>
		/// Converts a premultiplied <see cref="SKPMColor" /> to the unpremultiplied <see cref="SKColor" />.
		/// </summary>
		/// <param name="color">The premultiplied color to convert.</param>
		/// <returns>Returns the new unpremultiplied <see cref="SKColor" />.</returns>
		public static explicit operator SKColor (SKPMColor color) =>
			SKPMColor.UnPreMultiply (color);

		/// <summary>
		/// Returns the color as a string in the format: #AARRGGBB.
		/// </summary>
		public readonly override string ToString () =>
			$"#{Alpha:x2}{Red:x2}{Green:x2}{Blue:x2}";

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="obj"></param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
		public readonly bool Equals (SKPMColor obj) =>
			obj.color == color;

		/// <summary>
		/// Determines whether the specified object is equal to the current object.
		/// </summary>
		/// <param name="other">The object to compare with the current object.</param>
		/// <returns>Returns <see langword="true" /> if the specified object is equal to the current object; otherwise, <see langword="false" />.</returns>
		public readonly override bool Equals (object other) =>
			other is SKPMColor f && Equals (f);

		/// <summary>
		/// Indicates whether two <see cref="SKPMColor" /> objects are equal.
		/// </summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		/// <returns>Returns <see langword="true" /> if <paramref name="left" /> is equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
		public static bool operator == (SKPMColor left, SKPMColor right) =>
			left.Equals (right);

		/// <summary>
		/// Indicates whether two <see cref="SKPMColor" /> objects are different.
		/// </summary>
		/// <param name="left">The first color to compare.</param>
		/// <param name="right">The second color to compare.</param>
		/// <returns>Returns <see langword="true" /> if <paramref name="left" /> is not equal to <paramref name="right" />, otherwise <see langword="false" />.</returns>
		public static bool operator != (SKPMColor left, SKPMColor right) =>
			!left.Equals (right);

		/// <summary>
		/// Serves as the default hash function.
		/// </summary>
		/// <returns>Returns a hash code for the current object.</returns>
		public readonly override int GetHashCode () =>
			color.GetHashCode ();

		/// <summary>
		/// Converts a UInt32 to a <see cref="SKPMColor" />.
		/// </summary>
		/// <param name="color">The UInt32 representation of a color.</param>
		/// <returns>The new <see cref="SKPMColor" /> instance.</returns>
		public static implicit operator SKPMColor (uint color) =>
			new SKPMColor (color);

		/// <summary>
		/// Converts a <see cref="SKPMColor" /> to a UInt32.
		/// </summary>
		/// <param name="color">The color to convert.</param>
		/// <returns>The UInt32 value for the color.</returns>
		public static explicit operator uint (SKPMColor color) =>
			color.color;
	}
}
