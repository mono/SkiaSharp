using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// Maps a batch of points through an SKMatrix via the public allocating overload.
// This is a common per-frame transform pattern; the returned array makes the
// allocation a first-class tracked signal alongside the transform time.
public class MatrixMapPointsBenchmark
{
	[Params(256, 4096)]
	public int Points { get; set; }

	private SKPoint[] _source = null!;
	private SKMatrix _matrix;

	[GlobalSetup]
	public void Setup()
	{
		var rnd = new Random(42);
		_source = new SKPoint[Points];
		for (var i = 0; i < Points; i++)
			_source[i] = new SKPoint((float)rnd.NextDouble() * 1024f, (float)rnd.NextDouble() * 1024f);

		_matrix = SKMatrix.CreateRotationDegrees(30f, 512f, 512f);
	}

	[Benchmark]
	public int MapPoints()
	{
		var mapped = _matrix.MapPoints(_source);
		return mapped.Length;
	}
}
