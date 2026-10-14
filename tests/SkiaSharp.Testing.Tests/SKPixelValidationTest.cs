using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Testing.Tests;

public class SKPixelValidationTest : SKPixelComparerTestBase
{
	[Fact]
	public void LibraryAssemblyMarksItsEntireApiExperimentalWithoutRequiringPreviewFeatures()
	{
		var attributes = typeof(SKPixelComparer).Assembly.GetCustomAttributesData();
		var experimental = Assert.Single(attributes,
			attribute => attribute.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.ExperimentalAttribute");
		Assert.Equal("SKIA4001", Assert.Single(experimental.ConstructorArguments).Value);
		Assert.Equal("https://github.com/mono/SkiaSharp/blob/main/documentation/dev/pixel-comparison.md#experimental-api-contract",
			Assert.Single(experimental.NamedArguments).TypedValue.Value);
		Assert.DoesNotContain(attributes,
			attribute => attribute.AttributeType.FullName == "System.Runtime.Versioning.RequiresPreviewFeaturesAttribute");
	}

	[Fact]
	public void PublicEntryPointsUseOptionalNullSettingsWithoutConvenienceOverloads()
	{
		var methods = typeof(SKPixelComparer).GetMethods(
			System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
		Assert.Equal(6, methods.Length);
		foreach (var method in methods)
		{
			var parameters = method.GetParameters();
			var options = parameters[parameters.Length - 1];
			Assert.Equal("options", options.Name);
			Assert.True(options.IsOptional);
			Assert.Null(options.DefaultValue);
			if (method.Name == nameof(SKPixelComparer.GenerateDifferenceImage))
			{
				Assert.Equal(4, parameters.Length);
				Assert.Equal("style", parameters[2].Name);
				Assert.True(parameters[2].IsOptional);
				Assert.Equal(SKPixelDifferenceStyle.ThresholdOverlay, parameters[2].DefaultValue);
			}
			else
			{
				Assert.Equal(nameof(SKPixelComparer.Compare), method.Name);
				Assert.Equal(3, parameters.Length);
			}
		}
	}

	[Fact]
	public void MeanAbsoluteErrorInvalidLimitsFailInEveryComparisonAndDiagnosticFamily()
	{
		using var bitmap = CreateBitmap(SKColors.Black);
		using var image = SKImage.FromBitmap(bitmap);
		using var pixmap = bitmap.PeekPixels();
		foreach (var invalid in new[] { -0.01, 1.01, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
		{
			var options = new SKPixelComparerOptions { MaxNormalizedMeanAbsoluteError = invalid };
			foreach (var operation in Operations(bitmap, bitmap, image, image, pixmap, pixmap, options))
			{
				var error = Assert.Throws<ArgumentOutOfRangeException>(operation);
				Assert.Equal("options", error.ParamName);
				Assert.Contains(nameof(SKPixelComparerOptions.MaxNormalizedMeanAbsoluteError), error.Message);
			}
		}
	}

	[Fact]
	public void AllIgnoredCoverageFailsExplicitlyInAllSixEntryPointsRegardlessOfGatesOrStyle()
	{
		using var first = CreateBitmap(SKColors.Black, SKColors.Black);
		using var second = CreateBitmap(SKColors.White, SKColors.White);
		using var a = SKImage.FromBitmap(first);
		using var b = SKImage.FromBitmap(second);
		using var firstPixmap = first.PeekPixels();
		using var secondPixmap = second.PeekPixels();
		using var maskBitmap = CreateBitmap(SKColors.White, new SKColor(0, 0, 1, 0));
		using var mask = SKImage.FromBitmap(maskBitmap);
		var options = new SKPixelComparerOptions { IgnoreMask = mask };
		foreach (var configuredGates in new[] { false, true })
		{
			if (configuredGates)
			{
				options.MismatchBudget = SKPixelMismatchBudget.Percent(100);
				options.MaxNormalizedMeanAbsoluteError = 1;
				options.MaxNormalizedRootMeanSquaredError = 1;
				SetUniformTolerance(options, SKPixelTolerance.Absolute(255));
			}
			foreach (var style in new[] { SKPixelDifferenceStyle.BinaryMask, SKPixelDifferenceStyle.ChannelDifference, SKPixelDifferenceStyle.ThresholdOverlay })
			foreach (var operation in Operations(first, second, a, b, firstPixmap, secondPixmap, options, style))
			{
				var error = Assert.Throws<ArgumentException>(operation);
				Assert.Equal(nameof(SKPixelComparerOptions.IgnoreMask), error.ParamName);
				Assert.Contains("at least one pixel", error.Message);
			}
		}
		Assert.NotEqual(IntPtr.Zero, mask.Handle);
	}

	[Fact]
	public void IgnoreMaskSizeAndDisposedInputsFailInAllSixEntryPoints()
	{
		using var bitmap = CreateBitmap(SKColors.Black, SKColors.Black);
		using var image = SKImage.FromBitmap(bitmap);
		using var pixmap = bitmap.PeekPixels();
		using var otherSize = CreateBitmap(SKColors.Black);
		using var mismatchedMask = SKImage.FromBitmap(otherSize);
		var options = new SKPixelComparerOptions { IgnoreMask = mismatchedMask };
		foreach (var operation in Operations(bitmap, bitmap, image, image, pixmap, pixmap, options))
			Assert.Throws<InvalidOperationException>(operation);
		using var disposedMask = SKImage.FromBitmap(bitmap);
		disposedMask.Dispose();
		options.IgnoreMask = disposedMask;
		foreach (var operation in Operations(bitmap, bitmap, image, image, pixmap, pixmap, options))
			Assert.Equal(nameof(SKPixelComparerOptions.IgnoreMask), Assert.Throws<ArgumentException>(operation).ParamName);
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
		var allDisabled = new SKPixelComparerOptions
		{
			RedTolerance = null,
			GreenTolerance = null,
			BlueTolerance = null,
			AlphaTolerance = null,
		};
		Assert.Equal("options", Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(bitmap, bitmap, allDisabled)).ParamName);
		Assert.Equal("options", Assert.Throws<ArgumentException>(() =>
			SKPixelComparer.GenerateDifferenceImage(image, image, options: allDisabled)).ParamName);
		Assert.Equal(4, SKPixelComparer.Compare(bitmap, bitmap, null).ChannelCount);
		Assert.Equal(4, SKPixelComparer.Compare(bitmap, bitmap, new SKPixelComparerOptions()).ChannelCount);
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare((SKImage)null!, image));
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare(image, (SKImage)null!));
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare((SKBitmap)null!, bitmap));
		Assert.Throws<ArgumentNullException>(() => SKPixelComparer.Compare((SKPixmap)null!, pixmap));
		Assert.Throws<InvalidOperationException>(() => SKPixelComparer.Compare(image, mismatch));
		Assert.Throws<InvalidOperationException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { ToleranceMask = mismatch }));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(bitmap, emptyBitmap));
		Assert.Throws<ArgumentException>(() => SKPixelComparer.Compare(pixmap, emptyPixmap));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.GenerateDifferenceImage(image, image, (SKPixelDifferenceStyle)99, null));
		foreach (var invalid in new[] { SKAlphaType.Opaque, SKAlphaType.Unknown })
			Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.Compare(image, image, new SKPixelComparerOptions { AlphaType = invalid }));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKPixelComparer.GenerateDifferenceImage(image, image, SKPixelDifferenceStyle.BinaryMask, new SKPixelComparerOptions { AlphaType = SKAlphaType.Opaque }));
		foreach (var invalid in new[] { -0.01, 1.01, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
		{
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
}
