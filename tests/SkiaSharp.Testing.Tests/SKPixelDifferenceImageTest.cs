using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Testing.Tests;

public class SKPixelDifferenceImageTest : SKPixelComparerTestBase
{
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
		using var map = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ThresholdMap, options);
		using var raw = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ChannelDifference, options);
		using var strictMask = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.BinaryMask, null);
		using var bitmapMap = SKPixelComparer.GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.ThresholdMap);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		using var pixmapBinary = SKPixelComparer.GenerateDifferenceImage(firstPixmap, secondPixmap, SKPixelDifferenceStyle.BinaryMask, options);
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
		var options = new SKPixelComparerOptions
		{
			RedTolerance = SKPixelTolerance.Absolute(3),
			GreenTolerance = SKPixelTolerance.Absolute(5),
			BlueTolerance = SKPixelTolerance.Absolute(9),
			ToleranceMask = toleranceMask,
			IgnoreMask = ignoreMask,
		};
		var comparison = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(1, comparison.MismatchedPixelCount);
		Assert.Equal(3, comparison.ComparedPixelCount);
		Assert.Equal(1, comparison.IgnoredPixelCount);
		using var imageDefault = SKPixelComparer.GenerateDifferenceImage(a, b, options: options);
		using var imageOverlay = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ThresholdOverlay, options);
		using var bitmapDefault = SKPixelComparer.GenerateDifferenceImage(first, second, options: options);
		using var bitmapOverlay = SKPixelComparer.GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.ThresholdOverlay, options);
		using var pixmapDefault = SKPixelComparer.GenerateDifferenceImage(firstPixmap, secondPixmap, options: options);
		using var pixmapOverlay = SKPixelComparer.GenerateDifferenceImage(firstPixmap, secondPixmap, SKPixelDifferenceStyle.ThresholdOverlay, options);
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
		AssertEquivalent(comparison, SKPixelComparer.Compare(first, second, options));
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
		var options = new SKPixelComparerOptions
		{
			AlphaType = alphaType,
			RedTolerance = SKPixelTolerance.Absolute(3),
			IgnoreMask = ignoreMask,
		};
		var result = SKPixelComparer.Compare(first, second, options);
		Assert.Equal(1, result.MismatchedPixelCount);
		Assert.Equal(3, result.ComparedPixelCount);
		using var overlay = SKPixelComparer.GenerateDifferenceImage(first, second, options: options);
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
		var options = new SKPixelComparerOptions { GreenTolerance = null, BlueTolerance = null, AlphaTolerance = null };
		Assert.True(SKPixelComparer.Compare(first, second, options).IsMatch);
		using var overlay = SKPixelComparer.GenerateDifferenceImage(first, second, options: options);
		using var reversed = SKPixelComparer.GenerateDifferenceImage(second, first, options: options);
		using var map = SKPixelComparer.GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.ThresholdMap, options);
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
		using var overlay = SKPixelComparer.GenerateDifferenceImage(actual, actual,
			options: new SKPixelComparerOptions { AlphaType = alphaType });
		AssertPixel(overlay, 0, new SKColor(223, 191, 159));
	}

	[Fact]
	public void ThresholdMapDimsNormalizedActualWithoutFlatteningTransparency()
	{
		using var actual = CreateRawPremulBitmap(new SKColor(96, 64, 32, 128));
		using var map = SKPixelComparer.GenerateDifferenceImage(actual, actual, SKPixelDifferenceStyle.ThresholdMap,
			new SKPixelComparerOptions { AlphaType = SKAlphaType.Premul });
		AssertPixel(map, 0, new SKColor(24, 16, 8));
	}

	[Fact]
	public void AlphaOnlyDifferenceIsVisibleByDefaultInAllDecodedFamilies()
	{
		using var opaqueBlack = CreateBitmap(SKColors.Black);
		using var transparentBlack = CreateBitmap(SKColors.Transparent);
		Assert.Equal(1, SKPixelComparer.Compare(opaqueBlack, transparentBlack).MismatchedPixelCount);
		using var defaultOverlay = SKPixelComparer.GenerateDifferenceImage(opaqueBlack, transparentBlack);
		AssertPixel(defaultOverlay, 0, new SKColor(255, 128, 128));

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
		using var bitmapOverlay = SKPixelComparer.GenerateDifferenceImage(first, second, options: options);
		using var pixmapOverlay = SKPixelComparer.GenerateDifferenceImage(firstPixmap, secondPixmap, options: options);
		using var map = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ThresholdMap, options);
		using var binary = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.BinaryMask, options);
		using var delta = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ChannelDifference, options);
		foreach (var image in new[] { overlay, bitmapOverlay, pixmapOverlay })
			AssertPixel(image, 0, new SKColor(223, 64, 64));
		AssertPixel(map, 0, SKColors.Red);
		AssertPixel(binary, 0, SKColors.White);
		AssertPixel(delta, 0, SKColors.Black);
		options.RedTolerance = null;
		options.GreenTolerance = null;
		options.BlueTolerance = null;
		options.AlphaTolerance = SKPixelTolerance.Absolute(64);
		using var tolerated = SKPixelComparer.GenerateDifferenceImage(first, second, options: options);
		using var toleratedMap = SKPixelComparer.GenerateDifferenceImage(first, second, SKPixelDifferenceStyle.ThresholdMap, options);
		AssertPixel(tolerated, 0, new SKColor(223, 164, 64));
		AssertPixel(toleratedMap, 0, new SKColor(255, 200, 0));
		Assert.Equal(0, SKPixelComparer.Compare(a, b, options).MismatchedPixelCount);
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
		var options = new SKPixelComparerOptions { IgnoreMask = ignoreMask, ToleranceMask = toleranceMask };
		var result = SKPixelComparer.Compare(a, b, options);
		using var binary = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.BinaryMask, options);
		using var raw = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ChannelDifference, options);
		using var overlay = SKPixelComparer.GenerateDifferenceImage(a, b, options: options);
		using var map = SKPixelComparer.GenerateDifferenceImage(a, b, SKPixelDifferenceStyle.ThresholdMap, options);
		AssertEquivalent(result, SKPixelComparer.Compare(a, b, options));
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
	}
}
