using System;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

[MemoryDiagnoser]
public unsafe class SKStreamCopyBufferSizeBenchmark
{
	[Params(64 * 1024, 80 * 1024, 128 * 1024)]
	public int BufferSize { get; set; }

	[Params(2 * 1024 * 1024, 3 * 1024 * 1024)]
	public int Size { get; set; }

	// Exercise both copy mechanisms on the current runtime, not a different legacy runtime.
	[Params(false, true)]
	public bool UseArrayBuffer { get; set; }

	private MemoryStream source;
	private MemoryStream destination;
	private FileStream file;
	private string filename;
	private SKData data;

	[GlobalSetup]
	public void Setup()
	{
		var bytes = new byte[Size];
		new Random(42).NextBytes(bytes);
		source = new MemoryStream(bytes, writable: false);
		destination = new MemoryStream(Size);
		data = SKData.Create(Size);
		filename = Path.Combine(Path.GetTempPath(), $"skiasharp-buffer-{Guid.NewGuid():N}.bin");
		File.WriteAllBytes(filename, bytes);
		file = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read,
			bufferSize: 4096, FileOptions.SequentialScan);

		ReadMemory();
		if (!data.AsSpan().SequenceEqual(bytes))
			throw new InvalidOperationException("The memory read produced different data.");
		ReadFile();
		if (!data.AsSpan().SequenceEqual(bytes))
			throw new InvalidOperationException("The file read produced different data.");
		WriteMemory();
		if (!destination.GetBuffer().AsSpan(0, Size).SequenceEqual(bytes))
			throw new InvalidOperationException("The write produced different data.");
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		file.Dispose();
		File.Delete(filename);
		source.Dispose();
		destination.Dispose();
		data.Dispose();
	}

	[Benchmark]
	public long ReadMemory()
	{
		source.Position = 0;
		return Read(source);
	}

	[Benchmark]
	public long ReadFile()
	{
		file.Position = 0;
		return Read(file);
	}

	private long Read(Stream stream)
	{
		using var scratch = UseArrayBuffer ? Utils.RentArray<byte>(BufferSize) : default;
		var dst = (byte*)data.Data;
		var remaining = Size;
		while (remaining > 0)
		{
			var count = Math.Min(remaining, BufferSize);
			var read = UseArrayBuffer
				? stream.Read(scratch.Array, 0, count)
				: stream.Read(new Span<byte>(dst, count));
			if ((uint)read > (uint)count)
				throw new IOException("The stream returned an invalid byte count.");
			if (read == 0)
				throw new EndOfStreamException();
			if (UseArrayBuffer)
				scratch.Span.Slice(0, read).CopyTo(new Span<byte>(dst, read));
			dst += read;
			remaining -= read;
		}
		return Size - remaining;
	}

	[Benchmark]
	public long WriteMemory()
	{
		destination.Position = 0;
		using var scratch = UseArrayBuffer ? Utils.RentArray<byte>(BufferSize) : default;
		var src = (byte*)data.Data;
		for (var remaining = Size; remaining > 0;)
		{
			var count = Math.Min(remaining, BufferSize);
			var span = new ReadOnlySpan<byte>(src, count);
			if (UseArrayBuffer)
			{
				span.CopyTo(scratch.Span);
				destination.Write(scratch.Array, 0, count);
			}
			else
			{
				destination.Write(span);
			}
			src += count;
			remaining -= count;
		}
		return destination.Position;
	}
}
