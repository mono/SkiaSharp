#nullable enable

using System;
using BenchmarkDotNet.Attributes;
using SkiaSharp.Testing;

namespace SkiaSharp.Benchmarks;

[MemoryDiagnoser]
public class PixelMismatchBudgetBenchmark
{
	private SKPixelMismatchBudget budget;

	[Params(0.3, 1e-8, 99.99999999999999)]
	public double Percent { get; set; }

	[GlobalSetup]
	public void GlobalSetup() => budget = SKPixelMismatchBudget.Percent(Percent);

	[Benchmark]
	public bool AllowsPercent() => budget.Allows(3, 1000);
}

public enum PixelComparerScenario
{
	Icon32Rgb,
	Golden256Rgba,
	Document800x600Rgb,
	Screenshot1920x1080Rgba,
}

[MemoryDiagnoser]
public class PixelComparerComparisonBenchmark
{
	[Params(PixelComparerScenario.Icon32Rgb, PixelComparerScenario.Golden256Rgba,
		PixelComparerScenario.Document800x600Rgb, PixelComparerScenario.Screenshot1920x1080Rgba)]
	public PixelComparerScenario Scenario { get; set; }

	private PixelComparerFixture fixture = null!;

	[GlobalSetup]
	public void GlobalSetup() => fixture = new PixelComparerFixture(Scenario);

	[GlobalCleanup]
	public void GlobalCleanup() => fixture.Dispose();

	[Benchmark]
	public long CompareImage() =>
		SKImageComparison.Compare(fixture.ExpectedImage, fixture.ActualImage, fixture.Options).RawMetrics.SumSquaredError;

	[Benchmark]
	public long CompareIdenticalImage() =>
		SKImageComparison.Compare(fixture.ExpectedImage, fixture.ExpectedImage, fixture.Options).RawMetrics.SumSquaredError;

	[Benchmark]
	public long CompareBitmap() =>
		SKImageComparison.Compare(fixture.ExpectedBitmap, fixture.ActualBitmap, fixture.Options).RawMetrics.SumSquaredError;

	[Benchmark]
	public long ComparePixmap() =>
		SKImageComparison.Compare(fixture.ExpectedPixmap, fixture.ActualPixmap, fixture.Options).RawMetrics.SumSquaredError;

	[Benchmark]
	public long CompareWithMasks() =>
		SKImageComparison.Compare(fixture.ExpectedImage, fixture.ActualImage, fixture.MaskedOptions).RawMetrics.SumSquaredError;
}

[MemoryDiagnoser]
public class PixelComparerDiagnosticBenchmark
{
	[Params(PixelComparerScenario.Golden256Rgba, PixelComparerScenario.Document800x600Rgb,
		PixelComparerScenario.Screenshot1920x1080Rgba)]
	public PixelComparerScenario Scenario { get; set; }

	private PixelComparerFixture fixture = null!;

	[GlobalSetup]
	public void GlobalSetup() => fixture = new PixelComparerFixture(Scenario);

	[GlobalCleanup]
	public void GlobalCleanup() => fixture.Dispose();

	[Benchmark]
	public uint ThresholdMap()
	{
		using var image = SKImageComparison.GenerateDifferenceImage(fixture.ExpectedImage, fixture.ActualImage,
			SKDifferenceStyle.ThresholdMap, fixture.MaskedOptions);
		return Checksum(image);
	}

	[Benchmark]
	public uint ThresholdOverlay()
	{
		using var image = SKImageComparison.GenerateDifferenceImage(fixture.ExpectedImage, fixture.ActualImage,
			SKDifferenceStyle.ThresholdOverlay, fixture.MaskedOptions);
		return Checksum(image);
	}

	[Benchmark]
	public uint ChannelDifference()
	{
		using var image = SKImageComparison.GenerateDifferenceImage(fixture.ExpectedImage, fixture.ActualImage,
			SKDifferenceStyle.ChannelDifference, fixture.MaskedOptions);
		return Checksum(image);
	}

	[Benchmark]
	public uint BinaryMask()
	{
		using var image = SKImageComparison.GenerateDifferenceImage(fixture.ExpectedImage, fixture.ActualImage,
			SKDifferenceStyle.BinaryMask, fixture.MaskedOptions);
		return Checksum(image);
	}

	[Benchmark]
	public uint Heatmap()
	{
		using var image = SKImageComparison.GenerateDifferenceImage(fixture.ExpectedImage, fixture.ActualImage,
			SKDifferenceStyle.Heatmap, fixture.MaskedOptions);
		return Checksum(image);
	}

