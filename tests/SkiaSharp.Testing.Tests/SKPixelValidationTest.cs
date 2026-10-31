using System;
using SkiaSharp.Testing;
using Xunit;

namespace SkiaSharp.Testing.Tests;

public class SKPixelValidationTest : SKImageComparisonTestBase
{
	[Fact]
	public void LibraryAssemblyMarksItsEntireApiExperimentalWithoutRequiringPreviewFeatures()
	{
		var attributes = typeof(SKImageComparison).Assembly.GetCustomAttributesData();
		var experimental = Assert.Single(attributes,
			attribute => attribute.AttributeType.FullName == "System.Diagnostics.CodeAnalysis.ExperimentalAttribute");
		Assert.Equal("SKIA4001", Assert.Single(experimental.ConstructorArguments).Value);
		Assert.DoesNotContain(attributes,
			attribute => attribute.AttributeType.FullName == "System.Runtime.Versioning.RequiresPreviewFeaturesAttribute");
	}

	[Fact]
	public void PublicEntryPointsUseOptionalNullSettingsWithoutConvenienceOverloads()
	{
		var methods = typeof(SKImageComparison).GetMethods(
			System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
		Assert.Equal(6, methods.Length);
		foreach (var method in methods)
		{
			var parameters = method.GetParameters();
			var options = parameters[parameters.Length - 1];
			Assert.Equal("options", options.Name);
			Assert.True(options.IsOptional);
			Assert.Null(options.DefaultValue);
			if (method.Name == nameof(SKImageComparison.GenerateDifferenceImage))
			{
				Assert.Equal(4, parameters.Length);
				Assert.Equal("style", parameters[2].Name);
				Assert.True(parameters[2].IsOptional);
				Assert.Equal(SKDifferenceStyle.ThresholdOverlay, parameters[2].DefaultValue);
			}
			else
			{
				Assert.Equal(nameof(SKImageComparison.Compare), method.Name);
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
			var options = new SKComparisonOptions { MaxNormalizedMeanAbsoluteError = invalid };
			foreach (var operation in Operations(bitmap, bitmap, image, image, pixmap, pixmap, options))
			{
				var error = Assert.Throws<ArgumentOutOfRangeException>(operation);
				Assert.Equal("options", error.ParamName);
				Assert.Contains(nameof(SKComparisonOptions.MaxNormalizedMeanAbsoluteError), error.Message);
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
		var options = new SKComparisonOptions { IgnoreMask = mask };
		foreach (var configuredGates in new[] { false, true })
		{
			if (configuredGates)
			{
				options.MismatchBudget = SKPixelMismatchBudget.Percent(100);
				options.MaxNormalizedMeanAbsoluteError = 1;
				options.MaxNormalizedRootMeanSquaredError = 1;
				SetUniformTolerance(options, SKPixelTolerance.Absolute(255));
			}
			foreach (SKDifferenceStyle style in Enum.GetValues(typeof(SKDifferenceStyle)))
			foreach (var operation in Operations(first, second, a, b, firstPixmap, secondPixmap, options, style))
			{
				var error = Assert.Throws<ArgumentException>(operation);
				Assert.Equal(nameof(SKComparisonOptions.IgnoreMask), error.ParamName);
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
		var options = new SKComparisonOptions { IgnoreMask = mismatchedMask };
		foreach (var operation in Operations(bitmap, bitmap, image, image, pixmap, pixmap, options))
			Assert.Throws<InvalidOperationException>(operation);
		using var disposedMask = SKImage.FromBitmap(bitmap);
		disposedMask.Dispose();
		options.IgnoreMask = disposedMask;
		foreach (var operation in Operations(bitmap, bitmap, image, image, pixmap, pixmap, options))
			Assert.Equal(nameof(SKComparisonOptions.IgnoreMask), Assert.Throws<ArgumentException>(operation).ParamName);
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
		var allDisabled = new SKComparisonOptions
		{
			RedTolerance = null,
			GreenTolerance = null,
			BlueTolerance = null,
			AlphaTolerance = null,
		};
		Assert.Equal("options", Assert.Throws<ArgumentException>(() => SKImageComparison.Compare(bitmap, bitmap, allDisabled)).ParamName);
		Assert.Equal("options", Assert.Throws<ArgumentException>(() =>
			SKImageComparison.GenerateDifferenceImage(image, image, options: allDisabled)).ParamName);
		Assert.Equal(4, SKImageComparison.Compare(bitmap, bitmap, null).ChannelCount);
		Assert.Equal(4, SKImageComparison.Compare(bitmap, bitmap, new SKComparisonOptions()).ChannelCount);
		Assert.Throws<ArgumentNullException>(() => SKImageComparison.Compare((SKImage)null!, image));
		Assert.Throws<ArgumentNullException>(() => SKImageComparison.Compare(image, (SKImage)null!));
		Assert.Throws<ArgumentNullException>(() => SKImageComparison.Compare((SKBitmap)null!, bitmap));
		Assert.Throws<ArgumentNullException>(() => SKImageComparison.Compare((SKPixmap)null!, pixmap));
		Assert.Throws<InvalidOperationException>(() => SKImageComparison.Compare(image, mismatch));
		Assert.Throws<InvalidOperationException>(() => SKImageComparison.Compare(image, image, new SKComparisonOptions { ToleranceMask = mismatch }));
		Assert.Throws<ArgumentException>(() => SKImageComparison.Compare(bitmap, emptyBitmap));
		Assert.Throws<ArgumentException>(() => SKImageComparison.Compare(pixmap, emptyPixmap));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKImageComparison.GenerateDifferenceImage(image, image, (SKDifferenceStyle)99, null));
		foreach (var invalid in new[] { SKAlphaType.Opaque, SKAlphaType.Unknown })
			Assert.Throws<ArgumentOutOfRangeException>(() => SKImageComparison.Compare(image, image, new SKComparisonOptions { AlphaType = invalid }));
		Assert.Throws<ArgumentOutOfRangeException>(() => SKImageComparison.GenerateDifferenceImage(image, image, SKDifferenceStyle.BinaryMask, new SKComparisonOptions { AlphaType = SKAlphaType.Opaque }));
		foreach (var invalid in new[] { -0.01, 1.01, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
		{
			Assert.Throws<ArgumentOutOfRangeException>(() => SKImageComparison.Compare(image, image, new SKComparisonOptions { MaxNormalizedRootMeanSquaredError = invalid }));
		}
		using var disposedMask = SKImage.FromBitmap(bitmap);
		disposedMask.Dispose();
		Assert.Throws<ArgumentException>(() => SKImageComparison.Compare(image, image, new SKComparisonOptions { ToleranceMask = disposedMask }));
		image.Dispose();
		bitmap.Dispose();
		pixmap.Dispose();
		Assert.Throws<ArgumentException>(() => SKImageComparison.Compare(image, image));
		Assert.Throws<ArgumentException>(() => SKImageComparison.Compare(bitmap, bitmap));
		Assert.Throws<ArgumentException>(() => SKImageComparison.Compare(pixmap, pixmap));
	}
}
