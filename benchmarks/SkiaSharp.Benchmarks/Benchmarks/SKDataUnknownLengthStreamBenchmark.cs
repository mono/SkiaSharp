using System;
using System.Buffers;
using System.IO;
using BenchmarkDotNet.Attributes;

namespace SkiaSharp.Benchmarks;

[MemoryDiagnoser]
public class SKDataUnknownLengthStreamBenchmark
{
	[Params(4096, 2097152, 3145728)]
	public int Size { get; set; }

	[Params(4096, int.MaxValue)]
	public int MaxRead { get; set; }

	[Params(false, true)]
	public bool SupportsSpan { get; set; }

	private LimitedReadStream stream;

	[GlobalSetup]
	public void Setup()
	{
		var bytes = new byte[Size];
		new Random(42).NextBytes(bytes);
		stream = new LimitedReadStream(bytes, MaxRead, SupportsSpan);
		using var old = CreateBuffered(stream, useArrayRead: false);
		stream.Position = 0;
		using var candidate = SKData.Create(stream);
		if (!candidate.AsSpan().SequenceEqual(bytes) || !candidate.AsSpan().SequenceEqual(old.AsSpan()))
			throw new InvalidOperationException("The benchmark implementations produced different data.");
		stream.Position = 0;
		using var filledChunks = CreateBufferedFilledChunks(stream);
		if (!filledChunks.AsSpan().SequenceEqual(bytes))
			throw new InvalidOperationException("The batched native stream produced different data.");
		stream.Position = 0;
		using var previous = CreateHandRolled(stream);
		if (!previous.AsSpan().SequenceEqual(bytes))
			throw new InvalidOperationException("The previous collector produced different data.");
	}

	[GlobalCleanup]
	public void Cleanup() => stream.Dispose();

	[Benchmark(Baseline = true)]
	public long Old_DynamicStream()
	{
		stream.Position = 0;
		using var data = CreateBuffered(stream, useArrayRead: false);
		return data.Size;
	}

	[Benchmark]
	public long DynamicStream_ArrayRead()
	{
		stream.Position = 0;
		using var data = CreateBuffered(stream, useArrayRead: true);
		return data.Size;
	}

	[Benchmark]
	public long New_PooledChunks()
	{
		stream.Position = 0;
		using var data = SKData.Create(stream);
		return data.Size;
	}

	[Benchmark]
	public long Previous_HandRolledChunks()
	{
		stream.Position = 0;
		using var data = CreateHandRolled(stream);
		return data.Size;
	}

	private static unsafe SKData CreateHandRolled(Stream source)
	{
		var chunks = Utils.RentArray<(byte[] Array, int Length)>(8);
		var chunkCount = 0;
		ulong totalLength = 0;
		try
		{
			while (true)
			{
				if (chunkCount == chunks.Length)
				{
					var expanded = Utils.RentArray<(byte[] Array, int Length)>(checked(chunkCount * 2));
					chunks.Span.CopyTo(expanded.Span);
					chunks.Span.Clear();
					chunks.Dispose();
					chunks = expanded;
				}
				var buffer = ArrayPool<byte>.Shared.Rent(SKData.UnknownLengthStreamChunkSize);
				chunks[chunkCount++] = (buffer, 0);
				var filled = 0;
				while (filled < SKData.UnknownLengthStreamChunkSize)
				{
					var toRead = SKData.UnknownLengthStreamChunkSize - filled;
					var read = source.Read(buffer, filled, toRead);
					if ((uint)read > (uint)toRead)
						throw new IOException("The stream returned an invalid byte count.");
					if (read == 0)
						break;
					filled += read;
				}
				chunks[chunkCount - 1] = (buffer, filled);
				totalLength = checked(totalLength + (ulong)filled);
				if (IntPtr.Size == 4 && totalLength > uint.MaxValue)
					throw new ArgumentOutOfRangeException("length");
				if (filled < SKData.UnknownLengthStreamChunkSize)
					break;
			}
			if (chunkCount == 1)
				return SKData.CreateCopy(chunks[0].Array, (ulong)chunks[0].Length);
			var data = SKData.Create(totalLength);
			if (data == null)
				return null;
			try
			{
				var dst = (byte*)data.Data;
				for (var i = 0; i < chunkCount; i++)
				{
					var (buffer, length) = chunks[i];
					new ReadOnlySpan<byte>(buffer, 0, length).CopyTo(new Span<byte>(dst, length));
					dst += length;
				}
				return data;
			}
			catch
			{
				data.Dispose();
				throw;
			}
		}
		finally
		{
			for (var i = 0; i < chunkCount; i++)
				ArrayPool<byte>.Shared.Return(chunks[i].Array);
			chunks.Span.Clear();
			chunks.Dispose();
		}
	}

	[Benchmark]
	public long DynamicStream_FilledChunks()
	{
		stream.Position = 0;
		using var data = CreateBufferedFilledChunks(stream);
		return data.Size;
	}

	private static SKData CreateBufferedFilledChunks(Stream source)
	{
		using var memory = new SKDynamicMemoryWStream();
		using var buffer = Utils.RentArray<byte>(SKData.UnknownLengthStreamChunkSize);
		while (true)
		{
			var filled = 0;
			while (filled < buffer.Length)
			{
				var toRead = buffer.Length - filled;
				var read = source.Read(buffer.Array, filled, toRead);
				if ((uint)read > (uint)toRead)
					throw new IOException("The stream returned an invalid byte count.");
				if (read == 0)
					break;
				filled += read;
			}
			if (!memory.Write(buffer.Array, filled))
				return null;
			if (filled < buffer.Length)
				break;
		}
		return memory.DetachAsData();
	}

	private static SKData CreateBuffered(Stream source, bool useArrayRead)
	{
		using var memory = new SKDynamicMemoryWStream();
		using var buffer = Utils.RentArray<byte>(SKData.CopyBufferSize);
		while (true)
		{
			var read = useArrayRead
				? source.Read(buffer.Array, 0, buffer.Length)
				: source.Read(buffer.Span);
			if ((uint)read > (uint)buffer.Length)
				throw new IOException("The stream returned an invalid byte count.");
			if (read == 0)
				break;
			if (!memory.Write(buffer.Array, read))
				return null;
		}
		return memory.DetachAsData();
	}

	private sealed class LimitedReadStream : Stream
	{
		private readonly MemoryStream inner;
		private readonly int maxRead;
		private readonly bool supportsSpan;

		public LimitedReadStream(byte[] bytes, int maxRead, bool supportsSpan)
		{
			inner = new MemoryStream(bytes, writable: false);
			this.maxRead = maxRead;
			this.supportsSpan = supportsSpan;
		}

		public override bool CanRead => true;
		public override bool CanSeek => false;
		public override bool CanWrite => false;
		public override long Length => throw new NotSupportedException();
		public override long Position { get => inner.Position; set => inner.Position = value; }
		public override void Flush() => throw new NotSupportedException();
		public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
		public override void SetLength(long value) => throw new NotSupportedException();
		public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

		public override int Read(byte[] buffer, int offset, int count) =>
			inner.Read(buffer, offset, Math.Min(count, maxRead));

		public override int Read(Span<byte> buffer) =>
			supportsSpan
				? inner.Read(buffer.Slice(0, Math.Min(buffer.Length, maxRead)))
				: base.Read(buffer);

		protected override void Dispose(bool disposing)
		{
			if (disposing)
				inner.Dispose();
			base.Dispose(disposing);
		}
	}
}
