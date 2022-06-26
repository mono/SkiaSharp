using System;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;

namespace SkiaSharp.Benchmarks;

//[EtwProfiler]
//[NativeMemoryProfiler]
//[MemoryDiagnoser]
//[SimpleJob(RuntimeMoniker.Mono)]
//[SimpleJob(RuntimeMoniker.Net472)]
//[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net60)]
public class Benchmark
{
	private byte[] data;

	private Stream seekable;
	private Stream nonseekable;
	private Stream current;

	private int iterations = 100;

	[Params(1, 10)]
	public int SizeMB;

	[Params(true, false)]
	public bool IsSeekable;

	[GlobalSetup]
	public void GlobalSetup()
	{
		data = new byte[SizeMB * 1024 * 1024];
		var rnd = new Random();
		rnd.NextBytes(data);

		seekable = new MemoryStream(data);

		nonseekable = new NonSeekableReadOnlyStream(seekable);
	}

	[IterationSetup]
	public void IterationSetup()
	{
		current = IsSeekable ? seekable : nonseekable;

		iterations = 1000 / SizeMB;
	}

	[Benchmark(Baseline = true)]
	public void CreatePrevious()
	{
		for (var i = 0; i < iterations; i++)
		{
			seekable.Position = 0;
			var skdata = SKData.CreatePrevious(current);
			skdata.Dispose();
		}
	}

	[Benchmark]
	public void Create()
	{
		for (var i = 0; i < iterations; i++)
		{
			seekable.Position = 0;
			var skdata = SKData.Create(current);
			skdata.Dispose();
		}
	}

	[Benchmark]
	public void CreateUnregistered()
	{
		for (var i = 0; i < iterations; i++)
		{
			seekable.Position = 0;
			var skdata = SKData.CreateUnregistered(current);
			skdata.Dispose();
		}
	}
}

[SimpleJob(RuntimeMoniker.Net60)]
public class SkottieBenchmark
{
	private byte[] data;

	private Stream seekable;
	private Stream nonseekable;
	private Stream current;

	[Params(true, false)]
	public bool IsSeekable;

	[GlobalSetup]
	public void GlobalSetup()
	{
		data = File.ReadAllBytes("C:\\Projects\\SkiaSharp\\benchmarks\\SkiaSharp.Benchmarks\\LottieLogo1.json");

		seekable = new MemoryStream(data);

		nonseekable = new NonSeekableReadOnlyStream(seekable);
	}

	[IterationSetup]
	public void IterationSetup()
	{
		current = IsSeekable ? seekable : nonseekable;
		seekable.Position = 0;
	}

	[Benchmark(Baseline = true)]
	public void CreatePrevious()
	{
		var anim = Skottie.Animation.CreatePrevious(current);
		anim.Dispose();
	}

	[Benchmark]
	public void Create()
	{
		var anim = Skottie.Animation.Create(current);
		anim.Dispose();
	}

	[Benchmark]
	public void CreateUnregistered()
	{
		var anim = Skottie.Animation.CreateUnregistered(current);
		anim.Dispose();
	}
}

public class Program
{
	public static void Main(string[] args)
	{
		var summary = BenchmarkRunner.Run<Benchmark>();
	}
}
