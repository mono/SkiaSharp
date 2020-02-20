using System;
using System.ComponentModel;

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

		// Create

		public static SKTextBlob Create (string text, SKFont font, SKPoint origin = default) =>
			Create (text.AsSpan (), font, origin);

		public static SKTextBlob Create (ReadOnlySpan<char> text, SKFont font, SKPoint origin = default)
		{
			using var builder = new SKTextBlobBuilder ();
			builder.AddRun (text, font, origin);
			return builder.Build ();
		}

		public static SKTextBlob Create (IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPoint origin = default) =>
			Create (text.AsReadOnlySpan (length), encoding, font, origin);

		public static SKTextBlob Create (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, SKPoint origin = default)
		{
			using var builder = new SKTextBlobBuilder ();
			builder.AddRun (text, encoding, font, origin);
			return builder.Build ();
		}

		// CreateHorizontal

		public static SKTextBlob CreateHorizontal (string text, SKFont font, ReadOnlySpan<float> positions, float y) =>
			CreateHorizontal (text.AsSpan (), font, positions, y);

		public static SKTextBlob CreateHorizontal (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			using var builder = new SKTextBlobBuilder ();
			builder.AddHorizontalRun (text, font, positions, y);
			return builder.Build ();
		}

		public static SKTextBlob CreateHorizontal (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y) =>
			CreateHorizontal (text.AsReadOnlySpan (length), encoding, font, positions, y);

		public static SKTextBlob CreateHorizontal (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			using var builder = new SKTextBlobBuilder ();
			builder.AddHorizontalRun (text, encoding, font, positions, y);
			return builder.Build ();
		}

		// CreatePositioned

		public static SKTextBlob CreatePositioned (string text, SKFont font, ReadOnlySpan<SKPoint> positions) =>
			CreatePositioned (text.AsSpan (), font, positions);

		public static SKTextBlob CreatePositioned (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			using var builder = new SKTextBlobBuilder ();
			builder.AddPositionedRun (text, font, positions);
			return builder.Build ();
		}

		public static SKTextBlob CreatePositioned (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions) =>
			CreatePositioned (text.AsReadOnlySpan (length), encoding, font, positions);

		public static SKTextBlob CreatePositioned (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			using var builder = new SKTextBlobBuilder ();
			builder.AddPositionedRun (text, encoding, font, positions);
			return builder.Build ();
		}

		// CreateRotationScale

		public static SKTextBlob CreateRotationScale (string text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			CreateRotationScale (text.AsSpan (), font, positions);

		public static SKTextBlob CreateRotationScale (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			using var builder = new SKTextBlobBuilder ();
			builder.AddRotationScaleRun (text, font, positions);
			return builder.Build ();
		}

		public static SKTextBlob CreateRotationScale (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			CreateRotationScale (text.AsReadOnlySpan (length), encoding, font, positions);

		public static SKTextBlob CreateRotationScale (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			using var builder = new SKTextBlobBuilder ();
			builder.AddRotationScaleRun (text, encoding, font, positions);
			return builder.Build ();
		}

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

		// AddRun (text)

		public void AddRun (string text, SKFont font, SKPoint origin = default) =>
			AddRun (text.AsSpan (), font, origin);

		public void AddRun (ReadOnlySpan<char> text, SKFont font, SKPoint origin = default)
		{
			fixed (void* t = text) {
				AddRun (t, text.Length, SKTextEncoding.Utf16, font, origin);
			}
		}

		public void AddRun (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, SKPoint origin = default)
		{
			fixed (void* t = text) {
				AddRun (t, text.Length, encoding, font, origin);
			}
		}

		public void AddRun (IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPoint origin = default) =>
			AddRun ((void*)text, length, encoding, font, origin);

		internal void AddRun (void* text, int length, SKTextEncoding encoding, SKFont font, SKPoint origin = default)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			var buffer = AllocatePositionedRun (font, count);
			font.GetGlyphs (text, length, encoding, buffer.GetGlyphSpan ());
			font.GetGlyphPositions (buffer.GetGlyphSpan (), buffer.GetPositionSpan (), origin);
		}

		// AddRun (glyphs)

		public void AddRun (ReadOnlySpan<ushort> glyphs, SKFont font, SKPoint origin = default)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var buffer = AllocatePositionedRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.GetGlyphSpan ());
			font.GetGlyphPositions (buffer.GetGlyphSpan (), buffer.GetPositionSpan (), origin);
		}

		// AddHorizontalRun (text)

		public void AddHorizontalRun (string text, SKFont font, ReadOnlySpan<float> positions, float y) =>
			AddHorizontalRun (text.AsSpan (), font, positions, y);

		public void AddHorizontalRun (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			fixed (void* t = text) {
				AddHorizontalRun (t, text.Length, SKTextEncoding.Utf16, font, positions, y);
			}
		}

		public void AddHorizontalRun (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			fixed (void* t = text) {
				AddHorizontalRun (t, text.Length, encoding, font, positions, y);
			}
		}

		public void AddHorizontalRun (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y) =>
			AddHorizontalRun ((void*)text, length, encoding, font, positions, y);

		internal void AddHorizontalRun (void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			var buffer = AllocateHorizontalRun (font, count, y);
			font.GetGlyphs (text, length, encoding, buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
		}

		// AddHorizontalRun (glyphs)

		public void AddHorizontalRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var buffer = AllocateHorizontalRun (font, glyphs.Length, y);
			glyphs.CopyTo (buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
		}

		// AddPositionedRun (text)

		public void AddPositionedRun (string text, SKFont font, ReadOnlySpan<SKPoint> positions) =>
			AddPositionedRun (text.AsSpan (), font, positions);

		public void AddPositionedRun (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			fixed (void* t = text) {
				AddPositionedRun (t, text.Length, SKTextEncoding.Utf16, font, positions);
			}
		}

		public void AddPositionedRun (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			fixed (void* t = text) {
				AddPositionedRun (t, text.Length, encoding, font, positions);
			}
		}

		public void AddPositionedRun (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions) =>
			AddPositionedRun ((void*)text, length, encoding, font, positions);

		internal void AddPositionedRun (void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			var buffer = AllocatePositionedRun (font, count);
			font.GetGlyphs (text, length, encoding, buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
		}

		// AddPositionedRun (glyphs)

		public void AddPositionedRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var buffer = AllocatePositionedRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
		}

		// AddRotationScaleRun (text)

		public void AddRotationScaleRun (string text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			AddRotationScaleRun (text.AsSpan (), font, positions);

		public void AddRotationScaleRun (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			fixed (void* t = text) {
				AddRotationScaleRun (t, text.Length, SKTextEncoding.Utf16, font, positions);
			}
		}

		public void AddRotationScaleRun (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			fixed (void* t = text) {
				AddRotationScaleRun (t, text.Length, encoding, font, positions);
			}
		}

		public void AddRotationScaleRun (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			AddRotationScaleRun ((void*)text, length, encoding, font, positions);

		internal void AddRotationScaleRun (void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			var buffer = AllocateRotationScaleRun (font, count);
			font.GetGlyphs (text, length, encoding, buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetRotationScaleSpan ());
		}

		// AddRotationScaleRun (text)

		public void AddRotationScaleRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var buffer = AllocateRotationScaleRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetRotationScaleSpan ());
		}

		// AllocateRun

		public SKRunBuffer AllocateRun (SKFont font, int count, float x, float y, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run (Handle, font.Handle, count, x, y, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run (Handle, font.Handle, count, x, y, null, &runbuffer);

			return new SKRunBuffer (runbuffer, count);
		}

		// AllocateHorizontalRun

		public SKHorizontalRunBuffer AllocateHorizontalRun (SKFont font, int count, float y, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_pos_h (Handle, font.Handle, count, y, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_pos_h (Handle, font.Handle, count, y, null, &runbuffer);

			return new SKHorizontalRunBuffer (runbuffer, count);
		}

		// AllocatePositionedRun

		public SKPositionedRunBuffer AllocatePositionedRun (SKFont font, int count, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_pos (Handle, font.Handle, count, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_pos (Handle, font.Handle, count, null, &runbuffer);

			return new SKPositionedRunBuffer (runbuffer, count);
		}

		// AllocateRotationScaleRun

		public SKRotationScaleRunBuffer AllocateRotationScaleRun (SKFont font, int count)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			SkiaApi.sk_textblob_builder_alloc_run_rsxform (Handle, font.Handle, count, &runbuffer);

			return new SKRotationScaleRunBuffer (runbuffer, count);
		}

		// OBSOLETE OVERLOADS

		// AddRun

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddRun (font, x, y, glyphs, utf8Text, clusters, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, string text, uint[] clusters, SKRect bounds)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddRun (font, x, y, glyphs, utf8Text, clusters, (SKRect?)bounds);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs) =>
			AddRun (font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, SKRect bounds) =>
			AddRun (font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters) =>
			AddRun (font, x, y, glyphs, text, clusters, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
		public void AddRun (SKPaint font, float x, float y, ushort[] glyphs, byte[] text, uint[] clusters, SKRect bounds) =>
			AddRun (font, x, y, glyphs, text, clusters, (SKRect?)bounds);

		// AddRun (spans)

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
		public void AddRun (SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs) =>
			AddRun (font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
		public void AddRun (SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs, SKRect? bounds) =>
			AddRun (font, x, y, glyphs, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
		public void AddRun (SKPaint font, float x, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters) =>
			AddRun (font, x, y, glyphs, text, clusters, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddRun (ReadOnlySpan<ushort>, SKFont, float, float) instead.")]
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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddHorizontalRun (font, y, glyphs, positions, utf8Text, clusters, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, string text, uint[] clusters, SKRect bounds)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddHorizontalRun (font, y, glyphs, positions, utf8Text, clusters, (SKRect?)bounds);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions) =>
			AddHorizontalRun (font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, SKRect bounds) =>
			AddHorizontalRun (font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters) =>
			AddHorizontalRun (font, y, glyphs, positions, text, clusters, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
		public void AddHorizontalRun (SKPaint font, float y, ushort[] glyphs, float[] positions, byte[] text, uint[] clusters, SKRect bounds) =>
			AddHorizontalRun (font, y, glyphs, positions, text, clusters, (SKRect?)bounds);

		// AddHorizontalRun (spans)

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
		public void AddHorizontalRun (SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions) =>
			AddHorizontalRun (font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
		public void AddHorizontalRun (SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions, SKRect? bounds) =>
			AddHorizontalRun (font, y, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
		public void AddHorizontalRun (SKPaint font, float y, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters) =>
			AddHorizontalRun (font, y, glyphs, positions, text, clusters, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddHorizontalRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<float>, float) instead.")]
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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddPositionedRun (font, glyphs, positions, utf8Text, clusters, null);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, string text, uint[] clusters, SKRect bounds)
		{
			var utf8Text = StringUtilities.GetEncodedText (text, SKTextEncoding.Utf8);
			AddPositionedRun (font, glyphs, positions, utf8Text, clusters, (SKRect?)bounds);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions) =>
			AddPositionedRun (font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, SKRect bounds) =>
			AddPositionedRun (font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters) =>
			AddPositionedRun (font, glyphs, positions, text, clusters, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
		public void AddPositionedRun (SKPaint font, ushort[] glyphs, SKPoint[] positions, byte[] text, uint[] clusters, SKRect bounds) =>
			AddPositionedRun (font, glyphs, positions, text, clusters, (SKRect?)bounds);

		// AddPositionedRun (spans)

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
		public void AddPositionedRun (SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions) =>
			AddPositionedRun (font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
		public void AddPositionedRun (SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions, SKRect? bounds) =>
			AddPositionedRun (font, glyphs, positions, ReadOnlySpan<byte>.Empty, ReadOnlySpan<uint>.Empty, bounds);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
		public void AddPositionedRun (SKPaint font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<SKPoint> positions, ReadOnlySpan<byte> text, ReadOnlySpan<uint> clusters) =>
			AddPositionedRun (font, glyphs, positions, text, clusters, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AddPositionedRun (ReadOnlySpan<ushort>, SKFont, ReadOnlySpan<SKPoint>) instead.")]
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

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
		public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y) =>
			AllocateRun (font, count, x, y, 0, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
		public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, SKRect? bounds) =>
			AllocateRun (font, count, x, y, 0, bounds);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
		public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount) =>
			AllocateRun (font, count, x, y, textByteCount, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocateRun (SKFont, int, float, float, SKRect?) instead.")]
		public SKRunBuffer AllocateRun (SKPaint font, int count, float x, float y, int textByteCount, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			// backwards compat
			if (font.TextEncoding != SKTextEncoding.GlyphId)
				return new SKRunBuffer (new SKRunBufferInternal (), count, textByteCount);

			using (var lang = new SKString ()) {
				SKRunBufferInternal runbuffer;
				if (bounds is SKRect b) {
					SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.GetFont ().Handle, count, x, y, textByteCount, lang.Handle, &b, &runbuffer);
				} else {
					SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.GetFont ().Handle, count, x, y, textByteCount, lang.Handle, null, &runbuffer);
				}

				return new SKRunBuffer (runbuffer, count, textByteCount);
			}
		}

		// AllocateHorizontalRun

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
		public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y) =>
			AllocateHorizontalRun (font, count, y, 0, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
		public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, SKRect? bounds) =>
			AllocateHorizontalRun (font, count, y, 0, bounds);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
		public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount) =>
			AllocateHorizontalRun (font, count, y, textByteCount, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocateHorizontalRun (SKFont, int, float, SKRect?) instead.")]
		public SKHorizontalRunBuffer AllocateHorizontalRun (SKPaint font, int count, float y, int textByteCount, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			// backwards compat
			if (font.TextEncoding != SKTextEncoding.GlyphId)
				return new SKHorizontalRunBuffer (new SKRunBufferInternal (), count, textByteCount);

			using (var lang = new SKString ()) {
				SKRunBufferInternal runbuffer;
				if (bounds is SKRect b) {
					SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.GetFont ().Handle, count, y, textByteCount, lang.Handle, &b, &runbuffer);
				} else {
					SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.GetFont ().Handle, count, y, textByteCount, lang.Handle, null, &runbuffer);
				}

				return new SKHorizontalRunBuffer (runbuffer, count, textByteCount);
			}
		}

		// AllocatePositionedRun

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
		public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count) =>
			AllocatePositionedRun (font, count, 0, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
		public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, SKRect? bounds) =>
			AllocatePositionedRun (font, count, 0, bounds);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
		public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount) =>
			AllocatePositionedRun (font, count, textByteCount, null);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use AllocatePositionedRun (SKFont, int, SKRect?) instead.")]
		public SKPositionedRunBuffer AllocatePositionedRun (SKPaint font, int count, int textByteCount, SKRect? bounds)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			// backwards compat
			if (font.TextEncoding != SKTextEncoding.GlyphId)
				return new SKPositionedRunBuffer (new SKRunBufferInternal (), count, textByteCount);

			using (var lang = new SKString ()) {
				SKRunBufferInternal runbuffer;
				if (bounds is SKRect b) {
					SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.GetFont ().Handle, count, textByteCount, lang.Handle, &b, &runbuffer);
				} else {
					SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.GetFont ().Handle, count, textByteCount, lang.Handle, null, &runbuffer);
				}

				return new SKPositionedRunBuffer (runbuffer, count, textByteCount);
			}
		}
	}
}
