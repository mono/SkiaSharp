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

	/// <summary>Whether the configured rejected-pixel budget and/or raw RMS limit pass, or (without either) no pixel is rejected.</summary>
	public bool IsMatch { get; }

	/// <summary>Gets all compared image pixels, including exact matches and tolerated differences; both metric groups use this count.</summary>
	public int TotalPixels { get; }

	/// <summary>Gets enabled channels per pixel, from one to four; both metric groups use this count.</summary>
	public int ChannelCount { get; }

	/// <summary>Number of pixels whose difference exceeds at least one enabled channel allowance.</summary>
	public int ErrorPixelCount { get; }

	/// <summary>Gets rejected pixels divided by all compared pixels as a fraction from zero to one; returns zero when the total is zero.</summary>
	public double ErrorPixelFraction => TotalPixels == 0 ? 0.0 : (double)ErrorPixelCount / TotalPixels;

	/// <summary>Largest raw delta on any enabled channel, including tolerated differences.</summary>
	public int MaxChannelDelta { get; }

	/// <summary>Bounding rectangle of rejected pixels, or null when none are rejected.</summary>
	public SKRectI? ErrorBounds { get; }

	/// <summary>Gets enabled-channel error sums before tolerance or mask filtering, divided by all compared pixels times enabled channels.</summary>
	public SKPixelComparisonMetrics RawMetrics { get; }

	/// <summary>Gets full deltas only for enabled channels exceeding their effective allowances; tolerated deltas are zero and the denominator matches RawMetrics.</summary>
	public SKPixelComparisonMetrics ThresholdedMetrics { get; }
}
