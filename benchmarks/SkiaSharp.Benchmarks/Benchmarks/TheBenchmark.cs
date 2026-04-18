using System;
using System.Buffers;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace SkiaSharp.Benchmarks;

// [EtwProfiler]        // Windows-only, requires BenchmarkDotNet.Diagnostics.Windows
// [NativeMemoryProfiler] // Windows-only, requires BenchmarkDotNet.Diagnostics.Windows
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.HostProcess)]
public class TheBenchmark
{
	[GlobalSetup]
	public void GlobalSetup()
	{

	}

	[Benchmark(Baseline = true)]
	public void TheBaseline()
	{

	}

	[Benchmark]
	public void TheNew()
	{

	}
}
