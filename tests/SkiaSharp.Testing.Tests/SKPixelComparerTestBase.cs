using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Testing.Tests;

public abstract class SKPixelComparerTestBase
{
	protected static void SetUniformTolerance(SKPixelComparerOptions options, SKPixelTolerance tolerance)
	{
		options.RedTolerance = tolerance;
		options.GreenTolerance = tolerance;
		options.BlueTolerance = tolerance;
		options.AlphaTolerance = tolerance;
	}

	protected static double JustBelow(double positiveValue) =>
		BitConverter.Int64BitsToDouble(BitConverter.DoubleToInt64Bits(positiveValue) - 1);

	protected static Action[] Operations(SKBitmap first, SKBitmap second, SKImage a, SKImage b, SKPixmap firstPixmap, SKPixmap secondPixmap,
		SKPixelComparerOptions options, SKPixelDifferenceStyle style = SKPixelDifferenceStyle.ThresholdOverlay) =>
		new Action[]
		{
			() => SKPixelComparer.Compare(first, second, options),
			() => SKPixelComparer.Compare(a, b, options),
			() => SKPixelComparer.Compare(firstPixmap, secondPixmap, options),
			() => { using var diff = SKPixelComparer.GenerateDifferenceImage(first, second, style, options); },
			() => { using var diff = SKPixelComparer.GenerateDifferenceImage(a, b, style, options); },
			() => { using var diff = SKPixelComparer.GenerateDifferenceImage(firstPixmap, secondPixmap, style, options); },
		};

	protected static void AssertEquivalent(SKPixelComparisonResult expected, SKPixelComparisonResult actual)
	{
		Assert.Equal(expected.IsMatch, actual.IsMatch);
		Assert.Equal(expected.TotalPixels, actual.TotalPixels);
		Assert.Equal(expected.ComparedPixelCount, actual.ComparedPixelCount);
		Assert.Equal(expected.IgnoredPixelCount, actual.IgnoredPixelCount);
		Assert.Equal(expected.ChannelCount, actual.ChannelCount);
		Assert.Equal(expected.MismatchedPixelCount, actual.MismatchedPixelCount);
		Assert.Equal(expected.MismatchedPixelFraction, actual.MismatchedPixelFraction);
		Assert.Equal(expected.MismatchBounds, actual.MismatchBounds);
		Assert.Equal(expected.MaxChannelDifference, actual.MaxChannelDifference);
		AssertMetrics(expected.RawMetrics, actual.RawMetrics);
		AssertMetrics(expected.ThresholdedMetrics, actual.ThresholdedMetrics);

		static void AssertMetrics(SKPixelComparisonMetrics expected, SKPixelComparisonMetrics actual)
		{
			Assert.Equal(expected.SampleCount, actual.SampleCount);
			Assert.Equal(expected.SumAbsoluteError, actual.SumAbsoluteError);
			Assert.Equal(expected.SumSquaredError, actual.SumSquaredError);
			Assert.Equal(expected.MeanAbsoluteError, actual.MeanAbsoluteError);
			Assert.Equal(expected.NormalizedMeanAbsoluteError, actual.NormalizedMeanAbsoluteError);
			Assert.Equal(expected.MeanSquaredError, actual.MeanSquaredError);
			Assert.Equal(expected.RootMeanSquaredError, actual.RootMeanSquaredError);
			Assert.Equal(expected.NormalizedRootMeanSquaredError, actual.NormalizedRootMeanSquaredError);
			Assert.Equal(expected.PeakSignalToNoiseRatio, actual.PeakSignalToNoiseRatio);
		}
	}

	protected static SKBitmap CreateBitmap(params SKColor[] colors)
	{
		var bitmap = new SKBitmap(new SKImageInfo(colors.Length, 1, SKColorType.Bgra8888, SKAlphaType.Unpremul));
		using var pixmap = bitmap.PeekPixels();
		var pixels = pixmap.GetPixelSpan<SKColor>();
		for (var i = 0; i < colors.Length; i++)
			pixels[i] = colors[i];
		return bitmap;
	}

	protected static SKBitmap CreateRawPremulBitmap(SKColor storedPixel)
	{
		var bitmap = new SKBitmap(new SKImageInfo(1, 1, SKColorType.Bgra8888, SKAlphaType.Premul));
		using var pixmap = bitmap.PeekPixels();
		pixmap.GetPixelSpan<SKColor>()[0] = storedPixel;
		return bitmap;
	}

	protected static void AssertPixel(SKImage image, int x, SKColor expected)
	{
		using var bitmap = SKBitmap.FromImage(image);
		Assert.Equal(expected, bitmap.GetPixel(x, 0));
		Assert.Equal((byte)255, bitmap.GetPixel(x, 0).Alpha);
	}
}
