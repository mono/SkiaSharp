using System.Reflection;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains.InProcess.Emit;

namespace SkiaSharp.Benchmarks.Tracking;

public static class Program
{
	public static void Main(string[] args)
	{
		// Same MemoryDiagnoser + full JSON export as the package-mode project, so the
		// tracker records identical metrics for the "pr" column.
		//
		// InProcess toolchain (vs the default out-of-process one) runs the benchmarks in
		// this process instead of generating and building a child project. That keeps the
		// source build — which references SkiaSharp from this checkout plus native binaries
		// from output/native — reliable in CI, where regenerating a child build against a
		// source ProjectReference is fragile. It trades a little isolation for robustness,
		// which is the right call for a relative "this branch vs baselines" signal.
		var config = DefaultConfig.Instance
			.AddDiagnoser(MemoryDiagnoser.Default)
			.AddExporter(JsonExporter.Full)
			.AddJob(Job.Default.WithToolchain(InProcessEmitToolchain.Instance));

		BenchmarkSwitcher
			.FromAssembly(Assembly.GetExecutingAssembly())
			.Run(args, config);
	}
}
