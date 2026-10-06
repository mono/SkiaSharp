using System;

namespace SkiaSharp
{
	/// <summary>Represents multiple text runs of glyphs and positions.</summary>
	/// <remarks />
	public unsafe class SKTextBlob : SKObject, ISKNonVirtualReferenceCounted, ISKSkipObjectRegistration
	{
		internal SKTextBlob (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKTextBlob" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKTextBlob" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		void ISKNonVirtualReferenceCounted.ReferenceNative ()
		{
			SkiaApi.sk_textblob_ref (Handle);
			GC.KeepAlive (this);
		}

		void ISKNonVirtualReferenceCounted.UnreferenceNative ()
		{
			SkiaApi.sk_textblob_unref (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Gets the conservative blob bounding box.</summary>
		/// <value>The bounding rectangle of the text blob.</value>
		/// <remarks />
		public SKRect Bounds {
			get {
				SKRect bounds;
				SkiaApi.sk_textblob_get_bounds (Handle, &bounds);
				GC.KeepAlive (this);
				return bounds;
			}
		}

		/// <summary>Gets the unique, non-zero value representing the text blob.</summary>
		/// <value>The unique identifier for this text blob.</value>
		/// <remarks />
		public uint UniqueId {
			get {
				var r = SkiaApi.sk_textblob_get_unique_id (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		// Create

		/// <summary>Creates a new text blob from the specified text.</summary>
		/// <param name="text">The text to shape and render.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="origin">The origin point for the text blob.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? Create (string text, SKFont font, SKPoint origin = default) =>
			Create (text.AsSpan (), font, origin);

		/// <summary>Creates a new text blob from the specified text.</summary>
		/// <param name="text">The text to shape and render.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="origin">The origin point for the text blob.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? Create (ReadOnlySpan<char> text, SKFont font, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return Create (t, text.Length * 2, SKTextEncoding.Utf16, font, origin);
			}
		}

		/// <summary>Creates a new text blob from the specified encoded text pointer.</summary>
		/// <param name="text">A pointer to the encoded text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="encoding">The text encoding of the input text.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="origin">The origin point for the text blob.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? Create (IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPoint origin = default) =>
			Create (text.AsReadOnlySpan (length), encoding, font, origin);

		/// <summary>Creates a new text blob from the specified encoded text.</summary>
		/// <param name="text">The encoded text bytes to shape and render.</param>
		/// <param name="encoding">The text encoding of the input text.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="origin">The origin point for the text blob.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
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

		/// <summary>Creates a new horizontally-positioned text blob.</summary>
		/// <param name="text">The text to shape and render.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The horizontal X positions for each glyph.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreateHorizontal (string text, SKFont font, ReadOnlySpan<float> positions, float y) =>
			CreateHorizontal (text.AsSpan (), font, positions, y);

		/// <summary>Creates a new horizontally-positioned text blob.</summary>
		/// <param name="text">The text to shape and render.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The horizontal X positions for each glyph.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreateHorizontal (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			fixed (void* t = text) {
				return CreateHorizontal (t, text.Length * 2, SKTextEncoding.Utf16, font, positions, y);
			}
		}

		/// <summary>Creates a new horizontally-positioned text blob from encoded text pointer.</summary>
		/// <param name="text">A pointer to the encoded text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="encoding">The text encoding of the input text.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The horizontal X positions for each glyph.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreateHorizontal (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<float> positions, float y) =>
			CreateHorizontal (text.AsReadOnlySpan (length), encoding, font, positions, y);

		/// <summary>Creates a new horizontally-positioned text blob from encoded text.</summary>
		/// <param name="text">The encoded text bytes to shape and render.</param>
		/// <param name="encoding">The text encoding of the input text.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The horizontal X positions for each glyph.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
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

		/// <summary>Creates a new fully-positioned text blob.</summary>
		/// <param name="text">The text to shape and render.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The positions for each glyph.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreatePositioned (string text, SKFont font, ReadOnlySpan<SKPoint> positions) =>
			CreatePositioned (text.AsSpan (), font, positions);

		/// <summary>Creates a new fully-positioned text blob.</summary>
		/// <param name="text">The text to shape and render.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The positions for each glyph.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreatePositioned (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			fixed (void* t = text) {
				return CreatePositioned (t, text.Length * 2, SKTextEncoding.Utf16, font, positions);
			}
		}

