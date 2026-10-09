using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Tests;

public class SKPixelComparerTest
{
	[Fact]
	public void DefaultIsStrictRgbaAndReportsRawMetricsAndBounds()
	{
		using var first = CreateBitmap(new SKColor(10, 20, 30), SKColors.Black);
		using var second = CreateBitmap(new SKColor(13, 26, 39), SKColors.Black);
		var result = SKPixelComparer.Compare(first, second);
		Assert.False(result.IsMatch);
		Assert.Equal(2, result.TotalPixels);
		Assert.Equal(4, result.ChannelCount);
		Assert.Equal(1, result.ErrorPixelCount);
		Assert.Equal(0.5, result.ErrorPixelFraction);
		Assert.Equal(new SKRectI(0, 0, 1, 1), result.ErrorBounds);
		Assert.Equal(9, result.MaxChannelDelta);
		Assert.Equal(8L, result.RawMetrics.SampleCount);
		Assert.Equal(18L, result.RawMetrics.AbsoluteError);
		Assert.Equal(126L, result.RawMetrics.SumSquaredError);
		Assert.Equal(2.25, result.RawMetrics.MeanAbsoluteError);
		Assert.Equal(15.75, result.RawMetrics.MeanSquaredError);
		Assert.Equal(Math.Sqrt(15.75), result.RawMetrics.RootMeanSquaredError, 12);
		Assert.Equal(Math.Sqrt(15.75) / 255, result.RawMetrics.NormalizedRootMeanSquaredError, 12);
		Assert.Equal(10 * Math.Log10(65025.0 / 15.75), result.RawMetrics.PeakSignalToNoiseRatio, 12);
		Assert.Equal(result.RawMetrics.SumSquaredError, result.ThresholdedMetrics.SumSquaredError);

		var identical = SKPixelComparer.Compare(first, first);
		Assert.True(identical.IsMatch);
		Assert.Null(identical.ErrorBounds);
		Assert.Equal(0.0, identical.ErrorPixelFraction);
		Assert.Equal(0L, identical.RawMetrics.AbsoluteError);
		Assert.Equal(0.0, identical.RawMetrics.MeanSquaredError);
		Assert.Equal(double.PositiveInfinity, identical.RawMetrics.PeakSignalToNoiseRatio);
	}

