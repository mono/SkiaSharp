using System;

namespace SkiaSharp
{
	public unsafe class SKTextBlob : SKObject, ISKNonVirtualReferenceCounted, ISKSkipObjectRegistration
	{
		internal SKTextBlob (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		void ISKNonVirtualReferenceCounted.ReferenceNative () => SkiaApi.sk_textblob_ref (Handle);

		void ISKNonVirtualReferenceCounted.UnreferenceNative () => SkiaApi.sk_textblob_unref (Handle);

		public SKRect Bounds {
			get {
				SKRect bounds;
				SkiaApi.sk_textblob_get_bounds (Handle, &bounds);
				return bounds;
			}
		}

		public uint UniqueId => SkiaApi.sk_textblob_get_unique_id (Handle);

		internal static SKTextBlob GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKTextBlob (handle, true);
	}

	public unsafe class SKTextBlobBuilder : SKObject, ISKSkipObjectRegistration
	{
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
			SKTextBlob.GetObject (SkiaApi.sk_textblob_builder_make (Handle));

		// AddRun

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddRun (font, x, y, glyphs, utf8Text, clusters, null);
		}

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters, SKRect bounds)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddRun (font, x, y, glyphs, utf8Text, clusters, (SKRect?)bounds);
		}

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs) =>
			AddRun (font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, SKRect bounds) =>
			AddRun (font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters) =>
			AddRun (font, x, y, glyphs, text, clusters, null);

		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters, SKRect bounds) =>
			AddRun (font, x, y, glyphs, text, clusters, (SKRect?)bounds);

		// AddRun (spans)

		public void AddRun (SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs) =>
			AddRun (font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		public void AddRun (SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs, SKRect? bounds) =>
			AddRun (font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		public void AddRun (SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters) =>
			AddRun (font, x, y, glyphs, text, clusters, null);

		public void AddRun (SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));
			if (glyphs.IsEmpty)
				throw new ArgumentNullException (nameof (glyphs));

			if (!text.IsEmpty) {
				if (clusters.IsEmpty)
					throw new ArgumentNullException (nameof (clusters));
				if (glyphs.Length != clusters.Length)
					throw new ArgumentException ("The number of glyphs and clusters must be the same.");
			}

			var run = AllocateRun (font, glyphs.Length, x, y, text.IsEmpty ? 0 : text.Length, bounds);
			run.SetGlyphs (glyphs);

			if (!text.IsEmpty) {
				run.SetText (text);
				run.SetClusters (clusters);
			}
		}

		// AddHorizontalRun

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddHorizontalRun (font, y, glyphs, positions, utf8Text, clusters, null);
		}

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters, SKRect bounds)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddHorizontalRun (font, y, glyphs, positions, utf8Text, clusters, (SKRect?)bounds);
		}

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions) =>
			AddHorizontalRun (font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, SKRect bounds) =>
			AddHorizontalRun (font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters) =>
			AddHorizontalRun (font, y, glyphs, positions, text, clusters, null);

		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters, SKRect bounds) =>
			AddHorizontalRun (font, y, glyphs, positions, text, clusters, (SKRect?)bounds);

		// AddHorizontalRun (spans)

		public void AddHorizontalRun (SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions) =>
			AddHorizontalRun (font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		public void AddHorizontalRun (SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions, SKRect? bounds) =>
			AddHorizontalRun (font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		public void AddHorizontalRun (SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters) =>
			AddHorizontalRun (font, y, glyphs, positions, text, clusters, null);

		public void AddHorizontalRun (SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));
			if (glyphs.IsEmpty)
				throw new ArgumentNullException (nameof (glyphs));
			if (positions.IsEmpty)
				throw new ArgumentNullException (nameof (positions));
			if (glyphs.Length != positions.Length)
				throw new ArgumentException ("The number of glyphs and positions must be the same.");

			if (!text.IsEmpty) {
				if (clusters.IsEmpty)
					throw new ArgumentNullException (nameof (clusters));
				if (glyphs.Length != clusters.Length)
					throw new ArgumentException ("The number of glyphs and clusters must be the same.");
			}

			var run = AllocateHorizontalRun (font, glyphs.Length, y, text.IsEmpty ? 0 : text.Length, bounds);
			run.SetGlyphs (glyphs);
			run.SetPositions (positions);

			if (!text.IsEmpty) {
				run.SetText (text);
				run.SetClusters (clusters);
			}
		}

		// AddPositionedRun

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddPositionedRun (font, glyphs, positions, utf8Text, clusters, null);
		}

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters, SKRect bounds)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddPositionedRun (font, glyphs, positions, utf8Text, clusters, (SKRect?)bounds);
		}

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions) =>
			AddPositionedRun (font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, SKRect bounds) =>
			AddPositionedRun (font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters) =>
			AddPositionedRun (font, glyphs, positions, text, clusters, null);

		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters, SKRect bounds) =>
			AddPositionedRun (font, glyphs, positions, text, clusters, (SKRect?)bounds);

		// AddPositionedRun (spans)

		public void AddPositionedRun (SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions) =>
			AddPositionedRun (font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		public void AddPositionedRun (SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions, SKRect? bounds) =>
			AddPositionedRun (font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		public void AddPositionedRun (SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters) =>
			AddPositionedRun (font, glyphs, positions, text, clusters, null);

		public void AddPositionedRun (SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));
			if (glyphs.IsEmpty)
				throw new ArgumentNullException (nameof (glyphs));
			if (positions.IsEmpty)
				throw new ArgumentNullException (nameof (positions));
			if (glyphs.Length != positions.Length)
				throw new ArgumentException ("The number of glyphs and positions must be the same.");

			if (!text.IsEmpty) {
				if (clusters.IsEmpty)
					throw new ArgumentNullException (nameof (clusters));
				if (glyphs.Length != clusters.Length)
					throw new ArgumentException ("The number of glyphs and clusters must be the same.");
			}

			var run = AllocatePositionedRun (font, glyphs.Length, text.IsEmpty ? 0 : text.Length, bounds);
			run.SetGlyphs (glyphs);
			run.SetPositions (positions);

			if (!text.IsEmpty) {
				run.SetText (text);
				run.SetClusters (clusters);
			}
		}

		// AllocateRun

		public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y) =>
			AllocateRun (font, count, x, y, 0, null);

		public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, SKRect? bounds) =>
			AllocateRun (font, count, x, y, 0, bounds);

		public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount) =>
			AllocateRun (font, count, x, y, textByteCount, null);

		public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var originalEncoding = font.TextEncoding;
			try {
				font.TextEncoding = SKTextEncoding.GlyphId;

				SKRunBufferInternal runbuffer;
				if (bounds is SKRect b) {
					SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.Handle, count, x, y, textByteCount, &b, &runbuffer);
				} else {
					SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.Handle, count, x, y, textByteCount, null, &runbuffer);
				}

				return new SKRunBuffer (runbuffer, count, textByteCount);

			} finally {
				font.TextEncoding = originalEncoding;
			}
		}

		// AllocateHorizontalRun

		public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y) =>
			AllocateHorizontalRun (font, count, y, 0, null);

		public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, SKRect? bounds) =>
			AllocateHorizontalRun (font, count, y, 0, bounds);

		public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount) =>
			AllocateHorizontalRun (font, count, y, textByteCount, null);

		public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var originalEncoding = font.TextEncoding;
			try {
				font.TextEncoding = SKTextEncoding.GlyphId;

				SKRunBufferInternal runbuffer;
				if (bounds is SKRect b) {
					SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.Handle, count, y, textByteCount, &b, &runbuffer);
				} else {
					SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.Handle, count, y, textByteCount, null, &runbuffer);
				}

				return new SKHorizontalRunBuffer (runbuffer, count, textByteCount);
			} finally {
				font.TextEncoding = originalEncoding;
			}
		}

		// AllocatePositionedRun

		public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count) =>
			AllocatePositionedRun (font, count, 0, null);

		public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, SKRect? bounds) =>
			AllocatePositionedRun (font, count, 0, bounds);

		public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount) =>
			AllocatePositionedRun (font, count, textByteCount, null);

		public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var originalEncoding = font.TextEncoding;
			try {
				font.TextEncoding = SKTextEncoding.GlyphId;

				SKRunBufferInternal runbuffer;
				if (bounds is SKRect b) {
					SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.Handle, count, textByteCount, &b, &runbuffer);
				} else {
					SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.Handle, count, textByteCount, null, &runbuffer);
				}

				return new SKPositionedRunBuffer (runbuffer, count, textByteCount);
			} finally {
				font.TextEncoding = originalEncoding;
			}
		}
	}
}
