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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		internal SKRunBuffer (SKRunBufferInternal buffer, int size, int textSize)
		{
			internalBuffer = buffer;
			Size = size;
			TextSize = textSize;
		}

		public int Size { get; }

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public int TextSize { get; }

		public Span<ushort> GetGlyphSpan () =>
			new Span<ushort> (internalBuffer.glyphs, internalBuffer.glyphs == null ? 0 : Size);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public Span<byte> GetTextSpan () =>
			new Span<byte> (internalBuffer.utf8text, internalBuffer.utf8text == null ? 0 : TextSize);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public Span<uint> GetClusterSpan () =>
			new Span<uint> (internalBuffer.clusters, internalBuffer.clusters == null ? 0 : Size);

		public void SetGlyphs (ReadOnlySpan<ushort> glyphs) =>
			glyphs.CopyTo (GetGlyphSpan ());

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public void SetText (ReadOnlySpan<byte> text) =>
			text.CopyTo (GetTextSpan ());

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public void SetClusters (ReadOnlySpan<uint> clusters) =>
			clusters.CopyTo (GetClusterSpan ());
	}

	public sealed unsafe class SKHorizontalRunBuffer : SKRunBuffer
	{
		internal SKHorizontalRunBuffer (SKRunBufferInternal buffer, int count)
			: base (buffer, count)
		{
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		internal SKHorizontalRunBuffer (SKRunBufferInternal buffer, int count, int textSize)
			: base (buffer, count, textSize)
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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		internal SKPositionedRunBuffer (SKRunBufferInternal buffer, int count, int textSize)
			: base (buffer, count, textSize)
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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		internal SKRotationScaleRunBuffer (SKRunBufferInternal buffer, int count, int textSize)
			: base (buffer, count, textSize)
		{
		}

		public Span<SKRotationScaleMatrix> GetRotationScaleSpan () =>
			new Span<SKRotationScaleMatrix> (internalBuffer.pos, Size);

		public void SetRotationScale (ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			positions.CopyTo (GetRotationScaleSpan ());
	}
}
