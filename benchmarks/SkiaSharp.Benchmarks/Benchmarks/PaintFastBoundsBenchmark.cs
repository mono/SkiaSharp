using System;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// SKPaint.GetFastBounds exists to power quick-reject culling: before issuing an expensive
// draw, a renderer can ask the paint for the conservative device-space rect the draw would touch
// - composing the path effect, stroke, mask filter and image filter - and skip the draw entirely
// when that rect misses the clip/viewport.
//
// This benchmark models a frame that submits many blurred rects scattered across a virtual space
// four times larger than the viewport, so roughly three quarters of the items land off-screen and
// are cullable. It compares drawing every item unconditionally against using GetFastBounds to
// cull off-screen items first, plus an isolated measurement of the raw GetFastBounds call cost
// (the P/Invoke crossing plus the native geometry math).
//
// Runs on the project's current TFM with the default job, matching SurfaceCanvasBenchmark.
[MemoryDiagnoser]
public class PaintFastBoundsBenchmark
{
	private const int Width = 256;
	private const int Height = 256;

	// Frames rendered per invocation. 60 ~= one second at 60 FPS.
	[Params(1, 60)]
	public int Frames { get; set; }

	// Draw items submitted per frame.
	[Params(500)]
	public int ItemsPerFrame { get; set; }

	private SKSurface surface;
	private SKCanvas canvas;
	private SKPaint paint;
	private SKMaskFilter blur;
	private SKRect clip;
	private SKRect[] items;

	[GlobalSetup]
	public void GlobalSetup()
	{
		surface = SKSurface.Create(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul));
		canvas = surface.Canvas;
		blur = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f);
		paint = new SKPaint { IsAntialias = true, Color = SKColors.SteelBlue, MaskFilter = blur };

		// The on-screen viewport that draws must intersect to be visible.
		clip = SKRect.Create(0, 0, Width, Height);

		// Scatter items across a 4x larger virtual space so ~75% land off-screen and are
		// cullable. A fixed seed keeps the scene identical across runs.
		items = new SKRect[ItemsPerFrame];
		var rnd = new Random(42);
		for (var i = 0; i < items.Length; i++)
		{
			var x = rnd.Next(-Width, Width * 3);
			var y = rnd.Next(-Height, Height * 3);
			items[i] = SKRect.Create(x, y, 24, 24);
		}
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		paint?.Dispose();
		blur?.Dispose();
		surface?.Dispose();
	}

	// Baseline: draw every blurred item, letting the raster backend reject off-screen draws.
	[Benchmark(Baseline = true)]
	public void DrawAll()
	{
		for (var f = 0; f < Frames; f++)
		{
			foreach (var item in items)
				canvas.DrawRect(item, paint);
		}
	}

	// Optimized: use GetFastBounds to skip items whose drawn bounds miss the viewport.
	[Benchmark]
	public void DrawWithFastBoundsCulling()
	{
		for (var f = 0; f < Frames; f++)
		{
			foreach (var item in items)
			{
				paint.GetFastBounds(item, out var bounds);
				if (bounds.IntersectsWith(clip))
					canvas.DrawRect(item, paint);
			}
		}
	}

	// Isolates the raw GetFastBounds cost (P/Invoke crossing plus native geometry).
	[Benchmark]
	public float GetFastBoundsOnly()
	{
		var sum = 0f;
		for (var f = 0; f < Frames; f++)
		{
			foreach (var item in items)
			{
				paint.GetFastBounds(item, out var bounds);
				sum += bounds.Width;
			}
		}
		return sum;
	}
}
