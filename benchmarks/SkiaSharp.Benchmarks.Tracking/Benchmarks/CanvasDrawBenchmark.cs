using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// Core raster drawing workload: clear + fill rects + stroke circles + stroke a path,
// repeated `Shapes` times onto a reused CPU surface. Exercises the managed wrapper +
// P/Invoke + native rasterizer, which is exactly where per-OS native differences show
// up (e.g. a platform that is unexpectedly slow at a particular draw call).
public class CanvasDrawBenchmark
{
	[Params(64, 512)]
	public int Shapes { get; set; }

	private SKSurface _surface = null!;
	private SKCanvas _canvas = null!;
	private SKPaint _fill = null!;
	private SKPaint _stroke = null!;
	private SKPath _path = null!;

	[GlobalSetup]
	public void Setup()
	{
		_surface = SKSurface.Create(new SKImageInfo(256, 256));
		_canvas = _surface.Canvas;
		_fill = new SKPaint { Color = SKColors.CornflowerBlue, IsAntialias = true, Style = SKPaintStyle.Fill };
		_stroke = new SKPaint { Color = SKColors.OrangeRed, IsAntialias = true, Style = SKPaintStyle.Stroke, StrokeWidth = 2 };

		// Version-appropriate path construction: SKPathBuilder on 4.x+, classic SKPath below.
#if SKIASHARP_4_OR_GREATER
		using (var builder = new SKPathBuilder())
		{
			builder.MoveTo(10, 10);
			builder.LineTo(200, 40);
			builder.LineTo(120, 220);
			builder.Close();
			_path = builder.Snapshot();
		}
#else
		_path = new SKPath();
		_path.MoveTo(10, 10);
		_path.LineTo(200, 40);
		_path.LineTo(120, 220);
		_path.Close();
#endif
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		_path.Dispose();
		_stroke.Dispose();
		_fill.Dispose();
		_surface.Dispose();
	}

	[Benchmark]
	public int Draw()
	{
		_canvas.Clear(SKColors.White);
		for (var i = 0; i < Shapes; i++)
		{
			float o = i % 32;
			_canvas.DrawRect(new SKRect(o, o, o + 50, o + 50), _fill);
			_canvas.DrawCircle(o + 25, o + 25, 18, _stroke);
			_canvas.DrawPath(_path, _stroke);
		}
		_canvas.Flush();
		return Shapes;
	}
}
