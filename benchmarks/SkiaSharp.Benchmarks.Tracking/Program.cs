using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Running;

namespace SkiaSharp.Benchmarks.Tracking;

public static class Program
{
	public static void Main(string[] args)
	{
		// One central config for every benchmark in this assembly:
		//   * MemoryDiagnoser  -> allocated bytes + GC counts (the deterministic,
		//     machine-independent signal we gate hardest on).
		//   * JsonExporter.Full -> BenchmarkDotNet.Artifacts/results/*-report-full.json,
		//     which the tracker (.github/scripts/perf/benchmarks/track.py) parses for both
		//     Statistics.Mean (ns) and Memory.BytesAllocatedPerOperation.
		//
		// CLI args are still honoured and merged, e.g. `--filter *`, `--job short`,
		// or `--job dry` for a fast smoke test.
		var config = DefaultConfig.Instance
			.AddDiagnoser(MemoryDiagnoser.Default)
			.AddExporter(JsonExporter.Full);

		BenchmarkSwitcher
			.FromAssembly(Assembly.GetExecutingAssembly())
			.Run(args, config);
	}
}
