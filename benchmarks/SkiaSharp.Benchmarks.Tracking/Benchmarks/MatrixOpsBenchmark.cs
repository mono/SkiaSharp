using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// SKMatrix "algebra" operations that produce another matrix (as opposed to the geometry
// mapping ops in MatrixMapPointsBenchmark): Concat (pre/post multiply) and Invert. Each
// crosses into native per call today; PR #4241 ports the affine path to managed, so the
// removed P/Invoke shows here. The operands are affine (rotation+scale, translate), which
// is #4241's managed fast path. Deterministic and allocation-free (SKMatrix is a struct).
public class MatrixOpsBenchmark
{
	[Params(4096)]
	public int Count { get; set; }

	private SKMatrix _a;
	private SKMatrix _b;

	[GlobalSetup]
	public void Setup()
	{
		// A non-trivial affine matrix (rotation composed with a non-uniform scale) and a
		// translation — both stay on #4241's managed fast path.
		_a = SKMatrix.CreateRotationDegrees(30f, 100f, 100f).PreConcat(SKMatrix.CreateScale(1.5f, 0.75f));
		_b = SKMatrix.CreateTranslation(37f, -19f);
	}

	// #4241: matrix multiply, both directions (pre- and post-concat) once per iteration.
	[Benchmark]
	public float Concat()
	{
		var sink = 0f;
		for (var i = 0; i < Count; i++)
		{
			var pre = _a.PreConcat(_b);
			var post = _a.PostConcat(_b);
			sink += pre.TransX - post.TransY;
		}
		return sink;
	}

	// #4241: matrix inverse (hit-testing / mapping back to local coordinates).
	[Benchmark]
	public float Invert()
	{
		var sink = 0f;
		for (var i = 0; i < Count; i++)
		{
			if (_a.TryInvert(out var inv))
				sink += inv.ScaleX + inv.TransX;
		}
		return sink;
	}
}
