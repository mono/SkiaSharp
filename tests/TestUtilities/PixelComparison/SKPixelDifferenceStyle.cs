namespace SkiaSharp.Testing;

/// <summary>Diagnostic representations of a decoded pixel comparison.</summary>
internal enum SKPixelDifferenceStyle
{
	/// <summary>Rejected pixels are white; all other pixels are black.</summary>
	BinaryMask,
	/// <summary>Opaque RGB contains raw absolute RGB channel differences, independently of tolerance and alpha.</summary>
	ChannelDelta,
	/// <summary>Rejected pixels are red, tolerated differences amber, and exact selected-channel matches dimmed actual RGB.</summary>
	ThresholdOverlay,
}
