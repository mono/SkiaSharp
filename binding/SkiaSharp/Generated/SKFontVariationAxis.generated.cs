using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_fontarguments_variation_axis_t
	/// <summary>Describes a single variation axis defined in a variable font.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// `SKFontVariationAxis` describes one axis from the `fvar` table of a variable font, including its four-byte tag identifier, minimum, default, and maximum design-space values, and whether the axis should be hidden from the user.
	///
	/// Instances are returned by <xref:SkiaSharp.SKTypeface.VariationDesignParameters> and <xref:SkiaSharp.SKTypeface.GetVariationDesignParameters*>.
	///
	/// ## Examples
	///
	/// Listing the variation axes of a variable font:
	///
	/// ```csharp
	/// using var typeface = SKTypeface.FromFile("variable-font.ttf");
	/// foreach (var axis in typeface.VariationDesignParameters)
	/// {
	///     Console.WriteLine($"{axis.Tag}: [{axis.Min}, {axis.Default}, {axis.Max}] hidden={axis.IsHidden}");
	/// }
	/// ```
	/// ]]></remarks>
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKFontVariationAxis : IEquatable<SKFontVariationAxis> {
		// public sk_fourbytetag_t tag
		private SKFourByteTag tag;
		/// <summary>Gets or sets the four-byte tag that identifies this variation axis.</summary>
		/// <value>The four-byte OpenType tag identifying this variation axis (for example, <c>wght</c> for weight or <c>wdth</c> for width).</value>
		/// <remarks></remarks>
		public SKFourByteTag Tag {
			readonly get => tag;
			set => tag = value;
		}

		// public float min
		private Single min;
		/// <summary>Gets or sets the minimum design-space value for this variation axis.</summary>
		/// <value>The minimum design-space value for this axis.</value>
		/// <remarks></remarks>
		public Single Min {
			readonly get => min;
			set => min = value;
		}

		// public float def
		private Single def;
		/// <summary>Gets or sets the default design-space value for this variation axis.</summary>
		/// <value>The default design-space value for this axis.</value>
		/// <remarks></remarks>
		public Single Default {
			readonly get => def;
			set => def = value;
		}

		// public float max
		private Single max;
		/// <summary>Gets or sets the maximum design-space value for this variation axis.</summary>
		/// <value>The maximum design-space value for this axis.</value>
		/// <remarks></remarks>
		public Single Max {
			readonly get => max;
			set => max = value;
		}

		// public bool isHidden
		private Byte isHidden;
		/// <summary>Gets or sets a value indicating whether this variation axis is intended to be hidden from the user.</summary>
		/// <value><see langword="true" /> if this axis should not be presented in a user interface; otherwise, <see langword="false" />.</value>
		/// <remarks></remarks>
		public bool IsHidden {
			readonly get => isHidden > 0;
			set => isHidden = value ? (byte)1 : (byte)0;
		}

		/// <summary>Indicates whether this variation axis is equal to another <see cref="T:SkiaSharp.SKFontVariationAxis" />.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKFontVariationAxis" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if both axes have the same tag, range, and hidden flag; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public readonly bool Equals (SKFontVariationAxis obj) =>
#pragma warning disable CS8909
			tag == obj.tag && min == obj.min && def == obj.def && max == obj.max && isHidden == obj.isHidden;
#pragma warning restore CS8909

		/// <summary>Indicates whether this variation axis is equal to the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.SKFontVariationAxis" /> with the same values; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public readonly override bool Equals (object obj) =>
			obj is SKFontVariationAxis f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKFontVariationAxis" /> values are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKFontVariationAxis" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKFontVariationAxis" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public static bool operator == (SKFontVariationAxis left, SKFontVariationAxis right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKFontVariationAxis" /> values are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKFontVariationAxis" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKFontVariationAxis" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public static bool operator != (SKFontVariationAxis left, SKFontVariationAxis right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this variation axis.</summary>
		/// <returns>A hash code for this <see cref="T:SkiaSharp.SKFontVariationAxis" /> instance.</returns>
		/// <remarks></remarks>
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (tag);
			hash.Add (min);
			hash.Add (def);
			hash.Add (max);
			hash.Add (isHidden);
			return hash.ToHashCode ();
		}

	}
}
