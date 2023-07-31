using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public unsafe class SKRunBuffer
	{
		internal readonly SKRunBufferInternal internalBuffer;

		internal SKRunBuffer (SKRunBufferInternal buffer, int size)
		{
			internalBuffer = buffer;
			Size = size;
		}

		public int Size { get; }

		public Span<ushort> GetGlyphSpan () =>
			new Span<ushort> (internalBuffer.glyphs, internalBuffer.glyphs == null ? 0 : Size);

		public void SetGlyphs (ReadOnlySpan<ushort> glyphs) =>
			glyphs.CopyTo (GetGlyphSpan ());
	}

	public sealed unsafe class SKHorizontalRunBuffer : SKRunBuffer
	{
		internal SKHorizontalRunBuffer (SKRunBufferInternal buffer, int count)
			: base (buffer, count)
		{
		}

		public Span<float> GetPositionSpan () =>
			new Span<float> (internalBuffer.pos, internalBuffer.pos == null ? 0 : Size);

		public void SetPositions (ReadOnlySpan<float> positions) =>
			positions.CopyTo (GetPositionSpan ());
	}

	public sealed unsafe class SKPositionedRunBuffer : SKRunBuffer
	{
		internal SKPositionedRunBuffer (SKRunBufferInternal buffer, int count)
			: base (buffer, count)
		{
		}

		public Span<SKPoint> GetPositionSpan () =>
			new Span<SKPoint> (internalBuffer.pos, internalBuffer.pos == null ? 0 : Size);

		public void SetPositions (ReadOnlySpan<SKPoint> positions) =>
			positions.CopyTo (GetPositionSpan ());
	}

	public sealed unsafe class SKRotationScaleRunBuffer : SKRunBuffer
	{
		internal SKRotationScaleRunBuffer (SKRunBufferInternal buffer, int count)
			: base (buffer, count)
		{
		}

		public Span<SKRotationScaleMatrix> GetRotationScaleSpan () =>
			new Span<SKRotationScaleMatrix> (internalBuffer.pos, Size);

		public void SetRotationScale (ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			positions.CopyTo (GetRotationScaleSpan ());
	}
}
