using System;
using System.Runtime.InteropServices;
using System.Text;
using BenchmarkDotNet.Attributes;
using HarfBuzzSharp;

namespace SkiaSharp.Benchmarks;

// New vs Old for HarfBuzzSharp.Feature.ToString().
//
// Old = the current shipped implementation: it allocates a 128-byte native buffer with
//       Marshal.AllocHGlobal, fills it via hb_feature_to_string, marshals it back with
//       Marshal.PtrToStringAnsi, then frees it with Marshal.FreeHGlobal — TWO extra P/Invoke
//       transitions and a native-heap round-trip per call, on top of the one required native call.
// New = the proposed managed fast path: a stackalloc'd 128-byte buffer (no native-heap alloc,
//       no AllocHGlobal/FreeHGlobal transitions), decoded from the ASCII bytes HarfBuzz writes.
//
// The real caller shape is diagnostics / serialization of shaping features: logging a font's
// active OpenType features, round-tripping feature strings ("kern=1", "liga[3:5]=0", "aalt=2"),
// building UI or debug output — often a batch of many features at once.
[MemoryDiagnoser]
public unsafe class HarfBuzzFeatureToStringBenchmark
{
	private const int MaxFeatureStringSize = 128;

	private Feature[] features;

	[GlobalSetup]
	public void GlobalSetup()
	{
		features = new[]
		{
			new Feature (new Tag ('k', 'e', 'r', 'n'), 1, 0, uint.MaxValue),
			new Feature (new Tag ('l', 'i', 'g', 'a'), 0, 3, 5),
			new Feature (new Tag ('a', 'a', 'l', 't'), 2, 0, uint.MaxValue),
			new Feature (new Tag ('s', 'm', 'c', 'p'), 1, 0, uint.MaxValue),
			new Feature (new Tag ('o', 'n', 'u', 'm'), 1, 2, 10),
			new Feature (new Tag ('c', 'a', 'l', 't'), 0, 0, uint.MaxValue),
		};
	}

	[Benchmark (Baseline = true)]
	public int Old ()
	{
		var sink = 0;
		foreach (var f in features)
			sink += OldToString (f).Length;
		return sink;
	}

	[Benchmark]
	public int New ()
	{
		var sink = 0;
		foreach (var f in features)
			sink += f.ToString ().Length;
		return sink;
	}

	// Verbatim copy of the ORIGINAL shipped ToString() body, so the ratio is honest.
	private static string OldToString (Feature feature)
	{
		var f = &feature;
		var buffer = Marshal.AllocHGlobal (MaxFeatureStringSize);
		HarfBuzzApi.hb_feature_to_string (f, (void*)buffer, MaxFeatureStringSize);
		var str = Marshal.PtrToStringAnsi (buffer);
		Marshal.FreeHGlobal (buffer);
		return str;
	}
}
