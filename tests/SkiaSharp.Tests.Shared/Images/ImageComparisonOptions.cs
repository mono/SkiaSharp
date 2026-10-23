namespace SkiaSharp.Tests.Shared;

/// <summary>Controls dimensions and error limits for an image comparison.</summary>
public sealed class ImageComparisonOptions
{
	public SKSizeI? ExpectedSize { get; set; }
	public int ChannelTolerance { get; set; }
	public double MaxErrorPixelFraction { get; set; } = 0.00075;
	public double? MaxAverageColorErrorFraction { get; set; }
}
