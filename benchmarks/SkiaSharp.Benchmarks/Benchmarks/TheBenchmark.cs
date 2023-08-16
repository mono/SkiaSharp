using System;
using System.Buffers;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;

namespace SkiaSharp.Benchmarks;

// [EtwProfiler]
// [NativeMemoryProfiler]
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net472)]
[SimpleJob(RuntimeMoniker.Net70)]
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
