using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks.Tracking;

// Per-item colour conversions that live (mostly) in the managed wrapper — the kind of
// hot maths that per-frame / per-pixel loops hit through the public API:
//   * SKColor -> SKColorF   (ported to managed C# in #4370; hit by animated shader uniforms)
//   * SKPMColor.PreMultiply / UnPreMultiply (ported to managed int maths in #4385)
// All deterministic and allocation-free, so the Allocated column is a clean machine-
// independent signal, and the ToColorF/ToColor time ratio (reverse is still native, #4370
// left it so) flags a managed regression even on noisy shared CI runners.
public class ColorMathBenchmark
{
	[Params(4096)]
	public int Colors { get; set; }

	private SKColor[] _colors = null!;
	private SKColorF[] _colorFs = null!;
	private SKPMColor[] _pmColors = null!;

	[GlobalSetup]
	public void Setup ()
	{
		// Deterministic inputs (fixed seed) so the workload is identical everywhere. Alpha
		// is varied across the full 0..255 range so the premultiply maths is exercised.
		var rnd = new Random (42);
		_colors = new SKColor[Colors];
		_colorFs = new SKColorF[Colors];
		_pmColors = new SKPMColor[Colors];
		for (var i = 0; i < Colors; i++) {
			var c = new SKColor (
				(byte)rnd.Next (256), (byte)rnd.Next (256),
				(byte)rnd.Next (256), (byte)rnd.Next (256));
			_colors[i] = c;
			_colorFs[i] = c;                       // native reverse input, precomputed
			_pmColors[i] = SKPMColor.PreMultiply (c);
		}
	}

	// #4370: managed SKColor -> SKColorF. XOR into an int sink so the JIT cannot elide the
	// conversion; the returned sink keeps the result live.
	[Benchmark]
	public int ToColorF ()
	{
		var sink = 0;
		foreach (var c in _colors) {
			SKColorF f = c;
			sink ^= (int)(f.Red * 255f) ^ (int)(f.Green * 255f) ^ (int)(f.Blue * 255f);
		}
		return sink;
	}

	// Contrast sentinel: SKColorF -> SKColor is still native (#4370 deliberately left it),
	// so a managed regression in ToColorF surfaces as a diverging ToColorF/ToColor ratio.
	[Benchmark]
	public int ToColor ()
	{
		var sink = 0;
		foreach (var f in _colorFs)
			sink ^= (int)(uint)(SKColor)f;
		return sink;
	}

	// #4385: managed SKColor -> premultiplied SKPMColor.
	[Benchmark]
	public int PreMultiply ()
	{
		var sink = 0;
		foreach (var c in _colors)
			sink ^= (int)(uint)SKPMColor.PreMultiply (c);
		return sink;
	}

	// #4385: managed premultiplied -> straight SKColor (the 256-entry scale-table path).
	[Benchmark]
	public int UnPreMultiply ()
	{
		var sink = 0;
		foreach (var pm in _pmColors)
			sink ^= (int)(uint)SKPMColor.UnPreMultiply (pm);
		return sink;
	}
}
