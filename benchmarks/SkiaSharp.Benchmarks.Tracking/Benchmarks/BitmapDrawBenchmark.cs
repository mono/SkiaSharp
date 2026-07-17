using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// Reproduces the hot path from https://github.com/mono/SkiaSharp/issues/4421:
// software (CPU) rasterization of a *scaled* bitmap on an N32 + SrcOver surface.
// A scaled DrawBitmap needs a bitmap shader context; from Skia milestone 146
// (SK_ENABLE_LEGACY_SHADERCONTEXT removed) that falls back from the fast legacy
// SkARGB32_Shader_Blitter to the generic raster-pipeline blitter — 5x-20x slower,
// worst on Windows. PR #4428 re-enables the define.
//
// Two methods on purpose:
//   * DrawScaledTiles   — dst != src size => sampling shader => the regressing path.
//   * DrawUnscaledTiles — dst == src size, integer-aligned => fast copy => unaffected.
// The DrawScaledTiles/DrawUnscaledTiles ratio is a hardware-independent signal, so a
// platform hit by #4421 stands out even before cross-OS normalization exists.
public class BitmapDrawBenchmark
{
	[Params(64, 256)]
	public int Tiles { get; set; }

	private const int TileSize = 64;
	private const int SurfaceSize = 512;

	private SKSurface _surface = null!;
	private SKCanvas _canvas = null!;
	private SKBitmap _tile = null!;
	private SKPaint _paint = null!;

	[GlobalSetup]
	public void Setup()
	{
		// Platform-default (N32) surface — the colour type the regression affects.
		_surface = SKSurface.Create(new SKImageInfo(SurfaceSize, SurfaceSize));
		_canvas = _surface.Canvas;

		// A real (non-uniform) texture so the draw actually samples pixels.
		_tile = new SKBitmap(TileSize, TileSize);
		using (var tc = new SKCanvas(_tile))
		{
			tc.Clear(SKColors.White);
			using var fill = new SKPaint { Color = SKColors.CornflowerBlue, IsAntialias = true };
			tc.DrawCircle(32, 32, 28, fill);
			using var accent = new SKPaint { Color = SKColors.OrangeRed };
			tc.DrawRect(new SKRect(4, 4, 24, 24), accent);
		}

		_paint = new SKPaint(); // default: SrcOver, opaque draw
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		_paint.Dispose();
		_tile.Dispose();
		_surface.Dispose();
	}

	// #4421 path: scale the 64px tile to a non-integer size so sampling is required.
	[Benchmark]
	public int DrawScaledTiles()
	{
		_canvas.Clear(SKColors.White);
		const float size = 37.3f;
		const int cols = 8;
		for (var i = 0; i < Tiles; i++)
		{
			float x = (i % cols) * 40f;
			float y = ((i / cols) * 40f) % SurfaceSize;
			_canvas.DrawBitmap(_tile, new SKRect(x, y, x + size, y + size), _paint);
		}
		_canvas.Flush();
		return Tiles;
	}

	// Sibling fast path: 1:1, integer-aligned — not affected by the regression.
	[Benchmark]
	public int DrawUnscaledTiles()
	{
		_canvas.Clear(SKColors.White);
		const int cols = 8;
		for (var i = 0; i < Tiles; i++)
		{
			float x = (i % cols) * TileSize % SurfaceSize;
			float y = ((i / cols) * TileSize) % SurfaceSize;
			_canvas.DrawBitmap(_tile, new SKRect(x, y, x + TileSize, y + TileSize), _paint);
		}
		_canvas.Flush();
		return Tiles;
	}
}
