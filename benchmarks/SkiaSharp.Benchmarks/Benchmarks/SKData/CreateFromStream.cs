using System;
using System.Buffers;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;

namespace SkiaSharp.Benchmarks;

[EtwProfiler]
//[NativeMemoryProfiler]
[MemoryDiagnoser]
//[SimpleJob(RuntimeMoniker.Mono)]
[SimpleJob(RuntimeMoniker.Net472)]
[SimpleJob(RuntimeMoniker.NetCoreApp31)]
[SimpleJob(RuntimeMoniker.Net60)]
public class SKDataCreateFromStream
{
	private byte[] data;

	private Stream seekable;
	private Stream nonseekable;
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

	[IterationSetup]
	public void IterationSetup()
	{
		seekable.Position = 0;
	}

	[Benchmark]
	public void Seekable()
	{
		var skdata = SKData.Create(seekable);
		skdata.Dispose();
	}

	[Benchmark]
	public void NonSeekable()
	{
		var skdata = SKData.Create(nonseekable);
		skdata.Dispose();
	}

	[Benchmark]
	public unsafe void PInvoke()
	{
		fixed (byte* ptr = buffer)
		{
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
