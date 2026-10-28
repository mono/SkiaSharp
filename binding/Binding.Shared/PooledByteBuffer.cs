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
	internal const int ChunkSize = 64 * 1024;
	private const int InitialMetadataCapacity = 16;
	// Geometric metadata growth reaches managed array-size limits before this directory fills.
	private const int MaxMetadataSegments = 32;

	private readonly ArrayPool<byte> pool;
	private Chunk first;
	private Chunk[][] segments;
	private int segmentCount;
	private int lastSegmentCount;

	// Payloads use the supplied pool; metadata uses shared pools.
	internal PooledByteBuffer (ArrayPool<byte> pool)
	{
		this = default;
		this.pool = pool ?? throw new ArgumentNullException (nameof (pool));
	}

	// Committed bytes only, excluding unused chunk capacity.
	internal ulong Length { get; private set; }

	// Return writable space, renting another chunk only when the current one is full.
	internal ArraySegment<byte> GetBuffer ()
	{
		if (pool == null)
			throw new InvalidOperationException ("The buffer has not been initialized.");

		// Keep the first descriptor inline so small inputs need no metadata rental.
		if (first.Array == null) {
			first.Array = pool.Rent (ChunkSize);
			return new ArraySegment<byte> (first.Array, 0, ChunkSize);
		}
		if (segmentCount == 0 && first.Length < ChunkSize)
			return new ArraySegment<byte> (first.Array, first.Length, ChunkSize - first.Length);
		if (lastSegmentCount > 0) {
			var last = segments[segmentCount - 1][lastSegmentCount - 1];
			if (last.Length < ChunkSize)
				return new ArraySegment<byte> (last.Array, last.Length, ChunkSize - last.Length);
		}

		segments ??= ArrayPool<Chunk[]>.Shared.Rent (MaxMetadataSegments);
		if (segmentCount == 0 || lastSegmentCount == segments[segmentCount - 1].Length) {
			var previousCapacity = segmentCount == 0 ? 0 : segments[segmentCount - 1].Length;
			if (segmentCount == MaxMetadataSegments || previousCapacity > int.MaxValue / 2)
				throw new OutOfMemoryException ("The buffer contains too many chunks.");

			// Like SegmentedArrayBuilder, append larger metadata segments without copying old entries.
			var capacity = previousCapacity == 0 ? InitialMetadataCapacity : previousCapacity * 2;
			var segment = ArrayPool<Chunk>.Shared.Rent (capacity);
			segments[segmentCount++] = segment;
			lastSegmentCount = 0;
		}

		var array = pool.Rent (ChunkSize);
		segments[segmentCount - 1][lastSegmentCount++] = new Chunk { Array = array };
		return new ArraySegment<byte> (array, 0, ChunkSize);
	}

	// Commit bytes written into the range returned by GetBuffer.
	internal void Advance (int count)
	{
		if (first.Array == null || (segmentCount > 0 && lastSegmentCount == 0))
			throw new InvalidOperationException ("GetBuffer must be called before Advance.");

		var used = segmentCount == 0 ? first.Length : segments[segmentCount - 1][lastSegmentCount - 1].Length;
		if ((uint)count > (uint)(ChunkSize - used))
			throw new ArgumentOutOfRangeException (nameof (count));

		var length = checked (Length + (ulong)count);
		if (segmentCount == 0)
			first.Length += count;
		else
			segments[segmentCount - 1][lastSegmentCount - 1].Length += count;
		Length = length;
	}

	// Expose the small-input fast path without enumerating metadata segments.
	internal bool TryGetSingleBuffer (out ArraySegment<byte> buffer)
	{
		if (first.Array != null && segmentCount == 0) {
			buffer = new ArraySegment<byte> (first.Array, 0, first.Length);
			return true;
		}
		buffer = default;
		return false;
	}

	// Enumerate committed ranges in order; borrowed ranges are valid until disposal.
	public Enumerator GetEnumerator () =>
		new Enumerator (first, segments, segmentCount, lastSegmentCount);

	// Return every rental and clear metadata references. Repeated disposal is harmless.
	public void Dispose ()
	{
		if (first.Array != null)
			pool.Return (first.Array);
		first = default;
		if (segments != null) {
			for (var i = 0; i < segmentCount; i++) {
				var segment = segments[i];
				var count = i == segmentCount - 1 ? lastSegmentCount : segment.Length;
				for (var j = 0; j < count; j++)
					pool.Return (segment[j].Array);
				ArrayPool<Chunk>.Shared.Return (segment, clearArray: true);
			}
			ArrayPool<Chunk[]>.Shared.Return (segments, clearArray: true);
		}
		segments = null;
		segmentCount = lastSegmentCount = 0;
		Length = 0;
	}

	internal struct Chunk
	{
		internal byte[] Array;
		internal int Length;
	}

	public ref struct Enumerator
	{
		private readonly Chunk first;
		private readonly Chunk[][] segments;
		private readonly int segmentCount;
		private readonly int lastSegmentCount;
		private int segmentIndex;
		private int chunkIndex;

		// Capture the segment layout without allocating an enumerator object.
		internal Enumerator (Chunk first, Chunk[][] segments, int segmentCount, int lastSegmentCount)
		{
			this.first = first;
			this.segments = segments;
			this.segmentCount = segmentCount;
			this.lastSegmentCount = lastSegmentCount;
			segmentIndex = -1;
			chunkIndex = 0;
			Current = default;
		}

		public ArraySegment<byte> Current { get; private set; }

		// Visit the inline descriptor, then each populated metadata segment.
		public bool MoveNext ()
		{
			if (segmentIndex == -1) {
				segmentIndex = 0;
				if (first.Array != null) {
					Current = new ArraySegment<byte> (first.Array, 0, first.Length);
					return true;
				}
			}
			while (segmentIndex < segmentCount) {
				var segment = segments[segmentIndex];
				var count = segmentIndex == segmentCount - 1 ? lastSegmentCount : segment.Length;
				if (chunkIndex < count) {
					var chunk = segment[chunkIndex++];
					Current = new ArraySegment<byte> (chunk.Array, 0, chunk.Length);
					return true;
				}
				segmentIndex++;
				chunkIndex = 0;
			}
			Current = default;
			return false;
		}
	}
}
