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
		AssertPixel(overlay, 0, new SKColor(128, 128, 128));
		AssertPixel(overlay, 1, new SKColor(255, 200, 0));
	}
}
