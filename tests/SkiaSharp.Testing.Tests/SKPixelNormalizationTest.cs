using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Testing.Tests;

public class SKPixelNormalizationTest : SKPixelComparerTestBase
{
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
			using var overlay = SKPixelComparer.GenerateDifferenceImage(a, b, options: options);
			AssertPixel(overlay, 0, SKColors.White);
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
		using var difference = SKPixelComparer.GenerateDifferenceImage(firstPixmap, secondPixmap, SKPixelDifferenceStyle.ThresholdMap, options);
		using var pixels = SKBitmap.FromImage(difference);
		Assert.Equal(SKColors.Red, pixels.GetPixel(1, 1));
	}

	[Fact]
	public void IgnoreMaskRepresentationsAndPaddedRowsAgreeAcrossEveryInputAndDiagnosticFamily()
	{
		var info = new SKImageInfo(2, 2, SKColorType.Bgra8888, SKAlphaType.Unpremul);
		using var first = new SKBitmap(info, 16);
		using var second = new SKBitmap(info, 16);
		first.Erase(SKColors.Black);
		second.Erase(SKColors.Black);
		second.SetPixel(0, 1, SKColors.White);
		second.SetPixel(1, 1, new SKColor(3, 6, 9));
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		Assert.Equal(16, firstPixmap.RowBytes);
		Assert.Equal(16, secondPixmap.RowBytes);
		foreach (var colorType in new[] { SKColorType.Bgra8888, SKColorType.Rgba8888 })
		foreach (var alphaType in new[] { SKAlphaType.Unpremul, SKAlphaType.Premul, SKAlphaType.Opaque })
		{
			using var maskBitmap = new SKBitmap(new SKImageInfo(2, 2, colorType, alphaType), 16);
			maskBitmap.Erase(SKColors.Black);
			maskBitmap.SetPixel(0, 0, new SKColor(0, 0, 0, 0));
			maskBitmap.SetPixel(0, 1, new SKColor(0, 255, 0, 128));
			using var maskPixmap = maskBitmap.PeekPixels();
			using var mask = SKImage.FromPixels(maskPixmap);
			Assert.Equal(16, maskPixmap.RowBytes);
			var options = new SKPixelComparerOptions { IgnoreMask = mask };
			var image = SKPixelComparer.Compare(a, b, options);
			AssertEquivalent(image, SKPixelComparer.Compare(first, second, options));
			AssertEquivalent(image, SKPixelComparer.Compare(firstPixmap, secondPixmap, options));
			Assert.Equal(4, image.TotalPixels);
			Assert.Equal(3, image.ComparedPixelCount);
			Assert.Equal(1, image.IgnoredPixelCount);
			Assert.Equal(1, image.MismatchedPixelCount);
			Assert.Equal(1.0 / 3, image.MismatchedPixelFraction);
			Assert.Equal(new SKRectI(1, 1, 2, 2), image.MismatchBounds);
			Assert.Equal(9, image.MaxChannelDifference);
			Assert.Equal(12L, image.RawMetrics.SampleCount);
			Assert.Equal(18L, image.RawMetrics.SumAbsoluteError);
			Assert.Equal(126L, image.RawMetrics.SumSquaredError);
			using var encoded = mask.Encode(SKEncodedImageFormat.Png, 100);
			using var decodedMask = SKImage.FromEncodedData(encoded);
			options.IgnoreMask = decodedMask;
			AssertEquivalent(image, SKPixelComparer.Compare(first, second, options));
			options.IgnoreMask = mask;

			foreach (var style in Enum.GetValues<SKPixelDifferenceStyle>())
			{
				using var imageDiff = SKPixelComparer.GenerateDifferenceImage(a, b, style, options);
				using var bitmapDiff = SKPixelComparer.GenerateDifferenceImage(first, second, style, options);
				using var pixmapDiff = SKPixelComparer.GenerateDifferenceImage(firstPixmap, secondPixmap, style, options);
				options.IgnoreMask = null;
				using var unmaskedDiff = SKPixelComparer.GenerateDifferenceImage(first, second, style, options);
				using var unmaskedPixels = SKBitmap.FromImage(unmaskedDiff);
				foreach (var diff in new[] { imageDiff, bitmapDiff, pixmapDiff })
				{
					using var pixels = SKBitmap.FromImage(diff);
					for (var y = 0; y < first.Height; y++)
					for (var x = 0; x < first.Width; x++)
					{
						var expected = x == 0 && y == 1
							? style is SKPixelDifferenceStyle.ThresholdMap or SKPixelDifferenceStyle.Heatmap ? new SKColor(128, 128, 128)
								: style == SKPixelDifferenceStyle.ThresholdOverlay ? new SKColor(192, 192, 192) : SKColors.Black
							: unmaskedPixels.GetPixel(x, y);
						Assert.Equal(expected, pixels.GetPixel(x, y));
						Assert.Equal((byte)255, pixels.GetPixel(x, y).Alpha);
					}
				}
				options.IgnoreMask = mask;
			}
		}
	}
}
