using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// Builds an SKPath from a fixed set of points and computes its tight bounds.
// Each invocation allocates a fresh SKPath (a real caller pattern) so both the
// time and the allocation of path construction + native bounds are tracked.
public class PathBoundsBenchmark
{
	[Params(64, 1024)]
	public int Points { get; set; }

	private SKPoint[] _points = null!;

	[GlobalSetup]
	public void Setup()
	{
		// Deterministic points so the workload is identical on every machine/run.
		var rnd = new Random(42);
		_points = new SKPoint[Points];
		for (var i = 0; i < Points; i++)
			_points[i] = new SKPoint((float)rnd.NextDouble() * 512f, (float)rnd.NextDouble() * 512f);
	}

	[Benchmark]
	public float TightBounds()
	{
		// Version-appropriate path construction: SKPathBuilder on 4.x+, classic SKPath below.
#if SKIASHARP_4_OR_GREATER
		using var builder = new SKPathBuilder();
		builder.MoveTo(_points[0]);
		for (var i = 1; i < _points.Length; i++)
			builder.LineTo(_points[i]);
		builder.Close();
		using var path = builder.Detach();
#else
		using var path = new SKPath();
		path.MoveTo(_points[0]);
		for (var i = 1; i < _points.Length; i++)
			path.LineTo(_points[i]);
		path.Close();
#endif
		var bounds = path.TightBounds;
		return bounds.Width + bounds.Height;
	}
}
