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

	public long AllowedErrorPixels(GoldenComparisonOptions options)
	{
		if (options == null) throw new ArgumentNullException(nameof(options));
		options.Validate();
		return (long)Math.Floor(TotalPixels * options.MaxErrorPixelFraction);
	}

	public bool IsMatch(GoldenComparisonOptions options)
	{
		if (options == null) throw new ArgumentNullException(nameof(options));
		options.Validate();
		return options.MaxAverageColorErrorFraction is { } average
			? AverageColorErrorFraction <= average
			: ErrorPixelCount <= AllowedErrorPixels(options);
	}

	public string Failure(GoldenComparisonOptions options)
	{
		if (options == null) throw new ArgumentNullException(nameof(options));
		options.Validate();
		return options.MaxAverageColorErrorFraction is { } limit
			? $"average RGBA color error {AverageColorErrorFraction:P3} exceeds {limit:P3}."
			: $"{ErrorPixelCount}/{TotalPixels} pixels differ, exceeding " +
				$"{options.MaxErrorPixelFraction:P3}; max channel delta {MaxChannelDelta}.";
	}
}
