using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Testing.Tests;

public class SKPixelIgnoreCoverageTest : SKPixelComparerTestBase
{
	[Fact]
	public void IgnoreMaskExcludesHighErrorPixelsFromSumsMaximaAndImageCoordinateBounds()
	{
		var info = new SKImageInfo(4, 2, SKColorType.Bgra8888, SKAlphaType.Unpremul);
		using var first = new SKBitmap(info);
		using var second = new SKBitmap(info);
		using var maskBitmap = new SKBitmap(info);
		first.Erase(SKColors.Black);
		second.Erase(SKColors.Black);
		maskBitmap.Erase(SKColors.Black);
		second.SetPixel(0, 0, SKColors.White);
		second.SetPixel(1, 0, new SKColor(3, 6, 9));
		second.SetPixel(3, 1, new SKColor(5, 0, 0));
		maskBitmap.SetPixel(0, 0, SKColors.White);
		using var mask = SKImage.FromBitmap(maskBitmap);
		var options = new SKPixelComparerOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(3),
			GreenTolerance = SKPixelTolerance.Absolute(5),
			BlueTolerance = SKPixelTolerance.Absolute(9),
			IgnoreMask = mask,
		};
		var result = SKPixelComparer.Compare(first, second, options);
		Assert.False(result.IsMatch);
		Assert.Equal(8, result.TotalPixels);
		Assert.Equal(7, result.ComparedPixelCount);
		Assert.Equal(1, result.IgnoredPixelCount);
		Assert.Equal(2, result.MismatchedPixelCount);
		Assert.Equal(2.0 / 7, result.MismatchedPixelFraction);
		Assert.Equal(new SKRectI(1, 0, 4, 2), result.MismatchBounds);
		Assert.Equal(9, result.MaxChannelDifference);
		Assert.Equal(28L, result.RawMetrics.SampleCount);
		Assert.Equal(28L, result.ThresholdedMetrics.SampleCount);
		Assert.Equal(23L, result.RawMetrics.SumAbsoluteError);
		Assert.Equal(151L, result.RawMetrics.SumSquaredError);
		Assert.Equal(23.0 / 28, result.RawMetrics.MeanAbsoluteError);
		Assert.Equal(23.0 / (28 * 255.0), result.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(151.0 / 28, result.RawMetrics.MeanSquaredError);
		Assert.Equal(Math.Sqrt(151.0 / 28) / 255, result.RawMetrics.NormalizedRootMeanSquaredError);
		Assert.Equal(11L, result.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(61L, result.ThresholdedMetrics.SumSquaredError);
		Assert.Equal(11.0 / (28 * 255.0), result.ThresholdedMetrics.NormalizedMeanAbsoluteError);
		Assert.Equal(61.0 / 28, result.ThresholdedMetrics.MeanSquaredError);
	}

	[Fact]
	public void IgnoreMaskFractionBudgetsAndMagnitudeGatesUseRemainingCoverage()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black, SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(SKColors.White, SKColors.White, new SKColor(3, 0, 0), SKColors.Black);
		using var maskBitmap = CreateBitmap(SKColors.White, SKColors.White, SKColors.Black, SKColors.Black);
		using var mask = SKImage.FromBitmap(maskBitmap);
		var options = new SKPixelComparerOptions { IgnoreMask = mask };
		var result = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(4, result.TotalPixels);
		Assert.Equal(2, result.ComparedPixelCount);
		Assert.Equal(2, result.IgnoredPixelCount);
		Assert.Equal(1, result.MismatchedPixelCount);
		Assert.Equal(0.5, result.MismatchedPixelFraction);
		Assert.False(result.IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Percent(50);
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Percent(49);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Both(1, 50);
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Both(1, 49);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = SKPixelMismatchBudget.Exact;
		options.MaxNormalizedMeanAbsoluteError = result.RawMetrics.NormalizedMeanAbsoluteError;
		options.MaxNormalizedRootMeanSquaredError = result.RawMetrics.NormalizedRootMeanSquaredError;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MismatchBudget = null;
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		Assert.Equal(3.0 / (2 * 4 * 255.0), options.MaxNormalizedMeanAbsoluteError);
		options.MaxNormalizedMeanAbsoluteError = JustBelow(result.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxNormalizedMeanAbsoluteError = result.RawMetrics.NormalizedMeanAbsoluteError;
		options.MaxNormalizedRootMeanSquaredError = JustBelow(result.RawMetrics.NormalizedRootMeanSquaredError);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
	}

	[Fact]
	public void IgnoredOnlyDifferencesLeaveAnExactMatchWithNonemptyCoverage()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(SKColors.White, SKColors.Black);
		using var maskBitmap = CreateBitmap(SKColors.White, SKColors.Black);
		using var mask = SKImage.FromBitmap(maskBitmap);
		var result = SKPixelComparer.Compare(first, second, new SKPixelComparerOptions { IgnoreMask = mask });
		Assert.True(result.IsMatch);
		Assert.Equal(2, result.TotalPixels);
		Assert.Equal(1, result.ComparedPixelCount);
		Assert.Equal(1, result.IgnoredPixelCount);
		Assert.Equal(0, result.MismatchedPixelCount);
		Assert.Equal(0.0, result.MismatchedPixelFraction);
		Assert.Equal(0, result.MaxChannelDifference);
		Assert.Null(result.MismatchBounds);
		foreach (var metrics in new[] { result.RawMetrics, result.ThresholdedMetrics })
		{
			Assert.Equal(4L, metrics.SampleCount);
			Assert.Equal(0L, metrics.SumAbsoluteError);
			Assert.Equal(0L, metrics.SumSquaredError);
			Assert.Equal(0.0, metrics.NormalizedMeanAbsoluteError);
			Assert.Equal(0.0, metrics.NormalizedRootMeanSquaredError);
			Assert.Equal(double.PositiveInfinity, metrics.PeakSignalToNoiseRatio);
		}
	}

	[Fact]
	public void IgnoreMaskUsesOneThroughFourSelectedChannelsForBothMetricDenominators()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(SKColors.White, new SKColor(3, 6, 9), SKColors.Black);
		using var maskBitmap = CreateBitmap(SKColors.White, SKColors.Black, SKColors.Black);
		using var mask = SKImage.FromBitmap(maskBitmap);
		var absolute = new[] { 3L, 9L, 18L, 18L };
		var squared = new[] { 9L, 45L, 126L, 126L };
		var maxima = new[] { 3, 6, 9, 9 };
		for (var channels = 1; channels <= 4; channels++)
		{
			var options = new SKPixelComparerOptions
			{
				IgnoreMask = mask,
				GreenTolerance = channels >= 2 ? SKPixelTolerance.Exact : null,
				BlueTolerance = channels >= 3 ? SKPixelTolerance.Exact : null,
				AlphaTolerance = channels >= 4 ? SKPixelTolerance.Exact : null,
			};
			var result = SKPixelComparer.Compare(first, second, options);
			Assert.Equal(3, result.TotalPixels);
			Assert.Equal(2, result.ComparedPixelCount);
			Assert.Equal(1, result.IgnoredPixelCount);
			Assert.Equal(channels, result.ChannelCount);
			Assert.Equal(1, result.MismatchedPixelCount);
			Assert.Equal(0.5, result.MismatchedPixelFraction);
			Assert.Equal(new SKRectI(1, 0, 2, 1), result.MismatchBounds);
			Assert.Equal(maxima[channels - 1], result.MaxChannelDifference);
			foreach (var metrics in new[] { result.RawMetrics, result.ThresholdedMetrics })
			{
				Assert.Equal(2L * channels, metrics.SampleCount);
				Assert.Equal(absolute[channels - 1], metrics.SumAbsoluteError);
				Assert.Equal(squared[channels - 1], metrics.SumSquaredError);
				Assert.Equal(absolute[channels - 1] / (2 * channels * 255.0), metrics.NormalizedMeanAbsoluteError);
				Assert.Equal(squared[channels - 1] / (2.0 * channels), metrics.MeanSquaredError);
			}
			options.MaxNormalizedMeanAbsoluteError = result.RawMetrics.NormalizedMeanAbsoluteError;
			Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		}
	}

