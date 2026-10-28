using System;
using System.Buffers;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

// Compare the same buffering strategies through direct Stream reads or a forward-only SKManagedStream.
// MemoryDiagnoser reports managed allocations only; the baseline also allocates native blocks/data.
[MemoryDiagnoser]
public unsafe class SKDataNativeStreamFallbackBenchmark
{
	[Params(4096, 256 * 1024, 3 * 1024 * 1024, 100 * 1024 * 1024)]
	public int Size { get; set; }

	[Params(4096, int.MaxValue)]
	public int MaxRead { get; set; }

	[Params(false, true)]
	public bool ThroughSKStream { get; set; }

	private LimitedReadStream source;
	private SKManagedStream nativeStream;

	[GlobalSetup]
	public void Setup()
	{
		var bytes = new byte[Size];
		new Random(42).NextBytes(bytes);
		source = new LimitedReadStream(bytes, MaxRead);
		nativeStream = ThroughSKStream ? new SKManagedStream(source) : null;
		if (nativeStream != null && (nativeStream.HasLength || nativeStream.HasPosition))
			throw new InvalidOperationException("The benchmark must exercise the unknown-length fallback.");

		using var old = ThroughSKStream ? CreateNativeBlocks(nativeStream) : CreateNativeBlocks(source);
		source.Position = 0;
		using var candidate = ThroughSKStream ? CreatePooledCandidate(nativeStream) : SKData.Create(source);
		if (!old.AsSpan().SequenceEqual(bytes) || !candidate.AsSpan().SequenceEqual(bytes))
			throw new InvalidOperationException("The fallback implementations produced different data.");
	}

	[GlobalCleanup]
	public void Cleanup()
	{
		nativeStream?.Dispose();
		source.Dispose();
	}

	[Benchmark(Baseline = true)]
	public long Old_NativeBlocks()
	{
		source.Position = 0;
		using var data = ThroughSKStream ? CreateNativeBlocks(nativeStream) : CreateNativeBlocks(source);
		return data.Size;
	}

	[Benchmark]
	public long Candidate_PooledBuffers()
	{
		source.Position = 0;
		using var data = ThroughSKStream ? CreatePooledCandidate(nativeStream) : SKData.Create(source);
		return data.Size;
	}

	private static SKData CreateNativeBlocks(Stream stream)
	{
		// Original managed-stream implementation: the wrapper's CopyTo reads its underlying Stream directly.
		using var memory = new SKDynamicMemoryWStream();
		using (var managed = new SKManagedStream(stream))
		{
			managed.CopyTo(memory);
		}
		return memory.DetachAsData();
	}

	private static SKData CreateNativeBlocks(SKStream stream)
	{
		if (stream.HasLength && stream.HasPosition)
			throw new InvalidOperationException("Expected an unknown-length stream.");

		using var memory = new SKDynamicMemoryWStream();
		using var buffer = Utils.RentArray<byte>(SKData.CopyBufferSize);
		int read;
		while ((read = stream.Read(buffer.Array, buffer.Length)) > 0)
		{
			if (!memory.Write(buffer.Array, read))
				return null;
		}
		return memory.DetachAsData();
	}

	// Benchmark-only: SharedArrayPool staging regresses on large inputs; production keeps native blocks.
	private static SKData CreatePooledCandidate(SKStream stream)
	{
		if (stream.HasLength && stream.HasPosition)
			throw new InvalidOperationException("Expected an unknown-length stream.");

		using var chunks = new PooledByteBuffer(ArrayPool<byte>.Shared);
		while (true)
		{
			var buffer = chunks.GetBuffer();
			var filled = 0;
			fixed (byte* ptr = buffer.Array)
			{
				while (filled < buffer.Count)
				{
					var toRead = buffer.Count - filled;
					var read = stream.Read((IntPtr)(ptr + buffer.Offset + filled), toRead);
					if ((uint)read > (uint)toRead)
						throw new IOException("The stream returned an invalid byte count.");
					if (read == 0)
						break;
					filled += read;
				}
			}
			chunks.Advance(filled);
			if (IntPtr.Size == 4 && chunks.Length > uint.MaxValue)
				throw new ArgumentOutOfRangeException("length");
			if (filled < buffer.Count)
				break;
		}

		if (chunks.TryGetSingleBuffer(out var single))
			return SKData.CreateCopy(single.Array, (ulong)single.Count);

		var data = SKData.Create(chunks.Length);
		if (data == null)
			return null;
		try
		{
			var dst = (byte*)data.Data;
			foreach (var buffer in chunks)
			{
				buffer.AsSpan().CopyTo(new Span<byte>(dst, buffer.Count));
				dst += buffer.Count;
			}
			return data;
		}
		catch
		{
			data.Dispose();
			throw;
		}
	}

	private sealed class LimitedReadStream : MemoryStream
	{
		private readonly int maxRead;

		public LimitedReadStream(byte[] bytes, int maxRead)
			: base(bytes, writable: false)
		{
			this.maxRead = maxRead;
		}

		public override bool CanSeek => false;

		public override int Read(byte[] buffer, int offset, int count) =>
			base.Read(buffer, offset, Math.Min(count, maxRead));
	}
}
