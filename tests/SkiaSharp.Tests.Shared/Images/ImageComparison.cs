namespace SkiaSharp.Tests.Shared;

public sealed class ImageComparison
{
	internal ImageComparison(SKPixelComparisonResult pixels, ImageComparisonOptions options)
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
