// Based on SkiaSharp.Extended's SKPixelComparisonResult at
// https://github.com/mono/SkiaSharp.Extended/tree/579c974196199962dc1cb7c22bc68fe32e9a5b64
// Copyright (c) 2015-2016 Xamarin, Inc.
// Copyright (c) 2017-2020 Microsoft Corporation. Licensed under the MIT license.
using System;

namespace SkiaSharp.Testing;

/// <summary>Statistics of selected, threshold-filtered RGB or RGBA channel differences.</summary>
internal sealed class SKPixelComparisonResult
{
	internal SKPixelComparisonResult(int totalPixels, int errorPixelCount, long absoluteError, long sumSquaredError, int channelCount, int maxChannelDelta)
	{
		TotalPixels = totalPixels;
		ErrorPixelCount = errorPixelCount;
		AbsoluteError = absoluteError;
		SumSquaredError = sumSquaredError;
		ChannelCount = channelCount;
		MaxChannelDelta = maxChannelDelta;
	}

	/// <summary>Number of pixels compared.</summary>
	public int TotalPixels { get; }

	/// <summary>Number of pixels with at least one rejected channel or a rejected channel sum.</summary>
	public int ErrorPixelCount { get; }

	/// <summary>Ratio of rejected pixels, from zero to one.</summary>
	public double ErrorPixelPercentage => TotalPixels == 0 ? 0.0 : (double)ErrorPixelCount / TotalPixels;

	/// <summary>Sum of selected differences after tolerance filtering.</summary>
	public long AbsoluteError { get; }

	/// <summary>Sum of squares of selected differences after tolerance filtering.</summary>
	public long SumSquaredError { get; }

	/// <summary>Three for RGB, four for RGBA.</summary>
	public int ChannelCount { get; }

	/// <summary>Maximum raw selected-channel difference, including tolerated differences.</summary>
	public int MaxChannelDelta { get; }

	/// <summary>Threshold-filtered absolute error divided by total pixels times channel count.</summary>
	public double MeanAbsoluteError =>
		TotalPixels == 0 ? 0.0 : (double)AbsoluteError / (TotalPixels * (double)ChannelCount);

	/// <summary>Threshold-filtered squared error divided by total pixels times channel count.</summary>
	public double MeanSquaredError =>
		TotalPixels == 0 ? 0.0 : (double)SumSquaredError / (TotalPixels * (double)ChannelCount);

	/// <summary>Square root of mean squared error.</summary>
	public double RootMeanSquaredError => Math.Sqrt(MeanSquaredError);

	/// <summary>Root mean squared error divided by 255.</summary>
	public double NormalizedRootMeanSquaredError => RootMeanSquaredError / 255.0;

	/// <summary>Peak signal-to-noise ratio in dB, or positive infinity for zero squared error.</summary>
	public double PeakSignalToNoiseRatio =>
		MeanSquaredError == 0 ? double.PositiveInfinity : 10.0 * Math.Log10(255.0 * 255.0 / MeanSquaredError);
}
