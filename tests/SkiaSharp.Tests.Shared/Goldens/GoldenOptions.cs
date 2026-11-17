namespace SkiaSharp.Tests.Shared;

/// <summary>Controls dimensions and pixel tolerances for one golden verification.</summary>
public sealed record GoldenOptions
{
	public const double DefaultMaxErrorPixelFraction = 0.00075;
	private static readonly GoldenOptions Default = new();

	public GoldenOptions(
		SKSizeI? expectedSize = null,
		int channelTolerance = 0,
		double maxErrorPixelFraction = DefaultMaxErrorPixelFraction,
		double? maxAverageColorErrorFraction = null,
		Func<SKImage, SKImage>? comparisonTransform = null)
	{
		if (channelTolerance < 0 || channelTolerance > byte.MaxValue)
			throw new ArgumentOutOfRangeException(nameof(channelTolerance));

		if (double.IsNaN(maxErrorPixelFraction) || maxErrorPixelFraction < 0 || maxErrorPixelFraction > 1)
			throw new ArgumentOutOfRangeException(nameof(maxErrorPixelFraction));

		if (maxAverageColorErrorFraction is { } average && (double.IsNaN(average) || average < 0 || average > 1))
			throw new ArgumentOutOfRangeException(nameof(maxAverageColorErrorFraction));

		ExpectedSize = expectedSize;
		ChannelTolerance = channelTolerance;
		MaxErrorPixelFraction = maxErrorPixelFraction;
		MaxAverageColorErrorFraction = maxAverageColorErrorFraction;
		ComparisonTransform = comparisonTransform;
	}

	public SKSizeI? ExpectedSize { get; }

	public int ChannelTolerance { get; }

	public double MaxErrorPixelFraction { get; }

	public double? MaxAverageColorErrorFraction { get; }

	/// <summary>Transforms both images for comparison, leaving captures and diagnostics unchanged.</summary>
	/// <remarks>The input is borrowed. Return it unchanged or return a new image for the verifier to dispose.</remarks>
	public Func<SKImage, SKImage>? ComparisonTransform { get; }

	internal static GoldenOptions OrDefault(GoldenOptions? options) => options ?? Default;
}
