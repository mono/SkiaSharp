using System;

namespace SkiaSharp;

/// <summary>Specifies arguments for customizing a typeface clone, including the collection index, color palette, palette overrides, and variation design position.</summary>
/// <remarks><![CDATA[
/// ## Remarks
///
/// `SKFontArguments` is a `ref struct` that groups the parameters passed to <xref:SkiaSharp.SKTypeface.Clone*> when creating a customized typeface instance. Because it is a `ref struct`, it cannot be stored on the heap or used across async boundaries.
///
/// Typically you create an instance, set its properties, and pass it directly to `Clone`.
///
/// ## Examples
///
/// Cloning a variable font at a specific weight:
///
/// ```csharp
/// using var typeface = SKTypeface.FromFile("variable-font.ttf");
///
/// var wghtTag = SKFourByteTag.Parse("wght");
/// var args = new SKFontArguments
/// {
///     VariationDesignPosition = new[]
///     {
///         new SKFontVariationPositionCoordinate { Axis = wghtTag, Value = 700 },
///     },
/// };
/// using var bold = typeface.Clone(args);
/// ```
/// ]]></remarks>
public ref struct SKFontArguments
{
	/// <summary>Gets or sets the variation design position to apply to the cloned typeface.</summary>
	/// <value>A read-only span of <see cref="T:SkiaSharp.SKFontVariationPositionCoordinate" /> values specifying a design-space value for each variation axis.</value>
	/// <remarks />
	public ReadOnlySpan<SKFontVariationPositionCoordinate> VariationDesignPosition { get; set; }

	/// <summary>Gets or sets the index of the desired typeface within a font collection.</summary>
	/// <value>The zero-based index of the typeface within a TTC (TrueType Collection) font file.</value>
	/// <remarks />
	public int CollectionIndex { get; set; }

	/// <summary>Gets or sets the color palette index to use when cloning the typeface.</summary>
	/// <value>The zero-based index of the color palette from the font's CPAL table to use.</value>
	/// <remarks />
	public int PaletteIndex { get; set; }

	/// <summary>Gets or sets the per-entry color overrides to apply to the palette.</summary>
	/// <value>A read-only span of <see cref="T:SkiaSharp.SKFontPaletteOverride" /> values that override specific color entries in the chosen palette.</value>
	/// <remarks />
	public ReadOnlySpan<SKFontPaletteOverride> PaletteOverrides { get; set; }
}
