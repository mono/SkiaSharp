using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public unsafe class SKRunBuffer
	{
		internal readonly SKRunBufferInternal internalBuffer;

		internal SKRunBuffer (SKRunBufferInternal buffer, int size, int textSize)
		{
			internalBuffer = buffer;
			Size = size;
			TextSize = textSize;
		}

		public int Size { get; }

		public int TextSize { get; }

		public Span<ushort> GetGlyphSpan () =>
			new Span<ushort> (internalBuffer.glyphs, internalBuffer.glyphs == null ? 0 : Size);

		public Span<byte> GetTextSpan () =>
			new Span<byte> (internalBuffer.utf8text, internalBuffer.utf8text == null ? 0 : TextSize);

		public Span<uint> GetClusterSpan () =>
			new Span<uint> (internalBuffer.clusters, internalBuffer.clusters == null ? 0 : Size);

		public void SetGlyphs (ReadOnlySpan<ushort> glyphs) =>
			glyphs.CopyTo (GetGlyphSpan ());

		public void SetText (ReadOnlySpan<byte> text) =>
			text.CopyTo (GetTextSpan ());

		public void SetClusters (ReadOnlySpan<uint> clusters) =>
			clusters.CopyTo (GetClusterSpan ());
	}

	public sealed unsafe class SKHorizontalRunBuffer : SKRunBuffer
	{
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
		internal SKPositionedRunBuffer (SKRunBufferInternal buffer, int count, int textSize)
			: base (buffer, count, textSize)
		{
		}

		public Span<SKPoint> GetPositionSpan () =>
			new Span<SKPoint> (internalBuffer.pos, internalBuffer.pos == null ? 0 : Size);

		public void SetPositions (ReadOnlySpan<SKPoint> positions) =>
			positions.CopyTo (GetPositionSpan ());
	}
}
