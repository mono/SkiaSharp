// Based on SkiaSharp.Extended's SKPixelComparisonResult at
// https://github.com/mono/SkiaSharp.Extended/tree/579c974196199962dc1cb7c22bc68fe32e9a5b64
// Copyright (c) 2015-2016 Xamarin, Inc.
// Copyright (c) 2017-2020 Microsoft Corporation. Licensed under the MIT license.

namespace SkiaSharp.Testing;

/// <summary>Immutable comparison outcome and raw and tolerance-filtered selected-channel statistics.</summary>
/// <remarks>Contains no references to the compared inputs, options, or borrowed mask.</remarks>
public sealed class SKPixelComparisonResult
{
	internal SKPixelComparisonResult(int totalPixels, int channelCount, int errorPixelCount, int maxChannelDelta,
		SKRectI? errorBounds, SKPixelComparisonMetrics rawMetrics, SKPixelComparisonMetrics thresholdedMetrics, bool isMatch)
	{
		TotalPixels = totalPixels;
		ChannelCount = channelCount;
		ErrorPixelCount = errorPixelCount;
		MaxChannelDelta = maxChannelDelta;
		ErrorBounds = errorBounds;
		RawMetrics = rawMetrics;
		ThresholdedMetrics = thresholdedMetrics;
		IsMatch = isMatch;
	}

	/// <summary>Whether all configured whole-image budgets pass, or (without budgets) no pixel is rejected.</summary>
	public bool IsMatch { get; }

	/// <summary>Number of pixels compared.</summary>
	public int TotalPixels { get; }

	/// <summary>Number of enabled channels per pixel (one to four).</summary>
	public int ChannelCount { get; }

	/// <summary>Number of pixels whose difference exceeds at least one enabled channel allowance.</summary>
	public int ErrorPixelCount { get; }

	/// <summary>Rejected pixel count divided by total pixels (zero for an internal empty fixture).</summary>
	public double ErrorPixelFraction => TotalPixels == 0 ? 0.0 : (double)ErrorPixelCount / TotalPixels;

	/// <summary>Largest raw delta on any enabled channel, including tolerated differences.</summary>
	public int MaxChannelDelta { get; }

	/// <summary>Bounding rectangle of rejected pixels, or null when none are rejected.</summary>
	public SKRectI? ErrorBounds { get; }

	/// <summary>Metrics across all enabled channel differences, before any tolerance or mask.</summary>
	public SKPixelComparisonMetrics RawMetrics { get; }

	/// <summary>Metrics across enabled channel differences exceeding their allowances; non-exceeding differences contribute zero.</summary>
	public SKPixelComparisonMetrics ThresholdedMetrics { get; }
}
