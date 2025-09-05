using System;

namespace SkiaSharp
{
	/// <summary>
	/// Represents multiple text runs of glyphs and positions.
	/// </summary>
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

		/// <summary>
		/// Gets the conservative blob bounding box.
		/// </summary>
		public SKRect Bounds {
			get {
				SKRect bounds;
				SkiaApi.sk_textblob_get_bounds (Handle, &bounds);
				return bounds;
			}
		}

		/// <summary>
		/// Gets the unique, non-zero value representing the text blob.
		/// </summary>
		public uint UniqueId => SkiaApi.sk_textblob_get_unique_id (Handle);

		// Create

		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <param name="origin"></param>
		public static SKTextBlob? Create (string text, SKFont font, SKPoint origin = default) =>
			Create (text.AsSpan (), font, origin);

		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <param name="origin"></param>
		public static SKTextBlob? Create (ReadOnlySpan<char> text, SKFont font, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return Create (t, text.Length * 2, SKTextEncoding.Utf16, font, origin);
			}
		}

		/// <param name="text"></param>
		/// <param name="length"></param>
		/// <param name="encoding"></param>
		/// <param name="font"></param>
		/// <param name="origin"></param>
		public static SKTextBlob? Create (IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPoint origin = default) =>
			Create (text.AsReadOnlySpan (length), encoding, font, origin);

		/// <param name="text"></param>
		/// <param name="encoding"></param>
		/// <param name="font"></param>
		/// <param name="origin"></param>
		public static SKTextBlob? Create (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return Create (t, text.Length, encoding, font, origin);
			}
		}

		internal static SKTextBlob? Create (void* text, int length, SKTextEncoding encoding, SKFont font, SKPoint origin)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			if (count <= 0)
				return null;

			using var builder = new SKTextBlobBuilder ();
			var buffer = builder.AllocateRawPositionedRun (font, count);
			font.GetGlyphs (text, length, encoding, buffer.Glyphs);
			font.GetGlyphPositions (buffer.Glyphs, buffer.Positions, origin);
			return builder.Build ();
		}

		// CreateHorizontal

		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		/// <param name="y"></param>
		public static SKTextBlob? CreateHorizontal (string text, SKFont font, ReadOnlySpan<float> positions, float y) =>
			CreateHorizontal (text.AsSpan (), font, positions, y);

		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		/// <param name="y"></param>
		public static SKTextBlob? CreateHorizontal (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			fixed (void* t = text) {
				return CreateHorizontal (t, text.Length * 2, SKTextEncoding.Utf16, font, positions, y);
			}
		}

		/// <param name="text"></param>
		/// <param name="length"></param>
		/// <param name="encoding"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		/// <param name="y"></param>
		public static SKTextBlob? CreateHorizontal (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y) =>
			CreateHorizontal (text.AsReadOnlySpan (length), encoding, font, positions, y);

		/// <param name="text"></param>
		/// <param name="encoding"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		/// <param name="y"></param>
		public static SKTextBlob? CreateHorizontal (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			fixed (void* t = text) {
				return CreateHorizontal (t, text.Length, encoding, font, positions, y);
			}
		}

		internal static SKTextBlob? CreateHorizontal (void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			if (count <= 0)
				return null;

			using var builder = new SKTextBlobBuilder ();
			var buffer = builder.AllocateRawHorizontalRun (font, count, y);
			font.GetGlyphs (text, length, encoding, buffer.Glyphs);
			positions.CopyTo (buffer.Positions);
			return builder.Build ();
		}

		// CreatePositioned

		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		public static SKTextBlob? CreatePositioned (string text, SKFont font, ReadOnlySpan<SKPoint> positions) =>
			CreatePositioned (text.AsSpan (), font, positions);

		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		public static SKTextBlob? CreatePositioned (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			fixed (void* t = text) {
				return CreatePositioned (t, text.Length * 2, SKTextEncoding.Utf16, font, positions);
			}
		}

		/// <param name="text"></param>
		/// <param name="length"></param>
		/// <param name="encoding"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		public static SKTextBlob? CreatePositioned (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions) =>
			CreatePositioned (text.AsReadOnlySpan (length), encoding, font, positions);

		/// <param name="text"></param>
		/// <param name="encoding"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		public static SKTextBlob? CreatePositioned (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			fixed (void* t = text) {
				return CreatePositioned (t, text.Length, encoding, font, positions);
			}
		}

		internal static SKTextBlob? CreatePositioned (void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			if (count <= 0)
				return null;

			using var builder = new SKTextBlobBuilder ();
			var buffer = builder.AllocateRawPositionedRun (font, count);
			font.GetGlyphs (text, length, encoding, buffer.Glyphs);
			positions.CopyTo (buffer.Positions);
			return builder.Build ();
		}

		// CreateRotationScale

		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		public static SKTextBlob? CreateRotationScale (string text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			CreateRotationScale (text.AsSpan (), font, positions);

		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		public static SKTextBlob? CreateRotationScale (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			fixed (void* t = text) {
				return CreateRotationScale (t, text.Length * 2, SKTextEncoding.Utf16, font, positions);
			}
		}

		/// <param name="text"></param>
		/// <param name="length"></param>
		/// <param name="encoding"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		public static SKTextBlob? CreateRotationScale (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			CreateRotationScale (text.AsReadOnlySpan (length), encoding, font, positions);

		/// <param name="text"></param>
		/// <param name="encoding"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		public static SKTextBlob? CreateRotationScale (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			fixed (void* t = text) {
				return CreateRotationScale (t, text.Length, encoding, font, positions);
			}
		}

		internal static SKTextBlob? CreateRotationScale (void* text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			var count = font.CountGlyphs (text, length, encoding);
			if (count <= 0)
				return null;

			using var builder = new SKTextBlobBuilder ();
			var buffer = builder.AllocateRotationScaleRun (font, count);
			font.GetGlyphs (text, length, encoding, buffer.Glyphs);
			positions.CopyTo (buffer.Positions);
			return builder.Build ();
		}

		// CreatePathPositioned

		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <param name="path"></param>
		/// <param name="textAlign"></param>
		/// <param name="origin"></param>
		public static SKTextBlob? CreatePathPositioned (string text, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default) =>
			CreatePathPositioned (text.AsSpan (), font, path, textAlign, origin);

		/// <param name="text"></param>
		/// <param name="font"></param>
		/// <param name="path"></param>
		/// <param name="textAlign"></param>
		/// <param name="origin"></param>
		public static SKTextBlob? CreatePathPositioned (ReadOnlySpan<char> text, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return CreatePathPositioned (t, text.Length * 2, SKTextEncoding.Utf16, font, path, textAlign, origin);
			}
		}

		/// <param name="text"></param>
		/// <param name="length"></param>
		/// <param name="encoding"></param>
		/// <param name="font"></param>
		/// <param name="path"></param>
		/// <param name="textAlign"></param>
		/// <param name="origin"></param>
		public static SKTextBlob? CreatePathPositioned (IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default) =>
			CreatePathPositioned (text.AsReadOnlySpan (length), encoding, font, path, textAlign, origin);

		/// <param name="text"></param>
		/// <param name="encoding"></param>
		/// <param name="font"></param>
		/// <param name="path"></param>
		/// <param name="textAlign"></param>
		/// <param name="origin"></param>
		public static SKTextBlob? CreatePathPositioned (ReadOnlySpan<byte> text, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return CreatePathPositioned (t, text.Length, encoding, font, path, textAlign, origin);
			}
		}

		internal static SKTextBlob? CreatePathPositioned (void* text, int length, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default)
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

		/// <param name="upperBounds"></param>
		/// <param name="lowerBounds"></param>
		/// <param name="paint"></param>
		public float[] GetIntercepts (float upperBounds, float lowerBounds, SKPaint? paint = null)
		{
			var n = CountIntercepts (upperBounds, lowerBounds, paint);
			var intervals = new float[n];
			GetIntercepts (upperBounds, lowerBounds, intervals, paint);
			return intervals;
		}

		/// <param name="upperBounds"></param>
		/// <param name="lowerBounds"></param>
		/// <param name="intervals"></param>
		/// <param name="paint"></param>
		public void GetIntercepts (float upperBounds, float lowerBounds, Span<float> intervals, SKPaint? paint = null)
		{
			var bounds = stackalloc float[2];
			bounds[0] = upperBounds;
			bounds[1] = lowerBounds;
			fixed (float* i = intervals) {
				SkiaApi.sk_textblob_get_intercepts (Handle, bounds, i, paint?.Handle ?? IntPtr.Zero);
			}
		}

		// CountIntercepts

		/// <param name="upperBounds"></param>
		/// <param name="lowerBounds"></param>
		/// <param name="paint"></param>
		public int CountIntercepts (float upperBounds, float lowerBounds, SKPaint? paint = null)
		{
			var bounds = stackalloc float[2];
			bounds[0] = upperBounds;
			bounds[1] = lowerBounds;
			return SkiaApi.sk_textblob_get_intercepts (Handle, bounds, null, paint?.Handle ?? IntPtr.Zero);
		}

		//

		internal static SKTextBlob? GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKTextBlob (handle, true);
	}

	/// <summary>
	/// A builder object that is used to create a <see cref="T:SkiaSharp.SKTextBlob" />.
	/// </summary>
	public unsafe class SKTextBlobBuilder : SKObject, ISKSkipObjectRegistration
	{
		internal SKTextBlobBuilder (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		/// <summary>
		/// Creates a new instance of <see cref="T:SkiaSharp.SKTextBlobBuilder" />.
		/// </summary>
		public SKTextBlobBuilder ()
			: this (SkiaApi.sk_textblob_builder_new (), true)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_textblob_builder_delete (Handle);

		// Build

		/// <summary>
		/// Create the <see cref="T:SkiaSharp.SKTextBlob" /> from all the added runs.
		/// </summary>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKTextBlob" /> if there were runs, otherwise <see langword="null" />.</returns>
		public SKTextBlob? Build ()
		{
			var blob = SKTextBlob.GetObject (SkiaApi.sk_textblob_builder_make (Handle));
			GC.KeepAlive (this);
			return blob;
		}

		// AddRun

		/// <param name="glyphs"></param>
		/// <param name="font"></param>
		/// <param name="origin"></param>
		public void AddRun (ReadOnlySpan<ushort> glyphs, SKFont font, SKPoint origin = default)
		{
			var buffer = AllocateRawPositionedRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.Glyphs);
			font.GetGlyphPositions (buffer.Glyphs, buffer.Positions, origin);
		}

		// AddHorizontalRun

		/// <param name="glyphs"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		/// <param name="y"></param>
		public void AddHorizontalRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			var buffer = AllocateRawHorizontalRun (font, glyphs.Length, y);
			glyphs.CopyTo (buffer.Glyphs);
			positions.CopyTo (buffer.Positions);
		}

		// AddPositionedRun

		/// <param name="glyphs"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		public void AddPositionedRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			var buffer = AllocateRawPositionedRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.Glyphs);
			positions.CopyTo (buffer.Positions);
		}

		// AddRotationScaleRun 

		/// <param name="glyphs"></param>
		/// <param name="font"></param>
		/// <param name="positions"></param>
		public void AddRotationScaleRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			var buffer = AllocateRawRotationScaleRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.Glyphs);
			positions.CopyTo (buffer.Positions);
		}

		// AddPathPositionedRun

		/// <param name="glyphs"></param>
		/// <param name="font"></param>
		/// <param name="glyphWidths"></param>
		/// <param name="glyphOffsets"></param>
		/// <param name="path"></param>
		/// <param name="textAlign"></param>
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

		// Allocate*

		// Allocate*Run

		/// <param name="font"></param>
		/// <param name="count"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="bounds"></param>
		public SKRunBuffer AllocateRun (SKFont font, int count, float x, float y, SKRect? bounds = null)
		{
			var buffer = AllocateRawRun (font, count, x, y, bounds);
			return new SKRunBuffer (buffer.buffer, count);
		}

		public SKRawRunBuffer<float> AllocateRawRun (SKFont font, int count, float x, float y, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run (Handle, font.Handle, count, x, y, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run (Handle, font.Handle, count, x, y, null, &runbuffer);

			return new SKRawRunBuffer<float> (runbuffer, count, 0, 0);
		}

		public SKTextRunBuffer AllocateTextRun (SKFont font, int count, float x, float y, int textByteCount, SKRect? bounds = null)
		{
			var buffer = AllocateRawTextRun (font, count, x, y, textByteCount, bounds);
			return new SKTextRunBuffer (buffer.buffer, count, textByteCount);
		}

		public SKRawRunBuffer<float> AllocateRawTextRun (SKFont font, int count, float x, float y, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.Handle, count, x, y, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.Handle, count, x, y, textByteCount, null, &runbuffer);

			return new SKRawRunBuffer<float> (runbuffer, count, 0, textByteCount);
		}

		// Allocate*HorizontalRun

		/// <param name="font"></param>
		/// <param name="count"></param>
		/// <param name="y"></param>
		/// <param name="bounds"></param>
		public SKHorizontalRunBuffer AllocateHorizontalRun (SKFont font, int count, float y, SKRect? bounds = null)
		{
			var buffer = AllocateRawHorizontalRun (font, count, y, bounds);
			return new SKHorizontalRunBuffer (buffer.buffer, count);
		}

		public SKRawRunBuffer<float> AllocateRawHorizontalRun (SKFont font, int count, float y, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_pos_h (Handle, font.Handle, count, y, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_pos_h (Handle, font.Handle, count, y, null, &runbuffer);

			return new SKRawRunBuffer<float> (runbuffer, count, count, 0);
		}

		public SKHorizontalTextRunBuffer AllocateHorizontalTextRun (SKFont font, int count, float y, int textByteCount, SKRect? bounds = null)
		{
			var buffer = AllocateRawHorizontalTextRun (font, count, y, textByteCount, bounds);
			return new SKHorizontalTextRunBuffer (buffer.buffer, count, textByteCount);
		}

		public SKRawRunBuffer<float> AllocateRawHorizontalTextRun (SKFont font, int count, float y, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.Handle, count, y, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.Handle, count, y, textByteCount, null, &runbuffer);

			return new SKRawRunBuffer<float> (runbuffer, count, count, textByteCount);

		}

		// AllocatePositionedRun

		/// <param name="font"></param>
		/// <param name="count"></param>
		/// <param name="bounds"></param>
		public SKPositionedRunBuffer AllocatePositionedRun (SKFont font, int count, SKRect? bounds = null)
		{
			var buffer = AllocateRawPositionedRun (font, count, bounds);
			return new SKPositionedRunBuffer (buffer.buffer, count);
		}

		public SKRawRunBuffer<SKPoint> AllocateRawPositionedRun (SKFont font, int count, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_pos (Handle, font.Handle, count, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_pos (Handle, font.Handle, count, null, &runbuffer);

			return new SKRawRunBuffer<SKPoint> (runbuffer, count, count, 0);
		}

		public SKPositionedTextRunBuffer AllocatePositionedTextRun (SKFont font, int count, int textByteCount, SKRect? bounds = null)
		{
			var buffer = AllocateRawPositionedTextRun (font, count, textByteCount, bounds);
			return new SKPositionedTextRunBuffer (buffer.buffer, count, textByteCount);
		}

		public SKRawRunBuffer<SKPoint> AllocateRawPositionedTextRun (SKFont font, int count, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.Handle, count, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.Handle, count, textByteCount, null, &runbuffer);

			return new SKRawRunBuffer<SKPoint> (runbuffer, count, count, textByteCount);
		}

		// AllocateRotationScaleRun

		public SKRotationScaleRunBuffer AllocateRotationScaleRun (SKFont font, int count, SKRect? bounds = null)
		{
			var buffer = AllocateRawRotationScaleRun (font, count, bounds);
			return new SKRotationScaleRunBuffer (buffer.buffer, count);
		}

		public SKRawRunBuffer<SKRotationScaleMatrix> AllocateRawRotationScaleRun (SKFont font, int count, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_rsxform (Handle, font.Handle, count, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_rsxform (Handle, font.Handle, count, null, &runbuffer);

			return new SKRawRunBuffer<SKRotationScaleMatrix> (runbuffer, count, count, 0);
		}

		public SKRotationScaleTextRunBuffer AllocateRotationScaleTextRun (SKFont font, int count, int textByteCount, SKRect? bounds = null)
		{
			var buffer = AllocateRawRotationScaleTextRun (font, count, textByteCount, bounds);
			return new SKRotationScaleTextRunBuffer (buffer.buffer, count, textByteCount);
		}

		public SKRawRunBuffer<SKRotationScaleMatrix> AllocateRawRotationScaleTextRun (SKFont font, int count, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text_rsxform (Handle, font.Handle, count, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text_rsxform (Handle, font.Handle, count, textByteCount, null, &runbuffer);

			return new SKRawRunBuffer<SKRotationScaleMatrix> (runbuffer, count, count, textByteCount);
		}
	}
}
