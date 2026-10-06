using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_fontarguments_palette_override_t
	/// <summary>Overrides a single color entry in a font color palette.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// `SKFontPaletteOverride` specifies a replacement <xref:SkiaSharp.SKColor> for a specific zero-based index within the palette selected by <xref:SkiaSharp.SKFontArguments.PaletteIndex>. An array of overrides is assigned to <xref:SkiaSharp.SKFontArguments.PaletteOverrides> before passing the arguments to <xref:SkiaSharp.SKTypeface.Clone*>.
	///
	/// The `Color` property stores a packed `0xAARRGGBB` value (same format as <xref:SkiaSharp.SKColor>).
	///
	/// ## Examples
	///
	/// Overriding the first palette entry to red:
	///
	/// ```csharp
	/// var overrides = new[]
	/// {
	///     new SKFontPaletteOverride { Index = 0, Color = (uint)SKColors.Red },
	/// };
	/// var args = new SKFontArguments { PaletteOverrides = overrides };
	/// using var typeface = original.Clone(args);
	/// ```
	/// ]]></remarks>
	[StructLayout (LayoutKind.Sequential)]
	public unsafe partial struct SKFontPaletteOverride : IEquatable<SKFontPaletteOverride> {
		// public uint16_t index
		private UInt16 index;
		/// <summary>Gets or sets the index of the palette color entry to override.</summary>
		/// <value>The zero-based index of the color entry within the palette to override.</value>
		/// <remarks></remarks>
		public UInt16 Index {
			readonly get => index;
			set => index = value;
		}

		// public sk_color_t color
		private UInt32 color;
		/// <summary>Gets or sets the replacement color for the overridden palette entry.</summary>
		/// <value>The replacement color for the palette entry, stored as a packed <c>0xAARRGGBB</c> value.</value>
		/// <remarks></remarks>
		public UInt32 Color {
			readonly get => color;
			set => color = value;
		}

		/// <summary>Indicates whether this palette override is equal to another <see cref="T:SkiaSharp.SKFontPaletteOverride" />.</summary>
		/// <param name="obj">The <see cref="T:SkiaSharp.SKFontPaletteOverride" /> to compare with this instance.</param>
		/// <returns><see langword="true" /> if both instances have the same index and color; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public readonly bool Equals (SKFontPaletteOverride obj) =>
#pragma warning disable CS8909
			index == obj.index && color == obj.color;
#pragma warning restore CS8909

		/// <summary>Indicates whether this palette override is equal to the specified object.</summary>
		/// <param name="obj">The object to compare with this instance.</param>
		/// <returns><see langword="true" /> if <paramref name="obj" /> is a <see cref="T:SkiaSharp.SKFontPaletteOverride" /> with the same index and color; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public readonly override bool Equals (object obj) =>
			obj is SKFontPaletteOverride f && Equals (f);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKFontPaletteOverride" /> values are equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKFontPaletteOverride" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKFontPaletteOverride" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are equal; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public static bool operator == (SKFontPaletteOverride left, SKFontPaletteOverride right) =>
			left.Equals (right);

		/// <summary>Determines whether two <see cref="T:SkiaSharp.SKFontPaletteOverride" /> values are not equal.</summary>
		/// <param name="left">The first <see cref="T:SkiaSharp.SKFontPaletteOverride" /> to compare.</param>
		/// <param name="right">The second <see cref="T:SkiaSharp.SKFontPaletteOverride" /> to compare.</param>
		/// <returns><see langword="true" /> if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, <see langword="false" />.</returns>
		/// <remarks></remarks>
		public static bool operator != (SKFontPaletteOverride left, SKFontPaletteOverride right) =>
			!left.Equals (right);

		/// <summary>Returns a hash code for this palette override.</summary>
		/// <returns>A hash code for this <see cref="T:SkiaSharp.SKFontPaletteOverride" /> instance.</returns>
		/// <remarks></remarks>
		public readonly override int GetHashCode ()
		{
			var hash = new HashCode ();
			hash.Add (index);
			hash.Add (color);
			return hash.ToHashCode ();
		}

	}
}
