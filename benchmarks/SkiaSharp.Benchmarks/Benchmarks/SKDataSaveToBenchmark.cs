using System;
using System.IO;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Measures SKData.SaveTo(Stream).
//
// Old: the current shipped path — copy native memory into a pooled managed buffer with
//      Marshal.Copy, then write that buffer to the stream (two copies per chunk).
// New: on span-capable TFMs, write a ReadOnlySpan over the native memory straight to the
//      stream (one copy per chunk, no intermediate buffer / Marshal.Copy).
//
// Workload models the real caller: SKImage.Encode(...).SaveTo(memoryStream) — encoded image
// bytes flushed into an in-memory stream. MemoryStream is used so the benchmark measures the
// managed copy overhead (a FileStream would be dominated by I/O and hide the win).
[MemoryDiagnoser]
public class SKDataSaveToBenchmark
{
	private const int CopyBufferSize = 81920;

	// Encoded-image-sized payloads that span the single-chunk and multi-chunk paths.
	[Params(4096, 262144, 4194304)]
	public int N { get; set; }

	private SKData data;
	private MemoryStream stream;

	[GlobalSetup]
	public void GlobalSetup()
	{
		var bytes = new byte[N];
		for (var i = 0; i < N; i++)
			bytes[i] = (byte)(i * 31 + 7);
		data = SKData.CreateCopy(bytes);
		stream = new MemoryStream(N);
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		data?.Dispose();
		stream?.Dispose();
	}

	[Benchmark(Baseline = true)]
	public long Old()
	{
		stream.Position = 0;
		OldSaveTo(data, stream);
		return stream.Position;
	}

	[Benchmark]
	public long New()
	{
		stream.Position = 0;
		NewSaveTo(data, stream);
		return stream.Position;
	}

	// Verbatim copy of the current shipped SKData.SaveTo implementation.
	private static unsafe void OldSaveTo(SKData d, Stream target)
	{
		var ptr = d.Data;
		var total = d.Size;
		using var buffer = Utils.RentArray<byte>(CopyBufferSize);
		for (var left = total; left > 0;)
		{
			var copyCount = (int)Math.Min(CopyBufferSize, left);
			Marshal.Copy(ptr, (byte[])buffer, 0, copyCount);
			left -= copyCount;
			ptr += copyCount;
			target.Write((byte[])buffer, 0, copyCount);
		}
		GC.KeepAlive(d);
	}

	// Proposed fast path: write native memory directly, no intermediate managed buffer.
	private static unsafe void NewSaveTo(SKData d, Stream target)
	{
		var ptr = (byte*)d.Data;
		var total = d.Size;
		for (var left = total; left > 0;)
		{
			var copyCount = (int)Math.Min(CopyBufferSize, left);
			target.Write(new ReadOnlySpan<byte>(ptr, copyCount));
			left -= copyCount;
			ptr += copyCount;
		}
		GC.KeepAlive(d);
	}
}
