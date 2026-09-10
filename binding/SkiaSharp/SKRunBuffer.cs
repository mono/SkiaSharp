#nullable disable

using System;

namespace SkiaSharp
{
	// Base

	/// <summary>A run buffer that provides access to glyph data for a text run.</summary>
	/// <remarks />
	public unsafe class SKRunBuffer
	{
		internal readonly SKRunBufferInternal internalBuffer;

		internal SKRunBuffer (SKRunBufferInternal buffer, int size)
		{
			internalBuffer = buffer;
			Size = size;
		}

		/// <summary>Gets the number of glyphs in the run.</summary>
		/// <value>The number of glyphs in the run.</value>
		/// <remarks />
		public int Size { get; }

		/// <summary>Gets the span of glyph IDs for the run.</summary>
		/// <value>A span containing the glyph IDs.</value>
		/// <remarks />
		public Span<ushort> Glyphs => new (internalBuffer.glyphs, Size);

		/// <summary>Sets the glyph IDs for the run.</summary>
		/// <param name="glyphs">The glyph IDs to set.</param>
		/// <remarks />
		public void SetGlyphs (ReadOnlySpan<ushort> glyphs) => glyphs.CopyTo (Glyphs);

		/// <summary>Gets the span of glyph IDs for the run.</summary>
		/// <returns>A span containing the glyph IDs.</returns>
		/// <remarks />
		[Obsolete ("Use Glyphs instead.", error: true)]
		public Span<ushort> GetGlyphSpan () => Glyphs;
	}

	/// <summary>A run buffer for horizontally-positioned text where glyphs share a common Y coordinate.</summary>
	/// <remarks />
	public sealed unsafe class SKHorizontalRunBuffer : SKRunBuffer
	{
		internal SKHorizontalRunBuffer (SKRunBufferInternal buffer, int size)
			: base (buffer, size)
		{
		}

		/// <summary>Gets the span of horizontal X positions for each glyph.</summary>
		/// <value>A span containing the horizontal positions.</value>
		/// <remarks />
		public Span<float> Positions => new (internalBuffer.pos, Size);

		/// <summary>Sets the horizontal X positions for each glyph in the run.</summary>
		/// <param name="positions">The horizontal X positions to set for each glyph.</param>
		/// <remarks />
		public void SetPositions (ReadOnlySpan<float> positions) => positions.CopyTo (Positions);

		/// <summary>Gets the span of horizontal positions for each glyph.</summary>
		/// <returns>A span containing the horizontal positions.</returns>
		/// <remarks />
		[Obsolete ("Use Positions instead.", error: true)]
		public Span<float> GetPositionSpan () => Positions;
	}

	/// <summary>A run buffer for fully-positioned text where each glyph has an independent X and Y position.</summary>
	/// <remarks />
	public sealed unsafe class SKPositionedRunBuffer : SKRunBuffer
	{
		internal SKPositionedRunBuffer (SKRunBufferInternal buffer, int size)
			: base (buffer, size)
		{
		}

		/// <summary>Gets the span of positions for each glyph.</summary>
		/// <value>A span containing the positions.</value>
		/// <remarks />
		public Span<SKPoint> Positions => new (internalBuffer.pos, Size);

		/// <summary>Sets the positions for each glyph in the run.</summary>
		/// <param name="positions">The positions to set for each glyph.</param>
		/// <remarks />
		public void SetPositions (ReadOnlySpan<SKPoint> positions) => positions.CopyTo (Positions);

		/// <summary>Gets the span of positions for each glyph.</summary>
		/// <returns>A span containing the positions.</returns>
		/// <remarks />
		[Obsolete ("Use Positions instead.", error: true)]
		public Span<SKPoint> GetPositionSpan () => Positions;
	}

	/// <summary>A run buffer for text with rotation and scale transformations applied to each glyph.</summary>
	/// <remarks />
	public sealed unsafe class SKRotationScaleRunBuffer : SKRunBuffer
	{
		internal SKRotationScaleRunBuffer (SKRunBufferInternal buffer, int size)
			: base (buffer, size)
		{
		}

		/// <summary>Gets the span of rotation-scale matrices for each glyph.</summary>
		/// <value>A span containing the rotation-scale matrices.</value>
		/// <remarks />
		public Span<SKRotationScaleMatrix> Positions => new (internalBuffer.pos, Size);

		/// <summary>Sets the rotation-scale matrices for each glyph in the run.</summary>
		/// <param name="positions">The rotation-scale matrices to set for each glyph.</param>
		/// <remarks />
		public void SetPositions (ReadOnlySpan<SKRotationScaleMatrix> positions) => positions.CopyTo (Positions);

		/// <summary>Gets the span of rotation-scale matrices for each glyph.</summary>
		/// <returns>A span containing the rotation-scale matrices.</returns>
		/// <remarks />
		[Obsolete ("Use Positions instead.", error: true)]
		public Span<SKRotationScaleMatrix> GetRotationScaleSpan () => Positions;

		/// <summary>Sets the rotation-scale matrices for each glyph.</summary>
		/// <param name="positions">The rotation-scale matrices to set.</param>
		/// <remarks />
		[Obsolete ("Use SetPositions instead.", error: true)]
		public void SetRotationScale (ReadOnlySpan<SKRotationScaleMatrix> positions) => SetPositions (positions);
	}

	// Text