	private static uint Checksum(SKImage image)
	{
		using var pixmap = image.PeekPixels();
		var pixels = pixmap.GetPixelSpan<SKColor>();
		var width = image.Width;
		var row = image.Height / 2 * width;
		var checksum = 0u;
		for (var band = 0; band < 8; band++)
			checksum = unchecked(checksum * 31 + (uint)pixels[row + width * (band * 2 + 1) / 16]);
		return checksum;
	}
}

public sealed class PixelComparerFixture : IDisposable
{
	public PixelComparerFixture(PixelComparerScenario scenario)
	{
		var (width, height, compareAlpha) = scenario switch
		{
			PixelComparerScenario.Icon32Rgb => (32, 32, false),
			PixelComparerScenario.Golden256Rgba => (256, 256, true),
			PixelComparerScenario.Document800x600Rgb => (800, 600, false),
			PixelComparerScenario.Screenshot1920x1080Rgba => (1920, 1080, true),
			_ => throw new ArgumentOutOfRangeException(nameof(scenario)),
		};

		var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
		ExpectedBitmap = new SKBitmap(info);
		ActualBitmap = new SKBitmap(info);
		ExpectedPixmap = ExpectedBitmap.PeekPixels();
		ActualPixmap = ActualBitmap.PeekPixels();
		using var toleranceBitmap = new SKBitmap(info);
		using var ignoreBitmap = new SKBitmap(info);
		using var tolerancePixmap = toleranceBitmap.PeekPixels();
		using var ignorePixmap = ignoreBitmap.PeekPixels();
		var expected = ExpectedPixmap.GetPixelSpan<SKColor>();
		var actual = ActualPixmap.GetPixelSpan<SKColor>();
		var tolerance = tolerancePixmap.GetPixelSpan<SKColor>();
		var ignored = ignorePixmap.GetPixelSpan<SKColor>();

		for (var y = 0; y < height; y++)
		{
			for (var x = 0; x < width; x++)
			{
				var i = y * width + x;
				var band = x * 8 / width;
				var red = (byte)(32 + (x * 3 + y) % 128);
				var green = (byte)(48 + (x + y * 5) % 128);
				var blue = (byte)(24 + (x * 7 + y * 3) % 128);
				var alpha = (byte)(band == 5 ? 128 : 255);
				expected[i] = new SKColor(red, green, blue, alpha);
				actual[i] = band switch
				{
					3 => new SKColor((byte)(red + 2), green, blue, alpha),
					4 => new SKColor(red, green, (byte)(blue + 24), alpha),
					5 => new SKColor(red, green, blue, 192),
					6 => new SKColor(red, (byte)(green + 12), blue, alpha),
					7 => new SKColor((byte)(red + 48), green, blue, alpha),
					_ => expected[i],
				};
				// Nonzero alpha preserves the green allowance through image normalization.
				tolerance[i] = band == 6 ? new SKColor(0, 32, 0, 32) : new SKColor(0, 0, 0, 0);
				ignored[i] = band == 7 ? SKColors.White : SKColors.Black;
			}
		}

		ExpectedImage = SKImage.FromBitmap(ExpectedBitmap);
		ActualImage = SKImage.FromBitmap(ActualBitmap);
		ToleranceMask = SKImage.FromBitmap(toleranceBitmap);
		IgnoreMask = SKImage.FromBitmap(ignoreBitmap);
		Options = CreateOptions(compareAlpha);
		MaskedOptions = CreateOptions(compareAlpha);
		MaskedOptions.ToleranceMask = ToleranceMask;
		MaskedOptions.IgnoreMask = IgnoreMask;
	}

	public SKBitmap ExpectedBitmap { get; }
	public SKBitmap ActualBitmap { get; }
	public SKPixmap ExpectedPixmap { get; }
	public SKPixmap ActualPixmap { get; }
	public SKImage ExpectedImage { get; }
	public SKImage ActualImage { get; }
	public SKImage ToleranceMask { get; }
	public SKImage IgnoreMask { get; }
	public SKComparisonOptions Options { get; }
	public SKComparisonOptions MaskedOptions { get; }

	public void Dispose()
	{
		ExpectedImage.Dispose();
		ActualImage.Dispose();
		ToleranceMask.Dispose();
		IgnoreMask.Dispose();
		ExpectedPixmap.Dispose();
		ActualPixmap.Dispose();
		ExpectedBitmap.Dispose();
		ActualBitmap.Dispose();
	}

	private static SKComparisonOptions CreateOptions(bool compareAlpha) => new SKComparisonOptions
	{
		RedTolerance = SKPixelTolerance.Absolute(4),
		GreenTolerance = SKPixelTolerance.Absolute(4),
		BlueTolerance = SKPixelTolerance.Absolute(4),
		AlphaTolerance = compareAlpha ? SKPixelTolerance.Absolute(4) : null,
	};
}