	[Fact]
	public void ToleranceFactoriesValidateBoundsSelectionFractionalThresholdsAndEquality()
	{
		Assert.Equal(SKPixelTolerance.Exact, default(SKPixelTolerance));
		Assert.Equal(SKPixelTolerance.Absolute(0), default(SKPixelTolerance));
		Assert.Equal(SKPixelTolerance.Percent(0), default(SKPixelTolerance));
		Assert.True(SKPixelTolerance.Exact == SKPixelTolerance.Absolute(0));
		Assert.False(SKPixelTolerance.Exact != SKPixelTolerance.Percent(0));
		Assert.Equal(SKPixelTolerance.Exact.GetHashCode(), SKPixelTolerance.Absolute(0).GetHashCode());
		Assert.Equal(SKPixelTolerance.Absolute(255), SKPixelTolerance.Percent(100));
		Assert.Equal(SKPixelTolerance.Absolute(255).GetHashCode(), SKPixelTolerance.Percent(100).GetHashCode());
		Assert.Equal(4, SKPixelTolerance.Exact.ChannelCount);
		Assert.Equal(255.0, SKPixelTolerance.Absolute(255).Alpha);
		Assert.Equal(255.0, SKPixelTolerance.Percent(100).Red);
		Assert.Equal(1.275, SKPixelTolerance.Percent(0.5).Red!.Value, 12);
		var rgb = SKPixelTolerance.Absolute(3, 6, 9, null);
		Assert.Equal(3, rgb.ChannelCount);
		Assert.Null(rgb.Alpha);
		Assert.Equal(3.0, rgb.Red);
		Assert.Equal(6.0, rgb.Green);
		Assert.Equal(9.0, rgb.Blue);
		Assert.Equal(rgb, SKPixelTolerance.Absolute(3, 6, 9, null));
		Assert.True(rgb.Equals((object)SKPixelTolerance.Absolute(3, 6, 9, null)));
		Assert.Equal(rgb.GetHashCode(), SKPixelTolerance.Absolute(3, 6, 9, null).GetHashCode());
		Assert.True(rgb != SKPixelTolerance.Exact);
		Assert.False(rgb.Equals("not a tolerance"));
		Assert.Equal(1, SKPixelTolerance.Percent(null, 50, null, null).ChannelCount);
		Assert.Equal(127.5, SKPixelTolerance.Percent(null, 50, null, null).Green);
		Assert.Null(SKPixelTolerance.Percent(null, 50, null, null).Red);
		Assert.Equal("value", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelTolerance.Absolute(-1)).ParamName);
		Assert.Equal("value", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelTolerance.Absolute(256)).ParamName);
		Assert.Equal("blue", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelTolerance.Absolute(0, 0, -1, null)).ParamName);
		Assert.Equal("alpha", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelTolerance.Absolute(0, 0, 0, 256)).ParamName);
		foreach (var invalid in new[] { -0.001, 100.001, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
		{
			Assert.Equal("value", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelTolerance.Percent(invalid)).ParamName);
			Assert.Equal("green", Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelTolerance.Percent(null, invalid, null, null)).ParamName);
		}
		Assert.Throws<ArgumentException>(() => SKPixelTolerance.Absolute(null, null, null, null));
		Assert.Throws<ArgumentException>(() => SKPixelTolerance.Percent(null, null, null, null));
	}

	[Fact]
	public void UniformAndUnevenThresholdsFilterChannelsButPreserveRawStatistics()
	{
		using var first = CreateBitmap(SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 6, 9));
		var strict = SKPixelComparer.Compare(first, second);
		var options = new SKPixelComparerOptions { Tolerance = SKPixelTolerance.Absolute(3, 5, 9, null) };
		var filtered = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(3, filtered.ChannelCount);
		Assert.Equal(1, filtered.ErrorPixelCount);
		Assert.False(filtered.IsMatch);
		Assert.Equal(18L, filtered.RawMetrics.AbsoluteError);
		Assert.Equal(126L, filtered.RawMetrics.SumSquaredError);
		Assert.Equal(6L, filtered.ThresholdedMetrics.AbsoluteError);
		Assert.Equal(36L, filtered.ThresholdedMetrics.SumSquaredError);
		Assert.Equal(3L, filtered.ThresholdedMetrics.SampleCount);
		Assert.Equal(2.0, filtered.ThresholdedMetrics.MeanAbsoluteError);
		Assert.Equal(12.0, filtered.ThresholdedMetrics.MeanSquaredError);
		Assert.Equal(9, filtered.MaxChannelDelta);
		options.Tolerance = SKPixelTolerance.Absolute(3, 6, 9, null);
		var tolerated = SKPixelComparer.Compare(first, second, options);
		Assert.True(tolerated.IsMatch);
		Assert.Equal(0, tolerated.ErrorPixelCount);
		Assert.Null(tolerated.ErrorBounds);
		Assert.Equal(0L, tolerated.ThresholdedMetrics.AbsoluteError);
		Assert.Equal(filtered.RawMetrics.AbsoluteError, tolerated.RawMetrics.AbsoluteError);
		Assert.Equal(filtered.RawMetrics.SumSquaredError, tolerated.RawMetrics.SumSquaredError);
		Assert.Equal(9, tolerated.MaxChannelDelta);
		Assert.Equal(126L, strict.RawMetrics.SumSquaredError);

		using var small = CreateBitmap(new SKColor(1, 0, 0));
		using var larger = CreateBitmap(new SKColor(2, 0, 0));
		options.Tolerance = SKPixelTolerance.Percent(0.5);
		Assert.Equal(0, SKPixelComparer.Compare(first, first, options).ErrorPixelCount);
		Assert.Equal(0, SKPixelComparer.Compare(small, larger, options).ErrorPixelCount);
		using var beyond = CreateBitmap(new SKColor(3, 0, 0));
		Assert.Equal(1, SKPixelComparer.Compare(small, beyond, options).ErrorPixelCount);
		options.Tolerance = SKPixelTolerance.Exact;
		Assert.Equal(1, SKPixelComparer.Compare(small, larger, options).ErrorPixelCount);
	}

	[Fact]
	public void DisabledChannelsChangeMetricsDenominatorsAndNeverReject()
	{
		using var first = CreateRawPremulBitmap(new SKColor(64, 0, 0, 64));
		using var second = CreateRawPremulBitmap(new SKColor(64, 0, 0, 128));
		var options = new SKPixelComparerOptions { AlphaType = SKAlphaType.Premul };
		var rgba = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(1, rgba.ErrorPixelCount);
		Assert.Equal(64L, rgba.RawMetrics.AbsoluteError);
		Assert.Equal(4096L, rgba.RawMetrics.SumSquaredError);
		Assert.Equal(4L, rgba.RawMetrics.SampleCount);
		Assert.Equal(64, rgba.MaxChannelDelta);
		options.Tolerance = SKPixelTolerance.Absolute(0, 0, 0, null);
		var rgb = SKPixelComparer.Compare(first, second, options);
		Assert.True(rgb.IsMatch);
		Assert.Equal(3, rgb.ChannelCount);
		Assert.Equal(0L, rgb.RawMetrics.AbsoluteError);
		Assert.Equal(3L, rgb.RawMetrics.SampleCount);
		Assert.Equal(0, rgb.MaxChannelDelta);
		options.Tolerance = SKPixelTolerance.Absolute(null, null, null, 0);
		var alpha = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(1, alpha.ChannelCount);
		Assert.Equal(1L, alpha.RawMetrics.SampleCount);
		Assert.Equal(64.0, alpha.RawMetrics.MeanAbsoluteError);
		Assert.Equal(4096.0, alpha.RawMetrics.MeanSquaredError);
		options.Tolerance = SKPixelTolerance.Absolute(null, null, null, 64);
		Assert.Equal(0, SKPixelComparer.Compare(first, second, options).ErrorPixelCount);

		using var redFirst = CreateBitmap(new SKColor(10, 20, 30));
		using var redSecond = CreateBitmap(new SKColor(13, 26, 39));
		options.AlphaType = SKAlphaType.Unpremul;
		options.Tolerance = SKPixelTolerance.Absolute(0, null, null, null);
		var red = SKPixelComparer.Compare(redFirst, redSecond, options);
		Assert.Equal(1L, red.RawMetrics.SampleCount);
		Assert.Equal(3L, red.RawMetrics.AbsoluteError);
		Assert.Equal(9L, red.RawMetrics.SumSquaredError);
		Assert.Equal(3, red.MaxChannelDelta);
	}

	[Fact]
	public void MatchBudgetsAreInclusiveAndOnlyConfiguredBudgetsApply()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 0, 0), new SKColor(6, 0, 0), SKColors.Black);
		var options = new SKPixelComparerOptions();
		var strict = SKPixelComparer.Compare(first, second);
		Assert.False(strict.IsMatch);
		Assert.Equal(2, strict.ErrorPixelCount);
		var rms = strict.RawMetrics.NormalizedRootMeanSquaredError;
		options.MaxNormalizedRootMeanSquaredError = rms;
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxNormalizedRootMeanSquaredError = rms - 0.000001;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxNormalizedRootMeanSquaredError = null;
		options.MaxErrorPixels = 2;
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxErrorPixels = 0;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxErrorPixels = 1;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxErrorPixels = null;
		options.MaxErrorPixelFraction = 2.0 / 3.0;
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxErrorPixelFraction = 0.66;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxErrorPixelFraction = 0.5;
		options.MaxErrorPixels = 3;
		options.MaxNormalizedRootMeanSquaredError = rms;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxErrorPixelFraction = 1;
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxErrorPixels = 1;
		Assert.False(SKPixelComparer.Compare(first, second, options).IsMatch);
		options.MaxErrorPixels = null;
		options.Tolerance = SKPixelTolerance.Absolute(6);
		options.MaxNormalizedRootMeanSquaredError = rms - 0.000001;
		var tolerated = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(0, tolerated.ErrorPixelCount);
		Assert.False(tolerated.IsMatch);
		Assert.Equal(rms, tolerated.RawMetrics.NormalizedRootMeanSquaredError);
	}

	[Fact]
	public void RejectedPixelsHaveAnExclusiveBoundingRectangle()
	{
		var info = new SKImageInfo(4, 3, SKColorType.Bgra8888, SKAlphaType.Unpremul);
		using var first = new SKBitmap(info);
		using var second = new SKBitmap(info);
		first.Erase(SKColors.Black);
		second.Erase(SKColors.Black);
		second.SetPixel(1, 0, SKColors.White);
		second.SetPixel(3, 2, SKColors.White);
		var result = SKPixelComparer.Compare(first, second);
		Assert.Equal(2, result.ErrorPixelCount);
		Assert.Equal(new SKRectI(1, 0, 4, 3), result.ErrorBounds);
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
			Tolerance = SKPixelTolerance.Absolute(5, 0, 0, null),
			ToleranceMask = mask,
		};
		var withMask = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(2, withMask.ErrorPixelCount);
		Assert.Equal(3, withMask.ChannelCount);
		Assert.Equal(6L, withMask.RawMetrics.SampleCount);
		Assert.Equal(36L, withMask.RawMetrics.AbsoluteError);
		Assert.Equal(252L, withMask.RawMetrics.SumSquaredError);
		Assert.Equal(21L, withMask.ThresholdedMetrics.AbsoluteError);
		Assert.Equal(153L, withMask.ThresholdedMetrics.SumSquaredError);
		options.ToleranceMask = null;
		var withoutMask = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(withMask.RawMetrics.SumSquaredError, withoutMask.RawMetrics.SumSquaredError);
		Assert.Equal(30L, withoutMask.ThresholdedMetrics.AbsoluteError);
		Assert.Equal(234L, withoutMask.ThresholdedMetrics.SumSquaredError);
		options.ToleranceMask = mask;
		options.Tolerance = SKPixelTolerance.Absolute(null, null, null, 0);
		var alphaOnly = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(1, alphaOnly.ChannelCount);
		Assert.Equal(0L, alphaOnly.RawMetrics.AbsoluteError);
		Assert.Equal(0, alphaOnly.ErrorPixelCount);
		Assert.Equal(0, alphaOnly.MaxChannelDelta);
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
			Tolerance = SKPixelTolerance.Absolute(null, null, null, 0),
			ToleranceMask = threshold,
		};
		var tolerated = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(1, tolerated.ChannelCount);
		Assert.Equal(64L, tolerated.RawMetrics.AbsoluteError);
		Assert.Equal(0, tolerated.ErrorPixelCount);
		using var smallerBitmap = CreateRawPremulBitmap(new SKColor(0, 0, 0, 63));
		using var smaller = SKImage.FromBitmap(smallerBitmap);
		options.ToleranceMask = smaller;
		Assert.Equal(1, SKPixelComparer.Compare(first, second, options).ErrorPixelCount);
	}

	[Fact]
	public void DifferenceImagesAgreeWithRejectionAndStayOpaqueIndependentOfInput()
	{
		using var first = CreateBitmap(new SKColor(10, 20, 30), new SKColor(40, 40, 40), new SKColor(80, 100, 120));
		using var second = CreateBitmap(new SKColor(13, 26, 39), new SKColor(40, 40, 40), new SKColor(90, 100, 120));
		using var maskBitmap = CreateBitmap(SKColors.Black, SKColors.Black, new SKColor(10, 0, 0));
		using var mask = SKImage.FromBitmap(maskBitmap);
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		var options = new SKPixelComparerOptions { Tolerance = SKPixelTolerance.Absolute(3, 5, 9, null), ToleranceMask = mask };
		Assert.Equal(1, SKPixelComparer.Compare(a, b, options).ErrorPixelCount);
		options.MaxErrorPixels = 1;
		Assert.True(SKPixelComparer.Compare(a, b, options).IsMatch);
		using var binary = SKPixelComparer.GenerateDifferenceMask(a, b, options);
		using var styledBinary = SKPixelComparer.GenerateDifferenceImage(a, b, options, SKPixelDifferenceStyle.BinaryMask);
		using var overlay = SKPixelComparer.GenerateDifferenceImage(a, b, options);
		using var explicitOverlay = SKPixelComparer.GenerateDifferenceImage(a, b, options, SKPixelDifferenceStyle.ThresholdOverlay);
		using var raw = SKPixelComparer.GenerateDifferenceImage(a, b, options, SKPixelDifferenceStyle.ChannelDelta);
		using var strictMask = SKPixelComparer.GenerateDifferenceMask(a, b);
		using var bitmapOverlay = SKPixelComparer.GenerateDifferenceImage(first, second);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		using var pixmapBinary = SKPixelComparer.GenerateDifferenceMask(firstPixmap, secondPixmap, options);
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
		Assert.Equal(1, SKPixelComparer.Compare(opaqueBlack, transparentBlack).ErrorPixelCount);
		using var defaultOverlay = SKPixelComparer.GenerateDifferenceImage(opaqueBlack, transparentBlack);
		AssertPixel(defaultOverlay, 0, SKColors.Red);

		using var first = CreateRawPremulBitmap(new SKColor(64, 0, 0, 64));
		using var second = CreateRawPremulBitmap(new SKColor(64, 0, 0, 128));
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		var options = new SKPixelComparerOptions { AlphaType = SKAlphaType.Premul };
		Assert.Equal(1, SKPixelComparer.Compare(first, second, options).ErrorPixelCount);
		Assert.Equal(1, SKPixelComparer.Compare(firstPixmap, secondPixmap, options).ErrorPixelCount);
		Assert.Equal(1, SKPixelComparer.Compare(a, b, options).ErrorPixelCount);
		using var overlay = SKPixelComparer.GenerateDifferenceImage(a, b, options);
		using var binary = SKPixelComparer.GenerateDifferenceMask(a, b, options);
		using var delta = SKPixelComparer.GenerateDifferenceImage(a, b, options, SKPixelDifferenceStyle.ChannelDelta);
		AssertPixel(overlay, 0, SKColors.Red);
		AssertPixel(binary, 0, SKColors.White);
		AssertPixel(delta, 0, SKColors.Black);
		options.Tolerance = SKPixelTolerance.Absolute(null, null, null, 64);
		using var tolerated = SKPixelComparer.GenerateDifferenceImage(first, second, options);
		AssertPixel(tolerated, 0, new SKColor(255, 200, 0));
		Assert.Equal(0, SKPixelComparer.Compare(a, b, options).ErrorPixelCount);
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
				Assert.Equal(0L, result.RawMetrics.AbsoluteError);
				Assert.Equal(0, result.MaxChannelDelta);
			}
			using var diff = SKPixelComparer.GenerateDifferenceMask(a, b, options);
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
		Assert.Equal(6L, image.ThresholdedMetrics.AbsoluteError);
		Assert.Equal(image.RawMetrics.AbsoluteError, bitmap.RawMetrics.AbsoluteError);
		Assert.Equal(image.RawMetrics.AbsoluteError, pixmap.RawMetrics.AbsoluteError);
		Assert.Equal(image.ThresholdedMetrics.SumSquaredError, bitmap.ThresholdedMetrics.SumSquaredError);
		Assert.Equal(image.ThresholdedMetrics.SumSquaredError, pixmap.ThresholdedMetrics.SumSquaredError);
		Assert.Equal(image.ErrorPixelCount, pixmap.ErrorPixelCount);
		Assert.Equal(new SKRectI(1, 1, 2, 2), image.ErrorBounds);
		Assert.Equal(18L, SKPixelComparer.Compare(first, second).RawMetrics.AbsoluteError);
		Assert.Equal(18L, SKPixelComparer.Compare(firstPixmap, secondPixmap).RawMetrics.AbsoluteError);
		using var difference = SKPixelComparer.GenerateDifferenceImage(firstPixmap, secondPixmap, options);
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
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare((SKImage)null!, image));
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare(image, (SKImage)null!));
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare((SKBitmap)null!, bitmap));
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare((SKPixmap)null!, pixmap));
		Assert.Throws<InvalidOperationException>(() => SKPixelComparer.Compare(image, mismatch));
		Assert.Throws<InvalidOperationException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { ToleranceMask = mismatch }));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(bitmap, emptyBitmap));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(pixmap, emptyPixmap));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.GenerateDifferenceImage(image, image, null, (SKPixelDifferenceStyle)99));
		foreach (var invalid in new[] { SKAlphaType.Opaque, SKAlphaType.Unknown })
			Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { AlphaType = invalid }));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.GenerateDifferenceMask(image, image, new SKPixelComparerOptions { AlphaType = SKAlphaType.Opaque }));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { MaxErrorPixels = -1 }));
		foreach (var invalid in new[] { -0.01, 1.01, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { MaxErrorPixelFraction = invalid }));
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
			Tolerance = SKPixelTolerance.Absolute(0, 0, 0, null),
		});
		Assert.Equal(8_294_400, result.TotalPixels);
		Assert.Equal(8_294_400, result.ErrorPixelCount);
		Assert.Equal(3, result.ChannelCount);
		Assert.Equal(24_883_200L, result.RawMetrics.SampleCount);
		Assert.Equal(6_345_216_000L, result.RawMetrics.AbsoluteError);
		Assert.Equal(1_618_030_080_000L, result.RawMetrics.SumSquaredError);
		Assert.Equal(255, result.MaxChannelDelta);
		Assert.Equal(255.0, result.RawMetrics.MeanAbsoluteError);
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
		Assert.Equal(0.0, metrics.MeanSquaredError);
		Assert.Equal(0.0, metrics.RootMeanSquaredError);
		Assert.Equal(0.0, metrics.NormalizedRootMeanSquaredError);
		Assert.Equal(double.PositiveInfinity, metrics.PeakSignalToNoiseRatio);
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
