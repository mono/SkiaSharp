namespace SkiaSharp.Tests.Shared;

/// <summary>Contains aggregate pixel differences for two same-sized images.</summary>
public sealed class SKPixelComparisonResult
{
	public SKPixelComparisonResult(int totalPixels, int errorPixelCount, int absoluteError)
		: this(totalPixels, errorPixelCount, absoluteError, 0)
	{
	}

	public SKPixelComparisonResult(int totalPixels, int errorPixelCount, int absoluteError, int maxChannelDelta)
	{
		TotalPixels = totalPixels;
		ErrorPixelCount = errorPixelCount;
		AbsoluteError = absoluteError;
		MaxChannelDelta = maxChannelDelta;
	}

	public int TotalPixels { get; }

	public int ErrorPixelCount { get; }

	public double ErrorPixelPercentage =>
		(double)ErrorPixelCount / TotalPixels;

	public int AbsoluteError { get; }

	public double AverageColorErrorFraction =>
		AbsoluteError / (TotalPixels * 4.0 * 255);

	// The largest per-channel delta observed across all pixels. Only populated
	// by the tolerance-aware Compare overload; 0 otherwise.
	public int MaxChannelDelta { get; }

}
