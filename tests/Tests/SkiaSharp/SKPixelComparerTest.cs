using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Tests;

public class SKPixelComparerTest
{
	[Fact]
	public void StrictRgbStatisticsUseTheUpstreamChannelDenominator()
	{
		using var first = CreateBitmap(new SKColor(10, 20, 30), SKColors.Black);
		using var second = CreateBitmap(new SKColor(13, 26, 39), SKColors.Black);
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);

		var result = SKPixelComparer.Compare(a, b);
		Assert.Equal(2, result.TotalPixels);
		Assert.Equal(1, result.ErrorPixelCount);
		Assert.Equal(0.5, result.ErrorPixelPercentage);
		Assert.Equal(18L, result.AbsoluteError);
		Assert.Equal(126L, result.SumSquaredError);
		Assert.Equal(3, result.ChannelCount);
		Assert.Equal(9, result.MaxChannelDelta);
		Assert.Equal(3.0, result.MeanAbsoluteError);
		Assert.Equal(21.0, result.MeanSquaredError);
		Assert.Equal(Math.Sqrt(21), result.RootMeanSquaredError, 12);
		Assert.Equal(Math.Sqrt(21) / 255, result.NormalizedRootMeanSquaredError, 12);
		Assert.Equal(10 * Math.Log10(65025.0 / 21), result.PeakSignalToNoiseRatio, 12);

