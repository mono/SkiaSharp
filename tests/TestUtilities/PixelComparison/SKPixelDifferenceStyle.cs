namespace SkiaSharp.Testing;

/// <summary>Opaque diagnostic representations of a decoded pixel comparison.</summary>
public enum SKPixelDifferenceStyle
{
	/// <summary>Mismatched pixels are white; all other pixels are black, using the same predicate as the comparison.</summary>
	BinaryMask,
	/// <summary>Raw absolute RGB channel differences, including disabled RGB channels; alpha differences are never represented.</summary>
	ChannelDifference,
	/// <summary>Mismatched pixels are red, tolerated selected-channel differences amber, and exact selected-channel matches dimmed actual RGB.</summary>
	ThresholdOverlay,
}
