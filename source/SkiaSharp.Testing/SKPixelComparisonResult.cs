// Copyright (c) Microsoft Corporation. Licensed under the MIT license.

namespace SkiaSharp.Testing;

/// <summary>Immutable comparison outcome and raw and tolerance-filtered selected-channel statistics.</summary>
/// <remarks>Contains no references to the compared inputs, options, or borrowed masks.</remarks>
public sealed class SKPixelComparisonResult
{
	internal SKPixelComparisonResult(int totalPixels, int comparedPixelCount, int channelCount, int mismatchedPixelCount, int maxChannelDifference,
		SKRectI? mismatchBounds, SKPixelComparisonMetrics rawMetrics, SKPixelComparisonMetrics thresholdedMetrics, bool isMatch)
	{
		TotalPixels = totalPixels;
		ComparedPixelCount = comparedPixelCount;
		ChannelCount = channelCount;
		MismatchedPixelCount = mismatchedPixelCount;
		MaxChannelDifference = maxChannelDifference;
		MismatchBounds = mismatchBounds;
		RawMetrics = rawMetrics;
		ThresholdedMetrics = thresholdedMetrics;
		IsMatch = isMatch;
	}

	/// <summary>Whether all configured mismatched-pixel, raw RMS, and raw MAE gates pass, or (without any gate) no compared pixel is mismatched.</summary>
	public bool IsMatch { get; }

	/// <summary>Gets the full image area in pixels, including ignored coverage.</summary>
	public int TotalPixels { get; }

	/// <summary>Gets nonignored pixels, including exact matches and tolerated differences; both metric groups and fraction budgets use this count.</summary>
	public int ComparedPixelCount { get; }

	/// <summary>Gets pixels excluded by IgnoreMask, equal to TotalPixels minus ComparedPixelCount.</summary>
	public int IgnoredPixelCount => TotalPixels - ComparedPixelCount;

	/// <summary>Gets enabled channels per pixel, from one to four; both metric groups use this count.</summary>
	public int ChannelCount { get; }

	/// <summary>Number of nonignored pixels whose difference exceeds at least one enabled channel allowance.</summary>
	public int MismatchedPixelCount { get; }

	/// <summary>Gets mismatched pixels divided by nonignored compared pixels as a fraction from zero to one; returns zero when the compared count is zero.</summary>
	public double MismatchedPixelFraction => ComparedPixelCount == 0 ? 0.0 : (double)MismatchedPixelCount / ComparedPixelCount;

	/// <summary>Largest raw absolute difference on any enabled channel of nonignored pixels, including tolerated differences.</summary>
	public int MaxChannelDifference { get; }

	/// <summary>Bounding rectangle of nonignored mismatched pixels in image coordinates, or null when none are mismatched.</summary>
	public SKRectI? MismatchBounds { get; }

	/// <summary>Gets nonignored enabled-channel error statistics before tolerance or tolerance-mask filtering.</summary>
	public SKPixelComparisonMetrics RawMetrics { get; }

	/// <summary>Gets nonignored enabled-channel error statistics exceeding their allowances; tolerated differences are zero and the denominator matches RawMetrics.</summary>
	public SKPixelComparisonMetrics ThresholdedMetrics { get; }
}
