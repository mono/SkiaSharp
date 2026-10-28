using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SkiaSharp.Tests;

public class PooledByteBufferTest
{
	[Fact]
	public void EmptyBufferDoesNotRentPayload()
	{
		var pool = new TrackingPool();
		{
			using var buffer = new PooledByteBuffer(pool);
			Assert.Equal(0UL, buffer.Length);
			Assert.False(buffer.TryGetSingleBuffer(out _));
			var enumerator = buffer.GetEnumerator();
			Assert.False(enumerator.MoveNext());
		}
		Assert.Equal(0, pool.Rents);
		Assert.Equal(0, pool.Outstanding);
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void PartialAdvancesReuseTheCurrentDataBuffer(bool afterFullFirstBuffer)
	{
		var pool = new TrackingPool();
		{
			using var buffer = new PooledByteBuffer(pool);
			if (afterFullFirstBuffer)
			{
				var prefix = buffer.GetBuffer();
				buffer.Advance(prefix.Count);
			}
			var first = buffer.GetBuffer();
			first.AsSpan().Slice(0, 17).Fill(42);
			buffer.Advance(17);
			var remaining = buffer.GetBuffer();
			Assert.Same(first.Array, remaining.Array);
			Assert.Equal(17, remaining.Offset);
			Assert.Equal(PooledByteBuffer.DataBufferSize - 17, remaining.Count);
			remaining.AsSpan().Slice(0, 3).Fill(84);
			buffer.Advance(3);

			Assert.Equal((ulong)(afterFullFirstBuffer ? PooledByteBuffer.DataBufferSize : 0) + 20, buffer.Length);
			Assert.Equal(!afterFullFirstBuffer, buffer.TryGetSingleBuffer(out var single));
			if (!afterFullFirstBuffer)
				Assert.Equal(20, single.Count);
			Assert.Equal(42, first.Array[16]);
			Assert.Equal(84, first.Array[17]);
			Assert.Equal(afterFullFirstBuffer ? 2 : 1, pool.Rents);
		}
		Assert.Equal(0, pool.Outstanding);
	}

	[Theory]
	[InlineData(1)]
	[InlineData(17)]
	[InlineData(18)]
	[InlineData(49)]
	[InlineData(50)]
	public void EnumerationPreservesDataAcrossEntryBlocks(int count)
	{
		var pool = new TrackingPool();
		{
			using var buffer = new PooledByteBuffer(pool);
			for (var i = 0; i < count; i++)
			{
				var chunk = buffer.GetBuffer();
				var length = i == count - 1 ? 7 : chunk.Count;
				chunk.AsSpan().Slice(0, length).Fill((byte)(i + 1));
				buffer.Advance(length);
			}
			Assert.Equal((ulong)(count - 1) * PooledByteBuffer.DataBufferSize + 7, buffer.Length);
			Assert.Equal(count == 1, buffer.TryGetSingleBuffer(out _));

			var expected = new byte[PooledByteBuffer.DataBufferSize];
			var index = 0;
			foreach (var chunk in buffer)
			{
				var length = index == count - 1 ? 7 : PooledByteBuffer.DataBufferSize;
				expected.AsSpan().Fill((byte)(index + 1));
				Assert.Equal(length, chunk.Count);
				Assert.True(chunk.AsSpan().SequenceEqual(expected.AsSpan(0, length)));
				index++;
			}
			Assert.Equal(count, index);
		}
		Assert.Equal(count, pool.Returns);
		Assert.Equal(0, pool.Outstanding);
	}

	[Theory]
	[InlineData(-1, 0)]
	[InlineData(PooledByteBuffer.DataBufferSize + 1, 0)]
	[InlineData(int.MaxValue, 0)]
	[InlineData(PooledByteBuffer.DataBufferSize, 1)]
	public void AdvanceRejectsInvalidCounts(int count, int alreadyWritten)
	{
		var pool = new TrackingPool();
		Assert.Throws<ArgumentOutOfRangeException>(() => AdvanceInvalid(pool, count, alreadyWritten));
		Assert.Equal(0, pool.Outstanding);
	}

	private static void AdvanceInvalid(TrackingPool pool, int count, int alreadyWritten)
	{
		using var buffer = new PooledByteBuffer(pool);
		buffer.GetBuffer();
		buffer.Advance(alreadyWritten);
		buffer.Advance(count);
	}

	[Fact]
	public void AdvanceRequiresABuffer()
	{
		Assert.Throws<InvalidOperationException>(AdvanceWithoutBuffer);

		static void AdvanceWithoutBuffer()
		{
			using var buffer = new PooledByteBuffer(ArrayPool<byte>.Shared);
			buffer.Advance(1);
		}
	}

	[Theory]
	[InlineData(1)]
	[InlineData(2)]
	[InlineData(18)]
	[InlineData(50)]
	public void FailedRentDoesNotLosePreviouslyOwnedBuffers(int failAtRent)
	{
		var pool = new TrackingPool { FailAtRent = failAtRent };
		Assert.Throws<IOException>(() => FillUntilFailure(pool, failAtRent));
		Assert.Equal(failAtRent - 1, pool.Returns);
		Assert.Equal(0, pool.Outstanding);
	}

	private static void FillUntilFailure(TrackingPool pool, int count)
	{
		using var buffer = new PooledByteBuffer(pool);
		for (var i = 0; i < count; i++)
		{
			var chunk = buffer.GetBuffer();
			buffer.Advance(chunk.Count);
		}
	}

	[Fact]
	public void DisposeReturnsEachPayloadOnceAndClearsTheOwner()
	{
		var pool = new TrackingPool();
		var buffer = new PooledByteBuffer(pool);
		buffer.GetBuffer();
		buffer.Advance(7);
		buffer.Dispose();
		buffer.Dispose();
		Assert.Equal(1, pool.Returns);
		Assert.Equal(0, pool.Outstanding);
		Assert.Equal(0UL, buffer.Length);
		Assert.False(buffer.TryGetSingleBuffer(out _));
		var enumerator = buffer.GetEnumerator();
		Assert.False(enumerator.MoveNext());
	}

	private sealed class TrackingPool : ArrayPool<byte>
	{
		private readonly HashSet<byte[]> outstanding = new();
		public int Rents { get; private set; }
		public int Returns { get; private set; }
		public int Outstanding => outstanding.Count;
		public int FailAtRent { get; set; }

		public override byte[] Rent(int minimumLength)
		{
			if (++Rents == FailAtRent)
				throw new IOException("Simulated pool exhaustion.");
			var array = new byte[minimumLength];
			Assert.True(outstanding.Add(array));
			return array;
		}

		public override void Return(byte[] array, bool clearArray = false)
		{
			Assert.True(outstanding.Remove(array), "A buffer was returned twice or was not owned.");
			Returns++;
		}
	}
}
