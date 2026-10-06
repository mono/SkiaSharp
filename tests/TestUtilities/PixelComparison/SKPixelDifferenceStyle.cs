namespace SkiaSharp.Testing;

/// <summary>Opaque diagnostic representations of a decoded pixel comparison.</summary>
public enum SKPixelDifferenceStyle
{
	/// <summary>Rejected pixels are white; all other pixels are black, using the same predicate as the comparison.</summary>
	BinaryMask,
	/// <summary>Raw absolute RGB channel differences only; alpha differences are not represented, regardless of channel selection.</summary>
	ChannelDelta,
	/// <summary>Rejected pixels are red, tolerated selected-channel differences amber, and exact selected-channel matches dimmed actual RGB.</summary>
	ThresholdOverlay,
}
