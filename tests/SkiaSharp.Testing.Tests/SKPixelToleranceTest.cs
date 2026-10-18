using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Testing.Tests;

public class SKPixelToleranceTest : SKImageComparisonTestBase
{
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
	public void MixedThresholdsFilterChannelsButPreserveRawStatistics()
	{
		using var first = CreateBitmap(SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 6, 9));
		var strict = SKImageComparison.Compare(first, second);
		var options = new SKComparisonOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(3),
			GreenTolerance = SKPixelTolerance.Absolute(5),
			BlueTolerance = SKPixelTolerance.Absolute(9),
			AlphaTolerance = null,
		};
		var filtered = SKImageComparison.Compare(first, second, options);
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
		var tolerated = SKImageComparison.Compare(first, second, options);
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
		Assert.Equal(0, SKImageComparison.Compare(first, first, options).MismatchedPixelCount);
		Assert.Equal(0, SKImageComparison.Compare(small, larger, options).MismatchedPixelCount);
		using var beyond = CreateBitmap(new SKColor(3, 0, 0));
		Assert.Equal(1, SKImageComparison.Compare(small, beyond, options).MismatchedPixelCount);
		SetUniformTolerance(options, SKPixelTolerance.Exact);
		Assert.Equal(1, SKImageComparison.Compare(small, larger, options).MismatchedPixelCount);
	}

	[Fact]
	public void MixedAbsoluteAndPercentThresholdsUseEffectiveByteValues()
	{
		using var first = CreateBitmap(new SKColor(10, 20, 30));
		using var second = CreateBitmap(new SKColor(12, 22, 255));
		var options = new SKComparisonOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(2),
			GreenTolerance = SKPixelTolerance.Percent(0.5),
			BlueTolerance = SKPixelTolerance.Percent(100),
			AlphaTolerance = null,
		};
		var result = SKImageComparison.Compare(first, second, options);
		Assert.Equal(3, result.ChannelCount);
		Assert.Equal(3L, result.RawMetrics.SampleCount);
		Assert.Equal(229L, result.RawMetrics.SumAbsoluteError);
		Assert.Equal(50633L, result.RawMetrics.SumSquaredError);
		Assert.Equal(2L, result.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(4L, result.ThresholdedMetrics.SumSquaredError);
		Assert.Equal(1, result.MismatchedPixelCount);
		Assert.Equal(225, result.MaxChannelDifference);
		using var binary = SKImageComparison.GenerateDifferenceImage(first, second, SKDifferenceStyle.BinaryMask, options);
		AssertPixel(binary, 0, SKColors.White);
		Assert.Null(options.AlphaTolerance);
		options.GreenTolerance = SKPixelTolerance.Absolute(2);
		Assert.Equal(0, SKImageComparison.Compare(first, second, options).MismatchedPixelCount);
	}

	[Fact]
	public void MaskWidensEnabledChannelsWithoutChangingDenominatorOrRawMetrics()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 6, 9), new SKColor(3, 6, 9));
		using var maskBitmap = CreateBitmap(new SKColor(3, 5, 9), new SKColor(0, 0, 0));
		using var mask = SKImage.FromBitmap(maskBitmap);
		var options = new SKComparisonOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(5),
			AlphaTolerance = null,
			ToleranceMask = mask,
		};
		var withMask = SKImageComparison.Compare(first, second, options);
		Assert.Equal(2, withMask.MismatchedPixelCount);
		Assert.Equal(3, withMask.ChannelCount);
		Assert.Equal(6L, withMask.RawMetrics.SampleCount);
		Assert.Equal(36L, withMask.RawMetrics.SumAbsoluteError);
		Assert.Equal(252L, withMask.RawMetrics.SumSquaredError);
		Assert.Equal(21L, withMask.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(153L, withMask.ThresholdedMetrics.SumSquaredError);
		options.ToleranceMask = null;
		var withoutMask = SKImageComparison.Compare(first, second, options);
		Assert.Equal(withMask.RawMetrics.SumSquaredError, withoutMask.RawMetrics.SumSquaredError);
		Assert.Equal(30L, withoutMask.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(234L, withoutMask.ThresholdedMetrics.SumSquaredError);
		options.ToleranceMask = mask;
		options.RedTolerance = null;
		options.GreenTolerance = null;
		options.BlueTolerance = null;
		options.AlphaTolerance = SKPixelTolerance.Exact;
		var alphaOnly = SKImageComparison.Compare(first, second, options);
		Assert.Equal(1, alphaOnly.ChannelCount);
		Assert.Equal(0L, alphaOnly.RawMetrics.SumAbsoluteError);
		Assert.Equal(0, alphaOnly.MismatchedPixelCount);
		Assert.Equal(0, alphaOnly.MaxChannelDifference);
		using var alphaOnlyDifference = SKImageComparison.GenerateDifferenceImage(first, second, SKDifferenceStyle.BinaryMask, options);
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
		var options = new SKComparisonOptions
		{
			AlphaType = SKAlphaType.Premul,
			RedTolerance = null,
			GreenTolerance = null,
			BlueTolerance = null,
			ToleranceMask = threshold,
		};
		var tolerated = SKImageComparison.Compare(first, second, options);
		Assert.Equal(1, tolerated.ChannelCount);
		Assert.Equal(64L, tolerated.RawMetrics.SumAbsoluteError);
		Assert.Equal(0, tolerated.MismatchedPixelCount);
		using var smallerBitmap = CreateRawPremulBitmap(new SKColor(0, 0, 0, 63));
		using var smaller = SKImage.FromBitmap(smallerBitmap);
		options.ToleranceMask = smaller;
		Assert.Equal(1, SKImageComparison.Compare(first, second, options).MismatchedPixelCount);
	}
}
