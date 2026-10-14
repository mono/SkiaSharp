// Copyright (c) Microsoft Corporation. Licensed under the MIT license.

using System;

namespace SkiaSharp.Testing;

/// <summary>Raw or thresholded enabled-channel error statistics over all compared pixels.</summary>
/// <remarks>Both metric groups use the same sample denominator. Thresholding changes only the error sums: tolerated channel differences contribute zero, while differences exceeding their allowances contribute their full magnitudes.</remarks>
public sealed class SKPixelComparisonMetrics
{
	internal SKPixelComparisonMetrics(int totalPixels, int channelCount, long sumAbsoluteError, long sumSquaredError)
	{
		SampleCount = (long)totalPixels * channelCount;
		SumAbsoluteError = sumAbsoluteError;
		SumSquaredError = sumSquaredError;
	}

	/// <summary>Gets total compared pixels times enabled channels; tolerated samples remain in this denominator.</summary>
	public long SampleCount { get; }

	/// <summary>Gets the sum of enabled-channel absolute byte differences; thresholded metrics replace tolerated differences with zero.</summary>
	public long SumAbsoluteError { get; }

	/// <summary>Gets the sum of squared enabled-channel byte differences; thresholded metrics replace tolerated differences with zero.</summary>
	public long SumSquaredError { get; }

	/// <summary>Gets absolute error divided by all selected samples, in byte units; returns zero when the sample count is zero.</summary>
	public double MeanAbsoluteError => SampleCount == 0 ? 0.0 : (double)SumAbsoluteError / SampleCount;

	/// <summary>Gets mean absolute error divided by 255, from zero to one; returns zero when the sample count is zero.</summary>
	public double NormalizedMeanAbsoluteError => SampleCount == 0 ? 0.0 : (double)SumAbsoluteError / (SampleCount * 255.0);

	/// <summary>Gets squared error divided by all selected samples, in squared byte units; returns zero when the sample count is zero.</summary>
	public double MeanSquaredError => SampleCount == 0 ? 0.0 : (double)SumSquaredError / SampleCount;

	/// <summary>Square root of the mean squared error.</summary>
	public double RootMeanSquaredError => Math.Sqrt(MeanSquaredError);

	/// <summary>Root mean squared error divided by 255.</summary>
	public double NormalizedRootMeanSquaredError => RootMeanSquaredError / 255.0;

	/// <summary>Peak signal-to-noise ratio in decibels, or positive infinity when the squared error is zero.</summary>
	public double PeakSignalToNoiseRatio =>
		MeanSquaredError == 0 ? double.PositiveInfinity : 10.0 * Math.Log10(255.0 * 255.0 / MeanSquaredError);
}