	/// <summary>A buffer for storing a text run with glyph IDs, positions, clusters, and text data.</summary>
	/// <remarks />
	public unsafe class SKTextRunBuffer : SKRunBuffer
	{
		internal SKTextRunBuffer (SKRunBufferInternal buffer, int size, int textSize)
			: base (buffer, size)
		{
			TextSize = textSize;
		}

		/// <summary>Gets the size of the text buffer in bytes.</summary>
		/// <value>The size of the text buffer.</value>
		/// <remarks />
		public int TextSize { get; }

		/// <summary>Gets the span of UTF-8 encoded source text bytes for the run.</summary>
		/// <value>A span containing the UTF-8 text bytes.</value>
		/// <remarks />
		public Span<byte> Text => new (internalBuffer.utf8text, TextSize);

		/// <summary>Gets the span of cluster indices that map glyphs to their source text positions.</summary>
		/// <value>A span containing the cluster indices.</value>
		/// <remarks />
		public Span<uint> Clusters => new (internalBuffer.clusters, Size);

		/// <summary>Sets the source text bytes for the run.</summary>
		/// <param name="text">The UTF-8 encoded text bytes to set.</param>
		/// <remarks />
		public void SetText (ReadOnlySpan<byte> text) => text.CopyTo (Text);

		/// <summary>Sets the cluster indices that map glyphs to their source text positions.</summary>
		/// <param name="clusters">The cluster indices to set.</param>
		/// <remarks />
		public void SetClusters (ReadOnlySpan<uint> clusters) => clusters.CopyTo (Clusters);
	}

	/// <summary>A buffer for horizontally-positioned text runs with cluster and text data.</summary>
	/// <remarks />
	public sealed unsafe class SKHorizontalTextRunBuffer : SKTextRunBuffer
	{
		internal SKHorizontalTextRunBuffer (SKRunBufferInternal buffer, int size, int textSize)
			: base (buffer, size, textSize)
		{
		}

		/// <summary>Gets the span of horizontal X positions for each glyph.</summary>
		/// <value>A span containing the horizontal positions.</value>
		/// <remarks />
		public Span<float> Positions => new (internalBuffer.pos, Size);

		/// <summary>Sets the horizontal X positions for each glyph in the run.</summary>
		/// <param name="positions">The horizontal X positions to set for each glyph.</param>
		/// <remarks />
		public void SetPositions (ReadOnlySpan<float> positions) => positions.CopyTo (Positions);
	}

	/// <summary>A buffer for fully-positioned text runs with cluster and text data.</summary>
	/// <remarks />
	public sealed unsafe class SKPositionedTextRunBuffer : SKTextRunBuffer
	{
		internal SKPositionedTextRunBuffer (SKRunBufferInternal buffer, int size, int textSize)
			: base (buffer, size, textSize)
		{
		}

		/// <summary>Gets the span of positions for each glyph.</summary>
		/// <value>A span containing the positions.</value>
		/// <remarks />
		public Span<SKPoint> Positions => new (internalBuffer.pos, Size);

		/// <summary>Sets the positions for each glyph in the run.</summary>
		/// <param name="positions">The positions to set for each glyph.</param>
		/// <remarks />
		public void SetPositions (ReadOnlySpan<SKPoint> positions) => positions.CopyTo (Positions);
	}

	/// <summary>A buffer for storing a text run with rotation and scale transformations applied to each glyph.</summary>
	/// <remarks />
	public sealed unsafe class SKRotationScaleTextRunBuffer : SKTextRunBuffer
	{
		internal SKRotationScaleTextRunBuffer (SKRunBufferInternal buffer, int size, int textSize)
			: base (buffer, size, textSize)
		{
		}

		/// <summary>Gets the span of rotation-scale matrices for each glyph in the run.</summary>
		/// <value>A span containing the rotation-scale matrices for positioning and transforming each glyph.</value>
		/// <remarks />
		public Span<SKRotationScaleMatrix> Positions => new (internalBuffer.pos, Size);

		/// <summary>Sets the rotation-scale matrices for each glyph in the run.</summary>
		/// <param name="positions">The rotation-scale matrices to set for each glyph.</param>
		/// <remarks />
		public void SetPositions (ReadOnlySpan<SKRotationScaleMatrix> positions) => positions.CopyTo (Positions);
	}

	// Raw / Struct

	/// <summary>A raw run buffer providing direct access to glyph, position, cluster, and text data.</summary>
	/// <typeparam name="T">The type of position data used for each glyph.</typeparam>
	/// <remarks />
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

		/// <summary>Gets the span of glyph IDs for the run.</summary>
		/// <value>A span containing the glyph IDs.</value>
		/// <remarks />
		public Span<ushort> Glyphs => new (buffer.glyphs, size);

		/// <summary>Gets the span of positions for each glyph.</summary>
		/// <value>A span containing the positions of type T.</value>
		/// <remarks />
		public Span<T> Positions => new (buffer.pos, posSize);

		/// <summary>Gets the span of UTF-8 encoded source text bytes for the run.</summary>
		/// <value>A span containing the UTF-8 text bytes.</value>
		/// <remarks />
		public Span<byte> Text => new (buffer.utf8text, textSize);

		/// <summary>Gets the span of cluster indices that map glyphs to their source text positions.</summary>
		/// <value>A span containing the cluster indices.</value>
		/// <remarks />
		public Span<uint> Clusters => new (buffer.clusters, size);
	}
}
