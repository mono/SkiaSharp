using System;
using System.Buffers;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;

namespace SkiaSharp.Benchmarks;

[EtwProfiler]
// [NativeMemoryProfiler]
[MemoryDiagnoser]
// [SimpleJob(RuntimeMoniker.Mono)]
// [SimpleJob(RuntimeMoniker.Net472)]
// [SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net60)]
public class Benchmark
{
	[GlobalSetup]
	public void GlobalSetup()
	{

	}

	[Benchmark]
	public void TheBenchmark()
	{

	}
}
