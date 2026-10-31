namespace SkiaSharp.Tests.Shared;

/// <summary>Controls dimensions and pixel tolerances for one golden verification.</summary>
public sealed record GoldenOptions
{
	public const double DefaultMaxErrorPixelFraction = 0.00075;
	private static readonly GoldenOptions Default = new();

	/// <summary>Creates comparison options with an optional image transform that does not change the original images or diagnostics.</summary>
	/// <param name="expectedSize">The required original image dimensions, before transformation.</param>
	/// <param name="channelTolerance">The maximum per-channel delta that does not count as a differing pixel.</param>
	/// <param name="maxErrorPixelFraction">The allowed fraction of differing pixels in the comparison region.</param>
	/// <param name="maxAverageColorErrorFraction">An optional mean RGBA error limit over the comparison region.</param>
	/// <param name="comparisonTransform">An optional transform applied separately to both comparison images.
	/// The input is borrowed and must not be disposed. Return it unchanged or return an image owned by the verifier.</param>
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

	/// <summary>A transform applied to both comparison images only. Original dimensions, attachments, and reference proposals are retained.</summary>
	public Func<SKImage, SKImage>? ComparisonTransform { get; }

	internal static GoldenOptions OrDefault(GoldenOptions? options) => options ?? Default;
}
