using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_fontarguments_variation_position_coordinate_t
	/// <summary>Represents a design-space value for a single variation axis.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// `SKFontVariationPositionCoordinate` pairs an axis tag with a design-space value, used to specify a font's position along that axis. Arrays of these coordinates are passed to <xref:SkiaSharp.SKTypeface.Clone*> to create a variation instance of a variable font.
	///
	/// Instances are also returned by <xref:SkiaSharp.SKTypeface.VariationDesignPosition> and <xref:SkiaSharp.SKTypeface.GetVariationDesignPosition*>.
	///
	/// ## Examples
	///
	/// Setting the weight axis to bold (700):
	///
	/// ```csharp
	/// var coord = new SKFontVariationPositionCoordinate
	/// {
	///     Axis = SKFourByteTag.Parse("wght"),
	///     Value = 700f,
	/// };
	/// ```
	/// ]]></remarks>
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKFontVariationPositionCoordinate : IEquatable<SKFontVariationPositionCoordinate> {
		// public sk_fourbytetag_t axis
		private SKFourByteTag axis;
		/// <summary>Gets or sets the four-byte tag of the variation axis this coordinate applies to.</summary>
		/// <value>The four-byte tag identifying the variation axis (for example, <c>wght</c> for weight).</value>
		/// <remarks></remarks>
		public SKFourByteTag Axis {
			readonly get => axis;
			set => axis = value;
		}

		// public float value
		private Single value;
		/// <summary>Gets or sets the design-space value for this axis coordinate.</summary>
		/// <value>The design-space value for the variation axis identified by <see cref="P:SkiaSharp.SKFontVariationPositionCoordinate.Axis" />.</value>
		/// <remarks></remarks>
		public Single Value {
			readonly get => this.value;
			set => this.value = value;
		}

		/// <summary>Indicates whether this coordinate is equal to another <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" />.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if both coordinates have the same axis tag and value; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public readonly bool Equals (SKFontVariationPositionCoordinate obj) =>
#pragma warning disable CS8909
			axis == obj.axis && value == obj.value;
#pragma warning restore CS8909

		/// <summary>Indicates whether this coordinate is equal to the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> with the same axis tag and value; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public readonly override bool Equals (object obj) =>
			obj is SKFontVariationPositionCoordinate f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> values are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public static bool operator == (SKFontVariationPositionCoordinate left, SKFontVariationPositionCoordinate right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> values are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public static bool operator != (SKFontVariationPositionCoordinate left, SKFontVariationPositionCoordinate right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this coordinate.</summary>
		/// <returns>A hash code for this <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> instance.</returns>
		/// <remarks></remarks>
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (axis);
			hash.Add (value);
			return hash.ToHashCode ();
		}

	}
}
