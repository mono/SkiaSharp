using System;

namespace SkiaSharp;

public ref struct SKFontArguments
{
	public ReadOnlySpan<SKFontVariationPositionCoordinate> VariationDesignPosition { get; set; }

	public int CollectionIndex { get; set; }

	public int PaletteIndex { get; set; }

	public ReadOnlySpan<SKFontPaletteOverride> PaletteOverrides { get; set; }
}
