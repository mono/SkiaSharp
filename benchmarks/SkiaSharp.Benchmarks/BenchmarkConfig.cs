using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Json;

namespace SkiaSharp.Benchmarks;

// Shared BenchmarkDotNet configuration for both the in-repo benchmark project and the
// cross-version comparison harness (SkiaSharp.Benchmarks.Compare).
//
// The benchmarks CI workflow runs each (operating system, SkiaSharp version) combination
// in its own process and merges the results afterwards with scripts/benchmarks/merge-benchmarks.py.
// That merge step reads BenchmarkDotNet's "*-report-full-compressed.json" export, which is
// not emitted by the default configuration and cannot be selected through the command line
// in this BenchmarkDotNet version, so it is added here in code to guarantee every run
// produces the JSON the workflow consumes.
public static class BenchmarkConfig
{
	public static IConfig Create() =>
		DefaultConfig.Instance.AddExporter(JsonExporter.FullCompressed);
}
