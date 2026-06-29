using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Measures CPU raster blur of 8888 (color) content via SKImageFilter.CreateBlur, the
// exact path affected by the SK_AVOID_SLOW_RASTER_PIPELINE_BLURS native build flag.
//
// Skia's Raster8888BlurAlgorithm uses a slow per-pixel raster-pipeline GaussianPass for
// small sigmas (< 2.0) and a fast ThreeBoxApproxPass for larger sigmas. The native flag
// routes the small-sigma case to the fast approximation too, so this benchmark is split
// across both regimes:
//
//   Sigma = 1.0  -> small-sigma path (changes with the flag)
//   Sigma = 4.0  -> large-sigma path (already fast; acts as a control)
//
// The blur is applied to an image draw (8888 surface) rather than a mask filter, because
// the flag only gates the 8888 GaussianPass and leaves the A8 mask path untouched. This
// is a pure-CPU raster benchmark (no GPU) so it is comparable across all platforms and
// across the SkiaSharp versions the CI workflow benchmarks.
[MemoryDiagnoser]
public class BlurImageFilterBenchmark
{
	[Params(256, 1024)]
	public int Size { get; set; }

	// 1.0 exercises the slow small-sigma raster-pipeline path; 4.0 is the fast control.
	[Params(1.0f, 4.0f)]
	public float Sigma { get; set; }

	private SKSurface surface;
	private SKImage source;
	private SKPaint paint;

	[GlobalSetup]
	public void GlobalSetup()
	{
		var info = new SKImageInfo(Size, Size, SKColorType.Rgba8888, SKAlphaType.Premul);
		surface = SKSurface.Create(info);

		source = CreateSource(info);

		paint = new SKPaint
		{
			IsAntialias = true,
			ImageFilter = SKImageFilter.CreateBlur(Sigma, Sigma),
		};
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		paint?.Dispose();
		source?.Dispose();
		surface?.Dispose();
	}

	[Benchmark]
	public void BlurImage()
	{
		var canvas = surface.Canvas;
		canvas.Clear(SKColors.White);
		// The simple DrawImage overload exists in every compared version; the newer
		// SKSamplingOptions overload does not, so its obsoletion warning is suppressed.
#pragma warning disable CS0618
		canvas.DrawImage(source, 0, 0, paint);
#pragma warning restore CS0618
		canvas.Flush();
	}

	// A varied 8888 scene so the blur has real color content to process.
	private static SKImage CreateSource(SKImageInfo info)
	{
		using var temp = SKSurface.Create(info);
		var canvas = temp.Canvas;
		canvas.Clear(SKColors.CornflowerBlue);

		using var fill = new SKPaint { IsAntialias = true };
		var rng = new System.Random(42);
		var step = System.Math.Max(8, info.Width / 16);
		for (var y = 0; y < info.Height; y += step)
		{
			for (var x = 0; x < info.Width; x += step)
			{
				fill.Color = new SKColor(
					(byte)rng.Next(256),
					(byte)rng.Next(256),
					(byte)rng.Next(256),
					255);
				canvas.DrawRect(x, y, step * 0.8f, step * 0.8f, fill);
			}
		}

		canvas.Flush();
		return temp.Snapshot();
	}
}
