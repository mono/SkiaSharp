using System;
using System.Buffers;
using System.Drawing;
using System.IO;
using System.Reflection.Metadata;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Dia2Lib;

namespace SkiaSharp.Benchmarks;

//[EtwProfiler]
//[NativeMemoryProfiler]
[MemoryDiagnoser]
//[SimpleJob(RuntimeMoniker.Mono)]
//[SimpleJob(RuntimeMoniker.Net472)]
//[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net60)]
public class SkDataCreateBenchmark
{
	private byte[] data;

	private Stream seekable;
	private Stream nonseekable;
	private Stream current;
	private int iterations;

	[Params(1, 10)]
	// [Params(10)]
	public int SizeMB;

	[Params(true, false)]
	// [Params(false)]
	public bool IsSeekable;

	[GlobalSetup]
	public void GlobalSetup()
	{
		data = new byte[SizeMB * 1024 * 1024];
		var rnd = new Random();
		rnd.NextBytes(data);

		seekable = new MemoryStream(data);

		nonseekable = new NonSeekableReadOnlyStream(seekable);

		IsSeekable = !IsSeekable;
		IterationSetup();
		CreatePrevious();
		Create();
		CreateUnregistered();

		IsSeekable = !IsSeekable;
		IterationSetup();
		CreatePrevious();
		Create();
		CreateUnregistered();
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

	[Benchmark]
	public void CreateRaw()
	{
		var buffer = ArrayPool<byte>.Shared.Rent(SKData.CopyBufferSize);

		for (var i = 0; i < iterations; i++)
		{
			seekable.Position = 0;

			var memory = SkiaApi.sk_dynamicmemorywstream_new();

			int len;
			while ((len = current.Read(buffer, 0, buffer.Length)) > 0)
			{
				SkiaApi.sk_wstream_write(memory, buffer, (IntPtr)len);
			}
			SkiaApi.sk_wstream_flush(memory);

			var skdata = SkiaApi.sk_dynamicmemorywstream_detach_as_data(memory);

			SkiaApi.sk_dynamicmemorywstream_destroy(memory);

			SkiaApi.sk_data_unref(skdata);
		}

		ArrayPool<byte>.Shared.Return(buffer);
	}
}
