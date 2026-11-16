using System;
using System.IO;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Measures SKManagedStream read, SKManagedWStream write, and SKData.Create(Stream) paths.
//
// Compares:
//  - Before: renting full/bounded ArrayPool buffers + intermediate copy (Marshal.Copy / Span.CopyTo)
//  - After:  reading/writing Span<byte> / ReadOnlySpan<byte> directly over native memory, and
//            reading directly into SKData in SKData.Create(Stream, length) without a transient
//            SKManagedStream wrapper.
[MemoryDiagnoser]
public unsafe class SKManagedStreamBenchmark
{
	private const int CopyBufferSize = 81920;

	[Params(64, 4096, 262144, 4194304)]
	public int N { get; set; }

	private byte[] sourceBytes;
	private MemoryStream readStream;
	private MemoryStream writeStream;
	private IntPtr nativeScratch;

	[GlobalSetup]
	public void GlobalSetup()
	{
		sourceBytes = new byte[N];
		for (var i = 0; i < N; i++)
			sourceBytes[i] = (byte)(i * 31 + 7);

		readStream = new MemoryStream(sourceBytes, writable: false);
		writeStream = new MemoryStream(N);
		nativeScratch = Marshal.AllocHGlobal(N);
	}

	[GlobalCleanup]
	public void GlobalCleanup()
	{
		readStream?.Dispose();
		writeStream?.Dispose();
		if (nativeScratch != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(nativeScratch);
			nativeScratch = IntPtr.Zero;
		}
	}

	[Benchmark(Baseline = true)]
	public long Read_Before()
	{
		readStream.Position = 0;
		return (long)ReadBefore(readStream, nativeScratch, (IntPtr)N);
	}

	[Benchmark]
	public long Read_BoundedArrayPool()
	{
		readStream.Position = 0;
		return (long)ReadBoundedArrayPool(readStream, nativeScratch, (IntPtr)N);
	}

	[Benchmark]
	public long Read_DirectSpan()
	{
		readStream.Position = 0;
		return (long)ReadDirectSpan(readStream, nativeScratch, (IntPtr)N);
	}

	[Benchmark]
	public long Write_Before()
	{
		writeStream.Position = 0;
		WriteBefore(writeStream, nativeScratch, (IntPtr)N);
		return writeStream.Position;
	}

	[Benchmark]
	public long Write_BoundedArrayPool()
	{
		writeStream.Position = 0;
		WriteBoundedArrayPool(writeStream, nativeScratch, (IntPtr)N);
		return writeStream.Position;
	}

	[Benchmark]
	public long Write_DirectSpan()
	{
		writeStream.Position = 0;
		WriteDirectSpan(writeStream, nativeScratch, (IntPtr)N);
		return writeStream.Position;
	}

	[Benchmark]
	public long SKDataCreateStream_ViaManagedStream()
	{
		readStream.Position = 0;
		using var managed = new SKManagedStream(readStream);
		using var data = SKData.Create(managed, (ulong)N);
		return data.Size;
	}

	[Benchmark]
	public long SKDataCreateStream_DirectSpan()
	{
		readStream.Position = 0;
		using var data = SKData.Create(readStream, (ulong)N);
		return data.Size;
	}

	private static IntPtr ReadBefore(Stream stream, IntPtr buffer, IntPtr size)
	{
		using var managedBuffer = Utils.RentArray<byte>((int)size);
		var len = stream.Read(managedBuffer.Array, 0, managedBuffer.Length);
		if (buffer != IntPtr.Zero)
		{
			var src = managedBuffer.Span.Slice(0, len);
			var dst = buffer.AsSpan(managedBuffer.Length);
			src.CopyTo(dst);
		}
		return (IntPtr)len;
	}

	private static IntPtr ReadBoundedArrayPool(Stream stream, IntPtr buffer, IntPtr size)
	{
		var count = (long)size;
		if (count <= 0)
			return IntPtr.Zero;

		var bufferSize = (int)Math.Min(count, CopyBufferSize);
		using var managedBuffer = Utils.RentArray<byte>(bufferSize);

		long totalRead = 0;
		while (totalRead < count)
		{
			var toRead = (int)Math.Min(count - totalRead, managedBuffer.Length);
			var len = stream.Read(managedBuffer.Array, 0, toRead);
			if (len <= 0)
				break;

			if (buffer != IntPtr.Zero)
			{
				var src = managedBuffer.Span.Slice(0, len);
				var dst = new Span<byte>((byte*)buffer + totalRead, len);
				src.CopyTo(dst);
			}

			totalRead += len;
		}
		return (IntPtr)totalRead;
	}

	private static IntPtr ReadDirectSpan(Stream stream, IntPtr buffer, IntPtr size)
	{
		var count = (long)size;
		if (count <= 0)
			return IntPtr.Zero;

		long totalRead = 0;
		if (buffer != IntPtr.Zero)
		{
			while (totalRead < count)
			{
				var toRead = (int)Math.Min(count - totalRead, CopyBufferSize);
				var dst = new Span<byte>((byte*)buffer + totalRead, toRead);
				var len = stream.Read(dst);
				if (len <= 0)
					break;
				totalRead += len;
			}
		}
		else
		{
			var bufferSize = (int)Math.Min(count, CopyBufferSize);
			using var managedBuffer = Utils.RentArray<byte>(bufferSize);
			while (totalRead < count)
			{
				var toRead = (int)Math.Min(count - totalRead, managedBuffer.Length);
				var len = stream.Read(managedBuffer.Array, 0, toRead);
				if (len <= 0)
					break;
				totalRead += len;
			}
		}
		return (IntPtr)totalRead;
	}

	private static void WriteBefore(Stream stream, IntPtr buffer, IntPtr size)
	{
		var count = (int)size;
		using var managedBuffer = Utils.RentArray<byte>(count);
		if (buffer != IntPtr.Zero)
			Marshal.Copy(buffer, (byte[])managedBuffer, 0, count);
		stream.Write((byte[])managedBuffer, 0, count);
	}

	private static void WriteBoundedArrayPool(Stream stream, IntPtr buffer, IntPtr size)
	{
		var count = (long)size;
		if (count <= 0)
			return;

		var bufferSize = (int)Math.Min(count, CopyBufferSize);
		using var managedBuffer = Utils.RentArray<byte>(bufferSize);

		long totalWritten = 0;
		while (totalWritten < count)
		{
			var toWrite = (int)Math.Min(count - totalWritten, managedBuffer.Length);
			if (buffer != IntPtr.Zero)
				Marshal.Copy(buffer + (nint)totalWritten, (byte[])managedBuffer, 0, toWrite);
			stream.Write((byte[])managedBuffer, 0, toWrite);
			totalWritten += toWrite;
		}
	}

	private static void WriteDirectSpan(Stream stream, IntPtr buffer, IntPtr size)
	{
		var count = (long)size;
		if (count <= 0)
			return;

		var src = (byte*)buffer;
		for (var left = count; left > 0;)
		{
			var toWrite = (int)Math.Min(left, CopyBufferSize);
			stream.Write(new ReadOnlySpan<byte>(src, toWrite));
			left -= toWrite;
			src += toWrite;
		}
	}
}