	[Fact]
	public void NullAndBlackIgnoreMasksPreserveAllCountsMetricsAndDiagnosticPixels()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black, new SKColor(80, 100, 120));
		using var second = CreateBitmap(new SKColor(3, 0, 0), SKColors.Black, new SKColor(81, 100, 120));
		using var maskBitmap = CreateBitmap(new SKColor(0, 0, 0, 0), SKColors.Black, new SKColor(0, 0, 0, 64));
		using var mask = SKImage.FromBitmap(maskBitmap);
		var options = new SKPixelComparerOptions { RedTolerance = SKPixelTolerance.Absolute(1) };
		Assert.Null(options.IgnoreMask);
		Assert.Null(options.MaxNormalizedMeanAbsoluteError);
		var baseline = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(3, baseline.TotalPixels);
		Assert.Equal(3, baseline.ComparedPixelCount);
		Assert.Equal(0, baseline.IgnoredPixelCount);
		Assert.Equal(1, baseline.MismatchedPixelCount);
		Assert.Equal(1.0 / 3, baseline.MismatchedPixelFraction);
		options.IgnoreMask = mask;
		AssertEquivalent(baseline, SKPixelComparer.Compare(first, second, options));
		foreach (var style in new[] { SKPixelDifferenceStyle.BinaryMask, SKPixelDifferenceStyle.ChannelDifference, SKPixelDifferenceStyle.ThresholdMap, SKPixelDifferenceStyle.ThresholdOverlay })
		{
			using var masked = SKPixelComparer.GenerateDifferenceImage(first, second, style, options);
			options.IgnoreMask = null;
			using var unmasked = SKPixelComparer.GenerateDifferenceImage(first, second, style, options);
			using var expected = SKBitmap.FromImage(unmasked);
			for (var x = 0; x < first.Width; x++)
				AssertPixel(masked, x, expected.GetPixel(x, 0));
			options.IgnoreMask = mask;
		}
	}

	[Fact]
	public void IgnoreMaskReadsRgbAsUnpremultipliedDataWithoutUsingAlphaCoverage()
	{
		var colors = new[]
		{
			new SKColor(1, 0, 0, 0),
			new SKColor(0, 1, 0, 0),
			new SKColor(0, 0, 1, 1),
			SKColors.White,
			new SKColor(0, 0, 0, 0),
			SKColors.Black,
			new SKColor(0, 0, 0, 17),
		};
		using var maskBitmap = CreateBitmap(colors);
		using var maskPixmap = maskBitmap.PeekPixels();
		using var mask = SKImage.FromPixels(maskPixmap);
		using var first = CreateBitmap(SKColors.Black, SKColors.Black, SKColors.Black, SKColors.Black, SKColors.Black, SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(SKColors.White, SKColors.White, SKColors.White, SKColors.White, SKColors.White, SKColors.White, SKColors.White);
		foreach (var alphaType in new[] { SKAlphaType.Unpremul, SKAlphaType.Premul })
		{
			var options = new SKPixelComparerOptions { IgnoreMask = mask, AlphaType = alphaType };
			var result = SKPixelComparer.Compare(first, second, options);
			Assert.Equal(7, result.TotalPixels);
			Assert.Equal(3, result.ComparedPixelCount);
			Assert.Equal(4, result.IgnoredPixelCount);
			Assert.Equal(3, result.MismatchedPixelCount);
			Assert.Equal(1.0, result.MismatchedPixelFraction);
			Assert.Equal(new SKRectI(4, 0, 7, 1), result.MismatchBounds);
			Assert.Equal(12L, result.RawMetrics.SampleCount);
			Assert.Equal(3L * 3 * 255, result.RawMetrics.SumAbsoluteError);
			using var diff = SKPixelComparer.GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.ThresholdMap, options);
			for (var x = 0; x < first.Width; x++)
				AssertPixel(diff, x, x < 4 ? new SKColor(128, 128, 128) : SKColors.Red);
		}
		Assert.Equal(colors, maskPixmap.GetPixelSpan<SKColor>().ToArray());
	}

	[Fact]
	public void TransparentWhiteIgnoreMaskExcludesCoverageButTransparentBlackDoesNot()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(SKColors.White, SKColors.White);
		using var maskBitmap = CreateBitmap(SKColors.Transparent, new SKColor(0, 0, 0, 0));
		using var maskPixmap = maskBitmap.PeekPixels();
		using var mask = SKImage.FromPixels(maskPixmap);
		var options = new SKPixelComparerOptions { IgnoreMask = mask };
		var result = SKPixelComparer.Compare(first, second, options);
		Assert.False(result.IsMatch);
		Assert.Equal(2, result.TotalPixels);
		Assert.Equal(1, result.ComparedPixelCount);
		Assert.Equal(1, result.IgnoredPixelCount);
		Assert.Equal(1, result.MismatchedPixelCount);
		Assert.Equal(new SKRectI(1, 0, 2, 1), result.MismatchBounds);
		Assert.Equal(4L, result.RawMetrics.SampleCount);
		Assert.Equal(3L * 255, result.RawMetrics.SumAbsoluteError);
		using var overlay = SKPixelComparer.GenerateDifferenceImage(first, second, options: options);
		AssertPixel(overlay, 0, new SKColor(192, 192, 192));
		AssertPixel(overlay, 1, new SKColor(255, 128, 128));
	}

	[Fact]
	public void SameBorrowedImageCanServeAsToleranceAndIgnoreMaskWithoutChangingAlphaAllowance()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(SKColors.White, new SKColor(0, 0, 0, 191));
		using var maskBitmap = CreateBitmap(SKColors.White, new SKColor(0, 0, 0, 64));
		using var mask = SKImage.FromBitmap(maskBitmap);
		var options = new SKPixelComparerOptions
		{
			AlphaType = SKAlphaType.Premul,
			ToleranceMask = mask,
			IgnoreMask = mask,
		};
		var result = SKPixelComparer.Compare(first, second, options);
		Assert.True(result.IsMatch);
		Assert.Equal(2, result.TotalPixels);
		Assert.Equal(1, result.ComparedPixelCount);
		Assert.Equal(1, result.IgnoredPixelCount);
		Assert.Equal(0, result.MismatchedPixelCount);
		Assert.Null(result.MismatchBounds);
		Assert.Equal(64, result.MaxChannelDifference);
		Assert.Equal(4L, result.RawMetrics.SampleCount);
		Assert.Equal(64L, result.RawMetrics.SumAbsoluteError);
		Assert.Equal(4096L, result.RawMetrics.SumSquaredError);
		Assert.Equal(0L, result.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(0L, result.ThresholdedMetrics.SumSquaredError);
		options.MaxNormalizedMeanAbsoluteError = JustBelow(result.RawMetrics.NormalizedMeanAbsoluteError);
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		using var map = SKPixelComparer.GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.ThresholdMap, options);
		AssertPixel(map, 0, new SKColor(128, 128, 128));
		AssertPixel(map, 1, new SKColor(255, 200, 0));
		options.IgnoreMask = null;
		var toleranceOnly = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(2, toleranceOnly.ComparedPixelCount);
		Assert.Equal(0, toleranceOnly.IgnoredPixelCount);
		Assert.Equal(0, toleranceOnly.MismatchedPixelCount);
		Assert.Same(mask, options.ToleranceMask);
		Assert.NotEqual(IntPtr.Zero, mask.Handle);
		Assert.Equal(new SKColor(0, 0, 0, 64), maskBitmap.GetPixel(1, 0));
	}

	[Fact]
	public void DistinctToleranceAndIgnoreMasksApplyOnlyTheirOwnContracts()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(SKColors.White, new SKColor(3, 6, 9), new SKColor(3, 6, 9));
		using var toleranceBitmap = CreateBitmap(SKColors.Black, new SKColor(3, 6, 9), SKColors.Black);
		using var ignoreBitmap = CreateBitmap(SKColors.White, SKColors.Black, SKColors.Black);
		using var toleranceMask = SKImage.FromBitmap(toleranceBitmap);
		using var ignoreMask = SKImage.FromBitmap(ignoreBitmap);
		var options = new SKPixelComparerOptions { ToleranceMask = toleranceMask, IgnoreMask = ignoreMask };
		var result = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(2, result.ComparedPixelCount);
		Assert.Equal(1, result.IgnoredPixelCount);
		Assert.Equal(1, result.MismatchedPixelCount);
		Assert.Equal(new SKRectI(2, 0, 3, 1), result.MismatchBounds);
		Assert.Equal(36L, result.RawMetrics.SumAbsoluteError);
		Assert.Equal(252L, result.RawMetrics.SumSquaredError);
		Assert.Equal(18L, result.ThresholdedMetrics.SumAbsoluteError);
		Assert.Equal(126L, result.ThresholdedMetrics.SumSquaredError);
		Assert.Equal(8L, result.RawMetrics.SampleCount);
		Assert.Equal(result.RawMetrics.SampleCount, result.ThresholdedMetrics.SampleCount);
		Assert.Equal(9, result.MaxChannelDifference);
		foreach (var style in new[] { SKPixelDifferenceStyle.BinaryMask, SKPixelDifferenceStyle.ChannelDifference, SKPixelDifferenceStyle.ThresholdMap, SKPixelDifferenceStyle.ThresholdOverlay })
		{
			using var diff = SKPixelComparer.GenerateDifferenceImage(first, second, style, options);
			var expected = style switch
			{
				SKPixelDifferenceStyle.BinaryMask => new[] { SKColors.Black, SKColors.Black, SKColors.White },
				SKPixelDifferenceStyle.ChannelDifference => new[] { SKColors.Black, new SKColor(3, 6, 9), new SKColor(3, 6, 9) },
				SKPixelDifferenceStyle.ThresholdMap => new[] { new SKColor(128, 128, 128), new SKColor(255, 200, 0), SKColors.Red },
				_ => new[] { new SKColor(192, 192, 192), new SKColor(129, 103, 5), new SKColor(129, 3, 5) },
			};
			for (var x = 0; x < first.Width; x++)
				AssertPixel(diff, x, expected[x]);
		}
	}
}
