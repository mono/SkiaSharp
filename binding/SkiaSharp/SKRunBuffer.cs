#nullable disable

using System;

namespace SkiaSharp
{
	// Base

	public unsafe class SKRunBuffer
	{
		internal readonly SKRunBufferInternal internalBuffer;

		internal SKRunBuffer (SKRunBufferInternal buffer, int size)
		{
			internalBuffer = buffer;
			Size = size;
		}

		public int Size { get; }

		public Span<ushort> Glyphs => new (internalBuffer.glyphs, Size);

		/// <param name="glyphs"></param>
		public void SetGlyphs (ReadOnlySpan<ushort> glyphs) => glyphs.CopyTo (Glyphs);

		[Obsolete ("Use Glyphs instead.")]
		public Span<ushort> GetGlyphSpan () => Glyphs;
	}

	public sealed unsafe class SKHorizontalRunBuffer : SKRunBuffer
	{
		internal SKHorizontalRunBuffer (SKRunBufferInternal buffer, int size)
			: base (buffer, size)
		{
		}

		public Span<float> Positions => new (internalBuffer.pos, Size);

		/// <param name="positions"></param>
		public void SetPositions (ReadOnlySpan<float> positions) => positions.CopyTo (Positions);

		[Obsolete ("Use Positions instead.")]
		public Span<float> GetPositionSpan () => Positions;
	}

	public sealed unsafe class SKPositionedRunBuffer : SKRunBuffer
	{
		internal SKPositionedRunBuffer (SKRunBufferInternal buffer, int size)
			: base (buffer, size)
		{
		}

		public Span<SKPoint> Positions => new (internalBuffer.pos, Size);

		/// <param name="positions"></param>
		public void SetPositions (ReadOnlySpan<SKPoint> positions) => positions.CopyTo (Positions);

		[Obsolete ("Use Positions instead.")]
		public Span<SKPoint> GetPositionSpan () => Positions;
	}

	public sealed unsafe class SKRotationScaleRunBuffer : SKRunBuffer
	{
		internal SKRotationScaleRunBuffer (SKRunBufferInternal buffer, int size)
			: base (buffer, size)
		{
		}

		public Span<SKRotationScaleMatrix> Positions => new (internalBuffer.pos, Size);

		public void SetPositions (ReadOnlySpan<SKRotationScaleMatrix> positions) => positions.CopyTo (Positions);

		[Obsolete ("Use Positions instead.")]
		public Span<SKRotationScaleMatrix> GetRotationScaleSpan () => Positions;

		/// <param name="positions"></param>
		[Obsolete ("Use SetPositions instead.")]
		public void SetRotationScale (ReadOnlySpan<SKRotationScaleMatrix> positions) => SetPositions (positions);
	}

	// Text

	public unsafe class SKTextRunBuffer : SKRunBuffer
	{
		internal SKTextRunBuffer (SKRunBufferInternal buffer, int size, int textSize)
			: base (buffer, size)
		{
			TextSize = textSize;
		}

		public int TextSize { get; }

		public Span<byte> Text => new (internalBuffer.utf8text, TextSize);

		public Span<uint> Clusters => new (internalBuffer.clusters, Size);

		public void SetText (ReadOnlySpan<byte> text) => text.CopyTo (Text);

		public void SetClusters (ReadOnlySpan<uint> clusters) => clusters.CopyTo (Clusters);
	}

	public sealed unsafe class SKHorizontalTextRunBuffer : SKTextRunBuffer
	{
		internal SKHorizontalTextRunBuffer (SKRunBufferInternal buffer, int size, int textSize)
			: base (buffer, size, textSize)
		{
		}

		public Span<float> Positions => new (internalBuffer.pos, Size);

		public void SetPositions (ReadOnlySpan<float> positions) => positions.CopyTo (Positions);
	}

	public sealed unsafe class SKPositionedTextRunBuffer : SKTextRunBuffer
	{
		internal SKPositionedTextRunBuffer (SKRunBufferInternal buffer, int size, int textSize)
			: base (buffer, size, textSize)
		{
		}

		public Span<SKPoint> Positions => new (internalBuffer.pos, Size);

		public void SetPositions (ReadOnlySpan<SKPoint> positions) => positions.CopyTo (Positions);
	}

	public sealed unsafe class SKRotationScaleTextRunBuffer : SKTextRunBuffer
	{
		internal SKRotationScaleTextRunBuffer (SKRunBufferInternal buffer, int size, int textSize)
			: base (buffer, size, textSize)
		{
		}

		public Span<SKRotationScaleMatrix> Positions => new (internalBuffer.pos, Size);

		public void SetPositions (ReadOnlySpan<SKRotationScaleMatrix> positions) => positions.CopyTo (Positions);
	}

	// Raw / Struct

	public unsafe readonly struct SKRawRunBuffer<T>
	{
		internal readonly SKRunBufferInternal buffer;
		private readonly int size;
		private readonly int posSize;
		private readonly int textSize;

		internal SKRawRunBuffer (SKRunBufferInternal buffer, int size, int posSize, int textSize)
		{
			this.buffer = buffer;
			this.size = size;
			this.posSize = posSize;
			this.textSize = textSize;
		}

		public Span<ushort> Glyphs => new (buffer.glyphs, size);

		public Span<T> Positions => new (buffer.pos, posSize);

		public Span<byte> Text => new (buffer.utf8text, textSize);

		public Span<uint> Clusters => new (buffer.clusters, size);
	}
}
