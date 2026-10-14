using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Testing.Tests;

public class SKPixelComparisonMetricsTest : SKPixelComparerTestBase
{
	[Fact]
	public void MetricGroupsUseTheSameSampleDenominator()
	{
		var raw = new SKPixelComparisonMetrics(100, 4, 20, 40);
		var thresholded = new SKPixelComparisonMetrics(100, 4, 10, 20);
		Assert.Equal(400L, raw.SampleCount);
		Assert.Equal(raw.SampleCount, thresholded.SampleCount);
		Assert.Equal(0.05, raw.MeanAbsoluteError);
		Assert.Equal(0.025, thresholded.MeanAbsoluteError);
		Assert.Equal(20.0 / (400 * 255.0), raw.NormalizedMeanAbsoluteError);
		Assert.Equal(10.0 / (400 * 255.0), thresholded.NormalizedMeanAbsoluteError);
		Assert.Equal(0.1, raw.MeanSquaredError);
		Assert.Equal(0.05, thresholded.MeanSquaredError);
	}

	[Fact]
	public void NormalizedMeanAbsoluteErrorUsesDirectSampleDenominatorForOneThroughFourChannels()
	{
		const int pixels = 137;
		const long sumAbsoluteError = 3217;
		for (var channels = 1; channels <= 4; channels++)
		{
			var metrics = new SKPixelComparisonMetrics(pixels, channels, sumAbsoluteError, 0);
			Assert.Equal(sumAbsoluteError / (pixels * channels * 255.0), metrics.NormalizedMeanAbsoluteError);
		}

		const long nearThresholdSum = 131;
		var nearThreshold = new SKPixelComparisonMetrics(65_536, 4, nearThresholdSum, 0);
		Assert.Equal(nearThresholdSum / (65_536 * 4 * 255.0), nearThreshold.NormalizedMeanAbsoluteError);
		var maxContrast = new SKPixelComparisonMetrics(1, 4, 4 * 255, 4 * 255 * 255);
		Assert.Equal(1.0, maxContrast.NormalizedMeanAbsoluteError);
	}

