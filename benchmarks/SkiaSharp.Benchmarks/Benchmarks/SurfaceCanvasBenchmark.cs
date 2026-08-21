using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Simulates a render loop where each frame issues 1,000 draw items, and every item
// fetches SKSurface.Canvas. Every Canvas getter does a P/Invoke (sk_surface_get_canvas)
// plus a locked HandleDictionary lookup and an OwnedBy registration, so a draw-heavy
// frame hammers that path 1,000 times. This compares re-fetching the canvas on every
// item against fetching it once per frame and reusing the reference.
//
// Unlike a trivial DrawRect loop, each item is a single anti-aliased primitive under a
// transform (rounded rect / oval / star, cycled by index) so the frame is a varied scene
// costing a few ms rather than ~0.4 ms. Heavier features (gradient shaders, path
// stroking, mask blur) are intentionally avoided — any one of them stacked across 1,000
// items pushes the raster backend past the 16 ms/60 FPS budget, which is not a
// representative scene and would entirely mask the getter cost under noise.
//
// Runs on the project's current TFM with the default job. The benchmark project targets
// a single TFM, so cross-runtime [SimpleJob] attributes are intentionally omitted (they
// would fail to build the BenchmarkDotNet boilerplate).
[MemoryDiagnoser]
public class SurfaceCanvasBenchmark
{
	private const int Width = 512;
	private const int Height = 512;

	// Draw items per frame — each one fetches SKSurface.Canvas in the naive path.
	private const int CallsPerFrame = 1_000;

	// Frames rendered per benchmark invocation. 60 ~= one second at 60 FPS.
	[Params(1, 60)]
	public int Frames { get; set; }

	private SKSurface surface;
	private SKPaint fillPaint;
	private SKPaint accentPaint;
	private SKPath star;

	[GlobalSetup]
	public void GlobalSetup()
	{
		surface = SKSurface.Create(new SKImageInfo(Width, Height, SKColorType.Rgba8888, SKAlphaType.Premul));

		// Anti-aliased fills — non-trivial but far cheaper than gradient shaders or
		// path stroking, which alone push a 1,000-item frame past the 16 ms budget.
		fillPaint = new SKPaint { Color = SKColors.SteelBlue, IsAntialias = true };
		accentPaint = new SKPaint { Color = SKColors.Orange, IsAntialias = true };

		star = CreateStar(18f);
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		star?.Dispose();
		accentPaint?.Dispose();
		fillPaint?.Dispose();
		surface?.Dispose();
	}

	// Baseline: re-fetch SKSurface.Canvas on every draw item, as naive render loops do.
	[Benchmark(Baseline = true)]
	public void FetchCanvasEachDraw()
	{
		for (var frame = 0; frame < Frames; frame++)
		{
			for (var i = 0; i < CallsPerFrame; i++)
			{
				var canvas = surface.Canvas;
				DrawItem(canvas, i);
			}
		}
	}

	// Optimized: fetch the canvas once per frame and reuse it across the frame's items.
	[Benchmark]
	public void CacheCanvasPerFrame()
	{
		for (var frame = 0; frame < Frames; frame++)
		{
			var canvas = surface.Canvas;
			for (var i = 0; i < CallsPerFrame; i++)
			{
				DrawItem(canvas, i);
			}
		}
	}

	// Isolates the Canvas getter overhead: 1,000 fetches per frame with no drawing.
	[Benchmark]
	public SKCanvas FetchCanvasNoDraw()
	{
		SKCanvas canvas = null;
		for (var frame = 0; frame < Frames; frame++)
		{
			for (var i = 0; i < CallsPerFrame; i++)
			{
				canvas = surface.Canvas;
			}
		}
		return canvas;
	}

	// One draw item == one Canvas fetch (1,000 per frame). Each item is a single
	// non-trivial primitive: a transformed, anti-aliased, gradient-filled rounded rect.
	// Items cycle through a few primitive types by index so the frame is a varied scene
	// rather than one repeated shape, and land at a few ms/frame — representative without
	// the 60+ ms cost of stacking many anti-aliased ops per item.
	private void DrawItem(SKCanvas canvas, int i)
	{
		var x = (i * 37) % (Width - 48);
		var y = (i * 53) % (Height - 48);

		canvas.Save();
		canvas.Translate(x, y);
		canvas.RotateDegrees(i % 90);

		switch (i % 3)
		{
			case 0:
				canvas.DrawRoundRect(0, 0, 40, 40, 6, 6, fillPaint);
				break;
			case 1:
				canvas.DrawOval(new SKRect(0, 0, 40, 28), accentPaint);
				break;
			default:
				canvas.DrawPath(star, fillPaint);
				break;
		}

		canvas.Restore();
	}

	private static SKPath CreateStar(float radius)
	{
		var builder = new SKPathBuilder();
		const int points = 5;
		for (var k = 0; k < points * 2; k++)
		{
			var r = (k % 2 == 0) ? radius : radius / 2.4f;
			var angle = k * System.MathF.PI / points;
			var px = 20 + r * System.MathF.Sin(angle);
			var py = 20 - r * System.MathF.Cos(angle);
			if (k == 0)
				builder.MoveTo(px, py);
			else
				builder.LineTo(px, py);
		}
		builder.Close();
		return builder.Detach();
	}
}
