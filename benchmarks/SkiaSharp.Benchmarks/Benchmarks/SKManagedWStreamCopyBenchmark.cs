using System;
using System.IO;
using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

[MemoryDiagnoser]
public unsafe class SKManagedWStreamCopyBenchmark
{
	[Params(4096, 2097152, 3145728)]
	public int Size { get; set; }

	private SKData data;
	private MemoryStream destination;
	private SKManagedWStream current;
	private BufferedManagedWStream buffered;

	[GlobalSetup]
	public void Setup()
	{
		var bytes = new byte[Size];
		new Random(42).NextBytes(bytes);
		data = SKData.CreateCopy(bytes);
		destination = new MemoryStream(Size);
		current = new SKManagedWStream(destination);
		buffered = new BufferedManagedWStream(destination);
		Old_Buffered();
		if (!destination.GetBuffer().AsSpan(0, Size).SequenceEqual(bytes))
			throw new InvalidOperationException("The buffered write produced different data.");
		Current_DirectSpan();
		if (!destination.GetBuffer().AsSpan(0, Size).SequenceEqual(bytes))
			throw new InvalidOperationException("The span write produced different data.");
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		buffered.Dispose();
		current.Dispose();
		destination.Dispose();
		data.Dispose();
	}

	[Benchmark(Baseline = true)]
	public long Old_Buffered()
	{
		destination.Position = 0;
		if (!SkiaApi.sk_wstream_write(buffered.Handle, (void*)data.Data, (IntPtr)Size))
			throw new IOException("The buffered write failed.");
		return destination.Position;
	}

	[Benchmark]
	public long Current_DirectSpan()
	{
		destination.Position = 0;
		if (!SkiaApi.sk_wstream_write(current.Handle, (void*)data.Data, (IntPtr)Size))
			throw new IOException("The span write failed.");
		return destination.Position;
	}

	private sealed class BufferedManagedWStream : SKAbstractManagedWStream
	{
		private readonly MemoryStream destination;

		public BufferedManagedWStream(MemoryStream destination)
		{
			this.destination = destination;
		}

		protected internal override bool OnWrite(IntPtr buffer, IntPtr size)
		{
			var count = (int)size;
			using var rented = Utils.RentArray<byte>(count);
			Marshal.Copy(buffer, rented.Array, 0, count);
			destination.Write(rented.Array, 0, count);
			return true;
		}

		protected internal override void OnFlush() => destination.Flush();

		protected internal override IntPtr OnBytesWritten() => (IntPtr)destination.Position;
	}
}
