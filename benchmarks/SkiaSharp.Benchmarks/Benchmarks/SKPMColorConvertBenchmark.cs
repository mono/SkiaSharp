using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// SKPMColor.PreMultiply(SKColor) / UnPreMultiply(SKPMColor) convert a single colour to and from
// premultiplied form. Callers that composite or convert pixels one colour at a time (gradient
// ramps, palette expansion, per-pixel software blends) hit these in a tight loop, so the per-call
// cost dominates.
//
// The shipped implementation used to round-trip every single conversion through a native P/Invoke
// (SkiaApi.sk_color_premultiply / sk_color_unpremultiply) even though the work is a few integer
// ops. New is the managed port (integer-only, bit-exact with the native result — proven by
// SKPMColorEquivalenceTest); Old is the previous native call, reproduced verbatim as the baseline
// so both are measured in the same process/TFM.
//
// [Params] covers a small palette (256) and a 64x64 pixel tile (4096); alpha varies across the
// batch so the a != 255 premultiply branch is exercised realistically.
[MemoryDiagnoser]
public class SKPMColorPreMultiplyBenchmark
{
	[Params(256, 4096)]
	public int N { get; set; }

	private SKColor[] colors;

	[GlobalSetup]
	public void GlobalSetup()
	{
		colors = new SKColor[N];
		for (var i = 0; i < N; i++)
		{
			// Spread alpha and channels so most colours take the a != 255 (real premultiply) path.
			var a = (byte)((i * 7) & 0xFF);
			var r = (byte)((i * 13) & 0xFF);
			var g = (byte)((i * 29) & 0xFF);
			var b = (byte)((i * 53) & 0xFF);
			colors[i] = new SKColor(r, g, b, a);
		}
	}

	// New: the shipped managed integer implementation.
	[Benchmark]
	public uint New()
	{
		uint sink = 0;
		var src = colors;
		for (var i = 0; i < src.Length; i++)
			sink ^= (uint)SKPMColor.PreMultiply(src[i]);
		return sink;
	}

	// Baseline: the previous path — one native P/Invoke per colour.
	[Benchmark(Baseline = true)]
	public uint Old()
	{
		uint sink = 0;
		var src = colors;
		for (var i = 0; i < src.Length; i++)
			sink ^= SkiaApi.sk_color_premultiply((uint)src[i]);
		return sink;
	}
}

[MemoryDiagnoser]
public class SKPMColorUnPreMultiplyBenchmark
{
	[Params(256, 4096)]
	public int N { get; set; }

	private SKPMColor[] pmcolors;

	[GlobalSetup]
	public void GlobalSetup()
	{
		pmcolors = new SKPMColor[N];
		for (var i = 0; i < N; i++)
		{
			var a = (byte)((i * 7) & 0xFF);
			var r = (byte)((i * 13) & 0xFF);
			var g = (byte)((i * 29) & 0xFF);
			var b = (byte)((i * 53) & 0xFF);
			// Build a valid premultiplied colour to unpremultiply back.
			pmcolors[i] = SKPMColor.PreMultiply(new SKColor(r, g, b, a));
		}
	}

	// New: the shipped managed integer implementation.
	[Benchmark]
	public uint New()
	{
		uint sink = 0;
		var src = pmcolors;
		for (var i = 0; i < src.Length; i++)
			sink ^= (uint)(SKColor)SKPMColor.UnPreMultiply(src[i]);
		return sink;
	}

	// Baseline: the previous path — one native P/Invoke per colour.
	[Benchmark(Baseline = true)]
	public uint Old()
	{
		uint sink = 0;
		var src = pmcolors;
		for (var i = 0; i < src.Length; i++)
			sink ^= SkiaApi.sk_color_unpremultiply((uint)src[i]);
		return sink;
	}
}
