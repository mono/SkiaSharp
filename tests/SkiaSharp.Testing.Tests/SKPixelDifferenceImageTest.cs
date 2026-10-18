using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Testing.Tests;

public class SKPixelDifferenceImageTest : SKImageComparisonTestBase
{
	[Theory]
	[InlineData(0, 0, 0, 0)]
	[InlineData(1, 0, 0, 255)]
	[InlineData(2, 0, 3, 255)]
	[InlineData(43, 0, 127, 255)]
	[InlineData(84, 0, 251, 255)]
	[InlineData(85, 0, 255, 255)]
	[InlineData(86, 3, 255, 252)]
	[InlineData(127, 126, 255, 129)]
	[InlineData(169, 252, 255, 3)]
	[InlineData(170, 255, 255, 0)]
	[InlineData(171, 255, 252, 0)]
	[InlineData(212, 255, 129, 0)]
	[InlineData(254, 255, 3, 0)]
	[InlineData(255, 255, 0, 0)]
	public void HeatmapUsesFixedRawMagnitudeScaleInEitherInputOrder(byte difference, byte red, byte green, byte blue)
	{
		using var first = CreateBitmap(SKColors.Black);
		using var second = CreateBitmap(new SKColor(difference, 0, 0));
		using var heatmap = SKImageComparison.GenerateDifferenceImage(first, second, SKDifferenceStyle.Heatmap);
		using var reversed = SKImageComparison.GenerateDifferenceImage(second, first, SKDifferenceStyle.Heatmap);
		var expected = new SKColor(red, green, blue);
		AssertPixel(heatmap, 0, expected);
		AssertPixel(reversed, 0, expected);
	}

