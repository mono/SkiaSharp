namespace SkiaSharp.Testing;

/// <summary>Opaque diagnostic representations of a decoded pixel comparison.</summary>
public enum SKPixelDifferenceStyle
{
	/// <summary>Mismatched pixels are white; all other pixels, including ignored coverage, are black, using the same predicate as the comparison.</summary>
	BinaryMask,
	/// <summary>Raw absolute RGB channel differences, including disabled RGB channels; ignored pixels are black and alpha differences are never represented.</summary>
	ChannelDifference,
	/// <summary>Mismatched pixels are red, tolerated selected-channel differences amber, exact selected-channel matches dimmed actual RGB, and ignored pixels neutral gray (128,128,128).</summary>
	ThresholdOverlay,
}
