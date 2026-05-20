#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>
	/// Represents a HarfBuzz color value (<c>hb_color_t</c>) with BGRA byte ordering.
	/// </summary>
	/// <remarks>
	/// HarfBuzz stores colors as <c>0xBBGGRRAA</c> (Blue in high byte, Alpha in low byte).
	/// This differs from <c>SKColor</c> which uses ARGB: <c>0xAARRGGBB</c>.
	/// Use the named properties (<see cref="Red"/>, <see cref="Green"/>, <see cref="Blue"/>,
	/// <see cref="Alpha"/>) to correctly access color channels.
	/// </remarks>
	public readonly struct HBColor : IEquatable<HBColor>
	{
		private readonly uint color;

		/// <summary>
		/// Creates an <see cref="HBColor"/> from a raw <c>hb_color_t</c> value.
		/// </summary>
		public HBColor (uint value)
		{
			color = value;
		}

		/// <summary>
		/// Creates an <see cref="HBColor"/> from individual RGBA components.
		/// </summary>
		public HBColor (byte red, byte green, byte blue, byte alpha)
		{
			color = (uint)((blue << 24) | (green << 16) | (red << 8) | alpha);
		}

		/// <summary>
		/// Gets the red channel value.
		/// </summary>
		public byte Red => (byte)((color >> 8) & 0xFF);

		/// <summary>
		/// Gets the green channel value.
		/// </summary>
		public byte Green => (byte)((color >> 16) & 0xFF);

		/// <summary>
		/// Gets the blue channel value.
		/// </summary>
		public byte Blue => (byte)((color >> 24) & 0xFF);

		/// <summary>
		/// Gets the alpha channel value.
		/// </summary>
		public byte Alpha => (byte)(color & 0xFF);

		/// <summary>
		/// Gets the raw <c>hb_color_t</c> value (BGRA byte ordering: <c>0xBBGGRRAA</c>).
		/// </summary>
		public uint Value => color;

		public static implicit operator uint (HBColor color) => color.color;

		public static explicit operator HBColor (uint value) => new HBColor (value);

		public bool Equals (HBColor other) => color == other.color;

		public override bool Equals (object obj) => obj is HBColor other && Equals (other);

		public override int GetHashCode () => color.GetHashCode ();

		public static bool operator == (HBColor left, HBColor right) => left.Equals (right);

		public static bool operator != (HBColor left, HBColor right) => !left.Equals (right);

		public override string ToString () =>
			$"#{Alpha:X2}{Red:X2}{Green:X2}{Blue:X2}";
	}
}
