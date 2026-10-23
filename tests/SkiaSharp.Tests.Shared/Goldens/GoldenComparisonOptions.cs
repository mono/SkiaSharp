namespace SkiaSharp.Tests.Shared;

public sealed class GoldenComparisonOptions
{
	public SKSizeI? ExpectedSize { get; set; }
	public int ChannelTolerance { get; set; }
	public double MaxErrorPixelFraction { get; set; } = 0.00075;
	public double? MaxAverageColorErrorFraction { get; set; }

	internal void Validate()
	{
		if (ChannelTolerance < 0 || ChannelTolerance > byte.MaxValue)
			throw new ArgumentOutOfRangeException(nameof(ChannelTolerance));
		if (double.IsNaN(MaxErrorPixelFraction) || MaxErrorPixelFraction < 0 || MaxErrorPixelFraction > 1)
			throw new ArgumentOutOfRangeException(nameof(MaxErrorPixelFraction));
		if (MaxAverageColorErrorFraction is { } average &&
			(double.IsNaN(average) || average < 0 || average > 1))
			throw new ArgumentOutOfRangeException(nameof(MaxAverageColorErrorFraction));
	}
}
