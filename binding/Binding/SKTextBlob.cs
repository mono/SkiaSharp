using System;

namespace SkiaSharp
{
	public unsafe class SKTextBlob : SKObject, ISKNonVirtualReferenceCounted
	{
		[Preserve]
		internal SKTextBlob (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		void ISKNonVirtualReferenceCounted.ReferenceNative () =>
			SkiaApi.sk_textblob_ref (Handle);

		void ISKNonVirtualReferenceCounted.UnreferenceNative () =>
			SkiaApi.sk_textblob_unref (Handle);

		public SKRect Bounds {
			get {
				SKRect bounds;
				SkiaApi.sk_textblob_get_bounds (Handle, &bounds);
				return bounds;
			}
		}

		public uint UniqueId =>
			SkiaApi.sk_textblob_get_unique_id (Handle);

		// GetIntercepts

		public float[] GetIntercepts (float upperBounds, float lowerBounds, SKPaint paint = null)
		{
			var n = CountIntercepts (upperBounds, lowerBounds, paint);
			var intervals = new float[n];
			GetIntercepts (upperBounds, lowerBounds, intervals, paint);
			return intervals;
		}

		public void GetIntercepts (float upperBounds, float lowerBounds, Span<float> intervals, SKPaint paint = null)
		{
			var bounds = stackalloc float[2];
			bounds[0] = upperBounds;
			bounds[1] = lowerBounds;
			fixed (float* i = intervals) {
				SkiaApi.sk_textblob_get_intercepts (Handle, bounds, i, paint?.Handle ?? IntPtr.Zero);
			}
		}

		// CountIntercepts

		public int CountIntercepts (float upperBounds, float lowerBounds, SKPaint paint = null)
		{
			var bounds = stackalloc float[2];
			bounds[0] = upperBounds;
			bounds[1] = lowerBounds;
			return SkiaApi.sk_textblob_get_intercepts (Handle, bounds, null, paint?.Handle ?? IntPtr.Zero);
		}
	}

	public unsafe class SKTextBlobBuilder : SKObject
	{
		[Preserve]
		internal SKTextBlobBuilder (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		public SKTextBlobBuilder ()
			: this (SkiaApi.sk_textblob_builder_new (), true)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_textblob_builder_delete (Handle);

		// Build

		public SKTextBlob Build () =>
			GetObject<SKTextBlob> (SkiaApi.sk_textblob_builder_make (Handle));

		// AddRun

		public void AddRun (string text, SKFont font) =>
			AddRun (text.AsSpan (), font, 0, 0);

		public void AddRun (string text, SKFont font, float x, float y) =>
			AddRun (text.AsSpan (), font, x, y);

		public void AddRun (ReadOnlySpan<char> text, SKFont font) =>
			AddRun (text, font, 0, 0);

		public void AddRun (ReadOnlySpan<char> text, SKFont font, float x, float y)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text);
			var buffer = AllocatePositionedRun (font, count);
			font.GetGlyphs (text, buffer.GetGlyphSpan ());
			font.GetGlyphPositions (buffer.GetGlyphSpan (), buffer.GetPositionSpan (), new SKPoint (x, y));
		}

		public void AddRun (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font) =>
			AddRun (text, encoding, font, 0, 0);

		public void AddRun (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, float x, float y)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, encoding);
			var buffer = AllocatePositionedRun (font, count);
			font.GetGlyphs (text, encoding, buffer.GetGlyphSpan ());
			font.GetGlyphPositions (buffer.GetGlyphSpan (), buffer.GetPositionSpan (), new SKPoint (x, y));
		}

		public void AddRun (ReadOnlySpan<ushort> glyphs, SKFont font) =>
			AddRun (glyphs, font, 0, 0);

		public void AddRun (ReadOnlySpan<ushort> glyphs, SKFont font, float x, float y)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var buffer = AllocatePositionedRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.GetGlyphSpan ());
			font.GetGlyphPositions (buffer.GetGlyphSpan (), buffer.GetPositionSpan (), new SKPoint (x, y));
		}

		// AddHorizontalRun

		public void AddHorizontalRun (string text, SKFont font, ReadOnlySpan<float> positions, float y) =>
			AddHorizontalRun (text.AsSpan (), font, positions, y);

		public void AddHorizontalRun (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text);
			var buffer = AllocateHorizontalRun (font, count, y);
			font.GetGlyphs (text, buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
		}

		public void AddHorizontalRun (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, encoding);
			var buffer = AllocateHorizontalRun (font, count, y);
			font.GetGlyphs (text, encoding, buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
		}

		public void AddHorizontalRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var buffer = AllocateHorizontalRun (font, glyphs.Length, y);
			glyphs.CopyTo (buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
		}

		// AddPositionedRun

		public void AddPositionedRun (string text, SKFont font, ReadOnlySpan<SKPoint> positions) =>
			AddPositionedRun (text.AsSpan (), font, positions);

		public void AddPositionedRun (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text);
			var buffer = AllocatePositionedRun (font, count);
			font.GetGlyphs (text, buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
		}

		public void AddPositionedRun (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, encoding);
			var buffer = AllocatePositionedRun (font, count);
			font.GetGlyphs (text, encoding, buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
		}

		// AllocateRun

		public SKRunBuffer AllocateRun (SKFont font, int count, float x, float y) =>
			AllocateRun (font, count, x, y, null);

		public SKRunBuffer AllocateRun (SKFont font, int count, float x, float y, SKRect bounds) =>
			AllocateRun (font, count, x, y, &bounds);

		private SKRunBuffer AllocateRun (SKFont font, int count, float x, float y, SKRect* bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			SkiaApi.sk_textblob_builder_alloc_run (Handle, font.Handle, count, x, y, bounds, &runbuffer);

			return new SKRunBuffer (runbuffer, count);
		}

		// AllocateHorizontalRun

		public SKHorizontalRunBuffer AllocateHorizontalRun (SKFont font, int count, float y) =>
			AllocateHorizontalRun (font, count, y, null);

		public SKHorizontalRunBuffer AllocateHorizontalRun (SKFont font, int count, float y, SKRect bounds) =>
			AllocateHorizontalRun (font, count, y, &bounds);

		private SKHorizontalRunBuffer AllocateHorizontalRun (SKFont font, int count, float y, SKRect* bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			SkiaApi.sk_textblob_builder_alloc_run_pos_h (Handle, font.Handle, count, y, bounds, &runbuffer);

			return new SKHorizontalRunBuffer (runbuffer, count);
		}

		// AllocatePositionedRun

		public SKPositionedRunBuffer AllocatePositionedRun (SKFont font, int count) =>
			AllocatePositionedRun (font, count, null);

		public SKPositionedRunBuffer AllocatePositionedRun (SKFont font, int count, SKRect bounds) =>
			AllocatePositionedRun (font, count, &bounds);

		private SKPositionedRunBuffer AllocatePositionedRun (SKFont font, int count, SKRect* bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			SkiaApi.sk_textblob_builder_alloc_run_pos (Handle, font.Handle, count, bounds, &runbuffer);

			return new SKPositionedRunBuffer (runbuffer, count);
		}
	}
}