		var identical = SKPixelComparer.Compare(a, a);
		Assert.Equal(0L, identical.AbsoluteError);
		Assert.Equal(0L, identical.SumSquaredError);
		Assert.Equal(0.0, identical.MeanAbsoluteError);
		Assert.Equal(0.0, identical.MeanSquaredError);
		Assert.Equal(0.0, identical.RootMeanSquaredError);
		Assert.Equal(0.0, identical.NormalizedRootMeanSquaredError);
		Assert.Equal(double.PositiveInfinity, identical.PeakSignalToNoiseRatio);
	}

	[Fact]
	public void UniformToleranceFiltersIndividualChannelsOrTheWholeSum()
	{
		using var first = CreateBitmap(SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 6, 9));
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);

		var perChannel = SKPixelComparer.Compare(a, b, 5, null);
		Assert.Equal(1, perChannel.ErrorPixelCount);
		Assert.Equal(15L, perChannel.AbsoluteError);
		Assert.Equal(117L, perChannel.SumSquaredError);
		Assert.Equal(9, perChannel.MaxChannelDelta);

		var atBoundary = SKPixelComparer.Compare(a, b, 9, null);
		Assert.Equal(0, atBoundary.ErrorPixelCount);
		Assert.Equal(0L, atBoundary.AbsoluteError);
		Assert.Equal(0L, atBoundary.SumSquaredError);
		Assert.Equal(9, atBoundary.MaxChannelDelta);

		var summed = new SKPixelComparerOptions { TolerancePerChannel = false };
		Assert.Equal(1, SKPixelComparer.Compare(a, b, 17, summed).ErrorPixelCount);
		Assert.Equal(18L, SKPixelComparer.Compare(a, b, 17, summed).AbsoluteError);
		Assert.Equal(126L, SKPixelComparer.Compare(a, b, 17, summed).SumSquaredError);
		Assert.Equal(0, SKPixelComparer.Compare(a, b, 18, summed).ErrorPixelCount);
		Assert.Equal(0, SKPixelComparer.Compare(a, b, int.MaxValue, null).ErrorPixelCount);
		Assert.Equal(0, SKPixelComparer.Compare(a, b, int.MaxValue, summed).ErrorPixelCount);
	}

	[Fact]
	public void MaskThresholdsFilterChannelsAndSumUsingStrictGreaterThan()
	{
		using var first = CreateBitmap(SKColors.Black);
		using var second = CreateBitmap(new SKColor(3, 6, 9));
		using var mask = CreateBitmap(new SKColor(3, 5, 9));
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		using var m = SKImage.FromBitmap(mask);

		var result = SKPixelComparer.Compare(a, b, m, null);
		Assert.Equal(1, result.ErrorPixelCount);
		Assert.Equal(6L, result.AbsoluteError);
		Assert.Equal(36L, result.SumSquaredError);
		Assert.Equal(9, result.MaxChannelDelta);

		var summed = SKPixelComparer.Compare(a, b, m, new SKPixelComparerOptions { TolerancePerChannel = false });
		Assert.Equal(1, summed.ErrorPixelCount);
		Assert.Equal(18L, summed.AbsoluteError);
		Assert.Equal(126L, summed.SumSquaredError);

		mask.SetPixel(0, 0, new SKColor(3, 6, 9));
		using var exactMask = SKImage.FromBitmap(mask);
		Assert.Equal(0, SKPixelComparer.Compare(a, b, exactMask, null).ErrorPixelCount);
		Assert.Equal(0, SKPixelComparer.Compare(a, b, exactMask, new SKPixelComparerOptions { TolerancePerChannel = false }).ErrorPixelCount);
	}

	[Fact]
	public void AlphaSelectionAndPremultiplicationHaveDistinctEffects()
	{
		using var first = CreateBitmap(new SKColor(20, 30, 40, 64));
		using var second = CreateBitmap(new SKColor(20, 30, 40, 128));
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		Assert.Equal(0, SKPixelComparer.Compare(a, b).ErrorPixelCount);

		var withAlpha = new SKPixelComparerOptions { CompareAlpha = true };
		var result = SKPixelComparer.Compare(a, b, withAlpha);
		Assert.Equal(4, result.ChannelCount);
		Assert.Equal(1, result.ErrorPixelCount);
		Assert.Equal(64L, result.AbsoluteError);
		Assert.Equal(4096L, result.SumSquaredError);
		Assert.Equal(64, result.MaxChannelDelta);
		Assert.Equal(0, SKPixelComparer.Compare(a, b, 64, withAlpha).ErrorPixelCount);
		Assert.Equal(64, SKPixelComparer.Compare(a, b, 64, withAlpha).MaxChannelDelta);

		using var premulFirst = CreateRawPremulBitmap(new SKColor(64, 0, 0, 64));
		using var premulSecond = CreateRawPremulBitmap(new SKColor(64, 0, 0, 128));
		var premul = new SKPixelComparerOptions { AlphaType = SKAlphaType.Premul };
		Assert.Equal(0, SKPixelComparer.Compare(premulFirst, premulSecond, premul).ErrorPixelCount);
		Assert.Equal(1, SKPixelComparer.Compare(premulFirst, premulSecond, null).ErrorPixelCount);
		premul.CompareAlpha = true;
		var alphaOnly = SKPixelComparer.Compare(premulFirst, premulSecond, premul);
		Assert.Equal(64L, alphaOnly.AbsoluteError);
		Assert.Equal(64, alphaOnly.MaxChannelDelta);
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
			var options = new SKPixelComparerOptions { AlphaType = alphaType, CompareAlpha = true };
			var bitmapResult = SKPixelComparer.Compare(first, second, options);
			var pixmapResult = SKPixelComparer.Compare(firstPixmap, secondPixmap, options);
			var imageResult = SKPixelComparer.Compare(a, b, options);
			Assert.Equal(0, bitmapResult.ErrorPixelCount);
			Assert.Equal(0L, bitmapResult.AbsoluteError);
			Assert.Equal(0L, bitmapResult.SumSquaredError);
			Assert.Equal(0, bitmapResult.MaxChannelDelta);
			Assert.Equal(0, pixmapResult.ErrorPixelCount);
			Assert.Equal(0, imageResult.ErrorPixelCount);

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
		using var mask = new SKBitmap(info, 16);
		first.Erase(SKColors.Black);
		second.Erase(SKColors.Black);
		mask.Erase(SKColors.Black);
		second.SetPixel(1, 1, new SKColor(3, 6, 9));
		mask.SetPixel(1, 1, new SKColor(3, 5, 9));
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		using var maskPixmap = mask.PeekPixels();
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		using var m = SKImage.FromBitmap(mask);

		Assert.Equal(16, firstPixmap.RowBytes);
		var image = SKPixelComparer.Compare(a, b, m, null);
		var bitmap = SKPixelComparer.Compare(first, second, mask, null);
		var pixmap = SKPixelComparer.Compare(firstPixmap, secondPixmap, maskPixmap, null);
		Assert.Equal(6L, image.AbsoluteError);
		Assert.Equal(image.AbsoluteError, bitmap.AbsoluteError);
		Assert.Equal(image.AbsoluteError, pixmap.AbsoluteError);
		Assert.Equal(image.SumSquaredError, bitmap.SumSquaredError);
		Assert.Equal(image.SumSquaredError, pixmap.SumSquaredError);
		Assert.Equal(image.ErrorPixelCount, pixmap.ErrorPixelCount);
		Assert.Equal(18L, SKPixelComparer.Compare(first, second).AbsoluteError);
		Assert.Equal(18L, SKPixelComparer.Compare(firstPixmap, secondPixmap).AbsoluteError);
		Assert.Equal(15L, SKPixelComparer.Compare(first, second, 5, null).AbsoluteError);
		Assert.Equal(15L, SKPixelComparer.Compare(firstPixmap, secondPixmap, 5, null).AbsoluteError);
	}

	[Fact]
	public void DifferenceStylesProduceExactOpaqueColorsAndSurviveTemporaryDisposalAndPng()
	{
		using var first = CreateBitmap(new SKColor(10, 20, 30), new SKColor(40, 40, 40), new SKColor(80, 100, 120));
		using var second = CreateBitmap(new SKColor(13, 26, 39), new SKColor(40, 40, 40), new SKColor(90, 100, 120));
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		using var raw = SKPixelComparer.GenerateDifferenceImage(a, b);
		using var mask = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.BinaryMask, 9, null);
		using var strictMask = SKPixelComparer.GenerateDifferenceMask(a, b);
		using var overlay = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ThresholdOverlay, 9, null);
		using var bitmapDiff = SKPixelComparer.GenerateDifferenceImage(first, second);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		using var pixmapDiff = SKPixelComparer.GenerateDifferenceMask(firstPixmap, secondPixmap);

		AssertPixel(raw, 0, new SKColor(3, 6, 9));
		AssertPixel(raw, 1, SKColors.Black);
		AssertPixel(raw, 2, new SKColor(10, 0, 0));
		AssertPixel(bitmapDiff, 0, new SKColor(3, 6, 9));
		AssertPixel(mask, 0, SKColors.Black);
		AssertPixel(mask, 1, SKColors.Black);
		AssertPixel(mask, 2, SKColors.White);
		AssertPixel(strictMask, 0, SKColors.White);
		AssertPixel(pixmapDiff, 0, SKColors.White);
		AssertPixel(overlay, 0, new SKColor(255, 200, 0));
		AssertPixel(overlay, 1, new SKColor(10, 10, 10));
		AssertPixel(overlay, 2, SKColors.Red);
		using var png = overlay.Encode(SKEncodedImageFormat.Png, 100);
		using var encoded = SKBitmap.Decode(png);
		Assert.Equal(new SKColor(255, 200, 0), encoded.GetPixel(0, 0));
		Assert.Equal(new SKColor(10, 10, 10), encoded.GetPixel(1, 0));
		Assert.Equal(SKColors.Red, encoded.GetPixel(2, 0));
		Assert.Equal(new SKColor(13, 26, 39), second.GetPixel(0, 0));
		Assert.Equal(new SKColor(10, 20, 30), first.GetPixel(0, 0));
	}

	[Fact]
	public void AlphaOnlyDifferenceIsVisibleInBinaryAndOverlayButNotRawRgb()
	{
		using var first = CreateRawPremulBitmap(new SKColor(64, 0, 0, 64));
		using var second = CreateRawPremulBitmap(new SKColor(64, 0, 0, 128));
		var options = new SKPixelComparerOptions { CompareAlpha = true, AlphaType = SKAlphaType.Premul };
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		using var delta = SKPixelComparer.GenerateDifferenceImage(a, b, options);
		using var binary = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.BinaryMask, 63, options);
		using var overlay = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ThresholdOverlay, 64, options);
		AssertPixel(delta, 0, SKColors.Black);
		AssertPixel(binary, 0, SKColors.White);
		AssertPixel(overlay, 0, new SKColor(255, 200, 0));
		Assert.Equal(0, SKPixelComparer.Compare(a, b, 64, options).ErrorPixelCount);
	}

	[Fact]
	public void InvalidInputsFailExplicitlyBeforePixelReads()
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
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare(image, image, (SKImage)null!, null));
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare((SKBitmap)null!, bitmap));
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare((SKPixmap)null!, pixmap));
		Assert.Throws<InvalidOperationException>(() => SKPixelComparer.Compare(image, mismatch));
		Assert.Throws<InvalidOperationException>(() => SKPixelComparer.Compare(image, image, mismatch, null));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(bitmap, emptyBitmap));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(pixmap, emptyPixmap));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.Compare(image, image, -1, null));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.Compare(bitmap, bitmap, -1, null));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.GenerateDifferenceImage(image, image, SKPixelDifferenceStyle.BinaryMask, -1, null));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.GenerateDifferenceImage(image, image, (SKPixelDifferenceStyle)99, 0, null));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { AlphaType = SKAlphaType.Opaque }));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { AlphaType = SKAlphaType.Unknown }));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.GenerateDifferenceMask(image, image, new SKPixelComparerOptions { AlphaType = SKAlphaType.Opaque }));
		image.Dispose();
		bitmap.Dispose();
		pixmap.Dispose();
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(image, image));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(bitmap, bitmap));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(pixmap, pixmap));
	}

	[Fact]
	public void FourKMaximumContrastDoesNotOverflowAbsoluteError()
	{
		var info = new SKImageInfo(3840, 2160, SKColorType.Bgra8888, SKAlphaType.Opaque);
		using var first = new SKBitmap(info);
		using var second = new SKBitmap(info);
		first.Erase(SKColors.Black);
		second.Erase(SKColors.White);
		var result = SKPixelComparer.Compare(first, second);
		Assert.Equal(8_294_400, result.TotalPixels);
		Assert.Equal(8_294_400, result.ErrorPixelCount);
		Assert.Equal(6_345_216_000L, result.AbsoluteError);
		Assert.Equal(1_618_030_080_000L, result.SumSquaredError);
		Assert.Equal(255, result.MaxChannelDelta);
		Assert.Equal(255.0, result.MeanAbsoluteError);
		Assert.Equal(65025.0, result.MeanSquaredError);
		Assert.Equal(255.0, result.RootMeanSquaredError);
		Assert.Equal(1.0, result.NormalizedRootMeanSquaredError);
		Assert.Equal(0.0, result.PeakSignalToNoiseRatio);
	}

	[Fact]
	public void EmptyResultMetricsHaveDefinedZeroDenominators()
	{
		var result = new SKPixelComparisonResult(0, 0, 0, 0, 3, 0);
		Assert.Equal(0.0, result.ErrorPixelPercentage);
		Assert.Equal(0.0, result.MeanAbsoluteError);
		Assert.Equal(0.0, result.MeanSquaredError);
		Assert.Equal(0.0, result.RootMeanSquaredError);
		Assert.Equal(0.0, result.NormalizedRootMeanSquaredError);
		Assert.Equal(double.PositiveInfinity, result.PeakSignalToNoiseRatio);
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
