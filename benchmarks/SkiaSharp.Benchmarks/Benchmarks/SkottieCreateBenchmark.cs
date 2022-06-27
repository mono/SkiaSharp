using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace SkiaSharp.Benchmarks;

//[EtwProfiler]
//[NativeMemoryProfiler]
[MemoryDiagnoser]
//[SimpleJob(RuntimeMoniker.Mono)]
//[SimpleJob(RuntimeMoniker.Net472)]
//[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net60)]
public class SkottieCreateBenchmark
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