	[Fact]
	public void HeatmapRetainsScalarAndMaskToleratedMagnitudesAcrossEveryDecodedFamily()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black, SKColors.Black, SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(SKColors.Black, new SKColor(1, 0, 0), new SKColor(0, 85, 0),
			new SKColor(0, 0, 170), SKColors.White);
		using var toleranceBitmap = CreateBitmap(SKColors.Black, SKColors.Black, new SKColor(0, 85, 0), SKColors.Black, SKColors.Black);
		using var toleranceMask = SKImage.FromBitmap(toleranceBitmap);
		using var ignoreBitmap = CreateBitmap(SKColors.Black, SKColors.Black, SKColors.Black, SKColors.Black, SKColors.White);
		using var ignoreMask = SKImage.FromBitmap(ignoreBitmap);
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		var options = new SKComparisonOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(1),
			ToleranceMask = toleranceMask,
			IgnoreMask = ignoreMask,
		};
		var comparison = SKImageComparison.Compare(first, second, options);
		Assert.False(comparison.IsMatch);
		Assert.Equal(1, comparison.MismatchedPixelCount);
		Assert.Equal(170, comparison.MaxChannelDifference);
		Assert.Equal(256L, comparison.RawMetrics.SumAbsoluteError);
		Assert.Equal(170L, comparison.ThresholdedMetrics.SumAbsoluteError);
		using var imageHeatmap = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.Heatmap, options);
		using var bitmapHeatmap = SKImageComparison.GenerateDifferenceImage(first, second, SKDifferenceStyle.Heatmap, options);
		using var pixmapHeatmap = SKImageComparison.GenerateDifferenceImage(firstPixmap, secondPixmap, SKDifferenceStyle.Heatmap, options);
		foreach (var heatmap in new[] { imageHeatmap, bitmapHeatmap, pixmapHeatmap })
		{
			Assert.Equal(SKColorType.Bgra8888, heatmap.ColorType);
			Assert.Equal(SKAlphaType.Opaque, heatmap.AlphaType);
			AssertPixel(heatmap, 0, SKColors.Black);
			AssertPixel(heatmap, 1, SKColors.Blue);
			AssertPixel(heatmap, 2, SKColors.Cyan);
			AssertPixel(heatmap, 3, SKColors.Yellow);
			AssertPixel(heatmap, 4, new SKColor(128, 128, 128));
		}
		AssertEquivalent(comparison, SKImageComparison.Compare(first, second, options));
	}

	[Fact]
	public void HeatmapExcludesDisabledChannelsFromItsMaximum()
	{
		using var first = CreateBitmap(new SKColor(10, 20, 30), new SKColor(10, 20, 30));
		using var second = CreateBitmap(new SKColor(10, 255, 255), new SKColor(53, 255, 255));
		var options = new SKComparisonOptions { GreenTolerance = null, BlueTolerance = null, AlphaTolerance = null };
		Assert.Equal(43, SKImageComparison.Compare(first, second, options).MaxChannelDifference);
		using var heatmap = SKImageComparison.GenerateDifferenceImage(first, second, SKDifferenceStyle.Heatmap, options);
		AssertPixel(heatmap, 0, SKColors.Black);
		AssertPixel(heatmap, 1, new SKColor(0, 127, 255));
	}

	[Theory]
	[InlineData(SKAlphaType.Unpremul)]
	[InlineData(SKAlphaType.Premul)]
	public void HeatmapShowsToleratedAlphaOnlyMagnitudeWithoutFlattening(SKAlphaType alphaType)
	{
		using var first = CreateRawPremulBitmap(new SKColor(16, 32, 48, 64));
		using var second = CreateRawPremulBitmap(new SKColor(64, 64, 64, 128));
		var options = new SKComparisonOptions
		{
			AlphaType = alphaType,
			RedTolerance = null,
			GreenTolerance = null,
			BlueTolerance = null,
			AlphaTolerance = SKPixelTolerance.Absolute(64),
		};
		var comparison = SKImageComparison.Compare(first, second, options);
		Assert.True(comparison.IsMatch);
		Assert.Equal(64, comparison.MaxChannelDifference);
		Assert.Equal(64L, comparison.RawMetrics.SumAbsoluteError);
		Assert.Equal(0L, comparison.ThresholdedMetrics.SumAbsoluteError);
		using var heatmap = SKImageComparison.GenerateDifferenceImage(first, second, SKDifferenceStyle.Heatmap, options);
		AssertPixel(heatmap, 0, new SKColor(0, 191, 255));
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
		var options = new SKComparisonOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(3),
			GreenTolerance = SKPixelTolerance.Absolute(5),
			BlueTolerance = SKPixelTolerance.Absolute(9),
			AlphaTolerance = null,
			ToleranceMask = mask,
		};
		Assert.Equal(1, SKImageComparison.Compare(a, b, options).MismatchedPixelCount);
		options.MismatchBudget = SKPixelMismatchBudget.Pixels(1);
		Assert.True(SKImageComparison.Compare(a, b, options).IsMatch);
		using var binary = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.BinaryMask, options);
		using var styledBinary = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.BinaryMask, options);
		using var map = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.ThresholdMap, options);
		using var raw = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.ChannelDifference, options);
		using var strictMask = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.BinaryMask, null);
		using var bitmapMap = SKImageComparison.GenerateDifferenceImage(first, second, SKDifferenceStyle.ThresholdMap);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		using var pixmapBinary = SKImageComparison.GenerateDifferenceImage(firstPixmap, secondPixmap, SKDifferenceStyle.BinaryMask, options);
		AssertPixel(binary, 0, SKColors.White);
		AssertPixel(binary, 1, SKColors.Black);
		AssertPixel(binary, 2, SKColors.Black);
		AssertPixel(styledBinary, 2, SKColors.Black);
		AssertPixel(pixmapBinary, 0, SKColors.White);
		AssertPixel(map, 0, SKColors.Red);
		AssertPixel(map, 1, new SKColor(10, 10, 10));
		AssertPixel(map, 2, new SKColor(255, 200, 0));
		AssertPixel(bitmapMap, 0, SKColors.Red);
		AssertPixel(raw, 0, new SKColor(3, 6, 9));
		AssertPixel(raw, 1, SKColors.Black);
		AssertPixel(raw, 2, new SKColor(10, 0, 0));
		AssertPixel(strictMask, 2, SKColors.White);
		using var png = map.Encode(SKEncodedImageFormat.Png, 100);
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
		AssertPixel(map, 2, new SKColor(255, 200, 0));
	}

	[Fact]
	public void DefaultOverlayBlendsClassificationsOverActualInEveryDecodedFamily()
	{
		using var first = CreateBitmap(new SKColor(10, 20, 30), new SKColor(80, 100, 120),
			new SKColor(40, 60, 80), new SKColor(150, 160, 170));
		using var second = CreateBitmap(new SKColor(13, 26, 39), new SKColor(90, 100, 120),
			new SKColor(40, 60, 80), new SKColor(20, 40, 60));
		using var toleranceBitmap = CreateBitmap(SKColors.Black, new SKColor(10, 0, 0), SKColors.Black, SKColors.Black);
		using var toleranceMask = SKImage.FromBitmap(toleranceBitmap);
		using var ignoreBitmap = CreateBitmap(SKColors.Black, SKColors.Black, SKColors.Black, SKColors.White);
		using var ignoreMask = SKImage.FromBitmap(ignoreBitmap);
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		var options = new SKComparisonOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(3),
			GreenTolerance = SKPixelTolerance.Absolute(5),
			BlueTolerance = SKPixelTolerance.Absolute(9),
			ToleranceMask = toleranceMask,
			IgnoreMask = ignoreMask,
		};
		var comparison = SKImageComparison.Compare(first, second, options);
		Assert.Equal(1, comparison.MismatchedPixelCount);
		Assert.Equal(3, comparison.ComparedPixelCount);
		Assert.Equal(1, comparison.IgnoredPixelCount);
		using var imageDefault = SKImageComparison.GenerateDifferenceImage(a, b, options: options);
		using var imageOverlay = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.ThresholdOverlay, options);
		using var bitmapDefault = SKImageComparison.GenerateDifferenceImage(first, second, options: options);
		using var bitmapOverlay = SKImageComparison.GenerateDifferenceImage(first, second, SKDifferenceStyle.ThresholdOverlay, options);
		using var pixmapDefault = SKImageComparison.GenerateDifferenceImage(firstPixmap, secondPixmap, options: options);
		using var pixmapOverlay = SKImageComparison.GenerateDifferenceImage(firstPixmap, secondPixmap, SKDifferenceStyle.ThresholdOverlay, options);
		foreach (var image in new[] { imageDefault, imageOverlay, bitmapDefault, bitmapOverlay, pixmapDefault, pixmapOverlay })
		{
			Assert.Equal(SKColorType.Bgra8888, image.ColorType);
			Assert.Equal(SKAlphaType.Opaque, image.AlphaType);
			AssertPixel(image, 0, new SKColor(134, 13, 20));
			AssertPixel(image, 1, new SKColor(173, 150, 60));
			AssertPixel(image, 2, new SKColor(40, 60, 80));
			AssertPixel(image, 3, new SKColor(74, 84, 94));
		}
		using var encoded = imageDefault.Encode(SKEncodedImageFormat.Png, 100);
		using var decoded = SKBitmap.Decode(encoded);
		Assert.Equal(new SKColor(134, 13, 20), decoded.GetPixel(0, 0));
		Assert.Equal(new SKColor(74, 84, 94), decoded.GetPixel(3, 0));
		AssertEquivalent(comparison, SKImageComparison.Compare(first, second, options));
	}

	[Theory]
	[InlineData(SKAlphaType.Unpremul)]
	[InlineData(SKAlphaType.Premul)]
	public void OverlayFlattensActualTransparencyOntoWhiteBeforeEveryClassification(SKAlphaType alphaType)
	{
		var actual = new SKColor(60, 120, 180, 85);
		using var first = CreateBitmap(new SKColor(0, 0, 0, 85), new SKColor(57, 120, 180, 85), actual, SKColors.White);
		using var second = CreateBitmap(actual, actual, actual, actual);
		using var ignoreBitmap = CreateBitmap(SKColors.Black, SKColors.Black, SKColors.Black, SKColors.White);
		using var ignoreMask = SKImage.FromBitmap(ignoreBitmap);
		var options = new SKComparisonOptions
		{
			AlphaType = alphaType,
			RedTolerance = SKPixelTolerance.Absolute(3),
			IgnoreMask = ignoreMask,
		};
		var result = SKImageComparison.Compare(first, second, options);
		Assert.Equal(1, result.MismatchedPixelCount);
		Assert.Equal(3, result.ComparedPixelCount);
		using var overlay = SKImageComparison.GenerateDifferenceImage(first, second, options: options);
		AssertPixel(overlay, 0, new SKColor(223, 105, 115));
		AssertPixel(overlay, 1, new SKColor(223, 205, 115));
		AssertPixel(overlay, 2, new SKColor(190, 210, 230));
		AssertPixel(overlay, 3, new SKColor(159, 169, 179));
	}

	[Fact]
	public void OverlayUsesSecondInputAndDisplaysDisabledChannelsForExactSelectedMatches()
	{
		using var first = CreateBitmap(new SKColor(20, 30, 40));
		using var second = CreateBitmap(new SKColor(20, 100, 200));
		var options = new SKComparisonOptions { GreenTolerance = null, BlueTolerance = null, AlphaTolerance = null };
		Assert.True(SKImageComparison.Compare(first, second, options).IsMatch);
		using var overlay = SKImageComparison.GenerateDifferenceImage(first, second, options: options);
		using var reversed = SKImageComparison.GenerateDifferenceImage(second, first, options: options);
		using var map = SKImageComparison.GenerateDifferenceImage(first, second, SKDifferenceStyle.ThresholdMap, options);
		AssertPixel(overlay, 0, new SKColor(20, 100, 200));
		AssertPixel(reversed, 0, new SKColor(20, 30, 40));
		AssertPixel(map, 0, new SKColor(5, 25, 50));
	}

	[Theory]
	[InlineData(SKAlphaType.Unpremul)]
	[InlineData(SKAlphaType.Premul)]
	public void OverlayFlattensHalfTransparentExactMatchesWithRoundedChannels(SKAlphaType alphaType)
	{
		using var actual = CreateRawPremulBitmap(new SKColor(96, 64, 32, 128));
		using var overlay = SKImageComparison.GenerateDifferenceImage(actual, actual,
			options: new SKComparisonOptions { AlphaType = alphaType });
		AssertPixel(overlay, 0, new SKColor(223, 191, 159));
	}

	[Fact]
	public void ThresholdMapDimsNormalizedActualWithoutFlatteningTransparency()
	{
		using var actual = CreateRawPremulBitmap(new SKColor(96, 64, 32, 128));
		using var map = SKImageComparison.GenerateDifferenceImage(actual, actual, SKDifferenceStyle.ThresholdMap,
			new SKComparisonOptions { AlphaType = SKAlphaType.Premul });
		AssertPixel(map, 0, new SKColor(24, 16, 8));
	}

	[Fact]
	public void AlphaOnlyDifferenceIsVisibleByDefaultInAllDecodedFamilies()
	{
		using var opaqueBlack = CreateBitmap(SKColors.Black);
		using var transparentBlack = CreateBitmap(SKColors.Transparent);
		Assert.Equal(1, SKImageComparison.Compare(opaqueBlack, transparentBlack).MismatchedPixelCount);
		using var defaultOverlay = SKImageComparison.GenerateDifferenceImage(opaqueBlack, transparentBlack);
		AssertPixel(defaultOverlay, 0, new SKColor(255, 128, 128));

		using var first = CreateRawPremulBitmap(new SKColor(64, 0, 0, 64));
		using var second = CreateRawPremulBitmap(new SKColor(64, 0, 0, 128));
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		var options = new SKComparisonOptions { AlphaType = SKAlphaType.Premul };
		Assert.Equal(1, SKImageComparison.Compare(first, second, options).MismatchedPixelCount);
		Assert.Equal(1, SKImageComparison.Compare(firstPixmap, secondPixmap, options).MismatchedPixelCount);
		Assert.Equal(1, SKImageComparison.Compare(a, b, options).MismatchedPixelCount);
		using var overlay = SKImageComparison.GenerateDifferenceImage(a, b, options: options);
		using var bitmapOverlay = SKImageComparison.GenerateDifferenceImage(first, second, options: options);
		using var pixmapOverlay = SKImageComparison.GenerateDifferenceImage(firstPixmap, secondPixmap, options: options);
		using var map = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.ThresholdMap, options);
		using var binary = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.BinaryMask, options);
		using var delta = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.ChannelDifference, options);
		foreach (var image in new[] { overlay, bitmapOverlay, pixmapOverlay })
			AssertPixel(image, 0, new SKColor(223, 64, 64));
		AssertPixel(map, 0, SKColors.Red);
		AssertPixel(binary, 0, SKColors.White);
		AssertPixel(delta, 0, SKColors.Black);
		options.RedTolerance = null;
		options.GreenTolerance = null;
		options.BlueTolerance = null;
		options.AlphaTolerance = SKPixelTolerance.Absolute(64);
		using var tolerated = SKImageComparison.GenerateDifferenceImage(first, second, options: options);
		using var toleratedMap = SKImageComparison.GenerateDifferenceImage(first, second, SKDifferenceStyle.ThresholdMap, options);
		AssertPixel(tolerated, 0, new SKColor(223, 164, 64));
		AssertPixel(toleratedMap, 0, new SKColor(255, 200, 0));
		Assert.Equal(0, SKImageComparison.Compare(a, b, options).MismatchedPixelCount);
	}

	[Fact]
	public void IgnoreMaskIsBorrowedAndDiagnosticsRemainIndependentAfterInputsAreDisposed()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(SKColors.White, new SKColor(3, 0, 0));
		using var ignoreBitmap = CreateBitmap(SKColors.White, SKColors.Black);
		using var ignoreMask = SKImage.FromBitmap(ignoreBitmap);
		using var toleranceBitmap = CreateBitmap(SKColors.Black, new SKColor(3, 0, 0));
		using var toleranceMask = SKImage.FromBitmap(toleranceBitmap);
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		var options = new SKComparisonOptions { IgnoreMask = ignoreMask, ToleranceMask = toleranceMask };
		var result = SKImageComparison.Compare(a, b, options);
		using var binary = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.BinaryMask, options);
		using var raw = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.ChannelDifference, options);
		using var overlay = SKImageComparison.GenerateDifferenceImage(a, b, options: options);
		using var map = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.ThresholdMap, options);
		using var heatmap = SKImageComparison.GenerateDifferenceImage(a, b, SKDifferenceStyle.Heatmap, options);
		AssertEquivalent(result, SKImageComparison.Compare(a, b, options));
		Assert.Same(ignoreMask, options.IgnoreMask);
		Assert.Same(toleranceMask, options.ToleranceMask);
		Assert.NotEqual(IntPtr.Zero, ignoreMask.Handle);
		Assert.NotEqual(IntPtr.Zero, toleranceMask.Handle);
		Assert.Equal(SKColors.White, ignoreBitmap.GetPixel(0, 0));
		Assert.Equal(new SKColor(3, 0, 0), toleranceBitmap.GetPixel(1, 0));
		Assert.Equal(SKColors.Black, first.GetPixel(0, 0));
		Assert.Equal(SKColors.White, second.GetPixel(0, 0));
		options.IgnoreMask = null;
		options.ToleranceMask = null;
		ignoreMask.Dispose();
		toleranceMask.Dispose();
		first.Dispose();
		second.Dispose();
		a.Dispose();
		b.Dispose();
		Assert.True(result.IsMatch);
		Assert.Equal(1, result.ComparedPixelCount);
		Assert.Equal(1, result.IgnoredPixelCount);
		Assert.Equal(3L, result.RawMetrics.SumAbsoluteError);
		AssertPixel(binary, 0, SKColors.Black);
		AssertPixel(binary, 1, SKColors.Black);
		AssertPixel(raw, 0, SKColors.Black);
		AssertPixel(raw, 1, new SKColor(3, 0, 0));
		AssertPixel(map, 0, new SKColor(128, 128, 128));
		AssertPixel(map, 1, new SKColor(255, 200, 0));
		AssertPixel(overlay, 0, new SKColor(192, 192, 192));
		AssertPixel(overlay, 1, new SKColor(129, 100, 0));
		AssertPixel(heatmap, 0, new SKColor(128, 128, 128));
		AssertPixel(heatmap, 1, new SKColor(0, 6, 255));
	}
}
