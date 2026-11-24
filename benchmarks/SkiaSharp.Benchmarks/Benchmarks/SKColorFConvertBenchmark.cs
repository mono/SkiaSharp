using System;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// The implicit SKColor -> SKColorF conversion turns a packed 8-bit BGRA color into four normalized
// floats. It sits on hot paths for anyone feeding colors into the float pipeline: setting an
// SKRuntimeEffect color uniform (SKRuntimeEffect.cs converts SKColor -> SKColorF via this implicit
// operator) does it every frame for animated shaders, and gradient/paint code converts batches of
// colors. Because it is an implicit operator it is trivially hit inside per-frame / per-item loops.
//
// The shipped implementation used to round-trip through the native sk_color4f_from_color P/Invoke
// (an extra managed->native transition, plus an output-pointer pin, per single color). This
// benchmark compares that previous native path (Old, baseline) against the current managed port
// (New) which is a plain `byte * (1f/255f)` per channel - proven bit-identical to the native result
// by SKColorFConvertEquivalenceTest.
//
// Both variants convert the same representative batch of colors so the per-call difference is
// amortized over a realistic loop rather than lost in harness overhead.
[MemoryDiagnoser]
public unsafe class SKColorFConvertBenchmark
{
	private SKColor[] colors;

	[Params(1024)]
	public int Count { get; set; }

	[GlobalSetup]
	public void Setup()
	{
		colors = new SKColor[Count];
		// Spread across the full 32-bit color space so no channel is constant-folded.
		var seed = 0x9E3779B9u;
		for (var i = 0; i < colors.Length; i++)
		{
			seed = seed * 1664525u + 1013904223u;
			colors[i] = (SKColor)seed;
		}
	}

	// New: the shipped managed conversion (implicit operator, pure managed math).
	[Benchmark]
	public float New()
	{
		var sink = 0f;
		var local = colors;
		for (var i = 0; i < local.Length; i++)
		{
			SKColorF f = local[i];
			sink += f.Red + f.Green + f.Blue + f.Alpha;
		}
		return sink;
	}

	// Baseline: the previous implementation that round-tripped through the native P/Invoke.
	[Benchmark(Baseline = true)]
	public float Old()
	{
		var sink = 0f;
		var local = colors;
		for (var i = 0; i < local.Length; i++)
		{
			SKColorF f;
			SkiaApi.sk_color4f_from_color((uint)local[i], &f);
			sink += f.Red + f.Green + f.Blue + f.Alpha;
		}
		return sink;
	}
}