	[Fact]
	public void DefaultIsStrictRgbaAndReportsRawMetricsAndBounds()
	{
		using var first = CreateBitmap(new SKColor(10, 20, 30), SKColors.Black);
		using var second = CreateBitmap(new SKColor(13, 26, 39), SKColors.Black);
		var result = SKPixelComparer.Compare(first, second);
		Assert.False(result.IsMatch);
		Assert.Equal(2, result.TotalPixels);
		Assert.Equal(2, result.ComparedPixelCount);
		Assert.Equal(0, result.IgnoredPixelCount);
		Assert.Equal(4, result.ChannelCount);
		Assert.Equal(1, result.MismatchedPixelCount);
		Assert.Equal(0.5, result.MismatchedPixelFraction);
		Assert.Equal(new SKRectI(0, 0, 1, 1), result.MismatchBounds);
		Assert.Equal(9, result.MaxChannelDifference);
		Assert.Equal(8L, result.RawMetrics.SampleCount);
		Assert.Equal(18L, result.RawMetrics.SumAbsoluteError);
		Assert.Equal(126L, result.RawMetrics.SumSquaredError);
		Assert.Equal(2.25, result.RawMetrics.MeanAbsoluteError);
		Assert.Equal(18.0 / (2 * 4 * 255.0), result.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(15.75, result.RawMetrics.MeanSquaredError);
		Assert.Equal(Math.Sqrt(15.75), result.RawMetrics.RootMeanSquaredError, 12);
		Assert.Equal(Math.Sqrt(15.75) / 255, result.RawMetrics.NormalizedRootMeanSquaredError, 12);
		Assert.Equal(10 * Math.Log10(65025.0 / 15.75), result.RawMetrics.PeakSignalToNoiseRatio, 12);
		Assert.Equal(result.RawMetrics.SumSquaredError, result.ThresholdedMetrics.SumSquaredError);

		var identical = SKPixelComparer.Compare(first, first);
		Assert.True(identical.IsMatch);
		Assert.Equal(identical.TotalPixels, identical.ComparedPixelCount);
		Assert.Equal(0, identical.IgnoredPixelCount);
		Assert.Null(identical.MismatchBounds);
		Assert.Equal(0.0, identical.MismatchedPixelFraction);
		Assert.Equal(0L, identical.RawMetrics.SumAbsoluteError);
		Assert.Equal(0.0, identical.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(0.0, identical.RawMetrics.MeanSquaredError);
		Assert.Equal(double.PositiveInfinity, identical.RawMetrics.PeakSignalToNoiseRatio);
	}

	[Fact]
	public void DisabledChannelsChangeMetricsDenominatorsAndNeverMismatch()
	{
		using var first = CreateRawPremulBitmap(new SKColor(64, 0, 0, 64));
		using var second = CreateRawPremulBitmap(new SKColor(64, 0, 0, 128));
		var options = new SKPixelComparerOptions { AlphaType = SKAlphaType.Premul };
		var rgba = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(1, rgba.MismatchedPixelCount);
		Assert.Equal(64L, rgba.RawMetrics.SumAbsoluteError);
		Assert.Equal(4096L, rgba.RawMetrics.SumSquaredError);
		Assert.Equal(4L, rgba.RawMetrics.SampleCount);
		Assert.Equal(64.0 / (1 * 4 * 255.0), rgba.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(64, rgba.MaxChannelDifference);
		options.AlphaTolerance = null;
		var rgb = SKPixelComparer.Compare(first, second, options);
		Assert.True(rgb.IsMatch);
		Assert.Equal(3, rgb.ChannelCount);
		Assert.Equal(0L, rgb.RawMetrics.SumAbsoluteError);
		Assert.Equal(3L, rgb.RawMetrics.SampleCount);
		Assert.Equal(0.0, rgb.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(0.0, rgb.RawMetrics.NormalizedRootMeanSquaredError);
		Assert.Equal(0, rgb.MaxChannelDifference);
		options.RedTolerance = null;
		options.GreenTolerance = null;
		options.BlueTolerance = null;
		options.AlphaTolerance = SKPixelTolerance.Exact;
		var alpha = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(1, alpha.ChannelCount);
		Assert.Equal(1L, alpha.RawMetrics.SampleCount);
		Assert.Equal(64.0, alpha.RawMetrics.MeanAbsoluteError);
		Assert.Equal(64.0 / (1 * 1 * 255.0), alpha.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(4096.0, alpha.RawMetrics.MeanSquaredError);
		Assert.Equal(64.0 / 255, alpha.RawMetrics.NormalizedRootMeanSquaredError, 12);
		options.AlphaTolerance = SKPixelTolerance.Absolute(64);
		Assert.Equal(0, SKPixelComparer.Compare(first, second, options).MismatchedPixelCount);

		using var redFirst = CreateBitmap(new SKColor(10, 20, 30));
		using var redSecond = CreateBitmap(new SKColor(13, 26, 39));
		options.AlphaType = SKAlphaType.Unpremul;
		options.RedTolerance = SKPixelTolerance.Exact;
		options.AlphaTolerance = null;
		var red = SKPixelComparer.Compare(redFirst, redSecond, options);
		Assert.Equal(1, red.ChannelCount);
		Assert.Equal(1L, red.RawMetrics.SampleCount);
		Assert.Equal(3L, red.RawMetrics.SumAbsoluteError);
		Assert.Equal(9L, red.RawMetrics.SumSquaredError);
		Assert.Equal(3.0 / (1 * 1 * 255.0), red.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(3.0 / 255, red.RawMetrics.NormalizedRootMeanSquaredError, 12);
		Assert.Equal(3, red.MaxChannelDifference);
	}

	[Fact]
	public void PremultipliedRgbUsesOnlySelectedChannelsForNormalizedMeanAbsoluteError()
	{
		using var first = CreateRawPremulBitmap(new SKColor(32, 0, 0, 128));
		using var second = CreateRawPremulBitmap(new SKColor(64, 0, 0, 128));
		var options = new SKPixelComparerOptions
		{
			AlphaType = SKAlphaType.Premul,
			AlphaTolerance = null,
		};
		var rgb = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(3L, rgb.RawMetrics.SampleCount);
		Assert.Equal(32L, rgb.RawMetrics.SumAbsoluteError);
		Assert.Equal(32.0 / (1 * 3 * 255.0), rgb.RawMetrics.NormalizedMeanAbsoluteError);
		options.GreenTolerance = null;
		options.BlueTolerance = null;
		var red = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(1L, red.RawMetrics.SampleCount);
		Assert.Equal(32.0 / (1 * 1 * 255.0), red.RawMetrics.NormalizedMeanAbsoluteError);
	}

	[Fact]
	public void MismatchedPixelsHaveAnExclusiveBoundingRectangle()
	{
		var info = new SKImageInfo(4, 3, SKColorType.Bgra8888, SKAlphaType.Unpremul);
		using var first = new SKBitmap(info);
		using var second = new SKBitmap(info);
		first.Erase(SKColors.Black);
		second.Erase(SKColors.Black);
		second.SetPixel(1, 0, SKColors.White);
		second.SetPixel(3, 2, SKColors.White);
		var result = SKPixelComparer.Compare(first, second);
		Assert.Equal(2, result.MismatchedPixelCount);
		Assert.Equal(new SKRectI(1, 0, 4, 3), result.MismatchBounds);
	}

	[Fact]
	public void FourKMaximumContrastDoesNotOverflowWideRgbSums()
	{
		var info = new SKImageInfo(3840, 2160, SKColorType.Bgra8888, SKAlphaType.Opaque);
		using var first = new SKBitmap(info);
		using var second = new SKBitmap(info);
		first.Erase(SKColors.Black);
		second.Erase(SKColors.White);
		var result = SKPixelComparer.Compare(first, second, new SKPixelComparerOptions
		{
			AlphaTolerance = null,
		});
		Assert.Equal(8_294_400, result.TotalPixels);
		Assert.Equal(result.TotalPixels, result.ComparedPixelCount);
		Assert.Equal(0, result.IgnoredPixelCount);
		Assert.Equal(8_294_400, result.MismatchedPixelCount);
		Assert.Equal(3, result.ChannelCount);
		Assert.Equal(24_883_200L, result.RawMetrics.SampleCount);
		Assert.Equal(6_345_216_000L, result.RawMetrics.SumAbsoluteError);
		Assert.Equal(1_618_030_080_000L, result.RawMetrics.SumSquaredError);
		Assert.Equal(255, result.MaxChannelDifference);
		Assert.Equal(255.0, result.RawMetrics.MeanAbsoluteError);
		Assert.Equal(1.0, result.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(65025.0, result.RawMetrics.MeanSquaredError);
		Assert.Equal(255.0, result.RawMetrics.RootMeanSquaredError);
		Assert.Equal(1.0, result.RawMetrics.NormalizedRootMeanSquaredError);
		Assert.Equal(0.0, result.RawMetrics.PeakSignalToNoiseRatio);
	}

	[Fact]
	public void InternalEmptyMetricsHaveDefinedZeroDenominators()
	{
		var metrics = new SKPixelComparisonMetrics(0, 3, 0, 0);
		Assert.Equal(0L, metrics.SampleCount);
		Assert.Equal(0.0, metrics.MeanAbsoluteError);
		Assert.Equal(0.0, metrics.NormalizedMeanAbsoluteError);
		Assert.Equal(0.0, metrics.MeanSquaredError);
		Assert.Equal(0.0, metrics.RootMeanSquaredError);
		Assert.Equal(0.0, metrics.NormalizedRootMeanSquaredError);
		Assert.Equal(double.PositiveInfinity, metrics.PeakSignalToNoiseRatio);
	}
}