		/// <summary>Creates a new fully-positioned text blob from encoded text pointer.</summary>
		/// <param name="text">A pointer to the encoded text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="encoding">The text encoding of the input text.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The positions for each glyph.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreatePositioned (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKPoint> positions) =>
			CreatePositioned (text.AsReadOnlySpan (length), encoding, font, positions);

		/// <summary>Creates a new fully-positioned text blob from encoded text.</summary>
		/// <param name="text">The encoded text bytes to shape and render.</param>
		/// <param name="encoding">The text encoding of the input text.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The positions for each glyph.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
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

		/// <summary>Creates a new text blob with rotation and scale transformations for each glyph.</summary>
		/// <param name="text">The text to shape and render.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The rotation-scale matrices for each glyph.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreateRotationScale (string text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			CreateRotationScale (text.AsSpan (), font, positions);

		/// <summary>Creates a new text blob with rotation and scale transformations for each glyph.</summary>
		/// <param name="text">The text to shape and render.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The rotation-scale matrices for each glyph.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreateRotationScale (ReadOnlySpan<char> text, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			fixed (void* t = text) {
				return CreateRotationScale (t, text.Length * 2, SKTextEncoding.Utf16, font, positions);
			}
		}

		/// <summary>Creates a new text blob with rotation and scale transformations from encoded text pointer.</summary>
		/// <param name="text">A pointer to the encoded text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="encoding">The text encoding of the input text.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The rotation-scale matrices for each glyph.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreateRotationScale (IntPtr text, int length, SKTextEncoding encoding, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions) =>
			CreateRotationScale (text.AsReadOnlySpan (length), encoding, font, positions);

		/// <summary>Creates a new text blob with rotation and scale transformations from encoded text.</summary>
		/// <param name="text">The encoded text bytes to shape and render.</param>
		/// <param name="encoding">The text encoding of the input text.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="positions">The rotation-scale matrices for each glyph.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
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

		/// <summary>Creates a new text blob positioned along a path.</summary>
		/// <param name="text">The text to shape and render.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="path">The path along which the text is positioned.</param>
		/// <param name="textAlign">The alignment of the text along the path.</param>
		/// <param name="origin">The starting point along the path.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreatePathPositioned (string text, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default) =>
			CreatePathPositioned (text.AsSpan (), font, path, textAlign, origin);

		/// <summary>Creates a new text blob positioned along a path.</summary>
		/// <param name="text">The text to shape and render.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="path">The path along which the text is positioned.</param>
		/// <param name="textAlign">The alignment of the text along the path.</param>
		/// <param name="origin">The starting point along the path.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreatePathPositioned (ReadOnlySpan<char> text, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default)
		{
			fixed (void* t = text) {
				return CreatePathPositioned (t, text.Length * 2, SKTextEncoding.Utf16, font, path, textAlign, origin);
			}
		}

		/// <summary>Creates a new text blob positioned along a path from encoded text pointer.</summary>
		/// <param name="text">A pointer to the encoded text data.</param>
		/// <param name="length">The length of the text data in bytes.</param>
		/// <param name="encoding">The text encoding of the input text.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="path">The path along which the text is positioned.</param>
		/// <param name="textAlign">The alignment of the text along the path.</param>
		/// <param name="origin">The starting point along the path.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
		public static SKTextBlob? CreatePathPositioned (IntPtr text, int length, SKTextEncoding encoding, SKFont font, SKPath path, SKTextAlign textAlign = SKTextAlign.Left, SKPoint origin = default) =>
			CreatePathPositioned (text.AsReadOnlySpan (length), encoding, font, path, textAlign, origin);

		/// <summary>Creates a new text blob positioned along a path from encoded text.</summary>
		/// <param name="text">The encoded text bytes to shape and render.</param>
		/// <param name="encoding">The text encoding of the input text.</param>
		/// <param name="font">The font used to shape the text.</param>
		/// <param name="path">The path along which the text is positioned.</param>
		/// <param name="textAlign">The alignment of the text along the path.</param>
		/// <param name="origin">The starting point along the path.</param>
		/// <returns>A new text blob, or <see langword="null" /> if creation fails.</returns>
		/// <remarks />
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

		/// <summary>Returns the intervals that intersect the specified bounds.</summary>
		/// <param name="upperBounds">The upper Y bound of the horizontal band.</param>
		/// <param name="lowerBounds">The lower Y bound of the horizontal band.</param>
		/// <param name="paint">Optional paint used to modify the text blob's intercepts.</param>
		/// <returns>An array of intercept pairs (start, end) for the horizontal band.</returns>
		/// <remarks />
		public float[] GetIntercepts (float upperBounds, float lowerBounds, SKPaint? paint = null)
		{
			var n = CountIntercepts (upperBounds, lowerBounds, paint);
			var intervals = new float[n];
			GetIntercepts (upperBounds, lowerBounds, intervals, paint);
			return intervals;
		}

		/// <summary>Fills the intervals span with the intercepts that intersect the specified bounds.</summary>
		/// <param name="upperBounds">The upper Y bound of the horizontal band.</param>
		/// <param name="lowerBounds">The lower Y bound of the horizontal band.</param>
		/// <param name="intervals">The span to receive the intercept pairs (start, end).</param>
		/// <param name="paint">Optional paint used to modify the text blob's intercepts.</param>
		/// <remarks />
		public void GetIntercepts (float upperBounds, float lowerBounds, Span<float> intervals, SKPaint? paint = null)
		{
			var bounds = stackalloc float[2];
			bounds[0] = upperBounds;
			bounds[1] = lowerBounds;
			fixed (float* i = intervals) {
				SkiaApi.sk_textblob_get_intercepts (Handle, bounds, i, paint?.Handle ?? IntPtr.Zero);
				GC.KeepAlive (paint);
				GC.KeepAlive (this);
			}
		}

		// CountIntercepts

		/// <summary>Returns the number of intervals that intersect the specified bounds.</summary>
		/// <param name="upperBounds">The upper Y bound of the horizontal band.</param>
		/// <param name="lowerBounds">The lower Y bound of the horizontal band.</param>
		/// <param name="paint">Optional paint used to modify the text blob's intercepts.</param>
		/// <returns>The number of intercepts.</returns>
		/// <remarks />
		public int CountIntercepts (float upperBounds, float lowerBounds, SKPaint? paint = null)
		{
			var bounds = stackalloc float[2];
			bounds[0] = upperBounds;
			bounds[1] = lowerBounds;
			var result = SkiaApi.sk_textblob_get_intercepts (Handle, bounds, null, paint?.Handle ?? IntPtr.Zero);
			GC.KeepAlive (paint);
			GC.KeepAlive (this);
			return result;
		}

		//

		internal static SKTextBlob? GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKTextBlob (handle, true);
	}

	/// <summary>A builder object that is used to create a <see cref="T:SkiaSharp.SKTextBlob" />.</summary>
	/// <remarks />
	public unsafe class SKTextBlobBuilder : SKObject, ISKSkipObjectRegistration
	{
		internal SKTextBlobBuilder (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKTextBlobBuilder" />.</summary>
		/// <remarks />
		public SKTextBlobBuilder ()
			: this (SkiaApi.sk_textblob_builder_new (), true)
		{
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKTextBlobBuilder" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKTextBlobBuilder" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative ()
		{
			SkiaApi.sk_textblob_builder_delete (Handle);
			GC.KeepAlive (this);
		}

		// Build

		/// <summary>Create the <see cref="T:SkiaSharp.SKTextBlob" /> from all the added runs.</summary>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKTextBlob" /> if there were runs, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public SKTextBlob? Build ()
		{
			var blob = SKTextBlob.GetObject (SkiaApi.sk_textblob_builder_make (Handle));
			GC.KeepAlive (this);
			return blob;
		}

		// AddRun

		/// <summary>Adds a new run to the builder at the specified origin.</summary>
		/// <param name="glyphs">The glyph IDs for this run.</param>
		/// <param name="font">The font used for this run.</param>
		/// <param name="origin">The origin point for the run.</param>
		/// <remarks />
		public void AddRun (ReadOnlySpan<ushort> glyphs, SKFont font, SKPoint origin = default)
		{
			var buffer = AllocateRawPositionedRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.Glyphs);
			font.GetGlyphPositions (buffer.Glyphs, buffer.Positions, origin);
		}

		// AddHorizontalRun

		/// <summary>Adds a new horizontally-positioned run to the builder.</summary>
		/// <param name="glyphs">The glyph IDs for this run.</param>
		/// <param name="font">The font used for this run.</param>
		/// <param name="positions">The horizontal X positions for each glyph.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <remarks />
		public void AddHorizontalRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<float> positions, float y)
		{
			var buffer = AllocateRawHorizontalRun (font, glyphs.Length, y);
			glyphs.CopyTo (buffer.Glyphs);
			positions.CopyTo (buffer.Positions);
		}

		// AddPositionedRun

		/// <summary>Adds a new fully-positioned run to the builder.</summary>
		/// <param name="glyphs">The glyph IDs for this run.</param>
		/// <param name="font">The font used for this run.</param>
		/// <param name="positions">The positions for each glyph.</param>
		/// <remarks />
		public void AddPositionedRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKPoint> positions)
		{
			var buffer = AllocateRawPositionedRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.Glyphs);
			positions.CopyTo (buffer.Positions);
		}

		// AddRotationScaleRun 

		/// <summary>Adds a new run with rotation and scale transformations.</summary>
		/// <param name="glyphs">The glyph IDs for this run.</param>
		/// <param name="font">The font used for this run.</param>
		/// <param name="positions">The positions for each glyph.</param>
		/// <remarks />
		public void AddRotationScaleRun (ReadOnlySpan<ushort> glyphs, SKFont font, ReadOnlySpan<SKRotationScaleMatrix> positions)
		{
			var buffer = AllocateRawRotationScaleRun (font, glyphs.Length);
			glyphs.CopyTo (buffer.Glyphs);
			positions.CopyTo (buffer.Positions);
		}

		// AddPathPositionedRun

		/// <summary>Adds a new run positioned along a path.</summary>
		/// <param name="glyphs">The glyph IDs for this run.</param>
		/// <param name="font">The font used for this run.</param>
		/// <param name="glyphWidths">The widths for each glyph.</param>
		/// <param name="glyphOffsets">The offsets for each glyph.</param>
		/// <param name="path">The path along which glyphs are positioned.</param>
		/// <param name="textAlign">The text alignment along the path.</param>
		/// <remarks />
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

		/// <summary>Allocates a buffer for a run at a fixed origin.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="x">The horizontal X position for the origin.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A run buffer for writing glyph data.</returns>
		/// <remarks />
		public SKRunBuffer AllocateRun (SKFont font, int count, float x, float y, SKRect? bounds = null)
		{
			var buffer = AllocateRawRun (font, count, x, y, bounds);
			return new SKRunBuffer (buffer.buffer, count);
		}

		/// <summary>Allocates a raw buffer for a run at the specified position.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="x">The horizontal X position for the origin.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A raw run buffer for writing glyph data.</returns>
		/// <remarks />
		public SKRawRunBuffer<float> AllocateRawRun (SKFont font, int count, float x, float y, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run (Handle, font.Handle, count, x, y, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run (Handle, font.Handle, count, x, y, null, &runbuffer);

			GC.KeepAlive (font);
			GC.KeepAlive (this);
			return new SKRawRunBuffer<float> (runbuffer, count, 0, 0);
		}

		/// <summary>Allocates a buffer for a run with text storage at the specified position.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="x">The horizontal X position for the origin.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <param name="textByteCount">The number of bytes to allocate for text storage.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A run buffer for writing glyph data.</returns>
		/// <remarks />
		public SKTextRunBuffer AllocateTextRun (SKFont font, int count, float x, float y, int textByteCount, SKRect? bounds = null)
		{
			var buffer = AllocateRawTextRun (font, count, x, y, textByteCount, bounds);
			return new SKTextRunBuffer (buffer.buffer, count, textByteCount);
		}

		/// <summary>Allocates a buffer for a rotation-scale run.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="x">The horizontal X position for the origin.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <param name="textByteCount">The number of bytes to allocate for text storage.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A run buffer for writing glyph and rotation-scale matrix data.</returns>
		/// <remarks />
		public SKRawRunBuffer<float> AllocateRawTextRun (SKFont font, int count, float x, float y, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.Handle, count, x, y, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text (Handle, font.Handle, count, x, y, textByteCount, null, &runbuffer);

			GC.KeepAlive (font);
			GC.KeepAlive (this);
			return new SKRawRunBuffer<float> (runbuffer, count, 0, textByteCount);
		}

		// Allocate*HorizontalRun

		/// <summary>Allocates a buffer for a horizontally-positioned run.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A run buffer for writing glyph and position data.</returns>
		/// <remarks />
		public SKHorizontalRunBuffer AllocateHorizontalRun (SKFont font, int count, float y, SKRect? bounds = null)
		{
			var buffer = AllocateRawHorizontalRun (font, count, y, bounds);
			return new SKHorizontalRunBuffer (buffer.buffer, count);
		}

		/// <summary>Allocates a raw buffer for a horizontal run.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A raw run buffer for writing glyph and X position data.</returns>
		/// <remarks />
		public SKRawRunBuffer<float> AllocateRawHorizontalRun (SKFont font, int count, float y, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_pos_h (Handle, font.Handle, count, y, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_pos_h (Handle, font.Handle, count, y, null, &runbuffer);

			GC.KeepAlive (font);
			GC.KeepAlive (this);
			return new SKRawRunBuffer<float> (runbuffer, count, count, 0);
		}

		/// <summary>Allocates a buffer for a fully-positioned run.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <param name="textByteCount">The number of bytes to allocate for text storage.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A run buffer for writing glyph and position data.</returns>
		/// <remarks />
		public SKHorizontalTextRunBuffer AllocateHorizontalTextRun (SKFont font, int count, float y, int textByteCount, SKRect? bounds = null)
		{
			var buffer = AllocateRawHorizontalTextRun (font, count, y, textByteCount, bounds);
			return new SKHorizontalTextRunBuffer (buffer.buffer, count, textByteCount);
		}

		/// <summary>Allocates a raw buffer for a horizontally-positioned run with text storage.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="y">The vertical Y position for the text baseline.</param>
		/// <param name="textByteCount">The number of bytes to allocate for text storage.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A raw buffer containing glyph IDs, horizontal positions, and text data.</returns>
		/// <remarks />
		public SKRawRunBuffer<float> AllocateRawHorizontalTextRun (SKFont font, int count, float y, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.Handle, count, y, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text_pos_h (Handle, font.Handle, count, y, textByteCount, null, &runbuffer);

			GC.KeepAlive (font);
			GC.KeepAlive (this);
			return new SKRawRunBuffer<float> (runbuffer, count, count, textByteCount);

		}

		// AllocatePositionedRun

		/// <summary>Allocates a buffer for a fully-positioned run.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A run buffer for writing glyph and position data.</returns>
		/// <remarks />
		public SKPositionedRunBuffer AllocatePositionedRun (SKFont font, int count, SKRect? bounds = null)
		{
			var buffer = AllocateRawPositionedRun (font, count, bounds);
			return new SKPositionedRunBuffer (buffer.buffer, count);
		}

		/// <summary>Allocates a raw buffer for a positioned run.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A raw run buffer for writing glyph and position data.</returns>
		/// <remarks />
		public SKRawRunBuffer<SKPoint> AllocateRawPositionedRun (SKFont font, int count, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_pos (Handle, font.Handle, count, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_pos (Handle, font.Handle, count, null, &runbuffer);

			GC.KeepAlive (font);
			GC.KeepAlive (this);
			return new SKRawRunBuffer<SKPoint> (runbuffer, count, count, 0);
		}

		/// <summary>Allocates a buffer for a positioned run with text storage.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="textByteCount">The number of bytes to allocate for text storage.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A run buffer for writing glyph and position data.</returns>
		/// <remarks />
		public SKPositionedTextRunBuffer AllocatePositionedTextRun (SKFont font, int count, int textByteCount, SKRect? bounds = null)
		{
			var buffer = AllocateRawPositionedTextRun (font, count, textByteCount, bounds);
			return new SKPositionedTextRunBuffer (buffer.buffer, count, textByteCount);
		}

		/// <summary>Allocates a raw buffer for a positioned run with text storage.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="textByteCount">The number of bytes to allocate for text storage.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A raw run buffer for writing glyph and position data.</returns>
		/// <remarks />
		public SKRawRunBuffer<SKPoint> AllocateRawPositionedTextRun (SKFont font, int count, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.Handle, count, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text_pos (Handle, font.Handle, count, textByteCount, null, &runbuffer);

			GC.KeepAlive (font);
			GC.KeepAlive (this);
			return new SKRawRunBuffer<SKPoint> (runbuffer, count, count, textByteCount);
		}

		// AllocateRotationScaleRun

		/// <summary>Allocates a buffer for a rotation-scale text run with cluster data.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A text run buffer for writing glyph, rotation-scale, cluster, and text data.</returns>
		/// <remarks />
		public SKRotationScaleRunBuffer AllocateRotationScaleRun (SKFont font, int count, SKRect? bounds = null)
		{
			var buffer = AllocateRawRotationScaleRun (font, count, bounds);
			return new SKRotationScaleRunBuffer (buffer.buffer, count);
		}

		/// <summary>Allocates a raw buffer for a rotation-scale run.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A raw run buffer for writing glyph and rotation-scale matrix data.</returns>
		/// <remarks />
		public SKRawRunBuffer<SKRotationScaleMatrix> AllocateRawRotationScaleRun (SKFont font, int count, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_rsxform (Handle, font.Handle, count, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_rsxform (Handle, font.Handle, count, null, &runbuffer);

			GC.KeepAlive (font);
			GC.KeepAlive (this);
			return new SKRawRunBuffer<SKRotationScaleMatrix> (runbuffer, count, count, 0);
		}

		/// <summary>Allocates a buffer for a positioned run with text storage.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="textByteCount">The number of bytes to allocate for text storage.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A run buffer for writing glyph and position data.</returns>
		/// <remarks />
		public SKRotationScaleTextRunBuffer AllocateRotationScaleTextRun (SKFont font, int count, int textByteCount, SKRect? bounds = null)
		{
			var buffer = AllocateRawRotationScaleTextRun (font, count, textByteCount, bounds);
			return new SKRotationScaleTextRunBuffer (buffer.buffer, count, textByteCount);
		}

		/// <summary>Allocates a raw buffer for a rotation-scale run with text storage.</summary>
		/// <param name="font">The font used for this run.</param>
		/// <param name="count">The number of glyphs to allocate in the buffer.</param>
		/// <param name="textByteCount">The number of bytes to allocate for text storage.</param>
		/// <param name="bounds">Optional bounds for the run.</param>
		/// <returns>A raw run buffer for writing glyph and rotation-scale matrix data.</returns>
		/// <remarks />
		public SKRawRunBuffer<SKRotationScaleMatrix> AllocateRawRotationScaleTextRun (SKFont font, int count, int textByteCount, SKRect? bounds = null)
		{
			if (font == null)
				throw new ArgumentNullException (nameof (font));

			SKRunBufferInternal runbuffer;
			if (bounds is SKRect b)
				SkiaApi.sk_textblob_builder_alloc_run_text_rsxform (Handle, font.Handle, count, textByteCount, &b, &runbuffer);
			else
				SkiaApi.sk_textblob_builder_alloc_run_text_rsxform (Handle, font.Handle, count, textByteCount, null, &runbuffer);

			GC.KeepAlive (font);
			GC.KeepAlive (this);
			return new SKRawRunBuffer<SKRotationScaleMatrix> (runbuffer, count, count, textByteCount);
		}
	}
}
