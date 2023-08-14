#nullable disable

using System;
using System.ComponentModel;

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

		// Create

		public static SKTextBlob Create (string text, SKFont font, SKPoint origin = default) =>
			Create (text.AsSpan (), font, origin);

		public static SKTextBlob Create (ReadOnlySpan<char> text, SKFont font, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return Create (t, text.Length * 2, SKTextEncoding.Utf16, font, origin);
			}
		}

		public static SKTextBlob Create (IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPoint origin = default) =>
			Create (text.AsReadOnlySpan (length), encoding, font, origin);

		public static SKTextBlob Create (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return Create (t, text.Length, encoding, font, origin);
			}
		}

		internal static SKTextBlob Create (void* text, int length, SKTextEncoding encoding, SKFont font, SKPoint origin)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			if (count <= 0)
				return null;

			using var builder = new SKTextBlobBuilder ();
			var buffer = builder.AllocatePositionedRun (font, count);
			font.GetGlyphs (text, length, encoding, buffer.GetGlyphSpan ());
			font.GetGlyphPositions (buffer.GetGlyphSpan (), buffer.GetPositionSpan (), origin);
			return builder.Build ();
		}

		// CreateHorizontal

		public static SKTextBlob CreateHorizontal (string text, SKFont font, ReadOnlySpan<float> positions, float y) =>
			CreateHorizontal (text.AsSpan (), font, positions, y);

		public static SKTextBlob CreateHorizontal (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			fixed (void* t = text) {
				return CreateHorizontal (t, text.Length * 2, SKTextEncoding.Utf16, font, positions, y);
			}
		}

		public static SKTextBlob CreateHorizontal (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y) =>
			CreateHorizontal (text.AsReadOnlySpan (length), encoding, font, positions, y);

		public static SKTextBlob CreateHorizontal (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			fixed (void* t = text) {
				return CreateHorizontal (t, text.Length, encoding, font, positions, y);
			}
		}

		internal static SKTextBlob CreateHorizontal (void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			if (count <= 0)
				return null;

			using var builder = new SKTextBlobBuilder ();
			var buffer = builder.AllocateHorizontalRun (font, count, y);
			font.GetGlyphs (text, length, encoding, buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
			return builder.Build ();
		}

		// CreatePositioned

		public static SKTextBlob CreatePositioned (string text, SKFont font, ReadOnlySpan<SKPoint> positions) =>
			CreatePositioned (text.AsSpan (), font, positions);

		public static SKTextBlob CreatePositioned (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			fixed (void* t = text) {
				return CreatePositioned (t, text.Length * 2, SKTextEncoding.Utf16, font, positions);
			}
		}

		public static SKTextBlob CreatePositioned (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions) =>
			CreatePositioned (text.AsReadOnlySpan (length), encoding, font, positions);

		public static SKTextBlob CreatePositioned (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			fixed (void* t = text) {
				return CreatePositioned (t, text.Length, encoding, font, positions);
			}
		}

		internal static SKTextBlob CreatePositioned (void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			if (count <= 0)
				return null;

			using var builder = new SKTextBlobBuilder ();
			var buffer = builder.AllocatePositionedRun (font, count);
			font.GetGlyphs (text, length, encoding, buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
			return builder.Build ();
		}

		// CreateRotationScale

		public static SKTextBlob CreateRotationScale (string text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			CreateRotationScale (text.AsSpan (), font, positions);

		public static SKTextBlob CreateRotationScale (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			fixed (void* t = text) {
				return CreateRotationScale (t, text.Length * 2, SKTextEncoding.Utf16, font, positions);
			}
		}

		public static SKTextBlob CreateRotationScale (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			CreateRotationScale (text.AsReadOnlySpan (length), encoding, font, positions);

		public static SKTextBlob CreateRotationScale (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			fixed (void* t = text) {
				return CreateRotationScale (t, text.Length, encoding, font, positions);
			}
		}

		internal static SKTextBlob CreateRotationScale (void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			if (count <= 0)
				return null;

			using var builder = new SKTextBlobBuilder ();
			var buffer = builder.AllocateRotationScaleRun (font, count);
			font.GetGlyphs (text, length, encoding, buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetRotationScaleSpan ());
			return builder.Build ();
		}

		// CreatePathPositioned

		public static SKTextBlob CreatePathPositioned (string text, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default) =>
			CreatePathPositioned (text.AsSpan (), font, path, textAlign, origin);

		public static SKTextBlob CreatePathPositioned (ReadOnlySpan<char> text, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return CreatePathPositioned (t, text.Length * 2, SKTextEncoding.Utf16, font, path, textAlign, origin);
			}
		}

		public static SKTextBlob CreatePathPositioned (IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default) =>
			CreatePathPositioned (text.AsReadOnlySpan (length), encoding, font, path, textAlign, origin);

		public static SKTextBlob CreatePathPositioned (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return CreatePathPositioned (t, text.Length, encoding, font, path, textAlign, origin);
			}
		}

		internal static SKTextBlob CreatePathPositioned (void* text, int length, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			if (count <= 0)
				return null;

			// we use temporary arrays because we might only use part of the text
			using var glyphs = Utils.RentArray<ushort> (count);
			using var glyphWidths = Utils.RentArray<float> (glyphs.Length);
			using var glyphOffsets = Utils.RentArray<SKPoint> (glyphs.Length);

			font.GetGlyphs (text, length, encoding, glyphs);
			font.GetGlyphWidths (glyphs, glyphWidths, Span<SKRect>.Empty);
			font.GetGlyphPositions (glyphs, glyphOffsets, origin);

			using var builder = new SKTextBlobBuilder ();
			builder.AddPathPositionedRun (glyphs, font, glyphWidths, glyphOffsets, path, textAlign);
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

		//

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

		public SKTextBlob Build ()
		{
			var blob = SKTextBlob.GetObject (SkiaApi.sk_textblob_builder_make (Handle));
			GC.KeepAlive (this);
			return blob;
		}

		// AddRun

		public void AddRun (ReadOnlySpan<ushort> glyphs, SKFont font, SKPoint origin = default)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var buffer = AllocatePositionedRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.GetGlyphSpan ());
			font.GetGlyphPositions (buffer.GetGlyphSpan (), buffer.GetPositionSpan (), origin);
		}

		// AddHorizontalRun

		public void AddHorizontalRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var buffer = AllocateHorizontalRun (font, glyphs.Length, y);
			glyphs.CopyTo (buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
		}

		// AddPositionedRun

		public void AddPositionedRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var buffer = AllocatePositionedRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetPositionSpan ());
		}

		// AddRotationScaleRun 

		public void AddRotationScaleRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var buffer = AllocateRotationScaleRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.GetGlyphSpan ());
			positions.CopyTo (buffer.GetRotationScaleSpan ());
		}

		// AddPathPositionedRun

		public void AddPathPositionedRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<float> glyphWidths, ReadOnlySpan<SKPoint> glyphOffsets, SKPath path, SKTextAlign textAlign = SKTextAlign.Left)
		{
			using var pathMeasure = new SKPathMeasure (path);

			var contourLength = pathMeasure.Length;

			var textLength = glyphOffsets[glyphs.Length - 1].X + glyphWidths[glyphs.Length - 1];
			var alignment = (int)textAlign * 0.5f;
			var startOffset = glyphOffsets[0].X + (contourLength - textLength) * alignment;

			var firstGlyphIndex = 0;
			var pathGlyphCount = 0;

			using var glyphTransforms = Utils.RentArray<SKRotationScaleMatrix> (glyphs.Length);

			// TODO: deal with multiple contours?
			for (var index = 0; index < glyphOffsets.Length; index++) {
				var glyphOffset = glyphOffsets[index];
				var halfWidth = glyphWidths[index] * 0.5f;
				var pathOffset = startOffset + glyphOffset.X + halfWidth;

				// TODO: clip glyphs on both ends of paths
				if (pathOffset >= 0 && pathOffset < contourLength && pathMeasure.GetPositionAndTangent (pathOffset, out var position, out var tangent)) {
					if (pathGlyphCount == 0)
						firstGlyphIndex = index;

					var tx = tangent.X;
					var ty = tangent.Y;

					var px = position.X;
					var py = position.Y;

					// horizontally offset the position using the tangent vector
					px -= tx * halfWidth;
					py -= ty * halfWidth;

					// vertically offset the position using the normal vector  (-ty, tx)
					var dy = glyphOffset.Y;
					px -= dy * ty;
					py += dy * tx;

					glyphTransforms.Span[pathGlyphCount++] = new SKRotationScaleMatrix (tx, ty, px, py);
				}
			}

			var glyphSubset = glyphs.Slice (firstGlyphIndex, pathGlyphCount);
			var positions = glyphTransforms.Span.Slice (0, pathGlyphCount);

			AddRotationScaleRun (glyphSubset, font, positions);
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

		public SKTextRunBuffer AllocateTextRun (SKFont font, int count, float x, float y, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.Handle, count, x, y, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.Handle, count, x, y, textByteCount, null, &runbuffer);

			return new SKTextRunBuffer (runbuffer, count, textByteCount);
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

		public SKHorizontalTextRunBuffer AllocateHorizontalTextRun (SKFont font, int count, float y, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.Handle, count, y, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.Handle, count, y, textByteCount, null, &runbuffer);

			return new SKHorizontalTextRunBuffer (runbuffer, count, textByteCount);
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

		public SKPositionedTextRunBuffer AllocatePositionedTextRun (SKFont font, int count, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.Handle, count, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.Handle, count, textByteCount, null, &runbuffer);

			return new SKPositionedTextRunBuffer (runbuffer, count, textByteCount);
		}

		// AllocateRotationScaleRun

		public SKRotationScaleRunBuffer AllocateRotationScaleRun (SKFont font, int count, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_rsxform (Handle, font.Handle, count, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_rsxform (Handle, font.Handle, count, null, &runbuffer);

			return new SKRotationScaleRunBuffer (runbuffer, count);
		}

		public SKRotationScaleRunBuffer AllocateRotationScaleTextRun (SKFont font, int count, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text_rsxform (Handle, font.Handle, count, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text_rsxform (Handle, font.Handle, count, textByteCount, null, &runbuffer);

			return new SKRotationScaleRunBuffer (runbuffer, count);
		}
	}
}
