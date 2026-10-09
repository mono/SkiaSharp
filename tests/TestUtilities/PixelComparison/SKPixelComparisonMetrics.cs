// Copyright (c) Microsoft Corporation. Licensed under the MIT license.

using System;

namespace SkiaSharp.Testing;

/// <summary>Selected-channel error statistics, with a common pixel and channel denominator for raw and filtered metrics.</summary>
public sealed class SKPixelComparisonMetrics
{
	internal SKPixelComparisonMetrics(int totalPixels, int channelCount, long absoluteError, long sumSquaredError)
	{
		TotalPixels = totalPixels;
		ChannelCount = channelCount;
		AbsoluteError = absoluteError;
		SumSquaredError = sumSquaredError;
	}

	/// <summary>Number of considered pixels.</summary>
	public int TotalPixels { get; }

	/// <summary>Number of enabled channels per pixel.</summary>
	public int ChannelCount { get; }

	/// <summary>Number of samples: considered pixels times enabled channels (zero for an internal empty fixture).</summary>
	public long SampleCount => (long)TotalPixels * ChannelCount;

	/// <summary>Sum of absolute enabled-channel byte differences (filtered channels contribute zero in thresholded metrics).</summary>
	public long AbsoluteError { get; }

	/// <summary>Sum of squared enabled-channel byte differences (filtered channels contribute zero in thresholded metrics).</summary>
	public long SumSquaredError { get; }

	/// <summary>Absolute error divided by the number of samples, or zero for an internal empty fixture.</summary>
	public double MeanAbsoluteError => SampleCount == 0 ? 0.0 : (double)AbsoluteError / SampleCount;

	/// <summary>Squared error divided by the number of samples, or zero for an internal empty fixture.</summary>
	public double MeanSquaredError => SampleCount == 0 ? 0.0 : (double)SumSquaredError / SampleCount;

	/// <summary>Square root of the mean squared error.</summary>
	public double RootMeanSquaredError => Math.Sqrt(MeanSquaredError);

	/// <summary>Root mean squared error divided by 255.</summary>
	public double NormalizedRootMeanSquaredError => RootMeanSquaredError / 255.0;

	/// <summary>Peak signal-to-noise ratio in decibels, or positive infinity when the squared error is zero.</summary>
	public double PeakSignalToNoiseRatio =>
		MeanSquaredError == 0 ? double.PositiveInfinity : 10.0 * Math.Log10(255.0 * 255.0 / MeanSquaredError);
}
