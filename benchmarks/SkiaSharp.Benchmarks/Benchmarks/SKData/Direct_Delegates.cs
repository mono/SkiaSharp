using System;
using System.Buffers;
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
public class SKData_Direct_Delegates
{
	private byte[] data;

	private Stream seekable;
	private Stream nonseekable;
	private int iterations;
	private byte[] buffer;

	[Params(1, 100, 1_000, 10_000)]
	public int SizeKB;

	[GlobalSetup]
	public void GlobalSetup()
	{
		data = new byte[SizeKB * 1024];
		var rnd = new Random();
		rnd.NextBytes(data);

		seekable = new MemoryStream(data);
		nonseekable = new NonSeekableReadOnlyStream(seekable);

		iterations = 1; // 1000 / SizeKB;

		buffer = ArrayPool<byte>.Shared.Rent(SKData.CopyBufferSize);

		// init skiasharp static things
		SKObject.GetInstance<SKObject>(IntPtr.Zero, out _);
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		nonseekable.Dispose();
		seekable.Dispose();

		data = null;

		ArrayPool<byte>.Shared.Return(buffer);
	}

	[Benchmark]
	public void Seekable()
	{
		for (var i = 0; i < iterations; i++)
		{
			seekable.Position = 0;
			var skdata = SKData.Create(seekable);
			skdata.Dispose();
		}
	}

	[Benchmark]
	public void NonSeekable()
	{
		for (var i = 0; i < iterations; i++)
		{
			seekable.Position = 0;
			var skdata = SKData.Create(nonseekable);
			skdata.Dispose();
		}
	}

	[Benchmark]
	public unsafe void PInvoke()
	{
		fixed (byte* ptr = buffer)
		{
			for (var i = 0; i < iterations; i++)
			{
				seekable.Position = 0;

				var memory = SkiaApi.sk_dynamicmemorywstream_new();

				int len;
				while ((len = nonseekable.Read(buffer, 0, buffer.Length)) > 0)
				{
					SkiaApi.sk_wstream_write(memory, ptr, (IntPtr)len);
				}
				SkiaApi.sk_wstream_flush(memory);

				var skdata = SkiaApi.sk_dynamicmemorywstream_detach_as_data(memory);

				SkiaApi.sk_dynamicmemorywstream_destroy(memory);

				SkiaApi.sk_data_unref(skdata);
			}
		}
	}
}
