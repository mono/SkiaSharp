#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>Represents a 32-bit BGRA color value used by HarfBuzz.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// An `HBColor` stores a color as a packed 32-bit unsigned integer in BGRA byte order: bits 31–24 are blue, bits 23–16 are green, bits 15–8 are red, and bits 7–0 are alpha.
	///
	/// When formatted as a string, the color is expressed in `#AARRGGBB` hexadecimal notation matching the CSS convention.
	///
	/// ## Examples
	///
	/// Creating a fully opaque red color and reading its components:
	///
	/// ```csharp
	/// var red = new HBColor(255, 0, 0, 255);
	/// Console.WriteLine(red.Red);   // 255
	/// Console.WriteLine(red.Green); // 0
	/// Console.WriteLine(red.Blue);  // 0
	/// Console.WriteLine(red.Alpha); // 255
	/// Console.WriteLine(red);       // #FFFF0000
	/// ```
	/// ]]></remarks>
	public readonly struct HBColor : IEquatable<HBColor>
	{
		private readonly uint color;

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.HBColor" /> struct from a raw 32-bit BGRA value.</summary>
		/// <param name="value">The raw packed BGRA value to store.</param>
		/// <remarks />
		public HBColor (uint value)
		{
			color = value;
		}

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.HBColor" /> struct from individual RGBA channel values.</summary>
		/// <param name="red">The red channel value (0–255).</param>
		/// <param name="green">The green channel value (0–255).</param>
		/// <param name="blue">The blue channel value (0–255).</param>
		/// <param name="alpha">The alpha channel value (0–255), where 0 is fully transparent and 255 is fully opaque.</param>
		/// <remarks />
		public HBColor (byte red, byte green, byte blue, byte alpha)
		{
			color = (uint)((blue << 24) | (green << 16) | (red << 8) | alpha);
		}

		/// <summary>Gets the red component of the color.</summary>
		/// <value>The red channel value in the range [0, 255].</value>
		/// <remarks />
		public byte Red => (byte)((color >> 8) & 0xFF);

		/// <summary>Gets the green component of the color.</summary>
		/// <value>The green channel value in the range [0, 255].</value>
		/// <remarks />
		public byte Green => (byte)((color >> 16) & 0xFF);

		/// <summary>Gets the blue component of the color.</summary>
		/// <value>The blue channel value in the range [0, 255].</value>
		/// <remarks />
		public byte Blue => (byte)((color >> 24) & 0xFF);

		/// <summary>Gets the alpha component of the color.</summary>
		/// <value>The alpha channel value in the range [0, 255], where 0 is fully transparent and 255 is fully opaque.</value>
		/// <remarks />
		public byte Alpha => (byte)(color & 0xFF);

		/// <summary>Gets the underlying packed BGRA value of this color.</summary>
		/// <value>The raw packed 32-bit BGRA value of this color.</value>
		/// <remarks />
		public uint Value => color;

		/// <summary>Implicitly converts an <see cref="T:HarfBuzzSharp.HBColor" /> to a packed BGRA integer value.</summary>
		/// <param name="color">The <see cref="T:HarfBuzzSharp.HBColor" /> to convert.</param>
		/// <returns>The packed BGRA integer value of the color.</returns>
		/// <remarks />
		public static implicit operator uint (HBColor color) => color.color;

		/// <summary>Explicitly converts a packed BGRA integer value to an <see cref="T:HarfBuzzSharp.HBColor" />.</summary>
		/// <param name="value">The packed BGRA integer value to convert.</param>
		/// <returns>An <see cref="T:HarfBuzzSharp.HBColor" /> with the specified packed value.</returns>
		/// <remarks />
		public static explicit operator HBColor (uint value) => new HBColor (value);

		/// <summary>Indicates whether this color is equal to another <see cref="T:HarfBuzzSharp.HBColor" />.</summary>
		/// <param name="other">The <see cref="T:HarfBuzzSharp.HBColor" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if the two colors have the same packed value; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Equals (HBColor other) => color == other.color;

		/// <summary>Determines whether the specified object is equal to the current color.</summary>
		/// <param name="obj">The object to compare with the current color.</param>
		/// <returns><see langword="true" /> if the specified object is equal to the current color; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public override bool Equals (object obj) => obj is HBColor other && Equals (other);

		/// <summary>Returns a hash code for this color.</summary>
		/// <returns>A hash code for this <see cref="T:HarfBuzzSharp.HBColor" /> instance.</returns>
		/// <remarks />
		public override int GetHashCode () => color.GetHashCode ();

		/// <summary>Determines whether two <see cref="T:HarfBuzzSharp.HBColor" /> values are equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.HBColor" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.HBColor" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> have the same packed value; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator == (HBColor left, HBColor right) => left.Equals (right);

		/// <summary>Determines whether two <see cref="T:HarfBuzzSharp.HBColor" /> values are not equal.</summary>
		/// <param name="left">The first <see cref="T:HarfBuzzSharp.HBColor" /> to compare.</param>
		/// <param name="right">The second <see cref="T:HarfBuzzSharp.HBColor" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> have different packed values; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public static bool operator != (HBColor left, HBColor right) => !left.Equals (right);

		/// <summary>Returns a hexadecimal string representation of this color.</summary>
		/// <returns>A string in <c>#AARRGGBB</c> hexadecimal format representing this color.</returns>
		/// <remarks />
		public override string ToString () =>
			$"#{Alpha:X2}{Red:X2}{Green:X2}{Blue:X2}";
	}
}
