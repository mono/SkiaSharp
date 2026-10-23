namespace SkiaSharp.Tests.Shared;

/// <summary>Controls dimensions and error limits for a golden-image comparison.</summary>
public sealed class GoldenComparisonOptions
{
	/// <summary>Optional required dimensions for both images.</summary>
	public SKSizeI? ExpectedSize { get; set; }

	/// <summary>Maximum allowed absolute delta for each RGBA channel.</summary>
	public int ChannelTolerance { get; set; }

	/// <summary>Fraction of pixels allowed to exceed <see cref="ChannelTolerance"/>.</summary>
	public double MaxErrorPixelFraction { get; set; } = 0.00075;

	/// <summary>Optional normalized mean RGBA error limit, replacing the pixel-fraction limit.</summary>
	public double? MaxAverageColorErrorFraction { get; set; }
}
