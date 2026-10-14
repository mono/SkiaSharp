using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Tests;

public class SKPixelComparerTest
{
	[Fact]
	public void MetricGroupsExposeOnlyTheirSharedSampleDenominator()
	{
		Assert.Null(typeof(SKPixelComparisonMetrics).GetProperty("TotalPixels"));
		Assert.Null(typeof(SKPixelComparisonMetrics).GetProperty("ChannelCount"));
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
		Assert.Null(typeof(SKPixelComparisonMetrics).GetProperty("AbsoluteError"));
		Assert.Null(typeof(SKPixelComparisonResult).GetProperty("ErrorPixelCount"));
		Assert.Null(typeof(SKPixelComparisonResult).GetProperty("ErrorPixelFraction"));
		Assert.Null(typeof(SKPixelComparisonResult).GetProperty("ErrorBounds"));
		Assert.Null(typeof(SKPixelComparisonResult).GetProperty("MaxChannelDelta"));
		Assert.Null(typeof(SKPixelComparerOptions).GetProperty("ErrorBudget"));
		Assert.DoesNotContain("ChannelDelta", Enum.GetNames(typeof(SKPixelDifferenceStyle)));
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
	public void PublicEntryPointsUseOptionalNullSettingsWithoutConvenienceOverloads()
	{
		var methods = typeof(SKPixelComparer).GetMethods(
			System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
		Assert.Equal(6, methods.Length);
		foreach (var method in methods)
		{
			var parameters = method.GetParameters();
			var options = parameters[parameters.Length - 1];
			Assert.Equal("options", options.Name);
			Assert.True(options.IsOptional);
			Assert.Null(options.DefaultValue);
			if (method.Name == nameof(SKPixelComparer.GenerateDifferenceImage))
			{
				Assert.Equal(4, parameters.Length);
				Assert.Equal("style", parameters[2].Name);
				Assert.True(parameters[2].IsOptional);
				Assert.Equal(SKPixelDifferenceStyle.ThresholdOverlay, parameters[2].DefaultValue);
			}
			else
			{
				Assert.Equal(nameof(SKPixelComparer.Compare), method.Name);
				Assert.Equal(3, parameters.Length);
			}
		}
	}

	[Fact]
	public void DefaultIsStrictRgbaAndReportsRawMetricsAndBounds()
	{
		using var first = CreateBitmap(new SKColor(10, 20, 30), SKColors.Black);
		using var second = CreateBitmap(new SKColor(13, 26, 39), SKColors.Black);
		var result = SKPixelComparer.Compare(first, second);
		Assert.False(result.IsMatch);
		Assert.Equal(2, result.TotalPixels);
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
		Assert.Null(identical.MismatchBounds);
		Assert.Equal(0.0, identical.MismatchedPixelFraction);
		Assert.Equal(0L, identical.RawMetrics.SumAbsoluteError);
		Assert.Equal(0.0, identical.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(0.0, identical.RawMetrics.MeanSquaredError);
		Assert.Equal(double.PositiveInfinity, identical.RawMetrics.PeakSignalToNoiseRatio);
	}

	[Fact]
	public void ScalarToleranceFactoriesValidateBoundsFractionalThresholdsAndEquality()
	{
		Assert.Equal(SKPixelTolerance.Exact, default(SKPixelTolerance));
		Assert.Equal(SKPixelTolerance.Absolute(0), default(SKPixelTolerance));
		Assert.Equal(SKPixelTolerance.Percent(0), default(SKPixelTolerance));
		Assert.Equal(0.0, default(SKPixelTolerance).Value);
		Assert.True(SKPixelTolerance.Exact == SKPixelTolerance.Absolute(0));
		Assert.False(SKPixelTolerance.Exact != SKPixelTolerance.Percent(0));
		Assert.Equal(SKPixelTolerance.Exact.GetHashCode(), SKPixelTolerance.Absolute(0).GetHashCode());
		Assert.Equal(SKPixelTolerance.Absolute(255), SKPixelTolerance.Percent(100));
		Assert.Equal(SKPixelTolerance.Absolute(255).GetHashCode(), SKPixelTolerance.Percent(100).GetHashCode());
		Assert.Equal(255.0, SKPixelTolerance.Absolute(255).Value);
		Assert.Equal(255.0, SKPixelTolerance.Percent(100).Value);
		Assert.Equal(127.5, SKPixelTolerance.Percent(50).Value);
		Assert.Equal(1.275, SKPixelTolerance.Percent(0.5).Value, 12);
		var tolerance = SKPixelTolerance.Absolute(3);
		Assert.Equal(3.0, tolerance.Value);
		Assert.Equal(tolerance, SKPixelTolerance.Absolute(3));
		Assert.True(tolerance.Equals((object)SKPixelTolerance.Absolute(3)));
		Assert.Equal(tolerance.GetHashCode(), SKPixelTolerance.Absolute(3).GetHashCode());
		Assert.True(tolerance != SKPixelTolerance.Exact);
		Assert.False(tolerance.Equals("not a tolerance"));
		Assert.True(SKPixelTolerance.Exact.Allows(0));
		Assert.False(SKPixelTolerance.Exact.Allows(0.001));
		Assert.True(SKPixelTolerance.Absolute(3).Allows(3));
		Assert.False(SKPixelTolerance.Absolute(3).Allows(3.001));
		Assert.True(SKPixelTolerance.Percent(50).Allows(127.5));
		Assert.False(SKPixelTolerance.Percent(50).Allows(127.5001));
		Assert.True(SKPixelTolerance.Percent(100).Allows(255));
		Assert.False(SKPixelTolerance.Percent(100).Allows(256));
		foreach (var invalid in new[] { -0.001, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
			Assert.Equal("absoluteDifference", Assert.Throws<ArgumentOutOfRangeException>(() => tolerance.Allows(invalid)).ParamName);
		Assert.Equal("value", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelTolerance.Absolute(-1)).ParamName);
		Assert.Equal("value", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelTolerance.Absolute(256)).ParamName);
		foreach (var invalid in new[] { -0.001, 100.001, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
			Assert.Equal("value", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelTolerance.Percent(invalid)).ParamName);
	}

	[Fact]
	public void MismatchBudgetFactoriesValidateAndExposeImmutableLimits()
	{
		Assert.Empty(typeof(SKPixelMismatchBudget).GetConstructors());
		var exact = default(SKPixelMismatchBudget);
		Assert.Equal(SKPixelMismatchBudget.Exact, exact);
		Assert.Equal(SKPixelMismatchBudget.Pixels(0), exact);
		Assert.Equal(0L, exact.MaxPixels);
		Assert.Null(exact.MaxPercent);
		var count = SKPixelMismatchBudget.Pixels(long.MaxValue);
		Assert.Equal(long.MaxValue, count.MaxPixels);
		Assert.Null(count.MaxPercent);
		var percent = SKPixelMismatchBudget.Percent(0.2);
		Assert.Null(percent.MaxPixels);
		Assert.Equal(0.2, percent.MaxPercent);
		var both = SKPixelMismatchBudget.Both(131, 0.2);
		Assert.Equal(131L, both.MaxPixels);
		Assert.Equal(0.2, both.MaxPercent);
		Assert.True(both == SKPixelMismatchBudget.Both(131, 0.2));
		Assert.False(both != SKPixelMismatchBudget.Both(131, 0.2));
		Assert.True(both.Equals((object)SKPixelMismatchBudget.Both(131, 0.2)));
		Assert.Equal(both.GetHashCode(), SKPixelMismatchBudget.Both(131, 0.2).GetHashCode());
		Assert.Equal(exact.GetHashCode(), SKPixelMismatchBudget.Pixels(0).GetHashCode());
		Assert.NotEqual(SKPixelMismatchBudget.Percent(0), SKPixelMismatchBudget.Pixels(0));
		Assert.NotEqual(SKPixelMismatchBudget.Both(0, 100), SKPixelMismatchBudget.Pixels(0));
		Assert.NotEqual(SKPixelMismatchBudget.Pixels(1), SKPixelMismatchBudget.Pixels(2));
		Assert.NotEqual(SKPixelMismatchBudget.Percent(2), SKPixelMismatchBudget.Percent(3));
		Assert.False(both.Equals("not a budget"));
		Assert.Equal("maximum", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelMismatchBudget.Pixels(-1)).ParamName);
		Assert.Equal("pixels", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelMismatchBudget.Both(-1, 1)).ParamName);
		foreach (var invalid in new[] { -0.001, 100.001, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
		{
			Assert.Equal("percent", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelMismatchBudget.Both(0, invalid)).ParamName);
			Assert.Equal("maximum", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelMismatchBudget.Percent(invalid)).ParamName);
		}
	}

	[Fact]
	public void MismatchBudgetAllowsInclusiveCountPercentAndBothFloors()
	{
		var exact = SKPixelMismatchBudget.Exact;
		Assert.True(exact.Allows(0, 3));
		Assert.False(exact.Allows(1, 3));
		Assert.True(SKPixelMismatchBudget.Pixels(2).Allows(2, 3));
		Assert.False(SKPixelMismatchBudget.Pixels(1).Allows(2, 3));
		Assert.True(SKPixelMismatchBudget.Percent(0).Allows(0, 3));
		Assert.False(SKPixelMismatchBudget.Percent(0).Allows(1, 3));
		Assert.True(SKPixelMismatchBudget.Percent(100).Allows(3, 3));
		Assert.True(SKPixelMismatchBudget.Percent(200.0 / 3.0).Allows(2, 3));
		Assert.False(SKPixelMismatchBudget.Percent(66).Allows(2, 3));
		Assert.True(SKPixelMismatchBudget.Both(2, 200.0 / 3.0).Allows(2, 3));
		Assert.False(SKPixelMismatchBudget.Both(1, 100).Allows(2, 3));
		Assert.False(SKPixelMismatchBudget.Both(3, 50).Allows(2, 3));
		Assert.True(SKPixelMismatchBudget.Percent(0.3).Allows(3, 1000));
		Assert.False(SKPixelMismatchBudget.Percent(0.3).Allows(4, 1000));
		Assert.True(SKPixelMismatchBudget.Percent(0.3).Allows(30, 10_000));
		Assert.False(SKPixelMismatchBudget.Percent(0.3).Allows(31, 10_000));
		Assert.True(SKPixelMismatchBudget.Both(20, 0.3).Allows(3, 1000));
		Assert.False(SKPixelMismatchBudget.Both(20, 0.3).Allows(4, 1000));
		Assert.True(SKPixelMismatchBudget.Both(20, 0.3).Allows(20, 10_000));
		Assert.False(SKPixelMismatchBudget.Both(20, 0.3).Allows(21, 10_000));
		Assert.True(SKPixelMismatchBudget.Percent(0.002 * 100).Allows(1, 500));
		Assert.False(SKPixelMismatchBudget.Percent(0.002 * 100).Allows(2, 500));
		Assert.True(SKPixelMismatchBudget.Percent(0.02 * 100).Allows(1, 50));
		Assert.False(SKPixelMismatchBudget.Percent(0.02 * 100).Allows(2, 50));
		Assert.True(SKPixelMismatchBudget.Percent(0.002 * 100).Allows(131, 65_536));
		Assert.False(SKPixelMismatchBudget.Percent(0.002 * 100).Allows(132, 65_536));
		Assert.True(SKPixelMismatchBudget.Percent(0.02 * 100).Allows(1310, 65_536));
		Assert.False(SKPixelMismatchBudget.Percent(0.02 * 100).Allows(1311, 65_536));
		Assert.Equal("comparedPixels", Assert.Throws<ArgumentOutOfRangeException>(() => exact.Allows(0, 0)).ParamName);
		Assert.Equal("comparedPixels", Assert.Throws<ArgumentOutOfRangeException>(() => exact.Allows(0, -1)).ParamName);
		Assert.Equal("mismatchedPixels", Assert.Throws<ArgumentOutOfRangeException>(() => exact.Allows(-1, 1)).ParamName);
		Assert.Equal("mismatchedPixels", Assert.Throws<ArgumentOutOfRangeException>(() => exact.Allows(2, 1)).ParamName);
	}

	[Fact]
	public void MismatchBudgetAtLongLimitsDoesNotRoundUpOrOverflow()
	{
		const long total = long.MaxValue;
		const long half = total / 2;
		Assert.True(SKPixelMismatchBudget.Pixels(total).Allows(total, total));
		Assert.True(SKPixelMismatchBudget.Percent(100).Allows(total, total));
		Assert.False(SKPixelMismatchBudget.Exact.Allows(total, total));
		Assert.True(SKPixelMismatchBudget.Percent(50).Allows(half, total));
		Assert.False(SKPixelMismatchBudget.Percent(50).Allows(half + 1, total));
		Assert.False(SKPixelMismatchBudget.Percent(99.99999999999999).Allows(total, total));
		Assert.False(SKPixelMismatchBudget.Percent(99.99999999999999).Allows(total - 1, total));
		Assert.False(SKPixelMismatchBudget.Both(total, 99.99999999999999).Allows(total, total));
		Assert.True(SKPixelMismatchBudget.Percent(double.Epsilon).Allows(0, total));
		Assert.False(SKPixelMismatchBudget.Percent(double.Epsilon).Allows(1, total));
	}

	[Fact]
	public void MixedThresholdsFilterChannelsButPreserveRawStatistics()
	{
		using var first = CreateBitmap(SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 6, 9));
		var strict = SKPixelComparer.Compare(first, second);
		var options = new SKPixelComparerOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(3),
			GreenTolerance = SKPixelTolerance.Absolute(5),
			BlueTolerance = SKPixelTolerance.Absolute(9),
			AlphaTolerance = null,
		};
		var filtered = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(3, filtered.ChannelCount);
		Assert.Equal(1, filtered.MismatchedPixelCount);
		Assert.False(filtered.IsMatch);
		Assert.Equal(18L, filtered.RawMetrics.SumAbsoluteError);
		Assert.Equal(126L, filtered.RawMetrics.SumSquaredError);
		Assert.Equal(6L, filtered.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(36L, filtered.ThresholdedMetrics.SumSquaredError);
		Assert.Equal(3L, filtered.ThresholdedMetrics.SampleCount);
		Assert.Equal(2.0, filtered.ThresholdedMetrics.MeanAbsoluteError);
		Assert.Equal(6.0 / (1 * 3 * 255.0), filtered.ThresholdedMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(filtered.RawMetrics.SampleCount, filtered.ThresholdedMetrics.SampleCount);
		Assert.Equal(18.0 / (1 * 3 * 255.0), filtered.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(12.0, filtered.ThresholdedMetrics.MeanSquaredError);
		Assert.Equal(9, filtered.MaxChannelDifference);
		options.GreenTolerance = SKPixelTolerance.Absolute(6);
		options.BlueTolerance = SKPixelTolerance.Absolute(9);
		var tolerated = SKPixelComparer.Compare(first, second, options);
		Assert.True(tolerated.IsMatch);
		Assert.Equal(0, tolerated.MismatchedPixelCount);
		Assert.Null(tolerated.MismatchBounds);
		Assert.Equal(0L, tolerated.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(0.0, tolerated.ThresholdedMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(filtered.RawMetrics.SumAbsoluteError, tolerated.RawMetrics.SumAbsoluteError);
		Assert.Equal(filtered.RawMetrics.NormalizedMeanAbsoluteError, tolerated.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(filtered.RawMetrics.SumSquaredError, tolerated.RawMetrics.SumSquaredError);
		Assert.Equal(9, tolerated.MaxChannelDifference);
		Assert.Equal(126L, strict.RawMetrics.SumSquaredError);

		using var small = CreateBitmap(new SKColor(1, 0, 0));
		using var larger = CreateBitmap(new SKColor(2, 0, 0));
		SetUniformTolerance(options, SKPixelTolerance.Percent(0.5));
		Assert.Equal(0, SKPixelComparer.Compare(first, first, options).MismatchedPixelCount);
		Assert.Equal(0, SKPixelComparer.Compare(small, larger, options).MismatchedPixelCount);
		using var beyond = CreateBitmap(new SKColor(3, 0, 0));
		Assert.Equal(1, SKPixelComparer.Compare(small, beyond, options).MismatchedPixelCount);
		SetUniformTolerance(options, SKPixelTolerance.Exact);
		Assert.Equal(1, SKPixelComparer.Compare(small, larger, options).MismatchedPixelCount);
	}

	[Fact]
	public void MixedAbsoluteAndPercentThresholdsUseEffectiveByteValues()
	{
		using var first = CreateBitmap(new SKColor(10, 20, 30));
		using var second = CreateBitmap(new SKColor(12, 22, 255));
		var options = new SKPixelComparerOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(2),
			GreenTolerance = SKPixelTolerance.Percent(0.5),
			BlueTolerance = SKPixelTolerance.Percent(100),
			AlphaTolerance = null,
		};
		var result = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(3, result.ChannelCount);
		Assert.Equal(3L, result.RawMetrics.SampleCount);
		Assert.Equal(229L, result.RawMetrics.SumAbsoluteError);
		Assert.Equal(50633L, result.RawMetrics.SumSquaredError);
		Assert.Equal(2L, result.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(4L, result.ThresholdedMetrics.SumSquaredError);
		Assert.Equal(1, result.MismatchedPixelCount);
		Assert.Equal(225, result.MaxChannelDifference);
		using var binary = SKPixelComparer.GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.BinaryMask, options);
		AssertPixel(binary, 0, SKColors.White);
		Assert.Null(options.AlphaTolerance);
		options.GreenTolerance = SKPixelTolerance.Absolute(2);
		Assert.Equal(0, SKPixelComparer.Compare(first, second, options).MismatchedPixelCount);
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
	public void MatchBudgetsAreInclusiveAndOnlyConfiguredBudgetsApply()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 0, 0), new SKColor(6, 0, 0), SKColors.Black);
		var options = new SKPixelComparerOptions();
		var strict = SKPixelComparer.Compare(first, second);
		Assert.False(strict.IsMatch);
		Assert.Equal(2, strict.MismatchedPixelCount);
		var rms = strict.RawMetrics.NormalizedRootMeanSquaredError;
		options.MaxNormalizedRootMeanSquaredError = rms;
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxNormalizedRootMeanSquaredError = rms - 0.000001;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxNormalizedRootMeanSquaredError = null;
		options.MismatchBudget = SKPixelMismatchBudget.Pixels(2);
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Exact;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Pixels(1);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Percent(200.0 / 3.0);
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Percent(66);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Both(3, 50);
		options.MaxNormalizedRootMeanSquaredError = rms;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Both(3, 100);
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Both(1, 100);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = null;
		SetUniformTolerance(options, SKPixelTolerance.Absolute(6));
		options.MaxNormalizedRootMeanSquaredError = rms - 0.000001;
		var tolerated = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(0, tolerated.MismatchedPixelCount);
		Assert.False(tolerated.IsMatch);
		Assert.Equal(rms, tolerated.RawMetrics.NormalizedRootMeanSquaredError);
		options.MaxNormalizedRootMeanSquaredError = null;
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		SetUniformTolerance(options, SKPixelTolerance.Exact);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Exact;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Pixels(2);
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxNormalizedRootMeanSquaredError = rms - 0.000001;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
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
	public void MaskWidensEnabledChannelsWithoutChangingDenominatorOrRawMetrics()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 6, 9), new SKColor(3, 6, 9));
		using var maskBitmap = CreateBitmap(new SKColor(3, 5, 9), new SKColor(0, 0, 0));
		using var mask = SKImage.FromBitmap(maskBitmap);
		var options = new SKPixelComparerOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(5),
			AlphaTolerance = null,
			ToleranceMask = mask,
		};
		var withMask = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(2, withMask.MismatchedPixelCount);
		Assert.Equal(3, withMask.ChannelCount);
		Assert.Equal(6L, withMask.RawMetrics.SampleCount);
		Assert.Equal(36L, withMask.RawMetrics.SumAbsoluteError);
		Assert.Equal(252L, withMask.RawMetrics.SumSquaredError);
		Assert.Equal(21L, withMask.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(153L, withMask.ThresholdedMetrics.SumSquaredError);
		options.ToleranceMask = null;
		var withoutMask = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(withMask.RawMetrics.SumSquaredError, withoutMask.RawMetrics.SumSquaredError);
		Assert.Equal(30L, withoutMask.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(234L, withoutMask.ThresholdedMetrics.SumSquaredError);
		options.ToleranceMask = mask;
		options.RedTolerance = null;
		options.GreenTolerance = null;
		options.BlueTolerance = null;
		options.AlphaTolerance = SKPixelTolerance.Exact;
		var alphaOnly = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(1, alphaOnly.ChannelCount);
		Assert.Equal(0L, alphaOnly.RawMetrics.SumAbsoluteError);
		Assert.Equal(0, alphaOnly.MismatchedPixelCount);
		Assert.Equal(0, alphaOnly.MaxChannelDifference);
		using var alphaOnlyDifference = SKPixelComparer.GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.BinaryMask, options);
		AssertPixel(alphaOnlyDifference, 0, SKColors.Black);
		Assert.Null(options.RedTolerance);
		Assert.Null(options.GreenTolerance);
		Assert.Null(options.BlueTolerance);
		Assert.Equal(new SKColor(3, 5, 9), maskBitmap.GetPixel(0, 0));
	}

	[Fact]
	public void MaskAlphaByteIsAThresholdNotPixelCoverage()
	{
		using var first = CreateRawPremulBitmap(new SKColor(64, 0, 0, 64));
		using var second = CreateRawPremulBitmap(new SKColor(64, 0, 0, 128));
		using var thresholdBitmap = CreateRawPremulBitmap(new SKColor(0, 0, 0, 64));
		using var threshold = SKImage.FromBitmap(thresholdBitmap);
		var options = new SKPixelComparerOptions
		{
			AlphaType = SKAlphaType.Premul,
			RedTolerance = null,
			GreenTolerance = null,
			BlueTolerance = null,
			ToleranceMask = threshold,
		};
		var tolerated = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(1, tolerated.ChannelCount);
		Assert.Equal(64L, tolerated.RawMetrics.SumAbsoluteError);
		Assert.Equal(0, tolerated.MismatchedPixelCount);
		using var smallerBitmap = CreateRawPremulBitmap(new SKColor(0, 0, 0, 63));
		using var smaller = SKImage.FromBitmap(smallerBitmap);
		options.ToleranceMask = smaller;
		Assert.Equal(1, SKPixelComparer.Compare(first, second, options).MismatchedPixelCount);
	}

	[Fact]
	public void DifferenceImagesAgreeWithMismatchesAndStayOpaqueIndependentOfInput()
	{
		using var first = CreateBitmap(new SKColor(10, 20, 30), new SKColor(40, 40, 40), new SKColor(80, 100, 120));
		using var second = CreateBitmap(new SKColor(13, 26, 39), new SKColor(40, 40, 40), new SKColor(90, 100, 120));
		using var maskBitmap = CreateBitmap(SKColors.Black, SKColors.Black, new SKColor(10, 0, 0));
		using var mask = SKImage.FromBitmap(maskBitmap);
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		var options = new SKPixelComparerOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(3),
			GreenTolerance = SKPixelTolerance.Absolute(5),
			BlueTolerance = SKPixelTolerance.Absolute(9),
			AlphaTolerance = null,
			ToleranceMask = mask,
		};
		Assert.Equal(1, SKPixelComparer.Compare(a, b, options).MismatchedPixelCount);
		options.MismatchBudget = SKPixelMismatchBudget.Pixels(1);
		Assert.True(SKPixelComparer.Compare(a, b, options).IsMatch);
		using var binary = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.BinaryMask, options);
		using var styledBinary = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.BinaryMask, options);
		using var overlay = SKPixelComparer.GenerateDifferenceImage(a, b, options: options);
		using var explicitOverlay = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ThresholdOverlay, options);
		using var raw = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ChannelDifference, options);
		using var strictMask = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.BinaryMask, null);
		using var bitmapOverlay = SKPixelComparer.GenerateDifferenceImage(first, second);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		using var pixmapBinary = SKPixelComparer.GenerateDifferenceImage(firstPixmap, secondPixmap, SKPixelDifferenceStyle.BinaryMask, options);
		AssertPixel(binary, 0, SKColors.White);
		AssertPixel(binary, 1, SKColors.Black);
		AssertPixel(binary, 2, SKColors.Black);
		AssertPixel(styledBinary, 2, SKColors.Black);
		AssertPixel(pixmapBinary, 0, SKColors.White);
		AssertPixel(overlay, 0, SKColors.Red);
		AssertPixel(overlay, 1, new SKColor(10, 10, 10));
		AssertPixel(overlay, 2, new SKColor(255, 200, 0));
		AssertPixel(explicitOverlay, 2, new SKColor(255, 200, 0));
		AssertPixel(bitmapOverlay, 0, SKColors.Red);
		AssertPixel(raw, 0, new SKColor(3, 6, 9));
		AssertPixel(raw, 1, SKColors.Black);
		AssertPixel(raw, 2, new SKColor(10, 0, 0));
		AssertPixel(strictMask, 2, SKColors.White);
		using var png = overlay.Encode(SKEncodedImageFormat.Png, 100);
		using var decoded = SKBitmap.Decode(png);
		Assert.Equal(SKColors.Red, decoded.GetPixel(0, 0));
		Assert.Equal(new SKColor(255, 200, 0), decoded.GetPixel(2, 0));
		Assert.Equal(new SKColor(10, 20, 30), first.GetPixel(0, 0));
		Assert.Equal(new SKColor(13, 26, 39), second.GetPixel(0, 0));
		mask.Dispose();
		first.Dispose();
		second.Dispose();
		a.Dispose();
		b.Dispose();
		AssertPixel(binary, 0, SKColors.White);
		AssertPixel(overlay, 2, new SKColor(255, 200, 0));
	}

	[Fact]
	public void AlphaOnlyDifferenceIsVisibleByDefaultInAllDecodedFamilies()
	{
		using var opaqueBlack = CreateBitmap(SKColors.Black);
		using var transparentBlack = CreateBitmap(SKColors.Transparent);
		Assert.Equal(1, SKPixelComparer.Compare(opaqueBlack, transparentBlack).MismatchedPixelCount);
		using var defaultOverlay = SKPixelComparer.GenerateDifferenceImage(opaqueBlack, transparentBlack);
		AssertPixel(defaultOverlay, 0, SKColors.Red);

		using var first = CreateRawPremulBitmap(new SKColor(64, 0, 0, 64));
		using var second = CreateRawPremulBitmap(new SKColor(64, 0, 0, 128));
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		var options = new SKPixelComparerOptions { AlphaType = SKAlphaType.Premul };
		Assert.Equal(1, SKPixelComparer.Compare(first, second, options).MismatchedPixelCount);
		Assert.Equal(1, SKPixelComparer.Compare(firstPixmap, secondPixmap, options).MismatchedPixelCount);
		Assert.Equal(1, SKPixelComparer.Compare(a, b, options).MismatchedPixelCount);
		using var overlay = SKPixelComparer.GenerateDifferenceImage(a, b, options: options);
		using var binary = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.BinaryMask, options);
		using var delta = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ChannelDifference, options);
		AssertPixel(overlay, 0, SKColors.Red);
		AssertPixel(binary, 0, SKColors.White);
		AssertPixel(delta, 0, SKColors.Black);
		options.RedTolerance = null;
		options.GreenTolerance = null;
		options.BlueTolerance = null;
		options.AlphaTolerance = SKPixelTolerance.Absolute(64);
		using var tolerated = SKPixelComparer.GenerateDifferenceImage(first, second, options: options);
		AssertPixel(tolerated, 0, new SKColor(255, 200, 0));
		Assert.Equal(0, SKPixelComparer.Compare(a, b, options).MismatchedPixelCount);
	}

	[Fact]
	public void FullyTransparentHiddenRgbKeepsDrawNormalizedComparisonBehavior()
	{
		using var first = CreateBitmap(new SKColor(255, 0, 0, 0));
		using var second = CreateBitmap(SKColors.Transparent);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		foreach (var alphaType in new[] { SKAlphaType.Unpremul, SKAlphaType.Premul })
		{
			var options = new SKPixelComparerOptions { AlphaType = alphaType };
			foreach (var result in new[]
			{
				SKPixelComparer.Compare(first, second, options),
				SKPixelComparer.Compare(firstPixmap, secondPixmap, options),
				SKPixelComparer.Compare(a, b, options),
			})
			{
				Assert.True(result.IsMatch);
				Assert.Equal(0L, result.RawMetrics.SumAbsoluteError);
				Assert.Equal(0, result.MaxChannelDifference);
			}
			using var diff = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.BinaryMask, options);
			AssertPixel(diff, 0, SKColors.Black);
		}
		Assert.Equal(new SKColor(255, 0, 0, 0), firstPixmap.GetPixelSpan<SKColor>()[0]);
	}

	[Fact]
	public void BitmapPixmapImageAndPaddedRowsGiveTheSameAnswer()
	{
		var info = new SKImageInfo(2, 2, SKColorType.Bgra8888, SKAlphaType.Unpremul);
		using var first = new SKBitmap(info, 16);
		using var second = new SKBitmap(info, 16);
		using var maskBitmap = new SKBitmap(info, 16);
		first.Erase(SKColors.Black);
		second.Erase(SKColors.Black);
		maskBitmap.Erase(SKColors.Black);
		second.SetPixel(1, 1, new SKColor(3, 6, 9));
		maskBitmap.SetPixel(1, 1, new SKColor(3, 5, 9));
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		using var mask = SKImage.FromBitmap(maskBitmap);
		var options = new SKPixelComparerOptions { ToleranceMask = mask };
		Assert.Equal(16, firstPixmap.RowBytes);
		var image = SKPixelComparer.Compare(a, b, options);
		var bitmap = SKPixelComparer.Compare(first, second, options);
		var pixmap = SKPixelComparer.Compare(firstPixmap, secondPixmap, options);
		Assert.Equal(6L, image.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(image.RawMetrics.SumAbsoluteError, bitmap.RawMetrics.SumAbsoluteError);
		Assert.Equal(image.RawMetrics.SumAbsoluteError, pixmap.RawMetrics.SumAbsoluteError);
		Assert.Equal(image.ThresholdedMetrics.SumSquaredError, bitmap.ThresholdedMetrics.SumSquaredError);
		Assert.Equal(image.ThresholdedMetrics.SumSquaredError, pixmap.ThresholdedMetrics.SumSquaredError);
		Assert.Equal(image.MismatchedPixelCount, pixmap.MismatchedPixelCount);
		Assert.Equal(new SKRectI(1, 1, 2, 2), image.MismatchBounds);
		Assert.Equal(18L, SKPixelComparer.Compare(first, second).RawMetrics.SumAbsoluteError);
		Assert.Equal(18L, SKPixelComparer.Compare(firstPixmap, secondPixmap).RawMetrics.SumAbsoluteError);
		using var difference = SKPixelComparer.GenerateDifferenceImage(firstPixmap, secondPixmap, options: options);
		using var pixels = SKBitmap.FromImage(difference);
		Assert.Equal(SKColors.Red, pixels.GetPixel(1, 1));
	}

	[Fact]
	public void InvalidInputsOptionsMasksAndStylesFailExplicitly()
	{
		using var bitmap = CreateBitmap(SKColors.Black);
		using var otherSize = CreateBitmap(SKColors.Black, SKColors.Black);
		using var image = SKImage.FromBitmap(bitmap);
		using var mismatch = SKImage.FromBitmap(otherSize);
		using var pixmap = bitmap.PeekPixels();
		using var emptyBitmap = new SKBitmap();
		using var emptyPixmap = new SKPixmap();
		var allDisabled = new SKPixelComparerOptions
		{
			RedTolerance = null,
			GreenTolerance = null,
			BlueTolerance = null,
			AlphaTolerance = null,
		};
		Assert.Equal("options", Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(bitmap, bitmap, allDisabled)).ParamName);
		Assert.Equal("options", Assert.Throws<ArgumentException>(() =>
			SKPixelComparer.GenerateDifferenceImage(image, image, options: allDisabled)).ParamName);
		Assert.Equal(4, SKPixelComparer.Compare(bitmap, bitmap, null).ChannelCount);
		Assert.Equal(4, SKPixelComparer.Compare(bitmap, bitmap, new SKPixelComparerOptions()).ChannelCount);
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare((SKImage)null!, image));
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare(image, (SKImage)null!));
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare((SKBitmap)null!, bitmap));
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare((SKPixmap)null!, pixmap));
		Assert.Throws<InvalidOperationException>(() => SKPixelComparer.Compare(image, mismatch));
		Assert.Throws<InvalidOperationException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { ToleranceMask = mismatch }));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(bitmap, emptyBitmap));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(pixmap, emptyPixmap));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.GenerateDifferenceImage(image, image, (SKPixelDifferenceStyle)99, null));
		foreach (var invalid in new[] { SKAlphaType.Opaque, SKAlphaType.Unknown })
			Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { AlphaType = invalid }));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.GenerateDifferenceImage(image, image, SKPixelDifferenceStyle.BinaryMask, new SKPixelComparerOptions { AlphaType = SKAlphaType.Opaque }));
		foreach (var invalid in new[] { -0.01, 1.01, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { MaxNormalizedRootMeanSquaredError = invalid }));
		}
		using var disposedMask = SKImage.FromBitmap(bitmap);
		disposedMask.Dispose();
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { ToleranceMask = disposedMask }));
		image.Dispose();
		bitmap.Dispose();
		pixmap.Dispose();
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(image, image));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(bitmap, bitmap));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(pixmap, pixmap));
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

	private static void SetUniformTolerance(SKPixelComparerOptions options, SKPixelTolerance tolerance)
	{
		options.RedTolerance = tolerance;
		options.GreenTolerance = tolerance;
		options.BlueTolerance = tolerance;
		options.AlphaTolerance = tolerance;
	}

	private static SKBitmap CreateBitmap(params SKColor[] colors)
	{
		var bitmap = new SKBitmap(new SKImageInfo(colors.Length, 1, SKColorType.Bgra8888, SKAlphaType.Unpremul));
		using var pixmap = bitmap.PeekPixels();
		var pixels = pixmap.GetPixelSpan<SKColor>();
		for (var i = 0; i < colors.Length; i++)
			pixels[i] = colors[i];
		return bitmap;
	}

	private static SKBitmap CreateRawPremulBitmap(SKColor storedPixel)
	{
		var bitmap = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Bgra8888, SKAlphaType.Premul));
		using var pixmap = bitmap.PeekPixels();
		pixmap.GetPixelSpan<SKColor>()[0] = storedPixel;
		return bitmap;
	}

	private static void AssertPixel(SKImage image, int x, SKColor expected)
	{
		using var bitmap = SKBitmap.FromImage(image);
		Assert.Equal(expected, bitmap.GetPixel(x, 0));
		Assert.Equal((byte)255, bitmap.GetPixel(x, 0).Alpha);
	}
}
