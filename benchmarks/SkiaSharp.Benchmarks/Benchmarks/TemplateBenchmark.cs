using System;
using System.Buffers;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// TEMPLATE — copy this file, rename the class, and replace the two method bodies.
//
// It is the starting point the `performance-fixer` skill expects (see benchmarks/README.md).
// It deliberately compiles and runs as-is so you can confirm the harness works before editing:
//
//   dotnet run -c Release --project benchmarks/SkiaSharp.Benchmarks -- --filter '*TemplateBenchmark*' --job short
//
// The house rules this template encodes (keep them when you adapt it):
//
//  * [MemoryDiagnoser]  — allocations matter as much as time; many wins are "went zero-alloc",
//    which shows in the Allocated column before it shows in Mean.
//  * New vs Old in ONE process — Old is [Benchmark(Baseline = true)] and holds the CURRENT
//    shipped behaviour (copy the current implementation verbatim inline, or call the current
//    native path via SkiaApi.sk_* — SkiaSharp.Benchmarks has InternalsVisibleTo, so internal
//    SkiaApi members are reachable). New is the proposed fast path. Same job/TFM ⇒ honest Ratio.
//  * Return a value from every [Benchmark] (a "sink") so the JIT cannot fold the work away and
//    report a meaningless ~0 ns. Feed inputs via [Params]/fields, not consts.
//  * Shape the workload like the REAL caller (per-frame / per-point batch / startup parse) and
//    use [Params] to cover sizes and shapes that take different branches.
//
// Do NOT add cross-runtime [SimpleJob(RuntimeMoniker.NetXX)] attributes: this project targets a
// single TFM ($(TFMCurrent)), the .NET Framework toolchain only runs on Windows (it silently
// skips elsewhere), and cross-runtime jobs need those runtimes installed. Use the default job for
// final numbers and `--job short` for a fast directional signal while iterating.
[MemoryDiagnoser]
public class TemplateBenchmark
{
	// Sizes/shapes to measure. Pick values a real caller actually hits.
	[Params(16, 256)]
	public int N { get; set; }

	private int[] source;

	[GlobalSetup]
	public void GlobalSetup()
	{
		source = new int[N];
		for (var i = 0; i < N; i++)
			source[i] = i;
	}

	// Old: the current/naive path. Baseline for the Ratio column.
	// This placeholder allocates a temporary array; replace it with the current shipped code.
	[Benchmark(Baseline = true)]
	public long Old()
	{
		var tmp = new int[source.Length];        // <-- the allocation the "New" path removes
		Array.Copy(source, tmp, source.Length);
		long sum = 0;
		foreach (var v in tmp)
			sum += v;
		return sum;                              // return a sink so the work is not folded away
	}

	// New: the proposed fast path. Should be faster and/or lower-allocation than Old.
	// This placeholder uses a pooled buffer instead of allocating; replace it with your fix.
	[Benchmark]
	public long New()
	{
		var tmp = ArrayPool<int>.Shared.Rent(source.Length);
		try
		{
			source.AsSpan().CopyTo(tmp);
			long sum = 0;
			for (var i = 0; i < source.Length; i++)
				sum += tmp[i];
			return sum;
		}
		finally
		{
			ArrayPool<int>.Shared.Return(tmp);
		}
	}
}
