using System;

namespace SkiaSharp.Tests.Shared;

/// <summary>Reports measured image differences and whether they satisfy the configured limits.</summary>
public sealed class GoldenComparison
{
	internal GoldenComparison(SKPixelComparisonResult pixels, GoldenComparisonOptions options)
	{
		Pixels = pixels;
		AverageColorErrorFraction = pixels.AbsoluteError / (pixels.TotalPixels * 4.0 * 255);
		AllowedErrorPixels = (long)Math.Floor(pixels.TotalPixels * options.MaxErrorPixelFraction);
		IsMatch = options.MaxAverageColorErrorFraction is { } average
			? AverageColorErrorFraction <= average
			: pixels.ErrorPixelCount <= AllowedErrorPixels;
	}

	public SKPixelComparisonResult Pixels { get; }
	public double AverageColorErrorFraction { get; }
	public long AllowedErrorPixels { get; }
	public bool IsMatch { get; }
}
