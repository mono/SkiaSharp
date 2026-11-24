using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// SKMatrix "mapping" operations — applying a matrix to geometry — through the public API.
// MapPoints is the batch (allocating) overload; the single-op MapPoint/MapRect/MapVector/
// MapRadius each cross managed->native once per call today, so removing that P/Invoke (PR
// #4241 ports the affine path to managed) shows up most starkly on these single-op methods
// — the batch amortises its one native call over all the points and dilutes the win.
// The matrix is affine (rotation), which is #4241's managed fast path (perspective would
// fall back to native). Deterministic (fixed seed). Every single-op method is allocation-
// free; MapPoints keeps its allocating batch behaviour so the returned array stays a signal.
public class MatrixMapPointsBenchmark
{
	[Params(256, 4096)]
	public int Points { get; set; }

	private SKPoint[] _source = null!;
	private SKRect[] _rects = null!;
	private SKMatrix _matrix;

	[GlobalSetup]
	public void Setup()
	{
		var rnd = new Random(42);
		_source = new SKPoint[Points];
		_rects = new SKRect[Points];
		for (var i = 0; i < Points; i++)
		{
			var x = (float)rnd.NextDouble() * 1024f;
			var y = (float)rnd.NextDouble() * 1024f;
			_source[i] = new SKPoint(x, y);
			_rects[i] = new SKRect(x, y, x + 40f, y + 25f);
		}

		_matrix = SKMatrix.CreateRotationDegrees(30f, 512f, 512f);
	}

	// Batch overload — one native call for the whole array; allocates the result array
	// (kept as a tracked allocation signal). Never rename: this is a shipped history key.
	[Benchmark]
	public int MapPoints()
	{
		var mapped = _matrix.MapPoints(_source);
		return mapped.Length;
	}

	// #4241: single-point map, once per source point (one P/Invoke per call today).
	[Benchmark]
	public float MapPoint()
	{
		var sink = 0f;
		foreach (var p in _source)
		{
			var m = _matrix.MapPoint(p.X, p.Y);
			sink += m.X - m.Y;
		}
		return sink;
	}

	// #4241: map a rectangle (bounds / clip transform) — a very common per-frame op.
	[Benchmark]
	public float MapRect()
	{
		var sink = 0f;
		foreach (var r in _rects)
		{
			var m = _matrix.MapRect(r);
			sink += m.Width - m.Height;
		}
		return sink;
	}

	// #4241: map a vector (direction / offset — ignores translation).
	[Benchmark]
	public float MapVector()
	{
		var sink = 0f;
		foreach (var p in _source)
		{
			var m = _matrix.MapVector(p.X, p.Y);
			sink += m.X - m.Y;
		}
		return sink;
	}

	// #4241: map a radius (e.g. a stroke width / blur radius under the transform).
	[Benchmark]
	public float MapRadius()
	{
		var sink = 0f;
		foreach (var p in _source)
			sink += _matrix.MapRadius(p.X);
		return sink;
	}
}
