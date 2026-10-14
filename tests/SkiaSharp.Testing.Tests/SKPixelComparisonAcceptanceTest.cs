using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Testing.Tests;

public class SKPixelComparisonAcceptanceTest : SKPixelComparerTestBase
{
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
	public void MeanAbsoluteErrorOnlyGateIsInclusiveAtZeroOneAndMeasuredCutoff()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 0, 0), new SKColor(6, 0, 0), SKColors.Black);
		var options = new SKPixelComparerOptions { MaxNormalizedMeanAbsoluteError = 0 };
		Assert.Null(options.MismatchBudget);
		Assert.Null(options.MaxNormalizedRootMeanSquaredError);
		Assert.True(SKPixelComparer.Compare(first, first, options).IsMatch);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		var strict = SKPixelComparer.Compare(first, second);
		var mae = strict.RawMetrics.NormalizedMeanAbsoluteError;
		Assert.Equal(9.0 / (3 * 4 * 255.0), mae);
		options.MaxNormalizedMeanAbsoluteError = mae;
		var allowed = SKPixelComparer.Compare(first, second, options);
		Assert.True(allowed.IsMatch);
		Assert.Equal(2, allowed.MismatchedPixelCount);
		Assert.Equal(strict.MismatchBounds, allowed.MismatchBounds);
		options.MaxNormalizedMeanAbsoluteError = JustBelow(mae);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxNormalizedMeanAbsoluteError = 1;
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);

		using var transparent = CreateBitmap(SKColors.Transparent);
		using var white = CreateBitmap(SKColors.White);
		var maximum = SKPixelComparer.Compare(transparent, white, options);
		Assert.Equal(1.0, maximum.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.True(maximum.IsMatch);
		options.MaxNormalizedMeanAbsoluteError = JustBelow(1.0);
		Assert.False(SKPixelComparer.Compare(transparent, white, options).IsMatch);
		options.MaxNormalizedMeanAbsoluteError = null;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
	}

	[Fact]
	public void MeanAbsoluteErrorCombinesWithEveryExplicitBudgetAndRmsGateUsingAnd()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 0, 0), SKColors.Black);
		var strict = SKPixelComparer.Compare(first, second);
		var mae = strict.RawMetrics.NormalizedMeanAbsoluteError;
		var rms = strict.RawMetrics.NormalizedRootMeanSquaredError;
		var options = new SKPixelComparerOptions { MaxNormalizedMeanAbsoluteError = mae };
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Exact;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		foreach (var budget in new[]
		{
			SKPixelMismatchBudget.Pixels(1),
			SKPixelMismatchBudget.Percent(50),
			SKPixelMismatchBudget.Both(1, 50),
		})
		{
			options.MismatchBudget = budget;
			Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
			options.MaxNormalizedMeanAbsoluteError = JustBelow(mae);
			Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
			options.MaxNormalizedMeanAbsoluteError = mae;
		}
		options.MismatchBudget = SKPixelMismatchBudget.Percent(49);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = null;
		options.MaxNormalizedRootMeanSquaredError = rms;
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxNormalizedRootMeanSquaredError = JustBelow(rms);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxNormalizedRootMeanSquaredError = rms;
		options.MaxNormalizedMeanAbsoluteError = JustBelow(mae);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxNormalizedMeanAbsoluteError = mae;
		options.MismatchBudget = SKPixelMismatchBudget.Pixels(1);
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Exact;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		SetUniformTolerance(options, SKPixelTolerance.Absolute(3));
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
	}

	[Fact]
	public void MeanAbsoluteErrorUsesRawSelectedChannelsRatherThanToleranceFilteredErrors()
	{
		using var first = CreateBitmap(SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 6, 9));
		using var maskBitmap = CreateBitmap(new SKColor(3, 6, 9));
		using var mask = SKImage.FromBitmap(maskBitmap);
		var sums = new[] { 3L, 9L, 18L, 18L };
		for (var channels = 1; channels <= 4; channels++)
		{
			var options = new SKPixelComparerOptions
			{
				GreenTolerance = channels >= 2 ? SKPixelTolerance.Exact : null,
				BlueTolerance = channels >= 3 ? SKPixelTolerance.Exact : null,
				AlphaTolerance = channels >= 4 ? SKPixelTolerance.Exact : null,
			};
			var raw = SKPixelComparer.Compare(first, second, options);
			var mae = sums[channels - 1] / (channels * 255.0);
			Assert.Equal(channels, raw.ChannelCount);
			Assert.Equal(mae, raw.RawMetrics.NormalizedMeanAbsoluteError);
			options.ToleranceMask = mask;
			options.MaxNormalizedMeanAbsoluteError = JustBelow(mae);
			var masked = SKPixelComparer.Compare(first, second, options);
			Assert.Equal(0, masked.MismatchedPixelCount);
			Assert.Equal(0L, masked.ThresholdedMetrics.SumAbsoluteError);
			Assert.Equal(mae, masked.RawMetrics.NormalizedMeanAbsoluteError);
			Assert.False(masked.IsMatch);
			options.MaxNormalizedMeanAbsoluteError = mae;
			Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
			options.ToleranceMask = null;
			options.RedTolerance = SKPixelTolerance.Absolute(3);
			if (channels >= 2)
				options.GreenTolerance = SKPixelTolerance.Absolute(6);
			if (channels >= 3)
				options.BlueTolerance = SKPixelTolerance.Absolute(9);
			options.MaxNormalizedMeanAbsoluteError = JustBelow(mae);
			var tolerated = SKPixelComparer.Compare(first, second, options);
			Assert.Equal(0, tolerated.MismatchedPixelCount);
			Assert.Equal(mae, tolerated.RawMetrics.NormalizedMeanAbsoluteError);
			Assert.False(tolerated.IsMatch);
		}
	}
}
