using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// Isolates the CPU raster-pipeline image-sampling slowdown from
// https://github.com/mono/SkiaSharp/issues/4421 (PR #4428 re-enables the fix).
//
// Since Skia milestone 146 removed the default SK_ENABLE_LEGACY_SHADERCONTEXT define,
// SkShaderBase::makeContext() returns nullptr for every shader, so SkBlitter::Choose,
// for an N32 + SrcOver draw that carries a shader (a *scaled* image draw builds a
// sampling shader), falls back from the fast hand-tuned SkARGB32_Shader_Blitter to the
// generic raster-pipeline blitter. The per-pixel sampler stage chain (matrix -> tile ->
// gather -> premul -> srcover) is 5x-10x slower than the legacy SIMD kernel.
//
// The sibling BitmapDrawBenchmark issues many TINY (37px) scaled draws, so its
// scaled/unscaled ratio is diluted: the per-draw fixed cost (Choose + clip + the
// once-per-draw raster-pipeline compile in SkRasterPipelineBlitter::blitRect) is a
// common addend paid by BOTH the legacy and raster-pipeline paths, compressing the
// ratio toward 1. This benchmark instead issues a SMALL number of LARGE full-surface
// upscaled draws, so the per-pixel steady state dominates and the true per-pixel ratio
// shows through (matching the reporter's full-screen 60fps cross-fade repro).
//
// Two methods on purpose (never rename — the full name is the history key):
//   * UpscaleOpaque    — one full-surface upscaled opaque image draw. Isolates pure
//                        sampling. No Clear: an opaque full-surface draw overwrites.
//   * UpscaleCrossfade — an opaque base draw plus a second image at alpha 128, both
//                        timed (no Clear). This is the reporter's real cross-fade frame;
//                        (UpscaleCrossfade - UpscaleOpaque) isolates the extra sampling
//                        and alpha blend of the second layer.
// Larger Size makes each dst pixel more of the total work, so a platform hit by #4421
// should show the ratio climb with Size.
public class LargeImageScaleBenchmark
{
	// Surface/destination edge length. At least one LARGE size so per-pixel cost
	// dominates the per-draw setup that dilutes the tiny-tile benchmark.
	[Params(1024, 2048)]
	public int Size { get; set; }

	// Small source, upscaled to fill the surface. 300 does not divide 1024/2048, so the
	// scale is non-integer (nearest gather per dst pixel), matching the reporter who
	// upscales smaller bitmaps to a large, differently-sized canvas.
	private const int SourceSize = 300;

	// Paint alpha for the cross-fade's second (fading-in) layer: ~50%.
	private const byte CrossfadeAlpha = 128;

	private SKSurface _surface = null!;
	private SKCanvas _canvas = null!;
	private SKImage _imageA = null!;
	private SKImage _imageB = null!;
	private SKPaint _opaquePaint = null!;
	private SKPaint _alphaPaint = null!;
	private SKRect _source;
	private SKRect _dest;

	[GlobalSetup]
	public void Setup()
	{
		// Platform-default (N32) surface — the colour type the regression affects.
		_surface = SKSurface.Create(new SKImageInfo(Size, Size));
		_canvas = _surface.Canvas;

		// Two deterministic, non-uniform source images so sampling reads varied texels
		// and the cross-fade blends genuinely different content.
		_imageA = CreateSource(SKColors.CornflowerBlue, SKColors.OrangeRed);
		_imageB = CreateSource(SKColors.SeaGreen, SKColors.Gold);

		_opaquePaint = new SKPaint(); // default: SrcOver, opaque
		_alphaPaint = new SKPaint { Color = SKColors.Black.WithAlpha(CrossfadeAlpha) };

		_source = new SKRect(0, 0, SourceSize, SourceSize);
		_dest = new SKRect(0, 0, Size, Size);
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		_alphaPaint.Dispose();
		_opaquePaint.Dispose();
		_imageB.Dispose();
		_imageA.Dispose();
		_surface.Dispose();
	}

	// A busy, non-uniform texture (no flat regions that could shortcut sampling).
	private static SKImage CreateSource(SKColor a, SKColor b)
	{
		using var surface = SKSurface.Create(new SKImageInfo(SourceSize, SourceSize));
		var canvas = surface.Canvas;
		canvas.Clear(SKColors.White);
		using var fill = new SKPaint { IsAntialias = false };
		for (var y = 0; y < SourceSize; y += 20)
		{
			for (var x = 0; x < SourceSize; x += 20)
			{
				fill.Color = ((x + y) / 20 % 2 == 0) ? a : b;
				canvas.DrawRect(new SKRect(x, y, x + 14, y + 14), fill);
			}
		}
		using var line = new SKPaint { Color = SKColors.Black, IsAntialias = false, Style = SKPaintStyle.Stroke, StrokeWidth = 1 };
		for (var i = 0; i < SourceSize; i += 37)
			canvas.DrawLine(0, i, SourceSize, SourceSize - i, line);
		return surface.Snapshot();
	}

	// #4421 path: a single large upscaled N32 SrcOver image draw => sampling shader =>
	// raster-pipeline blitter fallback. Opaque + full surface, so no Clear is needed.
	[Benchmark]
	public int UpscaleOpaque()
	{
		_canvas.DrawImage(_imageA, _source, _dest, SKSamplingOptions.Default, _opaquePaint);
		_canvas.Flush();
		return Size;
	}

	// The reporter's real frame: an opaque base image plus a fading-in image at alpha
	// 128. Two large upscaled draws; both timed, no Clear (the opaque base overwrites).
	[Benchmark]
	public int UpscaleCrossfade()
	{
		_canvas.DrawImage(_imageA, _source, _dest, SKSamplingOptions.Default, _opaquePaint);
		_canvas.DrawImage(_imageB, _source, _dest, SKSamplingOptions.Default, _alphaPaint);
		_canvas.Flush();
		return Size;
	}
}
