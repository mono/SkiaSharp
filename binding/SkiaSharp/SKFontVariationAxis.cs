#nullable disable

using System;

namespace SkiaSharp;

/// <summary>
/// Utility class for creating OpenType tag values from four-character strings.
/// </summary>
public static class SKFontVariationTag
{
	/// <summary>
	/// Creates an OpenType tag from a four-character string (e.g. "wght", "wdth", "slnt").
	/// </summary>
	public static uint Make (string tag)
	{
		if (tag == null)
			throw new ArgumentNullException (nameof (tag));
		if (tag.Length != 4)
			throw new ArgumentException ("Tag must be exactly 4 characters.", nameof (tag));

		return ((uint)tag[0] << 24) | ((uint)tag[1] << 16) | ((uint)tag[2] << 8) | tag[3];
	}

	/// <summary>
	/// Converts an OpenType tag value to its four-character string representation.
	/// </summary>
	public static string GetName (uint tag) => new string (new[] {
		(char)((tag >> 24) & 0xFF),
		(char)((tag >> 16) & 0xFF),
		(char)((tag >> 8) & 0xFF),
		(char)(tag & 0xFF),
	});
}

public unsafe partial struct SKFontVariationAxis
{
	/// <summary>
	/// Gets the four-character OpenType tag name for this axis (e.g. "wght", "wdth", "slnt").
	/// </summary>
	public readonly string TagName => SKFontVariationTag.GetName (Tag);
}

public unsafe partial struct SKFontVariationDesignPositionCoordinate
{
	/// <summary>
	/// Gets the four-character OpenType tag name for this axis (e.g. "wght", "wdth", "slnt").
	/// </summary>
	public readonly string TagName => SKFontVariationTag.GetName (Axis);
}
