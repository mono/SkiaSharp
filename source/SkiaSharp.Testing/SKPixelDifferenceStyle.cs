namespace SkiaSharp.Testing;

/// <summary>Opaque diagnostic representations of a decoded pixel comparison.</summary>
public enum SKPixelDifferenceStyle
{
	/// <summary>Mismatched pixels are white; all other pixels, including ignored coverage, are black, using the same predicate as the comparison.</summary>
	BinaryMask = 0,
	/// <summary>Raw absolute RGB channel differences, including disabled RGB channels; ignored pixels are black and alpha differences are never represented.</summary>
	ChannelDifference = 1,
	/// <summary>Mismatched pixels are red, tolerated selected-channel differences amber (255,200,0), exact selected-channel matches dimmed actual RGB, and ignored pixels neutral gray (128,128,128).</summary>
	/// <remarks>The actual image is the second input. Exact-match RGB channels are divided by four, regardless of channel selection and without flattening transparency. Output alpha is always 255.</remarks>
	ThresholdMap = 2,
	/// <summary>Mismatched pixels are tinted red, tolerated selected-channel differences amber (255,200,0), and ignored pixels neutral gray (128,128,128), each blended at 50% over the actual image; exact selected-channel matches retain the actual image.</summary>
	/// <remarks>The actual image is the second input, flattened onto white before blending. All RGB channels remain visible regardless of channel selection. Half-channel values round upward and output alpha is always 255.</remarks>
	ThresholdOverlay = 3,
	/// <summary>Largest raw absolute selected-channel difference on a fixed 0-255 scale: black at zero, then blue through cyan and yellow to red; ignored pixels are gray (128,128,128).</summary>
	/// <remarks>Tolerances do not suppress magnitudes. Output alpha is always 255.</remarks>
	Heatmap = 4,
}
