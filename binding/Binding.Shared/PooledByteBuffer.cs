#nullable disable

using System;
using System.Buffers;

#if HARFBUZZ
namespace HarfBuzzSharp;
#else
namespace SkiaSharp;
#endif

internal ref struct PooledByteBuffer
{
	// An exact shared-pool bucket below the default LOH threshold; 80 KiB would rent 128 KiB.
	internal const int DataBufferSize = 64 * 1024;
	// Start with the shared pool's 16-entry bucket (~256 bytes of entries on 64-bit).
	// Those entries describe 1 MiB of data beyond the inline buffer.
	private const int InitialEntryBlockCapacity = 16;
	// Geometric entry-block growth reaches managed array-size limits before this directory fills.
	private const int MaxEntryBlocks = 32;

	private readonly ArrayPool<byte> pool;
	private BufferEntry firstEntry;
	// entryBlocks[block][entry].Buffer holds data; WrittenCount identifies its valid bytes.
	private BufferEntry[][] entryBlocks;
	private int entryBlockCount;
	private int lastBlockEntryCount;

	// Data buffers use the supplied pool; entry blocks use shared pools.
	internal PooledByteBuffer (ArrayPool<byte> pool)
	{
		this = default;
		this.pool = pool ?? throw new ArgumentNullException (nameof (pool));
	}

	// Committed bytes only, excluding unused data-buffer capacity.
	internal ulong Length { get; private set; }

	// Return writable space, renting another data buffer only when the current one is full.
	internal ArraySegment<byte> GetBuffer ()
	{
		if (pool == null)
			throw new InvalidOperationException ("The buffer has not been initialized.");

		// Keep the first entry inline so small inputs need no entry-block rental.
		if (firstEntry.Buffer == null) {
			firstEntry.Buffer = pool.Rent (DataBufferSize);
			return new ArraySegment<byte> (firstEntry.Buffer, 0, DataBufferSize);
		}
		if (entryBlockCount == 0 && firstEntry.WrittenCount < DataBufferSize)
			return new ArraySegment<byte> (firstEntry.Buffer, firstEntry.WrittenCount, DataBufferSize - firstEntry.WrittenCount);
		if (lastBlockEntryCount > 0) {
			var last = entryBlocks[entryBlockCount - 1][lastBlockEntryCount - 1];
			if (last.WrittenCount < DataBufferSize)
				return new ArraySegment<byte> (last.Buffer, last.WrittenCount, DataBufferSize - last.WrittenCount);
		}

		entryBlocks ??= ArrayPool<BufferEntry[]>.Shared.Rent (MaxEntryBlocks);
		if (entryBlockCount == 0 || lastBlockEntryCount == entryBlocks[entryBlockCount - 1].Length) {
			var previousCapacity = entryBlockCount == 0 ? 0 : entryBlocks[entryBlockCount - 1].Length;
			if (entryBlockCount == MaxEntryBlocks || previousCapacity > int.MaxValue / 2)
				throw new OutOfMemoryException ("The buffer contains too many data buffers.");

			// Like SegmentedArrayBuilder, append larger entry blocks without copying old entries.
			var capacity = previousCapacity == 0 ? InitialEntryBlockCapacity : previousCapacity * 2;
			var block = ArrayPool<BufferEntry>.Shared.Rent (capacity);
			entryBlocks[entryBlockCount++] = block;
			lastBlockEntryCount = 0;
		}

		var dataBuffer = pool.Rent (DataBufferSize);
		entryBlocks[entryBlockCount - 1][lastBlockEntryCount++] = new BufferEntry { Buffer = dataBuffer };
		return new ArraySegment<byte> (dataBuffer, 0, DataBufferSize);
	}

	// Commit bytes written into the range returned by GetBuffer.
	internal void Advance (int count)
	{
		if (firstEntry.Buffer == null || (entryBlockCount > 0 && lastBlockEntryCount == 0))
			throw new InvalidOperationException ("GetBuffer must be called before Advance.");

		var used = entryBlockCount == 0
			? firstEntry.WrittenCount
			: entryBlocks[entryBlockCount - 1][lastBlockEntryCount - 1].WrittenCount;
		if ((uint)count > (uint)(DataBufferSize - used))
			throw new ArgumentOutOfRangeException (nameof (count));

		var length = checked (Length + (ulong)count);
		if (entryBlockCount == 0)
			firstEntry.WrittenCount += count;
		else
			entryBlocks[entryBlockCount - 1][lastBlockEntryCount - 1].WrittenCount += count;
		Length = length;
	}

	// Expose the small-input fast path without enumerating entry blocks.
	internal bool TryGetSingleBuffer (out ArraySegment<byte> buffer)
	{
		if (firstEntry.Buffer != null && entryBlockCount == 0) {
			buffer = new ArraySegment<byte> (firstEntry.Buffer, 0, firstEntry.WrittenCount);
			return true;
		}
		buffer = default;
		return false;
	}

	// Enumerate committed ranges in order; borrowed ranges are valid until disposal.
	public Enumerator GetEnumerator () =>
		new Enumerator (firstEntry, entryBlocks, entryBlockCount, lastBlockEntryCount);

	// Return every data buffer and cleared entry block. Repeated disposal is harmless.
	public void Dispose ()
	{
		if (firstEntry.Buffer != null)
			pool.Return (firstEntry.Buffer);
		firstEntry = default;
		if (entryBlocks != null) {
			for (var i = 0; i < entryBlockCount; i++) {
				var block = entryBlocks[i];
				var count = i == entryBlockCount - 1 ? lastBlockEntryCount : block.Length;
				for (var j = 0; j < count; j++)
					pool.Return (block[j].Buffer);
				ArrayPool<BufferEntry>.Shared.Return (block, clearArray: true);
			}
			ArrayPool<BufferEntry[]>.Shared.Return (entryBlocks, clearArray: true);
		}
		entryBlocks = null;
		entryBlockCount = lastBlockEntryCount = 0;
		Length = 0;
	}

	internal struct BufferEntry
	{
		internal byte[] Buffer;
		internal int WrittenCount;
	}

	public ref struct Enumerator
	{
		private readonly BufferEntry firstEntry;
		private readonly BufferEntry[][] entryBlocks;
		private readonly int entryBlockCount;
		private readonly int lastBlockEntryCount;
		private int entryBlockIndex;
		private int entryIndex;

		// Capture the entry-block layout without allocating an enumerator object.
		internal Enumerator (BufferEntry firstEntry, BufferEntry[][] entryBlocks, int entryBlockCount, int lastBlockEntryCount)
		{
			this.firstEntry = firstEntry;
			this.entryBlocks = entryBlocks;
			this.entryBlockCount = entryBlockCount;
			this.lastBlockEntryCount = lastBlockEntryCount;
			entryBlockIndex = -1;
			entryIndex = 0;
			Current = default;
		}

		public ArraySegment<byte> Current { get; private set; }

		// Visit the inline entry, then each populated entry block.
		public bool MoveNext ()
		{
			if (entryBlockIndex == -1) {
				entryBlockIndex = 0;
				if (firstEntry.Buffer != null) {
					Current = new ArraySegment<byte> (firstEntry.Buffer, 0, firstEntry.WrittenCount);
					return true;
				}
			}
			while (entryBlockIndex < entryBlockCount) {
				var block = entryBlocks[entryBlockIndex];
				var count = entryBlockIndex == entryBlockCount - 1 ? lastBlockEntryCount : block.Length;
				if (entryIndex < count) {
					var entry = block[entryIndex++];
					Current = new ArraySegment<byte> (entry.Buffer, 0, entry.WrittenCount);
					return true;
				}
				entryBlockIndex++;
				entryIndex = 0;
			}
			Current = default;
			return false;
		}
	}
}
