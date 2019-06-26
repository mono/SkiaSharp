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

		public IntPtr Glyphs => (IntPtr)internalBuffer.Glyphs;

		public IntPtr Clusters => (IntPtr)internalBuffer.Clusters;

		public IntPtr Utf8Text => (IntPtr)internalBuffer.Utf8text;

		public Span<ushort> GetGlyphSpan () =>
			new Span<ushort> (internalBuffer.Glyphs, Size);

		public Span<byte> GetUtf8TextSpan () =>
			new Span<byte> (internalBuffer.Utf8text, TextSize);

		public Span<uint> GetClusterSpan () =>
			new Span<uint> (internalBuffer.Clusters, Size);

		public void SetGlyphs (ushort[] glyphs) =>
			glyphs.CopyTo (GetGlyphSpan ());

		public void SetGlyphs (ReadOnlySpan<ushort> glyphs) =>
			glyphs.CopyTo (GetGlyphSpan ());

		public void SetUtf8Text (byte[] text) =>
			text.CopyTo (GetUtf8TextSpan ());

		public void SetUtf8Text (ReadOnlySpan<byte> text) =>
			text.CopyTo (GetUtf8TextSpan ());

		public void SetClusters (uint[] clusters) =>
			clusters.CopyTo (GetClusterSpan ());

		public void SetClusters (ReadOnlySpan<uint> clusters) =>
			clusters.CopyTo (GetClusterSpan ());
	}

	public sealed unsafe class SKHorizontalRunBuffer : SKRunBuffer
	{
		internal SKHorizontalRunBuffer (SKRunBufferInternal buffer, int count, int textSize)
			: base (buffer, count, textSize)
		{
		}

		public IntPtr Positions => (IntPtr)internalBuffer.Positions;

		public Span<float> GetPositionSpan () =>
			new Span<float> (internalBuffer.Positions, Size);

		public void SetPositions (float[] positions) =>
			positions.CopyTo (GetPositionSpan ());

		public void SetPositions (ReadOnlySpan<float> positions) =>
			positions.CopyTo (GetPositionSpan ());
	}

	public sealed unsafe class SKPositionedRunBuffer : SKRunBuffer
	{
		internal SKPositionedRunBuffer (SKRunBufferInternal buffer, int count, int textSize)
			: base (buffer, count, textSize)
		{
		}

		public IntPtr Positions => (IntPtr)internalBuffer.Positions;

		public Span<SKPoint> GetPositionSpan () =>
			new Span<SKPoint> (internalBuffer.Positions, Size);

		public void SetPositions (SKPoint[] positions) =>
			positions.CopyTo (GetPositionSpan ());

		public void SetPositions (ReadOnlySpan<SKPoint> positions) =>
			positions.CopyTo (GetPositionSpan ());
	}

	[StructLayout (LayoutKind.Sequential)]
	internal unsafe struct SKRunBufferInternal
	{
		public void* Glyphs;
		public void* Positions;
		public void* Utf8text;
		public void* Clusters;
	}
}
